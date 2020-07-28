using Azure.Mobile.Server.Test.E2EServer.DataObjects;
using Azure.Mobile.Server.Test.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Azure.Mobile.Server.Test.TableController
{
    [TestClass]
    public class Delete_Tests : Base_Test
    {
        [TestMethod]
        public async Task DeleteItem_SoftDelete_Existing_Returns204()
        {
            var response = await SendRequestToServer<SUnit>(HttpMethod.Delete, "/tables/sunits/sunit-1", null);
            Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);

            // Check that the database has set the record to Deleted = true
            var item = GetItemFromDb<SUnit>("sunit-1");
            Assert.IsNotNull(item);
            Assert.IsTrue(item.Deleted);
        }

        [TestMethod]
        public async Task DeleteItem_HardDelete_Existing_Returns204()
        {
            var response = await SendRequestToServer<HUnit>(HttpMethod.Delete, "/tables/hunits/hunit-1", null);
            Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);

            // Check that the database has deleted the record
            var item = GetItemFromDb<HUnit>("hunit-1");
            Assert.IsNull(item);
        }

        [TestMethod]
        public async Task DeleteItem_SoftDelete_Missing_Returns404()
        {
            var response = await SendRequestToServer<SUnit>(HttpMethod.Delete, "/tables/sunits/not-found", null);
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task DeleteItem_HardDelete_Missing_Returns404()
        {
            var response = await SendRequestToServer<HUnit>(HttpMethod.Delete, "/tables/hunits/not-found", null);
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task DeleteItem_SoftDelete_Twice_Returns404()
        {
            var firstResponse = await SendRequestToServer<SUnit>(HttpMethod.Delete, "/tables/sunits/sunit-2", null);
            Assert.AreEqual(HttpStatusCode.NoContent, firstResponse.StatusCode);

            var response = await SendRequestToServer<SUnit>(HttpMethod.Delete, "/tables/sunits/sunit-2", null);
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);

            // Check that the database has set the record to Deleted = true
            var item = GetItemFromDb<SUnit>("sunit-2");
            Assert.IsNotNull(item);
            Assert.IsTrue(item.Deleted);
        }

        [TestMethod]
        public async Task DeleteItem_HardDelete_Twice_Returns404()
        {
            var firstResponse = await SendRequestToServer<HUnit>(HttpMethod.Delete, "/tables/hunits/hunit-2", null);
            Assert.AreEqual(HttpStatusCode.NoContent, firstResponse.StatusCode);

            var response = await SendRequestToServer<HUnit>(HttpMethod.Delete, "/tables/hunits/hunit-2", null);
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);

            // Check that the database has deleted the record
            var item = GetItemFromDb<HUnit>("hunit-2");
            Assert.IsNull(item);
        }

        [TestMethod]
        public async Task DeleteItem_Unauthorized_Returns404()
        {
            var response = await SendRequestToServer<E2EServer.DataObjects.Movie>(HttpMethod.Delete, "/tables/unauthorized/movie-3", null);
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task DeleteItem_SoftDelete_PreconditionsFail_Returns412()
        {
            var response = await SendRequestToServer<SUnit>(HttpMethod.Delete, "/tables/sunits/sunit-3", null, new Dictionary<string, string>
            {
                { "If-None-Match", "*" }
            });
            Assert.AreEqual(HttpStatusCode.PreconditionFailed, response.StatusCode);

            // Check that the database has set the record to Deleted = false
            var item = GetItemFromDb<SUnit>("sunit-3");
            Assert.IsNotNull(item);
            Assert.IsFalse(item.Deleted);
        }

        [TestMethod]
        public async Task DeleteItem_HardDelete_PreconditionsFail_Returns412()
        {
            var response = await SendRequestToServer<HUnit>(HttpMethod.Delete, "/tables/hunits/hunit-3", null, new Dictionary<string, string>
            {
                { "If-None-Match", "*" }
            });
            Assert.AreEqual(HttpStatusCode.PreconditionFailed, response.StatusCode);

            // Check that the database has set the record to Deleted = false
            var item = GetItemFromDb<HUnit>("hunit-3");
            Assert.IsNotNull(item);
            Assert.IsFalse(item.Deleted);
        }

        [TestMethod]
        public async Task DeleteItem_SoftDelete_PreconditionsSucceed_Returns204()
        {
            var response = await SendRequestToServer<SUnit>(HttpMethod.Delete, "/tables/sunits/sunit-4", null, new Dictionary<string, string>
            {
                { "If-Match", "*" }
            });
            Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);

            // Check that the database has set the record to Deleted = false
            var item = GetItemFromDb<SUnit>("sunit-4");
            Assert.IsNotNull(item);
            Assert.IsTrue(item.Deleted);
        }

        [TestMethod]
        public async Task DeleteItem_HardDelete_PreconditionsSucceed_Returns204()
        {
            var response = await SendRequestToServer<HUnit>(HttpMethod.Delete, "/tables/hunits/hunit-4", null, new Dictionary<string, string>
            {
                { "If-Match", "*" }
            });
            Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);

            // Check that the database has set the record to Deleted = false
            var item = GetItemFromDb<HUnit>("hunit-4");
            Assert.IsNull(item);
        }
    }
}
