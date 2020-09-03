
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Mvc;
using Microsoft.Zumo.E2EServer.DataObjects;
using Microsoft.Zumo.E2EServer.Models;
using Microsoft.Zumo.Server;
using Microsoft.Zumo.Server.Entity;

namespace Microsoft.Zumo.E2EServer.Controllers
{
    [Route("tables/[controller]")]
    [ApiController]
    public class OfflineReadyController : TableController<OfflineReady>
    {
        public OfflineReadyController(AppDbContext context)
        {
            TableRepository = new EntityTableRepository<OfflineReady>(context);
        }

        /// <summary>
        /// The LIST operation (GET .../route), which implements OData Semantics.
        /// </summary>
        /// <returns>The result of the LIST operation</returns>
        [HttpGet, ZumoQuery(MaxTop = 1000)]
        public override IActionResult GetItems()
            => Ok(QueryItems());
    }
}
