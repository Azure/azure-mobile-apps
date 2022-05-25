using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Azure.Mobile.Server;
using Microsoft.Azure.Mobile.Server.Authentication;
using Microsoft.Azure.Mobile.Server.Authentication.AppService;
using Microsoft.Azure.Mobile.Server.Config;

namespace ZumoE2EServerApp.Controllers
{
    [MobileAppController]
    public class IdentityController : ApiController
    {
        // GET api/Identity
        [Route("api/Identity/{provider}")]
        public async Task<string> Get(string provider)
        {
            string userId;
            ProviderCredentials creds;

            switch (provider.ToLowerInvariant())
            {
                case "facebook":
                    creds = await this.User.GetAppServiceIdentityAsync<FacebookCredentials>(this.Request);
                    break;
                case "google":
                    creds = await this.User.GetAppServiceIdentityAsync<GoogleCredentials>(this.Request);
                    break;
                case "twitter":
                    creds = await this.User.GetAppServiceIdentityAsync<TwitterCredentials>(this.Request);
                    break;
                case "microsoftaccount":
                    creds = await this.User.GetAppServiceIdentityAsync<MicrosoftAccountCredentials>(this.Request);
                    break;
                case "aad":
                    creds = await this.User.GetAppServiceIdentityAsync<AzureActiveDirectoryCredentials>(this.Request);
                    break;
                default:
                    creds = null;
                    break;
            }

            if (creds != null)
            {
                userId = creds.UserId;
            }
            else
            {
                userId = "Invalid. Token invalid or regression in GetAppServiceIdentityAsync";
            }

            return string.Format("UserId:{0}", userId);
        }
    }
}
