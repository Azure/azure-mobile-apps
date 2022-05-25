// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------------------- 

using System.Diagnostics.CodeAnalysis;
using System.Web.Http;
using System.Web.Http.Description;
using Microsoft.Azure.Mobile.Server.Config;
using Microsoft.Azure.Mobile.Server.Content;

namespace Microsoft.Azure.Mobile.Server.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public class HomeController : ApiController
    {
        [HttpGet]
        [AllowAnonymous]
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "This is part of a Web API route")]
        public IHttpActionResult Index()
        {
            return new StaticHtmlActionResult("Microsoft.Azure.Mobile.Server.Home.Index.html");
        }
    }
}
