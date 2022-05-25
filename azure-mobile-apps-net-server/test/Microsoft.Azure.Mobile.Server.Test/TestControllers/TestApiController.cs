// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System.Net;
using System.Net.Http;
using System.Web.Http;
using Microsoft.Azure.Mobile.Server.Config;

namespace Microsoft.Azure.Mobile.Server.TestControllers
{
    [MobileAppController]
    public class TestApiController : ApiController
    {
        public string Get()
        {
            return "hello world";
        }

        [Route("api/attribute/test")]
        public HttpResponseMessage GetWithAttribute()
        {
            return new HttpResponseMessage(HttpStatusCode.OK);
        }
    }
}