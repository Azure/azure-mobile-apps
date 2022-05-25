// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------------------- 

using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Cors;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Cors;
using System.Web.Http.Description;
using Microsoft.Azure.Mobile.Server.Config;
using Microsoft.Azure.Mobile.Server.Content;

namespace Microsoft.Azure.Mobile.Server.Controllers
{
    /// <summary>
    /// This controller returns files that support cross-domain communication for older browsers.
    /// </summary>
    [ApiExplorerSettings(IgnoreApi = true)]
    [MobileAppController]
    public class CrossDomainController : ApiController
    {
        private static IList<string> originsCache;

        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);
        }

        /// <summary>
        ///  A page that, when hosted in an <c>iframe</c>, will accept postMessage messages from approved
        ///  origins and will forward them as a same-domain ajax request to the runtime. This is needed
        ///  for the <c>IframeBridge</c> transport in <c>MobileApps.Web.js</c>, as used by IE8-9.
        /// </summary>
        /// <param name="origin">Origin to verify</param>
        /// <returns>An <see cref="IHttpActionResult"/> representing the result.</returns>
        [HttpGet]
        [AllowAnonymous]
        public async Task<IHttpActionResult> Bridge(string origin)
        {
            await InitializeOrigins(this.Configuration, this.Request);

            if (CheckAllowedOrigin(origin))
            {
                return new StaticHtmlActionResult("Microsoft.Azure.Mobile.Server.Bridge.html", origin);
            }

            return this.Unauthorized();
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IHttpActionResult> LoginReceiver([FromUri(Name = "completion_origin")]string completionOrigin)
        {
            await InitializeOrigins(this.Configuration, this.Request);

            if (CheckAllowedOrigin(completionOrigin))
            {
                return new StaticHtmlActionResult("Microsoft.Azure.Mobile.Server.LoginIFrameReceiver.html", completionOrigin);
            }

            return this.Unauthorized();
        }

        /// <summary>
        /// Reset helper for unit test purposes.
        /// </summary>
        internal static void Reset()
        {
            originsCache = null;
        }

        internal static async Task<IList<string>> InitializeOrigins(HttpConfiguration config, HttpRequestMessage request)
        {
            if (originsCache == null)
            {
                IEnumerable<string> originsLookup = null;
                originsLookup = config.GetCrossDomainOrigins();

                // If CrossDomainOrigins have not been explicitly set, pull from the CorsPolicy
                if (originsLookup == null)
                {
                    // The CorsPolicy cannot be accessed unless the request has an Origin header so copy the 
                    // passed-in HttpRequestMessage and add one.
                    HttpRequestMessage requestWithOrigin = new HttpRequestMessage(request.Method, request.RequestUri);
                    requestWithOrigin.Headers.Add("Origin", string.Empty);

                    ICorsPolicyProvider corsPolicyProvider = config.GetCorsPolicyProviderFactory().GetCorsPolicyProvider(requestWithOrigin);

                    if (corsPolicyProvider != null)
                    {
                        CorsPolicy corsPolicy = await corsPolicyProvider.GetCorsPolicyAsync(request, new CancellationToken());
                        if (corsPolicy.AllowAnyOrigin)
                        {
                            originsLookup = new List<string> { "*" };
                        }
                        else
                        {
                            originsLookup = corsPolicy.Origins;
                        }
                    }
                    else
                    {
                        originsLookup = new List<string>();
                    }
                }

                Interlocked.CompareExchange(ref originsCache, originsLookup.ToList(), null);
            }

            return originsCache;
        }

        internal static bool CheckAllowedOrigin(string origin)
        {
            return originsCache.Contains(origin) || originsCache.Contains("*");
        }
    }
}
