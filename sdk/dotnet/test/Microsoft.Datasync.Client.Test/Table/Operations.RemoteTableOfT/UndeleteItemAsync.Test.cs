// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Test;
using Microsoft.Datasync.Client.Http;
using Microsoft.Datasync.Client.Test.Helpers;
using Newtonsoft.Json;

namespace Microsoft.Datasync.Client.Test.Table.Operations.RemoteTableOfT;

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
        // Arrange
       var obj = new IdEntity { Id = null };

        // Act
        await Assert.ThrowsAsync<ArgumentException>(() => table.UndeleteItemAsync(obj)).ConfigureAwait(false);
    }

    [Theory]
    [MemberData(nameof(BaseOperationTest.GetInvalidIds), MemberType = typeof(BaseOperationTest))]
    [Trait("Method", "UndeleteItemAsync")]
    public async Task UndeleteItemAsync_ThrowsOnInvalidId(string id)
    {
        // Arrange
        var obj = new IdEntity { Id = id };

        // Act
        await Assert.ThrowsAsync<ArgumentException>(() => table.UndeleteItemAsync(obj)).ConfigureAwait(false);
    }

    [Fact]
    [Trait("Method", "UndeleteItemAsync")]
    public async Task UndeleteItemAsync_FormulatesCorrectResponse_NoPrecondition()
    {
        // Arrange
        MockHandler.AddResponse(HttpStatusCode.OK, payload);
        var obj = new IdEntity { Id = sId };

        // Act
        await table.UndeleteItemAsync(obj).ConfigureAwait(false);

        // Assert
        var request = AssertSingleRequest(ServiceRequest.PATCH, expectedEndpoint);
        Assert.Equal("{\"deleted\":false}", await request.Content.ReadAsStringAsync());
        Assert.False(request.Headers.Contains("If-Match"));
        Assert.Equal(payload, obj);
    }

    [Fact]
    [Trait("Method", "UndeleteItemAsync")]
    public async Task UndeleteItemAsync_FormulatesCorrectResponse_WithPrecondition()
    {
        // Arrange
        MockHandler.AddResponse(HttpStatusCode.OK, payload);
        var obj = new IdEntity { Id = sId, Version = "etag" };

        // Act
        await table.UndeleteItemAsync(obj).ConfigureAwait(false);

        // Assert
        var request = AssertSingleRequest(ServiceRequest.PATCH, expectedEndpoint);
        Assert.Equal("{\"deleted\":false}", await request.Content.ReadAsStringAsync());
        AssertEx.HasHeader(request.Headers, "If-Match", "\"etag\"");
        Assert.Equal(payload, obj);
    }

    [Fact]
    [Trait("Method", "UndeleteItemAsync")]
    public async Task UndeleteItemAsync_Auth_FormulatesCorrectResponse_NoPrecondition()
    {
        // Arrange
        MockHandler.AddResponse(HttpStatusCode.OK, payload);
        var obj = new IdEntity { Id = sId };

        // Act
        await authTable.UndeleteItemAsync(obj).ConfigureAwait(false);

        // Assert
        var request = AssertSingleRequest(ServiceRequest.PATCH, expectedEndpoint);
        Assert.Equal("{\"deleted\":false}", await request.Content.ReadAsStringAsync());
        AssertEx.HasHeader(request.Headers, "X-ZUMO-AUTH", ValidAuthenticationToken.Token);
        Assert.False(request.Headers.Contains("If-Match"));
        Assert.Equal(payload, obj);
    }

    [Fact]
    [Trait("Method", "UndeleteItemAsync")]
    public async Task UndeleteItemAsync_Auth_FormulatesCorrectResponse_WithPrecondition()
    {
        // Arrange
        MockHandler.AddResponse(HttpStatusCode.OK, payload);
        var obj = new IdEntity { Id = sId, Version = "etag" };

        // Act
        await authTable.UndeleteItemAsync(obj).ConfigureAwait(false);

        // Assert
        var request = AssertSingleRequest(ServiceRequest.PATCH, expectedEndpoint);
        Assert.Equal("{\"deleted\":false}", await request.Content.ReadAsStringAsync());
        AssertEx.HasHeader(request.Headers, "X-ZUMO-AUTH", ValidAuthenticationToken.Token);
        AssertEx.HasHeader(request.Headers, "If-Match", "\"etag\"");
        Assert.Equal(payload, obj);
    }

    [Fact]
    [Trait("Method", "UndeleteItemAsync")]
    public async Task UndeleteItemAsync_SuccessNoContent()
    {
        // Arrange
        MockHandler.AddResponse(HttpStatusCode.OK);
        var obj = new IdEntity { Id = sId };

        // Act
        await Assert.ThrowsAsync<DatasyncInvalidOperationException>(() => table.UndeleteItemAsync(obj)).ConfigureAwait(false);
    }

    [Theory]
    [InlineData(HttpStatusCode.Conflict)]
    [InlineData(HttpStatusCode.PreconditionFailed)]
    [Trait("Method", "UndeleteItemAsync")]
    public async Task UndeleteItemAsync_Conflict_FormulatesCorrectResponse(HttpStatusCode statusCode)
    {
        // Arrange
        MockHandler.AddResponse(statusCode, payload);
        var obj = new IdEntity { Id = sId };

        // Act
        var ex = await Assert.ThrowsAsync<DatasyncConflictException<IdEntity>>(() => table.UndeleteItemAsync(obj)).ConfigureAwait(false);

        // Assert
        Assert.Equal(payload, ex.Item);
    }

    [Theory]
    [InlineData(HttpStatusCode.Conflict)]
    [InlineData(HttpStatusCode.PreconditionFailed)]
    [Trait("Method", "UndeleteItemAsync")]
    public async Task UndeleteItemAsync_ConflictNoContent(HttpStatusCode statusCode)
    {
        // Arrange
        MockHandler.AddResponse(statusCode);
        var obj = new IdEntity { Id = sId };

        // Act
        await Assert.ThrowsAsync<DatasyncInvalidOperationException>(() => table.UndeleteItemAsync(obj)).ConfigureAwait(false);
    }

    [Theory]
    [InlineData(HttpStatusCode.Conflict)]
    [InlineData(HttpStatusCode.PreconditionFailed)]
    [Trait("Method", "UndeleteItemAsync")]
    public async Task UndeleteItemAsync_ConflictWithBadJson_Throws(HttpStatusCode statusCode)
    {
        // Arrange
        ReturnBadJson(statusCode);
        var obj = new IdEntity { Id = sId };

        // Act
        await Assert.ThrowsAnyAsync<JsonException>(() => table.UndeleteItemAsync(obj)).ConfigureAwait(false);
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
        var obj = new IdEntity { Id = sId };

        // Act
        var ex = await Assert.ThrowsAsync<DatasyncInvalidOperationException>(() => table.UndeleteItemAsync(obj)).ConfigureAwait(false);

        // Assert
        Assert.NotNull(ex.Request);
        Assert.Equal(statusCode, ex.Response?.StatusCode);
    }
}
