// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Test;
using Newtonsoft.Json;

namespace Microsoft.Datasync.Client.Test.Table.Operations.RemoteTable;

[ExcludeFromCodeCoverage]
public class InsertItemAsync_Tests : BaseOperationTest
{
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
        var response = await table.InsertItemAsync(jIdEntity).ConfigureAwait(false);

        // Assert
        var request = AssertSingleRequest(HttpMethod.Post, tableEndpoint);
        Assert.Equal(sIdEntity, await request.Content.ReadAsStringAsync().ConfigureAwait(false));
        Assert.Equal("application/json", request.Content.Headers.ContentType.MediaType);
        AssertJsonMatches(response);
    }

    [Theory]
    [InlineData(HttpStatusCode.OK)]
    [InlineData(HttpStatusCode.Created)]
    [Trait("Method", "InsertItemAsync")]
    public async Task InsertItemAsync_Success_FormulatesCorrectRequest_WithETag(HttpStatusCode statusCode)
    {
        // Arrange
        MockHandler.AddResponse(statusCode, payload);
        MockHandler.Responses[0].Headers.Add("ETag", "\"foo\"");

        // Act
        var response = await table.InsertItemAsync(jIdEntity).ConfigureAwait(false);

        // Assert
        var request = AssertSingleRequest(HttpMethod.Post, tableEndpoint);
        Assert.Equal(sIdEntity, await request.Content.ReadAsStringAsync().ConfigureAwait(false));
        Assert.Equal("application/json", request.Content.Headers.ContentType.MediaType);
        AssertJsonMatches(response);
        Assert.Equal("foo", response.Value<string>("version"));
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
        var response = await authTable.InsertItemAsync(jIdEntity).ConfigureAwait(false);

        // Assert
        var request = AssertSingleRequest(HttpMethod.Post, tableEndpoint);
        AssertEx.HasHeader(request.Headers, "X-ZUMO-AUTH", ValidAuthenticationToken.Token);
        Assert.Equal(sIdEntity, await request.Content.ReadAsStringAsync().ConfigureAwait(false));
        Assert.Equal("application/json", request.Content.Headers.ContentType.MediaType);
        AssertJsonMatches(response);
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
        await Assert.ThrowsAsync<DatasyncInvalidOperationException>(() => table.InsertItemAsync(jIdEntity)).ConfigureAwait(false);
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
        await Assert.ThrowsAnyAsync<JsonException>(() => table.InsertItemAsync(jIdEntity)).ConfigureAwait(false);
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
        var ex = await Assert.ThrowsAsync<DatasyncConflictException>(() => table.InsertItemAsync(jIdEntity)).ConfigureAwait(false);

        // Assert
        var request = AssertSingleRequest(HttpMethod.Post, tableEndpoint);
        Assert.Equal(sIdEntity, request.Content.ReadAsStringAsync().Result);
        Assert.Equal("application/json", request.Content.Headers.ContentType.MediaType);
        Assert.NotNull(ex.Request);
        Assert.NotNull(ex.Response);
        AssertJsonMatches(ex.Value);
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
        var ex = await Assert.ThrowsAsync<DatasyncInvalidOperationException>(() => table.InsertItemAsync(jIdEntity)).ConfigureAwait(false);

        // Assert
        Assert.NotNull(ex.Request);
        Assert.NotNull(ex.Response);
        Assert.Null(ex.Value);
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
        await Assert.ThrowsAnyAsync<JsonException>(() => table.InsertItemAsync(jIdEntity)).ConfigureAwait(false);
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
        var ex = await Assert.ThrowsAsync<DatasyncInvalidOperationException>(() => table.InsertItemAsync(jIdEntity)).ConfigureAwait(false);

        // Assert
        Assert.Equal(statusCode, ex.Response?.StatusCode);
    }
}
