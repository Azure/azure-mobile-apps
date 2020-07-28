using Azure.Mobile.Server;
using Azure.Mobile.Server.Entity;
using E2EServer.Database;
using E2EServer.DataObjects;
using Microsoft.AspNetCore.Mvc;

namespace E2EServer.Controllers
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
