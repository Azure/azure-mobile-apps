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
    public class IntIdRoundTripTableController : TableController<IntIdRoundTripTableItemDto>
    {
        private ITraceWriter traceWriter;

        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);
            var context = new SDKClientTestContext();

            this.DomainManager = new Int64IdMappedEntityDomainManager<IntIdRoundTripTableItemDto, IntIdRoundTripTableItem>(context, Request);
            this.traceWriter = this.Configuration.Services.GetTraceWriter();
        }

        public IQueryable<IntIdRoundTripTableItemDto> GetAllRoundTrips()
        {
            this.traceWriter.Info("IntId:GetAllRoundTrips");
            return Query();
        }

        public SingleResult<IntIdRoundTripTableItemDto> GetRoundTrip(int id)
        {
            this.traceWriter.Info("IntId:GetRoundTrip:" + id.ToString());
            return Lookup(id.ToString());
        }

        public Task<IntIdRoundTripTableItemDto> PatchRoundTrip(int id, Delta<IntIdRoundTripTableItemDto> patch)
        {
            this.traceWriter.Info("IntId:PatchRoundTrip:" + id.ToString());
            return UpdateAsync(id.ToString(), patch);
        }

        public Task<IntIdRoundTripTableItemDto> PostRoundTrip(IntIdRoundTripTableItemDto item)
        {
            this.traceWriter.Info("IntId:PostRoundTrip:" + item.Id);
            return InsertAsync(item);
        }

        public Task DeleteRoundTrip(int id)
        {
            this.traceWriter.Info("IntId:DeleteRoundTrip:" + id.ToString());
            return DeleteAsync(id.ToString());
        }
    }
}