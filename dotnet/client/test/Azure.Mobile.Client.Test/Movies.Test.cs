using Azure.Mobile.Client.Table;
using Azure.Mobile.Client.Test.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Azure.Mobile.Client.Test
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

    [TestClass]
    public class Movies : BaseTest
    {
        [TestMethod]
        public void MobileDataClient_IsNotNull()
        {
            var client = GetTestClient();
            Assert.IsNotNull(client);
            Assert.AreEqual("https://localhost:5001/", client.Endpoint.ToString());
        }

        [TestMethod]
        public void MobileDataClient_CanGetTable_NoPath()
        {
            var client = GetTestClient();
            var table = client.GetTable<Movie>();
            Assert.IsNotNull(table);
            Assert.IsInstanceOfType(table, typeof(MobileDataTable<Movie>));
            Assert.AreEqual("https://localhost:5001/tables/movies", table.Endpoint.ToString());
        }

        [TestMethod]
        public void MobileDataClient_CanGetTable_SpecifiedPath()
        {
            var client = GetTestClient();
            var table = client.GetTable<Movie>("tables/movies");
            Assert.IsNotNull(table);
            Assert.IsInstanceOfType(table, typeof(MobileDataTable<Movie>));
            Assert.AreEqual("https://localhost:5001/tables/movies", table.Endpoint.ToString());
        }

        [TestMethod]
        public async Task MobileDataTable_CanFetchItem_Async()
        {
            var client = GetTestClient();
            var table = client.GetTable<Movie>();
            var item = await table.GetItemAsync("movie-3");
            Assert.IsNotNull(item);
            var response = item.GetRawResponse();
            Assert.AreEqual(200, response.Status);
            Assert.AreEqual("Pulp Fiction", item.Value.Title);
        }

        [TestMethod]
        public void MobileDataTable_CanFetchItem_Sync()
        {
            var client = GetTestClient();
            var table = client.GetTable<Movie>();
            var item = table.GetItem("movie-3");
            Assert.IsNotNull(item);
            var response = item.GetRawResponse();
            Assert.AreEqual(200, response.Status);
            Assert.AreEqual("Pulp Fiction", item.Value.Title);
        }

        [TestMethod]
        public async Task MobileDataTable_CanFetchList_Async()
        {
            var client = GetTestClient();
            var table = client.GetTable<Movie>();
            var actual = table.GetItemsAsync();
            Assert.IsNotNull(actual);
            var items = new List<Movie>();
            await foreach (Movie item in actual)
            {
                items.Add(item);
            }

            Assert.AreEqual(248, items.Count);
            CollectionAssert.AllItemsAreNotNull(items);
            CollectionAssert.AllItemsAreUnique(items);
        }

        [TestMethod]
        public void MobileDataTable_CanFetchList_Sync()
        {
            var client = GetTestClient();
            var table = client.GetTable<Movie>();
            var actual = table.GetItems();
            Assert.IsNotNull(actual);
            var items = new List<Movie>();
             foreach (Movie item in actual)
            {
                items.Add(item);
            }

            Assert.AreEqual(248, items.Count);
            CollectionAssert.AllItemsAreNotNull(items);
            CollectionAssert.AllItemsAreUnique(items);
        }
    }
}
