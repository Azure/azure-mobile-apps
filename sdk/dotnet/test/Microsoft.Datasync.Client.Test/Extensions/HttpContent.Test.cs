// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Datasync.Client.Test.Extensions
{
    [ExcludeFromCodeCoverage(Justification = "Test suite")]
    public class HttpContent_Tests
    {
        [Fact]
        public async Task ReadAsByteArrayAsync_ReadsZeroLength()
        {
            // Arrange
            var token = CancellationToken.None;
            var message = new HttpResponseMessage(HttpStatusCode.OK);

            // Act
            var result = await message.Content.ReadAsByteArrayAsync(token).ConfigureAwait(false);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task ReadAsByteArrayAsync_ReadsByteData()
        {
            // Arrange
            var token = CancellationToken.None;
            var message = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"stringValue\":\"test\"}")
            };

            // Act
            var result = await message.Content.ReadAsByteArrayAsync(token).ConfigureAwait(false);
            var str = Encoding.UTF8.GetString(result);

            // Assert
            Assert.Equal("{\"stringValue\":\"test\"}", str);
        }
    }
}
