using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.OData;
using Microsoft.Azure.Mobile.Server;
using ZumoE2EServerApp.DataObjects;
using ZumoE2EServerApp.Models;

namespace ZumoE2EServerApp.Controllers
{
    public class blog_postsController : TableController<BlogPost>
    {
        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);
            SDKClientTestContext context = new SDKClientTestContext();
            DomainManager = new EntityDomainManager<BlogPost>(context, Request);
        }

        // GET tables/blog_posts
        public IQueryable<BlogPost> Get()
        {
            return Query();
        }

        // GET tables/blog_posts/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public SingleResult<BlogPost> Get(string id)
        {
            return Lookup(id);
        }

        // PATCH tables/blog_posts/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public Task<BlogPost> Patch(string id, Delta<BlogPost> patch)
        {
            return UpdateAsync(id, patch);
        }

        // POST tables/blog_posts
        public async Task<IHttpActionResult> Post(BlogPost item)
        {
            BlogPost current = await InsertAsync(item);
            return CreatedAtRoute("Tables", new { id = current.Id }, current);
        }

        // DELETE tables/blog_posts/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public Task Delete(string id)
        {
            return DeleteAsync(id);
        }
    }
}