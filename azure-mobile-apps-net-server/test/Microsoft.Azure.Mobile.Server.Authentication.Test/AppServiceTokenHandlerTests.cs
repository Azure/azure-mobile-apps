// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IdentityModel.Protocols.WSTrust;
using System.IdentityModel.Tokens;
using System.Security.Claims;
using System.Web.Http;
using Microsoft.Azure.Mobile.Server.Authentication;
using Microsoft.Azure.Mobile.Server.Login;
using Moq;
using TestUtilities;
using Xunit;

namespace Microsoft.Azure.Mobile.Server.Security
{
    public class AppServiceTokenHandlerTests
    {
        private readonly string testSecretKey = "69dfd814c383dda4ea7aaeb26e605d419011e7ef3863d5cd2eee7a3607ec8f4d"; // SHA256 hash of 'l9dsa634ksfdlds;lkw43-psdfd'
        private readonly string[] testWebsiteUrls = new string[] { "https://fakesite.fakeazurewebsites.net/" };

        private static readonly TimeSpan Lifetime = TimeSpan.FromDays(10);
        private static readonly Claim[] DefaultClaims = new Claim[] { new Claim("sub", "my:userid") };

        private HttpConfiguration config;
        private Mock<AppServiceTokenHandler> tokenHandlerMock;
        private AppServiceTokenHandler tokenHandler;
        private FacebookCredentials credentials;

        public AppServiceTokenHandlerTests()
        {
            this.config = new HttpConfiguration();
            this.tokenHandlerMock = new Mock<AppServiceTokenHandler>(this.config) { CallBase = true };
            this.tokenHandler = this.tokenHandlerMock.Object;
            this.credentials = new FacebookCredentials
            {
                UserId = "Facebook:1234",
                AccessToken = "abc123"
            };
        }

        public static TheoryDataCollection<string, string> CreateUserIdData
        {
            get
            {
                return new TheoryDataCollection<string, string>
                {
                    { "你好", "世界" },
                    { "Hello", "World" },
                    { "Hello", null },
                    { "Hello", string.Empty },
                    { "Hello", "   " },
                };
            }
        }

        public static TheoryDataCollection<string, bool, string, string> ParseUserIdData
        {
            get
            {
                return new TheoryDataCollection<string, bool, string, string>
                {
                    { null, false, null, null },
                    { string.Empty, false, null, null },
                    { ":", false, null, null },
                    { ":::::", false, null, null },
                    { "invalid", false, null, null },
                    { ":id", false, null, null },
                    { "name:", false, null, null },
                    { "你好:世界", true, "你好", "世界" },
                    { "你好:世:界", true, "你好", "世:界" },
                    { "你好:::::", true, "你好", "::::" },
                };
            }
        }

        public static TheoryDataCollection<string> InvalidUserIdData
        {
            get
            {
                return new TheoryDataCollection<string>
                {
                    null,
                    string.Empty,
                    "   ",
                    "你好世界",
                };
            }
        }

        public static TheoryDataCollection<string> TokenData
        {
            get
            {
                return new TheoryDataCollection<string>
                {
                    // Our token format as of 10/31/2015
                    "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJ1aWQiOiJmYWNlYm9vazoxMjM0IiwiYXVkIjoiaHR0cHM6Ly9mYWtlc2l0ZS5mYWtlYXp1cmV3ZWJzaXRlcy5uZXQvIiwiaXNzIjoiaHR0cHM6Ly9mYWtlc2l0ZS5mYWtlYXp1cmV3ZWJzaXRlcy5uZXQvIiwibmJmIjoxNDQ2Mjg0OTkzfQ.0tActGDSsR-5q8AbtNT3deiGMb525nbUg6wkvTd9ZQ0"
                };
            }
        }

        [Fact]
        public void CreateTokenInfo_AndValidateLoginToken_Works()
        {
            AppServiceAuthenticationOptions options = this.CreateTestOptions();

            Claim[] claims = new Claim[]
            {
                new Claim("sub", this.credentials.UserId),
            };

            // Create a login token for the provider
            JwtSecurityToken token = AppServiceLoginHandler.CreateToken(claims, this.testSecretKey, this.testWebsiteUrls[0], this.testWebsiteUrls[0], Lifetime);

            this.ValidateLoginToken(token.RawData, options);
        }

        private static ClaimsIdentity CreateMockClaimsIdentity(IEnumerable<Claim> claims, bool isAuthenticated)
        {
            Mock<ClaimsIdentity> claimsIdentityMock = new Mock<ClaimsIdentity>(claims);
            claimsIdentityMock.CallBase = true;
            claimsIdentityMock.SetupGet(c => c.IsAuthenticated).Returns(isAuthenticated);
            return claimsIdentityMock.Object;
        }

        /// <summary>
        /// This test verifies that token formats that we've previously issued continue to validate.
        /// To produce these token strings, use the runtime code to create a token, ensuring that you
        /// set the lifetime to 100 years so the tests continue to run. Then add that raw token value
        /// to this test. Be sure to use the above tests to create the token, to ensure the claim values
        /// and test key match. E.g., you can use the below CreateTokenInfo_CreatesExpectedToken test code
        /// to generate the token, substituting a far out expiry.
        /// </summary>
        [Theory]
        [MemberData("TokenData")]
        public void TryValidateLoginToken_AcceptsPreviousTokenVersions(string tokenValue)
        {
            AppServiceAuthenticationOptions options = this.CreateTestOptions();
            this.ValidateLoginToken(tokenValue, options);
        }

        private void ValidateLoginToken(string token, AppServiceAuthenticationOptions options)
        {
            // validate the token and get the claims principal
            ClaimsPrincipal claimsPrincipal = null;
            Assert.True(this.tokenHandler.TryValidateLoginToken(token, options.SigningKey, this.testWebsiteUrls, this.testWebsiteUrls, out claimsPrincipal));
        }

        [Fact]
        public void TryValidateLoginToken_RejectsMalformedTokens()
        {
            AppServiceAuthenticationOptions options = this.CreateTestOptions();
            ClaimsPrincipal claimsPrincipal = null;
            bool result = this.tokenHandler.TryValidateLoginToken("this is not a valid jwt", options.SigningKey, this.testWebsiteUrls, this.testWebsiteUrls, out claimsPrincipal);
            Assert.False(result);
            Assert.Null(claimsPrincipal);
        }

        [Fact]
        public void TryValidateLoginToken_RejectsTokensSignedWithWrongKey()
        {
            JwtSecurityToken token = AppServiceLoginHandler.CreateToken(DefaultClaims, this.testSecretKey, this.testWebsiteUrls[0], this.testWebsiteUrls[0], null);

            ClaimsPrincipal claimsPrincipal = null;

            string anotherKey = "f12aca0581d81554852c5cb1314ee230395b349bdf828bb3e1ce8d556cfd4c5a"; // SHA256 hash of 'another_key'
            bool isValid = this.tokenHandler.TryValidateLoginToken(token.RawData, anotherKey, this.testWebsiteUrls, this.testWebsiteUrls, out claimsPrincipal);
            Assert.False(isValid);
            Assert.Null(claimsPrincipal);
        }

        [Fact]
        public void ToClaimValue_ProducesCorrectValue()
        {
            // Act
            string actual = this.tokenHandler.ToClaimValue(this.credentials);

            // Assert
            Assert.Equal("{\"accessToken\":\"abc123\"}", actual);
            ValidateTestCredentials(this.credentials);
        }

        [Theory]
        [MemberData("CreateUserIdData")]
        public void CreateUserId_FormatsCorrectly(string providerName, string providerId)
        {
            // Arrange
            string expected = string.Format("{0}:{1}", providerName, providerId);

            // Act
            string actual = this.tokenHandler.CreateUserId(providerName, providerId);

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void CreateUserId_ThrowsIfNullProviderName()
        {
            Assert.Throws<ArgumentNullException>(() => this.tokenHandler.CreateUserId(null, "value"));
        }

        [Theory]
        [MemberData("ParseUserIdData")]
        public void TryParseUserId(string userId, bool expected, string providerName, string providerId)
        {
            // Arrange
            string actualProviderName;
            string actualProviderId;

            // Act
            bool actual = this.tokenHandler.TryParseUserId(userId, out actualProviderName, out actualProviderId);

            // Assert
            Assert.Equal(expected, actual);
            Assert.Equal(providerName, actualProviderName);
            Assert.Equal(providerId, actualProviderId);
        }

        [Fact]
        public void ValidateToken_ThrowsSecurityTokenValidationException_WhenValidFromIsAfterCurrentTime()
        {
            // Arrange
            string audience = this.testWebsiteUrls[0];
            string issuer = this.testWebsiteUrls[0];
            TimeSpan lifetimeFiveMinute = new TimeSpan(0, 5, 0);
            DateTime tokenCreationDateInFuture = DateTime.UtcNow + new TimeSpan(1, 0, 0);
            DateTime tokenExpiryDate = tokenCreationDateInFuture + lifetimeFiveMinute;

            SecurityTokenDescriptor tokenDescriptor = this.GetTestSecurityTokenDescriptor(tokenCreationDateInFuture, tokenExpiryDate, audience, issuer);

            JwtSecurityTokenHandler securityTokenHandler = new JwtSecurityTokenHandler();
            JwtSecurityToken token = securityTokenHandler.CreateToken(tokenDescriptor) as JwtSecurityToken;

            // Act
            // Assert
            SecurityTokenNotYetValidException ex = Assert.Throws<SecurityTokenNotYetValidException>(() =>
                AppServiceTokenHandler.ValidateToken(token.RawData, this.testSecretKey, audience, issuer));
            Assert.Contains("IDX10222: Lifetime validation failed. The token is not yet valid", ex.Message, StringComparison.Ordinal);
        }

        [Fact]
        public void ValidateToken_ThrowsSecurityTokenValidationException_WhenTokenExpired()
        {
            // Arrange
            string audience = this.testWebsiteUrls[0];
            string issuer = this.testWebsiteUrls[0];
            TimeSpan lifetime = new TimeSpan(0, 0, 1);
            DateTime tokenCreationDate = DateTime.UtcNow + new TimeSpan(-1, 0, 0);
            DateTime tokenExpiryDate = tokenCreationDate + lifetime;

            SecurityTokenDescriptor tokenDescriptor = this.GetTestSecurityTokenDescriptor(tokenCreationDate, tokenExpiryDate, audience, issuer);

            JwtSecurityTokenHandler securityTokenHandler = new JwtSecurityTokenHandler();
            JwtSecurityToken token = securityTokenHandler.CreateToken(tokenDescriptor) as JwtSecurityToken;

            // Act
            System.Threading.Thread.Sleep(1000);
            SecurityTokenExpiredException ex = Assert.Throws<SecurityTokenExpiredException>(() =>
                AppServiceTokenHandler.ValidateToken(token.RawData, this.testSecretKey, audience, issuer));

            // Assert
            Assert.Contains("IDX10223: Lifetime validation failed. The token is expired", ex.Message, StringComparison.Ordinal);
        }

        [Fact]
        public void ValidateToken_ThrowsSecurityTokenValidationException_WhenIssuerIsBlank()
        {
            // Arrange
            string audience = this.testWebsiteUrls[0];
            string issuer = string.Empty;
            TimeSpan lifetime = new TimeSpan(24, 0, 0);
            DateTime tokenCreationDate = DateTime.UtcNow;
            DateTime tokenExpiryDate = tokenCreationDate + lifetime;

            SecurityTokenDescriptor tokenDescriptor = this.GetTestSecurityTokenDescriptor(tokenCreationDate, tokenExpiryDate, audience, issuer);
            tokenDescriptor.TokenIssuerName = string.Empty;

            JwtSecurityTokenHandler securityTokenHandler = new JwtSecurityTokenHandler();
            JwtSecurityToken token = securityTokenHandler.CreateToken(tokenDescriptor) as JwtSecurityToken;

            // Act
            SecurityTokenInvalidIssuerException ex = Assert.Throws<SecurityTokenInvalidIssuerException>(() =>
               AppServiceTokenHandler.ValidateToken(token.RawData, this.testSecretKey, audience, issuer));

            // Assert
            Assert.Contains("IDX10211: Unable to validate issuer. The 'issuer' parameter is null or whitespace", ex.Message, StringComparison.Ordinal);
        }

        [Fact]
        public void ValidateToken_PassesWithValidToken()
        {
            // Arrange
            string audience = this.testWebsiteUrls[0];
            string issuer = this.testWebsiteUrls[0];
            TimeSpan lifetime = new TimeSpan(24, 0, 0);
            DateTime tokenCreationDate = DateTime.UtcNow;
            DateTime tokenExpiryDate = tokenCreationDate + lifetime;

            SecurityTokenDescriptor tokenDescriptor = this.GetTestSecurityTokenDescriptor(tokenCreationDate, tokenExpiryDate, audience, issuer);

            JwtSecurityTokenHandler securityTokenHandler = new JwtSecurityTokenHandler();
            JwtSecurityToken token = securityTokenHandler.CreateToken(tokenDescriptor) as JwtSecurityToken;

            // Act
            // Assert
            AppServiceTokenHandler.ValidateToken(token.RawData, this.testSecretKey, audience, issuer);
        }

        [Fact]
        public void ValidateToken_ThrowsArgumentException_WithMalformedToken()
        {
            // Arrange
            string audience = this.testWebsiteUrls[0];
            string issuer = this.testWebsiteUrls[0];
            TimeSpan lifetime = new TimeSpan(24, 0, 0);
            DateTime tokenCreationDate = DateTime.UtcNow;
            DateTime tokenExpiryDate = tokenCreationDate + lifetime;

            SecurityTokenDescriptor tokenDescriptor = this.GetTestSecurityTokenDescriptor(tokenCreationDate, tokenExpiryDate, audience, issuer);

            JwtSecurityTokenHandler securityTokenHandler = new JwtSecurityTokenHandler();
            JwtSecurityToken token = securityTokenHandler.CreateToken(tokenDescriptor) as JwtSecurityToken;

            // Act
            ArgumentException ex = Assert.Throws<ArgumentException>(() =>
                AppServiceTokenHandler.ValidateToken(token.RawData + ".malformedbits.!.2.", this.testSecretKey, audience, issuer));

            // Assert
            Assert.Contains("IDX10708: 'System.IdentityModel.Tokens.JwtSecurityTokenHandler' cannot read this string", ex.Message, StringComparison.Ordinal);
        }

        private SecurityTokenDescriptor GetTestSecurityTokenDescriptor(DateTime tokenLifetimeStart, DateTime tokenLifetimeEnd, string audience, string issuer)
        {
            List<Claim> claims = new List<Claim>()
            {
                new Claim("uid", this.credentials.UserId),
                new Claim("ver", "2"),
            };

            SecurityTokenDescriptor tokenDescriptor = new SecurityTokenDescriptor
            {
                AppliesToAddress = audience,
                TokenIssuerName = issuer,
                SigningCredentials = new HmacSigningCredentials(this.testSecretKey),
                Lifetime = new Lifetime(tokenLifetimeStart, tokenLifetimeEnd),
                Subject = new ClaimsIdentity(claims),
            };

            return tokenDescriptor;
        }

        private static void ValidateTestCredentials(FacebookCredentials credentials)
        {
            Assert.Equal("Facebook", credentials.Provider);
            Assert.Equal("Facebook:1234", credentials.UserId);
            Assert.Equal("abc123", credentials.AccessToken);
        }

        private AppServiceAuthenticationOptions CreateTestOptions()
        {
            AppServiceAuthenticationOptions options = new AppServiceAuthenticationOptions
            {
                SigningKey = this.testSecretKey,
            };
            return options;
        }
    }
}