using Azure.Mobile.Server.Test.Helpers;
using E2EServer.DataObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Azure.Mobile.Server.Test.TableController
{
    [TestClass]
    public class List_Tests : Base_Test
    {
        [TestMethod]
        public async Task GetItems_ReturnsSomeItems()
        {
            var response = await SendRequestToServer<E2EServer.DataObjects.Movie>(HttpMethod.Get, "/tables/movies", null);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var actual = await GetValueFromResponse<List<E2EServer.DataObjects.Movie>>(response);
            Assert.IsNotNull(actual);
            CollectionAssert.AllItemsAreNotNull(actual);
            CollectionAssert.AllItemsAreUnique(actual);
            Assert.AreEqual(50, actual.Count);
        }

        [TestMethod]
        public async Task GetItems_SoftDelete_DoesNotIncludeDeletedItems()
        {
            var deleteResponse = await SendRequestToServer<SUnit>(HttpMethod.Delete, "/tables/sunits/sunit-14", null);
            Assert.AreEqual(HttpStatusCode.NoContent, deleteResponse.StatusCode);

            var response = await SendRequestToServer<SUnit>(HttpMethod.Get, "/tables/sunits", null);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            var actual = await GetValueFromResponse<List<SUnit>>(response);

            Assert.IsFalse(actual.Where(item => item.Deleted == true).Any());
        }

        [TestMethod]
        public async Task GetItems_SoftDelete_CanIncludeDeletedItems()
        {
            var deleteResponse = await SendRequestToServer<SUnit>(HttpMethod.Delete, "/tables/sunits/sunit-2", null);
            var firstResponse = await SendRequestToServer<SUnit>(HttpMethod.Get, "/tables/sunits", null);
            Assert.AreEqual(HttpStatusCode.OK, firstResponse.StatusCode);
            var firstActual = await GetValueFromResponse<List<SUnit>>(firstResponse);

            var secondResponse = await SendRequestToServer<SUnit>(HttpMethod.Get, "/tables/sunits?__includedeleted=true", null);
            Assert.AreEqual(HttpStatusCode.OK, secondResponse.StatusCode);
            var secondActual = await GetValueFromResponse<List<SUnit>>(secondResponse);

            Assert.IsTrue(secondActual.Count > firstActual.Count);
            Assert.IsTrue(secondActual.Where(item => item.Deleted == true).Any());
        }

        [TestMethod]
        public async Task GetItems_Unauthorzed_Returns404()
        {
            var response = await SendRequestToServer<E2EServer.DataObjects.Movie>(HttpMethod.Get, "/tables/unauthorized", null);
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task GetItems_MaxTop_ReturnsNItems()
        {
            var response = await SendRequestToServer<E2EServer.DataObjects.Movie>(HttpMethod.Get, "/tables/movies?$top=5", null);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var actual = await GetValueFromResponse<List<E2EServer.DataObjects.Movie>>(response);
            Assert.IsNotNull(actual);
            CollectionAssert.AllItemsAreNotNull(actual);
            CollectionAssert.AllItemsAreUnique(actual);
            Assert.AreEqual(5, actual.Count);
        }
    }
}
