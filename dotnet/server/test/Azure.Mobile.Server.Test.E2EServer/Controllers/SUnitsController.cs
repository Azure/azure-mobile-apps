using Azure.Mobile.Server.Entity;
using Azure.Mobile.Server.Test.E2EServer.Database;
using Azure.Mobile.Server.Test.E2EServer.DataObjects;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Azure.Mobile.Server.Test.E2EServer.Controllers
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
