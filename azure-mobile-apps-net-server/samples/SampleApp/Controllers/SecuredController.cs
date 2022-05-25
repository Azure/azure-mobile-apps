// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Azure.Mobile.Server.Authentication;
using Microsoft.Azure.Mobile.Server.Config;

namespace Local.Controllers
{
    /// <summary>
    /// The endpoints of this controller are secured
    /// </summary>

    [MobileAppController]
    public class CustomSecuredController : ApiController
    {
        [Authorize]
        public async Task<object> Get()
        {
            FacebookCredentials fbCreds = await this.User.GetAppServiceIdentityAsync<FacebookCredentials>(this.Request);
            TwitterCredentials twitterCreds = await this.User.GetAppServiceIdentityAsync<TwitterCredentials>(this.Request);
            GoogleCredentials googCreds = await this.User.GetAppServiceIdentityAsync<GoogleCredentials>(this.Request);
            MicrosoftAccountCredentials msCreds = await this.User.GetAppServiceIdentityAsync<MicrosoftAccountCredentials>(this.Request);
            AzureActiveDirectoryCredentials aadCreds = await this.User.GetAppServiceIdentityAsync<AzureActiveDirectoryCredentials>(this.Request);

            return new
            {
                FacebookCreds = fbCreds,
                TwitterCreds = twitterCreds,
                GoogleCreds = googCreds,
                MicrosoftAccountCreds = msCreds,
                AadCreds = aadCreds,
                Claims = (this.User as ClaimsPrincipal).Claims.Select(c => new { Type = c.Type, Value = c.Value })
            };
        }
    }
}