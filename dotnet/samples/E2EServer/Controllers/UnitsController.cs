using Azure.Mobile.Server;
using Azure.Mobile.Server.Entity;
using E2EServer.Database;
using E2EServer.DataObjects;
using Microsoft.AspNetCore.Mvc;

namespace E2EServer.Controllers
{
    [Route("tables/[controller]")]
    [ApiController]
    public class UnitsController : TableController<Unit>
    {
        public UnitsController(E2EDbContext context)
        {
            TableRepository = new EntityTableRepository<Unit>(context);
        }
    }
}
