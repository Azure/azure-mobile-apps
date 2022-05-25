// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Configuration;
using System.Security.Claims;
using System.Web.Http;
using System.Web.Http.Controllers;
using Microsoft.Azure.Mobile.Server.Authentication;
using Microsoft.Azure.Mobile.Server.Config;
using Microsoft.Azure.Mobile.Server.Login;
using ZumoE2EServerApp.DataObjects;

namespace ZumoE2EServerApp.Controllers
{
    [MobileAppController]
    public class JwtTokenGeneratorController : ApiController
    {
        public IAppServiceTokenHandler handler { get; set; }

        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);
            this.handler = controllerContext.Configuration.GetAppServiceTokenHandler();
        }

        public LoginUser GetDummyUserToken()
        {
            Claim[] claims = new Claim[]
            {
               new Claim("sub", "Facebook:someuserid@hotmail.com")
            };

            string host = this.Request.RequestUri.GetLeftPart(UriPartial.Authority) + "/";

            var token = AppServiceLoginHandler.CreateToken(claims, GetSigningKey(), host, host, TimeSpan.FromDays(30));

            return new LoginUser()
            {
                UserId = token.Subject,
                AuthenticationToken = token.RawData
            };
        }

        private static string GetSigningKey()
        {
            // Check for the App Service Auth environment variable WEBSITE_AUTH_SIGNING_KEY,
            // which holds the signing key on the server. If it's not there, check for a SIGNING_KEY
            // app setting, which can be used for local debugging.

            string key = Environment.GetEnvironmentVariable("WEBSITE_AUTH_SIGNING_KEY");

            if (string.IsNullOrWhiteSpace(key))
            {
                key = ConfigurationManager.AppSettings["SigningKey"];
            }

            return key;
        }
    }
}