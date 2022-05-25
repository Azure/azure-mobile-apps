// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Azure.Mobile.Server.Authentication;
using Microsoft.Azure.Mobile.Server.Config;
using Microsoft.Azure.Mobile.Server.Login;
using Microsoft.Azure.Mobile.Server.Tables.Config;
using Microsoft.Owin.Testing;
using Newtonsoft.Json.Linq;
using Owin;
using Xunit;

namespace Microsoft.Azure.Mobile.Server
{
    public class SecuredControllerTests
    {
        private const string TestLocalhostName = "http://localhost/";
        private const string SigningKeyAlpha = "eedf7267f04f50f448729f60df30abf3148950f63ab499ee60f6b5b74b676004"; // SHA256 has of 'alpha_key'
        private const string SigningKeyBeta = "467f24c69e9f3d2e0709f13363e074b244b2d58e204f57b57a59b4f7fdf32a93"; // SHA256 hash of 'beta_key'

        [Fact]
        public async Task AnonymousAction_AnonymousRequest_ReturnsOk()
        {
            TestContext context = TestContext.Create();

            HttpResponseMessage response = await context.Client.GetAsync("api/secured/anonymous");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            JObject actual = await response.Content.ReadAsAsync<JObject>();
            Assert.Equal(JTokenType.Null, actual["id"].Type);
        }

        [Fact]
        public async Task AnonymousAction_AuthTokenInRequest_ReturnsOk()
        {
            TestContext context = TestContext.Create();
            string audience = TestLocalhostName;
            string issuer = TestLocalhostName;

            JwtSecurityToken token = context.GetTestToken(context.SigningKey, audience, issuer);

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "api/secured/anonymous");
            request.Headers.Add(AppServiceAuthenticationHandler.AuthenticationHeaderName, token.RawData);
            HttpResponseMessage response = await context.Client.SendAsync(request);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            JObject result = await response.Content.ReadAsAsync<JObject>();

            Assert.Equal("Facebook:1234", result["id"]);
        }

        [Fact]
        public async Task InvalidAuthToken_WrongSigningKey_ReturnsUnauthorized()
        {
            TestContext context = TestContext.Create();
            string audience = TestLocalhostName;
            string issuer = TestLocalhostName;

            JwtSecurityToken token = context.GetTestToken(SigningKeyBeta, audience, issuer);

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "api/secured/authorized");
            request.Headers.Add(AppServiceAuthenticationHandler.AuthenticationHeaderName, token.RawData);
            HttpResponseMessage response = await context.Client.SendAsync(request);
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task InvalidAuthToken_ToAnonymousAction_ReturnsOk()
        {
            TestContext context = TestContext.Create();

            string malformedToken = "no way is this a jwt";

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "api/secured/anonymous");
            request.Headers.Add(AppServiceAuthenticationHandler.AuthenticationHeaderName, malformedToken);
            HttpResponseMessage response = await context.Client.SendAsync(request);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task InvalidAuthToken_ToAuthorizedAction_ReturnsUnauthorized()
        {
            TestContext context = TestContext.Create();

            string malformedToken = "no way is this a jwt";

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "api/secured/authorized");
            request.Headers.Add(AppServiceAuthenticationHandler.AuthenticationHeaderName, malformedToken);
            HttpResponseMessage response = await context.Client.SendAsync(request);
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task AuthTokenNotInRequest_ToAuthorizedAction_ReturnsUnauthorized()
        {
            TestContext context = TestContext.Create();

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "api/secured/authorized");
            HttpResponseMessage response = await context.Client.SendAsync(request);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Theory]
        [InlineData("x-zumo-auth")]
        [InlineData("X-ZUMO-AUTH")]
        [InlineData("X-ZuMo-AuTh")]
        public async Task AuthorizedAction_TokenInRequest_CaseInsensitiveHeader_ReturnsOk(string authHeaderName)
        {
            TestContext context = TestContext.Create();
            string audience = TestLocalhostName;
            string issuer = TestLocalhostName;

            JwtSecurityToken token = context.GetTestToken(context.SigningKey, audience, issuer);

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "api/secured/authorized");
            request.Headers.Add(authHeaderName, token.RawData);
            HttpResponseMessage response = await context.Client.SendAsync(request);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Theory]
        [InlineData("api/secured/anonymous")]
        [InlineData("api/secured/authorized")]
        public async Task TokenInRequest_ReturnsOk(string action)
        {
            TestContext context = TestContext.Create();
            string audience = TestLocalhostName;
            string issuer = TestLocalhostName;

            JwtSecurityToken token = context.GetTestToken(context.SigningKey, audience, issuer);

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, action);
            request.Headers.Add(AppServiceAuthenticationHandler.AuthenticationHeaderName, token.RawData);
            HttpResponseMessage response = await context.Client.SendAsync(request);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task AnonymousRequest_ToSecured_ReturnsUnauthorized()
        {
            TestContext context = TestContext.Create();

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "api/secured/authorized");
            HttpResponseMessage response = await context.Client.SendAsync(request);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        private class TestContext
        {
            public HttpConfiguration Config { get; private set; }

            public HttpClient Client { get; private set; }

            public MobileAppSettingsDictionary Settings { get; private set; }

            public string SigningKey { get; set; }

            public IEnumerable<string> ValidAudiences { get; set; }

            public IEnumerable<string> ValidIssuers { get; set; }

            public static TestContext Create()
            {
                TestContext context = new TestContext();
                context.Config = new HttpConfiguration();
                TestServer server = context.CreateTestServer(context.Config);
                context.Client = server.HttpClient;
                context.Settings = context.Config.GetMobileAppSettingsProvider().GetMobileAppSettings();
                return context;
            }

            private TestServer CreateTestServer(HttpConfiguration config)
            {
                config.MapHttpAttributeRoutes();

                new MobileAppConfiguration()
                    .MapApiControllers()
                    .AddTables(
                        new MobileAppTableConfiguration()
                        .MapTableControllers())
                    .ApplyTo(config);

                // setup test authorization config values
                this.SigningKey = SigningKeyAlpha;
                this.ValidAudiences = new[] { TestLocalhostName };
                this.ValidIssuers = new[] { TestLocalhostName };

                return TestServer.Create((appBuilder) =>
                {
                    appBuilder.UseAppServiceAuthentication(this.GetMobileAppAuthOptions(config));
                    appBuilder.UseWebApi(config);
                });
            }

            private AppServiceAuthenticationOptions GetMobileAppAuthOptions(HttpConfiguration config)
            {
                return new AppServiceAuthenticationOptions
                {
                    SigningKey = this.SigningKey,
                    ValidAudiences = this.ValidAudiences,
                    ValidIssuers = this.ValidIssuers,
                    TokenHandler = config.GetAppServiceTokenHandler()
                };
            }

            public JwtSecurityToken GetTestToken(string secretKey, string audience, string issuer)
            {
                Claim[] claims = new Claim[]
                {
                    new Claim("sub", "Facebook:1234"),
                    new Claim("custom_claim_1", "CustomClaimValue1"),
                    new Claim("custom_claim_2", "CustomClaimValue2"),
                    new Claim("aud", audience),
                    new Claim("iss", issuer),
                };

                JwtSecurityToken token = AppServiceLoginHandler.CreateToken(claims, secretKey, audience, issuer, TimeSpan.FromDays(30));

                Assert.Equal(8, token.Claims.Count());

                return token;
            }
        }
    }
}