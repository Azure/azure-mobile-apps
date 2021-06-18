// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Extensions;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using Xunit;

namespace Microsoft.Datasync.Client.Test.Extensions
{
    [ExcludeFromCodeCoverage(Justification = "Test suite")]
    public class HttpResponseMessage_Tests
    {
        [Theory]
        [InlineData(HttpStatusCode.Continue, false)]
        [InlineData(HttpStatusCode.OK, false)]
        [InlineData(HttpStatusCode.Created, false)]
        [InlineData(HttpStatusCode.NoContent, false)]
        [InlineData(HttpStatusCode.BadRequest, false)]
        [InlineData(HttpStatusCode.Unauthorized, false)]
        [InlineData(HttpStatusCode.Forbidden, false)]
        [InlineData(HttpStatusCode.MethodNotAllowed, false)]
        [InlineData(HttpStatusCode.Conflict, true)]
        [InlineData(HttpStatusCode.PreconditionFailed, true)]
        [InlineData(HttpStatusCode.InternalServerError, false)]
        [Trait("Method", "IsConflictStatusCode")]
        public void IsConflictStatusCode_Works(HttpStatusCode statusCode, bool expected)
        {
            var message = new HttpResponseMessage(statusCode);
            Assert.Equal(expected, message.IsConflictStatusCode());
        }
    }
}
