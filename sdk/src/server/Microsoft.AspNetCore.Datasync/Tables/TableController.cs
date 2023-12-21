// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Datasync.Abstractions;
using Microsoft.AspNetCore.Datasync.Extensions;
using Microsoft.AspNetCore.Datasync.Filters;
using Microsoft.AspNetCore.Datasync.Models;
using Microsoft.AspNetCore.Datasync.Tables;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Query.Validator;
using Microsoft.AspNetCore.OData.Query.Wrapper;
using Microsoft.Extensions.Logging;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNetCore.Datasync;

/// <summary>
/// The core part of the datasync service; the <see cref="TableController{TEntity}"/> exposes the
/// endpoints that are used by the client to interact with the data.
/// </summary>
/// <typeparam name="TEntity">The type of the entity exposed to the client.</typeparam>
[DatasyncController]
public class TableController<TEntity> : TableControllerBase<TEntity> where TEntity : class, ITableData
{
    #region Controller Constructors
    /// <summary>
    /// Creates a new <see cref="TableController{TEntity}"/>. The table options (such as repository,
    /// access control provider, etc.) are set via the upstream controller.
    /// </summary>
    public TableController() : this(new Repository<TEntity>(), new AccessControlProvider<TEntity>(), null, new TableControllerOptions())
    {
    }

    /// <summary>
    /// Creates a new <see cref="TableController{TEntity}"/> with the specified repository.
    /// </summary>
    /// <param name="repository">The repository to use for this controller.</param>
    public TableController(IRepository<TEntity> repository) : this(repository, new AccessControlProvider<TEntity>(), null, new TableControllerOptions())
    {
    }

    /// <summary>
    /// Creates a new <see cref="TableController{TEntity}"/> with the specified repository.
    /// </summary>
    /// <param name="repository">The repository to use for this controller.</param>
    /// <param name="model">The <see cref="IEdmModel"/> to use for OData interactions.</param>
    public TableController(IRepository<TEntity> repository, IEdmModel model) : this(repository, new AccessControlProvider<TEntity>(), model, new TableControllerOptions())
    {
    }

    /// <summary>
    /// Creates a new <see cref="TableController{TEntity}"/> with the specified repository.
    /// </summary>
    /// <param name="repository">The repository to use for this controller.</param>
    /// <param name="accessControlProvider">The access control provider to use for this controller.</param>
    public TableController(IRepository<TEntity> repository, IAccessControlProvider<TEntity> accessControlProvider) : this(repository, accessControlProvider, null, new TableControllerOptions())
    {
    }

    /// <summary>
    /// Creates a new <see cref="TableController{TEntity}"/> with the specified repository.
    /// </summary>
    /// <param name="repository">The repository to use for this controller.</param>
    /// <param name="options">The <see cref="TableControllerOptions"/> to use for configuring this controller.</param>
    public TableController(IRepository<TEntity> repository, TableControllerOptions options) : this(repository, new AccessControlProvider<TEntity>(), null, options)
    {
    }

    /// <summary>
    /// Creates a new <see cref="TableController{TEntity}"/> with the specified repository.
    /// </summary>
    /// <param name="repository">The repository to use for this controller.</param>
    /// <param name="accessControlProvider">The access control provider to use for this controller.</param>
    /// <param name="options">The <see cref="TableControllerOptions"/> to use for configuring this controller.</param>
    public TableController(IRepository<TEntity> repository, IAccessControlProvider<TEntity> accessControlProvider, TableControllerOptions options) : this(repository, accessControlProvider, null, options)
    {
    }

    /// <summary>
    /// Creates a new <see cref="TableController{TEntity}"/> with the specified repository.
    /// </summary>
    /// <param name="repository">The repository to use for this controller.</param>
    /// <param name="accessControlProvider">The access control provider to use for this controller.</param>
    /// <param name="model">The <see cref="IEdmModel"/> to use for OData interactions.</param>
    public TableController(IRepository<TEntity> repository, IAccessControlProvider<TEntity> accessControlProvider, IEdmModel model) : this(repository, accessControlProvider, model, new TableControllerOptions())
    {
    }
    #endregion

    /// <summary>
    /// Creates a new <see cref="TableController{TEntity}"/>.
    /// </summary>
    /// <param name="repository">The repository that will be used for data access operations.</param>
    /// <param name="accessControlProvider">The access control provider that will be used for authorizing requests.</param>
    /// <param name="options">The options for this table controller.</param>
    public TableController(IRepository<TEntity> repository, IAccessControlProvider<TEntity> accessControlProvider, IEdmModel? edmModel, TableControllerOptions options)
        : base(repository, accessControlProvider, edmModel, options)
    {
    }

    /// <summary>
    /// Creates a new entity in the repository.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
    /// <returns>A <see cref="CreatedAtActionResult"/> for the created entity.</returns>
    /// <exception cref="HttpException">Thrown if there is an HTTP exception, such as unauthorized usage.</exception>
    [HttpPost]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public virtual async Task<IActionResult> CreateAsync(CancellationToken cancellationToken = default)
    {
        Logger.LogInformation("CreateAsync");
        TEntity entity = await DeserializeJsonContent(cancellationToken).ConfigureAwait(false);
        Logger.LogInformation("CreateAsync: {entity}", entity.ToJsonString());
        await AuthorizeRequestAsync(TableOperation.Create, entity, cancellationToken).ConfigureAwait(false);
        await AccessControlProvider.PreCommitHookAsync(TableOperation.Create, entity, cancellationToken).ConfigureAwait(false);
        await Repository.CreateAsync(entity, cancellationToken).ConfigureAwait(false);
        Logger.LogInformation("CreateAsync: {entity}", entity.ToJsonString());
        await PostCommitHookAsync(TableOperation.Create, entity, cancellationToken).ConfigureAwait(false);
        return CreatedAtRoute(new { id = entity.Id }, entity);
    }

    /// <summary>
    /// Requests that the repository deletes an entity or marks an entity as deleted (depending on the EnableSoftDelete option).
    /// </summary>
    /// <param name="id">Tne ID of the entity to be deleted.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
    /// <returns>A <see cref="NoContentResult"/> when complete.</returns>
    /// <exception cref="HttpException">Thrown if there is an HTTP exception, such as unauthorized usage.</exception>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public virtual async Task<IActionResult> DeleteAsync([FromRoute] string id, CancellationToken cancellationToken = default)
    {
        Logger.LogInformation("DeleteAsync: {id}", id);
        TEntity entity = await Repository.ReadAsync(id, cancellationToken).ConfigureAwait(false);
        if (!AccessControlProvider.EntityIsInView(entity))
        {
            Logger.LogWarning("DeleteAsync: {id} statusCode=404 not in view", id);
            throw new HttpException(StatusCodes.Status404NotFound);
        }
        await AuthorizeRequestAsync(TableOperation.Delete, entity, cancellationToken).ConfigureAwait(false);
        if (Options.EnableSoftDelete && entity.Deleted)
        {
            Logger.LogWarning("DeleteAsync: {id} statusCode=410 already deleted", id);
            throw new HttpException(StatusCodes.Status410Gone);
        }
        Request.ParseConditionalRequest(entity, out byte[] version);
        if (Options.EnableSoftDelete)
        {
            Logger.LogInformation("DeleteAsync: {id} marking item as deleted (soft-delete)", id);
            entity.Deleted = true;
            await AccessControlProvider.PreCommitHookAsync(TableOperation.Update, entity, cancellationToken).ConfigureAwait(false);
            await Repository.ReplaceAsync(entity, version, cancellationToken).ConfigureAwait(false);
            await PostCommitHookAsync(TableOperation.Update, entity, cancellationToken).ConfigureAwait(false);
        }
        else
        {
            Logger.LogInformation("DeleteAsync: {id} deleting item (hard-delete)", id);
            await Repository.DeleteAsync(id, version, cancellationToken).ConfigureAwait(false);
            await PostCommitHookAsync(TableOperation.Delete, entity, cancellationToken).ConfigureAwait(false);
        }
        return NoContent();
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
    /// <param name="cancellationToken">A cancellation token</param>
    /// <returns>An <see cref="OkObjectResult"/> response object with the items.</returns>
    [HttpGet]
    [ActionName("QueryAsync")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> QueryAsync(CancellationToken cancellationToken = default)
    {
        Logger.LogInformation("QueryAsync: {querystring}", HttpContext.Request.QueryString);
        await AuthorizeRequestAsync(TableOperation.Query, null, cancellationToken).ConfigureAwait(false);

        IQueryable<TEntity> dataset = (await Repository.AsQueryableAsync(cancellationToken).ConfigureAwait(false))
            .ApplyDataView(AccessControlProvider.GetDataView())
            .ApplyDeletedView(Request, Options.EnableSoftDelete);

        ODataValidationSettings validationSettings = new() { MaxTop = Options.MaxTop };
        ODataQuerySettings querySettings = new() { PageSize = Options.PageSize, EnsureStableOrdering = true };
        ODataQueryContext queryContext = new(EdmModel, typeof(TEntity), new ODataPath());
        ODataQueryOptions<TEntity> queryOptions = new(queryContext, Request);

        try
        {
            queryOptions.Validate(validationSettings);
        }
        catch (ODataException validationException)
        {
            Logger.LogWarning("Query: Error when validating query: {Message}", validationException.Message);
            return BadRequest(validationException.Message);
        }

        return CatchAndLogException("Error while executing results - query: Possible client-side evaluation", () =>
        {
            IEnumerable<object> results = (IEnumerable<object>)queryOptions.ApplyTo(dataset, querySettings);
            int resultCount = results.Count();
            PagedResult pagedResult = results is IEnumerable<ISelectExpandWrapper> wrapper ? new(wrapper.Select(x => x.ToDictionary())) : new(results);

            IQueryable<TEntity> query = (IQueryable<TEntity>?)queryOptions.Filter?.ApplyTo(dataset, new ODataQuerySettings()) ?? dataset;
            int totalCount = query.Count();

            if (queryOptions.Count?.Value == true)
            {
                pagedResult.Count = totalCount;
            }

            int skip = (queryOptions.Skip?.Value ?? 0) + resultCount;
            if (queryOptions.Top != null)
            {
                int top = queryOptions.Top.Value - resultCount;
                pagedResult.NextLink = skip >= totalCount || top <= 0 ? null : Request.CreateNextLink(skip, top);
            }
            else
            {
                pagedResult.NextLink = skip >= totalCount ? null : Request.CreateNextLink(skip, 0);
            }

            return Ok(pagedResult);
        });
    }

    /// <summary>
    /// Retrieves an entity from the repository.
    /// </summary>
    /// <param name="id">The ID of the entity to be returned.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
    /// <returns>An <see cref="OkObjectResult"/> encapsulating the entity value.</returns>
    /// <exception cref="HttpException">Thrown if there is an HTTP exception, such as unauthorized usage.</exception>
    [HttpGet("{id}")]
    [ActionName("ReadAsync")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> ReadAsync([FromRoute] string id, CancellationToken cancellationToken = default)
    {
        Logger.LogInformation("ReadAsync: {id}", id);
        TEntity entity = await Repository.ReadAsync(id, cancellationToken).ConfigureAwait(false);
        if (!AccessControlProvider.EntityIsInView(entity))
        {
            Logger.LogWarning("ReadAsync: {id} statusCode=404 not in view", id);
            throw new HttpException(StatusCodes.Status404NotFound);
        }
        await AuthorizeRequestAsync(TableOperation.Read, entity, cancellationToken).ConfigureAwait(false);
        if (Options.EnableSoftDelete && entity.Deleted && !Request.ShouldIncludeDeletedItems())
        {
            Logger.LogWarning("ReadAsync: {id} statusCode=410 deleted", id);
            throw new HttpException(StatusCodes.Status410Gone);
        }
        Request.ParseConditionalRequest(entity, out _);
        Logger.LogInformation("ReadAsync: {id} {entity} returned", id, entity.ToJsonString());
        return Ok(entity);
    }

    /// <summary>
    /// Replaces the value of an entity within the repository with new data.
    /// </summary>
    /// <param name="id">The ID of the entity to be replaced.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
    /// <returns>An <see cref="OkObjectResult"/> encapsulating the new value of the entity.</returns>
    /// <exception cref="HttpException">Throw if there is an HTTP exception, such as unauthorized usage.</exception>
    [HttpPut("{id}")]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> ReplaceAsync([FromRoute] string id, CancellationToken cancellationToken = default)
    {
        Logger.LogInformation("CreateAsync");
        TEntity entity = await DeserializeJsonContent(cancellationToken).ConfigureAwait(false);
        Logger.LogInformation("ReplaceAsync: {id} {entity}", id, entity.ToJsonString());
        if (id != entity.Id)
        {
            Logger.LogWarning("ReplaceAsync: {id} statusCode=400 id mismatch", id);
            throw new HttpException(StatusCodes.Status400BadRequest);
        }
        TEntity existing = await Repository.ReadAsync(id, cancellationToken).ConfigureAwait(false);
        if (!AccessControlProvider.EntityIsInView(existing))
        {
            Logger.LogWarning("ReplaceAsync: {id} statusCode=404 not in view", id);
            throw new HttpException(StatusCodes.Status404NotFound);
        }
        await AuthorizeRequestAsync(TableOperation.Update, entity, cancellationToken).ConfigureAwait(false);
        if (Options.EnableSoftDelete && existing.Deleted && !Request.ShouldIncludeDeletedItems())
        {
            Logger.LogWarning("ReplaceAsync: {id} statusCode=410 deleted", id);
            throw new HttpException(StatusCodes.Status410Gone);
        }
        Request.ParseConditionalRequest(existing, out byte[] version);
        await AccessControlProvider.PreCommitHookAsync(TableOperation.Update, entity, cancellationToken).ConfigureAwait(false);
        await Repository.ReplaceAsync(entity, version, cancellationToken).ConfigureAwait(false);
        await PostCommitHookAsync(TableOperation.Update, entity, cancellationToken).ConfigureAwait(false);
        Logger.LogInformation("ReplaceAsync: {id} {entity} replaced", id, entity.ToJsonString());
        return Ok(entity);
    }
}
