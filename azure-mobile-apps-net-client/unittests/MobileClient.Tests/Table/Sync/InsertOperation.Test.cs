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
    public class InsertOperationTests
    {
        private InsertOperation operation;

        public InsertOperationTests()
        {
            this.operation = new InsertOperation("test", MobileServiceTableKind.Table, "abc");
        }

        [Fact]
        public async Task ExecuteAsync_InsertsItemOnTable()
        {
            var client = new Mock<MobileServiceClient>(MockBehavior.Strict);

            var table = new Mock<MobileServiceTable>("test", client.Object);
            this.operation.Table = table.Object;

            var item = JObject.Parse("{\"id\":\"abc\",\"Text\":\"Example\"}");
            var itemWithProperties = JObject.Parse("{\"id\":\"abc\",\"Text\":\"Example\",\"version\":\"1\",\"__system\":12}");
            this.operation.Item = itemWithProperties;

            table.Setup(t => t.InsertAsync(item)).Returns(Task.FromResult<JToken>(item));

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
            Assert.Equal("An insert operation on the item is already in the queue.", ex.Message);
        }

        [Fact]
        public void Validate_Succeeds_WithUpdateOperation()
        {
            var newOperation = new UpdateOperation("test", MobileServiceTableKind.Table, "abc");
            this.operation.Validate(newOperation);
        }

        [Fact]
        public void Validate_Throws_WithDeleteOperation_WhenInsertIsAttempted()
        {
            var newOperation = new DeleteOperation("test", MobileServiceTableKind.Table, "abc");
            this.operation.State = MobileServiceTableOperationState.Attempted;
            var ex = Assert.Throws<InvalidOperationException>(() => this.operation.Validate(newOperation));
            Assert.Equal("The item is in inconsistent state in the local store. Please complete the pending sync by calling PushAsync() before deleting the item.", ex.Message);
        }

        [Fact]
        public void Validate_Succeeds_WithDeleteOperation()
        {
            var newOperation = new DeleteOperation("test", MobileServiceTableKind.Table, "abc");
            this.operation.Validate(newOperation);
        }

        [Fact]
        public void Collapse_CancelsExistingOperation_WithUpdateOperation()
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
        public void Collapse_CancelsBothOperations_WithDeleteOperation()
        {
            var newOperation = new DeleteOperation("test", MobileServiceTableKind.Table, "abc");
            this.operation.Collapse(newOperation);

            // new operation should be cancelled
            Assert.True(newOperation.IsCancelled);

            // existing operation should also be cancelled
            Assert.True(this.operation.IsCancelled);
        }
    }
}