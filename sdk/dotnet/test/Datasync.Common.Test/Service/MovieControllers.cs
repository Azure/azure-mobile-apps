// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Test.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Datasync;
using Microsoft.AspNetCore.Datasync.EFCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;

namespace Datasync.Common.Test.Service;

[Route("tables/movies")]
[ExcludeFromCodeCoverage(Justification = "Test suite")]
public class MovieController : TableController<EFMovie>
{
    public MovieController(MovieDbContext context, ILogger<EFMovie> logger) : base()
    {
        Repository = new EntityTableRepository<EFMovie>(context);
        Logger = logger;
    }
}

/// <summary>
/// A movies controller that has a limited page size.
/// </summary>
[Route("tables/movies_pagesize")]
[ExcludeFromCodeCoverage(Justification = "Test suite")]
public class MovieWithPageSizeController : TableController<EFMovie>
{
    public MovieWithPageSizeController(MovieDbContext context) : base()
    {
        Repository = new EntityTableRepository<EFMovie>(context);
        Options = new TableControllerOptions { PageSize = 25 };
    }
}

/// <summary>
/// A movies controller that only shows R Rated movies.
/// </summary>
[AllowAnonymous]
[Route("tables/movies_rated")]
[ExcludeFromCodeCoverage(Justification = "Test suite")]
public class RRatedMoviesController : TableController<EFMovie>, IAccessControlProvider<EFMovie>
{
    public RRatedMoviesController(MovieDbContext context, ILogger<EFMovie> logger) : base()
    {
        Repository = new EntityTableRepository<EFMovie>(context);
        AccessControlProvider = this;
        Logger = logger;
    }

    /// <summary>
    /// Provides a function used in a LINQ <see cref="Where{TEntity}"/> clause to limit
    /// the data that the client can see.  Return null to provide all data.
    /// </summary>
    /// <returns>A LINQ <see cref="Where{TEntity}"/> function, or null for all data.</returns>
    public virtual Expression<Func<EFMovie, bool>> GetDataView() => movie => movie.Rating == "R";

    /// <summary>
    /// Determines if the user is authorized to see the data provided for the purposes of the operation.
    /// </summary>
    /// <param name="operation">The operation being performed</param>
    /// <param name="entity">The entity being handled (null if more than one entity)</param>
    /// <param name="token">A cancellation token</param>
    /// <returns>True if the client is allowed to perform the operation</returns>
    public virtual Task<bool> IsAuthorizedAsync(TableOperation operation, EFMovie entity, CancellationToken token = default)
        => Task.FromResult(HttpContext.User?.Identity?.IsAuthenticated == true);

    /// <summary>
    /// Allows the user to set up any modifications that are necessary to support the chosen access control rules.
    /// Called immediately before writing to the data store.
    /// </summary>
    /// <param name="operation">The operation being performed</param>
    /// <param name="entity">The entity being handled</param>
    /// <param name="token">A cancellation token</param>
    public virtual Task PreCommitHookAsync(TableOperation operation, EFMovie entity, CancellationToken token = default)
    {
        entity.Title = entity.Title.ToUpper();
        return Task.CompletedTask;
    }
}

/// <summary>
/// A movies controller that returns "Unavailable for Legal Reasons".
/// </summary>
[AllowAnonymous]
[Route("tables/movies_legal")]
[ExcludeFromCodeCoverage(Justification = "Test suite")]
public class LegalMoviesController : TableController<EFMovie>, IAccessControlProvider<EFMovie>
{
    public LegalMoviesController(MovieDbContext context) : base()
    {
        Repository = new EntityTableRepository<EFMovie>(context);
        AccessControlProvider = this;
        Options = new TableControllerOptions { UnauthorizedStatusCode = StatusCodes.Status451UnavailableForLegalReasons };
    }

    /// <summary>
    /// Provides a function used in a LINQ <see cref="Where{TEntity}"/> clause to limit
    /// the data that the client can see.  Return null to provide all data.
    /// </summary>
    /// <returns>A LINQ <see cref="Where{TEntity}"/> function, or null for all data.</returns>
    public virtual Expression<Func<EFMovie, bool>> GetDataView() => movie => movie.Rating == "R";

    /// <summary>
    /// Determines if the user is authorized to see the data provided for the purposes of the operation.
    /// </summary>
    /// <param name="operation">The operation being performed</param>
    /// <param name="entity">The entity being handled (null if more than one entity)</param>
    /// <param name="token">A cancellation token</param>
    /// <returns>True if the client is allowed to perform the operation</returns>
    public virtual Task<bool> IsAuthorizedAsync(TableOperation operation, EFMovie entity, CancellationToken token = default)
        => Task.FromResult(HttpContext.User?.Identity?.IsAuthenticated == true);

    /// <summary>
    /// Allows the user to set up any modifications that are necessary to support the chosen access control rules.
    /// Called immediately before writing to the data store.
    /// </summary>
    /// <param name="operation">The operation being performed</param>
    /// <param name="entity">The entity being handled</param>
    /// <param name="token">A cancellation token</param>
    public virtual Task PreCommitHookAsync(TableOperation operation, EFMovie entity, CancellationToken token = default)
    {
        entity.Title = entity.Title.ToUpper();
        return Task.CompletedTask;
    }
}

/// <summary>
/// A movies controller with soft-delete turned on.
/// </summary>
[Route("tables/soft")]
[ExcludeFromCodeCoverage(Justification = "Test suite")]
public class SoftController : TableController<EFMovie>
{
    public SoftController(MovieDbContext context) : base()
    {
        Repository = new EntityTableRepository<EFMovie>(context);
        Options = new TableControllerOptions
        {
            EnableSoftDelete = true
        };
    }
}

/// <summary>
/// A movies controller with soft-delete turned on and logging.
/// </summary>
[Route("tables/soft_logged")]
[ExcludeFromCodeCoverage(Justification = "Test suite")]
public class SoftLoggedController : TableController<EFMovie>
{
    public SoftLoggedController(MovieDbContext context, ILogger<EFMovie> logger) : base()
    {
        Repository = new EntityTableRepository<EFMovie>(context);
        Options = new TableControllerOptions
        {
            EnableSoftDelete = true
        };
        Logger = logger;
    }
}
