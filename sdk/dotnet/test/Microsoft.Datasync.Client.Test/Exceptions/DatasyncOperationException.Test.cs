// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using Xunit;

namespace Microsoft.Datasync.Client.Test.Exceptions
{
    [ExcludeFromCodeCoverage]
    public class DatasyncOperationException_Test : OldBaseTest
    {
        [Fact]
        [Trait("Method", "Ctor")]
        public void Ctor_NullRequest_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new DatasyncOperationException(null, new HttpResponseMessage()));
        }

        [Fact]
        [Trait("Method", "Ctor")]
        public void Ctor_NullResponse_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new DatasyncOperationException(new HttpRequestMessage(), null));
        }

        [Fact]
        [Trait("Method", "Ctor")]
        public void Ctor_CanConstruct()
        {
            var request = new HttpRequestMessage(HttpMethod.Put, "");
            var response = new HttpResponseMessage(HttpStatusCode.Conflict);
            var exception = new DatasyncOperationException(request, response);

            Assert.IsAssignableFrom<Exception>(exception);
            Assert.Same(request, exception.Request);
            Assert.Same(response, exception.Response);
            Assert.Equal("Conflict", exception.Message);
        }
    }
}
