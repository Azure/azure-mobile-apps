// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Test;
using Datasync.Common.Test.Models;
using Microsoft.Datasync.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Datasync.Integration.Test.Client.OfflineTable
{
    [ExcludeFromCodeCoverage]
    public class GetItemAsync_Tests : BaseOperationTest
    {
        [Fact]
        [Trait("Method", "GetItemAsync")]
        public async Task GetItemAsync_Basic()
        {
            await InitializeAsync(true);

            // Arrange
            var id = GetRandomId();
            var expected = MovieServer.GetMovieById(id)!;

            // Act
            var response = await table!.GetItemAsync(id);

            // Assert
            Assert.NotNull(response);
            var movie = client.Serializer.Deserialize<ClientMovie>(response);
            Assert.Equal<IMovie>(expected, movie);
        }

        [Fact]
        [Trait("Method", "GetItemAsync")]
        public async Task GetItemAsync_NotFound()
        {
            await InitializeAsync(true);

            // Arrange
            const string id = "not-found";

            // Act
            var result = await table!.GetItemAsync(id);

            // Assert
            Assert.Null(result);
        }
    }
}
