using Azure.Mobile.Server;
using Azure.Mobile.Server.Entity;
using Azure.Mobile.Test.E2EServer.Database;
using Azure.Mobile.Test.E2EServer.DataObjects;
using Microsoft.AspNetCore.Mvc;

namespace Azure.Mobile.Test.E2EServer.Controllers
{
    [Route("/tables/[controller]")]
    [ApiController]
    public class OfflineReadyController : TableController<OfflineReady>
    {
        public OfflineReadyController(TableServiceContext dbContext)
        {
            TableRepository = new EntityTableRepository<OfflineReady>(dbContext);
        }
    }
}
