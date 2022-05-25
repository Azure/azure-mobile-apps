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
    public class DatesController : TableController<Dates>
    {
        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);
            SDKClientTestContext context = new SDKClientTestContext();
            DomainManager = new EntityDomainManager<Dates>(context, Request);
        }

        // GET tables/dates
        public IQueryable<Dates> GetAll()
        {
            return Query();
        }

        // GET tables/dates/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public SingleResult<Dates> Get(string id)
        {
            return Lookup(id);
        }

        // PATCH tables/dates/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public Task<Dates> Patch(string id, Delta<Dates> patch)
        {
            return UpdateAsync(id, patch);
        }

        // POST tables/dates
        public async Task<IHttpActionResult> Post(Dates item)
        {
            Dates current = await InsertAsync(item);
            return CreatedAtRoute("Tables", new { id = current.Id }, current);
        }

        // DELETE tables/dates/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public Task Delete(string id)
        {
            return DeleteAsync(id);
        }
    }
}