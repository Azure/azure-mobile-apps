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
            TableControllerOptions = new TableControllerOptions<OfflineReady>
            {
                MaxTop = 1000
            };
            TableRepository = new EntityTableRepository<OfflineReady>(context);
        }
    }
}
