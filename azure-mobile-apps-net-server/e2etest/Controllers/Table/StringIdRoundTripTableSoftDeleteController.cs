// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.OData;
using System.Web.Http.Tracing;
using Microsoft.Azure.Mobile.Server;
using ZumoE2EServerApp.DataObjects;
using ZumoE2EServerApp.Models;
using ZumoE2EServerApp.Utils;

namespace ZumoE2EServerApp.Controllers
{
    public class StringIdRoundTripTableSoftDeleteController : TableController<StringIdRoundTripTableSoftDeleteItem>
    {
        private ITraceWriter traceWriter;
        SDKClientTestContext context;
        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);
            this.context = new SDKClientTestContext();
            this.DomainManager = new EntityDomainManager<StringIdRoundTripTableSoftDeleteItem>(context, Request, true);

            this.traceWriter = this.Configuration.Services.GetTraceWriter();
        }

        public IQueryable<StringIdRoundTripTableSoftDeleteItem> GetAll()
        {
            this.traceWriter.Info("GetAllRoundTrips");
            return Query();
        }

        public SingleResult<StringIdRoundTripTableSoftDeleteItem> Get(string id)
        {
            this.traceWriter.Info("GetRoundTrip:" + id);
            return Lookup(id);
        }

        public Task<StringIdRoundTripTableSoftDeleteItem> Patch(string id, Delta<StringIdRoundTripTableSoftDeleteItem> patch)
        {
            this.traceWriter.Info("PatchRoundTrip:" + id);
            return UpdateAsync(id, patch);
        }

        public Task<StringIdRoundTripTableSoftDeleteItem> Post(StringIdRoundTripTableSoftDeleteItem item)
        {
            if (string.IsNullOrWhiteSpace(item.Id))
            {
                item.Id = null;
            }

            this.traceWriter.Info("PostRoundTrip:" + item.Id);
            return InsertAsync(item);
        }

        public Task<StringIdRoundTripTableSoftDeleteItem> PostUndeleteStringIdRoundTripTableSoftDeleteItem(string id)
        {
            return this.UndeleteAsync(id);
        }

        public Task Delete(string id)
        {
            this.traceWriter.Info("SoftDeleteRoundTrip:" + id);
            return DeleteAsync(id);
        }
    }
}