// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AzureMobile.Server.Exceptions;
using Microsoft.AzureMobile.Server.Extensions;
using Microsoft.AzureMobile.Server.Tables;
using Microsoft.OData;
using Microsoft.OData.Edm;

namespace Microsoft.AzureMobile.Server
{
    [ApiController]
    [AzureMobileController]
    [AzureMobileExceptions]
    [ZumoVersionFilter]
    public class TableController<TEntity> : ControllerBase where TEntity : class, ITableData
    {
        /// <summary>
        /// The OData <see cref="IEdmModel"/> for the entity type.  Used for generating the
        /// appropriate response in OData query syntax.
        /// </summary>
        private IEdmModel EdmModel { get; init; }

        /// <summary>
        /// Creates a new <see cref="TableController{TEntity}"/> object with the specified
        /// repository, accessControlProvider, and options.
        /// </summary>
        /// <param name="repository">The repository to use as the data store.</param>
        /// <param name="accessControlProvider">The access control provider.</param>
        /// <param name="options">The <see cref="TableControllerOptions"/> for this controller.</param>
        public TableController(IRepository<TEntity> repository = null, IAccessControlProvider<TEntity> accessControlProvider = null, TableControllerOptions options = null) : base()
        {
            _repository = repository;
            AccessControlProvider = accessControlProvider ?? new AccessControlProvider<TEntity>();
            Options = options ?? new TableControllerOptions();

            var modelBuilder = new ODataConventionModelBuilder();
            modelBuilder.AddEntityType(typeof(TEntity));
            EdmModel = modelBuilder.GetEdmModel();
        }

        #region Repository Property
        private IRepository<TEntity> _repository;

        /// <summary>
        /// The <see cref="IRepository{TEntity}"/> to use for data store operations.
        /// </summary>
        public IRepository<TEntity> Repository
        {
            get { return _repository ?? throw new InvalidOperationException("Repository is not set"); }
            set { _repository = value ?? throw new ArgumentNullException(nameof(Repository)); }
        }
        #endregion

        #region Options Property
        /// <summary>
        /// The <see cref="TableControllerOptions"/> object for configuring this controller.
        /// </summary>
        public TableControllerOptions Options { get; set; }
        #endregion

        #region AccessControlProvider Property
        /// <summary>
        /// The <see cref="IAccessControlProvider{TEntity}"/> object for securing the table controller.
        /// </summary>
        public IAccessControlProvider<TEntity> AccessControlProvider { get; set; }
        #endregion

        #region Private Methods
        /// <summary>
        /// Checks the authorization of the request and throws a <see cref="HttpException"/> if the
        /// authorization fails.
        /// </summary>
        /// <param name="operation">The operation being performed</param>
        /// <param name="entity">The entity (potentially null)</param>
        /// <param name="token">A cancellation token</param>
        [NonAction]
        private async Task AuthorizeRequest(TableOperation operation, TEntity entity, CancellationToken token)
        {
            var isAuthorized = await AccessControlProvider.IsAuthorizedAsync(operation, entity, token).ConfigureAwait(false);
            if (!isAuthorized)
            {
                throw new HttpException(StatusCodes.Status401Unauthorized);
            }
        }

        /// <summary>
        /// Determines if the entity provided is in the view of the data requested.
        /// </summary>
        /// <param name="entity">The entity</param>
        /// <returns>true if in view</returns>
        [NonAction]
        internal bool EntityIsInView(TEntity entity)
        {
            Func<TEntity, bool> dataView = AccessControlProvider.GetDataView();
            return dataView?.Invoke(entity) != false;
        }
        #endregion

        #region HTTP Methods
        /// <summary>
        /// The POST method is used to request that the server accept the entity in the request as a new
        /// suboriginate of the resource.
        /// </summary>
        /// <remarks>
        /// The data store will update the entity metadata. This is returned as an ETag header.
        /// </remarks>
        /// <param name="item">The item to be created.</param>
        /// <param name="token">A cancellation token.</param>
        /// <returns>A <see cref="CreatedAtRouteResult"/> for the created entity.</returns>
        [HttpPost]
        [ActionName("CreateAsync")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [Consumes("application/json")]
        public virtual async Task<IActionResult> CreateAsync([FromBody] TEntity item, CancellationToken token = default)
        {
            await AuthorizeRequest(TableOperation.Create, item, token).ConfigureAwait(false);

            await AccessControlProvider.PreCommitHookAsync(TableOperation.Create, item, token).ConfigureAwait(false);
            await Repository.CreateAsync(item, token).ConfigureAwait(false);
            return CreatedAtAction(nameof(ReadAsync), new { id = item.Id }, item);
        }

        /// <summary>
        /// The DELETE method requests that the origin server delete the resource identified in the
        /// request URI.
        /// </summary>
        /// <param name="id">The globally unique ID of the enitty to be deleted</param>
        /// <param name="token">A cancellation token</param>
        /// <returns>A <see cref="NoContentResult"/> action result</returns>
        [HttpDelete("{id}")]
        [ActionName("DeleteAsync")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public virtual async Task<IActionResult> DeleteAsync([FromRoute] string id, CancellationToken token = default)
        {
            var entity = await Repository.ReadAsync(id, token).ConfigureAwait(false);
            if (entity == null || !EntityIsInView(entity))
            {
                return NotFound();
            }

            await AuthorizeRequest(TableOperation.Delete, entity, token).ConfigureAwait(false);

            if (Options.EnableSoftDelete && entity.Deleted)
            {
                return StatusCode(StatusCodes.Status410Gone);
            }

            Request.ParseConditionalRequest(entity, out byte[] version);

            if (Options.EnableSoftDelete)
            {
                entity.Deleted = true;
                await Repository.ReplaceAsync(entity, version, token).ConfigureAwait(false);
            }
            else
            {
                await Repository.DeleteAsync(id, version, token).ConfigureAwait(false);
            }
            return NoContent();
        }

        /// <summary>
        /// The PATCH method updates an existing entity according to the JSONPATCH (RFC 6902)
        /// input format.
        /// </summary>
        /// <remarks>
        /// Only the "replace" operation is supported.
        /// </remarks>
        /// <param name="id">The globally unique ID of the entity</param>
        /// <param name="patchDocument">The RFC 6902 patch document</param>
        /// <param name="token">A cancellation token</param>
        /// <returns>An <see cref="OkObjectResult"/> with the resulting entity.</returns>
        [HttpPatch("{id}")]
        [ActionName("PatchAsync")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Consumes("application/json-patch+json")]
        public virtual async Task<IActionResult> PatchAsync([FromRoute] string id, [FromBody] JsonPatchDocument<TEntity> patchDocument, CancellationToken token = default)
        {
            if (patchDocument.ModifiesSystemProperties())
            {
                return BadRequest();
            }

            var entity = await Repository.ReadAsync(id, token).ConfigureAwait(false);
            if (entity == null || !EntityIsInView(entity))
            {
                return NotFound();
            }

            await AuthorizeRequest(TableOperation.Update, entity, token).ConfigureAwait(false);

            if (Options.EnableSoftDelete && entity.Deleted && !patchDocument.Contains("replace", "/deleted", false))
            {
                return StatusCode(StatusCodes.Status410Gone);
            }

            Request.ParseConditionalRequest(entity, out byte[] version);

            patchDocument.ApplyTo(entity);
            if (!TryValidateModel(entity))
            {
                return BadRequest();
            }

            await AccessControlProvider.PreCommitHookAsync(TableOperation.Update, entity, token).ConfigureAwait(false);
            await Repository.ReplaceAsync(entity, version, token).ConfigureAwait(false);
            return Ok(entity);
        }

        /// <summary>
        /// <para>
        /// The GET method is used to retrieve resource representation.  The resource is never modified.
        /// In this case, an OData v4 query is accepted with the following options:
        /// </para>
        /// <para>
        /// - <c>$count</c> is used to return a count of entities within the search parameters within the <see cref="PagedResult{TEntity}"/> response.
        /// - <c>$filter</c> is used to restrict the entities to be sent.
        /// - <c>$orderby</c> is used for ordering the entities to be sent.
        /// - <c>$select</c> is used to select which properties of the entities are sent.
        /// - <c>$skip</c> is used to skip some entities
        /// - <c>$top</c> is used to limit the number of entities returned.
        /// </para>
        /// </summary>
        /// <param name="token">A cancellation token</param>
        /// <returns>An <see cref="OkObjectResult"/> response object with the items.</returns>
        [HttpGet]
        [ActionName("QueryAsync")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> QueryAsync(CancellationToken token = default)
        {
            await AuthorizeRequest(TableOperation.Query, null, token).ConfigureAwait(false);

            var dataset = Repository.AsQueryable()
                .ApplyDataView(AccessControlProvider.GetDataView())
                .ApplyDeletedView(Request, Options.EnableSoftDelete);
            var validationSettings = new ODataValidationSettings() { MaxTop = Options.PageSize };
            var querySettings = new ODataQuerySettings() { PageSize = Options.PageSize, EnsureStableOrdering = true };
            var queryContext = new ODataQueryContext(EdmModel, typeof(TEntity), new ODataPath());
            var queryOptions = new ODataQueryOptions<TEntity>(queryContext, Request);

            try
            {
                queryOptions.Validate(validationSettings);
            }
            catch (ODataException validationException)
            {
                return BadRequest(validationException.Message);
            }

            var results = (IEnumerable<object>)queryOptions.ApplyTo(dataset, querySettings);
            var query = (IQueryable<TEntity>)queryOptions.Filter?.ApplyTo(dataset, new ODataQuerySettings()) ?? dataset;
            int skip = (queryOptions.Skip?.Value ?? 0) + results.Count();
            long count = query.LongCount();

            var result = new PagedResult(results)
            {
                Count = queryOptions.Count != null ? count : null,
                NextLink = skip >= count ? null : Request.CreateNextLink(skip, queryOptions.Top?.Value ?? 0)
            };

            return Ok(result);
        }

        /// <summary>
        /// The GET method is used to retrieve resource representation.  The resource is never modified
        /// in any way.  A single entity is returned.
        /// </summary>
        /// <param name="id">The globally unique ID for the entity</param>
        /// <param name="token">A cancellation token</param>
        /// <returns>An <see cref="OkObjectResult"/> with the entity.</returns>
        [HttpGet("{id}")]
        [ActionName("ReadAsync")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> ReadAsync([FromRoute] string id, CancellationToken token = default)
        {
            var entity = await Repository.ReadAsync(id, token).ConfigureAwait(false);
            if (entity == null || !EntityIsInView(entity))
            {
                return NotFound();
            }

            await AuthorizeRequest(TableOperation.Delete, entity, token).ConfigureAwait(false);

            if (Options.EnableSoftDelete && entity.Deleted)
            {
                return StatusCode(StatusCodes.Status410Gone);
            }

            Request.ParseConditionalRequest(entity, out _);
            return Ok(entity);
        }

        [HttpPut("{id}")]
        [ActionName("ReplaceAsync")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Consumes("application/json")]
        public virtual async Task<IActionResult> ReplaceAsync([FromRoute] string id, [FromBody] TEntity item, CancellationToken token = default)
        {
            if (item.Id != id)
            {
                return BadRequest();
            }

            var entity = await Repository.ReadAsync(id, token).ConfigureAwait(false);
            if (entity == null || !EntityIsInView(entity))
            {
                return NotFound();
            }

            await AuthorizeRequest(TableOperation.Update, entity, token).ConfigureAwait(false);

            if (Options.EnableSoftDelete && entity.Deleted)
            {
                return StatusCode(StatusCodes.Status410Gone);
            }

            Request.ParseConditionalRequest(entity, out byte[] version);
            await AccessControlProvider.PreCommitHookAsync(TableOperation.Update, item, token).ConfigureAwait(false);
            await Repository.ReplaceAsync(item, version, token).ConfigureAwait(false);
            return Ok(item);
        }
        #endregion
    }
}
