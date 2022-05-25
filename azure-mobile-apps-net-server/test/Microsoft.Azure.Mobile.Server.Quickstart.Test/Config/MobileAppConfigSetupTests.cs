// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Azure.Mobile.Server.Authentication;
using Microsoft.Azure.Mobile.Server.Login;
using Microsoft.Azure.Mobile.Server.Notifications;
using Microsoft.Azure.NotificationHubs;
using Microsoft.Azure.NotificationHubs.Messaging;
using Microsoft.Owin.Security;
using Microsoft.Owin.Testing;
using Moq;
using Newtonsoft.Json.Linq;
using Owin;
using TestUtilities;
using Xunit;

namespace Microsoft.Azure.Mobile.Server.Config
{
    public class MobileAppConfigSetupTests
    {
        private const string TestWebsiteUrl = "http://localhost/";
        private const string SigningKey = "bMyklciUDJxqSxtSiJlSusbiZrMnuG99";

        public static TheoryDataCollection<AuthenticationMode, bool, bool> AuthStatusData
        {
            get
            {
                return new TheoryDataCollection<AuthenticationMode, bool, bool>
                {
                    { AuthenticationMode.Active, true, true },
                    { AuthenticationMode.Active, true, false },
                    { AuthenticationMode.Passive, true, true },
                    { AuthenticationMode.Passive, false, false },
                };
            }
        }

        [Theory]
        [MemberData("AuthStatusData")]
        public async Task MobileAppAuth_Succeeds_AsPassiveAndActive(AuthenticationMode mode, bool isMiddlewareRegistered, bool isAuthenticated)
        {
            NotificationInstallation notification = new NotificationInstallation();
            notification.InstallationId = Guid.NewGuid().ToString();
            notification.PushChannel = Guid.NewGuid().ToString();
            notification.Platform = "wns";

            using (var testServer = TestServer.Create(app =>
            {
                // Arrange
                HttpConfiguration config = new HttpConfiguration();
                config.MapHttpAttributeRoutes();
#pragma warning disable 618
                new MobileAppConfiguration()
                    .UseDefaultConfiguration()
                    .AddPushNotifications()
                    .ApplyTo(config);
#pragma warning restore 618
                var pushClientMock = new Mock<PushClient>(config);
                pushClientMock.Setup(p => p.CreateOrUpdateInstallationAsync(It.IsAny<Installation>()))
                    .Returns(Task.FromResult(0));
                pushClientMock.Setup(p => p.GetRegistrationsByTagAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                    .Returns(Task.FromResult(this.CreateCollectionQueryResult<RegistrationDescription>()));

                config.SetPushClient(pushClientMock.Object);

                if (isMiddlewareRegistered)
                {
                    if (mode == AuthenticationMode.Passive)
                    {
                        config.SuppressDefaultHostAuthentication();
                        config.Filters.Add(new HostAuthenticationFilter(AppServiceAuthenticationOptions.AuthenticationName));
                    }

                    app.UseAppServiceAuthentication(GetMobileAppAuthOptions(config, mode));
                }

                app.UseWebApi(config);
            }))
            {
                HttpClient client = new HttpClient(new AddMobileAppAuthHeaderHttpHandler(testServer.Handler, isAuthenticated));
                client.BaseAddress = new Uri(TestWebsiteUrl);

                // Act
                var notificationsPut = await client.PutAsJsonAsync("push/installations/" + notification.InstallationId, notification);
                var apiNotificationsPut = await client.PutAsJsonAsync("api/notificationinstallations/" + notification.InstallationId, notification);
                var tableGet = await client.GetAsync("tables/testtable");
                var tableGetApplication = await client.GetAsync("tables/testtable/someId");
                var tableGetApiRoute = await client.GetAsync("api/testtable");
                var apiGetAnonymous = await client.GetAsync("auth/anonymous");
                var apiGetAuthorize = await client.GetAsync("auth/authorize");
                var apiDirectRoute = await client.GetAsync("api/testapi");
                var apiGetTableRoute = await client.GetAsync("table/testapi");

                // Assert
                Assert.Equal(HttpStatusCode.OK, notificationsPut.StatusCode);
                ValidateHeaders(notificationsPut, true);

                Assert.Equal(HttpStatusCode.NotFound, apiNotificationsPut.StatusCode);
                ValidateHeaders(apiNotificationsPut, true);

                // Succeeds: Api action with no AuthorizeLevel attribute
                Assert.Equal(HttpStatusCode.OK, tableGet.StatusCode);
                ValidateHeaders(tableGet, true);

                // Authorize attribute will deny any unauthenticated requests.
                Assert.Equal(isAuthenticated ? HttpStatusCode.OK : HttpStatusCode.Unauthorized, tableGetApplication.StatusCode);
                ValidateHeaders(tableGetApplication, true);

                // Succeeds: TableControllers will show up in the api route as well.
                Assert.Equal(HttpStatusCode.NotFound, tableGetApiRoute.StatusCode);
                ValidateHeaders(tableGetApiRoute, true);

                // Succeeds: Auth is not set up so no IPrincipal is created. But
                // the AllowAnonymousAttribute lets these through.
                Assert.Equal(HttpStatusCode.OK, apiGetAnonymous.StatusCode);
                ValidateHeaders(apiGetAnonymous, false);

                Assert.Equal(HttpStatusCode.OK, apiDirectRoute.StatusCode);
                ValidateHeaders(apiDirectRoute, true);

                Assert.Equal(HttpStatusCode.NotFound, apiGetTableRoute.StatusCode);
                ValidateHeaders(apiGetTableRoute, true);

                Assert.Equal(isAuthenticated ? HttpStatusCode.OK : HttpStatusCode.Unauthorized, apiGetAuthorize.StatusCode);
                ValidateHeaders(apiGetAuthorize, false);

                if (isAuthenticated)
                {
                    string requestAuthToken = apiGetAuthorize.RequestMessage.Headers.Single(h => h.Key == "x-zumo-auth").Value.Single();
                    JToken responseAuthToken = await apiGetAuthorize.Content.ReadAsAsync<JToken>();
                    Assert.Equal(requestAuthToken, responseAuthToken.ToString());
                }
            }
        }

        private static AppServiceAuthenticationOptions GetMobileAppAuthOptions(HttpConfiguration config, AuthenticationMode mode)
        {
            return new AppServiceAuthenticationOptions
            {
                ValidAudiences = new[] { TestWebsiteUrl },
                ValidIssuers = new[] { TestWebsiteUrl },
                SigningKey = SigningKey,
                TokenHandler = config.GetAppServiceTokenHandler(),
                AuthenticationMode = mode
            };
        }

        private static void ValidateHeaders(HttpResponseMessage response, bool isMobileController)
        {
            IEnumerable<string> versions = response.Headers.Where(h => h.Key.ToUpperInvariant() == "X-ZUMO-SERVER-VERSION").SelectMany(h => h.Value);

            if (isMobileController && response.IsSuccessStatusCode)
            {
                Assert.Equal(versions.Single(), "net-" + AssemblyUtils.AssemblyFileVersion);
            }
            else
            {
                Assert.Empty(versions);
            }

            var length = response.Content.Headers.ContentLength;
            if (length > 0)
            {
                Assert.Equal("application/json", response.Content.Headers.ContentType.MediaType);
            }
        }

        private static JwtSecurityToken GetTestToken()
        {
            Claim[] claims = new Claim[]
            {
                new Claim("sub", "Facebook:1234")
            };
            JwtSecurityToken token = AppServiceLoginHandler.CreateToken(claims, SigningKey, TestWebsiteUrl, TestWebsiteUrl, TimeSpan.FromDays(30));
            return token;
        }

        // CollectionQueryResult is internal so we need to use reflection to make one for mocking purposes.
        private CollectionQueryResult<T> CreateCollectionQueryResult<T>() where T : EntityDescription
        {
            var constructor = typeof(CollectionQueryResult<T>)
                .GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance)
                .Single();
            return constructor.Invoke(new object[] { null, null }) as CollectionQueryResult<T>;
        }

        private class AddMobileAppAuthHeaderHttpHandler : DelegatingHandler
        {
            private bool isAuthenticated;

            public AddMobileAppAuthHeaderHttpHandler(HttpMessageHandler handler, bool isAuthenticated)
                : base(handler)
            {
                this.isAuthenticated = isAuthenticated;
            }

            protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
            {
                if (this.isAuthenticated)
                {
                    request.Headers.Add(AppServiceAuthenticationHandler.AuthenticationHeaderName, GetTestToken().RawData);
                }

                return await base.SendAsync(request, cancellationToken);
            }
        }
    }
}