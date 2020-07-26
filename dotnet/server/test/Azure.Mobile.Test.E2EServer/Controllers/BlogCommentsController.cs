using Azure.Mobile.Server;
using Azure.Mobile.Server.Entity;
using Azure.Mobile.Test.E2EServer.Database;
using Azure.Mobile.Test.E2EServer.DataObjects;
using Microsoft.AspNetCore.Mvc;

namespace Azure.Mobile.Test.E2EServer.Controllers
{
    [Route("/tables/blog_comments")]
    [ApiController]
    public class BlogCommentsController : TableController<BlogComment>
    {
        public BlogCommentsController(TableServiceContext dbContext)
        {
            TableRepository = new EntityTableRepository<BlogComment>(dbContext);
        }
    }
}
