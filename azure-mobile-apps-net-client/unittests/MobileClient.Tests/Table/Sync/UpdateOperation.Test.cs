// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using Microsoft.WindowsAzure.MobileServices;
using Microsoft.WindowsAzure.MobileServices.Sync;
using Moq;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace MobileClient.Tests.Table.Sync
{
    public class UpdateOperationTests
    {
        private UpdateOperation operation;

        public UpdateOperationTests()
        {
            this.operation = new UpdateOperation("test", MobileServiceTableKind.Table, "abc");
        }

        [Fact]
        public async Task ExecuteAsync_UpdatesItemOnTable()
        {
            var client = new Mock<MobileServiceClient>(MockBehavior.Strict);

            var table = new Mock<MobileServiceTable>("test", client.Object);
            this.operation.Table = table.Object;

            var item = JObject.Parse("{\"id\":\"abc\",\"Text\":\"Example\"}");
            this.operation.Item = item;

            table.Setup(t => t.UpdateAsync(item)).Returns(Task.FromResult<JToken>(item));

            await this.operation.ExecuteAsync();
        }

        [Fact]
        public async Task ExecuteLocalAsync_UpsertsItemOnStore()
        {
            var store = new Mock<IMobileServiceLocalStore>();
            var item = JObject.Parse("{\"id\":\"abc\",\"Text\":\"Example\"}");

            await this.operation.ExecuteLocalAsync(store.Object, item);
            store.Verify(s => s.UpsertAsync("test", It.Is<JObject[]>(list => list.Contains(item)), false), Times.Once());
        }

        [Fact]
        public async Task ExecuteLocalAsync_Throws_WhenStoreThrows()
        {
            var store = new Mock<IMobileServiceLocalStore>();
            var storeError = new InvalidOperationException();
            var item = JObject.Parse("{\"id\":\"abc\",\"Text\":\"Example\"}");

            store.Setup(s => s.UpsertAsync("test", It.Is<JObject[]>(list => list.Contains(item)), false)).Throws(storeError);
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => this.operation.ExecuteLocalAsync(store.Object, item));
            Assert.Same(storeError, ex);
        }

        [Fact]
        public void Validate_Throws_WithInsertOperation()
        {
            var newOperation = new InsertOperation("test", MobileServiceTableKind.Table, "abc");
            var ex = Assert.Throws<InvalidOperationException>(() => this.operation.Validate(newOperation));
            Assert.Equal("An update operation on the item is already in the queue.", ex.Message);
        }

        [Fact]
        public void Validate_Succeeds_WithUpdateOperation()
        {
            var newOperation = new UpdateOperation("test", MobileServiceTableKind.Table, "abc");
            this.operation.Validate(newOperation);
        }

        [Fact]
        public void Validate_Succeeds_WithDeleteOperation()
        {
            var newOperation = new DeleteOperation("test", MobileServiceTableKind.Table, "abc");
            this.operation.Validate(newOperation);
        }

        [Fact]
        public void Collapse_CancelsNewOperation_WithUpdateOperation()
        {
            var newOperation = new UpdateOperation("test", MobileServiceTableKind.Table, "abc");
            this.operation.Collapse(newOperation);

            // new operation should be cancelled
            Assert.True(newOperation.IsCancelled);

            // existing operation should be updated and not cancelled
            Assert.False(this.operation.IsCancelled);
            Assert.True(this.operation.IsUpdated);
            Assert.Equal(2, this.operation.Version);
        }

        [Fact]
        public void Collapse_CancelsExistingOperation_WithDeleteOperation()
        {
            var newOperation = new DeleteOperation("test", MobileServiceTableKind.Table, "abc");
            this.operation.Collapse(newOperation);

            // new operation should not be cancelled but rather updated
            Assert.False(newOperation.IsCancelled);
            Assert.True(newOperation.IsUpdated);
            Assert.Equal(2L, newOperation.Version);

            // existing operation should be cancelled
            Assert.True(this.operation.IsCancelled);
        }
    }
}