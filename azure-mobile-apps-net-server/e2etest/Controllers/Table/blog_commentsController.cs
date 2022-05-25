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
    public class blog_commentsController : TableController<BlogComments>
    {
        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);
            SDKClientTestContext context = new SDKClientTestContext();
            DomainManager = new EntityDomainManager<BlogComments>(context, Request);
        }

        // GET tables/blog_comments
        public IQueryable<BlogComments> GetAll()
        {
            return Query();
        }

        // GET tables/blog_comments/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public SingleResult<BlogComments> Get(string id)
        {
            return Lookup(id);
        }

        // PATCH tables/blog_comments/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public Task<BlogComments> Patch(string id, Delta<BlogComments> patch)
        {
            return UpdateAsync(id, patch);
        }

        // POST tables/blog_comments
        public async Task<IHttpActionResult> Post(BlogComments item)
        {
            BlogComments current = await InsertAsync(item);
            return CreatedAtRoute("Tables", new { id = current.Id }, current);
        }

        // DELETE tables/blog_comments/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public Task Delete(string id)
        {
            return DeleteAsync(id);
        }
    }
}