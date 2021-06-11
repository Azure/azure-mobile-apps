// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Test.Models;
using Microsoft.AspNetCore.Datasync;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.CodeAnalysis;

namespace Datasync.Webservice.Controllers
{
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
}
