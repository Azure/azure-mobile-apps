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
    public class RoundTripTableController : TableController<RoundTripTableItem>
    {
        public RoundTripTableController(AppDbContext context)
        {
            TableRepository = new EntityTableRepository<RoundTripTableItem>(context);
        }

        // TODO: Patch has a wierd semantic that needs to be dealt with
    }
}
