using Azure.Mobile.Server.Entity;
using Azure.Mobile.Server.Test.E2EServer.Database;
using Azure.Mobile.Server.Test.E2EServer.DataObjects;
using Microsoft.AspNetCore.Mvc;

namespace Azure.Mobile.Server.Test.E2EServer.Controllers
{
    [Route("tables/movies")]
    [ApiController]
    public class MoviesController : TableController<Movie>
    {
        public MoviesController(E2EDbContext context)
        {
            TableRepository = new EntityTableRepository<Movie>(context);
        }

        public override bool IsAuthorized(TableOperation operation, Movie item)
        {
            return operation == TableOperation.Read || operation == TableOperation.List;
        }
    }
}
