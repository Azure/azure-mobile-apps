// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Test;
using Microsoft.Datasync.Client.Http;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Text;
using Xunit;

namespace Microsoft.Datasync.Client.Test.Http
{
    [ExcludeFromCodeCoverage]
    public class HttpMessageExtensions_Tests : BaseTest
    {
        [Theory]
        [InlineData(null, null, false)]
        [InlineData("None", null, false)]
        [InlineData("Accept-Encoding", null, true)]
        [InlineData(null, "foo", false)]
        [InlineData(null, "gzip", true)]
        [InlineData(null, "deflate", true)]
        [InlineData(null, "br", true)]
        [InlineData(null, "compress", true)]
        [InlineData("None", "foo", false)]
        [InlineData("None", "gzip", true)]
        [InlineData("None", "deflate", true)]
        [InlineData("None", "br", true)]
        [InlineData("None", "compress", true)]
        [InlineData("Accept-Encoding", "foo", false)]
        [InlineData("Accept-Encoding", "gzip", true)]
        [InlineData("Accept-Encoding", "deflate", true)]
        [InlineData("Accept-Encoding", "br", true)]
        [InlineData("Accept-Encoding", "compress", true)]
        public void IsCompressed_Works(string vary, string contentEncoding, bool expected)
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            if (vary != null)
            {
                response.Headers.Add("Vary", vary);
            }
            if (contentEncoding != null)
            {
                response.Content = new StringContent("{}", Encoding.UTF8, "application/json");
                response.Content.Headers.Add("Content-Encoding", contentEncoding);
            }

            var actual = response.IsCompressed();
            Assert.Equal(expected, actual);
        }
    }
}
