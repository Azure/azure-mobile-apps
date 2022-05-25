// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.OData;
using Microsoft.Azure.Mobile.Server.TestModels;

namespace Microsoft.Azure.Mobile.Server.TestControllers
{
    public class TestEntityController : TableController<TestEntity>
    {
        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);
            TestEntityContext context = new TestEntityContext();
            this.DomainManager = new EntityDomainManager<TestEntity>(context, Request);
            this.Request.RegisterForDispose(context);
        }

        public IQueryable<TestEntity> Get()
        {
            return this.Query();
        }

        public SingleResult<TestEntity> GetItem(string id)
        {
            return this.Lookup(id);
        }

        public Task<TestEntity> PatchItem(string id, Delta<TestEntity> patch)
        {
            return this.UpdateAsync(id, patch);
        }

        public async Task<IHttpActionResult> PostItem(TestEntity entity)
        {
            TestEntity current = await this.InsertAsync(entity);
            return this.CreatedAtRoute("Tables", new { id = current.Id }, current);
        }

        public Task<TestEntity> PutItem(string id, TestEntity item)
        {
            return this.ReplaceAsync(id, item);
        }

        public Task Delete(string id)
        {
            return this.DeleteAsync(id);
        }
    }
}