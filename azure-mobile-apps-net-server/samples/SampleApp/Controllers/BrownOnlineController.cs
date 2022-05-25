// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------------------- 

using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Description;
using System.Web.Http.OData;
using Local.Models;
using Microsoft.Azure.Mobile.Server;

namespace Local.Controllers
{
    public class BrownOnlineController : TableController<BrownOnline>
    {
        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);
            BrownContext context = new BrownContext();
            DomainManager = new BrownOnlineDomainManager(context, Request);
        }

        public IQueryable<BrownOnline> GetAllTodoItems()
        {
            return Query();
        }

        public SingleResult<BrownOnline> GetTodoItem(string id)
        {
            return Lookup(id);
        }

        public Task<BrownOnline> PatchTodoItem(string id, Delta<BrownOnline> patch)
        {
            return UpdateAsync(id, patch);
        }

        [ResponseType(typeof(BrownOnline))]
        public async Task<IHttpActionResult> PostTodoItem(BrownOnline item)
        {
            BrownOnline current = await InsertAsync(item);
            return CreatedAtRoute("Tables", new { id = current.Id }, current);
        }

        public Task<BrownOnline> PostUndeleteTodoItem(string id)
        {
            return UndeleteAsync(id);
        }

        public Task DeleteTodoItem(string id)
        {
            return DeleteAsync(id);
        }
    }
}