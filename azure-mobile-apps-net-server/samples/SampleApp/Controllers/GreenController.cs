// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------------------- 

using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.OData;
using Local.DataObjects;
using Local.Models;
using Microsoft.Azure.Mobile.Server;
using Microsoft.Azure.Mobile.Server.Config;

namespace Local.Controllers
{
    public class GreenController : TableController<Green>
    {
        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);
            IMobileAppSettingsProvider provider = controllerContext.Configuration.GetMobileAppSettingsProvider();
            GreenContext context = new GreenContext();
            DomainManager = new EntityDomainManager<Green>(context, Request, enableSoftDelete: true);
        }

        [Queryable(PageSize = 2)]
        public IQueryable<Green> GetGreens()
        {
            return Query();
        }

        public SingleResult<Green> GetGreen(string id)
        {
            return Lookup(id);
        }

        public Task<Green> PatchGreen(string id, Delta<Green> patch)
        {
            return UpdateAsync(id, patch);
        }

        public async Task<IHttpActionResult> PostTodoItem(Green item)
        {
            Green current = await InsertAsync(item);
            return CreatedAtRoute("Tables", new { id = current.Id }, current);
        }

        public Task<Green> PostUndeleteGreen(string id)
        {
            return UndeleteAsync(id);
        }

        public Task DeleteGreen(string id)
        {
            return DeleteAsync(id);
        }
    }
}