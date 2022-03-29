// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Test;
using Datasync.Common.Test.Mocks;
using Datasync.Common.Test.Models;
using Microsoft.Datasync.Client.Table;
using System;
using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace Microsoft.Datasync.Client.Test.Table
{
    [ExcludeFromCodeCoverage]
    public class OfflineTable_generic_Tests : BaseTest
    {
        [Fact]
        public void Ctor_Throws_OnNullTableName()
        {
            var client = GetMockClient();
            Assert.Throws<ArgumentNullException>(() => new OfflineTable<ClientMovie>(null, client));
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("\t")]
        [InlineData("abcdef gh")]
        [InlineData("!!!")]
        [InlineData("?")]
        [InlineData(";")]
        [InlineData("{EA235ADF-9F38-44EA-8DA4-EF3D24755767}")]
        [InlineData("###")]
        [InlineData("1abcd")]
        [InlineData("true.false")]
        [InlineData("a-b-c-d")]
        public void Ctor_Throws_OnInvalidTable(string tableName)
        {
            var client = GetMockClient();
            Assert.Throws<ArgumentException>(() => new OfflineTable<ClientMovie>(tableName, client));
        }

        [Fact]
        public void Ctor_Throws_OnNullClient()
        {
            Assert.Throws<ArgumentNullException>(() => new OfflineTable<ClientMovie>("movies", null));
        }

        [Fact]
        public void Ctor_Throws_WhenNoOfflineStore()
        {
            var client = GetMockClient();
            Assert.Throws<InvalidOperationException>(() => new OfflineTable<ClientMovie>("movies", client));
        }

        [Fact]
        public void Ctor_CreateTable_WhenArgsCorrect()
        {
            var store = new MockOfflineStore();
            var options = new DatasyncClientOptions { OfflineStore = store };
            var client = new DatasyncClient(Endpoint, options);
            var table = new OfflineTable<ClientMovie>("movies", client);

            Assert.Same(client, table.ServiceClient);
            Assert.Equal("movies", table.TableName);
        }
    }
}
