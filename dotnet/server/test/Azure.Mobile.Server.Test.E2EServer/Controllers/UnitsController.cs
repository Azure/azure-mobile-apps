using Azure.Mobile.Server.Entity;
using Azure.Mobile.Server.Test.E2EServer.Database;
using Azure.Mobile.Server.Test.E2EServer.DataObjects;
using Microsoft.AspNetCore.Mvc;

namespace Azure.Mobile.Server.Test.E2EServer.Controllers
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
