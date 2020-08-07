// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Zumo.Server.Entity;
using Microsoft.Zumo.Server.Test.E2EServer.Database;
using Microsoft.Zumo.Server.Test.E2EServer.DataObjects;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Zumo.Server.Test.E2EServer.Controllers
{
    [Route("tables/[controller]")]
    [ApiController]
    public class SUnitsController : TableController<SUnit>
    {
        public SUnitsController(E2EDbContext context)
        {
            TableControllerOptions = new TableControllerOptions<SUnit> { SoftDeleteEnabled = true };
            TableRepository = new EntityTableRepository<SUnit>(context);
        }
    }
}
