// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.OData;
using Microsoft.Azure.Mobile.Server;
using ZumoE2EServerApp.DataObjects;
using ZumoE2EServerApp.Utils;

namespace ZumoE2EServerApp.Controllers
{
    public abstract class PermissionTableControllerBase : TableController<TestUser>
    {
        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);
            this.DomainManager = new InMemoryDomainManager<TestUser>();
        }

        public virtual Task<IQueryable<TestUser>> GetAll()
        {
            return Task.FromResult(Query());
        }

        public virtual Task<SingleResult<TestUser>> Get(string id)
        {
            return Task.FromResult(Lookup(id));
        }

        public virtual async Task<HttpResponseMessage> Patch(string id, Delta<TestUser> patch)
        {
            return this.Request.CreateResponse(HttpStatusCode.OK, await UpdateAsync(id, patch));
        }

        public virtual async Task<HttpResponseMessage> Post(TestUser item)
        {
            return this.Request.CreateResponse(HttpStatusCode.Created, await InsertAsync(item));
        }

        public virtual async Task<HttpResponseMessage> Delete(string id)
        {
            await this.DeleteAsync(id);
            return this.Request.CreateResponse(HttpStatusCode.NoContent);
        }
    }

    public class PublicController : PermissionTableControllerBase { }

    [Authorize]
    public class ApplicationController : PermissionTableControllerBase { }

    [Authorize]
    public class UserController : PermissionTableControllerBase { }

    [Authorize]
    public class AdminController : PermissionTableControllerBase { }
}