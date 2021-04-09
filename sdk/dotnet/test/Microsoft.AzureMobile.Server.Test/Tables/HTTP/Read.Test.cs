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
    public class Read_Tests
    {
        [Theory, CombinatorialData]
        public async Task BasicReadTests(
            [CombinatorialRange(0, Movies.Count)] int index,
            [CombinatorialValues("movies", "movies_pagesize")] string table
        )
        {
            // Arrange
            var server = Program.CreateTestServer();
            var repository = server.GetRepository<InMemoryMovie>();
            string id = Utils.GetMovieId(index);
            var expected = repository.GetEntity(id);

            // Act
            var response = await server.SendRequest(HttpMethod.Get, $"tables/{table}/{id}").ConfigureAwait(false);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var actual = response.DeserializeContent<ClientMovie>();

            // Records match the repository
            Assert.Equal<IMovie>(expected, actual);
            AssertEx.SystemPropertiesMatch(expected, actual);
            AssertEx.ResponseHasConditionalHeaders(expected, response);
        }

        [Theory]
        [InlineData("tables/movies/not-found", HttpStatusCode.NotFound)]
        [InlineData("tables/movies_pagesize/not-found", HttpStatusCode.NotFound)]
        public async Task FailedReadTests(
            string relativeUri,
            HttpStatusCode expectedStatusCode,
            string headerName = null,
            string headerValue = null)
        {
            // Arrange
            var server = Program.CreateTestServer();
            Dictionary<string, string> headers = new();
            if (headerName != null && headerValue != null) headers.Add(headerName, headerValue);

            // Act
            var response = await server.SendRequest(HttpMethod.Get, relativeUri, headers).ConfigureAwait(false);

            // Assert
            Assert.Equal(expectedStatusCode, response.StatusCode);
        }

        [Theory, CombinatorialData]
        public async Task AuthenticatedReadTests(
            [CombinatorialValues(0, 1, 2, 3, 7, 14, 25)] int index,
            [CombinatorialValues(null, "failed", "success")] string userId,
            [CombinatorialValues("movies_rated", "movies_legal")] string table)
        {
            // Arrange
            var server = Program.CreateTestServer();
            var repository = server.GetRepository<InMemoryMovie>();
            string id = Utils.GetMovieId(index);
            var expected = repository.GetEntity(id);
            Dictionary<string, string> headers = new();
            if (userId != null)
            {
                headers.Add("X-Auth", userId);
            }

            // Act
            var response = await server.SendRequest(HttpMethod.Get, $"tables/{table}/{id}", headers).ConfigureAwait(false);

            // Assert
            if (userId != "success")
            {
                var statusCode = table.Contains("legal") ? HttpStatusCode.UnavailableForLegalReasons : HttpStatusCode.Unauthorized;
                Assert.Equal(statusCode, response.StatusCode);
            }
            else
            {
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                var actual = response.DeserializeContent<ClientMovie>();

                // Records match the repository
                Assert.Equal<IMovie>(expected, actual);
                AssertEx.SystemPropertiesMatch(expected, actual);
                AssertEx.ResponseHasConditionalHeaders(expected, response);
            }
        }

        [Theory]
        [InlineData("If-Match", null, HttpStatusCode.OK)]
        [InlineData("If-Match", "\"dGVzdA==\"", HttpStatusCode.PreconditionFailed)]
        [InlineData("If-None-Match", null, HttpStatusCode.NotModified)]
        [InlineData("If-None-Match", "\"dGVzdA==\"", HttpStatusCode.OK)]
        [InlineData("If-Modified-Since", "Fri, 01 Mar 2019 15:00:00 GMT", HttpStatusCode.OK)]
        [InlineData("If-Modified-Since", "Sun, 03 Mar 2019 15:00:00 GMT", HttpStatusCode.NotModified)]
        [InlineData("If-Unmodified-Since", "Sun, 03 Mar 2019 15:00:00 GMT", HttpStatusCode.OK)]
        [InlineData("If-Unmodified-Since", "Fri, 01 Mar 2019 15:00:00 GMT", HttpStatusCode.PreconditionFailed)]
        public async Task ConditionalReadTests(string headerName, string headerValue, HttpStatusCode expectedStatusCode)
        {
            // Arrange
            var server = Program.CreateTestServer();
            var repository = server.GetRepository<InMemoryMovie>();
            const string id = "id-107";
            var expected = repository.GetEntity(id);
            expected.UpdatedAt = DateTimeOffset.Parse("Sat, 02 Mar 2019 15:00:00 GMT");
            Dictionary<string, string> headers = new() { { headerName, headerValue ?? expected.GetETag() } };

            // Act
            var response = await server.SendRequest(HttpMethod.Get, $"tables/movies/{id}", headers).ConfigureAwait(false);

            // Assert
            Assert.Equal(expectedStatusCode, response.StatusCode);

            if (expectedStatusCode == HttpStatusCode.OK || expectedStatusCode == HttpStatusCode.PreconditionFailed)
            {
                var actual = response.DeserializeContent<ClientMovie>();

                // Records match the repository
                Assert.Equal<IMovie>(expected, actual);
                AssertEx.SystemPropertiesMatch(expected, actual);
                AssertEx.ResponseHasConditionalHeaders(expected, response);
            }
        }

        [Theory, CombinatorialData]
        public async Task ReadSoftDeletedItem_WorksIfNotDeleted([CombinatorialValues("soft", "soft_logged")] string table)
        {
            // Arrange
            int index = 24;
            var server = Program.CreateTestServer();
            var repository = server.GetRepository<SoftMovie>();
            string id = Utils.GetMovieId(index);
            var expected = repository.GetEntity(id);

            // Act
            var response = await server.SendRequest(HttpMethod.Get, $"tables/{table}/{id}").ConfigureAwait(false);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var actual = response.DeserializeContent<ClientMovie>();

            // Records match the repository
            Assert.Equal<IMovie>(expected, actual);
            AssertEx.SystemPropertiesMatch(expected, actual);
            AssertEx.ResponseHasConditionalHeaders(expected, response);
        }

        [Theory, CombinatorialData]
        public async Task ReadSoftDeletedItem_ReturnsGoneIfDeleted([CombinatorialValues("soft", "soft_logged")] string table)
        {
            // Arrange
            int index = 25;
            var server = Program.CreateTestServer();
            string id = Utils.GetMovieId(index);

            // Act
            var response = await server.SendRequest(HttpMethod.Get, $"tables/{table}/{id}").ConfigureAwait(false);

            // Assert
            Assert.Equal(HttpStatusCode.Gone, response.StatusCode);
        }
    }
}
