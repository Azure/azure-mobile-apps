// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Test;
using Microsoft.Datasync.Client.Test.Helpers;
using Newtonsoft.Json;

namespace Microsoft.Datasync.Client.Test.Table.Operations.RemoteTableOfT;

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
        // Arrange
        var obj = new IdEntity { Id = null };

        // Act
        await Assert.ThrowsAsync<ArgumentException>(() => table.ReplaceItemAsync(obj)).ConfigureAwait(false);
    }

    [Theory]
    [MemberData(nameof(BaseOperationTest.GetInvalidIds), MemberType = typeof(BaseOperationTest))]
    [Trait("Method", "ReplaceItemAsync")]
    public async Task ReplaceItemAsync_ThrowsOnInvalidId(string id)
    {
        // Arrange
        var obj = new IdEntity { Id = id };

        // Act
        await Assert.ThrowsAsync<ArgumentException>(() => table.ReplaceItemAsync(obj)).ConfigureAwait(false);
    }

    [Theory, CombinatorialData]
    [Trait("Method", "ReplaceItemAsync")]
    public async Task ReplaceItemAsync_Success_FormulatesCorrectResponse(bool hasPrecondition)
    {
        // Arrange
        MockHandler.AddResponse(HttpStatusCode.OK, payload);
        var obj = new IdEntity { Id = sId, Version = hasPrecondition ? "etag" : null };
        var original = new IdEntity { Id = sId, Version = hasPrecondition ? "etag" : null };

        // Act
        await table.ReplaceItemAsync(obj).ConfigureAwait(false);

        // Assert
        var request = AssertSingleRequest(HttpMethod.Put, expectedEndpoint);
        await AssertRequestContentMatchesAsync(request, original);
        if (hasPrecondition)
        {
            AssertEx.HasHeader(request.Headers, "If-Match", "\"etag\"");
        }
        else
        {
            Assert.False(request.Headers.Contains("If-Match"));
        }
        Assert.Equal(payload, obj);
    }

    [Theory, CombinatorialData]
    [Trait("Method", "ReplaceItemAsync")]
    public async Task ReplaceItemAsync_Success_FormulatesCorrectResponse_WithAuth(bool hasPrecondition)
    {
        // Arrange
        MockHandler.AddResponse(HttpStatusCode.OK, payload);
        var obj = new IdEntity { Id = sId, Version = hasPrecondition ? "etag" : null };
        var original = new IdEntity { Id = sId, Version = hasPrecondition ? "etag" : null };

        // Act
        await authTable.ReplaceItemAsync(obj).ConfigureAwait(false);

        // Assert
        var request = AssertSingleRequest(HttpMethod.Put, expectedEndpoint);
        await AssertRequestContentMatchesAsync(request, original);
        AssertEx.HasHeader(request.Headers, "X-ZUMO-AUTH", ValidAuthenticationToken.Token);
        if (hasPrecondition)
        {
            AssertEx.HasHeader(request.Headers, "If-Match", "\"etag\"");
        }
        else
        {
            Assert.False(request.Headers.Contains("If-Match"));
        }
        Assert.Equal(payload, obj);
    }

    [Fact]
    [Trait("Method", "ReplaceItemAsync")]
    public async Task ReplaceItemAsync_SuccessNoContent()
    {
        // Arrange
        MockHandler.AddResponse(HttpStatusCode.OK);
        var obj = new IdEntity { Id = sId };

        // Act
        await Assert.ThrowsAsync<DatasyncInvalidOperationException>(() => table.ReplaceItemAsync(obj)).ConfigureAwait(false);
    }

    [Theory]
    [InlineData(HttpStatusCode.Conflict)]
    [InlineData(HttpStatusCode.PreconditionFailed)]
    [Trait("Method", "ReplaceItemAsync")]
    public async Task ReplaceItemAsync_Conflict_FormulatesCorrectResponse(HttpStatusCode statusCode)
    {
        // Arrange
        MockHandler.AddResponse(statusCode, payload);
        var obj = new IdEntity { Id = sId };

        // Act
        var ex = await Assert.ThrowsAsync<DatasyncConflictException<IdEntity>>(() => table.ReplaceItemAsync(obj)).ConfigureAwait(false);

        // Assert
        Assert.Equal(payload, ex.Item);
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
        var ex = await Assert.ThrowsAsync<DatasyncInvalidOperationException>(() => table.ReplaceItemAsync(payload)).ConfigureAwait(false);
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
        await Assert.ThrowsAnyAsync<JsonException>(() => table.ReplaceItemAsync(payload)).ConfigureAwait(false);
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
        var ex = await Assert.ThrowsAsync<DatasyncInvalidOperationException>(() => table.ReplaceItemAsync(payload)).ConfigureAwait(false);

        // Assert
        Assert.Equal(statusCode, ex.Response?.StatusCode);
    }
}
