// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Test;
using Newtonsoft.Json;

namespace Microsoft.Datasync.Client.Test.Table.Operations.RemoteTable;

[ExcludeFromCodeCoverage]
public class GetItemAsync_Tests : BaseOperationTest
{
    [Fact]
    [Trait("Method", "GetItemAsync")]
    public async Task GetItemAsync_ThrowsOnNull()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() => table.GetItemAsync(null)).ConfigureAwait(false);
    }

    [Theory]
    [MemberData(nameof(BaseOperationTest.GetInvalidIds), MemberType = typeof(BaseOperationTest))]
    [Trait("Method", "GetItemAsync")]
    public async Task GetItemAsync_ThrowsOnInvalidId(string id)
    {
        await Assert.ThrowsAsync<ArgumentException>(() => table.GetItemAsync(id)).ConfigureAwait(false);
    }

    [Fact]
    [Trait("Method", "GetItemAsync")]
    public async Task GetItemAsync_FormulatesCorrectRequest()
    {
        // Arrange
        MockHandler.AddResponse(HttpStatusCode.OK, payload);

        // Act
        var response = await table.GetItemAsync(sId).ConfigureAwait(false);

        // Assert
        var request = AssertSingleRequest(HttpMethod.Get, expectedEndpoint);
        Assert.False(request.Headers.Contains("If-None-Match"));
        AssertJsonMatches(response);
    }

    [Fact]
    [Trait("Method", "GetItemAsync")]
    public async Task GetItemAsync_FormulatesCorrectRequest_IncludeDeleted()
    {
        // Arrange
        MockHandler.AddResponse(HttpStatusCode.OK, payload);

        // Act
        var response = await table.GetItemAsync(sId, true).ConfigureAwait(false);

        // Assert
        var request = AssertSingleRequest(HttpMethod.Get, expectedEndpoint + "?__includedeleted=true");
        Assert.False(request.Headers.Contains("If-None-Match"));
        AssertJsonMatches(response);
    }

    [Fact]
    [Trait("Method", "GetItemAsync")]
    public async Task GetItemAsync_FormulatesCorrectRequest_WithAuth()
    {
        // Arrange
        MockHandler.AddResponse(HttpStatusCode.OK, payload);

        // Act
        var response = await authTable.GetItemAsync(sId).ConfigureAwait(false);

        // Assert
        var request = AssertSingleRequest(HttpMethod.Get, expectedEndpoint);
        AssertEx.HasHeader(request.Headers, "X-ZUMO-AUTH", ValidAuthenticationToken.Token);
        Assert.False(request.Headers.Contains("If-None-Match"));
        AssertJsonMatches(response);
    }

    [Fact]
    [Trait("Method", "GetItemAsync")]
    public async Task GetItemAsync_SuccessNoContent()
    {
        // Arrange
        MockHandler.AddResponse(HttpStatusCode.OK);

        // Act
        await Assert.ThrowsAsync<DatasyncInvalidOperationException>(() => table.GetItemAsync(sId)).ConfigureAwait(false);
    }

    [Theory]
    [InlineData(HttpStatusCode.BadRequest)]
    [InlineData(HttpStatusCode.MethodNotAllowed)]
    [InlineData(HttpStatusCode.Unauthorized)]
    [InlineData(HttpStatusCode.InternalServerError)]
    [Trait("Method", "GetItemAsync")]
    public async Task GetItemAsync_RequestFailed_Throws(HttpStatusCode statusCode)
    {
        // Arrange
        MockHandler.AddResponse(statusCode);

        // Act
        var ex = await Assert.ThrowsAsync<DatasyncInvalidOperationException>(() => table.GetItemAsync(sId)).ConfigureAwait(false);

        // Assert
        Assert.Equal(statusCode, ex.Response?.StatusCode);
    }

    [Fact]
    [Trait("Method", "GetItemAsync")]
    public async Task GetItemAsync_Fails_OnBadJson()
    {
        // Arrange
        ReturnBadJson(HttpStatusCode.OK);

        // Act
        await Assert.ThrowsAnyAsync<JsonException>(() => table.GetItemAsync(sId)).ConfigureAwait(false);
    }
}
