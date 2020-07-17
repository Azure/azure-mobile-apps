using Azure.Mobile.Server;
using Azure.Mobile.Server.Entity;
using Azure.Mobile.Test.E2EServer.Database;
using Azure.Mobile.Test.E2EServer.DataObjects;

namespace Azure.Mobile.Test.E2EServer.Controllers
{
    public class BlogPostsController : TableController<BlogPost>
    {
        public BlogPostsController(TableServiceContext dbContext)
        {
            TableRepository = new EntityTableRepository<BlogPost>(dbContext);
        }
    }
}
