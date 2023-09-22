// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Test;
using Microsoft.Datasync.Client.Test.Helpers;
using Newtonsoft.Json;

namespace Microsoft.Datasync.Client.Test.Table.Operations.RemoteTable;

[ExcludeFromCodeCoverage]
public class DeleteItemAsync_Tests : BaseOperationTest
{
    [Fact]
    [Trait("Method", "DeleteItemAsync")]
    public async Task DeleteItemAsync_ThrowsOnNull()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() => table.DeleteItemAsync(null)).ConfigureAwait(false);
    }

    [Theory]
    [MemberData(nameof(BaseOperationTest.GetInvalidIds), MemberType = typeof(BaseOperationTest))]
    [Trait("Method", "DeleteItemAsync")]
    public async Task DeleteItemAsync_ThrowsOnInvalidId(string id)
    {
        var json = CreateJsonDocument(new IdOnly { Id = id });
        await Assert.ThrowsAsync<ArgumentException>(() => table.DeleteItemAsync(json)).ConfigureAwait(false);
    }

    [Fact]
    [Trait("Method", "DeleteItemAsync")]
    public async Task DeleteItemAsync_FormsCorrectResponse_NoPrecondition()
    {
        // Arrange
        MockHandler.AddResponse(HttpStatusCode.NoContent);

        // Act
        _ = await table.DeleteItemAsync(jIdOnly).ConfigureAwait(false);

        // Assert
        var request = AssertSingleRequest(HttpMethod.Delete, expectedEndpoint);
        Assert.False(request.Headers.Contains("If-Match"));
    }

    [Fact]
    [Trait("Method", "DeleteItemAsync")]
    public async Task DeleteItemAsync_FormsCorrectResponse_WithPrecondition()
    {
        // Arrange
        MockHandler.AddResponse(HttpStatusCode.NoContent);

        // Act
        _ = await table.DeleteItemAsync(jIdEntity).ConfigureAwait(false);

        // Check Request
        var request = AssertSingleRequest(HttpMethod.Delete, expectedEndpoint);
        AssertEx.HasHeader(request.Headers, "If-Match", "\"etag\"");
    }

    [Fact]
    [Trait("Method", "DeleteItemAsync")]
    public async Task DeleteItemAsync_Auth_NoPrecondition()
    {
        // Arrange
        MockHandler.AddResponse(HttpStatusCode.NoContent);

        // Act
        _ = await authTable.DeleteItemAsync(jIdOnly).ConfigureAwait(false);

        // Assert
        var request = AssertSingleRequest(HttpMethod.Delete, expectedEndpoint);
        AssertEx.HasHeader(request.Headers, "X-ZUMO-AUTH", ValidAuthenticationToken.Token);
        Assert.False(request.Headers.Contains("If-Match"));
    }

    [Fact]
    [Trait("Method", "DeleteItemAsync")]
    public async Task DeleteItemAsync_Auth_WithPrecondition()
    {
        // Arrange
        MockHandler.AddResponse(HttpStatusCode.NoContent);

        // Act
        _ = await authTable.DeleteItemAsync(jIdEntity).ConfigureAwait(false);

        // Assert
        var request = AssertSingleRequest(HttpMethod.Delete, expectedEndpoint);
        AssertEx.HasHeader(request.Headers, "X-ZUMO-AUTH", ValidAuthenticationToken.Token);
        AssertEx.HasHeader(request.Headers, "If-Match", "\"etag\"");
    }

    [Theory]
    [InlineData(HttpStatusCode.Conflict)]
    [InlineData(HttpStatusCode.PreconditionFailed)]
    [Trait("Method", "DeleteItemAsync")]
    public async Task DeleteItemAsync_Conflict_FormulatesCorrectResponse(HttpStatusCode statusCode)
    {
        // Arrange
        MockHandler.AddResponse(statusCode, payload);

        // Act
        var ex = await Assert.ThrowsAsync<DatasyncConflictException>(() => table.DeleteItemAsync(jIdEntity)).ConfigureAwait(false);

        // Assert
        Assert.Equal(statusCode, ex.Response.StatusCode);
        AssertJsonMatches(ex.Value);
    }

    [Theory]
    [InlineData(HttpStatusCode.Conflict)]
    [InlineData(HttpStatusCode.PreconditionFailed)]
    [Trait("Method", "DeleteItemAsync")]
    public async Task DeleteItemAsync_ConflictNoContent_FormulatesCorrectResponse(HttpStatusCode statusCode)
    {
        // Arrange
        MockHandler.AddResponse(statusCode);

        // Act
        var ex = await Assert.ThrowsAsync<DatasyncInvalidOperationException>(() => table.DeleteItemAsync(jIdEntity)).ConfigureAwait(false);

        // Assert
        Assert.Equal(statusCode, ex.Response.StatusCode);
    }

    [Theory]
    [InlineData(HttpStatusCode.Conflict)]
    [InlineData(HttpStatusCode.PreconditionFailed)]
    [Trait("Method", "DeleteItemAsync")]
    public async Task DeleteItemAsync_ConflictWithBadJson_Throws(HttpStatusCode statusCode)
    {
        // Arrange
        ReturnBadJson(statusCode);

        // Act
        await Assert.ThrowsAnyAsync<JsonException>(() => table.DeleteItemAsync(jIdEntity)).ConfigureAwait(false);
    }

    [Theory]
    [InlineData(HttpStatusCode.BadRequest)]
    [InlineData(HttpStatusCode.MethodNotAllowed)]
    [InlineData(HttpStatusCode.Unauthorized)]
    [InlineData(HttpStatusCode.InternalServerError)]
    [Trait("Method", "DeleteItemAsync")]
    public async Task DeleteItemAsync_RequestFailed_Throws(HttpStatusCode statusCode)
    {
        // Arrange
        MockHandler.AddResponse(statusCode);

        // Act
        var ex = await Assert.ThrowsAsync<DatasyncInvalidOperationException>(() => table.DeleteItemAsync(jIdEntity)).ConfigureAwait(false);

        // Assert
        Assert.Equal(statusCode, ex.Response.StatusCode);
    }
}
