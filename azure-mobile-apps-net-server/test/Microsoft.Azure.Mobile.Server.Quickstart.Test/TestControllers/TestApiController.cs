// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System.Web.Http;
using Microsoft.Azure.Mobile.Server.Config;

namespace Microsoft.Azure.Mobile.Server.Quickstart.Test.TestControllers
{
    [MobileAppController]
    public class TestApiController : ApiController
    {
        [HttpGet]
        public string NoAttribute()
        {
            return "Hello";
        }
    }
}