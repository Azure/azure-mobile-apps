using Azure.Mobile.Server.Entity;
using Azure.Mobile.Server.Test.E2EServer.Database;
using Azure.Mobile.Server.Test.E2EServer.DataObjects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Azure.Mobile.Server.Test.E2EServer.Controllers
{
    [Route("tables/unauthorized")]
    [ApiController]
    public class UnauthorizedController : TableController<Movie>
    {
        public UnauthorizedController(E2EDbContext context)
        {
            TableRepository = new EntityTableRepository<Movie>(context);
        }

        public override bool IsAuthorized(TableOperation operation, Movie item)
        {
            return HttpContext.User.Identity.IsAuthenticated;
        }
    }
}
