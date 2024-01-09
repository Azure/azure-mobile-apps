// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Datasync.Abstractions;
using Microsoft.AspNetCore.Datasync.Extensions;
using Microsoft.AspNetCore.Datasync.Filters;
using Microsoft.AspNetCore.Datasync.Models;
using Microsoft.AspNetCore.Datasync.Tables;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Query.Expressions;
using Microsoft.AspNetCore.OData.Query.Validator;
using Microsoft.AspNetCore.OData.Query.Wrapper;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace Microsoft.AspNetCore.Datasync;

/// <summary>
/// The core part of the datasync service; the <see cref="TableController{TEntity}"/> exposes the
/// endpoints that are used by the client to interact with the data.
/// </summary>
/// <typeparam name="TEntity">The type of the entity exposed to the client.</typeparam>
[DatasyncController]
public partial class TableController<TEntity> : ODataController where TEntity : class, ITableData
{
    #region Controller Constructors
    /// <summary>
    /// Creates a new <see cref="TableController{TEntity}"/>. The table options (such as repository,
    /// access control provider, etc.) are set via the upstream controller.
    /// </summary>
    public TableController()
        : this(new Repository<TEntity>(), new AccessControlProvider<TEntity>(), null, new TableControllerOptions())
    {
    }

    /// <summary>
    /// Creates a new <see cref="TableController{TEntity}"/> with the specified repository.
    /// </summary>
    /// <param name="repository">The repository to use for this controller.</param>
    public TableController(IRepository<TEntity> repository)
        : this(repository, new AccessControlProvider<TEntity>(), null, new TableControllerOptions())
    {
    }

    /// <summary>
    /// Creates a new <see cref="TableController{TEntity}"/> with the specified repository.
    /// </summary>
    /// <param name="repository">The repository to use for this controller.</param>
    /// <param name="model">The <see cref="IEdmModel"/> to use for OData interactions.</param>
    public TableController(IRepository<TEntity> repository, IEdmModel model)
        : this(repository, new AccessControlProvider<TEntity>(), model, new TableControllerOptions())
    {
    }

    /// <summary>
    /// Creates a new <see cref="TableController{TEntity}"/> with the specified repository.
    /// </summary>
    /// <param name="repository">The repository to use for this controller.</param>
    /// <param name="accessControlProvider">The access control provider to use for this controller.</param>
    public TableController(IRepository<TEntity> repository, IAccessControlProvider<TEntity> accessControlProvider)
        : this(repository, accessControlProvider, null, new TableControllerOptions())
    {
    }

    /// <summary>
    /// Creates a new <see cref="TableController{TEntity}"/> with the specified repository.
    /// </summary>
    /// <param name="repository">The repository to use for this controller.</param>
    /// <param name="options">The <see cref="TableControllerOptions"/> to use for configuring this controller.</param>
    public TableController(IRepository<TEntity> repository, TableControllerOptions options)
        : this(repository, new AccessControlProvider<TEntity>(), null, options)
    {
    }

    /// <summary>
    /// Creates a new <see cref="TableController{TEntity}"/> with the specified repository.
    /// </summary>
    /// <param name="repository">The repository to use for this controller.</param>
    /// <param name="accessControlProvider">The access control provider to use for this controller.</param>
    /// <param name="options">The <see cref="TableControllerOptions"/> to use for configuring this controller.</param>
    public TableController(IRepository<TEntity> repository, IAccessControlProvider<TEntity> accessControlProvider, TableControllerOptions options)
        : this(repository, accessControlProvider, null, options)
    {
    }

    /// <summary>
    /// Creates a new <see cref="TableController{TEntity}"/> with the specified repository.
    /// </summary>
    /// <param name="repository">The repository to use for this controller.</param>
    /// <param name="accessControlProvider">The access control provider to use for this controller.</param>
    /// <param name="model">The <see cref="IEdmModel"/> to use for OData interactions.</param>
    public TableController(IRepository<TEntity> repository, IAccessControlProvider<TEntity> accessControlProvider, IEdmModel model)
        : this(repository, accessControlProvider, model, new TableControllerOptions())
    {
    }
    #endregion

    /// <summary>
    /// Creates a new <see cref="TableController{TEntity}"/>.
    /// </summary>
    /// <param name="repository">The repository that will be used for data access operations.</param>
    /// <param name="accessControlProvider">The access control provider that will be used for authorizing requests.</param>
    /// <param name="options">The options for this table controller.</param>
    protected TableController(IRepository<TEntity> repository, IAccessControlProvider<TEntity> accessControlProvider, IEdmModel? edmModel, TableControllerOptions options)
    {
        Repository = repository;
        AccessControlProvider = accessControlProvider;
        Options = options;

        EdmModel = edmModel ?? ModelCache.GetEdmModel(typeof(TEntity));
        if (EdmModel.FindType(typeof(TEntity).FullName) == null)
        {
            throw new InvalidOperationException($"The type {typeof(TEntity).FullName} is not registered in the OData model");
        }
    }

    /// <summary>
    /// The access control provider that will be used for authorizing requests.
    /// </summary>
    public IAccessControlProvider<TEntity> AccessControlProvider { get; set; }

    /// <summary>
    /// The <see cref="IEdmModel"/> that is constructed for the service.
    /// </summary>
    public IEdmModel EdmModel { get; init; }

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
    [NonAction]
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
    /// Deserializes the body content when submitting an entity to the service, using the datasync serializer options.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
    /// <returns>A task that returns the entity when complete.</returns>
    /// <exception cref="HttpException">If the content is invalid.</exception>
    [NonAction]
    protected async ValueTask<TEntity> DeserializeJsonContent(CancellationToken cancellationToken = default)
    {
        IDatasyncServiceOptions options = HttpContext.RequestServices?.GetService<IDatasyncServiceOptions>() ?? new DatasyncServiceOptions();
        HttpContext.Request.EnableBuffering();
        if (HttpContext.Request.HasJsonContentType())
        {
            TEntity entity = await JsonSerializer.DeserializeAsync<TEntity>(HttpContext.Request.Body, options.JsonSerializerOptions, cancellationToken).ConfigureAwait(false)
                ?? throw new HttpException(StatusCodes.Status400BadRequest, "Invalid JSON content");
            List<ValidationResult> validationErrors = new();
            if (!Validator.TryValidateObject(entity, new ValidationContext(entity), validationErrors, true))
            {
                throw new HttpException(StatusCodes.Status400BadRequest, "Invalid entity") { Payload = validationErrors };
            }
            return entity;
        }
        else
        {
            throw new HttpException(StatusCodes.Status415UnsupportedMediaType, "Unsupported media type");
        }
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
