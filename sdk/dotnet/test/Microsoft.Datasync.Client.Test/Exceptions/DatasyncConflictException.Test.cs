// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Test;
using Datasync.Common.Test.Models;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Datasync.Client.Test.Exceptions
{
    [ExcludeFromCodeCoverage]
    public class DatasyncConflictException_Tests : BaseTest
    {
        [Fact]
        [Trait("Method", "CreateAsync")]
        public async Task CreateAsync_NullRequest_Throws()
        {
            var options = new DatasyncClientOptions();
            var response = new HttpResponseMessage(HttpStatusCode.Conflict);
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                DatasyncConflictException<NoIdEntity>.CreateAsync(null, response, options.DeserializerOptions)).ConfigureAwait(false);
        }

        [Fact]
        [Trait("Method", "CreateAsync")]
        public async Task CreateAsync_NullResponse_Throws()
        {
            var options = new DatasyncClientOptions();
            var request = new HttpRequestMessage(HttpMethod.Post, Endpoint);
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                DatasyncConflictException<NoIdEntity>.CreateAsync(request, null, options.DeserializerOptions)).ConfigureAwait(false);
        }

        [Fact]
        [Trait("Method", "CreateAsync")]
        public async Task CreateAsync_NullOptions_Throws()
        {
            var request = new HttpRequestMessage(HttpMethod.Post, Endpoint);
            var response = new HttpResponseMessage(HttpStatusCode.Conflict);
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                DatasyncConflictException<NoIdEntity>.CreateAsync(request, response, null)).ConfigureAwait(false);
        }

        [Fact]
        [Trait("Method", "CreateAsync")]
        public async Task CreateAsync_CreatesException()
        {
            var options = new DatasyncClientOptions();
            const string json = "{\"stringValue\":\"test\"}";
            var request = new HttpRequestMessage(HttpMethod.Post, Endpoint);
            var response = new HttpResponseMessage(HttpStatusCode.Conflict)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };

            var exception = await DatasyncConflictException<NoIdEntity>.CreateAsync(request, response, options.DeserializerOptions).ConfigureAwait(false);

            Assert.Same(request, exception.Request);
            Assert.Same(response, exception.Response);
            Assert.Equal(409, exception.StatusCode);
            Assert.NotNull(exception.ServerItem);
            Assert.Equal("test", exception.ServerItem.StringValue);
            Assert.Equal(json, Encoding.UTF8.GetString(exception.Content));
        }

        [Fact]
        [Trait("Method", "CreateAsync")]
        public async Task CreateAsync_Empty_CreatesException()
        {
            var options = new DatasyncClientOptions();
            var request = new HttpRequestMessage(HttpMethod.Post, Endpoint);
            var emptyResponse = new HttpResponseMessage(HttpStatusCode.NoContent);
            var exception = await DatasyncConflictException<NoIdEntity>.CreateAsync(request, emptyResponse, options.DeserializerOptions).ConfigureAwait(false);

            Assert.Same(request, exception.Request);
            Assert.Same(emptyResponse, exception.Response);
            Assert.Equal(204, exception.StatusCode);
            Assert.Null(exception.ServerItem);
            Assert.Empty(exception.Content);
        }
    }
}
