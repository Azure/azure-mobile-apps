// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Test;
using Microsoft.Datasync.Client.Http;
using System.Text;

namespace Microsoft.Datasync.Client.Test.Http;

[ExcludeFromCodeCoverage]
public class ServiceResponse_Tests : BaseTest
{
    [Fact]
    [Trait("Method", "ServiceResponse.FromResponseAsync")]
    public async Task CreateResponseAsync_Null_Throws()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() => ServiceResponse.CreateResponseAsync(null)).ConfigureAwait(false);
    }

    [Theory]
    [InlineData(HttpStatusCode.Continue, false, false, false)]
    [InlineData(HttpStatusCode.OK, false, true, false)]
    [InlineData(HttpStatusCode.Created, false, true, false)]
    [InlineData(HttpStatusCode.NoContent, false, true, false)]
    [InlineData(HttpStatusCode.BadRequest, false, false, false)]
    [InlineData(HttpStatusCode.Unauthorized, false, false, false)]
    [InlineData(HttpStatusCode.Forbidden, false, false, false)]
    [InlineData(HttpStatusCode.MethodNotAllowed, false, false, false)]
    [InlineData(HttpStatusCode.Conflict, false, false, true)]
    [InlineData(HttpStatusCode.PreconditionFailed, false, false, true)]
    [InlineData(HttpStatusCode.InternalServerError, false, false, false)]
    [InlineData(HttpStatusCode.Continue, true, false, false)]
    [InlineData(HttpStatusCode.OK, true, true, false)]
    [InlineData(HttpStatusCode.Created, true, true, false)]
    [InlineData(HttpStatusCode.NoContent, true, true, false)]
    [InlineData(HttpStatusCode.BadRequest, true, false, false)]
    [InlineData(HttpStatusCode.Unauthorized, true, false, false)]
    [InlineData(HttpStatusCode.Forbidden, true, false, false)]
    [InlineData(HttpStatusCode.MethodNotAllowed, true, false, false)]
    [InlineData(HttpStatusCode.Conflict, true, false, true)]
    [InlineData(HttpStatusCode.PreconditionFailed, true, false, true)]
    [InlineData(HttpStatusCode.InternalServerError, true, false, false)]
    [Trait("Method", "ServiceResponse.FromResponseAsync")]
    public async Task CreateResponseAsync_ContentNoHeaders(HttpStatusCode statusCode, bool hasContent, bool isSuccessful, bool isConflict)
    {
        // Arrange
        const string jsonPayload = "{\"stringValue\":\"test\"}";
        var sut = new HttpResponseMessage(statusCode);
        if (hasContent)
        {
            sut.Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
        }

        // Act
        var response = await ServiceResponse.CreateResponseAsync(sut).ConfigureAwait(false);

        // Assert
        Assert.Equal(hasContent, response.HasContent);
        if (hasContent)
        {
            Assert.Equal(jsonPayload, response.Content);
            AssertEx.Contains("Content-Type", "application/json; charset=utf-8", response.Headers);
        }
        else
        {
            Assert.Empty(response.Content);
        }
        Assert.Equal(isConflict, response.IsConflictStatusCode);
        Assert.Equal(isSuccessful, response.IsSuccessStatusCode);
        Assert.NotNull(response.ReasonPhrase);
        Assert.Equal((int)statusCode, response.StatusCode);
        Assert.NotNull(response.Version);
    }

    [Theory]
    [InlineData(HttpStatusCode.Continue, false, false, false)]
    [InlineData(HttpStatusCode.OK, false, true, false)]
    [InlineData(HttpStatusCode.Created, false, true, false)]
    [InlineData(HttpStatusCode.NoContent, false, true, false)]
    [InlineData(HttpStatusCode.BadRequest, false, false, false)]
    [InlineData(HttpStatusCode.Unauthorized, false, false, false)]
    [InlineData(HttpStatusCode.Forbidden, false, false, false)]
    [InlineData(HttpStatusCode.MethodNotAllowed, false, false, false)]
    [InlineData(HttpStatusCode.Conflict, false, false, true)]
    [InlineData(HttpStatusCode.PreconditionFailed, false, false, true)]
    [InlineData(HttpStatusCode.InternalServerError, false, false, false)]
    [InlineData(HttpStatusCode.Continue, true, false, false)]
    [InlineData(HttpStatusCode.OK, true, true, false)]
    [InlineData(HttpStatusCode.Created, true, true, false)]
    [InlineData(HttpStatusCode.NoContent, true, true, false)]
    [InlineData(HttpStatusCode.BadRequest, true, false, false)]
    [InlineData(HttpStatusCode.Unauthorized, true, false, false)]
    [InlineData(HttpStatusCode.Forbidden, true, false, false)]
    [InlineData(HttpStatusCode.MethodNotAllowed, true, false, false)]
    [InlineData(HttpStatusCode.Conflict, true, false, true)]
    [InlineData(HttpStatusCode.PreconditionFailed, true, false, true)]
    [InlineData(HttpStatusCode.InternalServerError, true, false, false)]
    [Trait("Method", "ServiceResponse.FromResponseAsync")]
    public async Task CreateResponseAsync_ContentWithHeaders(HttpStatusCode statusCode, bool hasContent, bool isSuccessful, bool isConflict)
    {
        // Arrange
        const string jsonPayload = "{\"stringValue\":\"test\"}";
        var sut = new HttpResponseMessage(statusCode);
        if (hasContent)
        {
            sut.Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
        }
        sut.Headers.Add("ETag", "\"42\"");
        sut.Headers.Add("ZUMO-API-VERSION", "3.0.0");

        // Act
        var response = await ServiceResponse.CreateResponseAsync(sut).ConfigureAwait(false);

        // Assert
        Assert.Equal(hasContent, response.HasContent);
        if (hasContent)
        {
            Assert.Equal(jsonPayload, response.Content);
            AssertEx.Contains("Content-Type", "application/json; charset=utf-8", response.Headers);
        }
        else
        {
            Assert.Empty(response.Content);
        }
        Assert.Equal(isConflict, response.IsConflictStatusCode);
        Assert.Equal(isSuccessful, response.IsSuccessStatusCode);
        Assert.NotNull(response.ReasonPhrase);
        Assert.Equal((int)statusCode, response.StatusCode);
        Assert.NotNull(response.Version);
        AssertEx.Contains("ETag", "\"42\"", response.Headers);
        AssertEx.Contains("ZUMO-API-VERSION", "3.0.0", response.Headers);
    }
}
