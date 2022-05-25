// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Security.Claims;
using System.ServiceModel.Security.Tokens;
using Xunit;

namespace Microsoft.Azure.Mobile.Server.Login.Test
{
    public class AppServiceLoginHandlerTests
    {
        private const string Audience = "https://audience";
        private const string Issuer = "https://issuer";
        private const string SigningKey = "6523e58bc0eec42c31b9635d5e0dfc23b6d119b73e633bf3a5284c79bb4a1ede"; // SHA256 hash of 'secret_key'
        private const string UserId = "Facebook:1234";

        [Fact]
        public void CreateToken_CreatesExpectedToken()
        {
            Claim[] claims = new Claim[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, UserId),
                new Claim("custom_claim_1", "CustomClaimValue1"),
                new Claim("custom_claim_2", "CustomClaimValue2")
            };            
            JwtSecurityToken token = AppServiceLoginHandler.CreateToken(claims, SigningKey, Audience, Issuer, TimeSpan.FromDays(10));
            
            Assert.Equal(8, token.Claims.Count());    
                    
            Assert.Equal(10, (token.ValidTo - token.ValidFrom).Days);
            Assert.NotNull(token.Payload.Exp);
            Assert.Equal("CustomClaimValue1", token.Claims.Single(p => p.Type == "custom_claim_1").Value);
            Assert.Equal("CustomClaimValue2", token.Claims.Single(p => p.Type == "custom_claim_2").Value);

            ValidateDefaultClaims(token);
            ValidateToken(token.RawData);
        }

        [Fact]
        public void CreateToken_Throws_IfNoSub()
        {
            Claim[] claims = new Claim[]
            {
                new Claim("uid", UserId)
            };

            ArgumentOutOfRangeException ex = Assert.Throws<ArgumentOutOfRangeException>(() =>
                AppServiceLoginHandler.CreateToken(claims, SigningKey, Audience, Issuer, TimeSpan.FromDays(10)));

            Assert.Equal("claims", ex.ParamName);
        }

        [Fact]
        public void CreateToken_Throws_IfSecretNull()
        {
            Claim[] claims = new Claim[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, UserId)
            };

            Assert.Throws<ArgumentNullException>(() =>
                AppServiceLoginHandler.CreateToken(claims, null, Audience, Issuer, TimeSpan.FromDays(10)));
        }

        [Fact]
        public void CreateToken_Throws_IfClaimsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
                  AppServiceLoginHandler.CreateToken(null, SigningKey, Audience, Issuer, TimeSpan.FromDays(10)));
        }

        [Fact]
        public void CreateToken_Throws_IfNegativeLifetime()
        {
            Claim[] claims = new Claim[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, UserId)
            };

            // Act
            ArgumentOutOfRangeException ex = Assert.Throws<ArgumentOutOfRangeException>(() => AppServiceLoginHandler.CreateToken(claims, SigningKey, Audience, Issuer, TimeSpan.FromDays(-10)));

            // Assert
            Assert.Contains("Argument must be greater than or equal to 00:00:00.", ex.Message);
        }

        [Fact]
        public void CreateToken_CreatesTokenWithNoExpiry_WhenLifetimeIsNull()
        {
            Claim[] claims = new Claim[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, UserId)
            };

            JwtSecurityToken token = AppServiceLoginHandler.CreateToken(claims, SigningKey, Audience, Issuer, null);

            // no exp claim
            Assert.Null(token.Payload.Exp);
            Assert.Equal(5, token.Claims.Count());

            Assert.Equal(default(DateTime), token.ValidTo);
            Assert.Null(token.Payload.Exp);

            ValidateDefaultClaims(token);
            ValidateToken(token.RawData);
        }

        private static void ValidateDefaultClaims(JwtSecurityToken token)
        {
            Assert.Equal(Audience, token.Audiences.Single());
            Assert.Equal(Issuer, token.Issuer);
            Assert.Equal("3", token.Claims.Single(p => p.Type == "ver").Value);
            Assert.Equal(UserId, token.Subject);
        }

        /// <summary>
        /// A helper that will throw if the tokenString cannot be parsed or the signature is invalid
        /// </summary>
        /// <param name="tokenString">The JWT token string</param>
        /// <param name="secretKey">The key used to sign the token JWT token</param>
        private static void ValidateToken(string tokenString)
        {
            JwtSecurityToken parsedToken = new JwtSecurityToken(tokenString);

            TokenValidationParameters validationParams = new TokenValidationParameters
            {
                ValidateAudience = true,
                ValidAudience = Audience,
                ValidateIssuer = true,
                ValidIssuer = Issuer,
                ValidateLifetime = parsedToken.Payload.Exp.HasValue,  // support tokens with no expiry
                IssuerSigningToken = new BinarySecretSecurityToken(HmacSigningCredentials.ParseKeyString(SigningKey))
            };

            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken validatedToken = null;
            tokenHandler.ValidateToken(tokenString, validationParams, out validatedToken);
        }
    }
}