using Azure.Mobile.Server.Test.E2EServer;
using Azure.Mobile.Server.Test.E2EServer.Database;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OData.Edm;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Azure.Mobile.Server.Test.Helpers
{
    /// <summary>
    /// A set of basic helper methods for the tests.
    /// </summary>
    public abstract class Base_Test
    {
        protected readonly TestServer server = Program.GetTestServer();

        /// <summary>
        /// The base URI for the request
        /// </summary>
        private Uri BaseUri = new Uri("https://localhost");

        /// <summary>
        /// The secret to use for generating an auth token
        /// </summary>
        private string secret = "PDv7DrqznYL6nv7DrqzjnQYO9JxIsWdcjnQYL6nu0f";

        /// <summary>
        /// The JSON Serializer options to use
        /// </summary>
        private JsonSerializerOptions JsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

        /// <summary>
        /// Send a request to the server, receiving the response
        /// </summary>
        /// <typeparam name="T">The type for the content</typeparam>
        /// <param name="server">The server to send the request to</param>
        /// <param name="method">The HTTP Method</param>
        /// <param name="relativePath">The path for the requet</param>
        /// <param name="content">The typed content</param>
        /// <param name="additionalHeaders">Any additional headers</param>
        /// <returns></returns>
        public Task<HttpResponseMessage> SendRequestToServer<T>(
            HttpMethod method,
            string relativePath,
            T content,
            Dictionary<string, string> additionalHeaders = null) where T : class
        {
            var client = server.CreateClient();

            var request = new HttpRequestMessage
            {
                Method = method,
                RequestUri = new Uri(BaseUri, relativePath)
            };

            if (content != null)
            {
                if (typeof(T) == typeof(String))
                {
                    request.Content = new StringContent(content as string, Encoding.UTF8, "application/json-patch+json");
                }
                else
                {
                    var json = JsonSerializer.Serialize<T>(content, JsonOptions);
                    request.Content = new StringContent(json, Encoding.UTF8, "application/json");
                }
            }

            if (additionalHeaders != null)
            {
                foreach (var kv in additionalHeaders)
                {
                    request.Headers.Add(kv.Key, kv.Value);
                }
            }

            return client.SendAsync(request); 
        }

        public async Task<T> GetValueFromResponse<T>(HttpResponseMessage response) where T : class
        {
            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<T>(json, JsonOptions);
            return result;
        }

        public T GetItemFromDb<T>(string id) where T : class, ITableData
        {
            var context = server.Services.GetRequiredService<E2EDbContext>();
            var item = context.Set<T>().Where(t => t.Id == id).AsNoTracking().FirstOrDefault();
            return item;
        }

        public string GenerateSecurityToken(string userId, string email)
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
