// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AzureMobile.Common.Test;
using Microsoft.AzureMobile.Common.Test.Extensions;
using Microsoft.AzureMobile.Common.Test.Models;
using Microsoft.AzureMobile.Common.Test.TestData;
using Microsoft.AzureMobile.Server.Extensions;
using Microsoft.AzureMobile.WebService.Test;
using Xunit;

namespace Microsoft.AzureMobile.Server.Test.Tables.HTTP
{
    [ExcludeFromCodeCoverage(Justification = "Test suite")]
    public class Delete_Tests
    {
        [Theory, CombinatorialData]
        public async Task BasicDeleteTests([CombinatorialRange(0, Movies.Count)] int index)
        {
            // Arrange
            var server = Program.CreateTestServer();
            var repository = server.GetRepository<InMemoryMovie>();
            var entityCount = repository.Entities.Count;
            var id = Utils.GetMovieId(index);

            // Act
            var response = await server.SendRequest(HttpMethod.Delete, $"tables/movies/{id}").ConfigureAwait(false);

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            Assert.Equal(entityCount - 1, repository.Entities.Count);
            Assert.Null(repository.GetEntity(id));
        }

        [Theory]
        [InlineData("tables/movies/not-found", HttpStatusCode.NotFound)]
        [InlineData("tables/movies_pagesize/not-found", HttpStatusCode.NotFound)]
        public async Task FailedDeleteTests(
            string relativeUri,
            HttpStatusCode expectedStatusCode,
            string headerName = null,
            string headerValue = null)
        {
            // Arrange
            var server = Program.CreateTestServer();
            var repository = server.GetRepository<InMemoryMovie>();
            var entityCount = repository.Entities.Count;

            Dictionary<string, string> headers = new();
            if (headerName != null && headerValue != null) headers.Add(headerName, headerValue);

            // Act
            var response = await server.SendRequest(HttpMethod.Delete, relativeUri, headers).ConfigureAwait(false);

            // Assert
            Assert.Equal(expectedStatusCode, response.StatusCode);
            Assert.Equal(entityCount, repository.Entities.Count);
        }

        [Theory, CombinatorialData]
        public async Task AuthenticatedDeleteTests(
            [CombinatorialValues(0, 1, 2, 3, 7, 14, 25)] int index,
            [CombinatorialValues(null, "failed", "success")] string userId,
            [CombinatorialValues("movies_rated", "movies_legal")] string table)
        {
            // Arrange
            var server = Program.CreateTestServer();
            var repository = server.GetRepository<InMemoryMovie>();
            var expectedCount = repository.Entities.Count;
            string id = Utils.GetMovieId(index);

            Dictionary<string, string> headers = new();
            Utils.AddAuthHeaders(headers, userId);

            // Act
            var response = await server.SendRequest(HttpMethod.Delete, $"tables/{table}/{id}", headers).ConfigureAwait(false);

            // Assert
            if (userId != "success")
            {
                var statusCode = table.Contains("legal") ? HttpStatusCode.UnavailableForLegalReasons : HttpStatusCode.Unauthorized;
                Assert.Equal(statusCode, response.StatusCode);
                Assert.Equal(expectedCount, repository.Entities.Count);
                Assert.NotNull(repository.GetEntity(id));
            }
            else
            {
                Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
                Assert.Equal(expectedCount - 1, repository.Entities.Count);
                Assert.Null(repository.GetEntity(id));
            }
        }

        [Theory]
        [InlineData("If-Match", null, HttpStatusCode.NoContent)]
        [InlineData("If-Match", "\"dGVzdA==\"", HttpStatusCode.PreconditionFailed)]
        [InlineData("If-None-Match", null, HttpStatusCode.PreconditionFailed)]
        [InlineData("If-None-Match", "\"dGVzdA==\"", HttpStatusCode.NoContent)]
        [InlineData("If-Modified-Since", "Fri, 01 Mar 2019 15:00:00 GMT", HttpStatusCode.NoContent)]
        [InlineData("If-Modified-Since", "Sun, 03 Mar 2019 15:00:00 GMT", HttpStatusCode.PreconditionFailed)]
        [InlineData("If-Unmodified-Since", "Sun, 03 Mar 2019 15:00:00 GMT", HttpStatusCode.NoContent)]
        [InlineData("If-Unmodified-Since", "Fri, 01 Mar 2019 15:00:00 GMT", HttpStatusCode.PreconditionFailed)]
        public async Task ConditionalDeleteTests(string headerName, string headerValue, HttpStatusCode expectedStatusCode)
        {
            // Arrange
            var server = Program.CreateTestServer();
            var repository = server.GetRepository<InMemoryMovie>();
            var expectedCount = repository.Entities.Count;
            const string id = "id-107";
            var expected = repository.GetEntity(id);
            expected.UpdatedAt = DateTimeOffset.Parse("Sat, 02 Mar 2019 15:00:00 GMT");
            Dictionary<string, string> headers = new() { { headerName, headerValue ?? expected.GetETag() } };

            // Act
            var response = await server.SendRequest(HttpMethod.Delete, $"tables/movies/{id}", headers).ConfigureAwait(false);

            // Assert
            Assert.Equal(expectedStatusCode, response.StatusCode);

            switch (expectedStatusCode)
            {
                case HttpStatusCode.NoContent:
                    Assert.Equal(expectedCount - 1, repository.Entities.Count);
                    Assert.Null(repository.GetEntity(id));
                    break;
                case HttpStatusCode.PreconditionFailed:
                    var actual = response.DeserializeContent<ClientMovie>();
                    Assert.Equal<IMovie>(expected, actual);
                    AssertEx.SystemPropertiesMatch(expected, actual);
                    AssertEx.ResponseHasConditionalHeaders(expected, response);
                    break;
            }
        }

        [Theory, CombinatorialData]
        public async Task SoftDeleteItem_SetsDeletedFlag([CombinatorialValues("soft", "soft_logged")] string table)
        {
            // Arrange
            const int index = 24;
            var server = Program.CreateTestServer();
            var repository = server.GetRepository<SoftMovie>();
            var entityCount = repository.Entities.Count;
            var id = Utils.GetMovieId(index);

            // Act
            var response = await server.SendRequest(HttpMethod.Delete, $"tables/{table}/{id}").ConfigureAwait(false);

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            Assert.Equal(entityCount, repository.Entities.Count);
            var entity = repository.GetEntity(id);
            Assert.True(entity.Deleted);
        }

        [Theory, CombinatorialData]
        public async Task SoftDeleteItem_GoneWhenDeleted([CombinatorialValues("soft", "soft_logged")] string table)
        {
            // Arrange
            const int index = 25;
            var server = Program.CreateTestServer();
            var repository = server.GetRepository<SoftMovie>();
            var entityCount = repository.Entities.Count;
            var id = Utils.GetMovieId(index);

            // Act
            var response = await server.SendRequest(HttpMethod.Delete, $"tables/{table}/{id}").ConfigureAwait(false);

            // Assert
            Assert.Equal(HttpStatusCode.Gone, response.StatusCode);
            Assert.Equal(entityCount, repository.Entities.Count);
            var entity = repository.GetEntity(id);
            Assert.True(entity.Deleted);
        }
    }
}
