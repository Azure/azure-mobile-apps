using Azure.Mobile.Client.Table;
using Azure.Mobile.Client.Test.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
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
    /// This is the client-side of the Movie DTO from E2EServer
    /// </summary>
    public class RMovie : TableData
    {
        public string Title { get; set; }
        public int Duration { get; set; }
        public string MpaaRating { get; set; }
        public DateTime ReleaseDate { get; set; }
        public bool BestPictureWinner { get; set; }
        public int Year { get; set; }
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
            var table = client.GetTable<RMovie>();
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
            var table = client.GetTable<RMovie>();
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
            var table = client.GetTable<RMovie>();
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
            var table = client.GetTable<RMovie>();
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
            var table = client.GetTable<RMovie>();
            var query = new MobileTableQuery
            {
                IncludeDeleted = true
            };
            var actual = table.GetItemsAsync(query);

            var items = new List<RMovie>();
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
            var table = client.GetTable<RMovie>();
            var actual = table.GetItemsAsync();

            var items = new List<RMovie>();
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
            var table = client.GetTable<RMovie>();
            var query = new MobileTableQuery
            {
                IncludeDeleted = true
            };
            var actual = table.GetItems(query);

            var items = new List<RMovie>();
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
            var table = client.GetTable<RMovie>();
            var actual = table.GetItems();

            var items = new List<RMovie>();
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
    }
}
