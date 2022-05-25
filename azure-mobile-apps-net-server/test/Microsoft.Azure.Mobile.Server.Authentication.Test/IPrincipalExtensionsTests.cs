// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Azure.Mobile.Server.Authentication;
using Microsoft.Azure.Mobile.Server.Authentication.AppService;
using Microsoft.Azure.Mobile.Server.Authentication.Test;
using Microsoft.Azure.Mobile.Server.Login;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace Microsoft.Azure.Mobile.Server.Security
{
    public class IPrincipalExtensionsTests
    {
        private const string ObjectIdentifierClaimType = @"http://schemas.microsoft.com/identity/claims/objectidentifier";
        private const string TenantIdClaimType = @"http://schemas.microsoft.com/identity/claims/tenantid";
        private const string AuthHeaderName = "x-zumo-auth";
        private const string TestLocalhostUrl = "http://localhost/";
        private const string TestSigningKey = "6523e58bc0eec42c31b9635d5e0dfc23b6d119b73e633bf3a5284c79bb4a1ede"; // SHA256 hash of 'secret_key'
        private FacebookCredentials facebookCredentials;
        private Mock<AppServiceTokenHandler> tokenHandlerMock;
        private IAppServiceTokenHandler tokenHandler;

        public IPrincipalExtensionsTests()
        {
            this.facebookCredentials = new FacebookCredentials() { UserId = "Facebook:FBUserId", AccessToken = "ABCDEF" };

            HttpConfiguration config = new HttpConfiguration();
            this.tokenHandlerMock = new Mock<AppServiceTokenHandler>(config) { CallBase = true };
            this.tokenHandler = this.tokenHandlerMock.Object;
        }

        [Fact]
        public void UserPropertiesAreValid()
        {
            // Act
            ClaimsPrincipal user = this.CreateTestUser();

            // Assert
            ClaimsIdentity identity = user.Identity as ClaimsIdentity;
            this.tokenHandlerMock.Verify();
            Assert.Equal(this.facebookCredentials.UserId, identity.FindFirst(ClaimTypes.NameIdentifier).Value);
            Assert.True(user.Identity.IsAuthenticated);
        }

        [Fact]
        public void PopulateProviderCredentials_Facebook_CreatesExpectedCredentials()
        {
            const string UserIdClaimValue = "FacebookId";

            FacebookCredentials credentials = new FacebookCredentials();

            TokenEntry tokenEntry = new TokenEntry("facebook");
            tokenEntry.AccessToken = "TestAccessToken";
            List<ClaimSlim> claims = new List<ClaimSlim>
            {
                new ClaimSlim("Claim1", "Value1"),
                new ClaimSlim("Claim2", "Value2"),
                new ClaimSlim("Claim3", "Value3"),
            };
            tokenEntry.UserClaims = claims;
            tokenEntry.UserId = UserIdClaimValue;

            IPrincipalExtensions.PopulateProviderCredentials(tokenEntry, credentials);

            Assert.Equal("TestAccessToken", credentials.AccessToken);
            Assert.Equal(UserIdClaimValue, credentials.UserId);
            Assert.Equal(claims.Count, credentials.UserClaims.Count());
        }

        [Fact]
        public void PopulateProviderCredentials_Google_CreatesExpectedCredentials()
        {
            const string UserIdClaimValue = "GoogleId";

            GoogleCredentials credentials = new GoogleCredentials();

            TokenEntry tokenEntry = new TokenEntry("google");
            tokenEntry.AccessToken = "TestAccessToken";
            tokenEntry.RefreshToken = "TestRefreshToken";
            tokenEntry.ExpiresOn = DateTime.Parse("2015-03-12T16:49:28.504Z");
            List<ClaimSlim> claims = new List<ClaimSlim>
            {
                new ClaimSlim("Claim1", "Value1"),
                new ClaimSlim("Claim2", "Value2"),
                new ClaimSlim("Claim3", "Value3"),
            };
            tokenEntry.UserClaims = claims;
            tokenEntry.UserId = UserIdClaimValue;

            IPrincipalExtensions.PopulateProviderCredentials(tokenEntry, credentials);

            Assert.Equal("TestAccessToken", credentials.AccessToken);
            Assert.Equal("TestRefreshToken", credentials.RefreshToken);
            Assert.Equal(DateTimeOffset.Parse("2015-03-12T16:49:28.504Z"), credentials.AccessTokenExpiration);
            Assert.Equal(UserIdClaimValue, credentials.UserId);
            Assert.Equal(claims.Count, credentials.UserClaims.Count());
        }

        [Fact]
        public void PopulateProviderCredentials_MicrosoftAccount_CreatesExpectedCredentials()
        {
            const string UserIdClaimValue = "MicrosoftId";

            MicrosoftAccountCredentials credentials = new MicrosoftAccountCredentials();

            TokenEntry tokenEntry = new TokenEntry("microsoft");
            tokenEntry.AccessToken = "TestAccessToken";
            tokenEntry.RefreshToken = "TestRefreshToken";
            tokenEntry.ExpiresOn = DateTime.Parse("2015-03-12T16:49:28.504Z");
            List<ClaimSlim> claims = new List<ClaimSlim>
            {
                new ClaimSlim("Claim1", "Value1"),
                new ClaimSlim("Claim2", "Value2"),
                new ClaimSlim("Claim3", "Value3"),
            };
            tokenEntry.UserClaims = claims;
            tokenEntry.UserId = UserIdClaimValue;

            IPrincipalExtensions.PopulateProviderCredentials(tokenEntry, credentials);

            Assert.Equal("TestAccessToken", credentials.AccessToken);
            Assert.Equal("TestRefreshToken", credentials.RefreshToken);
            Assert.Equal(DateTimeOffset.Parse("2015-03-12T16:49:28.504Z"), credentials.AccessTokenExpiration);
            Assert.Equal(UserIdClaimValue, credentials.UserId);
            Assert.Equal(claims.Count, credentials.UserClaims.Count());
        }

        [Fact]
        public void PopulateProviderCredentials_AzureActiveDirectory_CreatesExpectedCredentials()
        {
            const string UserIdClaimValue = "AadId";

            AzureActiveDirectoryCredentials credentials = new AzureActiveDirectoryCredentials();

            TokenEntry tokenEntry = new TokenEntry("aad");
            tokenEntry.AccessToken = "TestAccessToken";
            tokenEntry.IdToken = "TestIdToken";
            tokenEntry.ExpiresOn = DateTime.Parse("2015-03-12T16:49:28.504Z");
            List<ClaimSlim> claims = new List<ClaimSlim>
            {
                new ClaimSlim("Claim1", "Value1"),
                new ClaimSlim("Claim2", "Value2"),
                new ClaimSlim("Claim3", "Value3"),
                new ClaimSlim(TenantIdClaimType, "TestTenantId"),
                new ClaimSlim(ObjectIdentifierClaimType, "TestObjectId"),
            };
            tokenEntry.UserClaims = claims;
            tokenEntry.UserId = UserIdClaimValue;

            IPrincipalExtensions.PopulateProviderCredentials(tokenEntry, credentials);

            // For AAD, the id_token value needs to get copied to AccessToken, not the access_token value.
            Assert.Equal("TestIdToken", credentials.AccessToken);
            Assert.Equal("TestTenantId", credentials.UserClaims.Single(c => string.Equals(c.Type, TenantIdClaimType, StringComparison.Ordinal)).Value);
            Assert.Equal("TestObjectId", credentials.UserClaims.Single(c => string.Equals(c.Type, ObjectIdentifierClaimType, StringComparison.Ordinal)).Value);
            Assert.Equal(UserIdClaimValue, credentials.UserId);
            Assert.Equal(claims.Count, credentials.UserClaims.Count());
        }

        [Fact]
        public void PopulateProviderCredentials_Twitter_CreatesExpectedCredentials()
        {
            TwitterCredentials credentials = new TwitterCredentials();

            TokenEntry tokenEntry = new TokenEntry("twitter");
            tokenEntry.AccessToken = "TestAccessToken";
            tokenEntry.AccessTokenSecret = "TestAccessTokenSecret";
            List<ClaimSlim> claims = new List<ClaimSlim>
            {
                new ClaimSlim("Claim1", "Value1"),
                new ClaimSlim("Claim2", "Value2"),
                new ClaimSlim("Claim3", "Value3"),
            };
            tokenEntry.UserClaims = claims;

            IPrincipalExtensions.PopulateProviderCredentials(tokenEntry, credentials);

            Assert.Equal("TestAccessToken", credentials.AccessToken);
            Assert.Equal("TestAccessTokenSecret", credentials.AccessTokenSecret);
            Assert.Equal(claims.Count, credentials.UserClaims.Count());
        }

        [Fact]
        public void IsTokenValid_ReturnsFalse_WhenTokenIsInvalid()
        {
            // Arrange
            // This is what is returned when a token is not found.
            TokenEntry tokenEntry = null;

            // Act
            bool result = IPrincipalExtensions.IsTokenValid(tokenEntry);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsTokenValid_ReturnsTrue_WhenTokenIsValid()
        {
            // Arrange
            TokenEntry tokenEntry = new TokenEntry("facebook");
            tokenEntry.UserId = "userId";
            tokenEntry.AuthenticationToken = "zumoAuthToken";
            tokenEntry.AccessToken = "accessToken";

            // Act
            bool result = IPrincipalExtensions.IsTokenValid(tokenEntry);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task GetIdentityAsync_Succeeds()
        {
            // Arrange
            TokenEntry tokenEntry = new TokenEntry("facebook");
            tokenEntry.UserId = "userId";
            tokenEntry.AccessToken = "accessToken";
            tokenEntry.UserClaims = new List<ClaimSlim>()
            {
                new ClaimSlim(ClaimTypes.NameIdentifier, "11111111"),
                new ClaimSlim("groups", "group 1"),
                new ClaimSlim("groups", "group 2")
            };

            HttpResponseMessage response = CreateOkResponseWithContent(tokenEntry);
            MockHttpMessageHandler handler = new MockHttpMessageHandler(response);

            ClaimsPrincipal user = new ClaimsPrincipal(CreateMockClaimsIdentity(new[] { new Claim(JwtRegisteredClaimNames.Iss, "http://contoso.com") }, true));
            HttpRequestMessage request = new HttpRequestMessage();
            request.Headers.Add(AuthHeaderName, "token");

            //Act
            FacebookCredentials creds = await user.GetAppServiceIdentityAsync<FacebookCredentials>(request, new HttpClient(handler));

            // Assert
            Assert.Equal(tokenEntry.UserId, creds.UserId);
            Assert.Equal(tokenEntry.AccessToken, creds.AccessToken);
            Assert.Equal("Facebook", creds.Provider);

            // Verify UserClaims
            Assert.Equal(3, creds.UserClaims.Count());
            Assert.Equal("11111111", creds.UserClaims.Single(c => c.Type == ClaimTypes.NameIdentifier).Value);
            Assert.Equal(2, creds.UserClaims.Count(c => c.Type == "groups"));
            Assert.Equal(new[] { "group 1", "group 2" }, creds.UserClaims.Where(c => c.Type == "groups").Select(c => c.Value));

            Assert.Equal("http://contoso.com/.auth/me?provider=facebook", handler.ActualRequest.RequestUri.ToString());
            Assert.Equal("token", handler.ActualRequest.GetHeaderOrDefault(AuthHeaderName));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task GetIdentityAsync_ReturnsNull_IfMobileAppAuthenticationTokenIsNullOrEmpty(string token)
        {
            // Arrange
            ClaimsPrincipal user = new ClaimsPrincipal(CreateMockClaimsIdentity(Enumerable.Empty<Claim>(), true));
            HttpRequestMessage request = new HttpRequestMessage();
            request.RequestUri = new Uri(TestLocalhostUrl);
            request.Headers.Add(AuthHeaderName, token);

            // Act
            var tokenResult = await user.GetAppServiceIdentityAsync<FacebookCredentials>(request);

            // Assert
            Assert.Null(tokenResult);
        }

        [Fact]
        public async Task GetIdentityAsync_ReturnsNull_IfUserNotAuthenticated()
        {
            // Arrange
            ClaimsPrincipal user = new ClaimsPrincipal(CreateMockClaimsIdentity(Enumerable.Empty<Claim>(), false));
            HttpRequestMessage request = new HttpRequestMessage();

            // Act
            var tokenResult = await user.GetAppServiceIdentityAsync<FacebookCredentials>(request);

            // Assert
            Assert.Null(tokenResult);
        }

        [Fact]
        public async Task GetIdentityAsync_Throws_IfNotClaimsPrincipal()
        {
            // Arrange
            TestPrincipal user = new TestPrincipal();
            HttpRequestMessage request = new HttpRequestMessage();

            // Act
            ArgumentOutOfRangeException ex = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => await user.GetAppServiceIdentityAsync<FacebookCredentials>(request));

            // Assert
            Assert.Equal("The 'principal' parameter must be of type 'ClaimsPrincipal'.", ex.Message);
        }

        [Fact]
        public async Task GetIdentityAsync_Throws_IfNoIssuer()
        {
            // Arrange
            ClaimsPrincipal user = new ClaimsPrincipal(CreateMockClaimsIdentity(new Claim[] { new Claim("notiss", "nope") }, true));
            HttpRequestMessage request = new HttpRequestMessage();
            request.Headers.Add(AuthHeaderName, "some jwt");

            // Act
            ArgumentOutOfRangeException ex = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => await user.GetAppServiceIdentityAsync<FacebookCredentials>(request));

            // Assert
            Assert.Equal("The IPrincipal's Claims must contain an 'iss' Claim.", ex.Message);
        }

        /// <summary>
        /// Create a test user
        /// </summary>
        private ClaimsPrincipal CreateTestUser()
        {
            AppServiceAuthenticationOptions options = this.CreateTestOptions();

            Claim[] claims = new Claim[]
            {
                new Claim("sub", this.facebookCredentials.UserId)
            };

            JwtSecurityToken token = AppServiceLoginHandler.CreateToken(claims, TestSigningKey, TestLocalhostUrl, TestLocalhostUrl, TimeSpan.FromDays(10));

            ClaimsPrincipal user = null;
            string[] validIssAud = new[] { TestLocalhostUrl };
            this.tokenHandler.TryValidateLoginToken(token.RawData, options.SigningKey, validIssAud, validIssAud, out user);

            return user;
        }

        private static ClaimsIdentity CreateMockClaimsIdentity(IEnumerable<Claim> claims, bool isAuthenticated)
        {
            Mock<ClaimsIdentity> claimsIdentityMock = new Mock<ClaimsIdentity>(claims);
            claimsIdentityMock.CallBase = true;
            claimsIdentityMock.SetupGet(c => c.IsAuthenticated).Returns(isAuthenticated);
            return claimsIdentityMock.Object;
        }

        private AppServiceAuthenticationOptions CreateTestOptions()
        {
            AppServiceAuthenticationOptions options = new AppServiceAuthenticationOptions
            {
                SigningKey = TestSigningKey,
            };
            return options;
        }

        private static HttpResponseMessage CreateOkResponseWithContent(object content)
        {
            HttpResponseMessage response = new HttpResponseMessage();
            response.Content = new StringContent(JsonConvert.SerializeObject(content));
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            return response;
        }

        private class TestPrincipal : IPrincipal
        {
            public IIdentity Identity { get; set; }

            public bool IsInRole(string role)
            {
                throw new NotImplementedException();
            }
        }
    }
}