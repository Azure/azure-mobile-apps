// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Test;

namespace Microsoft.Datasync.Client.Test.Table.Operations.RemoteTableOfT;

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
        var actual = await table.GetItemAsync(sId).ConfigureAwait(false);

        // Assert
        AssertSingleRequest(HttpMethod.Get, expectedEndpoint);
        Assert.Equal(payload, actual);
    }

    [Fact]
    [Trait("Method", "GetItemAsync")]
    public async Task GetItemAsync_FormulatesCorrectRequest_IncludeDeleted()
    {
        // Arrange
        MockHandler.AddResponse(HttpStatusCode.OK, payload);

        // Act
        var actual = await table.GetItemAsync(sId, true).ConfigureAwait(false);

        // Assert
        AssertSingleRequest(HttpMethod.Get, expectedEndpoint + "?__includedeleted=true");
        Assert.Equal(payload, actual);
    }

    [Fact]
    [Trait("Method", "GetItemAsync")]
    public async Task GetItemAsync_FormulatesCorrectRequest_WithAuth()
    {
        // Arrange
        MockHandler.AddResponse(HttpStatusCode.OK, payload);

        // Act
        var actual = await authTable.GetItemAsync(sId).ConfigureAwait(false);

        // Assert
        var request = AssertSingleRequest(HttpMethod.Get, expectedEndpoint);
        AssertEx.HasHeader(request.Headers, "X-ZUMO-AUTH", ValidAuthenticationToken.Token);
        Assert.Equal(payload, actual);
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
    [InlineData(HttpStatusCode.NotModified)]
    [InlineData(HttpStatusCode.MethodNotAllowed)]
    [InlineData(HttpStatusCode.Unauthorized)]
    [InlineData(HttpStatusCode.InternalServerError)]
    [InlineData(HttpStatusCode.Conflict)]
    [InlineData(HttpStatusCode.PreconditionFailed)]
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
}
