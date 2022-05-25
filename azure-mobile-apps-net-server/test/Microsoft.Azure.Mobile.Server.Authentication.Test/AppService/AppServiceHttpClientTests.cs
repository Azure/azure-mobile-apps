// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Azure.Mobile.Server.Authentication.AppService;
using Microsoft.Azure.Mobile.Server.Config;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace Microsoft.Azure.Mobile.Server.Authentication.Test.AppService
{
    public class AppServiceHttpClientTests
    {
        [Fact]
        public async Task GetRawTokenAsync_SendsCorrectRequest()
        {
            HttpConfiguration config = new HttpConfiguration();
            config.SetMobileAppSettingsProvider(new MobileAppSettingsProvider());

            string accessToken = "facebookAccessToken";
            string authToken = "zumoAuthToken";
            string facebookId = "facebookUserId";
            string providerName = "Facebook";
            TokenEntry tokenEntry = new TokenEntry(providerName);
            tokenEntry.AccessToken = accessToken;
            tokenEntry.AuthenticationToken = authToken;
            tokenEntry.UserId = facebookId;

            MockHttpMessageHandler handlerMock = new MockHttpMessageHandler(CreateResponse(tokenEntry));

            var webappUri = "http://test";
            AppServiceHttpClient appServiceClientMock = new AppServiceHttpClient(new HttpClient(handlerMock));

            // Act
            TokenEntry result = await appServiceClientMock.GetRawTokenAsync(new Uri(webappUri), accessToken, "Facebook");

            // Assert
            Assert.Equal(accessToken, result.AccessToken);
            Assert.Equal(authToken, result.AuthenticationToken);
            Assert.Equal(facebookId, result.UserId);
            Assert.Equal(webappUri + "/.auth/me?provider=facebook", handlerMock.ActualRequest.RequestUri.ToString());
            Assert.Equal(accessToken, handlerMock.ActualRequest.Headers.GetValues("x-zumo-auth").Single());
            Assert.Equal("MobileAppNetServerSdk", handlerMock.ActualRequest.Headers.GetValues("User-Agent").Single());
        }

        [Fact]
        public async Task GetRawTokenAsync_ReturnsNull_IfNoToken()
        {
            string accessToken = "facebookAccessToken";
            MockHttpMessageHandler handlerMock = new MockHttpMessageHandler(CreateEmptyResponse());

            var webappUri = "http://test";
            AppServiceHttpClient appServiceClientMock = new AppServiceHttpClient(new HttpClient(handlerMock));

            // Act
            TokenEntry result = await appServiceClientMock.GetRawTokenAsync(new Uri(webappUri), accessToken, "Facebook");

            // Assert
            Assert.Null(result);
            Assert.Equal(webappUri + "/.auth/me?provider=facebook", handlerMock.ActualRequest.RequestUri.ToString());
            Assert.Equal(accessToken, handlerMock.ActualRequest.Headers.GetValues("x-zumo-auth").Single());
            Assert.Equal("MobileAppNetServerSdk", handlerMock.ActualRequest.Headers.GetValues("User-Agent").Single());
        }

        [Theory]
        [InlineData(HttpStatusCode.InternalServerError)]
        [InlineData(HttpStatusCode.RequestTimeout)]
        [InlineData(HttpStatusCode.Unauthorized)]
        public async Task GetRawTokenAsync_Throws_IfResponseIsNotSuccess(HttpStatusCode status)
        {
            HttpConfiguration config = new HttpConfiguration();
            config.SetMobileAppSettingsProvider(new MobileAppSettingsProvider());

            var response = new HttpResponseMessage(status);
            MockHttpMessageHandler handlerMock = new MockHttpMessageHandler(response);
            var gatewayUri = "http://test";

            AppServiceHttpClient appServiceClientMock = new AppServiceHttpClient(new HttpClient(handlerMock));

            // Act
            var ex = await Assert.ThrowsAsync<HttpResponseException>(() => appServiceClientMock.GetRawTokenAsync(new Uri(gatewayUri), "123456", "Facebook"));

            // Assert
            Assert.NotNull(ex);
            Assert.Same(response, ex.Response);
        }

        [Theory]
        [InlineData(null, "Facebook", "authToken")]
        [InlineData("123456", null, "tokenProviderName")]
        [InlineData(null, null, "authToken")]
        public async Task GetRawTokenAsync_Throws_IfParametersAreNull(string authToken, string tokenProviderName, string parameterThatThrows)
        {
            AppServiceHttpClient appServiceClient = new AppServiceHttpClient(new HttpClient());

            // Act
            var ex = await Assert.ThrowsAsync<ArgumentNullException>(() => appServiceClient.GetRawTokenAsync(new Uri("http://testuri"), authToken, tokenProviderName));

            // Assert
            Assert.NotNull(ex);
            Assert.Equal(parameterThatThrows, ex.ParamName);
        }

        private static HttpResponseMessage CreateResponse(TokenEntry tokenEntry)
        {
            HttpResponseMessage response = new HttpResponseMessage();
            response.Content = new StringContent(JsonConvert.SerializeObject(tokenEntry));
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            return response;
        }

        // App Service Authentication returns {} if there is no token.
        private static HttpResponseMessage CreateEmptyResponse()
        {
            HttpResponseMessage response = new HttpResponseMessage();
            response.Content = new StringContent("{}");
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            return response;
        }
    }
}