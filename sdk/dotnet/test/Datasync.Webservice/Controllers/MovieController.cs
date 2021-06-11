// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Test.Models;
using Microsoft.AspNetCore.Datasync;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;

namespace Datasync.Webservice.Controllers
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
}
