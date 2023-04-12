// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Test;
using Microsoft.Datasync.Client.Test.Helpers;
using Newtonsoft.Json;

namespace Microsoft.Datasync.Client.Test.Table.Operations.RemoteTableOfT;

[ExcludeFromCodeCoverage]
public class InsertItemAsync_Tests : BaseOperationTest
{
    private readonly IdEntity original, sut;

    public InsertItemAsync_Tests() : base()
    {
        sut = new() { Id = sId };
        original = new() { Id = sId };
    }

    [Fact]
    [Trait("Method", "InsertItemAsync")]
    public async Task InsertItemAsync_ThrowsOnNull()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() => table.InsertItemAsync(null)).ConfigureAwait(false);
    }

    [Theory]
    [MemberData(nameof(BaseOperationTest.GetInvalidIds), MemberType = typeof(BaseOperationTest))]
    [Trait("Method", "InsertItemAsync")]
    public async Task InsertItemAsync_ThrowsOnInvalidId(string id)
    {
        // Arrange
        var obj = new IdEntity { Id = id };

        // Act
        await Assert.ThrowsAsync<ArgumentException>(() => table.InsertItemAsync(obj)).ConfigureAwait(false);
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
        await table.InsertItemAsync(sut).ConfigureAwait(false);

        // Assert
        var request = AssertSingleRequest(HttpMethod.Post, tableEndpoint);
        await AssertRequestContentMatchesAsync(request, original);
        Assert.Equal(payload, sut);
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
        await authTable.InsertItemAsync(sut).ConfigureAwait(false);

        // Assert
        var request = AssertSingleRequest(HttpMethod.Post, tableEndpoint);
        await AssertRequestContentMatchesAsync(request, original);
        AssertEx.HasHeader(request.Headers, "X-ZUMO-AUTH", ValidAuthenticationToken.Token);
        Assert.Equal(payload, sut);
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
        await Assert.ThrowsAsync<DatasyncInvalidOperationException>(() => table.InsertItemAsync(sut)).ConfigureAwait(false);
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
        await Assert.ThrowsAnyAsync<JsonException>(() => table.InsertItemAsync(payload)).ConfigureAwait(false);
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
        var ex = await Assert.ThrowsAsync<DatasyncConflictException<IdEntity>>(() => table.InsertItemAsync(sut)).ConfigureAwait(false);

        // Assert
        var request = AssertSingleRequest(HttpMethod.Post, tableEndpoint);
        await AssertRequestContentMatchesAsync(request, original);
        Assert.Equal(payload, ex.Item);
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
        var ex = await Assert.ThrowsAsync<DatasyncInvalidOperationException>(() => table.InsertItemAsync(sut)).ConfigureAwait(false);
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
        await Assert.ThrowsAnyAsync<JsonException>(() => table.InsertItemAsync(sut)).ConfigureAwait(false);
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
        var ex = await Assert.ThrowsAsync<DatasyncInvalidOperationException>(() => table.InsertItemAsync(sut)).ConfigureAwait(false);

        // Assert
        Assert.Equal(statusCode, ex.Response?.StatusCode);
    }
}
