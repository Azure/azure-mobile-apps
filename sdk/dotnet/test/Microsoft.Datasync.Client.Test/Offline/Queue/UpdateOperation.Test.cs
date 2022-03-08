// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Exceptions;
using Microsoft.Datasync.Client.Offline.Queue;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace Microsoft.Datasync.Client.Test.Offline.Queue
{
    [ExcludeFromCodeCoverage]
    public class UpdateOperation_Tests : BaseOperationTest
    {
        private readonly JObject serializedObject = JObject.Parse("{\"kind\":3,\"itemId\":\"1234\",\"item\":null, \"sequence\":0, \"tableName\":\"test\", \"version\":1}");
        private readonly JObject testObject = JObject.Parse("{\"id\":\"1234\"}");

        [Fact]
        public void Ctor_NullTable_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new UpdateOperation(null, "1234"));
        }

        [Fact]
        public void Ctor_NullItemId_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new UpdateOperation("test", null));
        }

        [Theory]
        [MemberData(nameof(BaseOperationTest.GetInvalidTableNames), MemberType = typeof(BaseOperationTest))]
        public void Ctor_InvalidTable_Throws(string tableName)
        {
            Assert.Throws<ArgumentException>(() => new UpdateOperation(tableName, "1234"));
        }

        [Theory]
        [MemberData(nameof(BaseOperationTest.GetInvalidIds), MemberType = typeof(BaseOperationTest))]
        public void Ctor_InvalidId_Throws(string itemId)
        {
            Assert.Throws<ArgumentException>(() => new UpdateOperation("test", itemId));
        }

        [Fact]
        public void Ctor_SetsContext()
        {
            var operation = new UpdateOperation("test", "1234");

            // Note - we check the int value for compatibility at the storage level.
            Assert.Equal(3, (int)operation.Kind);
            Assert.NotEmpty(operation.Id);
            Assert.Equal("1234", operation.ItemId);
            Assert.Equal("test", operation.TableName);
            Assert.Null(operation.Item);
            Assert.Equal(0, operation.Sequence);
            // Note - we check the int value for compatibility at the storage level.
            Assert.Equal(0, (int)operation.State);
            Assert.Equal(1, operation.Version);
        }

        [Fact]
        public void Operation_CanAbort()
        {
            var operation = new UpdateOperation("test", "1234");
            Assert.Throws<PushAbortedException>(() => operation.AbortPush());
        }

        [Fact]
        public void Operation_CanBeCancelled()
        {
            var operation = new UpdateOperation("test", "1234");
            Assert.False(operation.IsCancelled);
            operation.Cancel();
            Assert.True(operation.IsCancelled);
        }

        [Fact]
        public void Operation_CanBeUpdated()
        {
            var operation = new UpdateOperation("test", "1234");
            Assert.False(operation.IsUpdated);
            operation.Update();
            Assert.Equal(2, operation.Version);
            Assert.True(operation.IsUpdated);
        }

        [Fact]
        public void Operation_CanSetItem()
        {
            var operation = new UpdateOperation("test", "1234") { Item = testObject };
            Assert.Equal(operation.Item, testObject);
        }

        [Fact]
        public void Serialize_Works_WithItem()
        {
            var operation = new UpdateOperation("test", "1234") { Item = testObject };

            JObject actual = operation.Serialize();
            actual.Remove("id");

            Assert.Equal(serializedObject, actual);
        }

        [Fact]
        public void Serialize_Works_WithoutItem()
        {
            var operation = new UpdateOperation("test", "1234");

            JObject actual = operation.Serialize();
            actual.Remove("id");

            Assert.Equal(serializedObject, actual);
        }

        [Fact]
        public void Deserialize_Null_ReturnsNull()
        {
            var operation = TableOperation.Deserialize(null);
            Assert.Null(operation);
        }

        [Fact]
        public void Deserialize_Works_WithItem()
        {
            var expected = new UpdateOperation("test", "1234") { Item = testObject };
            var operation = TableOperation.Deserialize(serializedObject);
            Assert.IsAssignableFrom<UpdateOperation>(operation);
            Assert.Equal(expected, operation);
        }

        [Fact]
        public void Deserialize_Works_WithoutItem()
        {
            var expected = new UpdateOperation("test", "1234");
            var operation = TableOperation.Deserialize(serializedObject);
            Assert.IsAssignableFrom<UpdateOperation>(operation);
            Assert.Equal(expected, operation);
        }
    }
}