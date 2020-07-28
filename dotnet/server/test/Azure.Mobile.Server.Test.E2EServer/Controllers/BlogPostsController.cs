using Azure.Mobile.Server.Entity;
using Azure.Mobile.Server.Test.E2EServer.Database;
using Azure.Mobile.Server.Test.E2EServer.DataObjects;
using Microsoft.AspNetCore.Mvc;

namespace Azure.Mobile.Server.Test.E2EServer.Controllers
{
    [Route("tables/blog_posts")]
    [ApiController]
    public class BlogPostsController : TableController<BlogPost>
    {
        public BlogPostsController(E2EDbContext context)
        {
            TableRepository = new EntityTableRepository<BlogPost>(context);
        }
    }
}
