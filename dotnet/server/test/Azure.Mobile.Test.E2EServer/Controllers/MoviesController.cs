using Azure.Mobile.Server;
using Azure.Mobile.Server.Entity;
using Azure.Mobile.Test.E2EServer.Database;
using Azure.Mobile.Test.E2EServer.DataObjects;
using Microsoft.AspNetCore.Mvc;

namespace Azure.Mobile.Test.E2EServer.Controllers
{
    [Route("/tables/[controller]")]
    [ApiController]
    public class MoviesController : TableController<Movie>
    {
        public MoviesController(TableServiceContext dbContext)
        {
            TableControllerOptions = new TableControllerOptions<Movie>()
            {
                MaxTop = 1000
            };
            TableRepository = new EntityTableRepository<Movie>(dbContext);
        }

        public override bool IsAuthorized(TableOperation operation, Movie item)
        {
            return operation == TableOperation.List || operation == TableOperation.Read;
        }
    }
}
