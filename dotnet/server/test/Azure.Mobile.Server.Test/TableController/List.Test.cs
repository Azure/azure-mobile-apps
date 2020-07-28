using Azure.Mobile.Server.Test.Helpers;
using E2EServer.DataObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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
            var response = await SendRequestToServer<E2EServer.DataObjects.Movie>(HttpMethod.Get, "/tables/movies?$count=true", null);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var actual = await GetValueFromResponse<PagedList<E2EServer.DataObjects.Movie>>(response);
            Assert.IsNotNull(actual);
            CollectionAssert.AllItemsAreNotNull(actual.Values);
            CollectionAssert.AllItemsAreUnique(actual.Values);
            Assert.AreEqual(50, actual.Values.Count);
            Assert.IsNotNull(actual.NextLink);

            // TODO: This does not work right now because GetEntityCount() information in GetItems() . 
            //Assert.IsNotNull(actual.Count);
            //Assert.AreEqual(248, actual.Count);
        }

        [TestMethod]
        public async Task GetItems_SoftDelete_DoesNotIncludeDeletedItems()
        {
            var deleteResponse = await SendRequestToServer<SUnit>(HttpMethod.Delete, "/tables/sunits/sunit-14", null);
            Assert.AreEqual(HttpStatusCode.NoContent, deleteResponse.StatusCode);

            var response = await SendRequestToServer<SUnit>(HttpMethod.Get, "/tables/sunits", null);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            var actual = await GetValueFromResponse<PagedList<SUnit>>(response);

            Assert.IsFalse(actual.Values.Where(item => item.Deleted == true).Any());
        }

        [TestMethod]
        public async Task GetItems_SoftDelete_CanIncludeDeletedItems()
        {
            var deleteResponse = await SendRequestToServer<SUnit>(HttpMethod.Delete, "/tables/sunits/sunit-2", null);
            var firstResponse = await SendRequestToServer<SUnit>(HttpMethod.Get, "/tables/sunits", null);
            Assert.AreEqual(HttpStatusCode.OK, firstResponse.StatusCode);
            var firstActual = await GetValueFromResponse<PagedList<SUnit>>(firstResponse);

            var secondResponse = await SendRequestToServer<SUnit>(HttpMethod.Get, "/tables/sunits?__includedeleted=true&$top=500", null);
            Assert.AreEqual(HttpStatusCode.OK, secondResponse.StatusCode);
            var secondActual = await GetValueFromResponse<PagedList<SUnit>>(secondResponse);

            //Assert.IsTrue(secondActual.Count > firstActual.Count);
            Assert.IsTrue(secondActual.Values.Where(item => item.Deleted == true).Any());
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

            var actual = await GetValueFromResponse<PagedList<E2EServer.DataObjects.Movie>>(response);
            Assert.IsNotNull(actual);
            CollectionAssert.AllItemsAreNotNull(actual.Values);
            CollectionAssert.AllItemsAreUnique(actual.Values);
            Assert.AreEqual(5, actual.Values.Count);
        }
    }
}
