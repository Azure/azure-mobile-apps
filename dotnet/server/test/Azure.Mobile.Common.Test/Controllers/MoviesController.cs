using Azure.Mobile.Server;
using Azure.Mobile.Server.Entity;
using Microsoft.AspNetCore.Mvc;

namespace Azure.Mobile.Common.Test.Controllers
{
    /// <summary>
    /// This is the test controller that is used (and linked) to the TestHost
    /// </summary>
    [Route("/tables/[controller]")]
    [ApiController]
    public class MoviesController: TableController<Movie>
    {
        public MoviesController(MovieDbContext context)
        {
            TableControllerOptions = new TableControllerOptions<Movie>()
            {
                PageSize = 500,
                MaxTop = 500,
                SoftDeleteEnabled = true
            };
            TableRepository = new EntityTableRepository<Movie>(context);
        }
    }
}
