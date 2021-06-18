// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Http;
using Microsoft.Datasync.Client.Test.Helpers;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Datasync.Client.Test.Http
{
    [ExcludeFromCodeCoverage(Justification = "Test suite")]
    public class ConflictException_Tests : BaseTest
    {
        [Fact]
        [Trait("Method", "Ctor")]
        public void Ctor_Null_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new ConflictException<MockObject>(null));
        }

        [Fact]
        [Trait("Method", "Ctor")]
        public async Task Ctor_StoresResponse()
        {
            var message = new HttpResponseMessage(HttpStatusCode.Conflict)
            {
                Content = new StringContent("{\"stringValue\":\"test\"}", Encoding.UTF8, "application/json")
            };
            var response = await HttpResponse.FromResponseAsync<MockObject>(message, DeserializerOptions).ConfigureAwait(false);
            var exception = new ConflictException<MockObject>(response);

            Assert.Same(response, exception.Response);
            Assert.NotNull(exception.ServerItem);
            Assert.Equal("test", exception.ServerItem.StringValue);
            Assert.Equal(409, exception.StatusCode);
        }
    }
}
