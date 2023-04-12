// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Test;
using Microsoft.Datasync.Client.Test.Helpers;
using Newtonsoft.Json;

namespace Microsoft.Datasync.Client.Test.Table.Operations.RemoteTable;

[ExcludeFromCodeCoverage]
public class ReplaceItemAsync_Tests : BaseOperationTest
{
    [Fact]
    [Trait("Method", "ReplaceItemAsync")]
    public async Task ReplaceItemAsync_ThrowsOnNull()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() => table.ReplaceItemAsync(null)).ConfigureAwait(false);
    }

    [Fact]
    [Trait("Method", "ReplaceItemAsync")]
    public async Task ReplaceItemAsync_ThrowsOnNullId()
    {
        var json = CreateJsonDocument(new IdEntity { Id = null });
        await Assert.ThrowsAsync<ArgumentException>(() => table.ReplaceItemAsync(json)).ConfigureAwait(false);
    }

    [Theory]
    [MemberData(nameof(BaseOperationTest.GetInvalidIds), MemberType =typeof(BaseOperationTest))]
    [Trait("Method", "ReplaceItemAsync")]
    public async Task ReplaceItemAsync_ThrowsOnInvalidId(string id)
    {
        var json = CreateJsonDocument(new IdEntity { Id = id });
        await Assert.ThrowsAsync<ArgumentException>(() => table.ReplaceItemAsync(json)).ConfigureAwait(false);
    }

    [Fact]
    [Trait("Method", "ReplaceItemAsync")]
    public async Task ReplaceItemAsync_FormulatesCorrectResponse_NoPrecondition()
    {
        // Arrange
        MockHandler.AddResponse(HttpStatusCode.OK, payload);

        // Act
        var response = await table.ReplaceItemAsync(jIdOnly).ConfigureAwait(false);

        // Assert
        var request = AssertSingleRequest(HttpMethod.Put, expectedEndpoint);
        Assert.False(request.Headers.Contains("If-Match"));
        AssertJsonMatches(response);
    }

    [Fact]
    [Trait("Method", "ReplaceItemAsync")]
    public async Task ReplaceItemAsync_FormulatesCorrectResponse_WithPrecondition()
    {
        // Arrange
        MockHandler.AddResponse(HttpStatusCode.OK, payload);

        // Act
        var response = await table.ReplaceItemAsync(jIdEntity).ConfigureAwait(false);

        // Assert
        var request = AssertSingleRequest(HttpMethod.Put, expectedEndpoint);
        AssertEx.HasHeader(request.Headers, "If-Match", "\"etag\"");
        AssertJsonMatches(response);
    }

    [Fact]
    [Trait("Method", "ReplaceItemAsync")]
    public async Task ReplaceItemAsync_FormulatesCorrectResponse_WithETag()
    {
        // Arrange
        MockHandler.AddResponse(HttpStatusCode.OK, payload);
        MockHandler.Responses[0].Headers.Add("ETag", "\"foo\"");

        // Act
        var response = await table.ReplaceItemAsync(jIdEntity).ConfigureAwait(false);

        // Assert
        var request = AssertSingleRequest(HttpMethod.Put, expectedEndpoint);
        AssertEx.HasHeader(request.Headers, "If-Match", "\"etag\"");
        AssertJsonMatches(response);
        Assert.Equal("foo", response.Value<string>("version"));
    }

    [Fact]
    [Trait("Method", "ReplaceItemAsync")]
    public async Task ReplaceItemAsync_Auth_FormulatesCorrectResponse_NoPrecondition()
    {
        // Arrange
        MockHandler.AddResponse(HttpStatusCode.OK, payload);

        // Act
        var response = await authTable.ReplaceItemAsync(jIdOnly).ConfigureAwait(false);

        // Assert
        var request = AssertSingleRequest(HttpMethod.Put, expectedEndpoint);
        AssertEx.HasHeader(request.Headers, "X-ZUMO-AUTH", ValidAuthenticationToken.Token);
        Assert.False(request.Headers.Contains("If-Match"));
        AssertJsonMatches(response);
    }

    [Fact]
    [Trait("Method", "ReplaceItemAsync")]
    public async Task ReplaceItemAsync_Auth_FormulatesCorrectResponse_WithPrecondition()
    {
        // Arrange
        MockHandler.AddResponse(HttpStatusCode.OK, payload);

        // Act
        var response = await authTable.ReplaceItemAsync(jIdEntity).ConfigureAwait(false);

        // Assert
        var request = AssertSingleRequest(HttpMethod.Put, expectedEndpoint);
        AssertEx.HasHeader(request.Headers, "X-ZUMO-AUTH", ValidAuthenticationToken.Token);
        AssertEx.HasHeader(request.Headers, "If-Match", "\"etag\"");
        AssertJsonMatches(response);
    }

    [Fact]
    [Trait("Method", "ReplaceItemAsync")]
    public async Task ReplaceItemAsync_SuccessNoContent()
    {
        // Arrange
        MockHandler.AddResponse(HttpStatusCode.OK);

        // Act
       await Assert.ThrowsAsync<DatasyncInvalidOperationException>(() => table.ReplaceItemAsync(jIdEntity)).ConfigureAwait(false);
    }

    [Theory]
    [InlineData(HttpStatusCode.Conflict)]
    [InlineData(HttpStatusCode.PreconditionFailed)]
    [Trait("Method", "ReplaceItemAsync")]
    public async Task ReplaceItemAsync_Conflict_FormulatesCorrectResponse(HttpStatusCode statusCode)
    {
        // Arrange
        MockHandler.AddResponse(statusCode, payload);

        // Act
        var ex = await Assert.ThrowsAsync<DatasyncConflictException>(() => table.ReplaceItemAsync(jIdEntity)).ConfigureAwait(false);

        // Assert
        Assert.NotNull(ex.Request);
        Assert.NotNull(ex.Response);
        AssertJsonMatches(ex.Value);
    }

    [Theory]
    [InlineData(HttpStatusCode.Conflict)]
    [InlineData(HttpStatusCode.PreconditionFailed)]
    [Trait("Method", "ReplaceItemAsync")]
    public async Task ReplaceItemAsync_ConflictNoContent(HttpStatusCode statusCode)
    {
        // Arrange
        MockHandler.AddResponse(statusCode);

        // Act
        var ex = await Assert.ThrowsAsync<DatasyncInvalidOperationException>(() => table.ReplaceItemAsync(jIdEntity)).ConfigureAwait(false);

        // Assert
        Assert.NotNull(ex.Request);
        Assert.NotNull(ex.Response);
        Assert.Null(ex.Value);
    }

    [Theory]
    [InlineData(HttpStatusCode.Conflict)]
    [InlineData(HttpStatusCode.PreconditionFailed)]
    [Trait("Method", "ReplaceItemAsync")]
    public async Task ReplaceItemAsync_ConflictWithBadJson_Throws(HttpStatusCode statusCode)
    {
        // Arrange
        ReturnBadJson(statusCode);

        // Act
        await Assert.ThrowsAnyAsync<JsonException>(() => table.ReplaceItemAsync(jIdEntity)).ConfigureAwait(false);
    }

    [Theory]
    [InlineData(HttpStatusCode.BadRequest)]
    [InlineData(HttpStatusCode.MethodNotAllowed)]
    [InlineData(HttpStatusCode.Unauthorized)]
    [InlineData(HttpStatusCode.InternalServerError)]
    [Trait("Method", "ReplaceItemAsync")]
    public async Task ReplaceItemAsync_RequestFailed_Throws(HttpStatusCode statusCode)
    {
        // Arrange
        MockHandler.AddResponse(statusCode);

        // Act
        var ex = await Assert.ThrowsAsync<DatasyncInvalidOperationException>(() => table.ReplaceItemAsync(jIdEntity)).ConfigureAwait(false);

        // Assert
        Assert.NotNull(ex.Request);
        Assert.Equal(statusCode, ex.Response?.StatusCode);
    }
}
