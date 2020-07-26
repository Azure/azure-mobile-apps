using Azure.Mobile.Server;
using Azure.Mobile.Server.Entity;
using E2EServer.Database;
using E2EServer.DataObjects;
using Microsoft.AspNetCore.Mvc;

namespace E2EServer.Controllers
{
    [Route("tables/[controller]")]
    [ApiController]
    public class HUnitsController : TableController<HUnit>
    {
        public HUnitsController(E2EDbContext context)
        {
            TableControllerOptions = new TableControllerOptions<HUnit> { SoftDeleteEnabled = false };
            TableRepository = new EntityTableRepository<HUnit>(context);
        }
    }
}
