// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------------------- 

using System;
using System.Data.Entity;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.OData;
using Microsoft.Azure.Mobile.Server;

namespace Local.Models
{
    public class BrownOnlineDomainManager : MappedEntityDomainManager<BrownOnline, Order>
    {
        private BrownContext context;

        public BrownOnlineDomainManager(BrownContext context, HttpRequestMessage request)
            : base(context, request, enableSoftDelete: true)
        {
            Request = request;
            this.context = context;
        }

        public override SingleResult<BrownOnline> Lookup(string id)
        {
            int orderId = GetKey<int>(id);
            return LookupEntity(o => o.OrderId == orderId);
        }

        public override async Task<BrownOnline> InsertAsync(BrownOnline data)
        {
            Customer[] customers = await this.context.Customers.Where(c => c.Name == data.CustomerName).ToArrayAsync();
            if (customers.Length == 0)
            {
                throw new HttpResponseException(Request.CreateBadRequestResponse("Customer with name '{0}' was not found", data.CustomerName));
            }

            data.CustomerId = customers.First().CustomerId;
            return await base.InsertAsync(data);
        }

        public override Task<BrownOnline> UpdateAsync(string id, Delta<BrownOnline> patch)
        {
            int orderId = GetKey<int>(id);
            return UpdateEntityAsync(patch, orderId);
        }

        public override Task<BrownOnline> UpdateAsync(string id, Delta<BrownOnline> patch, bool includeDeleted)
        {
            int orderId = GetKey<int>(id);
            return UpdateEntityAsync(patch, includeDeleted, orderId);
        }

        public override async Task<BrownOnline> ReplaceAsync(string id, BrownOnline data)
        {
            Customer[] customers = await this.context.Customers.Where(c => string.Equals(c.Name, data.CustomerName, StringComparison.Ordinal)).ToArrayAsync();
            if (customers.Length == 0)
            {
                throw new HttpResponseException(Request.CreateBadRequestResponse("Customer with name '{0}' was not found", data.CustomerName));
            }

            return await base.ReplaceAsync(id, data);
        }

        public override Task<bool> DeleteAsync(string id)
        {
            int orderId = GetKey<int>(id);
            return DeleteItemAsync(orderId);
        }
    }
}