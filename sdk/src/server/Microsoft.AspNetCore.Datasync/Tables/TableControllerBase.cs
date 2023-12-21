// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Datasync.Abstractions;
using Microsoft.AspNetCore.Datasync.Extensions;
using Microsoft.AspNetCore.Datasync.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.OData.Edm;
using System.Text.Json;

namespace Microsoft.AspNetCore.Datasync.Tables;

public class TableControllerBase<TEntity> : ControllerBase where TEntity : class, ITableData
{
    protected TableControllerBase(IRepository<TEntity> repository, IAccessControlProvider<TEntity> accessControlProvider, IEdmModel? edmModel, TableControllerOptions options)
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
    /// Ensure that the proper error is transmitted to the client when an exception is thrown.
    /// </summary>
    /// <param name="message">The message to be sent.</param>
    /// <param name="act">The action to execute for which we are trapping errors.</param>
    /// <exception cref="HttpException">If the exception is one of the relevant exceptions.</exception>
    [NonAction]
    protected IActionResult CatchAndLogException(string message, Func<IActionResult> act)
    {
        try
        {
            return act.Invoke();
        }
        catch (Exception ex) when (ex is InvalidOperationException || ex is NotSupportedException)
        {
            Logger.LogWarning("{message}: {exception}", message, ex.Message);
            throw new HttpException(StatusCodes.Status417ExpectationFailed, $"{message}: {ex.Message}");
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
            return await JsonSerializer.DeserializeAsync<TEntity>(HttpContext.Request.Body, options.JsonSerializerOptions, cancellationToken).ConfigureAwait(false)
                ?? throw new HttpException(StatusCodes.Status400BadRequest, "Invalid JSON content");
        }
        else
        {
            throw new HttpException(StatusCodes.Status415UnsupportedMediaType, "Unsupported media type");
        }
    }
}
