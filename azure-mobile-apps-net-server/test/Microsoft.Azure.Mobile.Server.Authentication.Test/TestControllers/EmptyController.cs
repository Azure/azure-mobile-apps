// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------------------- 

using System.Net.Http;
using System.Web.Http;

namespace Microsoft.Azure.Mobile.Server.Mocks
{
    public class EmptyController : ApiController
    {
        public EmptyController()
        {
            this.Request = new HttpRequestMessage();
        }
    }
}
