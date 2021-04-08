// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AzureMobile.Common.Test.Extensions;
using Microsoft.AzureMobile.Common.Test.Models;
using Microsoft.AzureMobile.Server.InMemory;
using Microsoft.AzureMobile.WebService.Test;
using Xunit;

namespace Microsoft.AzureMobile.Server.Test.Tables
{
    [ExcludeFromCodeCoverage(Justification = "Test suite")]
    public class TableController_Tests
    {
        [Fact]
        public void Repository_Throws_WhenSetNull()
        {
            // Arrange
            var controller = new TableController<InMemoryMovie>();

            // Act
            Assert.Throws<ArgumentNullException>(() => controller.Repository = null);
        }

        [Fact]
        public void Repository_Throws_WhenGetNull()
        {
            // Arrange
            var controller = new TableController<InMemoryMovie>();

            // Act
            Assert.Throws<InvalidOperationException>(() => controller.Repository);
        }

        [Fact]
        [SuppressMessage("Style", "IDE0017:Simplify object initialization", Justification = "Proper split of arrange/act/assert")]
        public void Repository_CanBeStored()
        {
            // Arrange
            var repository = new InMemoryRepository<InMemoryMovie>();
            var controller = new TableController<InMemoryMovie>();

            // Act
            controller.Repository = repository;

            // Assert
            Assert.NotNull(controller.Repository);
            Assert.Equal(repository, controller.Repository);
        }

        [Theory]
        [InlineData("tables/movies/id-001", null)]
        [InlineData("tables/movies/id-001?X-ZUMO-Version=true", null)]
        [InlineData("tables/movies/id-001?X-ZUMO-Version=0", null)]
        [InlineData("tables/movies/id-001?X-ZUMO-Version=somevalue", null)]
        [InlineData("tables/movies/id-001?X-ZUMO-Version=1.0", null)]
        [InlineData("tables/movies/id-001?X-ZUMO-Version=9.0.0", null)]
        [InlineData("tables/movies/id-001", "true")]
        [InlineData("tables/movies/id-001", "0")]
        [InlineData("tables/movies/id-001", "somevalue")]
        [InlineData("tables/movies/id-001", "1.0")]
        [InlineData("tables/movies/id-001", "9.0.0")]
        public async Task ZumoVersion_MissingOrInvalid_BadRequest(string relativeUri,string headerValue)
        {
            // Arrange
            var server = Program.CreateTestServer();
            var client = server.CreateClient();
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"https://localhost/{relativeUri}")
            };
            if (headerValue != null)
            {
                request.Headers.Add("X-ZUMO-Version", headerValue);
            }

            // Act
            var response = await client.SendAsync(request).ConfigureAwait(false);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var result = await response.DeserializeContentAsync<Dictionary<string, object>>().ConfigureAwait(false);
            Assert.True(result.ContainsKey("title"));
            Assert.True(result.ContainsKey("detail"));
        }

        [Theory]
        [InlineData("tables/movies/id-001?X-ZUMO-Version=3.0", null)]
        [InlineData("tables/movies/id-001?X-ZUMO-Version=3.0.0", null)]
        [InlineData("tables/movies/id-001?X-ZUMO-Version=3.0.1", null)]
        [InlineData("tables/movies/id-001", "3.0")]
        [InlineData("tables/movies/id-001", "3.0.0")]
        [InlineData("tables/movies/id-001", "3.0.1")]
        public async Task ZumoVersion_V3_0_OK(string relativeUri, string headerValue)
        {
            // Arrange
            var server = Program.CreateTestServer();
            var client = server.CreateClient();
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"https://localhost/{relativeUri}")
            };
            if (headerValue != null)
            {
                request.Headers.Add("X-ZUMO-Version", headerValue);
            }

            // Act
            var response = await client.SendAsync(request).ConfigureAwait(false);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}
