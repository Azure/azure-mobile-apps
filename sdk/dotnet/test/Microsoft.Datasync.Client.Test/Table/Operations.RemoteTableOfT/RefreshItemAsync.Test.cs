// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Test;
using Microsoft.Datasync.Client.Test.Helpers;

namespace Microsoft.Datasync.Client.Test.Table.Operations.RemoteTableOfT;

[ExcludeFromCodeCoverage]
public class RefreshItemAsync_Tests : BaseOperationTest
{
    [Fact]
    [Trait("Method", "RefreshItemAsync")]
    public async Task RefreshItemAsync_ThrowsOnNull()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() => table.RefreshItemAsync(null));
    }

    [Fact]
    [Trait("Method", "RefreshItemAsync")]
    public async Task RefreshItemAsync_ReturnsOnNullId()
    {
        var item = new IdEntity { Id = null };
        await table.RefreshItemAsync(item);
        Assert.Null(item.Id);
    }

    [Theory]
    [MemberData(nameof(BaseOperationTest.GetInvalidIds), MemberType = typeof(BaseOperationTest))]
    [Trait("Method", "RefreshItemAsync")]
    public async Task RefreshItemAsync_ThrowsOnInvalidId(string id)
    {
        var item = new IdEntity { Id = id };
        await Assert.ThrowsAsync<ArgumentException>(() => table.RefreshItemAsync(item));
    }

    [Fact]
    [Trait("Method", "RefreshItemAsync")]
    public async Task RefreshItemAsync_FormulatesCorrectRequest()
    {
        // Arrange
        MockHandler.AddResponse(HttpStatusCode.OK, payload);
        var item = new IdEntity { Id = sId };

        // Act
        await table.RefreshItemAsync(item);

        // Assert
        AssertSingleRequest(HttpMethod.Get, expectedEndpoint);
        Assert.Equal(payload.StringValue, item.StringValue);
    }

    [Fact]
    [Trait("Method", "RefreshItemAsync")]
    public async Task RefreshItemAsync_FormulatesCorrectRequest_WithAuth()
    {
        // Arrange
        MockHandler.AddResponse(HttpStatusCode.OK, payload);
        var item = new IdEntity { Id = sId };

        // Act
        await authTable.RefreshItemAsync(item);

        // Assert
        var request = AssertSingleRequest(HttpMethod.Get, expectedEndpoint);
        AssertEx.HasHeader(request.Headers, "X-ZUMO-AUTH", ValidAuthenticationToken.Token);
        Assert.Equal(payload.StringValue, item.StringValue);
    }

    [Fact]
    [Trait("Method", "RefreshItemAsync")]
    public async Task RefreshItemAsync_SuccessNoContent()
    {
        // Arrange
        MockHandler.AddResponse(HttpStatusCode.OK);
        var item = new IdEntity { Id = sId };

        // Act
        await Assert.ThrowsAsync<DatasyncInvalidOperationException>(() => table.RefreshItemAsync(item));
    }

    [Theory]
    [InlineData(HttpStatusCode.BadRequest)]
    [InlineData(HttpStatusCode.NotModified)]
    [InlineData(HttpStatusCode.MethodNotAllowed)]
    [InlineData(HttpStatusCode.Unauthorized)]
    [InlineData(HttpStatusCode.InternalServerError)]
    [InlineData(HttpStatusCode.Conflict)]
    [InlineData(HttpStatusCode.PreconditionFailed)]
    [Trait("Method", "RefreshItemAsync")]
    public async Task RefreshItemAsync_RequestFailed_Throws(HttpStatusCode statusCode)
    {
        // Arrange
        MockHandler.AddResponse(statusCode);
        var item = new IdEntity { Id = sId };

        // Act
        var ex = await Assert.ThrowsAsync<DatasyncInvalidOperationException>(() => table.RefreshItemAsync(item));

        // Assert
        Assert.Equal(statusCode, ex.Response?.StatusCode);
    }
}
