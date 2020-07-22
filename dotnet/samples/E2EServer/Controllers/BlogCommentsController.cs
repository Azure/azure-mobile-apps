using Azure.Mobile.Server;
using Azure.Mobile.Server.Entity;
using E2EServer.Database;
using E2EServer.DataObjects;
using Microsoft.AspNetCore.Mvc;

namespace E2EServer.Controllers
{
    [Route("tables/blog_comments")]
    [ApiController]
    public class BlogCommentsController : TableController<BlogComment>
    {
        public BlogCommentsController(E2EDbContext context)
        {
            TableRepository = new EntityTableRepository<BlogComment>(context);
        }
    }
}
