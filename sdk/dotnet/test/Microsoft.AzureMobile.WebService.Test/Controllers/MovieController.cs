// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AzureMobile.Common.Test.Models;
using Microsoft.AzureMobile.Server;
using Microsoft.Extensions.Logging;

namespace Microsoft.AzureMobile.WebService.Test.Controllers
{
    /// <summary>
    /// The basic controller for doing query and read tests against a static
    /// data set.  The data set is a collection of 248 movies.
    /// </summary>
    [Route("tables/movies")]
    [ExcludeFromCodeCoverage(Justification = "Test suite")]
    public class MovieController : TableController<InMemoryMovie>
    {
        public MovieController(IRepository<InMemoryMovie> repository, ILogger<InMemoryMovie> logger) : base(repository)
        {
            Logger = logger;
        }
    }

    /// <summary>
    /// An adjusted movies controller that has a limited page size.
    /// </summary>
    [Route("tables/movies_pagesize")]
    [ExcludeFromCodeCoverage(Justification = "Test suite")]
    public class MovieWithPageSizeController : TableController<InMemoryMovie>
    {
        public MovieWithPageSizeController(IRepository<InMemoryMovie> repository) : base(repository)
        {
            Options = new TableControllerOptions { PageSize = 25 };
        }
    }

    [AllowAnonymous]
    [Route("tables/movies_rated")]
    [ExcludeFromCodeCoverage(Justification = "Test suite")]
    public class RRatedMoviesController : TableController<InMemoryMovie>, IAccessControlProvider<InMemoryMovie>
    {
        public RRatedMoviesController(IRepository<InMemoryMovie> repository, ILogger<InMemoryMovie> logger) : base(repository)
        {
            AccessControlProvider = this;
            Logger = logger;
        }

        /// <summary>
        /// Provides a function used in a LINQ <see cref="Where{TEntity}"/> clause to limit
        /// the data that the client can see.  Return null to provide all data.
        /// </summary>
        /// <returns>A LINQ <see cref="Where{TEntity}"/> function, or null for all data.</returns>
        public virtual Func<InMemoryMovie, bool> GetDataView() => movie => movie.Rating == "R";

        /// <summary>
        /// Determines if the user is authorized to see the data provided for the purposes of the operation.
        /// </summary>
        /// <param name="operation">The operation being performed</param>
        /// <param name="entity">The entity being handled (null if more than one entity)</param>
        /// <param name="token">A cancellation token</param>
        /// <returns>True if the client is allowed to perform the operation</returns>
        public virtual Task<bool> IsAuthorizedAsync(TableOperation operation, InMemoryMovie entity, CancellationToken token = default)
            => Task.FromResult(HttpContext.User?.Identity?.IsAuthenticated == true);

        /// <summary>
        /// Allows the user to set up any modifications that are necessary to support the chosen access control rules.
        /// Called immediately before writing to the data store.
        /// </summary>
        /// <param name="operation">The operation being performed</param>
        /// <param name="entity">The entity being handled</param>
        /// <param name="token">A cancellation token</param>
        public virtual Task PreCommitHookAsync(TableOperation operation, InMemoryMovie entity, CancellationToken token = default)
        {
            entity.Title = entity.Title.ToUpper();
            return Task.CompletedTask;
        }
    }

    [AllowAnonymous]
    [Route("tables/movies_legal")]
    [ExcludeFromCodeCoverage(Justification = "Test suite")]
    public class LegalMoviesController : TableController<InMemoryMovie>, IAccessControlProvider<InMemoryMovie>
    {
        public LegalMoviesController(IRepository<InMemoryMovie> repository) : base(repository)
        {
            AccessControlProvider = this;
            Options = new TableControllerOptions { UnauthorizedStatusCode = StatusCodes.Status451UnavailableForLegalReasons };
        }

        /// <summary>
        /// Provides a function used in a LINQ <see cref="Where{TEntity}"/> clause to limit
        /// the data that the client can see.  Return null to provide all data.
        /// </summary>
        /// <returns>A LINQ <see cref="Where{TEntity}"/> function, or null for all data.</returns>
        public virtual Func<InMemoryMovie, bool> GetDataView() => movie => movie.Rating == "R";

        /// <summary>
        /// Determines if the user is authorized to see the data provided for the purposes of the operation.
        /// </summary>
        /// <param name="operation">The operation being performed</param>
        /// <param name="entity">The entity being handled (null if more than one entity)</param>
        /// <param name="token">A cancellation token</param>
        /// <returns>True if the client is allowed to perform the operation</returns>
        public virtual Task<bool> IsAuthorizedAsync(TableOperation operation, InMemoryMovie entity, CancellationToken token = default)
            => Task.FromResult(HttpContext.User?.Identity?.IsAuthenticated == true);

        /// <summary>
        /// Allows the user to set up any modifications that are necessary to support the chosen access control rules.
        /// Called immediately before writing to the data store.
        /// </summary>
        /// <param name="operation">The operation being performed</param>
        /// <param name="entity">The entity being handled</param>
        /// <param name="token">A cancellation token</param>
        public virtual Task PreCommitHookAsync(TableOperation operation, InMemoryMovie entity, CancellationToken token = default)
        {
            entity.Title = entity.Title.ToUpper();
            return Task.CompletedTask;
        }
    }
}
