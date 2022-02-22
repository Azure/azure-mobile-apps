﻿// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Test;
using Microsoft.Datasync.Client.Serialization;
using Newtonsoft.Json;
using System;
using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace Microsoft.Datasync.Client.Test.Serialization
{
    [ExcludeFromCodeCoverage]
    public class DatasyncContractResolver_Tests : BaseTest
    {
        private DatasyncContractResolver contractResolver;

        public DatasyncContractResolver_Tests()
        {
            contractResolver = new DatasyncContractResolver();
        }

        [Fact]
        [Trait("Method", "CamelCasePropertyNames")]
        public void CamelCasePropertyNames_Roundtrips()
        {
            contractResolver.CamelCasePropertyNames = true;
            Assert.True(contractResolver.CamelCasePropertyNames);

            contractResolver.CamelCasePropertyNames = false;
            Assert.False(contractResolver.CamelCasePropertyNames);
        }

        [Theory]
        [InlineData(typeof(PocoType), "pocotype")]
        [InlineData(typeof(DataTableType), "nameddatatabletype")]
        [InlineData(typeof(JsonContainerType), "namedjsoncontainertype")]
        [InlineData(typeof(UnnamedJsonContainerType), "unnamedjsoncontainertype")]
        [Trait("Method", "ResolveTableName")]
        public void ResolveTableName_Tests(Type sut, string expected)
        {
            var actual = contractResolver.ResolveTableName(sut);
            Assert.Equal(expected, actual);
        }

        #region Test Models
        [DataTable("nameddatatabletype")]
        public class DataTableType
        {
        }

        public class PocoType
        {
        }

        [JsonObject(Title = "namedjsoncontainertype")]
        public class JsonContainerType
        {
        }

        [JsonObject]
        public class UnnamedJsonContainerType
        {
        }
        #endregion
    }
}
