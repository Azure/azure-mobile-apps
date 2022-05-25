// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;
using Microsoft.Azure.Mobile.Server.Config;

namespace Microsoft.Azure.Mobile.Server.TestControllers
{
    [MobileAppController]
    public class CorsController : ApiController
    {
        [Route("api/cors/test")]
        public HttpResponseMessage Test()
        {
            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        [Route("api/cors/testattribute")]
        [EnableCors("*", "*", "POST")]
        public HttpResponseMessage TestAttribute()
        {
            return new HttpResponseMessage(HttpStatusCode.OK);
        }
    }
}