// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Test;
using Datasync.Common.Test.Models;
using Newtonsoft.Json;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Datasync.Client.Test.Table.Operations.RemoteTableOfT
{
    [ExcludeFromCodeCoverage]
    public class InsertItemAsync_Tests : BaseOperationTest
    {
        private readonly IdEntity sut = new() { Id = Guid.NewGuid().ToString("N") };

        [Fact]
        [Trait("Method", "InsertItemAsync")]
        public async Task InsertItemAsync_ThrowsOnNull()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => table.InsertItemAsync(null)).ConfigureAwait(false);
        }

        [Theory]
        [InlineData(HttpStatusCode.OK)]
        [InlineData(HttpStatusCode.Created)]
        [Trait("Method", "InsertItemAsync")]
        public async Task InsertItemAsync_Success_FormulatesCorrectRequest(HttpStatusCode statusCode)
        {
            // Arrange
            MockHandler.AddResponse(statusCode, payload);

            // Act
            await table.InsertItemAsync(sut).ConfigureAwait(false);

            // Assert
            var request = AssertSingleRequest(HttpMethod.Post, tableEndpoint);
            Assert.Equal(sJsonPayload, await request.Content.ReadAsStringAsync().ConfigureAwait(false));
            Assert.Equal("application/json", request.Content.Headers.ContentType.MediaType);
            Assert.Equal(payload, sut);
        }

        [Theory]
        [InlineData(HttpStatusCode.OK)]
        [InlineData(HttpStatusCode.Created)]
        [Trait("Method", "InsertItemAsync")]
        public async Task InsertItemAsync_Success_FormulatesCorrectRequest_WithAuth(HttpStatusCode statusCode)
        {
            // Arrange
            MockHandler.AddResponse(statusCode, payload);

            // Act
            await authTable.InsertItemAsync(sut).ConfigureAwait(false);

            // Assert
            var request = AssertSingleRequest(HttpMethod.Post, tableEndpoint);
            Assert.Equal(sJsonPayload, await request.Content.ReadAsStringAsync().ConfigureAwait(false));
            Assert.Equal("application/json", request.Content.Headers.ContentType.MediaType);
            AssertEx.HasHeader(request.Headers, "X-ZUMO-AUTH", ValidAuthenticationToken.Token);
            Assert.Equal(payload, sut);
        }

        [Theory]
        [InlineData(HttpStatusCode.OK)]
        [InlineData(HttpStatusCode.Created)]
        [Trait("Method", "InsertItemAsync")]
        public async Task InsertItemAsync_SuccessNoContent(HttpStatusCode statusCode)
        {
            // Arrange
            MockHandler.AddResponse(statusCode);

            // Act
            await table.InsertItemAsync(sut).ConfigureAwait(false);

            // Assert
            Assert.Null(sut.Id);
            Assert.Null(sut.StringValue);
        }

        [Theory]
        [InlineData(HttpStatusCode.OK)]
        [InlineData(HttpStatusCode.Created)]
        [Trait("Method", "InsertItemAsync")]
        public async Task InsertItemAsync_SuccessWithBadJson_Throws(HttpStatusCode statusCode)
        {
            // Arrange
            ReturnBadJson(statusCode);

            // Act
            await Assert.ThrowsAnyAsync<JsonException>(() => table.InsertItemAsync(payload)).ConfigureAwait(false);
        }

        [Theory]
        [InlineData(HttpStatusCode.Conflict)]
        [InlineData(HttpStatusCode.PreconditionFailed)]
        [Trait("Method", "InsertItemAsync")]
        public async Task InsertItemAsync_Conflict_FormulatesCorrectResponse(HttpStatusCode statusCode)
        {
            // Arrange
            MockHandler.AddResponse(statusCode, payload);

            // Act
            var ex = await Assert.ThrowsAsync<DatasyncConflictException<IdEntity>>(() => table.InsertItemAsync(sut)).ConfigureAwait(false);

            // Assert
            var request = AssertSingleRequest(HttpMethod.Post, tableEndpoint);
            Assert.Equal(sJsonPayload, request.Content.ReadAsStringAsync().Result);
            AssertEx.Equals("application/json", request.Content.Headers.ContentType.MediaType);
            Assert.Equal(payload, ex.Item);
        }

        [Theory]
        [InlineData(HttpStatusCode.Conflict)]
        [InlineData(HttpStatusCode.PreconditionFailed)]
        [Trait("Method", "InsertItemAsync")]
        public async Task InsertItemAsync_ConflictNoContent_Throws(HttpStatusCode statusCode)
        {
            // Arrange
            MockHandler.AddResponse(statusCode);

            // Act
            var ex = await Assert.ThrowsAsync<DatasyncConflictException<IdEntity>>(() => table.InsertItemAsync(sut)).ConfigureAwait(false);

            // Assert
            Assert.Null(ex.Item);
        }

        [Theory]
        [InlineData(HttpStatusCode.Conflict)]
        [InlineData(HttpStatusCode.PreconditionFailed)]
        [Trait("Method", "InsertItemAsync")]
        public async Task InsertItemAsync_ConflictWithBadJson_Throws(HttpStatusCode statusCode)
        {
            // Arrange
            ReturnBadJson(statusCode);

            // Act
            await Assert.ThrowsAnyAsync<JsonException>(() => table.InsertItemAsync(sut)).ConfigureAwait(false);
        }

        [Theory]
        [InlineData(HttpStatusCode.BadRequest)]
        [InlineData(HttpStatusCode.MethodNotAllowed)]
        [InlineData(HttpStatusCode.Unauthorized)]
        [InlineData(HttpStatusCode.InternalServerError)]
        [Trait("Method", "InsertItemAsync")]
        public async Task InsertItemAsync_RequestFailed_Throws(HttpStatusCode statusCode)
        {
            // Arrange
            MockHandler.AddResponse(statusCode);

            // Act
            var ex = await Assert.ThrowsAsync<DatasyncInvalidOperationException>(() => table.InsertItemAsync(sut)).ConfigureAwait(false);

            // Assert
            Assert.Equal(statusCode, ex.Response?.StatusCode);
        }
    }
}
