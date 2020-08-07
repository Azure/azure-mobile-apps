// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Azure.Core;
using Azure.Core.Pipeline;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Zumo.Server.Test.E2EServer;
using Microsoft.Zumo.Server.Test.E2EServer.Database;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Microsoft.Zumo.MobileData.Test.Helpers
{
    public abstract class BaseTest
    {
        private readonly TestServer server = Program.GetTestServer();

        /// <summary>
        /// Returns a MobileDataClient that hits the test server.
        /// </summary>
        /// <returns>A <see cref="MobileTableClient"/> reference.</returns>
        internal MobileTableClient GetTestClient()
        {
            var httpClient = server.CreateClient();

            var clientOptions = new MobileTableClientOptions()
            {
                Transport = new HttpClientTransport(httpClient)
            };

            return new MobileTableClient(new Uri("https://localhost:5001"), clientOptions);
        }

        /// <summary>
        /// Returns a MobilDataClient with authentication
        /// </summary>
        internal MobileTableClient GetTestClient(TokenCredential credential)
        {
            var httpClient = server.CreateClient();
            var clientOptions = new MobileTableClientOptions()
            {
                Transport = new HttpClientTransport(httpClient)
            };
            return new MobileTableClient(new Uri("https://localhost:5001"), credential, clientOptions);
        }

        /// <summary>
        /// Reference to the internal service database for checking backend operations
        /// </summary>
        internal E2EDbContext DbContext
        {
            get
            {
                return server.Services.GetRequiredService<E2EDbContext>();
            }
        }

        /// <summary>
        /// The secret to use for generating an auth token
        /// </summary>
        private readonly string secret = "PDv7DrqznYL6nv7DrqzjnQYO9JxIsWdcjnQYL6nu0f";

        internal string GenerateSecurityToken(string userId, string email)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(secret);
            var credentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Audience = "localhost",
                Issuer = "localhost",
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, userId),
                    new Claim(ClaimTypes.Email, email)
                }),
                Expires = DateTime.UtcNow.AddMinutes(1440),
                SigningCredentials = credentials
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
