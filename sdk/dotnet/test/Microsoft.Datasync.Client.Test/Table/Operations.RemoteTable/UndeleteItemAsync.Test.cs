// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Test;
using Microsoft.Datasync.Client.Http;
using Microsoft.Datasync.Client.Test.Helpers;
using Newtonsoft.Json;

namespace Microsoft.Datasync.Client.Test.Table.Operations.RemoteTable;

[ExcludeFromCodeCoverage]
public class UndeleteItemAsync_Tests : BaseOperationTest
{
    [Fact]
    [Trait("Method", "UndeleteItemAsync")]
    public async Task UndeleteItemAsync_ThrowsOnNull()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() => table.UndeleteItemAsync(null)).ConfigureAwait(false);
    }

    [Fact]
    [Trait("Method", "UndeleteItemAsync")]
    public async Task UndeleteItemAsync_ThrowsOnNullId()
    {
        var json = CreateJsonDocument(new IdEntity { Id = null });
        await Assert.ThrowsAsync<ArgumentException>(() => table.UndeleteItemAsync(json)).ConfigureAwait(false);
    }

    [Theory]
    [MemberData(nameof(BaseOperationTest.GetInvalidIds), MemberType = typeof(BaseOperationTest))]
    [Trait("Method", "UndeleteItemAsync")]
    public async Task UndeleteItemAsync_ThrowsOnInvalidId(string id)
    {
        var json = CreateJsonDocument(new IdEntity { Id = id });
        await Assert.ThrowsAsync<ArgumentException>(() => table.UndeleteItemAsync(json)).ConfigureAwait(false);
    }

    [Fact]
    [Trait("Method", "UndeleteItemAsync")]
    public async Task UndeleteItemAsync_FormulatesCorrectResponse_NoPrecondition()
    {
        // Arrange
        MockHandler.AddResponse(HttpStatusCode.OK, payload);

        // Act
        var response = await table.UndeleteItemAsync(jIdOnly).ConfigureAwait(false);

        // Assert
        var request = AssertSingleRequest(ServiceRequest.PATCH, expectedEndpoint);
        Assert.Equal("{\"deleted\":false}", await request.Content.ReadAsStringAsync());
        Assert.False(request.Headers.Contains("If-Match"));
        AssertJsonMatches(response);
    }

    [Fact]
    [Trait("Method", "UndeleteItemAsync")]
    public async Task UndeleteItemAsync_FormulatesCorrectResponse_WithPrecondition()
    {
        // Arrange
        MockHandler.AddResponse(HttpStatusCode.OK, payload);

        // Act
        var response = await table.UndeleteItemAsync(jIdEntity).ConfigureAwait(false);

        // Assert
        var request = AssertSingleRequest(ServiceRequest.PATCH, expectedEndpoint);
        Assert.Equal("{\"deleted\":false}", await request.Content.ReadAsStringAsync());
        AssertEx.HasHeader(request.Headers, "If-Match", "\"etag\"");
        AssertJsonMatches(response);
    }

    [Fact]
    [Trait("Method", "UndeleteItemAsync")]
    public async Task UndeleteItemAsync_Auth_FormulatesCorrectResponse_NoPrecondition()
    {
        // Arrange
        MockHandler.AddResponse(HttpStatusCode.OK, payload);

        // Act
        var response = await authTable.UndeleteItemAsync(jIdOnly).ConfigureAwait(false);

        // Assert
        var request = AssertSingleRequest(ServiceRequest.PATCH, expectedEndpoint);
        Assert.Equal("{\"deleted\":false}", await request.Content.ReadAsStringAsync());
        AssertEx.HasHeader(request.Headers, "X-ZUMO-AUTH", ValidAuthenticationToken.Token);
        Assert.False(request.Headers.Contains("If-Match"));
        AssertJsonMatches(response);
    }

    [Fact]
    [Trait("Method", "UndeleteItemAsync")]
    public async Task UndeleteItemAsync_Auth_FormulatesCorrectResponse_WithPrecondition()
    {
        // Arrange
        MockHandler.AddResponse(HttpStatusCode.OK, payload);

        // Act
        var response = await authTable.UndeleteItemAsync(jIdEntity).ConfigureAwait(false);

        // Assert
        var request = AssertSingleRequest(ServiceRequest.PATCH, expectedEndpoint);
        Assert.Equal("{\"deleted\":false}", await request.Content.ReadAsStringAsync());
        AssertEx.HasHeader(request.Headers, "X-ZUMO-AUTH", ValidAuthenticationToken.Token);
        AssertEx.HasHeader(request.Headers, "If-Match", "\"etag\"");
        AssertJsonMatches(response);
    }

    [Fact]
    [Trait("Method", "UndeleteItemAsync")]
    public async Task UndeleteItemAsync_SuccessNoContent()
    {
        // Arrange
        MockHandler.AddResponse(HttpStatusCode.OK);

        // Act
        await Assert.ThrowsAsync<DatasyncInvalidOperationException>(() => table.UndeleteItemAsync(jIdEntity)).ConfigureAwait(false);
    }

    [Theory]
    [InlineData(HttpStatusCode.Conflict)]
    [InlineData(HttpStatusCode.PreconditionFailed)]
    [Trait("Method", "UndeleteItemAsync")]
    public async Task UndeleteItemAsync_Conflict_FormulatesCorrectResponse(HttpStatusCode statusCode)
    {
        // Arrange
        MockHandler.AddResponse(statusCode, payload);

        // Act
        var ex = await Assert.ThrowsAsync<DatasyncConflictException>(() => table.UndeleteItemAsync(jIdEntity)).ConfigureAwait(false);

        // Assert
        Assert.NotNull(ex.Request);
        Assert.NotNull(ex.Response);
        AssertJsonMatches(ex.Value);
    }

    [Theory]
    [InlineData(HttpStatusCode.Conflict)]
    [InlineData(HttpStatusCode.PreconditionFailed)]
    [Trait("Method", "UndeleteItemAsync")]
    public async Task UndeleteItemAsync_ConflictNoContent(HttpStatusCode statusCode)
    {
        // Arrange
        MockHandler.AddResponse(statusCode);

        // Act
        var ex = await Assert.ThrowsAsync<DatasyncInvalidOperationException>(() => table.UndeleteItemAsync(jIdEntity)).ConfigureAwait(false);

        // Assert
        Assert.NotNull(ex.Request);
        Assert.NotNull(ex.Response);
        Assert.Null(ex.Value);
    }

    [Theory]
    [InlineData(HttpStatusCode.Conflict)]
    [InlineData(HttpStatusCode.PreconditionFailed)]
    [Trait("Method", "UndeleteItemAsync")]
    public async Task UndeleteItemAsync_ConflictWithBadJson_Throws(HttpStatusCode statusCode)
    {
        // Arrange
        ReturnBadJson(statusCode);

        // Act
        await Assert.ThrowsAnyAsync<JsonException>(() => table.UndeleteItemAsync(jIdEntity)).ConfigureAwait(false);
    }

    [Theory]
    [InlineData(HttpStatusCode.BadRequest)]
    [InlineData(HttpStatusCode.MethodNotAllowed)]
    [InlineData(HttpStatusCode.Unauthorized)]
    [InlineData(HttpStatusCode.InternalServerError)]
    [Trait("Method", "UndeleteItemAsync")]
    public async Task UndeleteItemAsync_RequestFailed_Throws(HttpStatusCode statusCode)
    {
        // Arrange
        MockHandler.AddResponse(statusCode);

        // Act
        var ex = await Assert.ThrowsAsync<DatasyncInvalidOperationException>(() => table.UndeleteItemAsync(jIdEntity)).ConfigureAwait(false);

        // Assert
        Assert.NotNull(ex.Request);
        Assert.Equal(statusCode, ex.Response?.StatusCode);
    }
}
