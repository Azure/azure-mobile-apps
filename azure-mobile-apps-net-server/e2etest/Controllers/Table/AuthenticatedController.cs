// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.OData;
using Microsoft.Azure.Mobile.Server.Authentication;
using Newtonsoft.Json.Linq;
using ZumoE2EServerApp.DataObjects;

namespace ZumoE2EServerApp.Controllers
{
    [Authorize]
    public class AuthenticatedController : PermissionTableControllerBase
    {
        public override async Task<IQueryable<TestUser>> GetAll()
        {
            ClaimsPrincipal user = (ClaimsPrincipal)this.User;
            string userId = user.FindFirst(ClaimTypes.NameIdentifier).Value;

            string identity = null;
            var fbcreds = await user.GetAppServiceIdentityAsync<FacebookCredentials>(this.Request);
            if (fbcreds != null && fbcreds.UserClaims.Any())
            {
                identity = "{\"facebook\":{\"access_token\":\"" + fbcreds.AccessToken + "\"}}";
            }

            var twitterCreds = await user.GetAppServiceIdentityAsync<TwitterCredentials>(this.Request);
            if (twitterCreds != null && twitterCreds.UserClaims.Any())
            {
                identity = "{\"twitter\":{\"access_token\":\"" + twitterCreds.AccessToken + "\",\"access_token_secret\":\"" + twitterCreds.AccessTokenSecret + "\"}}";
            }

            var googleCreds = await user.GetAppServiceIdentityAsync<GoogleCredentials>(this.Request);
            if (googleCreds != null && googleCreds.UserClaims.Any())
            {
                identity = "{\"google\":{\"access_token\":\"" + googleCreds.AccessToken + "\",\"authorization-code\":\"\"}}";
            }

            var msaCreds = await user.GetAppServiceIdentityAsync<MicrosoftAccountCredentials>(this.Request);
            if (msaCreds != null && msaCreds.UserClaims.Any())
            {
                identity = "{\"microsoftaccount\":{\"access_token\":\"" + msaCreds.AccessToken + "\"}}";
            }

            var aadCreds = await user.GetAppServiceIdentityAsync<AzureActiveDirectoryCredentials>(this.Request);
            if (aadCreds != null && aadCreds.UserClaims.Any())
            {
                identity = "{\"aad\":{\"access_token\":\"\"}}";
            }

            var all = (await base.GetAll()).Where(p => p.UserId == userId).ToArray();
            foreach (var item in all)
            {
                item.Identities = identity;
            }

            return all.AsQueryable();
        }

        public override async Task<SingleResult<TestUser>> Get(string id)
        {
            return SingleResult.Create((await GetAll()).Where(p => p.Id == id));
        }

        public override async Task<HttpResponseMessage> Patch(string id, Delta<TestUser> patch)
        {
            ClaimsPrincipal user = this.User as ClaimsPrincipal;

            Claim userIdClaim = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            string userId = (userIdClaim != null) ? userIdClaim.Value : string.Empty;

            var all = (await base.GetAll()).Where(p => p.UserId == userId).ToArray();
            if (all.Length == 0)
            {
                return this.Request.CreateResponse(HttpStatusCode.NotFound);
            }
            else if (all[0].UserId != userId)
            {
                return this.Request.CreateResponse(HttpStatusCode.BadRequest, new JObject(new JProperty("error", "Mismatching user id")));
            }
            else
            {
                return await base.Patch(id, patch);
            }
        }

        public override Task<HttpResponseMessage> Post(TestUser item)
        {
            ClaimsPrincipal user = this.User as ClaimsPrincipal;

            Claim userIdClaim = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            string userId = (userIdClaim != null) ? userIdClaim.Value : string.Empty;
            item.UserId = userId;
            return base.Post(item);
        }

        public override async Task<HttpResponseMessage> Delete(string id)
        {
            ClaimsPrincipal user = this.User as ClaimsPrincipal;

            Claim userIdClaim = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            string userId = (userIdClaim != null) ? userIdClaim.Value : string.Empty;

            var all = (await base.GetAll()).Where(p => p.UserId == userId).ToArray();
            if (all.Length == 0)
            {
                return this.Request.CreateResponse(HttpStatusCode.NotFound);
            }
            else if (all[0].UserId != userId)
            {
                return this.Request.CreateResponse(HttpStatusCode.BadRequest, new JObject(new JProperty("error", "Mismatching user id")));
            }
            else
            {
                return await base.Delete(id);
            }
        }
    }
}