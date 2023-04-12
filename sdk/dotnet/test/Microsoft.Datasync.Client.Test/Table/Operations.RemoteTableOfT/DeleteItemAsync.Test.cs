// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Test;
using Microsoft.Datasync.Client.Test.Helpers;
using Newtonsoft.Json;

namespace Microsoft.Datasync.Client.Test.Table.Operations.RemoteTableOfT;

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
        var item = new IdEntity { Id = id };
        await Assert.ThrowsAsync<ArgumentException>(() => table.DeleteItemAsync(item)).ConfigureAwait(false);
    }

    [Theory, CombinatorialData]
    [Trait("Method", "DeleteItemAsync")]
    public async Task DeleteItemAsync_FormulatesCorrectResponse(bool hasPrecondition)
    {
        // Arrange
        MockHandler.AddResponse(HttpStatusCode.NoContent);
        var item = new IdEntity { Id = sId, Version = hasPrecondition ? "etag" : null };

        // Act
        await table.DeleteItemAsync(item).ConfigureAwait(false);

        // Assert
        var request = AssertSingleRequest(HttpMethod.Delete, expectedEndpoint);
        if (hasPrecondition)
        {
            AssertEx.HasHeader(request.Headers, "If-Match", "\"etag\"");
        }
        else
        {
            Assert.False(request.Headers.Contains("If-Match"));
        }
    }

    [Theory, CombinatorialData]
    [Trait("Method", "DeleteItemAsync")]
    public async Task DeleteItemAsync_FormulatesCorrectResponse_WithAuth(bool hasPrecondition)
    {
        // Arrange
        MockHandler.AddResponse(HttpStatusCode.NoContent);
        var item = new IdEntity { Id = sId, Version = hasPrecondition ? "etag" : null };

        // Act
        await authTable.DeleteItemAsync(item).ConfigureAwait(false);

        // Check Request
        var request = AssertSingleRequest(HttpMethod.Delete, expectedEndpoint);
        AssertEx.HasHeader(request.Headers, "X-ZUMO-AUTH", ValidAuthenticationToken.Token);
        if (hasPrecondition)
        {
            AssertEx.HasHeader(request.Headers, "If-Match", "\"etag\"");
        }
        else
        {
            Assert.False(request.Headers.Contains("If-Match"));
        }
    }

    [Theory]
    [InlineData(HttpStatusCode.Conflict)]
    [InlineData(HttpStatusCode.PreconditionFailed)]
    [Trait("Method", "DeleteItemAsync")]
    public async Task DeleteItemAsync_Conflict_FormulatesCorrectResponse(HttpStatusCode statusCode)
    {
        // Arrange
        MockHandler.AddResponse(statusCode, payload);
        var item = new IdEntity { Id = sId };

        // Act
        var ex = await Assert.ThrowsAsync<DatasyncConflictException<IdEntity>>(() => table.DeleteItemAsync(item)).ConfigureAwait(false);

        // Assert
        Assert.Equal(statusCode, ex.Response.StatusCode);
        Assert.Equal(payload.Id, ex.Item.Id);
    }

    [Theory]
    [InlineData(HttpStatusCode.Conflict)]
    [InlineData(HttpStatusCode.PreconditionFailed)]
    [Trait("Method", "DeleteItemAsync")]
    public async Task DeleteItemAsync_ConflictNoContent_FormulatesCorrectResponse(HttpStatusCode statusCode)
    {
        // Arrange
        MockHandler.AddResponse(statusCode);
        var item = new IdEntity { Id = sId };

        var ex = await Assert.ThrowsAsync<DatasyncInvalidOperationException>(() => table.DeleteItemAsync(item)).ConfigureAwait(false);

        // Check Response
        Assert.Equal(statusCode, ex.Response.StatusCode);
        Assert.Null(ex.Value);
    }

    [Theory]
    [InlineData(HttpStatusCode.Conflict)]
    [InlineData(HttpStatusCode.PreconditionFailed)]
    [Trait("Method", "DeleteItemAsync")]
    public async Task DeleteItemAsync_ConflictWithBadJson_Throws(HttpStatusCode statusCode)
    {
        // Arrange
        ReturnBadJson(statusCode);
        var item = new IdEntity { Id = sId };

        // Act
        await Assert.ThrowsAnyAsync<JsonException>(() => table.DeleteItemAsync(item)).ConfigureAwait(false);
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
        var item = new IdEntity { Id = sId };

        // Act
        var ex = await Assert.ThrowsAsync<DatasyncInvalidOperationException>(() => table.DeleteItemAsync(item)).ConfigureAwait(false);

        // Assert
        Assert.Equal(statusCode, ex.Response?.StatusCode);
    }
}
