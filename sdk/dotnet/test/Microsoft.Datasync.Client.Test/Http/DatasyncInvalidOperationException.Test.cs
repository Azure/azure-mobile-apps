// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Test;

namespace Microsoft.Datasync.Client.Test.Http;

[ExcludeFromCodeCoverage]
public class DatasyncInvalidOperationException_Tests : BaseTest
{
    [Fact]
    public void Ctor_StringInner_Works()
    {
        var innerException = new ApplicationException();
        var ex = new DatasyncInvalidOperationException("message", innerException);

        Assert.Equal("message", ex.Message);
        Assert.Same(innerException, ex.InnerException);
        Assert.Null(ex.Request);
        Assert.Null(ex.Response);
        Assert.False(ex.IsConflictStatusCode());
    }

    [Theory]
    [InlineData(HttpStatusCode.Conflict)]
    [InlineData(HttpStatusCode.PreconditionFailed)]
    public void IsConflictStatusCode_True_OnConflict(HttpStatusCode statusCode)
    {
        var req = new HttpRequestMessage(HttpMethod.Get, Endpoint);
        var resp = new HttpResponseMessage(statusCode);
        var ex = new DatasyncInvalidOperationException("message", req, resp);
        Assert.True(ex.IsConflictStatusCode());
    }

    [Theory]
    [InlineData(HttpStatusCode.OK)]
    [InlineData(HttpStatusCode.BadRequest)]
    public void IsConflictStatusCode_False_OnOthers(HttpStatusCode statusCode)
    {
        var req = new HttpRequestMessage(HttpMethod.Get, Endpoint);
        var resp = new HttpResponseMessage(statusCode);
        var ex = new DatasyncInvalidOperationException("message", req, resp);
        Assert.False(ex.IsConflictStatusCode());
    }
}
