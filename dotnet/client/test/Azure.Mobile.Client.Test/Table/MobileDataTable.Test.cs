using Azure.Mobile.Client.Table;
using Azure.Mobile.Client.Test.Helpers;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Azure.Mobile.Client.Test.Table
{
    /// <summary>
    /// This is the client-side of the Movie DTO from E2EServer
    /// </summary>
    public class Movie : TableData
    {
        public string Title { get; set; }
        public int Duration { get; set; }
        public string MpaaRating { get; set; }
        public DateTime ReleaseDate { get; set; }
        public bool BestPictureWinner { get; set; }
        public int Year { get; set; }
    }

    /// <summary>
    /// This is the client-side of the Unit DTO fro E2EServer, serving
    /// as the basis for HUnit and SUnit.
    /// </summary>
    public class Unit : TableData
    {
        public string Data { get; set; }
    }

    [TestClass]
    public class MobileDataTable_Tests : BaseTest
    {
        #region GetItemAsync
        [TestMethod]
        public async Task GetItemAsync_WithValidId_Returns200()
        {
            var client = GetTestClient();
            var table = client.GetTable<Movie>();
            var actual = await table.GetItemAsync("movie-4");
            var response = actual.GetRawResponse();
            var item = actual.Value;

            Assert.AreEqual(200, response.Status);
            Assert.IsNotNull(item);
            Assert.AreEqual("The Good, the Bad and the Ugly", item.Title);
        }

        [TestMethod]
        public async Task GetItemAsync_NotAuthorized_Returns404()
        {
            var client = GetTestClient();
            var table = client.GetTable<Movie>("tables/unauthorized");

            try
            {
                var actual = await table.GetItemAsync("movie-4");
                var response = actual.GetRawResponse();
                Assert.Fail("RequestFailedException expected");
            }
            catch (RequestFailedException ex)
            {
                Assert.AreEqual(404, ex.Status);
            }
        }

        [TestMethod]
        public async Task GetItemAsync_WithInvalidId_Returns404()
        {
            var client = GetTestClient();
            var table = client.GetTable<Movie>();
            try
            {
                var actual = await table.GetItemAsync("missing");
                var response = actual.GetRawResponse();
                Assert.Fail("RequestFailedException expected");
            }
            catch (RequestFailedException ex)
            {
                Assert.AreEqual(404, ex.Status);
            }
        }

        [TestMethod]
        public async Task GetItemAsync_SoftDelete_DeletedItem_Returns404()
        {
            var client = GetTestClient();
            var table = client.GetTable<Movie>("tables/rmovies");
            try
            {
                var actual = await table.GetItemAsync("rmovie-0");
                var response = actual.GetRawResponse();
                Assert.Fail("RequestFailedException expected");
            }
            catch (RequestFailedException ex)
            {
                Assert.AreEqual(404, ex.Status);
            }
        }

        [TestMethod]
        public async Task GetItemAsync_SoftDelete_ValidItem_Returns200()
        {
            var client = GetTestClient();
            var table = client.GetTable<Movie>("tables/rmovies");
            var actual = await table.GetItemAsync("rmovie-6");
            var response = actual.GetRawResponse();
            var item = actual.Value;

            Assert.AreEqual(200, response.Status);
            Assert.IsNotNull(item);
            Assert.AreEqual("The Dark Knight", item.Title);
        }
        #endregion

        #region GetItem
        [TestMethod]
        public void GetItem_WithValidId_Returns200()
        {
            var client = GetTestClient();
            var table = client.GetTable<Movie>();
            var actual = table.GetItem("movie-4");
            var response = actual.GetRawResponse();
            var item = actual.Value;

            Assert.AreEqual(200, response.Status);
            Assert.IsNotNull(item);
            Assert.AreEqual("The Good, the Bad and the Ugly", item.Title);
        }

        [TestMethod]
        public void GetItem_NotAuthorized_Returns404()
        {
            var client = GetTestClient();
            var table = client.GetTable<Movie>("tables/unauthorized");
            try
            {
                var actual = table.GetItem("movie-4");
                var response = actual.GetRawResponse();
                Assert.Fail("RequestFailedException expected");
            }
            catch (RequestFailedException ex)
            {
                Assert.AreEqual(404, ex.Status);
            }
        }

        [TestMethod]
        public void GetItem_WithInvalidId_Returns404()
        {
            var client = GetTestClient();
            var table = client.GetTable<Movie>();
            try
            {
                var actual = table.GetItem("missing");
                var response = actual.GetRawResponse();
                Assert.Fail("RequestFailedException expected");
            }
            catch (RequestFailedException ex)
            {
                Assert.AreEqual(404, ex.Status);
            }
        }

        [TestMethod]
        public void GetItem_SoftDelete_DeletedItem_Returns404()
        {
            var client = GetTestClient();
            var table = client.GetTable<Movie>("tables/rmovies");
            try
            {
                var actual = table.GetItem("rmovie-0");
                var response = actual.GetRawResponse();
                Assert.Fail("RequestFailedException expected");
            }
            catch (RequestFailedException ex)
            {
                Assert.AreEqual(404, ex.Status);
            }
        }

        [TestMethod]
        public void GetItem_SoftDelete_ValidItem_Returns200()
        {
            var client = GetTestClient();
            var table = client.GetTable<Movie>("tables/rmovies");
            var actual = table.GetItem("rmovie-6");
            var response = actual.GetRawResponse();
            var item = actual.Value;

            Assert.AreEqual(200, response.Status);
            Assert.IsNotNull(item);
            Assert.AreEqual("The Dark Knight", item.Title);
        }
        #endregion

        #region GetItemsAsync
        [TestMethod]
        public async Task GetItemsAsync_ReturnsItems()
        {
            var client = GetTestClient();
            var table = client.GetTable<Movie>();
            var actual = table.GetItemsAsync();

            var items = new List<Movie>();
            await foreach (var item in actual)
            {
                items.Add(item);
            }

            Assert.AreEqual(248, items.Count);
            CollectionAssert.AllItemsAreNotNull(items);
            CollectionAssert.AllItemsAreUnique(items);
        }

        [TestMethod]
        public async Task GetItemsAsync_IncludeCount_ReturnsItems()
        {
            var client = GetTestClient();
            var table = client.GetTable<Movie>();
            var query = new MobileTableQuery { IncludeCount = true };
            var actual = table.GetItemsAsync(query);

            var items = new List<Movie>();
            await foreach (var item in actual)
            {
                items.Add(item);
            }

            Assert.AreEqual(248, items.Count());
            // TODO: Need to see how we can include the Count response.
        }

        [TestMethod]
        public async Task GetItemsAsync_WithFilter_ReturnsItems()
        {
            var client = GetTestClient();
            var table = client.GetTable<Movie>();
            var query = new MobileTableQuery { Filter = "mpaaRating eq 'R'" };
            var actual = table.GetItemsAsync(query);

            var items = new List<Movie>();
            await foreach (var item in actual)
            {
                items.Add(item);
            }

            Assert.AreEqual(94, items.Count());
            Assert.IsFalse(items.Any(p => p.MpaaRating != "R"));
        }

        [TestMethod]
        public async Task GetItemsAsync_WithOrderBy_ReturnsItems()
        {
            var client = GetTestClient();
            var table = client.GetTable<Movie>();
            var query = new MobileTableQuery { OrderBy = "releaseDate" };
            var actual = table.GetItemsAsync(query);

            var items = new List<Movie>();
            await foreach (var item in actual)
            {
                items.Add(item);
            }

            Assert.AreEqual(248, items.Count());
            Assert.AreEqual("The Kid", items.First().Title);
        }

        [TestMethod]
        public async Task GetItemsAsync_WithSkipTop_ReturnsItems()
        {
            var client = GetTestClient();
            var table = client.GetTable<Movie>();
            var query = new MobileTableQuery { 
                OrderBy = "releaseDate",
                Skip = 5,
                Top = 5
            };
            var actual = table.GetItemsAsync(query);

            var items = new List<Movie>();
            await foreach (var item in actual)
            {
                items.Add(item);
            }

            Assert.AreEqual(5, items.Count());
            Assert.AreEqual("Nosferatu", items[0].Title);
            Assert.AreEqual("All Quiet on the Western Front", items[1].Title);
            Assert.AreEqual("City Lights", items[2].Title);
            Assert.AreEqual("M", items[3].Title);
            Assert.AreEqual("It Happened One Night", items[4].Title);
        }

        [TestMethod]
        public async Task GetItemsAsync_IncludeDeleted_ReturnsItems()
        {
            var client = GetTestClient();
            var table = client.GetTable<Movie>("tables/rmovies");
            var query = new MobileTableQuery
            {
                IncludeDeleted = true
            };
            var actual = table.GetItemsAsync(query);

            var items = new List<Movie>();
            await foreach (var item in actual)
            {
                items.Add(item);
            }

            Assert.AreEqual(248, items.Count());
            Assert.AreEqual(94, items.Where(p => p.Deleted).Count());
        }

        [TestMethod]
        public async Task GetItemsAsync_NotIncludeDeleted_ReturnsItems()
        {
            var client = GetTestClient();
            var table = client.GetTable<Movie>("tables/rmovies");
            var actual = table.GetItemsAsync();

            var items = new List<Movie>();
            await foreach (var item in actual)
            {
                items.Add(item);
            }

            Assert.AreEqual(154, items.Count());
            Assert.AreEqual(0, items.Where(p => p.Deleted).Count());
        }

        [TestMethod]
        public async Task GetItemsAsync_NotAuthorized_Returns404()
        {
            var client = GetTestClient();
            var table = client.GetTable<Movie>("tables/unauthorized");
            try
            {
                var actual = table.GetItemsAsync();
                var items = new List<Movie>();
                await foreach (var item in actual) { items.Add(item); }
                Assert.Fail("RequestFailedException expected");
            }
            catch (RequestFailedException ex)
            {
                Assert.AreEqual(404, ex.Status);
            }
        }

        [TestMethod]
        public async Task GetItemsAsync_MissingTable_Returns404()
        {
            var client = GetTestClient();
            var table = client.GetTable<Movie>("tables/missing_table");
            try
            {
                var actual = table.GetItemsAsync();
                var items = new List<Movie>();
                await foreach (var item in actual) { items.Add(item); }
                Assert.Fail("RequestFailedException expected");
            }
            catch (RequestFailedException ex)
            {
                Assert.AreEqual(404, ex.Status);
            }
        }
        #endregion

        #region GetItems
        [TestMethod]
        public void GetItems_ReturnsItems()
        {
            var client = GetTestClient();
            var table = client.GetTable<Movie>();
            var actual = table.GetItems();

            var items = new List<Movie>();
            foreach (var item in actual)
            {
                items.Add(item);
            }

            Assert.AreEqual(248, items.Count);
            CollectionAssert.AllItemsAreNotNull(items);
            CollectionAssert.AllItemsAreUnique(items);
        }

        [TestMethod]
        public void GetItems_WithFilter_ReturnsItems()
        {
            var client = GetTestClient();
            var table = client.GetTable<Movie>();
            var query = new MobileTableQuery { Filter = "mpaaRating eq 'R'" };
            var actual = table.GetItems(query);

            var items = new List<Movie>();
            foreach (var item in actual)
            {
                items.Add(item);
            }

            Assert.AreEqual(94, items.Count());
            Assert.IsFalse(items.Any(p => p.MpaaRating != "R"));
        }

        [TestMethod]
        public void GetItems_WithOrderBy_ReturnsItems()
        {
            var client = GetTestClient();
            var table = client.GetTable<Movie>();
            var query = new MobileTableQuery { OrderBy = "releaseDate" };
            var actual = table.GetItems(query);

            var items = new List<Movie>();
            foreach (var item in actual)
            {
                items.Add(item);
            }

            Assert.AreEqual(248, items.Count());
            Assert.AreEqual("The Kid", items.First().Title);
        }

        [TestMethod]
        public void GetItems_WithSkipTop_ReturnsItems()
        {
            var client = GetTestClient();
            var table = client.GetTable<Movie>();
            var query = new MobileTableQuery
            {
                OrderBy = "releaseDate",
                Skip = 5,
                Top = 5
            };
            var actual = table.GetItems(query);

            var items = new List<Movie>();
            foreach (var item in actual)
            {
                items.Add(item);
            }

            Assert.AreEqual(5, items.Count());
            Assert.AreEqual("Nosferatu", items[0].Title);
            Assert.AreEqual("All Quiet on the Western Front", items[1].Title);
            Assert.AreEqual("City Lights", items[2].Title);
            Assert.AreEqual("M", items[3].Title);
            Assert.AreEqual("It Happened One Night", items[4].Title);
        }

        [TestMethod]
        public void GetItems_IncludeDeleted_ReturnsItems()
        {
            var client = GetTestClient();
            var table = client.GetTable<Movie>("tables/rmovies");
            var query = new MobileTableQuery
            {
                IncludeDeleted = true
            };
            var actual = table.GetItems(query);

            var items = new List<Movie>();
            foreach (var item in actual)
            {
                items.Add(item);
            }

            Assert.AreEqual(248, items.Count());
            Assert.AreEqual(94, items.Where(p => p.Deleted).Count());
        }

        [TestMethod]
        public void GetItems_NotIncludeDeleted_ReturnsItems()
        {
            var client = GetTestClient();
            var table = client.GetTable<Movie>("tables/rmovies");
            var actual = table.GetItems();

            var items = new List<Movie>();
            foreach (var item in actual)
            {
                items.Add(item);
            }

            Assert.AreEqual(154, items.Count());
            Assert.AreEqual(0, items.Where(p => p.Deleted).Count());
        }

        [TestMethod]
        public void GetItems_NotAuthorized_Returns404()
        {
            var client = GetTestClient();
            var table = client.GetTable<Movie>("tables/unauthorized");
            try
            {
                var actual = table.GetItems();
                var items = actual.ToList();
                Assert.Fail("RequestFailedException expected");
            }
            catch (RequestFailedException ex)
            {
                Assert.AreEqual(404, ex.Status);
            }
        }

        [TestMethod]
        public void GetItems_MissingTable_Returns404()
        {
            var client = GetTestClient();
            var table = client.GetTable<Movie>("tables/missing_table");
            try
            {
                var actual = table.GetItems();
                var items = actual.ToList();
                Assert.Fail("RequestFailedException expected");
            }
            catch (RequestFailedException ex)
            {
                Assert.AreEqual(404, ex.Status);
            }
        }
        #endregion

        #region CreateItemAsync
        [TestMethod]
        public async Task CreateItemAsync_MissingItem_IsCreated()
        {
            var item = new Unit
            {
                Id = Guid.NewGuid().ToString("N"),
                Data = "create-item-missing-item-returns-201"
            };

            var client = GetTestClient();
            var table = client.GetTable<Unit>("tables/hunits");
            var actual = await table.InsertItemAsync(item);
            var response = actual.GetRawResponse();
            var storedItem = actual.Value;

            Assert.AreEqual(201, response.Status);

            Assert.AreEqual(item.Id, storedItem.Id);
            Assert.AreEqual(item.Data, storedItem.Data);
            Assert.IsNotNull(response.Headers.ETag);

            var dbItem = DbContext.HUnits.Where(m => m.Id == item.Id);
            Assert.AreEqual(1, dbItem.Count());
            Assert.AreEqual(item.Data, dbItem.First().Data);
        }

        [TestMethod]
        public async Task CreateItemAsync_NotAuthorized_Returns401()
        {
            var item = new Unit
            {
                Id = Guid.NewGuid().ToString("N"),
                Data = "create-item-missing-item-returns-201"
            };

            var client = GetTestClient();
            var table = client.GetTable<Unit>("tables/unauthorized");

            try
            {
                var actual = await table.InsertItemAsync(item);
                var response = actual.GetRawResponse();
                var storedItem = actual.Value;

                Assert.Fail("RequestFailedException expected");
            }
            catch (RequestFailedException ex)
            {
                Assert.AreEqual(401, ex.Status);
            }

            var dbItem = DbContext.HUnits.Where(m => m.Id == item.Id);
            Assert.AreEqual(0, dbItem.Count());
        }

        [TestMethod]
        public async Task CreateItemAsync_DoubleCreate_Returns409()
        {
            var item = new Unit
            {
                Id = Guid.NewGuid().ToString("N"),
                Data = "create-item-double-create-returns-409"
            };

            var client = GetTestClient();
            var table = client.GetTable<Unit>("tables/hunits");
            var actual = await table.InsertItemAsync(item);
            var response = actual.GetRawResponse();
            var storedItem = actual.Value;

            Assert.AreEqual(201, response.Status);

            Assert.AreEqual(item.Id, storedItem.Id);
            Assert.AreEqual(item.Data, storedItem.Data);
            Assert.IsNotNull(response.Headers.ETag);

            var dbItem = DbContext.HUnits.Where(m => m.Id == item.Id).FirstOrDefault();
            Assert.AreEqual(item.Data, dbItem.Data);

            try
            {
                actual = await table.InsertItemAsync(item);
                response = actual.GetRawResponse();
                storedItem = actual.Value;
                Assert.Fail("RequestFailedException expected");
            }
            catch (ConflictException<Unit> ex)
            {
                Assert.IsTrue(ex.Status == 409 || ex.Status == 412);
                Assert.IsNotNull(ex.Value);
                Assert.IsNotNull(ex.ETag);
                Assert.IsNotNull(ex.LastModified);
            }
        }

        [TestMethod]
        public async Task CreateItemAsync_MissingId_Returns201_AndCreatesId()
        {
            var item = new Unit
            {
                Data = "create-item-missing-id-returns-201"
            };

            var client = GetTestClient();
            var table = client.GetTable<Unit>("tables/hunits");
            var actual = await table.InsertItemAsync(item);
            var response = actual.GetRawResponse();
            var storedItem = actual.Value;

            Assert.AreEqual(201, response.Status);

            Assert.IsNotNull(storedItem.Id);
            Assert.IsTrue(storedItem.Id.Length > 0);
            Assert.AreEqual(item.Data, storedItem.Data);
            Assert.IsNotNull(response.Headers.ETag);

            var dbItem = DbContext.HUnits.Where(m => m.Id == storedItem.Id);
            Assert.AreEqual(1, dbItem.Count());
            Assert.AreEqual(item.Data, dbItem.First().Data);
        }
        #endregion

        #region CreateItem
        [TestMethod]
        public void CreateItem_MissingItem_IsCreated()
        {
            var item = new Unit
            {
                Id = Guid.NewGuid().ToString("N"),
                Data = "create-item-missing-item-returns-201"
            };

            var client = GetTestClient();
            var table = client.GetTable<Unit>("tables/hunits");
            var actual = table.InsertItem(item);
            var response = actual.GetRawResponse();
            var storedItem = actual.Value;

            Assert.AreEqual(201, response.Status);

            Assert.AreEqual(item.Id, storedItem.Id);
            Assert.AreEqual(item.Data, storedItem.Data);
            Assert.IsNotNull(response.Headers.ETag);

            var dbItem = DbContext.HUnits.Where(m => m.Id == item.Id);
            Assert.AreEqual(1, dbItem.Count());
            Assert.AreEqual(item.Data, dbItem.First().Data);
        }

        [TestMethod]
        public void CreateItem_NotAuthorized_Returns401()
        {
            var item = new Unit
            {
                Id = Guid.NewGuid().ToString("N"),
                Data = "create-item-missing-item-returns-201"
            };

            var client = GetTestClient();
            var table = client.GetTable<Unit>("tables/unauthorized");

            try
            {
                var actual = table.InsertItem(item);
                var response = actual.GetRawResponse();
                var storedItem = actual.Value;

                Assert.Fail("RequestFailedException expected");
            }
            catch (RequestFailedException ex)
            {
                Assert.AreEqual(401, ex.Status);
            }

            var dbItem = DbContext.HUnits.Where(m => m.Id == item.Id);
            Assert.AreEqual(0, dbItem.Count());
        }

        [TestMethod]
        public void CreateItem_DoubleCreate_Returns409()
        {
            var item = new Unit
            {
                Id = Guid.NewGuid().ToString("N"),
                Data = "create-item-double-create-returns-409"
            };

            var client = GetTestClient();
            var table = client.GetTable<Unit>("tables/hunits");
            var actual = table.InsertItem(item);
            var response = actual.GetRawResponse();
            var storedItem = actual.Value;

            Assert.AreEqual(201, response.Status);

            Assert.AreEqual(item.Id, storedItem.Id);
            Assert.AreEqual(item.Data, storedItem.Data);
            Assert.IsNotNull(response.Headers.ETag);

            var dbItem = DbContext.HUnits.Where(m => m.Id == item.Id).FirstOrDefault();
            Assert.AreEqual(item.Data, dbItem.Data);

            try
            {
                actual = table.InsertItem(item);
                response = actual.GetRawResponse();
                storedItem = actual.Value;
                Assert.Fail("RequestFailedException expected");
            }
            catch (ConflictException<Unit> ex)
            {
                Assert.IsTrue(ex.Status == 409 || ex.Status == 412);
                Assert.IsNotNull(ex.Value);
                Assert.IsNotNull(ex.ETag);
                Assert.IsNotNull(ex.LastModified);
            }
        }

        [TestMethod]
        public void CreateItem_MissingId_Returns201_AndCreatesId()
        {
            var item = new Unit
            {
                Data = "create-item-missing-id-returns-201"
            };

            var client = GetTestClient();
            var table = client.GetTable<Unit>("tables/hunits");
            var actual = table.InsertItem(item);
            var response = actual.GetRawResponse();
            var storedItem = actual.Value;

            Assert.AreEqual(201, response.Status);

            Assert.IsNotNull(storedItem.Id);
            Assert.IsTrue(storedItem.Id.Length > 0);
            Assert.AreEqual(item.Data, storedItem.Data);
            Assert.IsNotNull(response.Headers.ETag);

            var dbItem = DbContext.HUnits.Where(m => m.Id == storedItem.Id);
            Assert.AreEqual(1, dbItem.Count());
            Assert.AreEqual(item.Data, dbItem.First().Data);
        }
        #endregion

        #region DeleteItemAsync
        [TestMethod]
        public async Task DeleteItemAsync_SoftDelete_Existing_Returns204()
        {
            var client = GetTestClient();
            var table = client.GetTable<Unit>("tables/sunits");
            Unit item = table.GetItem("sunit-1");
            Assert.IsNotNull(item);

            var actual = await table.DeleteItemAsync(item);
            Assert.AreEqual(204, actual.Status);

            var dbItems = DbContext.SUnits.Where(e => e.Id == item.Id && e.Deleted);
            Assert.IsNotNull(dbItems.First());
            Assert.AreEqual(1, dbItems.Count());
        }

        [TestMethod]
        public async Task DeleteItemAsync_HardDelete_Existing_Returns204()
        {
            var client = GetTestClient();
            var table = client.GetTable<Unit>("tables/hunits");
            Unit item = table.GetItem("hunit-1");
            Assert.IsNotNull(item);

            var actual = await table.DeleteItemAsync(item);
            Assert.AreEqual(204, actual.Status);

            var dbItems = DbContext.SUnits.Where(e => e.Id == item.Id);
            Assert.AreEqual(0, dbItems.Count());
        }

        [TestMethod]
        public async Task DeleteItemAsync_SoftDelete_Missing_Returns404()
        {
            var client = GetTestClient();
            var table = client.GetTable<Unit>("tables/sunits");
            var item = new Unit { Id = "missing" };

            try
            {
                var actual = await table.DeleteItemAsync(item);
                Assert.Fail("RequestFailedException expected");
            }
            catch (RequestFailedException ex)
            {
                Assert.AreEqual(404, ex.Status);
            }
        }

        [TestMethod]
        public async Task DeleteItemAsync_HardDelete_Missing_Returns404()
        {
            var client = GetTestClient();
            var table = client.GetTable<Unit>("tables/hunits");
            var item = new Unit { Id = "missing" };

            try
            {
                var actual = await table.DeleteItemAsync(item);
                Assert.Fail("RequestFailedException expected");
            }
            catch (RequestFailedException ex)
            {
                Assert.AreEqual(404, ex.Status);
            }
        }

        [TestMethod]
        public async Task DeleteItemAsync_SoftDelete_Twice_Returns404()
        {
            var item = new Unit { Id = "sunit-2" };

            var client = GetTestClient();
            var table = client.GetTable<Unit>("tables/sunits");
            var actual = await table.DeleteItemAsync(item);
            Assert.AreEqual(204, actual.Status);

            try
            {
                actual = await table.DeleteItemAsync(item);
                Assert.Fail("RequestFailedException expected");
            }
            catch (RequestFailedException ex)
            {
                Assert.AreEqual(404, ex.Status);
            }

            var dbItems = DbContext.SUnits.Where(e => e.Id == item.Id && e.Deleted);
            Assert.IsNotNull(dbItems.First());
            Assert.AreEqual(1, dbItems.Count());
        }

        [TestMethod]
        public async Task DeleteItemAsync_HardDelete_Twice_Returns404()
        {
            var item = new Unit { Id = "hunit-2" };
            var options = new MatchConditions();
            var client = GetTestClient();
            var table = client.GetTable<Unit>("tables/hunits");
            var actual = await table.DeleteItemAsync(item, options);
            Assert.AreEqual(204, actual.Status);

            try
            {
                actual = await table.DeleteItemAsync(item);
                Assert.Fail("RequestFailedException expected");
            }
            catch (RequestFailedException ex)
            {
                Assert.AreEqual(404, ex.Status);
            }
        }

        [TestMethod]
        public async Task DeleteItemAsync_Unauthorized_Returns404()
        {
            var item = new Movie { Id = "movie-3" };

            var client = GetTestClient();
            var table = client.GetTable<Movie>("tables/unauthorized");

            try
            {
                await table.DeleteItemAsync(item);
                Assert.Fail("RequestFailedException expected");
            }
            catch (RequestFailedException ex)
            {
                Assert.AreEqual(404, ex.Status);
            }
        }

        [TestMethod]
        public async Task DeleteItemAsync_SoftDelete_PreconditionsFail_Returns412()
        {
            var client = GetTestClient();
            var table = client.GetTable<Unit>("tables/sunits");
            Unit item = table.GetItem("sunit-3");
            MatchConditions options = new MatchConditions { IfNoneMatch = ETag.All };
            Assert.IsNotNull(item);

            try
            {
                await table.DeleteItemAsync(item, options);
            }
            catch (ConflictException<Unit> ex)
            {
                Assert.AreEqual(412, ex.Status);
                Assert.IsNotNull(ex.Value);
                Assert.IsNotNull(ex.ETag);
                Assert.IsNotNull(ex.LastModified);
            }
        }

        [TestMethod]
        public async Task DeleteItemAsync_HardDelete_PreconditionsFail_Returns412()
        {
            var client = GetTestClient();
            var table = client.GetTable<Unit>("tables/hunits");
            Unit item = table.GetItem("hunit-3");
            MatchConditions options = new MatchConditions { IfNoneMatch = ETag.All };
            Assert.IsNotNull(item);

            try
            {
                await table.DeleteItemAsync(item, options);
            }
            catch (ConflictException<Unit> ex)
            {
                Assert.AreEqual(412, ex.Status);
                Assert.IsNotNull(ex.Value);
                Assert.IsNotNull(ex.ETag);
                Assert.IsNotNull(ex.LastModified);
            }
        }

        [TestMethod]
        public async Task DeleteItemAsync_SoftDelete_PreconditionsSucceed_Returns204()
        {
            var client = GetTestClient();
            var table = client.GetTable<Unit>("tables/sunits");
            Unit item = table.GetItem("sunit-4");
            MatchConditions options = new MatchConditions { IfMatch = ETag.All };
            Assert.IsNotNull(item);

            var actual = await table.DeleteItemAsync(item);
            Assert.AreEqual(204, actual.Status);

            var dbItems = DbContext.SUnits.Where(e => e.Id == item.Id && e.Deleted);
            Assert.IsNotNull(dbItems.First());
            Assert.AreEqual(1, dbItems.Count());
        }

        [TestMethod]
        public async Task DeleteItemAsync_HardDelete_PreconditionsSucceed_Returns204()
        {
            var client = GetTestClient();
            var table = client.GetTable<Unit>("tables/hunits");
            Unit item = table.GetItem("hunit-4");
            MatchConditions options = new MatchConditions { IfMatch = ETag.All };
            Assert.IsNotNull(item);

            var actual = await table.DeleteItemAsync(item);
            Assert.AreEqual(204, actual.Status);

            var dbItems = DbContext.SUnits.Where(e => e.Id == item.Id);
            Assert.AreEqual(0, dbItems.Count());
        }
        #endregion

        #region DeleteItem
        [TestMethod]
        public void DeleteItem_SoftDelete_Existing_Returns204()
        {
            var client = GetTestClient();
            var table = client.GetTable<Unit>("tables/sunits");
            Unit item = table.GetItem("sunit-1");
            Assert.IsNotNull(item);

            var actual = table.DeleteItem(item);
            Assert.AreEqual(204, actual.Status);

            var dbItems = DbContext.SUnits.Where(e => e.Id == item.Id && e.Deleted);
            Assert.IsNotNull(dbItems.First());
            Assert.AreEqual(1, dbItems.Count());
        }

        [TestMethod]
        public void DeleteItem_HardDelete_Existing_Returns204()
        {
            var client = GetTestClient();
            var table = client.GetTable<Unit>("tables/hunits");
            Unit item = table.GetItem("hunit-1");
            Assert.IsNotNull(item);

            var actual = table.DeleteItem(item);
            Assert.AreEqual(204, actual.Status);

            var dbItems = DbContext.SUnits.Where(e => e.Id == item.Id);
            Assert.AreEqual(0, dbItems.Count());
        }

        [TestMethod]
        public void DeleteItem_SoftDelete_Missing_Returns404()
        {
            var client = GetTestClient();
            var table = client.GetTable<Unit>("tables/sunits");
            var item = new Unit { Id = "missing" };

            try
            {
                var actual = table.DeleteItem(item);
                Assert.Fail("RequestFailedException expected");
            }
            catch (RequestFailedException ex)
            {
                Assert.AreEqual(404, ex.Status);
            }
        }

        [TestMethod]
        public void DeleteItem_HardDelete_Missing_Returns404()
        {
            var client = GetTestClient();
            var table = client.GetTable<Unit>("tables/hunits");
            var item = new Unit { Id = "missing" };

            try
            {
                var actual = table.DeleteItem(item);
                Assert.Fail("RequestFailedException expected");
            }
            catch (RequestFailedException ex)
            {
                Assert.AreEqual(404, ex.Status);
            }
        }

        [TestMethod]
        public void DeleteItem_SoftDelete_Twice_Returns404()
        {
            var item = new Unit { Id = "sunit-2" };

            var client = GetTestClient();
            var table = client.GetTable<Unit>("tables/sunits");
            var actual = table.DeleteItem(item);
            Assert.AreEqual(204, actual.Status);

            try
            {
                actual = table.DeleteItem(item);
                Assert.Fail("RequestFailedException expected");
            }
            catch (RequestFailedException ex)
            {
                Assert.AreEqual(404, ex.Status);
            }

            var count = DbContext.SUnits.Count(e => e.Id == item.Id && e.Deleted);
            Assert.AreEqual(1, count);
        }

        [TestMethod]
        public void DeleteItem_HardDelete_Twice_Returns404()
        {
            var item = new Unit { Id = "hunit-2" };
            var options = new MatchConditions();

            var client = GetTestClient();
            var table = client.GetTable<Unit>("tables/hunits");
            var actual = table.DeleteItem(item, options);
            Assert.AreEqual(204, actual.Status);

            try
            {
                actual = table.DeleteItem(item);
                Assert.Fail("RequestFailedException expected");
            }
            catch (RequestFailedException ex)
            {
                Assert.AreEqual(404, ex.Status);
            }
        }

        [TestMethod]
        public void DeleteItem_Unauthorized_Returns404()
        {
            var item = new Movie { Id = "movie-3" };

            var client = GetTestClient();
            var table = client.GetTable<Movie>("tables/unauthorized");

            try
            {
                table.DeleteItem(item);
                Assert.Fail("RequestFailedException expected");
            }
            catch (RequestFailedException ex)
            {
                Assert.AreEqual(404, ex.Status);
            }
        }

        [TestMethod]
        public void DeleteItem_SoftDelete_PreconditionsFail_Returns412()
        {
            var client = GetTestClient();
            var table = client.GetTable<Unit>("tables/sunits");
            Unit item = table.GetItem("sunit-3");
            MatchConditions options = new MatchConditions { IfNoneMatch = ETag.All };
            Assert.IsNotNull(item);

            try
            {
                table.DeleteItem(item, options);
            }
            catch (ConflictException<Unit> ex)
            {
                Assert.AreEqual(412, ex.Status);
                Assert.IsNotNull(ex.Value);
                Assert.IsNotNull(ex.ETag);
                Assert.IsNotNull(ex.LastModified);
            }
        }

        [TestMethod]
        public void DeleteItem_HardDelete_PreconditionsFail_Returns412()
        {
            var client = GetTestClient();
            var table = client.GetTable<Unit>("tables/hunits");
            Unit item = table.GetItem("hunit-3");
            MatchConditions options = new MatchConditions { IfNoneMatch = ETag.All };
            Assert.IsNotNull(item);

            try
            {
                table.DeleteItem(item, options);
            }
            catch (ConflictException<Unit> ex)
            {
                Assert.AreEqual(412, ex.Status);
                Assert.IsNotNull(ex.Value);
                Assert.IsNotNull(ex.ETag);
                Assert.IsNotNull(ex.LastModified);
            }
        }

        [TestMethod]
        public void DeleteItem_SoftDelete_PreconditionsSucceed_Returns204()
        {
            var client = GetTestClient();
            var table = client.GetTable<Unit>("tables/sunits");
            Unit item = table.GetItem("sunit-4");
            MatchConditions options = new MatchConditions { IfMatch = ETag.All };
            Assert.IsNotNull(item);

            var actual = table.DeleteItem(item);
            Assert.AreEqual(204, actual.Status);

            var dbItems = DbContext.SUnits.Where(e => e.Id == item.Id && e.Deleted);
            Assert.IsNotNull(dbItems.First());
            Assert.AreEqual(1, dbItems.Count());
        }

        [TestMethod]
        public void DeleteItem_HardDelete_PreconditionsSucceed_Returns204()
        {
            var client = GetTestClient();
            var table = client.GetTable<Unit>("tables/hunits");
            Unit item = table.GetItem("hunit-4");
            MatchConditions options = new MatchConditions { IfMatch = ETag.All };
            Assert.IsNotNull(item);

            var actual = table.DeleteItem(item);
            Assert.AreEqual(204, actual.Status);

            var dbItems = DbContext.SUnits.Where(e => e.Id == item.Id);
            Assert.AreEqual(0, dbItems.Count());
        }
        #endregion

        #region ReplaceItemAsync
        [TestMethod]
        public async Task ReplaceItemAsync_ValidId_Returns200()
        {
            var client = GetTestClient();
            var table = client.GetTable<Unit>("tables/hunits");
            Unit item = table.GetItem("hunit-6");
            item.Data = "Replaced";

            var actual = await table.ReplaceItemAsync(item);
            var response = actual.GetRawResponse();
            var serverItem = actual.Value;

            Assert.AreEqual(200, response.Status);
            Assert.AreEqual(item.Id, serverItem.Id);
            Assert.AreEqual(item.Data, serverItem.Data);

            var dbItem = DbContext.HUnits.Where(t => t.Id == item.Id);
            Assert.AreEqual(1, dbItem.Count());
            Assert.AreEqual("Replaced", dbItem.First().Data);
        }

        [TestMethod]
        public async Task ReplaceItemAsync_MismatchedVersion_Returns412()
        {
            var client = GetTestClient();
            var table = client.GetTable<Unit>("tables/hunits");
            Unit item = table.GetItem("hunit-6");
            item.Data = "Replaced";
            var version = item.Version;
            item.Version = Convert.ToBase64String(Encoding.UTF8.GetBytes("also-replaced"));

            try
            {
                var actual = await table.ReplaceItemAsync(item);
                var response = actual.GetRawResponse();
                var serverItem = actual.Value;
                Assert.Fail("RequestFailedException expected");
            }
            catch (ConflictException<Unit> ex)
            {
                Assert.AreEqual(412, ex.Status);
                Assert.AreEqual(version, ex.ETag.ToString());
            }
        }

        [TestMethod]
        public async Task ReplaceItemAsync_MissingItem_Returns404()
        {
            var client = GetTestClient();
            var table = client.GetTable<Unit>("tables/hunits");
            Unit item = new Unit { Id = "missing", Data = "Replaced" };

            try
            {
                var actual = await table.ReplaceItemAsync(item);
                var response = actual.GetRawResponse();
                var serverItem = actual.Value;
                Assert.Fail("RequestFailedException expected");
            }
            catch (RequestFailedException ex)
            {
                Assert.AreEqual(404, ex.Status);
            }
        }

        [TestMethod]
        public async Task ReplaceItemAsync_SoftDelete_DeletedItem_Returns404()
        {
            var client = GetTestClient();
            var table = client.GetTable<Unit>("tables/sunits");
            Unit item = table.GetItem("sunit-7");
            table.DeleteItem(item);

            item.Data = "Replaced";
            try
            {
                var actual = await table.ReplaceItemAsync(item);
                var response = actual.GetRawResponse();
                var serverItem = actual.Value;
                Assert.Fail("RequestFailedException expected");
            }
            catch (RequestFailedException ex)
            {
                Assert.AreEqual(404, ex.Status);
            }
        }

        [TestMethod]
        public async Task ReplaceItemAsync_Unauthorized_Returns404()
        {
            var client = GetTestClient();
            var table = client.GetTable<Movie>("tables/unauthorized");
            var item = new Movie { Id = "movie-8", Title = "Foo" };

            try
            {
                var actual = await table.ReplaceItemAsync(item);
                var response = actual.GetRawResponse();
                var serverItem = actual.Value;
                Assert.Fail("RequestFailedException expected");
            }
            catch (RequestFailedException ex)
            {
                Assert.AreEqual(404, ex.Status);
            }
        }

        [TestMethod]
        public async Task ReplaceItemAsync_PreconditionsFail_Returns412()
        {
            var client = GetTestClient();
            var table = client.GetTable<Unit>("tables/hunits");
            Unit item = table.GetItem("hunit-6");
            item.Data = "Replaced";

            var options = new MatchConditions { IfNoneMatch = new ETag(item.Version) };
            try
            {
                var actual = await table.ReplaceItemAsync(item, options);
                var response = actual.GetRawResponse();
                var serverItem = actual.Value;
            }
            catch (ConflictException<Unit> ex)
            {
                Assert.AreEqual(412, ex.Status);
                Assert.AreEqual(item.Version, ex.ETag.ToString());
                Assert.IsNotNull(ex.Value);
            }
        }

        [TestMethod]
        public async Task ReplaceItemAsync_PreconditionsSucceed_Returns200()
        {
            var client = GetTestClient();
            var table = client.GetTable<Unit>("tables/hunits");
            Unit item = await table.GetItemAsync("hunit-6");
            item.Data = "Replaced";
            var options = new MatchConditions { IfMatch = new ETag(item.Version) };

            var actual = await table.ReplaceItemAsync(item, options);
            var response = actual.GetRawResponse();
            var serverItem = actual.Value;

            Assert.AreEqual(200, response.Status);
            Assert.AreEqual(item.Id, serverItem.Id);
            Assert.AreEqual(item.Data, serverItem.Data);

            var dbItem = DbContext.HUnits.Where(t => t.Id == item.Id);
            Assert.AreEqual(1, dbItem.Count());
            Assert.AreEqual("Replaced", dbItem.First().Data);
        }
        #endregion

        #region ReplaceItem
        [TestMethod]
        public void ReplaceItem_ValidId_Returns200()
        {
            var client = GetTestClient();
            var table = client.GetTable<Unit>("tables/hunits");
            Unit item = table.GetItem("hunit-6");
            item.Data = "Replaced";

            var actual = table.ReplaceItem(item);
            var response = actual.GetRawResponse();
            var serverItem = actual.Value;

            Assert.AreEqual(200, response.Status);
            Assert.AreEqual(item.Id, serverItem.Id);
            Assert.AreEqual(item.Data, serverItem.Data);

            var dbItem = DbContext.HUnits.Where(t => t.Id == item.Id);
            Assert.AreEqual(1, dbItem.Count());
            Assert.AreEqual("Replaced", dbItem.First().Data);
        }

        [TestMethod]
        public void ReplaceItem_MismatchedVersion_Returns412()
        {
            var client = GetTestClient();
            var table = client.GetTable<Unit>("tables/hunits");
            Unit item = table.GetItem("hunit-6");
            item.Data = "Replaced";
            var version = item.Version;
            item.Version = Convert.ToBase64String(Encoding.UTF8.GetBytes("also-replaced"));

            try
            {
                var actual = table.ReplaceItem(item);
                var response = actual.GetRawResponse();
                var serverItem = actual.Value;
                Assert.Fail("RequestFailedException expected");
            }
            catch (ConflictException<Unit> ex)
            {
                Assert.AreEqual(412, ex.Status);
                Assert.AreEqual(version, ex.ETag.ToString());
            }
        }

        [TestMethod]
        public void ReplaceItem_MissingItem_Returns404()
        {
            var client = GetTestClient();
            var table = client.GetTable<Unit>("tables/hunits");
            Unit item = new Unit { Id = "missing", Data = "Replaced" };

            try
            {
                var actual = table.ReplaceItem(item);
                var response = actual.GetRawResponse();
                var serverItem = actual.Value;
                Assert.Fail("RequestFailedException expected");
            }
            catch (RequestFailedException ex)
            {
                Assert.AreEqual(404, ex.Status);
            }
        }

        [TestMethod]
        public void ReplaceItem_SoftDelete_DeletedItem_Returns404()
        {
            var client = GetTestClient();
            var table = client.GetTable<Unit>("tables/sunits");
            Unit item = table.GetItem("sunit-7");
            table.DeleteItem(item);

            item.Data = "Replaced";
            try
            {
                var actual = table.ReplaceItem(item);
                var response = actual.GetRawResponse();
                var serverItem = actual.Value;
                Assert.Fail("RequestFailedException expected");
            }
            catch (RequestFailedException ex)
            {
                Assert.AreEqual(404, ex.Status);
            }
        }

        [TestMethod]
        public void ReplaceItem_Unauthorized_Returns404()
        {
            var client = GetTestClient();
            var table = client.GetTable<Movie>("tables/unauthorized");
            var item = new Movie { Id = "movie-8", Title = "Foo" };

            try
            {
                var actual = table.ReplaceItem(item);
                var response = actual.GetRawResponse();
                var serverItem = actual.Value;
                Assert.Fail("RequestFailedException expected");
            }
            catch (RequestFailedException ex)
            {
                Assert.AreEqual(404, ex.Status);
            }
        }

        [TestMethod]
        public void ReplaceItem_PreconditionsFail_Returns412()
        {
            var client = GetTestClient();
            var table = client.GetTable<Unit>("tables/hunits");
            Unit item = table.GetItem("hunit-6");
            item.Data = "Replaced";

            var options = new MatchConditions { IfNoneMatch = new ETag(item.Version) };
            try
            {
                var actual = table.ReplaceItem(item, options);
                var response = actual.GetRawResponse();
                var serverItem = actual.Value;
            }
            catch (ConflictException<Unit> ex)
            {
                Assert.AreEqual(412, ex.Status);
                Assert.AreEqual(item.Version, ex.ETag.ToString());
                Assert.IsNotNull(ex.Value);
            }
        }

        [TestMethod]
        public void ReplaceItem_PreconditionsSucceed_Returns200()
        {
            var client = GetTestClient();
            var table = client.GetTable<Unit>("tables/hunits");
            Unit item = table.GetItem("hunit-6");
            item.Data = "Replaced";
            var options = new MatchConditions { IfMatch = new ETag(item.Version) };

            var actual = table.ReplaceItem(item, options);
            var response = actual.GetRawResponse();
            var serverItem = actual.Value;

            Assert.AreEqual(200, response.Status);
            Assert.AreEqual(item.Id, serverItem.Id);
            Assert.AreEqual(item.Data, serverItem.Data);

            var dbItem = DbContext.HUnits.Where(t => t.Id == item.Id);
            Assert.AreEqual(1, dbItem.Count());
            Assert.AreEqual("Replaced", dbItem.First().Data);
        }
        #endregion
    }
}
