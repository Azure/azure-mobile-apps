// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Web.Http;
using Microsoft.Azure.Mobile.Server.Config;
using Newtonsoft.Json.Linq;

namespace Microsoft.Azure.Mobile.Server.TestControllers
{
    [Authorize]
    public class SecuredController : ApiController
    {
        [AllowAnonymous]
        [Route("auth/anonymous")]
        [HttpGet]
        public IHttpActionResult AuthNotRequired()
        {
            return this.GetUserDetails();
        }

        [Route("auth/authorize")]
        [HttpGet]
        public string AuthRequired()
        {
            return this.Request.Headers.GetValues("x-zumo-auth").FirstOrDefault<string>();
        }

        private IHttpActionResult GetUserDetails()
        {
            ClaimsPrincipal user = this.User as ClaimsPrincipal;
            JObject details = null;
            if (user != null)
            {
                Claim userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);
                string userId = null;
                if (userIdClaim != null)
                {
                    userId = userIdClaim.Value;
                }
                details = new JObject
                {
                    { "id", userId }
                };
            }

            return this.Json(details);
        }
    }
}