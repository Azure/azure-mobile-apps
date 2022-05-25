// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using DeviceTests.Shared.Helpers.Data;
using DeviceTests.Shared.Helpers.Models;
using DeviceTests.Shared.TestPlatform;
using Microsoft.WindowsAzure.MobileServices;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Xunit;

// Several parts of this check to see if CreatedAt/UpdatedAt are non-null, which is ridiculous based on xUnit, but totally valid according to spec.
#pragma warning disable xUnit2002 // Do not use null check on value type

namespace DeviceTests.Shared.Tests
{
    [Collection(nameof(SingleThreadedCollection))]
    public class MobileServiceTable_Tests : E2ETestBase
    {
        /// <summary>
        /// Makes sure the named table is empty
        /// </summary>
        /// <typeparam name="T">The type the table holds</typeparam>
        /// <returns></returns>
        private async Task EnsureEmptyTableAsync<T>()
        {
            IMobileServiceTable<T> table = GetClient().GetTable<T>();
            try
            {
                List<T> items = items = await table.ToListAsync();
                foreach (var item in items)
                {
                    await table.DeleteAsync(item);
                }
            } catch (MobileServiceInvalidOperationException ex)
            {
                Console.WriteLine($"ERROR (ignoring): Cannot clean up table: {ex.Message}");
            }
        }

        [Fact]
        public async Task AsyncTableOperationsWithValidStringIdAgainstStringIdTable()
        {
            await EnsureEmptyTableAsync<ToDoWithStringId>();

            string[] testIdData = IdTestData.ValidStringIds;
            IMobileServiceTable<ToDoWithStringId> table = GetClient().GetTable<ToDoWithStringId>();

            foreach (string testId in testIdData)
            {
                ToDoWithStringId item = new ToDoWithStringId() { Id = testId, Name = "Hey" };
                await table.InsertAsync(item);

                // Read
                IEnumerable<ToDoWithStringId> results = await table.ReadAsync();
                ToDoWithStringId[] items = results.ToArray();

                Assert.Single(items);
                Assert.Equal(testId, items[0].Id);
                Assert.Equal("Hey", items[0].Name);

                // Filter
                results = await table.Where(i => i.Id == testId).ToEnumerableAsync();
                items = results.ToArray();

                Assert.Single(items);
                Assert.Equal(testId, items[0].Id);
                Assert.Equal("Hey", items[0].Name);

                // Projection
                var projectedResults = await table.Select(i => new { XId = i.Id, XString = i.Name }).ToEnumerableAsync();
                var projectedItems = projectedResults.ToArray();

                Assert.Single(projectedItems);
                Assert.Equal(testId, projectedItems[0].XId);
                Assert.Equal("Hey", projectedItems[0].XString);

                // Lookup
                item = await table.LookupAsync(testId);
                Assert.Equal(testId, item.Id);
                Assert.Equal("Hey", item.Name);

                // Update
                item.Name = "What?";
                await table.UpdateAsync(item);
                Assert.Equal(testId, item.Id);
                Assert.Equal("What?", item.Name);

                // Refresh
                item = new ToDoWithStringId() { Id = testId, Name = "Hey" };
                await table.RefreshAsync(item);
                Assert.Equal(testId, item.Id);
                Assert.Equal("What?", item.Name);

                // Read Again
                results = await table.ReadAsync();
                items = results.ToArray();

                Assert.Single(items);
                Assert.Equal(testId, items[0].Id);
                Assert.Equal("What?", items[0].Name);

                await table.DeleteAsync(item);
            }
        }

        [Fact]
        public async Task OrderingReadAsyncWithValidStringIdAgainstStringIdTable()
        {
            await EnsureEmptyTableAsync<ToDoWithStringId>();

            string[] testIdData = new string[] { "a", "b", "C", "_A", "_B", "_C", "1", "2", "3" };
            IMobileServiceTable<ToDoWithStringId> table = GetClient().GetTable<ToDoWithStringId>();

            foreach (string testId in testIdData)
            {
                ToDoWithStringId item = new ToDoWithStringId() { Id = testId, Name = "Hey" };
                await table.InsertAsync(item);
            }

            IEnumerable<ToDoWithStringId> results = await table.OrderBy(p => p.Id).ToEnumerableAsync();
            ToDoWithStringId[] items = results.ToArray();

            Assert.Equal(9, items.Count());
            Assert.Equal("_A", items[0].Id);
            Assert.Equal("_B", items[1].Id);
            Assert.Equal("_C", items[2].Id);
            Assert.Equal("1", items[3].Id);
            Assert.Equal("2", items[4].Id);
            Assert.Equal("3", items[5].Id);
            Assert.Equal("a", items[6].Id);
            Assert.Equal("b", items[7].Id);
            Assert.Equal("C", items[8].Id);

            results = await table.OrderByDescending(p => p.Id).ToEnumerableAsync();
            items = results.ToArray();

            Assert.Equal(9, items.Count());
            Assert.Equal("_A", items[8].Id);
            Assert.Equal("_B", items[7].Id);
            Assert.Equal("_C", items[6].Id);
            Assert.Equal("1", items[5].Id);
            Assert.Equal("2", items[4].Id);
            Assert.Equal("3", items[3].Id);
            Assert.Equal("a", items[2].Id);
            Assert.Equal("b", items[1].Id);
            Assert.Equal("C", items[0].Id);

            foreach (string testId in testIdData)
            {
                ToDoWithStringId item = new ToDoWithStringId() { Id = testId };
                await table.DeleteAsync(item);
            }
        }

        [Fact]
        public async Task FilterReadAsyncWithEmptyStringIdAgainstStringIdTable()
        {
            await EnsureEmptyTableAsync<ToDoWithStringId>();

            string[] testIdData = IdTestData.ValidStringIds;
            IMobileServiceTable<ToDoWithStringId> table = GetClient().GetTable<ToDoWithStringId>();

            foreach (string testId in testIdData)
            {
                ToDoWithStringId item = new ToDoWithStringId() { Id = testId, Name = "Hey" };
                await table.InsertAsync(item);
            }

            string[] invalidIdData = IdTestData.EmptyStringIds.Concat(
                                    IdTestData.InvalidStringIds).Concat(
                                    new string[] { null }).ToArray();

            foreach (string invalidId in invalidIdData)
            {
                IEnumerable<ToDoWithStringId> results = await table.Where(p => p.Id == invalidId).ToEnumerableAsync();
                ToDoWithStringId[] items = results.ToArray();

                Assert.Empty(items);
            }

            foreach (string testId in testIdData)
            {
                ToDoWithStringId item = new ToDoWithStringId() { Id = testId };
                await table.DeleteAsync(item);
            }
        }

        [Fact]
        public async Task LookupAsyncWithNosuchItemAgainstStringIdTable()
        {
            await EnsureEmptyTableAsync<ToDoWithStringId>();

            string[] testIdData = IdTestData.ValidStringIds;
            IMobileServiceTable<ToDoWithStringId> table = GetClient().GetTable<ToDoWithStringId>();

            foreach (string testId in testIdData)
            {
                ToDoWithStringId item = new ToDoWithStringId() { Id = testId, Name = "Hey" };
                await table.InsertAsync(item);
            }

            foreach (string testId in testIdData)
            {
                ToDoWithStringId item = await table.LookupAsync(testId);
                await table.DeleteAsync(item);

                MobileServiceInvalidOperationException exception = await Assert.ThrowsAsync<MobileServiceInvalidOperationException>(() => table.LookupAsync(testId));
                Assert.Equal(HttpStatusCode.NotFound, exception.Response.StatusCode);
                Assert.True(exception.Message == "The item does not exist" || exception.Message == "The request could not be completed.  (Not Found)");
            }
        }

        [Fact]
        public async Task RefreshAsyncWithNoSuchItemAgainstStringIdTable()
        {
            await EnsureEmptyTableAsync<ToDoWithStringId>();

            string[] testIdData = IdTestData.ValidStringIds;
            IMobileServiceTable<ToDoWithStringId> table = GetClient().GetTable<ToDoWithStringId>();

            foreach (string testId in testIdData)
            {
                ToDoWithStringId item = new ToDoWithStringId() { Id = testId, Name = "Hey" };
                await table.InsertAsync(item);
            }

            foreach (string testId in testIdData)
            {
                ToDoWithStringId item = await table.LookupAsync(testId);
                await table.DeleteAsync(item);
                item.Id = testId;

                await Assert.ThrowsAsync<InvalidOperationException>(() => table.RefreshAsync(item));
            }
        }

        [Fact]
        public async Task InsertAsyncWithEmptyStringIdAgainstStringIdTable()
        {
            string[] emptyIdData = IdTestData.EmptyStringIds.Concat(
                                    new string[] { null }).ToArray();
            IMobileServiceTable<ToDoWithStringId> table = GetClient().GetTable<ToDoWithStringId>();

            int count = 0;
            List<ToDoWithStringId> itemsToDelete = new List<ToDoWithStringId>();

            foreach (string emptyId in emptyIdData)
            {
                ToDoWithStringId item = new ToDoWithStringId() { Id = emptyId, Name = (++count).ToString() };
                await table.InsertAsync(item);

                Assert.NotNull(item.Id);
                Assert.Equal(count.ToString(), item.Name);
                itemsToDelete.Add(item);
            }

            foreach (var item in itemsToDelete)
            {
                await table.DeleteAsync(item);
            }
        }

        [Fact]
        public async Task InsertAsyncWithExistingItemAgainstStringIdTable()
        {
            await EnsureEmptyTableAsync<ToDoWithStringId>();

            string[] testIdData = IdTestData.ValidStringIds;
            IMobileServiceTable<ToDoWithStringId> table = GetClient().GetTable<ToDoWithStringId>();

            foreach (string testId in testIdData)
            {
                ToDoWithStringId item = new ToDoWithStringId() { Id = testId, Name = "Hey" };
                await table.InsertAsync(item);
            }

            foreach (string testId in testIdData)
            {
                ToDoWithStringId item = await table.LookupAsync(testId);
                item.Name = "No we're talking!";

                var exception = await Assert.ThrowsAnyAsync<MobileServiceInvalidOperationException>(() => table.InsertAsync(item));
                Assert.Equal(HttpStatusCode.Conflict, exception.Response.StatusCode);
                Assert.True(exception.Message.Contains("Could not insert the item because an item with that id already exists.") ||
                              exception.Message == "The request could not be completed.  (Conflict)");
            }
        }

        [Fact]
        public async Task UpdateAsyncWithNosuchItemAgainstStringIdTable()
        {
            await EnsureEmptyTableAsync<ToDoWithStringId>();

            string[] testIdData = IdTestData.ValidStringIds;
            IMobileServiceTable<ToDoWithStringId> table = GetClient().GetTable<ToDoWithStringId>();

            foreach (string testId in testIdData)
            {
                ToDoWithStringId item = new ToDoWithStringId() { Id = testId, Name = "Hey" };
                await table.InsertAsync(item);
            }

            foreach (string testId in testIdData)
            {
                ToDoWithStringId item = await table.LookupAsync(testId);
                await table.DeleteAsync(item);
                item.Id = testId;
                item.Name = "Alright!";

                var exception = await Assert.ThrowsAsync<MobileServiceInvalidOperationException>(() => table.UpdateAsync(item));
                Assert.Equal(HttpStatusCode.NotFound, exception.Response.StatusCode);
                Assert.True(exception.Message == "The item does not exist" ||
                              exception.Message == "The request could not be completed.  (Not Found)");
            }
        }

        [Fact]
        public async Task DeleteAsyncWithNosuchItemAgainstStringIdTable()
        {
            await EnsureEmptyTableAsync<ToDoWithStringId>();

            string[] testIdData = IdTestData.ValidStringIds;
            IMobileServiceTable<ToDoWithStringId> table = GetClient().GetTable<ToDoWithStringId>();

            foreach (string testId in testIdData)
            {
                ToDoWithStringId item = new ToDoWithStringId() { Id = testId, Name = "Hey" };
                await table.InsertAsync(item);
            }

            foreach (string testId in testIdData)
            {
                ToDoWithStringId item = await table.LookupAsync(testId);
                await table.DeleteAsync(item);
                item.Id = testId;

                var exception = await Assert.ThrowsAnyAsync<MobileServiceInvalidOperationException>(() => table.DeleteAsync(item));
                Assert.Equal(HttpStatusCode.NotFound, exception.Response.StatusCode);
                Assert.True(exception.Message == "The item does not exist" ||
                              exception.Message == "The request could not be completed.  (Not Found)");
            }
        }

        [Fact]
        public async Task DeleteAsync_ThrowsPreconditionFailedException_WhenMergeConflictOccurs()
        {
            await EnsureEmptyTableAsync<RoundTripTableItemWithSystemPropertiesType>();
            string id = Guid.NewGuid().ToString();
            IMobileServiceTable table = GetClient().GetTable("RoundTripTable");

            var item = new JObject() { { "id", id }, { "name", "a value" } };
            var inserted = await table.InsertAsync(item);
            item["version"] = "3q3A3g==";

            var expectedException = await Assert.ThrowsAsync<MobileServicePreconditionFailedException>(() => table.DeleteAsync(item));
            Assert.Equal(expectedException.Value["version"], inserted["version"]);
            Assert.Equal(expectedException.Value["name"], inserted["name"]);
        }

        [Fact]
        public async Task DeleteAsync_ThrowsPreconditionFailedException_WhenMergeConflictOccurs_Generic()
        {
            string id = Guid.NewGuid().ToString();
            var table = GetClient().GetTable<RoundTripTableItemWithSystemPropertiesType>();

            // insert a new item
            var item = new RoundTripTableItemWithSystemPropertiesType() { Id = id, Name = "a value" };
            await table.InsertAsync(item);

            Assert.NotNull(item.CreatedAt);
            Assert.NotNull(item.UpdatedAt);
            Assert.NotNull(item.Version);

            string version = item.Version;

            // Delete with wrong version
            item.Version = "3q3A3g==";
            item.Name = "But wait!";
            var expectedException = await Assert.ThrowsAsync<MobileServicePreconditionFailedException<RoundTripTableItemWithSystemPropertiesType>>(() => table.DeleteAsync(item));
            Assert.Equal(HttpStatusCode.PreconditionFailed, expectedException.Response.StatusCode);

            string responseContent = await expectedException.Response.Content.ReadAsStringAsync();

            RoundTripTableItemWithSystemPropertiesType serverItem = expectedException.Item;
            string serverVersion = serverItem.Version;
            string stringValue = serverItem.Name;

            Assert.Equal(version, serverVersion);
            Assert.Equal("a value", stringValue);

            Assert.NotNull(expectedException.Item);
            Assert.Equal(version, expectedException.Item.Version);
            Assert.Equal(stringValue, expectedException.Item.Name);

            // Delete one last time with the version from the server
            item.Version = serverVersion;
            await table.DeleteAsync(item);

            Assert.Null(item.Id);
        }

        [Fact]
        public async Task AsyncTableOperationsWithIntegerAsStringIdAgainstIntIdTable()
        {
            await EnsureEmptyTableAsync<ToDoWithStringIdAgainstIntIdTable>();

            IMobileServiceTable<ToDoWithStringIdAgainstIntIdTable> stringIdTable = GetClient().GetTable<ToDoWithStringIdAgainstIntIdTable>();
            ToDoWithStringIdAgainstIntIdTable item = new ToDoWithStringIdAgainstIntIdTable() { Name = "Hey" };

            // Insert
            await stringIdTable.InsertAsync(item);
            string testId = item.Id.ToString();

            // Read
            IEnumerable<ToDoWithStringIdAgainstIntIdTable> results = await stringIdTable.ReadAsync();
            ToDoWithStringIdAgainstIntIdTable[] items = results.ToArray();

            Assert.Single(items);
            Assert.Equal(testId, items[0].Id);
            Assert.Equal("Hey", items[0].Name);

            // Filter
            results = await stringIdTable.Where(i => i.Id == testId).ToEnumerableAsync();
            items = results.ToArray();

            Assert.Single(items);
            Assert.Equal(testId, items[0].Id);
            Assert.Equal("Hey", items[0].Name);

            // Projection
            var projectedResults = await stringIdTable.Select(i => new { XId = i.Id, XString = i.Name }).ToEnumerableAsync();
            var projectedItems = projectedResults.ToArray();

            Assert.Single(projectedItems);
            Assert.Equal(testId, projectedItems[0].XId);
            Assert.Equal("Hey", projectedItems[0].XString);

            // Lookup
            ToDoWithStringIdAgainstIntIdTable stringIdItem = await stringIdTable.LookupAsync(testId);
            Assert.Equal(testId, stringIdItem.Id);
            Assert.Equal("Hey", stringIdItem.Name);

            // Update
            stringIdItem.Name = "What?";
            await stringIdTable.UpdateAsync(stringIdItem);
            Assert.Equal(testId, stringIdItem.Id);
            Assert.Equal("What?", stringIdItem.Name);

            // Refresh
            stringIdItem = new ToDoWithStringIdAgainstIntIdTable() { Id = testId, Name = "Hey" };
            await stringIdTable.RefreshAsync(stringIdItem);
            Assert.Equal(testId, stringIdItem.Id);
            Assert.Equal("What?", stringIdItem.Name);

            // Read Again
            results = await stringIdTable.ReadAsync();
            items = results.ToArray();

            Assert.Single(items);
            Assert.Equal(testId, items[0].Id);
            Assert.Equal("What?", items[0].Name);

            // Delete
            await stringIdTable.DeleteAsync(item);
        }

        [Fact]
        public async Task OrderingReadAsyncWithStringIdAgainstIntegerIdTable()
        {
            await EnsureEmptyTableAsync<ToDoWithIntId>();

            IMobileServiceTable<ToDoWithIntId> table = GetClient().GetTable<ToDoWithIntId>();
            List<ToDoWithIntId> integerIdItems = new List<ToDoWithIntId>();
            for (var i = 0; i < 10; i++)
            {
                ToDoWithIntId item = new ToDoWithIntId() { Name = i.ToString() };
                await table.InsertAsync(item);
                integerIdItems.Add(item);
            }

            IMobileServiceTable<ToDoWithStringIdAgainstIntIdTable> stringIdTable = GetClient().GetTable<ToDoWithStringIdAgainstIntIdTable>();

            IEnumerable<ToDoWithStringIdAgainstIntIdTable> results = await stringIdTable.OrderBy(p => p.Id).ToEnumerableAsync();
            ToDoWithStringIdAgainstIntIdTable[] items = results.ToArray();

            Assert.Equal(10, items.Count());
            for (var i = 0; i < 8; i++)
            {
                Assert.Equal((int.Parse(items[i].Id) + 1).ToString(), items[i + 1].Id);
            }

            results = await stringIdTable.OrderByDescending(p => p.Id).ToEnumerableAsync();
            items = results.ToArray();

            Assert.Equal(10, items.Count());
            for (var i = 8; i >= 0; i--)
            {
                Assert.Equal((int.Parse(items[i].Id) - 1).ToString(), items[i + 1].Id);
            }

            foreach (ToDoWithIntId integerIdItem in integerIdItems)
            {
                await table.DeleteAsync(integerIdItem);
            }
        }

        [Fact]
        public async Task FilterReadAsyncWithIntegerAsStringIdAgainstIntegerIdTable()
        {
            await EnsureEmptyTableAsync<ToDoWithIntId>();

            IMobileServiceTable<ToDoWithIntId> table = GetClient().GetTable<ToDoWithIntId>();
            List<ToDoWithIntId> integerIdItems = new List<ToDoWithIntId>();
            for (var i = 0; i < 10; i++)
            {
                ToDoWithIntId item = new ToDoWithIntId() { Name = i.ToString() };
                await table.InsertAsync(item);
                integerIdItems.Add(item);
            }

            IMobileServiceTable<ToDoWithStringIdAgainstIntIdTable> stringIdTable = GetClient().GetTable<ToDoWithStringIdAgainstIntIdTable>();

            IEnumerable<ToDoWithStringIdAgainstIntIdTable> results = await stringIdTable.Where(p => p.Id == integerIdItems[0].Id.ToString()).ToEnumerableAsync();
            ToDoWithStringIdAgainstIntIdTable[] items = results.ToArray();
            Assert.Single(items);
            Assert.Equal(integerIdItems[0].Id.ToString(), items[0].Id);
            Assert.Equal("0", items[0].Name);

            foreach (ToDoWithIntId integerIdItem in integerIdItems)
            {
                await table.DeleteAsync(integerIdItem);
            }
        }

        [Fact]
        public async Task FilterReadAsyncWithEmptyStringIdAgainstIntegerIdTable()
        {
            await EnsureEmptyTableAsync<ToDoWithIntId>();

            IMobileServiceTable<ToDoWithIntId> table = GetClient().GetTable<ToDoWithIntId>();
            List<ToDoWithIntId> integerIdItems = new List<ToDoWithIntId>();
            for (var i = 0; i < 10; i++)
            {
                ToDoWithIntId item = new ToDoWithIntId() { Name = i.ToString() };
                await table.InsertAsync(item);
                integerIdItems.Add(item);
            }

            string[] testIdData = new string[] { "", " ", null };

            IMobileServiceTable<ToDoWithStringIdAgainstIntIdTable> stringIdTable = GetClient().GetTable<ToDoWithStringIdAgainstIntIdTable>();

            foreach (string testId in testIdData)
            {
                IEnumerable<ToDoWithStringIdAgainstIntIdTable> results = await stringIdTable.Where(p => p.Id == testId).ToEnumerableAsync();
                ToDoWithStringIdAgainstIntIdTable[] items = results.ToArray();

                Assert.Empty(items);
            }

            foreach (ToDoWithIntId integerIdItem in integerIdItems)
            {
                await table.DeleteAsync(integerIdItem);
            }
        }

        [Fact]
        public async Task ReadAsyncWithValidIntIdAgainstIntIdTable()
        {
            await EnsureEmptyTableAsync<ToDoWithIntId>();

            IMobileServiceTable<ToDoWithIntId> table = GetClient().GetTable<ToDoWithIntId>();

            ToDoWithIntId item = new ToDoWithIntId() { Name = "Hey" };
            await table.InsertAsync(item);

            IEnumerable<ToDoWithIntId> results = await table.ReadAsync();
            ToDoWithIntId[] items = results.ToArray();

            Assert.Single(items);
            Assert.True(items[0].Id > 0);
            Assert.Equal("Hey", items[0].Name);

            await table.DeleteAsync(item);
        }

        [Fact]
        public async Task AsyncTableOperationsWithAllSystemProperties()
        {
            await EnsureEmptyTableAsync<RoundTripTableItemWithSystemPropertiesType>();

            string id = Guid.NewGuid().ToString();
            IMobileServiceTable<RoundTripTableItemWithSystemPropertiesType> table = GetClient().GetTable<RoundTripTableItemWithSystemPropertiesType>();

            RoundTripTableItemWithSystemPropertiesType item = new RoundTripTableItemWithSystemPropertiesType() { Id = id, Name = "a value" };
            await table.InsertAsync(item);

            Assert.NotNull(item.CreatedAt);
            Assert.NotNull(item.UpdatedAt);
            Assert.NotNull(item.Version);

            // Read
            IEnumerable<RoundTripTableItemWithSystemPropertiesType> results = await table.ReadAsync();
            RoundTripTableItemWithSystemPropertiesType[] items = results.ToArray();

            Assert.Single(items);
            Assert.NotNull(items[0].CreatedAt);
            Assert.NotNull(items[0].UpdatedAt);
            Assert.NotNull(items[0].Version);

            // Filter against version
            // BUG #1706815 (OData query for version field (string <--> byte[] mismatch)
            /*
            results = await table.Where(i => i.Version == items[0].Version).ToEnumerableAsync();
            RoundTripTableItemWithSystemPropertiesType[] filterItems = results.ToArray();

            Assert.Equal(1, items.Count());
            Assert.Equal(filterItems[0].CreatedAt, items[0].CreatedAt);
            Assert.Equal(filterItems[0].UpdatedAt, items[0].UpdatedAt);
            Assert.Equal(filterItems[0].Version, items[0].Version);

            // Filter against createdAt
            results = await table.Where(i => i.CreatedAt == items[0].CreatedAt).ToEnumerableAsync();
            RoundTripTableItemWithSystemPropertiesType[] filterItems = results.ToArray();

            Assert.Equal(1, items.Count());
            Assert.Equal(filterItems[0].CreatedAt, items[0].CreatedAt);
            Assert.Equal(filterItems[0].UpdatedAt, items[0].UpdatedAt);
            Assert.Equal(filterItems[0].Version, items[0].Version);

            // Filter against updatedAt
            results = await table.Where(i => i.UpdatedAt == items[0].UpdatedAt).ToEnumerableAsync();
            filterItems = results.ToArray();

            Assert.Equal(1, items.Count());
            Assert.Equal(filterItems[0].CreatedAt, items[0].CreatedAt);
            Assert.Equal(filterItems[0].UpdatedAt, items[0].UpdatedAt);
            Assert.Equal(filterItems[0].Version, items[0].Version);
            */

            // Projection
            var projectedResults = await table.Select(i => new { XId = i.Id, XCreatedAt = i.CreatedAt, XUpdatedAt = i.UpdatedAt, XVersion = i.Version }).ToEnumerableAsync();
            var projectedItems = projectedResults.ToArray();

            Assert.Single(projectedResults);
            Assert.Equal(projectedItems[0].XId, items[0].Id);
            Assert.Equal(projectedItems[0].XCreatedAt, items[0].CreatedAt);
            Assert.Equal(projectedItems[0].XUpdatedAt, items[0].UpdatedAt);
            Assert.Equal(projectedItems[0].XVersion, items[0].Version);

            // Lookup
            item = await table.LookupAsync(id);
            Assert.Equal(id, item.Id);
            Assert.Equal(item.Id, items[0].Id);
            Assert.Equal(item.CreatedAt, items[0].CreatedAt);
            Assert.Equal(item.UpdatedAt, items[0].UpdatedAt);
            Assert.Equal(item.Version, items[0].Version);

            // Refresh
            item = new RoundTripTableItemWithSystemPropertiesType() { Id = id };
            await table.RefreshAsync(item);
            Assert.Equal(id, item.Id);
            Assert.Equal(item.Id, items[0].Id);
            Assert.Equal(item.CreatedAt, items[0].CreatedAt);
            Assert.Equal(item.UpdatedAt, items[0].UpdatedAt);
            Assert.Equal(item.Version, items[0].Version);

            // Update
            item.Name = "Hello!";
            await table.UpdateAsync(item);
            Assert.Equal(item.Id, items[0].Id);
            Assert.Equal(item.CreatedAt, items[0].CreatedAt);
            Assert.True(item.UpdatedAt >= items[0].UpdatedAt);
            Assert.NotNull(item.Version);
            Assert.NotEqual(item.Version, items[0].Version);

            // Read Again
            results = await table.ReadAsync();
            items = results.ToArray();
            Assert.Equal(id, item.Id);
            Assert.Equal(item.Id, items[0].Id);
            Assert.Equal(item.CreatedAt, items[0].CreatedAt);
            Assert.Equal(item.UpdatedAt, items[0].UpdatedAt);
            Assert.Equal(item.Version, items[0].Version);

            await table.DeleteAsync(item);
        }

        [Fact]
        public async Task AsyncTableOperationsWithSystemPropertiesSetExplicitly()
        {
            await EnsureEmptyTableAsync<RoundTripTableItemWithSystemPropertiesType>();

            IMobileServiceTable<RoundTripTableItemWithSystemPropertiesType> allSystemPropertiesTable = GetClient().GetTable<RoundTripTableItemWithSystemPropertiesType>();

            // Regular insert
            RoundTripTableItemWithSystemPropertiesType item = new RoundTripTableItemWithSystemPropertiesType() { Name = "a value" };
            await allSystemPropertiesTable.InsertAsync(item);

            Assert.NotNull(item.CreatedAt);
            Assert.NotNull(item.UpdatedAt);
            Assert.NotNull(item.Version);

            // Explicit System Properties Read
            IEnumerable<RoundTripTableItemWithSystemPropertiesType> results = await allSystemPropertiesTable.Where(p => p.Id == item.Id).ToEnumerableAsync();
            RoundTripTableItemWithSystemPropertiesType[] items = results.ToArray();

            Assert.Single(items);
            Assert.NotNull(items[0].CreatedAt);
            Assert.NotNull(items[0].UpdatedAt);
            Assert.NotNull(items[0].Version);

            // Lookup
            var item3 = await allSystemPropertiesTable.LookupAsync(item.Id);
            Assert.Equal(item.CreatedAt, item3.CreatedAt);
            Assert.Equal(item.UpdatedAt, item3.UpdatedAt);
            Assert.NotNull(item3.Version);

            await allSystemPropertiesTable.DeleteAsync(item);
        }

        [Fact]
        public async Task AsyncFilterSelectOrdering_OrderByCreatedAt_NotImpactedBySystemProperties()
        {
            // Set up the table.
            await EnsureEmptyTableAsync<RoundTripTableItemWithSystemPropertiesType>();

            IMobileServiceTable<RoundTripTableItemWithSystemPropertiesType> table = GetClient().GetTable<RoundTripTableItemWithSystemPropertiesType>();
            List<RoundTripTableItemWithSystemPropertiesType> items = new List<RoundTripTableItemWithSystemPropertiesType>();
            for (int id = 0; id < 5; id++)
            {
                RoundTripTableItemWithSystemPropertiesType item = new RoundTripTableItemWithSystemPropertiesType() { Id = id.ToString(), Name = "a value" };
                await table.InsertAsync(item);
                Assert.NotNull(item.CreatedAt);
                Assert.NotNull(item.UpdatedAt);
                Assert.NotNull(item.Version);
                items.Add(item);
            }

            // Run test
            var results = await table.OrderBy(t => t.CreatedAt).ToListAsync(); // Fails here with .NET runtime. Why??
            RoundTripTableItemWithSystemPropertiesType[] orderItems = results.ToArray();
            for (int i = 0; i < orderItems.Length - 1; i++)
            {
                Assert.True(int.Parse(orderItems[i].Id) < int.Parse(orderItems[i + 1].Id));
            }

            // Cleanup
            foreach (var itemToDelete in items)
            {
                await table.DeleteAsync(itemToDelete);
            }
        }

        [Fact]
        public async Task AsyncFilterSelectOrdering_OrderByUpdatedAt_NotImpactedBySystemProperties()
        {
            // Set up the table.
            await EnsureEmptyTableAsync<RoundTripTableItemWithSystemPropertiesType>();

            IMobileServiceTable<RoundTripTableItemWithSystemPropertiesType> table = GetClient().GetTable<RoundTripTableItemWithSystemPropertiesType>();
            List<RoundTripTableItemWithSystemPropertiesType> items = new List<RoundTripTableItemWithSystemPropertiesType>();
            for (int id = 0; id < 5; id++)
            {
                RoundTripTableItemWithSystemPropertiesType item = new RoundTripTableItemWithSystemPropertiesType() { Id = id.ToString(), Name = "a value" };
                await table.InsertAsync(item);
                Assert.NotNull(item.CreatedAt);
                Assert.NotNull(item.UpdatedAt);
                Assert.NotNull(item.Version);
                items.Add(item);
            }

            // Run test
            var results = await table.OrderBy(t => t.UpdatedAt).ToListAsync(); // Fails here with .NET runtime. Why??
            RoundTripTableItemWithSystemPropertiesType[] orderItems = results.ToArray();
            for (int i = 0; i < orderItems.Length - 1; i++)
            {
                Assert.True(int.Parse(orderItems[i].Id) < int.Parse(orderItems[i + 1].Id));
            }

            // Cleanup
            foreach (var itemToDelete in items)
            {
                await table.DeleteAsync(itemToDelete);
            }
        }

        [Fact]
        public async Task AsyncFilterSelectOrdering_OrderByVersion_NotImpactedBySystemProperties()
        {
            // Set up the table.
            await EnsureEmptyTableAsync<RoundTripTableItemWithSystemPropertiesType>();

            IMobileServiceTable<RoundTripTableItemWithSystemPropertiesType> table = GetClient().GetTable<RoundTripTableItemWithSystemPropertiesType>();
            List<RoundTripTableItemWithSystemPropertiesType> items = new List<RoundTripTableItemWithSystemPropertiesType>();
            for (int id = 0; id < 5; id++)
            {
                RoundTripTableItemWithSystemPropertiesType item = new RoundTripTableItemWithSystemPropertiesType() { Id = id.ToString(), Name = "a value" };
                await table.InsertAsync(item);
                Assert.NotNull(item.CreatedAt);
                Assert.NotNull(item.UpdatedAt);
                Assert.NotNull(item.Version);
                items.Add(item);
            }

            // Run test
            var results = await table.OrderBy(t => t.Version).ToListAsync(); // Fails here with .NET runtime. Why??
            RoundTripTableItemWithSystemPropertiesType[] orderItems = results.ToArray();
            for (int i = 0; i < orderItems.Length - 1; i++)
            {
                Assert.True(int.Parse(orderItems[i].Id) < int.Parse(orderItems[i + 1].Id));
            }

            // Cleanup
            items.ForEach(async t => await table.DeleteAsync(t));
        }

        [Fact]
        public async Task AsyncFilterSelectOrdering_FilterByCreatedAt_NotImpactedBySystemProperties()
        {
            // Set up the table.
            await EnsureEmptyTableAsync<RoundTripTableItemWithSystemPropertiesType>();

            IMobileServiceTable<RoundTripTableItemWithSystemPropertiesType> table = GetClient().GetTable<RoundTripTableItemWithSystemPropertiesType>();
            List<RoundTripTableItemWithSystemPropertiesType> items = new List<RoundTripTableItemWithSystemPropertiesType>();
            for (int id = 0; id < 5; id++)
            {
                RoundTripTableItemWithSystemPropertiesType item = new RoundTripTableItemWithSystemPropertiesType() { Id = id.ToString(), Name = "a value" };
                await table.InsertAsync(item);
                Assert.NotNull(item.CreatedAt);
                Assert.NotNull(item.UpdatedAt);
                Assert.NotNull(item.Version);
                items.Add(item);
            }

            // Run test
            var results = await table.Where(t => t.CreatedAt >= items[4].CreatedAt).ToListAsync();
            RoundTripTableItemWithSystemPropertiesType[] filteredItems = results.ToArray();

            for (int i = 0; i < filteredItems.Length - 1; i++)
            {
                Assert.True(filteredItems[i].CreatedAt >= items[4].CreatedAt);
            }

            // Cleanup
            items.ForEach(async t => await table.DeleteAsync(t));
        }

        [Fact]
        public async Task AsyncFilterSelectOrdering_FilterByUpdatedAt_NotImpactedBySystemProperties()
        {
            // Set up the table.
            await EnsureEmptyTableAsync<RoundTripTableItemWithSystemPropertiesType>();

            IMobileServiceTable<RoundTripTableItemWithSystemPropertiesType> table = GetClient().GetTable<RoundTripTableItemWithSystemPropertiesType>();
            List<RoundTripTableItemWithSystemPropertiesType> items = new List<RoundTripTableItemWithSystemPropertiesType>();
            for (int id = 0; id < 5; id++)
            {
                RoundTripTableItemWithSystemPropertiesType item = new RoundTripTableItemWithSystemPropertiesType() { Id = id.ToString(), Name = "a value" };
                await table.InsertAsync(item);
                Assert.NotNull(item.CreatedAt);
                Assert.NotNull(item.UpdatedAt);
                Assert.NotNull(item.Version);
                items.Add(item);
            }

            // Run test
            var results = await table.Where(t => t.UpdatedAt >= items[4].UpdatedAt).ToListAsync();
            RoundTripTableItemWithSystemPropertiesType[] filteredItems = results.ToArray();

            for (int i = 0; i < filteredItems.Length - 1; i++)
            {
                Assert.True(filteredItems[i].UpdatedAt >= items[4].UpdatedAt);
            }

            // Cleanup
            items.ForEach(async t => await table.DeleteAsync(t));
        }

        [Fact]
        public async Task AsyncFilterSelectOrderingOperationsNotImpactedBySystemProperties()
        {
            await EnsureEmptyTableAsync<RoundTripTableItemWithSystemPropertiesType>();

            IMobileServiceTable<RoundTripTableItemWithSystemPropertiesType> table = GetClient().GetTable<RoundTripTableItemWithSystemPropertiesType>();
            List<RoundTripTableItemWithSystemPropertiesType> items = new List<RoundTripTableItemWithSystemPropertiesType>();

            // Insert some items
            for (int id = 0; id < 5; id++)
            {
                RoundTripTableItemWithSystemPropertiesType item = new RoundTripTableItemWithSystemPropertiesType() { Id = id.ToString(), Name = "a value" };

                await table.InsertAsync(item);

                Assert.NotNull(item.CreatedAt);
                Assert.NotNull(item.UpdatedAt);
                Assert.NotNull(item.Version);
                items.Add(item);
            }

            // Selection
            var selectionResults = await table.Select(t => new { Id = t.Id, CreatedAt = t.CreatedAt }).ToEnumerableAsync();
            var selectedItems = selectionResults.ToArray();

            for (int i = 0; i < selectedItems.Length; i++)
            {
                var item = items.Where(t => t.Id == selectedItems[i].Id).FirstOrDefault();
                Assert.Equal(item.CreatedAt, selectedItems[i].CreatedAt);
            }

            var selectionResults2 = await table.Select(t => new { Id = t.Id, UpdatedAt = t.UpdatedAt }).ToEnumerableAsync();
            var selectedItems2 = selectionResults2.ToArray();

            for (int i = 0; i < selectedItems2.Length; i++)
            {
                var item = items.Where(t => t.Id == selectedItems2[i].Id).FirstOrDefault();
                Assert.Equal(item.UpdatedAt, selectedItems2[i].UpdatedAt);
            }

            var selectionResults3 = await table.Select(t => new { Id = t.Id, Version = t.Version }).ToEnumerableAsync();
            var selectedItems3 = selectionResults3.ToArray();

            for (int i = 0; i < selectedItems3.Length; i++)
            {
                var item = items.Where(t => t.Id == selectedItems3[i].Id).FirstOrDefault();
                Assert.Equal(item.Version, selectedItems3[i].Version);
            }

            // Delete
            foreach (var item in items)
            {
                await table.DeleteAsync(item);
            }
        }

        [Fact]
        public async Task UpdateAsyncWithMergeConflict()
        {
            await EnsureEmptyTableAsync<RoundTripTableItemWithSystemPropertiesType>();
            string id = Guid.NewGuid().ToString();
            IMobileServiceTable table = GetClient().GetTable("RoundTripTable");

            var item = new JObject() { { "id", id }, { "name", "a value" } };
            var inserted = await table.InsertAsync(item);
            item["version"] = "3q3A3g==";

            var expectedException = await Assert.ThrowsAsync<MobileServicePreconditionFailedException>(() => table.UpdateAsync(item));
            Assert.Equal(expectedException.Value["version"], inserted["version"]);
            Assert.Equal(expectedException.Value["name"], inserted["name"]);
        }

        [Fact]
        public async Task UpdateAsyncWithMergeConflict_Generic()
        {
            await EnsureEmptyTableAsync<RoundTripTableItemWithSystemPropertiesType>();

            string id = Guid.NewGuid().ToString();
            IMobileServiceTable<RoundTripTableItemWithSystemPropertiesType> table = GetClient().GetTable<RoundTripTableItemWithSystemPropertiesType>();

            RoundTripTableItemWithSystemPropertiesType item = new RoundTripTableItemWithSystemPropertiesType() { Id = id, Name = "a value" };
            await table.InsertAsync(item);

            Assert.NotNull(item.CreatedAt);
            Assert.NotNull(item.UpdatedAt);
            Assert.NotNull(item.Version);

            string version = item.Version;

            // Update
            item.Name = "Hello!";
            await table.UpdateAsync(item);
            Assert.NotNull(item.Version);
            Assert.NotEqual(item.Version, version);

            string newVersion = item.Version;

            // Update again but with the original version
            item.Version = version;
            item.Name = "But wait!";
            var expectedException = await Assert.ThrowsAsync<MobileServicePreconditionFailedException<RoundTripTableItemWithSystemPropertiesType>>(() => table.UpdateAsync(item));
            Assert.Equal(HttpStatusCode.PreconditionFailed, expectedException.Response.StatusCode);

            Assert.NotNull(expectedException.Item);

            string serverVersion = expectedException.Item.Version;
            string stringValue = expectedException.Item.Name;

            Assert.Equal(newVersion, serverVersion);
            Assert.Equal("Hello!", stringValue);

            // Update one last time with the version from the server
            item.Version = serverVersion;
            await table.UpdateAsync(item);
            Assert.NotNull(item.Version);
            Assert.Equal("But wait!", item.Name);
            Assert.NotEqual(item.Version, serverVersion);

            await table.DeleteAsync(item);
        }
    }
}
