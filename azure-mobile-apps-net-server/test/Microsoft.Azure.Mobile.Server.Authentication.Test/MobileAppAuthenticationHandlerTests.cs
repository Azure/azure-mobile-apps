// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens;
using System.Security.Claims;
using System.Web.Http;
using Microsoft.Azure.Mobile.Server.Authentication;
using Microsoft.Azure.Mobile.Server.Login;
using Microsoft.Owin;
using Microsoft.Owin.Logging;
using Microsoft.Owin.Security;
using Moq;
using TestUtilities;
using Xunit;

namespace Microsoft.Azure.Mobile.Server.Security
{
    public class MobileAppAuthenticationHandlerTests
    {
        private Mock<ILogger> loggerMock;

        private const string TestWebsiteUrl = @"https://faketestapp.faketestazurewebsites.net/";
        private static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        private const string SigningKeyAlpha = "eedf7267f04f50f448729f60df30abf3148950f63ab499ee60f6b5b74b676004"; // SHA256 has of 'alpha_key'
        private const string SigningKeyBeta = "467f24c69e9f3d2e0709f13363e074b244b2d58e204f57b57a59b4f7fdf32a93"; // SHA256 hash of 'beta_key'

        public MobileAppAuthenticationHandlerTests()
        {
            this.loggerMock = new Mock<ILogger>();
        }

        [Flags]
        private enum AuthOptions
        {
            None = 0x00,
            UserAuthKey = 0x01
        }

        public static TheoryDataCollection<string, string, bool> KeysMatchingData
        {
            get
            {
                return new TheoryDataCollection<string, string, bool>
                {
                    { null, null, false },
                    { string.Empty, string.Empty, false },
                    { "你好世界", null, false },
                    { "你好世界", string.Empty, false },
                    { null, "你好世界", false },
                    { string.Empty, "你好世界", false },
                    { "hello", "Hello", false },
                    { "HELLO", "Hello", false },
                    { "hello", "hello", true },
                    { "你好世界", "你好世界", true },
                };
            }
        }

        public static TheoryDataCollection<string, string> AuthorizationData
        {
            get
            {
                return new TheoryDataCollection<string, string>
                {
                    { null, null },
                    { string.Empty, null },
                    { "Unknown OlBhc3N3b3Jk", null },
                    { "Basic Unknown", null },
                    { "Basic VXNlck5hbWU6", string.Empty },
                    { "Basic OlBhc3N3b3Jk", "Password" },
                    { "Basic VXNlck5hbWU6UGFzc3dvcmQ=", "Password" },
                    { "Basic OuS9oOWlveS4lueVjA==", "你好世界" },
                    { "Basic 5L2g5aW9OuS4lueVjA==", "世界" },
                };
            }
        }

        [Theory]
        [InlineData(SigningKeyAlpha, true)]
        [InlineData(SigningKeyBeta, false)]
        [InlineData(null, false)]
        [InlineData("", false)]
        public void Authenticate_CorrectlyAuthenticates(string otherSigningKey, bool expectAuthenticated)
        {
            // Arrange
            HttpConfiguration config = new HttpConfiguration();
            AppServiceAuthenticationOptions optionsDefault = CreateTestOptions(config);
            optionsDefault.SigningKey = SigningKeyAlpha;

            AppServiceAuthenticationOptions optionsOtherSigningKey = CreateTestOptions(config);
            optionsOtherSigningKey.SigningKey = otherSigningKey;

            var mock = new MobileAppAuthenticationHandlerMock(this.loggerMock.Object);
            var request = CreateAuthRequest(new Uri(TestWebsiteUrl), GetTestToken());

            // Act
            AuthenticationTicket authTicket = mock.Authenticate(request, optionsOtherSigningKey);

            // Assert
            if (expectAuthenticated)
            {
                // ensure the AuthenticationTicket is set correctly
                Assert.NotNull(authTicket);
                Assert.NotNull(authTicket.Identity);
                Assert.True(authTicket.Identity.IsAuthenticated);
            }
            else
            {
                Assert.NotNull(authTicket);
                Assert.NotNull(authTicket.Identity);
                Assert.False(authTicket.Identity.IsAuthenticated);
            }
        }

        [Fact]
        public void Authenticate_FailsToAuthenticate_ValidIdentity_WithoutSigningKey()
        {
            // Arrange
            AppServiceAuthenticationOptions options = CreateTestOptions(new HttpConfiguration());

            var mock = new MobileAppAuthenticationHandlerMock(this.loggerMock.Object);
            var request = CreateAuthRequest(new Uri(TestWebsiteUrl), GetTestToken());

            options.SigningKey = null;

            // Act
            AuthenticationTicket authticket = mock.Authenticate(request, options);

            // Assert
            Assert.NotNull(authticket);
            Assert.NotNull(authticket.Identity);
            Assert.False(authticket.Identity.IsAuthenticated, "Expected Authenticate to fail without signing key specified in MobileAppAuthenticationOptions");
        }

        [Fact]
        public void Authenticate_Fails_WithInvalidAudience()
        {
            // Arrange
            AppServiceAuthenticationOptions options = CreateTestOptions(new HttpConfiguration());
            var mock = new MobileAppAuthenticationHandlerMock(this.loggerMock.Object);
            var request = CreateAuthRequest(new Uri(TestWebsiteUrl), GetTestToken(audience: "https://invalidAudience/"));

            // Act
            AuthenticationTicket authticket = mock.Authenticate(request, options);

            // Assert
            Assert.NotNull(authticket);
            Assert.NotNull(authticket.Identity);
            Assert.False(authticket.Identity.IsAuthenticated, "Expected Authenticate to fail with invalid audience");
        }

        [Fact]
        public void Authenticate_Fails_WithInvalidIssuer()
        {
            // Arrange
            AppServiceAuthenticationOptions options = CreateTestOptions(new HttpConfiguration());
            var mock = new MobileAppAuthenticationHandlerMock(this.loggerMock.Object);
            var request = CreateAuthRequest(new Uri(TestWebsiteUrl), GetTestToken(issuer: "https://invalidIssuer/"));

            // Act
            AuthenticationTicket authticket = mock.Authenticate(request, options);

            // Assert
            Assert.NotNull(authticket);
            Assert.NotNull(authticket.Identity);
            Assert.False(authticket.Identity.IsAuthenticated, "Expected Authenticate to fail with invalid issuer");
        }

        private static ClaimsIdentity CreateTestIdentity(string audience = null, string issuer = null, bool validNotBefore = true, bool validExpiration = true)
        {
            ClaimsIdentity myIdentity = new ClaimsIdentity();
            myIdentity.AddClaim(new Claim("sub", "my:userid"));

            if (!string.IsNullOrEmpty(issuer))
            {
                myIdentity.AddClaim(new Claim("iss", issuer));
            }

            if (!string.IsNullOrEmpty(audience))
            {
                myIdentity.AddClaim(new Claim("aud", audience));
            }

            DateTime now = DateTime.UtcNow;
            if (validNotBefore)
            {
                DateTime nbf = now.Subtract(TimeSpan.FromHours(1));
                string nbfAsString = Convert.ToInt64(nbf.Subtract(Epoch).TotalSeconds).ToString();
                myIdentity.AddClaim(new Claim("nbf", nbfAsString));
            }

            if (validExpiration)
            {
                DateTime exp = now.Add(TimeSpan.FromHours(1));
                string expAsString = Convert.ToInt64(exp.Subtract(Epoch).TotalSeconds).ToString();
                myIdentity.AddClaim(new Claim("exp", expAsString));
            }

            return myIdentity;
        }

        /// <summary>
        /// Makes a test token out of the specified claims, or a set of default claims if claims is unspecified.
        /// </summary>
        /// <param name="claims">The claims identity to make a token. Issuer and Audience in the claims will not be changed.</param>
        /// <param name="options">The <see cref="AppServiceAuthenticationOptions"/> object that wraps the signing key.</param>
        /// <param name="audience">The accepted valid audience used if claims is unspecified.</param>
        /// <param name="issuer">The accepted valid issuer used if claims is unspecified.</param>
        /// <returns></returns>
        private static JwtSecurityToken GetTestToken(string signingKey = SigningKeyAlpha, string audience = TestWebsiteUrl, string issuer = TestWebsiteUrl, List<Claim> claims = null)
        {
            if (claims == null || claims.Count == 0)
            {
                claims = new List<Claim>();
                claims.Add(new Claim("sub", "Facebook:1234"));
                claims.Add(new Claim(ClaimTypes.GivenName, "Frank"));
                claims.Add(new Claim(ClaimTypes.Surname, "Miller"));
                claims.Add(new Claim(ClaimTypes.Role, "Admin"));
                claims.Add(new Claim("my_custom_claim", "MyClaimValue"));
            }

            return AppServiceLoginHandler.CreateToken(claims, signingKey, audience, issuer, TimeSpan.FromDays(10));
        }

        private static IOwinRequest CreateAuthRequest(Uri webappUri, JwtSecurityToken token = null)
        {
            OwinContext context = new OwinContext();
            IOwinRequest request = context.Request;
            request.Host = HostString.FromUriComponent(webappUri.Host);
            request.Path = PathString.FromUriComponent(webappUri);
            request.Protocol = "HTTP/1.1";
            request.Method = "GET";
            request.Scheme = "https";
            request.PathBase = PathString.Empty;
            request.QueryString = QueryString.FromUriComponent(webappUri);
            request.Body = new System.IO.MemoryStream();

            if (token != null)
            {
                request.Headers.Append(AppServiceAuthenticationHandler.AuthenticationHeaderName, token.RawData);
            }

            return request;
        }

        private static AppServiceAuthenticationOptions CreateTestOptions(HttpConfiguration config)
        {
            AppServiceAuthenticationOptions options = new AppServiceAuthenticationOptions
            {
                ValidAudiences = new[] { TestWebsiteUrl },
                ValidIssuers = new[] { TestWebsiteUrl },
                SigningKey = SigningKeyAlpha,
                TokenHandler = config.GetAppServiceTokenHandler()
            };

            return options;
        }

        internal class MobileAppAuthenticationHandlerMock : AppServiceAuthenticationHandler
        {
            public MobileAppAuthenticationHandlerMock(ILogger logger)
                : base(logger)
            {
            }

            public new AuthenticationTicket Authenticate(IOwinRequest request, AppServiceAuthenticationOptions options)
            {
                return base.Authenticate(request, options);
            }
        }
    }
}