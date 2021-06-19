// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Internal;
using Microsoft.Datasync.Client.Test.Helpers;
using System;
using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace Microsoft.Datasync.Client.Test.Internal
{
    [ExcludeFromCodeCoverage(Justification = "Test suite")]
    public class Utils_Test
    {
        #region GetIdFromItem
        [Fact]
        [Trait("Method", "GetIdFromItem")]
        public void GetIdFromItem_Null_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => Utils.GetIdFromItem(null));
        }

        [Fact]
        [Trait("Method", "GetIdFromItem")]
        public void GetIdFromItem_IdEntity_ReturnsId()
        {
            IdEntity entity = new() { Id = "test" };
            var id = Utils.GetIdFromItem(entity);
            Assert.Equal("test", id);
        }

        [Fact]
        [Trait("Method", "GetIdFromItem")]
        public void GetIdFromItem_IdEntity_Null_Throws()
        {
            IdEntity entity = new() { Id = null };
            Assert.Throws<ArgumentNullException>(() => Utils.GetIdFromItem(entity));
        }

        [Fact]
        [Trait("Method", "GetIdFromItem")]
        public void GetIdFromItem_KeyAttribute_ReturnsId()
        {
            KeyEntity entity = new() { KeyId = "test" };
            var id = Utils.GetIdFromItem(entity);
            Assert.Equal("test", id);
        }

        [Fact]
        [Trait("Method", "GetIdFromItem")]
        public void GetIdFromItem_KeyAttribute_Null_Throws()
        {
            KeyEntity entity = new() { KeyId = null };
            Assert.Throws<ArgumentNullException>(() => Utils.GetIdFromItem(entity));
        }

        [Fact]
        [Trait("Method", "GetIdFromItem")]
        public void GetIdFromItem_NoId_Throws()
        {
            NoIdEntity entity = new() { Test = "test" };
            Assert.Throws<MissingMemberException>(() => Utils.GetIdFromItem(entity));
        }

        [Fact]
        [Trait("Method", "GetIdFromItem")]
        public void GetIdFromItem_NonStringId_Throws()
        {
            NonStringIdEntity entity = new() { Id = true };
            Assert.Throws<MemberAccessException>(() => Utils.GetIdFromItem(entity));
        }
        #endregion

        #region GetVersionFromItem
        [Fact]
        [Trait("Method", "GetVersionFromItem")]
        public void GetVersionFromItem_Null_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => Utils.GetVersionFromItem(null));
        }

        [Fact]
        [Trait("Method", "GetVersionFromItem")]
        public void GetVersionFromItem_IdEntity_ReturnsValue()
        {
            IdEntity entity = new() { Version = "test" };
            var id = Utils.GetVersionFromItem(entity);
            Assert.Equal("test", id);
        }

        [Fact]
        [Trait("Method", "GetVersionFromItem")]
        public void GetVersionFromItem_IdEntity_Null_Throws()
        {
            IdEntity entity = new() { Version = null };
            Assert.Throws<ArgumentNullException>(() => Utils.GetVersionFromItem(entity));
        }

        [Fact]
        [Trait("Method", "GetVersionFromItem")]
        public void GetVersionFromItem_Attribute_ReturnsValue()
        {
            KeyEntity entity = new() { KeyVersion = "test" };
            var id = Utils.GetVersionFromItem(entity);
            Assert.Equal("test", id);
        }

        [Fact]
        [Trait("Method", "GetVersionFromItem")]
        public void GetVersionFromItem_Attribute_Null_Throws()
        {
            KeyEntity entity = new() { KeyVersion = null };
            Assert.Throws<ArgumentNullException>(() => Utils.GetVersionFromItem(entity));
        }

        [Fact]
        [Trait("Method", "GetVersionFromItem")]
        public void GetVersionFromItem_NoId_Throws()
        {
            NoIdEntity entity = new() { Test = "test" };
            Assert.Throws<MissingMemberException>(() => Utils.GetVersionFromItem(entity));
        }

        [Fact]
        [Trait("Method", "GetVersionFromItem")]
        public void GetVersionFromItem_NonStringId_Throws()
        {
            NonStringIdEntity entity = new() { Version = true };
            Assert.Throws<MemberAccessException>(() => Utils.GetVersionFromItem(entity));
        }
        #endregion
    }
}
