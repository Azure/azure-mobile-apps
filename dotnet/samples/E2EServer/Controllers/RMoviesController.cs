using Azure.Mobile.Server;
using Azure.Mobile.Server.Entity;
using E2EServer.Database;
using E2EServer.DataObjects;
using Microsoft.AspNetCore.Mvc;

namespace E2EServer.Controllers
{
    [Route("tables/rmovies")]
    [ApiController]
    public class RMoviesController : TableController<RMovie>
    {
        public RMoviesController(E2EDbContext context)
        {
            TableControllerOptions = new TableControllerOptions<RMovie>
            {
                SoftDeleteEnabled = true
            };
            TableRepository = new EntityTableRepository<RMovie>(context);
        }

        public override bool IsAuthorized(TableOperation operation, RMovie item)
        {
            return operation == TableOperation.Read || operation == TableOperation.List;
        }
    }
}
