﻿using Azure.Mobile.Server.Extensions;
using Azure.Mobile.Server.Utils;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OData;
using Microsoft.OData.Edm;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Azure.Mobile.Server
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
            modelBuilder.AddEntityType(typeof(TEntity));
            EdmModel = modelBuilder.GetEdmModel();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TableController{TEntity}"/> class with
        /// a given <paramref name="tableRepository"/>.
        /// </summary>
        /// <param name="tableRepository">The <see cref="ITableRepository{TEntity}"/> for the backend store.</param>
        protected TableController(ITableRepository<TEntity> tableRepository): this()
        {
            if (tableRepository == null)
            {
                throw new ArgumentNullException(nameof(tableRepository));
            }
            this.tableRepository = tableRepository;
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
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }
                if (tableRepository != null)
                {
                    throw new InvalidOperationException("TableRepository cannot be set twice.");
                }
                tableRepository = value;
            }
        }

        /// <summary>
        /// Returns true if the user is authorized to perform the operation on the specified entity.
        /// </summary>
        /// <param name="operation">The <see cref="TableOperation"/> that is being performed</param>
        /// <param name="item">The item that is being accessed (or null for a list operation).</param>
        /// <returns>true if the user is allowed to perform the operation.</returns>
        [NonAction]
        public virtual bool IsAuthorized(TableOperation operation, TEntity item) => true;

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
            => 200;

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
        public virtual async ValueTask<int> ValidateOperationAsync(TableOperation operation, TEntity item, CancellationToken cancellationToken = default)
        {
            int result = await Task.Run(() => ValidateOperation(operation, item), cancellationToken).ConfigureAwait(false);
            return result;
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

            if (!IsAuthorized(TableOperation.List, null))
            {
                return NotFound();
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
            var excludeItems = Request.Query.ContainsKey("__excludeitems") && Request.Query["__excludeitems"].First().ToLower() == "true";

            var result = new PagedListResult<TEntity>
            {
                Values = !excludeItems ? items : null,
                NextLink = (!excludeItems && items.Count() > 0) ? Request.GetNextPageLink(TableControllerOptions.PageSize) : null,
                Count = odataOptions.Count?.GetEntityCount(odataOptions.Filter?.ApplyTo(dataView.AsQueryable(), new ODataQuerySettings()) ?? dataView.AsQueryable()),
                MaxTop = TableControllerOptions.MaxTop,
                PageSize = TableControllerOptions.PageSize
            };

            return Ok(result); 
        }

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

            if (!IsAuthorized(TableOperation.Create, item))
            {
                return Unauthorized();
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

            if (!IsAuthorized(TableOperation.Delete, entity))
            {
                return NotFound();
            }

            var preconditionStatusCode = ETag.EvaluatePreconditions(entity, Request.GetTypedHeaders());
            if (preconditionStatusCode != StatusCodes.Status200OK)
            {
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

            if (!IsAuthorized(TableOperation.Read, entity))
            {
                return NotFound();
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

            if (!IsAuthorized(TableOperation.Replace, item))
            {
                return NotFound();
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

#if SUPPORTS_PATCH
        /// <summary>
        /// Patch operation: PATCH {path}/{id}
        /// 
        /// Note that unlike most of the other operations, this one works all the time on soft-deleted records, as long
        /// as you are undeleting the record..
        /// </summary>
        /// <remarks>
        /// Disabling JSON Patch Support until Microsoft.AspNetCore.JsonPatch support System.Text.Json
        /// </remarks>
        /// <param name="id">The ID of the entity to be patched</param>
        /// <param name="patchDocument">A patch operation document (see RFC 6901, RFC 6902)</param>
        /// <returns>200 OK with the new entity in the body</returns>
        [HttpPatch("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status412PreconditionFailed)]
        public virtual async Task<IActionResult> PatchItemAsync(string id, [FromBody] JsonPatchDocument<TEntity> patchDocument)
        {
            var entity = await TableRepository.LookupAsync(id).ConfigureAwait(false);

            if (entity == null)
            {
                return NotFound();
            }
            var operationValidation = await ValidateOperationAsync(TableOperation.Patch, entity);
            if (operationValidation != StatusCodes.Status200OK)
            {
                return StatusCode(operationValidation);
            }

            if (!IsAuthorized(TableOperation.Patch, entity))
            {
                return NotFound();
            }
            var entityIsDeleted = (TableControllerOptions.SoftDeleteEnabled && entity.Deleted);

            var preconditionStatusCode = ETag.EvaluatePreconditions(entity, Request.GetTypedHeaders());
            if (preconditionStatusCode != StatusCodes.Status200OK)
            {
                AddHeadersToResponse(entity);
                return StatusCode(preconditionStatusCode, entity);
            }

            patchDocument.ApplyTo(entity);
            if (entity.Id != id)
            {
                return BadRequest();
            }

            // Special Case:
            //  If SoftDelete is enabled, and the original record is DELETED, then you can
            //  continue as long as one of the operations is an undelete operation.
            if (entityIsDeleted && entity.Deleted)
            {
                return NotFound();
            }

            var preparedItem = await PrepareItemForStoreAsync(entity).ConfigureAwait(false);
            var replacement = await TableRepository.ReplaceAsync(preparedItem).ConfigureAwait(false);
            AddHeadersToResponse(replacement);
            return Ok(replacement);
        }
#endif

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
