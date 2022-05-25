// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System.Net;
using System.Net.Http;
using System.Web.Http;
using Microsoft.Azure.Mobile.Server.Tables;

namespace Microsoft.Azure.Mobile.Server.TestControllers
{
    public class TestTableController : TableController
    {
        public HttpResponseMessage Get()
        {
            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        public HttpResponseMessage Get(string id)
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new StringContent(id);
            return response;
        }

        [Route("tables/attribute/mapped")]
        public HttpResponseMessage GetWithAttribute()
        {
            return new HttpResponseMessage(HttpStatusCode.OK);
        }
    }
}