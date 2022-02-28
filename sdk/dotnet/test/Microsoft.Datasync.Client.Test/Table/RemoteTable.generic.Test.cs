// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Test;
using Datasync.Common.Test.Models;
using Microsoft.Datasync.Client.Table;
using System;
using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace Microsoft.Datasync.Client.Test.Table
{
    [ExcludeFromCodeCoverage]
    public class RemoteTable_generic_Tests : BaseTest
    {
        [Fact]
        [Trait("Method", "Ctor")]
        public void Ctor_SetsInternals()
        {
            var client = GetMockClient();
            var sut = new RemoteTable<ClientMovie>("movies", client);

            Assert.Equal("movies", sut.TableName);
            Assert.Same(client, sut.ServiceClient);
        }

        [Fact]
        [Trait("Method", "Ctor")]
        public void Ctor_NullTableName_Throws()
        {
            var client = GetMockClient();
            Assert.Throws<ArgumentNullException>(() => new RemoteTable<ClientMovie>(null, client));
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
        [Trait("Method", "IsValidTableName")]
        public void Ctor_InvalidTableName_Throws(string sut)
        {
            var client = GetMockClient();
            Assert.Throws<ArgumentException>(() => new RemoteTable<ClientMovie>(sut, client));
        }

        [Fact]
        [Trait("Method", "Ctor")]
        public void Ctor_NullClient_Throws()
        {
            var client = GetMockClient();
            Assert.Throws<ArgumentNullException>(() => new RemoteTable<ClientMovie>("movies", null));
        }

        [Fact]
        [Trait("Method", "CreateQuery")]
        public void CreateQuery_ProducesQuery()
        {
            var client = GetMockClient();
            var table = client.GetRemoteTable<ClientMovie>();
            var query = table.CreateQuery();

            Assert.NotNull(query);
            Assert.Same(table, query.RemoteTable);
        }
    }
}
