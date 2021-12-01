// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

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
    public class DatasyncConflictException_Tests : OldBaseTest
    {
        private class MockObject
        {
            public string StringValue { get; set; }
        }

        private readonly string StandardContent = "{\"stringValue\":\"test\"}";
        private readonly HttpRequestMessage StandardRequest;
        private readonly HttpResponseMessage StandardResponse;

        public DatasyncConflictException_Tests()
        {
            StandardRequest = new(HttpMethod.Post, Endpoint);
            StandardResponse = new(HttpStatusCode.Conflict) { Content = new StringContent(StandardContent, Encoding.UTF8, "application/json") };
        }

        [Fact]
        [Trait("Method", "CreateAsync")]
        public async Task CreateAsync_NullRequest_Throws()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => DatasyncConflictException<MockObject>.CreateAsync(null, StandardResponse, ClientOptions.DeserializerOptions)).ConfigureAwait(false);
        }

        [Fact]
        [Trait("Method", "CreateAsync")]
        public async Task CreateAsync_NullResponse_Throws()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => DatasyncConflictException<MockObject>.CreateAsync(StandardRequest, null, ClientOptions.DeserializerOptions)).ConfigureAwait(false);
        }

        [Fact]
        [Trait("Method", "CreateAsync")]
        public async Task CreateAsync_NullOptions_Throws()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => DatasyncConflictException<MockObject>.CreateAsync(StandardRequest, StandardResponse, null)).ConfigureAwait(false);
        }

        [Fact]
        [Trait("Method", "CreateAsync")]
        public async Task CreateAsync_CreatesException()
        {
            var exception = await DatasyncConflictException<MockObject>.CreateAsync(StandardRequest, StandardResponse, ClientOptions.DeserializerOptions).ConfigureAwait(false);

            Assert.Same(StandardRequest, exception.Request);
            Assert.Same(StandardResponse, exception.Response);
            Assert.Equal(409, exception.StatusCode);
            Assert.NotNull(exception.ServerItem);
            Assert.Equal("test", exception.ServerItem.StringValue);
            Assert.Equal(StandardContent, Encoding.UTF8.GetString(exception.Content));
        }

        [Fact]
        [Trait("Method", "CreateAsync")]
        public async Task CreateAsync_Empty_CreatesException()
        {
            var emptyResponse = new HttpResponseMessage(HttpStatusCode.NoContent);
            var exception = await DatasyncConflictException<MockObject>.CreateAsync(StandardRequest, emptyResponse, ClientOptions.DeserializerOptions).ConfigureAwait(false);

            Assert.Same(StandardRequest, exception.Request);
            Assert.Same(emptyResponse, exception.Response);
            Assert.Equal(204, exception.StatusCode);
            Assert.Null(exception.ServerItem);
            Assert.Empty(exception.Content);
        }
    }
}
