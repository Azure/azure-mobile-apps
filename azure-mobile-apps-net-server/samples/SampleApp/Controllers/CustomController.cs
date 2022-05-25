// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------------------- 

using System.Web.Http;
using Microsoft.Azure.Mobile.Server.Config;

namespace Local.Controllers
{
    /// <summary>
    /// The custom controller allows for whatever you want...
    /// </summary>
    [MobileAppController]
    public class CustomController : ApiController
    {
        public string Get()
        {
            this.Configuration.Services.GetTraceWriter().Info("Hello from custom controller!");
            return "Hello from custom controller!";
        }
    }
}