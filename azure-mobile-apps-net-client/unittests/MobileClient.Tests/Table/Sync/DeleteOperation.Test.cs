// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using Microsoft.WindowsAzure.MobileServices;
using Microsoft.WindowsAzure.MobileServices.Sync;
using Moq;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace MobileClient.Tests.Table.Sync
{
    public class DeleteOperationTests
    {
        private DeleteOperation operation;

        public DeleteOperationTests()
        {
            this.operation = new DeleteOperation("test", MobileServiceTableKind.Table, "abc");
        }

        [Fact]
        public void WriteResultToStore_IsFalse()
        {
            Assert.False(this.operation.CanWriteResultToStore);
        }

        [Fact]
        public async Task ExecuteAsync_DeletesItemOnTable()
        {
            var client = new Mock<MobileServiceClient>(MockBehavior.Strict);

            var table = new Mock<MobileServiceTable>("test", client.Object);
            this.operation.Table = table.Object;

            var item = JObject.Parse("{\"id\":\"abc\",\"Text\":\"Example\"}");
            this.operation.Item = item;

            table.Setup(t => t.DeleteAsync(item)).Returns(Task.FromResult<JToken>(item));

            await this.operation.ExecuteAsync();
        }

        [Fact]
        public async Task ExecuteAsync_IgnoresNotFound()
        {
            var client = new Mock<MobileServiceClient>(MockBehavior.Strict);

            var table = new Mock<MobileServiceTable>("test", client.Object);
            this.operation.Table = table.Object;

            var item = JObject.Parse("{\"id\":\"abc\",\"Text\":\"Example\"}");
            this.operation.Item = item;

            table.Setup(t => t.DeleteAsync(item)).Throws(new MobileServiceInvalidOperationException("not found", new HttpRequestMessage(), new HttpResponseMessage(HttpStatusCode.NotFound)));

            JObject result = await this.operation.ExecuteAsync();
            Assert.Null(result);
        }

        [Fact]
        public async Task ExecuteLocalAsync_DeletesItemOnStore()
        {
            var store = new Mock<IMobileServiceLocalStore>();
            await this.operation.ExecuteLocalAsync(store.Object, null);
            store.Verify(s => s.DeleteAsync("test", It.Is<string[]>(i => i.Contains("abc"))), Times.Once());
        }

        [Fact]
        public async Task ExecuteLocalAsync_Throws_WhenStoreThrows()
        {
            var store = new Mock<IMobileServiceLocalStore>();
            var storeError = new InvalidOperationException();
            store.Setup(s => s.DeleteAsync("test", It.Is<string[]>(i => i.Contains("abc")))).Throws(storeError);

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => this.operation.ExecuteLocalAsync(store.Object, null));
            Assert.Same(storeError, ex);
        }

        [Fact]
        public void Validate_Throws_WithInsertOperation()
        {
            var tableOperation = new InsertOperation("test", MobileServiceTableKind.Table, "abc");
            TestDeleteValidateThrows(tableOperation);
        }

        [Fact]
        public void Validate_Throws_WithUpdateOperation()
        {
            var tableOperation = new UpdateOperation("test", MobileServiceTableKind.Table, "abc");
            TestDeleteValidateThrows(tableOperation);
        }

        [Fact]
        public void Validate_Throws_WithDeleteOperation()
        {
            var tableOperation = new DeleteOperation("test", MobileServiceTableKind.Table, "abc");
            TestDeleteValidateThrows(tableOperation);
        }

        private void TestDeleteValidateThrows(MobileServiceTableOperation tableOperation)
        {
            var ex = Assert.Throws<InvalidOperationException>(() => this.operation.Validate(tableOperation));
            Assert.Equal("A delete operation on the item is already in the queue.", ex.Message);
        }

        [Fact]
        public void Serialize_Succeeds_DeleteHasItem()
        {
            var serializedItem = "{\"id\":\"abc\",\"text\":\"example\"}";
            this.operation.Item = JObject.Parse(serializedItem);

            var serializedOperation = this.operation.Serialize();

            // Check delete successfully overrides keeping an item
            Assert.Equal(2, serializedOperation["kind"]);
            Assert.Equal(serializedOperation["item"], serializedItem);
        }
    }
}