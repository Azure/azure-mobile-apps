// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using Microsoft.WindowsAzure.MobileServices;
using Microsoft.WindowsAzure.MobileServices.Sync;
using Moq;
using Moq.Protected;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using Xunit;

namespace MobileClient.Tests.Table.Sync
{
    public class MobileServiceTableOperationTests
    {
        private Mock<MobileServiceTableOperation> operation;
        private Mock<MobileServiceTable> table;

        public MobileServiceTableOperationTests()
        {
            this.operation = new Mock<MobileServiceTableOperation>("test", MobileServiceTableKind.Table, "abc") { CallBase = true };
            var client = new Mock<MobileServiceClient>(MockBehavior.Strict);
            client.Object.Serializer = new MobileServiceSerializer();
            this.table = new Mock<MobileServiceTable>("test", client.Object);
            operation.Object.Table = this.table.Object;
        }

        [Fact]
        public async Task ExecuteAsync_Throws_WhenItemIsNull()
        {
            var ex = await Assert.ThrowsAsync<MobileServiceInvalidOperationException>(() => this.operation.Object.ExecuteAsync());
            Assert.Equal("Operation must have an item associated with it.", ex.Message);
        }

        [Fact]
        public async Task ExecuteAsync_Returns_WhenItIsCancelled()
        {
            this.operation.Object.Cancel();
            await this.operation.Object.ExecuteAsync();
        }

        [Fact]
        public async Task ExecuteAsync_Throws_WhenResultIsNotJObject()
        {
            this.operation.Protected()
                          .Setup<Task<JToken>>("OnExecuteAsync")
                          .Returns(Task.FromResult<JToken>(new JArray()));

            this.operation.Object.Item = new JObject();

            var ex = await Assert.ThrowsAsync<MobileServiceInvalidOperationException>(() => this.operation.Object.ExecuteAsync());
            Assert.Equal("Mobile Service table operation returned an unexpected response.", ex.Message);
        }

        [Fact]
        public async Task ExecuteAsync_DoesNotThrow_WhenResultIsNull()
        {
            this.operation.Protected()
                          .Setup<Task<JToken>>("OnExecuteAsync")
                          .Returns(Task.FromResult<JToken>(null));

            this.operation.Object.Item = new JObject();

            JObject result = await this.operation.Object.ExecuteAsync();
            Assert.Null(result);
        }

        [Fact]
        public void Serialize_Succeeds()
        {
            this.operation.Object.Item = JObject.Parse("{\"id\":\"abc\",\"text\":\"example\"}");

            var serializedOperation = this.operation.Object.Serialize();

            Assert.NotNull(serializedOperation["id"]);
            Assert.Equal("abc", serializedOperation["itemId"]);
            Assert.Equal("test", serializedOperation["tableName"]);
            Assert.Equal(0, serializedOperation["kind"]);
            Assert.Equal(serializedOperation["item"], JValue.CreateString(null));
            Assert.NotNull(serializedOperation["sequence"]);
        }

        [Fact]
        public void Deserialize_Succeeds()
        {
            var serializedOperation = JObject.Parse("{\"id\":\"70cf6cc2-5981-4a32-ae6c-249572917a46\",\"kind\": 0,\"tableName\":\"test\",\"itemId\":\"abc\",\"item\":null,\"createdAt\":\"2014-03-11T20:37:10.3366689Z\",\"sequence\":0}");

            var operation = MobileServiceTableOperation.Deserialize(serializedOperation);

            Assert.Equal(serializedOperation["id"], operation.Id);
            Assert.Equal(serializedOperation["itemId"], operation.ItemId);
            Assert.Equal(serializedOperation["tableName"], operation.TableName);
            Assert.Equal(MobileServiceTableOperationKind.Insert, operation.Kind);
            Assert.Null(operation.Item);
            Assert.Equal(serializedOperation["sequence"], operation.Sequence);
        }

        [Fact]
        public void Deserialize_Succeeds_WithItem()
        {
            var serializedOperation = JObject.Parse(@"
            {""id"":""70cf6cc2-5981-4a32-ae6c-249572917a46"",
            ""kind"": 2,
            ""tableName"":""test"",
            ""itemId"":""abc"",
            ""version"":123,
            ""sequence"":null,
            ""state"":null,
            ""item"":""{\""id\"":\""abc\"",\""text\"":\""example\""}"",
            ""createdAt"":""2014-03-11T20:37:10.3366689Z"",
            ""sequence"":0
            }");
            var operation = MobileServiceTableOperation.Deserialize(serializedOperation);

            Assert.Equal(serializedOperation["id"], operation.Id);
            Assert.Equal(serializedOperation["itemId"], operation.ItemId);
            Assert.Equal(serializedOperation["version"], operation.Version);
            Assert.Equal(serializedOperation["tableName"], operation.TableName);
            Assert.Equal(MobileServiceTableOperationKind.Delete, operation.Kind);
            Assert.Equal(serializedOperation["sequence"], operation.Sequence);
            Assert.Equal("abc", operation.Item["id"]);
            Assert.Equal("example", operation.Item["text"]);
        }

        [Fact]
        public void Deserialize_Succeeds_WhenVersionSequenceOrStateIsNull()
        {
            var serializedOperation = JObject.Parse(@"
            {""id"":""70cf6cc2-5981-4a32-ae6c-249572917a46"",
            ""kind"": 2,
            ""tableName"":""test"",
            ""itemId"":""abc"",
            ""version"":null,
            ""sequence"":null,
            ""state"":null,
            ""item"":""{\""id\"":\""abc\"",\""text\"":\""example\""}"",
            ""createdAt"":""2014-03-11T20:37:10.3366689Z"",
            ""sequence"":0
            }");
            var operation = MobileServiceTableOperation.Deserialize(serializedOperation);

            Assert.Equal(serializedOperation["id"], operation.Id);
            Assert.Equal(serializedOperation["itemId"], operation.ItemId);
            Assert.Equal(serializedOperation["tableName"], operation.TableName);
            Assert.Equal(MobileServiceTableOperationKind.Delete, operation.Kind);
            Assert.Equal(serializedOperation["sequence"], operation.Sequence);
            Assert.Equal(0, operation.Version);
            Assert.Equal("abc", operation.Item["id"]);
            Assert.Equal("example", operation.Item["text"]);
        }
    }
}