// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Test;
using Datasync.Common.Test.Models;
using Microsoft.AspNetCore.Datasync.Extensions;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

using TestData = Datasync.Common.Test.TestData;

namespace Microsoft.Datasync.Integration.Test.MovieServer
{
    [ExcludeFromCodeCoverage(Justification = "Test suite")]
    public class Read_Tests : BaseTest
    {
        [Theory, CombinatorialData]
        public async Task BasicReadTests([CombinatorialValues("movies", "movies_pagesize")] string table)
        {
            string id = GetRandomId();
            var expected = MovieServer.GetMovieById(id)!;

            var response = await MovieServer.SendRequest(HttpMethod.Get, $"tables/{table}/{id}").ConfigureAwait(false);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var actual = response.DeserializeContent<ClientMovie>();
            Assert.Equal<IMovie>(expected, actual!);
            AssertEx.SystemPropertiesMatch(expected, actual);
            AssertEx.ResponseHasConditionalHeaders(expected, response);
        }

        [Theory]
        [InlineData("tables/movies/not-found", HttpStatusCode.NotFound)]
        [InlineData("tables/movies_pagesize/not-found", HttpStatusCode.NotFound)]
        public async Task FailedReadTests(string relativeUri, HttpStatusCode expectedStatusCode)
        {
            var response = await MovieServer.SendRequest(HttpMethod.Get, relativeUri).ConfigureAwait(false);
            Assert.Equal(expectedStatusCode, response.StatusCode);
        }

        [Theory, CombinatorialData]
        public async Task AuthenticatedReadTests(
            [CombinatorialValues(0, 1, 2, 3, 7, 14, 25)] int index,
            [CombinatorialValues(null, "failed", "success")] string userId,
            [CombinatorialValues("movies_rated", "movies_legal")] string table)
        {
            string id = Utils.GetMovieId(index);
            var expected = MovieServer.GetMovieById(id)!;
            Dictionary<string, string> headers = new();
            Utils.AddAuthHeaders(headers, userId);

            var response = await MovieServer.SendRequest(HttpMethod.Get, $"tables/{table}/{id}", headers).ConfigureAwait(false);

            if (userId != "success")
            {
                var statusCode = table.Contains("legal") ? HttpStatusCode.UnavailableForLegalReasons : HttpStatusCode.Unauthorized;
                Assert.Equal(statusCode, response.StatusCode);
            }
            else
            {
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                var actual = response.DeserializeContent<ClientMovie>();
                Assert.Equal<IMovie>(expected, actual!);
                AssertEx.SystemPropertiesMatch(expected, actual);
                AssertEx.ResponseHasConditionalHeaders(expected, response);
            }
        }

        [Theory]
        [InlineData("If-Match", null, HttpStatusCode.OK)]
        [InlineData("If-Match", "\"dGVzdA==\"", HttpStatusCode.PreconditionFailed)]
        [InlineData("If-None-Match", null, HttpStatusCode.NotModified)]
        [InlineData("If-None-Match", "\"dGVzdA==\"", HttpStatusCode.OK)]
        public async Task ConditionalVersionReadTests(string headerName, string headerValue, HttpStatusCode expectedStatusCode)
        {
            var id = GetRandomId();
            var expected = MovieServer.GetMovieById(id)!;
            Dictionary<string, string> headers = new()
            {
                { headerName, headerValue ?? expected.GetETag() }
            };

            var response = await MovieServer.SendRequest(HttpMethod.Get, $"tables/movies/{id}", headers).ConfigureAwait(false);

            Assert.Equal(expectedStatusCode, response.StatusCode);
            if (expectedStatusCode == HttpStatusCode.OK || expectedStatusCode == HttpStatusCode.PreconditionFailed)
            {
                var actual = response.DeserializeContent<ClientMovie>();
                Assert.Equal<IMovie>(expected, actual!);
                AssertEx.SystemPropertiesMatch(expected, actual);
                AssertEx.ResponseHasConditionalHeaders(expected, response);
            }
        }

        [Theory]
        [InlineData("If-Modified-Since", -1, HttpStatusCode.OK)]
        [InlineData("If-Modified-Since", 1, HttpStatusCode.NotModified)]
        [InlineData("If-Unmodified-Since", 1, HttpStatusCode.OK)]
        [InlineData("If-Unmodified-Since", -1, HttpStatusCode.PreconditionFailed)]
        public async Task ConditionalReadTests(string headerName, int offset, HttpStatusCode expectedStatusCode)
        {
            var id = GetRandomId();
            var expected = MovieServer.GetMovieById(id)!;
            Dictionary<string, string> headers = new()
            {
                { headerName, expected.UpdatedAt.AddHours(offset).ToString("R") }
            };

            var response = await MovieServer.SendRequest(HttpMethod.Get, $"tables/movies/{id}", headers).ConfigureAwait(false);

            Assert.Equal(expectedStatusCode, response.StatusCode);
            if (expectedStatusCode == HttpStatusCode.OK || expectedStatusCode == HttpStatusCode.PreconditionFailed)
            {
                var actual = response.DeserializeContent<ClientMovie>();
                Assert.Equal<IMovie>(expected, actual!);
                AssertEx.SystemPropertiesMatch(expected, actual);
                AssertEx.ResponseHasConditionalHeaders(expected, response);
            }
        }

        [Theory, CombinatorialData]
        public async Task ReadSoftDeletedItem_WorksIfNotDeleted([CombinatorialValues("soft", "soft_logged")] string table)
        {
            var id = GetRandomId();
            var expected = MovieServer.GetMovieById(id)!;

            var response = await MovieServer.SendRequest(HttpMethod.Get, $"tables/{table}/{id}").ConfigureAwait(false);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var actual = response.DeserializeContent<ClientMovie>();
            Assert.Equal<IMovie>(expected, actual!);
            AssertEx.SystemPropertiesMatch(expected, actual);
            AssertEx.ResponseHasConditionalHeaders(expected, response);
        }

        [Theory, CombinatorialData]
        public async Task ReadSoftDeletedItem_ReturnsGoneIfDeleted([CombinatorialValues("soft", "soft_logged")] string table)
        {
            var id = GetRandomId();
            await MovieServer.SoftDeleteMoviesAsync(x => x.Id == id).ConfigureAwait(false);

            var response = await MovieServer.SendRequest(HttpMethod.Get, $"tables/{table}/{id}").ConfigureAwait(false);
            Assert.Equal(HttpStatusCode.Gone, response.StatusCode);
        }
    }
}
