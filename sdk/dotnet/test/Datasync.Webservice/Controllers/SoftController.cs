// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;
using Datasync.Common.Test.Models;
using Microsoft.AspNetCore.Datasync;
using Microsoft.AspNetCore.Mvc;

namespace Datasync.Webservice.Controllers
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
}
