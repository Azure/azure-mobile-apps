// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Datasync.Abstractions;
using Microsoft.AspNetCore.Datasync.Extensions;
using Microsoft.AspNetCore.Datasync.Filters;
using Microsoft.AspNetCore.Datasync.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Query.Validator;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Linq.Expressions;

namespace Microsoft.AspNetCore.Datasync;

/// <summary>
/// The core part of the datasync service; the <see cref="TableController{TEntity}"/> exposes the
/// endpoints that are used by the client to interact with the data.
/// </summary>
/// <typeparam name="TEntity">The type of the entity exposed to the client.</typeparam>
[DatasyncController]
public class TableController<TEntity> : ODataController where TEntity : class, ITableData
{
    /// <summary>
    /// Creates a new <see cref="TableController{TEntity}"/>. The table options (such as repository,
    /// access control provider, etc.) are set via the upstream controller.
    /// </summary>
    public TableController() : this(new Repository<TEntity>(), new AccessControlProvider<TEntity>(), new TableControllerOptions())
    {
    }

    /// <summary>
    /// Creates a new <see cref="TableController{TEntity}"/> with the specified repository.
    /// </summary>
    /// <param name="repository">The repository to use for this </param>
    public TableController(IRepository<TEntity> repository) : this(repository, new AccessControlProvider<TEntity>(), new TableControllerOptions())
    {
    }

    public TableController(IRepository<TEntity> repository, IAccessControlProvider<TEntity> accessControlProvider) : this(repository, accessControlProvider, new TableControllerOptions())
    {
    }

    public TableController(IRepository<TEntity> repository, TableControllerOptions options) : this(repository, new AccessControlProvider<TEntity>(), options)
    {
    }

    /// <summary>
    /// Creates a new <see cref="TableController{TEntity}"/>.
    /// </summary>
    /// <param name="repository">The repository that will be used for data access operations.</param>
    /// <param name="accessControlProvider">The access control provider that will be used for authorizing requests.</param>
    /// <param name="options">The options for this table controller.</param>
    public TableController(IRepository<TEntity> repository, IAccessControlProvider<TEntity> accessControlProvider, TableControllerOptions options)
    {
        Repository = repository;
        AccessControlProvider = accessControlProvider;
        Options = options;
    }

    /// <summary>
    /// The access control provider that will be used for authorizing requests.
    /// </summary>
    public IAccessControlProvider<TEntity> AccessControlProvider { get; set; }

    /// <summary>
    /// The logger that is used for logging request/response information.
    /// </summary>
    public ILogger Logger { get; set; } = NullLogger.Instance;

    /// <summary>
    /// The options for this table controller.
    /// </summary>
    public TableControllerOptions Options { get; set; }

    /// <summary>
    /// The repository that will be used for data access operations.
    /// </summary>
    public IRepository<TEntity> Repository { get; set; }

    /// <summary>
    /// An event handler to use for receiving notifications when the repository is updated.
    /// </summary>
    public event EventHandler<RepositoryUpdatedEventArgs>? RepositoryUpdated;

    /// <summary>
    /// Checks that the requestor is authorized to perform the requested operation on the provided entity.
    /// </summary>
    /// <param name="operation">The operation to be performed.</param>
    /// <param name="entity">The entity (pre-modification) to be operated on (null for query).</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
    /// <returns>A task that completes when the authorization check is finished.</returns>
    /// <exception cref="HttpException">Thrown if the requestor is not authorized to perform the operation.</exception>
    protected virtual async ValueTask AuthorizeRequestAsync(TableOperation operation, TEntity? entity, CancellationToken cancellationToken = default)
    {
        bool isAuthorized = await AccessControlProvider.IsAuthorizedAsync(operation, entity, cancellationToken).ConfigureAwait(false);
        if (!isAuthorized)
        {
            Logger.LogWarning("{operation} {entity} statusCode=401 unauthorized", operation, entity?.ToJsonString() ?? "");
            throw new HttpException(Options.UnauthorizedStatusCode);
        }
    }

    /// <summary>
    /// Handles post-commit operation event handlers.
    /// </summary>
    /// <param name="operation">The operation being performed.</param>
    /// <param name="entity">The entity that was updated (except for a hard-delete, which is the entity before deletion)</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
    /// <returns>A taks the completes when the post-commit hook has been called.</returns>
    [NonAction]
    protected virtual ValueTask PostCommitHookAsync(TableOperation operation, TEntity entity, CancellationToken cancellationToken = default)
    {
        RepositoryUpdatedEventArgs args = new(operation, typeof(TEntity).Name, entity);
        RepositoryUpdated?.Invoke(this, args);
        return AccessControlProvider.PostCommitHookAsync(operation, entity, cancellationToken);
    }

    /// <summary>
    /// Creates a new entity in the repository.
    /// </summary>
    /// <param name="entity">The entity to be created.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
    /// <returns>A <see cref="CreatedAtActionResult"/> for the created entity.</returns>
    /// <exception cref="HttpException">Thrown if there is an HTTP exception, such as unauthorized usage.</exception>
    [HttpPost]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public virtual async Task<IActionResult> CreateAsync([FromBody] TEntity entity, CancellationToken cancellationToken = default)
    {
        Logger.LogInformation("CreateAsync: {entity}", entity.ToJsonString());
        await AuthorizeRequestAsync(TableOperation.Create, entity, cancellationToken).ConfigureAwait(false);
        await AccessControlProvider.PreCommitHookAsync(TableOperation.Create, entity, cancellationToken).ConfigureAwait(false);
        await Repository.CreateAsync(entity, cancellationToken).ConfigureAwait(false);
        Logger.LogInformation("CreateAsync: {entity}", entity.ToJsonString());
        await PostCommitHookAsync(TableOperation.Create, entity, cancellationToken).ConfigureAwait(false);
        return CreatedAtAction(nameof(ReadAsync), new { id = entity.Id }, entity);
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
    /// Updates an existing entity according to the provided patch document.
    /// </summary>
    /// <param name="id">The ID of the entity being modified.</param>
    /// <param name="patchDocument">The delta patch document.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
    /// <returns>An <see cref="OkObjectResult"/> encapsulating the resulting entity.</returns>
    /// <exception cref="HttpException">Thrown if there is an HTTP exception, such as unauthorized usage.</exception>
    [HttpPatch("{id}")]
    [Consumes("application/json-patch+json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> PatchAsync(string id, [FromBody] JsonPatchDocument<TEntity> patchDocument, CancellationToken cancellationToken = default)
    {
        Logger.LogInformation("PatchAsync: {id}", id);
        TEntity entity = await Repository.ReadAsync(id, cancellationToken).ConfigureAwait(false);
        if (!AccessControlProvider.EntityIsInView(entity))
        {
            Logger.LogWarning("PatchAsync: {id} statusCode=404 not in view", id);
            throw new HttpException(StatusCodes.Status404NotFound);
        }
        if (patchDocument.ModifiesSystemProperties(entity, out Dictionary<string, string[]> systemPropertyValidationErrors))
        {
            Logger.LogWarning("PatchAsync: {id} Patch documnet changes system properties", id);
            return ValidationProblem(new ValidationProblemDetails(systemPropertyValidationErrors));
        }
        await AuthorizeRequestAsync(TableOperation.Update, entity, cancellationToken).ConfigureAwait(false);
        if (Options.EnableSoftDelete && entity.Deleted && !patchDocument.Contains("replace", "/deleted", false))
        {
            Logger.LogWarning("PatchAsync: {id} statusCode=410 deleted", id);
            throw new HttpException(StatusCodes.Status410Gone);
        }
        Request.ParseConditionalRequest(entity, out byte[] version);
        patchDocument.ApplyTo(entity);
        if (!TryValidateModel(entity))
        {
            return ValidationProblem(new ValidationProblemDetails(ModelState));
        }
        await AccessControlProvider.PreCommitHookAsync(TableOperation.Update, entity, cancellationToken).ConfigureAwait(false);
        await Repository.ReplaceAsync(entity, version, cancellationToken).ConfigureAwait(false);
        await PostCommitHookAsync(TableOperation.Update, entity, cancellationToken).ConfigureAwait(false);
        Logger.LogInformation("PatchAsync: {id} {entity} patched", id, entity.ToJsonString());
        return Ok(entity);
    }

    /// <summary>
    /// Perform an OData query against the data set.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
    /// <returns>An <see cref="IQueryable{T}"/> for the data set under consideration.  This is handled by the OData library.</returns>
    /// <exception cref="HttpException">Thrown if there is an HTTP exception, such as unauthorized usage.</exception>
    [EnableQuery]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public virtual async Task<IQueryable<TEntity>> QueryAsync(CancellationToken cancellationToken = default)
    {
        Logger.LogInformation("QueryAsync: {querystring}", HttpContext.Request.QueryString);
        await AuthorizeRequestAsync(TableOperation.Query, null, cancellationToken).ConfigureAwait(false);
        IQueryable<TEntity> dataset = await Repository.AsQueryableAsync(cancellationToken).ConfigureAwait(false);
        Expression<Func<TEntity, bool>>? filter = AccessControlProvider.GetDataView();
        if (filter != null)
        {
            dataset = dataset.Where(filter);
        }
        if (!Request.ShouldIncludeDeletedItems())
        {
            dataset = dataset.Where(m => !m.Deleted);
        }
        return dataset;
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
    /// <param name="entity">The new value for the entity.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
    /// <returns>An <see cref="OkObjectResult"/> encapsulating the new value of the entity.</returns>
    /// <exception cref="HttpException">Throw if there is an HTTP exception, such as unauthorized usage.</exception>
    [HttpPut("{id}")]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> ReplaceAsync([FromRoute] string id, [FromBody] TEntity entity, CancellationToken cancellationToken = default)
    {
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
        if (Options.EnableSoftDelete && entity.Deleted && !Request.ShouldIncludeDeletedItems())
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
