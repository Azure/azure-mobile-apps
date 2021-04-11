// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;
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
    [Route("tables/soft")]
    [ExcludeFromCodeCoverage(Justification = "Test suite")]
    public class SoftController : TableController<SoftMovie>
    {
        public SoftController(IRepository<SoftMovie> repository) : base(repository)
        {
            Options = new TableControllerOptions
            {
                EnableSoftDelete = true
            };
        }
    }

    /// <summary>
    /// The basic controller for doing query and read tests against a static
    /// data set.  The data set is a collection of 248 movies.
    /// </summary>
    [Route("tables/soft_logged")]
    [ExcludeFromCodeCoverage(Justification = "Test suite")]
    public class SoftLoggedController : TableController<SoftMovie>
    {
        public SoftLoggedController(IRepository<SoftMovie> repository, ILogger<SoftMovie> logger) : base(repository)
        {
            Options = new TableControllerOptions
            {
                EnableSoftDelete = true
            };
            Logger = logger;
        }
    }
}
