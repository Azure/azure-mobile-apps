// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.Zumo.Server.Extensions;
using Microsoft.Zumo.Server.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Zumo.Server
{
    /// <summary>
    /// Provides a common <see cref="Controller"/> abstraction for the REST API controllers
    /// that implement table access.
    /// </summary>
    /// <typeparam name="TEntity">The model type for the served entities.</typeparam>
    public class TableController<TEntity> : ControllerBase where TEntity : class, ITableData
    {
        /// <summary>
        /// The table repository that handles storage of entities in this table controller
        /// </summary>
        private ITableRepository<TEntity> tableRepository = null;

        /// <summary>
        /// The EdmModel for the entity, used in OData processing.
        /// </summary>
        private IEdmModel EdmModel { get; set; }

        /// <summary>
        /// Initialze a new instance of the <see cref="TableController{TEntity}"/> class.
        /// </summary>
        protected TableController()
        {
            var modelBuilder = new ODataConventionModelBuilder();
            modelBuilder.EntityType<TEntity>().Filter().Count().OrderBy().Expand().Select();
            EdmModel = modelBuilder.GetEdmModel();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TableController{TEntity}"/> class with
        /// a given <paramref name="tableRepository"/>.
        /// </summary>
        /// <param name="tableRepository">The <see cref="ITableRepository{TEntity}"/> for the backend store.</param>
        protected TableController(ITableRepository<TEntity> tableRepository): this()
        {
            this.tableRepository = tableRepository ?? throw new ArgumentNullException(nameof(tableRepository));
        }

        /// <summary>
        /// The <see cref="ITableRepository{TEntity}"/> to be used for accessing the backend store.
        /// </summary>
        public ITableRepository<TEntity> TableRepository
        {
            get
            {
                if (tableRepository == null)
                {
                    throw new InvalidOperationException("TableRepository must be set before use.");
                }
                return tableRepository;
            }
            set
            {
                if (tableRepository != null)
                {
                    throw new InvalidOperationException("TableRepository cannot be set twice.");
                }
                tableRepository = value ?? throw new ArgumentNullException(nameof(value));
            }
        }

        /// <summary>
        /// Returns true if the user is authorized to perform the operation on the specified entity.
        /// </summary>
        /// <param name="operation">The <see cref="TableOperation"/> that is being performed</param>
        /// <param name="item">The item that is being accessed (or null for a list operation).</param>
        /// <returns>true if the user is allowed to perform the operation.</returns>
        [NonAction]
        public virtual bool IsAuthorized(TableOperation operation, TEntity item) 
            => true;

        /// <summary>
        /// If the <see cref="IsAuthorized(TableOperation, TEntity)"/> method doesn't provide enough flexibility,
        /// then you can use the <see cref="ValidateOperation(TableOperation, TEntity)"/> or its async equivalent
        /// to return a HTTP status code.  Returning StatusCodes.OK will indicate that the operation can proceed,
        /// whereas returning a different status code will stop processing and return that status code to the
        /// user.
        /// 
        /// No additional information is provided to the user, so avoid the use of 409 Conflict or other response
        /// codes that suggest additional information will be available.
        /// </summary>
        /// <param name="operation">The operation being performed</param>
        /// <param name="item">The item being used (null for lists)</param>
        /// <returns>A HTTP Status Code</returns>
        [NonAction]
        public virtual int ValidateOperation(TableOperation operation, TEntity item) 
            => StatusCodes.Status200OK;

        /// <summary>
        /// If the <see cref="IsAuthorized(TableOperation, TEntity)"/> method doesn't provide enough flexibility,
        /// then you can use the <see cref="ValidateOperationAsync(TableOperation, TEntity)"/> or its sync equivalent
        /// to return a HTTP status code.  Returning StatusCodes.OK will indicate that the operation can proceed,
        /// whereas returning a different status code will stop processing and return that status code to the
        /// user.
        /// 
        /// No additional information is provided to the user, so avoid the use of 409 Conflict or other response
        /// codes that suggest additional information will be available.
        /// </summary>
        /// <param name="operation">The operation being performed</param>
        /// <param name="item">The item being used (null for lists)</param>
        /// <returns>A HTTP Status Code</returns>
        [NonAction]
        public virtual Task<int> ValidateOperationAsync(TableOperation operation, TEntity item, CancellationToken cancellationToken = default)
        {
            if (!IsAuthorized(operation, item))
            {
                return Task.FromResult(StatusCodes.Status403Forbidden);
            }

            var validateOperationResult = ValidateOperation(operation, item);
            if (validateOperationResult != StatusCodes.Status200OK)
            {
                return Task.FromResult(validateOperationResult);
            }

            return Task.FromResult(StatusCodes.Status200OK);
        }

        /// <summary>
        /// Prepares the item for storing into the backend store.  This is a opportunity for the application
        /// to add additional meta-data that is not passed back to the client (such as the user ID in a 
        /// personal data store).
        /// </summary>
        /// <param name="item">The item to be prepared</param>
        /// <returns>The prepared item</returns>
        [NonAction]
        public virtual TEntity PrepareItemForStore(TEntity item) => item;

        /// <summary>
        /// Prepares the item for storing into the backend store.  This is a opportunity for the application
        /// to add additional meta-data that is not passed back to the client (such as the user ID in a 
        /// personal data store).
        /// </summary>
        /// <param name="item">The item to be prepared</param>
        /// <returns>The prepared item</returns>
        [NonAction]
        public virtual Task<TEntity> PrepareItemForStoreAsync(TEntity item)
            => Task.Run(() => PrepareItemForStore(item));

        /// <summary>
        /// The <see cref="TableControllerOptions{T}"/> for this controller.  This is used to specify
        /// the data view, soft-delete, and list limits.
        /// </summary>
        public TableControllerOptions<TEntity> TableControllerOptions { get; set; } 
            = new TableControllerOptions<TEntity>();

        /// <summary>
        /// List operation: GET {path}?{odata-filters}
        /// 
        /// Additional Query Parameters:
        ///     __includedeleted = true     Include deleted records in a soft-delete situation.
        /// </summary>
        /// <returns>200 OK with the results of the list (paged)</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public virtual async Task<IActionResult> GetItems()
        {
            var operationValidation = await ValidateOperationAsync(TableOperation.List, null);
            if (operationValidation != StatusCodes.Status200OK)
            {
                return StatusCode(operationValidation);
            }

            var dataView = TableRepository.AsQueryable()
                .ApplyDeletedFilter(TableControllerOptions, Request)
                .Where(TableControllerOptions.DataView);

            var odataValidationSettings = new ODataValidationSettings
            {
                MaxTop = TableControllerOptions.MaxTop
            };

            var odataQuerySettings = new ODataQuerySettings
            {
                PageSize = TableControllerOptions.PageSize,
                EnsureStableOrdering = true
            };

            // Construct the OData context and parse the query
            var queryContext = new ODataQueryContext(EdmModel, typeof(TEntity), new ODataPath());
            var odataOptions = new ODataQueryOptions<TEntity>(queryContext, Request);
            try
            {
                odataOptions.Validate(odataValidationSettings);
            }
            catch (ODataException ex)
            {
                return BadRequest(ex.Message);
            }
            var odataQuery = odataOptions.ApplyTo(dataView.AsQueryable(), odataQuerySettings);
            var items = (odataQuery as IEnumerable<TEntity>).ToList();

            var odata3count = Request.Query.ContainsKey("$inlinecount") && Request.Query["$inlinecount"].First().ToLower() == "allpages";
            var odata4count = odataOptions.Count != null && odataOptions.Count.Value;
            if (odata3count || odata4count)
            {
                var view = odataOptions.Filter?.ApplyTo(dataView.AsQueryable(), new ODataQuerySettings()) ?? dataView.AsQueryable();

                var result = new PagedListResult<TEntity>
                {
                    Results = items,
                    Count = odataOptions.Count?.GetEntityCount(view) ?? GetEntityCount(view)
                };
                return Ok(result);
            }

            return Ok(items); 
        }

        /// <summary>
        /// Obtains the number of elements in the IQueryable, so that we can provide the inline count
        /// This is only used for old clients - new clients will use $count = true.
        /// </summary>
        /// <param name="view">The <see cref="IQueryable"/> to count</param>
        /// <returns></returns>
        private long GetEntityCount(IQueryable query) => (query as IQueryable<TEntity>).LongCount();

        /// <summary>
        /// Create operation: POST {path}
        /// </summary>
        /// <param name="item">The item submitted for creation</param>
        /// <returns>201 response with the item that was added (if successful)</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status412PreconditionFailed)]
        public virtual async Task<IActionResult> CreateItemAsync([FromBody] TEntity item)
        {
            if (item.Id == null)
            {
                item.Id = Guid.NewGuid().ToString("N");
            }

            var operationValidation = await ValidateOperationAsync(TableOperation.Create, item);
            if (operationValidation != StatusCodes.Status200OK)
            {
                return StatusCode(operationValidation);
            }

            var entity = await TableRepository.LookupAsync(item.Id).ConfigureAwait(false);
            var preconditionStatusCode = ETag.EvaluatePreconditions(entity, Request.GetTypedHeaders());
            if (preconditionStatusCode != StatusCodes.Status200OK)
            {
                AddHeadersToResponse(entity);
                return StatusCode(preconditionStatusCode, entity);
            }

            if (entity != null)
            {
                AddHeadersToResponse(entity);
                return Conflict(entity);
            }

            var preparedItem = await PrepareItemForStoreAsync(item).ConfigureAwait(false);
            var createdEntity = await TableRepository.CreateAsync(preparedItem).ConfigureAwait(false);
            AddHeadersToResponse(createdEntity);
            var uri = $"{Request.GetEncodedUrl()}/{createdEntity.Id}";
            return Created(uri, createdEntity);
        }

        /// <summary>
        /// Delete operation: DELETE {path}/{id}
        /// </summary>
        /// <param name="id">The ID of the entity to be deleted</param>
        /// <returns>204 No Content response</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status412PreconditionFailed)]
        public virtual async Task<IActionResult> DeleteItemAsync(string id)
        {
            var entity = await TableRepository.LookupAsync(id).ConfigureAwait(false);
            if (entity == null || entity.Deleted)
            {
                return NotFound();
            }

            var operationValidation = await ValidateOperationAsync(TableOperation.Delete, entity);
            if (operationValidation != StatusCodes.Status200OK)
            {
                return StatusCode(operationValidation);
            }

            var preconditionStatusCode = ETag.EvaluatePreconditions(entity, Request.GetTypedHeaders());
            if (preconditionStatusCode != StatusCodes.Status200OK)
            {
                AddHeadersToResponse(entity);
                return StatusCode(preconditionStatusCode, entity);
            }

            if (TableControllerOptions.SoftDeleteEnabled)
            {
                entity.Deleted = true;
                var preparedItem = await PrepareItemForStoreAsync(entity).ConfigureAwait(false);
                await TableRepository.ReplaceAsync(preparedItem).ConfigureAwait(false);
            } 
            else
            {
                await TableRepository.DeleteAsync(entity.Id).ConfigureAwait(false);
            }
            return NoContent();
        }

        /// <summary>
        /// Delete operation: PATCH {path}/{id}
        /// </summary>
        /// <param name="id">The ID of the entity to be patched</param>
        /// <param name="patchDoc">The patch document provided in the body</param>
        /// <returns>200 OK response with the new entity</returns>
        [HttpPatch("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status412PreconditionFailed)]
        public virtual async Task<IActionResult> PatchItemAsync(string id, [FromBody] Delta<TEntity> patchDoc)
        {
            var entity = await TableRepository.LookupAsync(id).ConfigureAwait(false);
            if (entity == null) // Note: You can patch an item to undelete it
            {
                return NotFound();
            }

            var operationValidation = await ValidateOperationAsync(TableOperation.Patch, entity);
            if (operationValidation != StatusCodes.Status200OK)
            {
                return StatusCode(operationValidation);
            }

            var preconditionStatusCode = ETag.EvaluatePreconditions(entity, Request.GetTypedHeaders());
            if (preconditionStatusCode != StatusCodes.Status200OK)
            {
                AddHeadersToResponse(entity);
                return StatusCode(preconditionStatusCode, entity);
            }

            // Apply the patch to the entity
            patchDoc.Patch(entity);
            var preparedItem = await PrepareItemForStoreAsync(entity).ConfigureAwait(false);
            var replacement = await TableRepository.ReplaceAsync(preparedItem).ConfigureAwait(false);
            AddHeadersToResponse(replacement);
            return Ok(replacement);
        }

        /// <summary>
        /// Read operation: GET {path}/{id}
        /// </summary>
        /// <param name="id">The ID of the entity to be deleted</param>
        /// <returns>200 OK with the entity in the body</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status304NotModified)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status412PreconditionFailed)]
        public virtual async Task<IActionResult> ReadItemAsync(string id)
        {
            var entity = await TableRepository.LookupAsync(id).ConfigureAwait(false);
            if (entity == null || entity.Deleted)
            {
                return NotFound();
            }

            var operationValidation = await ValidateOperationAsync(TableOperation.Read, entity);
            if (operationValidation != StatusCodes.Status200OK)
            {
                return StatusCode(operationValidation);
            }

            var preconditionStatusCode = ETag.EvaluatePreconditions(entity, Request.GetTypedHeaders(), true);
            if (preconditionStatusCode != StatusCodes.Status200OK)
            {
                if (preconditionStatusCode == StatusCodes.Status304NotModified)
                {
                    AddHeadersToResponse(entity);
                }
                return StatusCode(preconditionStatusCode, entity);
            }

            AddHeadersToResponse(entity);
            return Ok(entity);
        }

        /// <summary>
        /// Replaces the item in the dataset.  The ID within the item must match the id provided on the path.
        /// </summary>
        /// <param name="id">The ID of the entity to replace</param>
        /// <param name="item">The new entity contents</param>
        /// <returns>200 OK with the new entity within the body</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status412PreconditionFailed)]
        public virtual async Task<IActionResult> ReplaceItemAsync(string id, [FromBody] TEntity item)
        {
            if (item.Id != id)
            {
                return BadRequest();
            }

            var entity = await TableRepository.LookupAsync(id).ConfigureAwait(false);
            if (entity == null || entity.Deleted)
            {
                return NotFound();
            }

            var operationValidation = await ValidateOperationAsync(TableOperation.Replace, item);
            if (operationValidation != StatusCodes.Status200OK)
            {
                return StatusCode(operationValidation);
            }

            var preconditionStatusCode = ETag.EvaluatePreconditions(entity, Request.GetTypedHeaders());
            if (preconditionStatusCode != StatusCodes.Status200OK)
            {
                AddHeadersToResponse(entity);
                return StatusCode(preconditionStatusCode, entity);
            }

            var preparedItem = await PrepareItemForStoreAsync(item).ConfigureAwait(false);
            var replacement = await TableRepository.ReplaceAsync(preparedItem).ConfigureAwait(false);
            AddHeadersToResponse(replacement);
            return Ok(replacement);
        }

        /// <summary>
        /// Adds any necessary response headers, such as ETag and Last-Modified
        /// </summary>
        /// <param name="item">The item being returned</param>
        private void AddHeadersToResponse(TEntity item)
        {
            if (item != null)
            {
                Response.Headers["ETag"] = ETag.FromByteArray(item.Version);
                Response.Headers["Last-Modified"] = item.UpdatedAt.ToString(DateTimeFormatInfo.InvariantInfo.RFC1123Pattern);
            }
        }
    }
}
