// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System.Web.Http;
using Microsoft.Azure.Mobile.Server.Config;

namespace Microsoft.Azure.Mobile.Server.Swagger.Test.TestControllers
{
    [Authorize]
    [MobileAppController]
    public class AuthenticatedController : ApiController
    {
        public IHttpActionResult Get()
        {
            return this.Ok();
        }

        public IHttpActionResult Post()
        {
            return this.Ok();
        }
    }
}