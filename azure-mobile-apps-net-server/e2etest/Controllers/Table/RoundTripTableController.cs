// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.OData;
using System.Web.Http.Tracing;
using Microsoft.Azure.Mobile.Server;
using ZumoE2EServerApp.DataObjects;
using ZumoE2EServerApp.Models;

namespace ZumoE2EServerApp.Controllers
{
    public class RoundTripTableController : TableController<RoundTripTableItem>
    {
        private SDKClientTestContext context;
        private ITraceWriter traceWriter;

        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);
            this.context = new SDKClientTestContext();
            this.DomainManager = new EntityDomainManager<RoundTripTableItem>(context, Request);
            this.traceWriter = this.Configuration.Services.GetTraceWriter();
        }

        public IQueryable<RoundTripTableItem> GetAll()
        {
            this.traceWriter.Info("GetAllRoundTrips");
            return Query();
        }

        public SingleResult<RoundTripTableItem> Get(string id)
        {
            this.traceWriter.Info("GetRoundTrip:" + id);
            return Lookup(id);
        }

        public async Task<RoundTripTableItem> Patch(string id, Delta<RoundTripTableItem> patch)
        {
            this.traceWriter.Info("PatchRoundTrip:" + id);
            const int NumAttempts = 5;

            HttpResponseException lastException = null;
            for (int i = 0; i < NumAttempts; i++)
            {
                try
                {
                    return await UpdateAsync(id, patch);
                }
                catch (HttpResponseException ex)
                {
                    lastException = ex;
                }

                if (lastException.Response != null && lastException.Response.StatusCode == HttpStatusCode.PreconditionFailed)
                {
                    // Handle conflict
                    var content = lastException.Response.Content as ObjectContent;
                    RoundTripTableItem serverItem = (RoundTripTableItem)content.Value;

                    KeyValuePair<string, string> kvp = this.Request.GetQueryNameValuePairs().FirstOrDefault(p => p.Key == "conflictPolicy");
                    if (kvp.Key == "conflictPolicy")
                    {
                        switch (kvp.Value)
                        {
                            case "clientWins":
                                this.traceWriter.Info("Client wins");
                                this.Request.Headers.IfMatch.Clear();
                                this.Request.Headers.IfMatch.Add(new EntityTagHeaderValue("\"" + Convert.ToBase64String(serverItem.Version) + "\""));
                                continue; // try again with the new ETag...
                            case "serverWins":
                                this.traceWriter.Info("Server wins");
                                return serverItem;
                        }
                    }
                }
                throw lastException;
            }

            throw lastException;
        }

        public Task<RoundTripTableItem> Post(RoundTripTableItem item)
        {
            if (string.IsNullOrWhiteSpace(item.Id))
            {
                item.Id = null;
            }
            this.traceWriter.Info("PostRoundTrip:" + item.Id);
            return InsertAsync(item);
        }

        public Task Delete(string id)
        {
            this.traceWriter.Info("DeleteRoundTrip:" + id);
            return DeleteAsync(id);
        }
    }
}