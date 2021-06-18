// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Http;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Datasync.Client.Test.Http
{
    [ExcludeFromCodeCoverage(Justification = "Test suite")]
    public class RequestFailedException_Tests
    {
        [Fact]
        [Trait("Method", "Ctor")]
        public void Ctor_Null_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new RequestFailedException(null));
        }

        [Theory]
        [InlineData(HttpStatusCode.BadRequest)]
        [InlineData(HttpStatusCode.Unauthorized)]
        [InlineData(HttpStatusCode.MethodNotAllowed)]
        [InlineData(HttpStatusCode.InternalServerError)]
        [Trait("Method", "Ctor")]
        public async Task Ctor_Stores_Response(HttpStatusCode statusCode)
        {
            var response = await HttpResponse.FromResponseAsync(new HttpResponseMessage(statusCode)).ConfigureAwait(false);

            var exception = new RequestFailedException(response);

            Assert.Same(response, exception.Response);
            Assert.Equal((int)statusCode, exception.StatusCode);
        }
    }

    [ExcludeFromCodeCoverage(Justification = "Test suite")]
    public class NotModifiedException_Tests
    {
        [Fact]
        [Trait("Method", "Ctor")]
        public void NotModifiedCtor_Null_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new NotModifiedException(null));
        }

        [Fact]
        [Trait("Method", "Ctor")]
        public async Task NotModifiedCtor_Stores_Response()
        {
            var response = await HttpResponse.FromResponseAsync(new HttpResponseMessage(HttpStatusCode.NotModified)).ConfigureAwait(false);

            var exception = new NotModifiedException(response);

            Assert.Same(response, exception.Response);
            Assert.Equal(304, exception.StatusCode);
        }
    }
}
