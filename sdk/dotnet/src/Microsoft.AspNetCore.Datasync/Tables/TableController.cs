// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Datasync.Extensions;
using Microsoft.AspNetCore.Datasync.Filters;
using Microsoft.AspNetCore.Datasync.Models;
using Microsoft.AspNetCore.Datasync.Tables;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Query.Validator;
using Microsoft.Extensions.Logging;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Datasync
{
    [ApiController]
    [DatasyncController]
    [DatasyncExceptions]
    [DatasyncProtocolVersionFilter]
    [AzureMobileBackwardsCompatibilityFilter]
    public class TableController<TEntity> : ControllerBase where TEntity : class, ITableData
    {
        /// <summary>
        /// The OData <see cref="IEdmModel"/> for the entity type.  Used for generating the
        /// appropriate response in OData query syntax.
        /// </summary>
        private IEdmModel EdmModel { get; init; }

        /// <summary>
        /// The backing store for the <see cref="Repository"/> property.
        /// </summary>
        private IRepository<TEntity> _repository;

        /// <summary>
        /// Creates a new <see cref="TableController{TEntity}"/> object with the specified
        /// repository, accessControlProvider, and options.
        /// </summary>
        /// <param name="repository">The repository to use as the data store.</param>
        /// <param name="accessControlProvider">The access control provider.</param>
        /// <param name="options">The <see cref="TableControllerOptions"/> for this controller.</param>
        public TableController(IRepository<TEntity> repository = null, IAccessControlProvider<TEntity> accessControlProvider = null, IEdmModel edmModel = null, TableControllerOptions options = null)
        {
            _repository = repository;
            AccessControlProvider = accessControlProvider ?? new AccessControlProvider<TEntity>();
            Options = options ?? new TableControllerOptions();
            EdmModel = edmModel ?? ModelCache.GetEdmModel(typeof(TEntity));
            if (EdmModel.FindType(typeof(TEntity).FullName) == null)
            {
                throw new InvalidOperationException($"The type {typeof(TEntity).FullName} is not registered in the OData model");
            }
        }

        #region Properties
        /// <summary>
        /// The <see cref="IRepository{TEntity}"/> to use for data store operations.
        /// </summary>
        public IRepository<TEntity> Repository
        {
            get { return _repository ?? throw new InvalidOperationException("Repository is not set"); }
            set { _repository = value ?? throw new ArgumentNullException(nameof(Repository)); }
        }

        /// <summary>
        /// An event handler to use (instead of <see cref="IAccessControlProvider{TEntity}.PostCommitHookAsync(TableOperation, TEntity, CancellationToken)"/>
        /// for receiving updates to the repository.
        /// </summary>
        public event EventHandler<RepositoryUpdatedEventArgs> RepositoryUpdated;

        /// <summary>
        /// The <see cref="TableControllerOptions"/> object for configuring this controller.
        /// </summary>
        public TableControllerOptions Options { get; set; }

        /// <summary>
        /// The <see cref="IAccessControlProvider{TEntity}"/> object for securing the table controller.
        /// </summary>
        public IAccessControlProvider<TEntity> AccessControlProvider { get; set; }

        /// <summary>
        /// Where to send request/response log messages.
        /// </summary>
        public ILogger Logger { get; set; } = null;
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
                Logger?.LogWarning("Not authorized to perform operation {Operation} on entity {Id}", operation, entity?.Id);
                throw new HttpException(Options.UnauthorizedStatusCode);
            }
            else
            {
                Logger?.LogDebug("Authorized to perform operation {Operation} on entity {Id}", operation, entity?.Id);
            }
        }

        /// <summary>
        /// When doing a query evaluation, certain providers (e.g. Entity Framework) require some things
        /// to be done client side.  We use a client side evaluator to handle this case when it happens.
        /// </summary>
        /// <param name="ex">The exception thrown by the service-side evaluator</param>
        /// <param name="reason">The reason if the client-side evaluator throws.</param>
        /// <param name="clientSideEvaluator">The client-side evaluator</param>
        [NonAction]
        internal void CatchClientSideEvaluationException(Exception ex, string reason, Action clientSideEvaluator)
        {
            if (IsClientSideEvaluationException(ex) || IsClientSideEvaluationException(ex.InnerException))
            {
                try
                {
                    clientSideEvaluator.Invoke();
                }
                catch (Exception err)
                {
                    Logger?.LogError("Error while {reason}: {Message}", reason, err.Message);
                    throw;
                }
            } 
            else
            {
                throw ex;
            }
            
        }

        /// <summary>
        /// Determines if a particular exception indicates a client-side evaluation is required.
        /// </summary>
        /// <param name="ex">The exception that was thrown by the service-side evaluator</param>
        /// <returns>true if a client-side evaluation is required.</returns>
        [NonAction]
        internal static bool IsClientSideEvaluationException(Exception ex)
            => ex != null && (ex is InvalidOperationException || ex is NotSupportedException);

        /// <summary>
        /// Determines if the entity provided is in the view of the data requested.
        /// </summary>
        /// <param name="entity">The entity</param>
        /// <returns>true if in view</returns>
        [NonAction]
        internal bool EntityIsInView(TEntity entity)
            => AccessControlProvider.GetDataView()?.Compile().Invoke(entity) != false;

        /// <summary>
        /// Converts the provided JSON string to a JSON Patch Document based on the Request type.
        /// Supports both RFC 6902 and RFC 7286 type patches.
        /// </summary>
        /// <param name="id">The ID of the request</param>
        /// <param name="json">The JSON Patch Document</param>
        /// <returns>A <see cref="JsonPatchDocument{TModel}"/> object</returns>
        [NonAction]
        internal JsonPatchDocument<TEntity> GetPatchDocument(string id, string json)
        {
            ContentType contentType = new(Request.ContentType);

            try
            {
                if (contentType.MediaType == "application/json-patch+json")
                {
                    Logger?.LogInformation("Patch({Id}): Received JSON PATCH", id);
                    return JsonConvert.DeserializeObject<JsonPatchDocument<TEntity>>(json);
                }
                else
                {
                    Logger?.LogInformation("Patch({Id}): Received MERGE PATCH", id);
                    var mergepatch = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
                    var patch = new JsonPatchDocument<TEntity>();
                    foreach (var kv in mergepatch)
                    {
                        patch.Operations.Add(new Operation<TEntity>("replace", $"/{kv.Key}", null, kv.Value));
                    }
                    return patch;
                }
            }
            catch (Exception ex)
            {
                throw new BadRequestException(ex.Message, ex);
            }
        }

        /// <summary>
        /// Calls the appropriate <see cref="PostCommitHookAsync(TableOperation, TEntity, CancellationToken)"/> and raises the appropriate
        /// event on the <see cref="RepositoryUpdated"/> event publisher.
        /// </summary>
        /// <param name="op">The operation being performed.</param>
        /// <param name="entity">The entity that was updated (post update; except for hard delete)</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that completes when the post commit hook has been called.</returns>
        protected virtual Task PostCommitHookAsync(TableOperation op, TEntity entity, CancellationToken cancellationToken = default)
        {
            RepositoryUpdatedEventArgs e = new(op, typeof(TEntity).Name, entity);
            RepositoryUpdated?.Invoke(this, e);
            return AccessControlProvider.PostCommitHookAsync(op, entity, cancellationToken);
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
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public virtual async Task<IActionResult> CreateAsync([FromBody] TEntity item, CancellationToken token = default)
        {
            Logger?.LogInformation("Create: initiated");
            await AuthorizeRequest(TableOperation.Create, item, token).ConfigureAwait(false);
            await AccessControlProvider.PreCommitHookAsync(TableOperation.Create, item, token).ConfigureAwait(false);
            await Repository.CreateAsync(item, token).ConfigureAwait(false);
            Logger?.LogInformation("Create: Item stored at {Id}", item.Id);
            await PostCommitHookAsync(TableOperation.Create, item, token).ConfigureAwait(false);
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
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public virtual async Task<IActionResult> DeleteAsync([FromRoute] string id, CancellationToken token = default)
        {
            Logger?.LogInformation("Delete({Id}): initiated", id);
            var entity = await Repository.ReadAsync(id, token).ConfigureAwait(false);
            if (entity == null || !EntityIsInView(entity))
            {
                Logger?.LogWarning("Delete({Id}): Item not found (not in view)", id);
                return NotFound();
            }
            await AuthorizeRequest(TableOperation.Delete, entity, token).ConfigureAwait(false);
            if (Options.EnableSoftDelete && entity.Deleted)
            {
                Logger?.LogWarning("Delete({Id}): Item not found (soft-delete)", id);
                return StatusCode(StatusCodes.Status410Gone);
            }
            Request.ParseConditionalRequest(entity, out byte[] version);
            if (Options.EnableSoftDelete)
            {
                Logger?.LogInformation("Delete({Id}): Marking item as deleted (soft-delete)", id);
                entity.Deleted = true;
                await AccessControlProvider.PreCommitHookAsync(TableOperation.Delete, entity, token).ConfigureAwait(false);
                await Repository.ReplaceAsync(entity, version, token).ConfigureAwait(false);
                await PostCommitHookAsync(TableOperation.Update, entity, token).ConfigureAwait(false);

            }
            else
            {
                Logger?.LogInformation("Delete({Id}): Deleting item (hard-delete)", id);
                await Repository.DeleteAsync(id, version, token).ConfigureAwait(false);
                await PostCommitHookAsync(TableOperation.Delete, entity, token).ConfigureAwait(false);
            }
            return NoContent();
        }

        /// <summary>
        /// The PATCH method updates an existing entity according to the Delta patch format used by
        /// ZUMO-API-VERSION v2.0.0
        /// </summary>
        /// <param name="id">The globally unique ID of the entity</param>
        /// <param name="patchDocument">The delta patch document</param>
        /// <param name="token">A cancellation token</param>
        /// <returns>An <see cref="OkObjectResult"/> with the resulting entity.</returns>
        [HttpPatch("{id}")]
        [Consumes("application/json", "application/json-patch+json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> PatchAsync([FromRoute] string id, CancellationToken token = default)
        {
            Logger?.LogInformation("Patch({Id}): initiated", id);

            // Read the body as a string and convert into a JsonPatchDocument.
            string json = await Request.GetBodyAsStringAsync().ConfigureAwait(false);
            JsonPatchDocument<TEntity> patchDocument = GetPatchDocument(id, json);

            var entity = await Repository.ReadAsync(id, token).ConfigureAwait(false);
            if (entity == null || !EntityIsInView(entity))
            {
                Logger?.LogWarning("Patch({Id}): Item not found (not in view)", id);
                return NotFound();
            }
            if (patchDocument.ModifiesSystemProperties(entity, out Dictionary<string, string[]> systemPropertyValidationErrors))
            {
                Logger?.LogWarning("Patch({Id}): Patch document changes system properties (which is disallowed)", id);
                return ValidationProblem(new ValidationProblemDetails(systemPropertyValidationErrors));
            }
            await AuthorizeRequest(TableOperation.Update, entity, token).ConfigureAwait(false);
            if (Options.EnableSoftDelete && entity.Deleted && !patchDocument.Contains("replace", "/deleted", false))
            {
                Logger?.LogWarning("Patch({Id}): Item not found (soft-delete)", id);
                return StatusCode(StatusCodes.Status410Gone);
            }
            Request.ParseConditionalRequest(entity, out byte[] version);

            patchDocument.ApplyTo(entity);
            if (!TryValidateModel(entity))
            {
                return ValidationProblem(ModelState);
            }

            await AccessControlProvider.PreCommitHookAsync(TableOperation.Update, entity, token).ConfigureAwait(false);
            await Repository.ReplaceAsync(entity, version, token).ConfigureAwait(false);
            await PostCommitHookAsync(TableOperation.Update, entity, token).ConfigureAwait(false);
            Logger?.LogDebug("Patch({Id}): successfully patched", id);
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
            Logger?.LogInformation("Query: {QueryString}", Request.QueryString);
            await AuthorizeRequest(TableOperation.Query, null, token).ConfigureAwait(false);

            var dataset = await Repository.AsQueryableAsync().ConfigureAwait(false);
            dataset = dataset
                .ApplyDataView(AccessControlProvider.GetDataView())
                .ApplyDeletedView(Request, Options.EnableSoftDelete);
            var validationSettings = new ODataValidationSettings() { MaxTop = Options.MaxTop };
            var querySettings = new ODataQuerySettings() { PageSize = Options.PageSize, EnsureStableOrdering = true };
            var queryContext = new ODataQueryContext(EdmModel, typeof(TEntity), new ODataPath());
            var queryOptions = new ODataQueryOptions<TEntity>(queryContext, Request);

            try
            {
                queryOptions.Validate(validationSettings);
            }
            catch (ODataException validationException)
            {
                Logger?.LogWarning("Query: Error when validating query: {Message}", validationException.Message);
                return BadRequest(validationException.Message);
            }

            // Get the actual items needed for the response.
            // Some IQueryable providers cannot execute all queries, so have to revert to client-side evaluation.
            // EF Core, in particular, does not handle this case, so we have to do it for them.
            IEnumerable<object> results = null;
            int resultCount = 0;
            try
            {
                results = (IEnumerable<object>)queryOptions.ApplyTo(dataset, querySettings);
                resultCount = results.Count();
            }
            catch (Exception ex)
            {
                CatchClientSideEvaluationException(ex, "executing query", () =>
                {
                    var message = ex.InnerException?.Message ?? ex.Message;
                    Logger?.LogWarning("Error while executing query: Possible client-side evaluation ({Message})", message);
                    results = (IEnumerable<object>)queryOptions.ApplyTo(dataset.ToList().AsQueryable(), querySettings);
                    resultCount = results.Count();
                });
            }

            // Get the information needed for the nextLink.
            int skip = (queryOptions.Skip?.Value ?? 0) + resultCount;
            int top = (queryOptions.Top?.Value ?? 0) - resultCount;

            // Get the total count of items that would be returned.
            // Some IQueryable providers cannot execute all queries, so have to revert to client-side evaluation.
            // EF Core, in particular, does not handle this case, so we have to do it for them.
            long count = 0;
            try
            {
                var query = (IQueryable<TEntity>)queryOptions.Filter?.ApplyTo(dataset, new ODataQuerySettings()) ?? dataset;
                count = (long)query.Count();
            }
            catch (Exception ex)
            {
                CatchClientSideEvaluationException(ex, "executing query for count", () =>
                {
                    var message = ex.InnerException?.Message ?? ex.Message;
                    Logger?.LogWarning("Error while executing query for long count: Possible client-side evaluation ({Message})", message);
                    var query = (IQueryable<TEntity>)queryOptions.Filter?.ApplyTo(dataset.ToList().AsQueryable(), new ODataQuerySettings()) ?? dataset.ToList().AsQueryable();
                    count = (long)query.Count();
                });
            }

            // Construct the output object.
            var result = new PagedResult(results ?? Array.Empty<object>()) { Count = queryOptions.Count != null ? count : null };
            if (queryOptions.Top != null)
            {
                result.NextLink = skip >= count || top <= 0 ? null : Request.CreateNextLink(skip, top);
            }
            else
            {
                result.NextLink = skip >= count ? null : Request.CreateNextLink(skip, 0);
            }

            Logger?.LogInformation("Query: {Count} items being returned", result.Items.Count());
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
            Logger?.LogInformation("Read({Id}): initiated", id);
            var entity = await Repository.ReadAsync(id, token).ConfigureAwait(false);
            if (entity == null || !EntityIsInView(entity))
            {
                Logger?.LogWarning("Read({Id}): Item not found (not in view)", id);
                return NotFound();
            }
            await AuthorizeRequest(TableOperation.Read, entity, token).ConfigureAwait(false);
            if (Options.EnableSoftDelete && entity.Deleted && !Request.ShouldIncludeDeletedItems())
            {
                Logger?.LogWarning("Read({Id}): Item not found (soft-delete)", id);
                return StatusCode(StatusCodes.Status410Gone);
            }
            Request.ParseConditionalRequest(entity, out _);
            Logger?.LogInformation("Read({Id}): Item found - returning", id);
            return Ok(entity);
        }

        /// <summary>
        /// The PUT operation is an idempotent mechanism for replacing the entire record.  This version does not
        /// do create operations (use POST for that)
        /// </summary>
        /// <param name="id">The id of the record to be replaced</param>
        /// <param name="item">The replacement item</param>
        /// <param name="token">A cancellation token</param>
        /// <returns></returns>
        [HttpPut("{id}")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> ReplaceAsync([FromRoute] string id, [FromBody] TEntity item, CancellationToken token = default)
        {
            Logger?.LogInformation("Replace({Id}): initiating", id);
            if (item.Id != id)
            {
                Logger?.LogWarning("Replace({Id}): item does not match ID", id);
                return BadRequest();
            }

            var entity = await Repository.ReadAsync(id, token).ConfigureAwait(false);
            if (entity == null || !EntityIsInView(entity))
            {
                Logger?.LogWarning("Replace({Id}): Item not found (not in view)", id);
                return NotFound();
            }

            await AuthorizeRequest(TableOperation.Update, entity, token).ConfigureAwait(false);
            if (Options.EnableSoftDelete && entity.Deleted)
            {
                Logger?.LogWarning("Replace({Id}): Item not found (soft-delete)", id);
                return StatusCode(StatusCodes.Status410Gone);
            }
            Request.ParseConditionalRequest(entity, out byte[] version);
            await AccessControlProvider.PreCommitHookAsync(TableOperation.Update, item, token).ConfigureAwait(false);
            await Repository.ReplaceAsync(item, version, token).ConfigureAwait(false);
            await PostCommitHookAsync(TableOperation.Update, item, token).ConfigureAwait(false);
            Logger?.LogInformation("Replace({Id}): Item replaced", id);
            return Ok(item);
        }
        #endregion
    }
}
