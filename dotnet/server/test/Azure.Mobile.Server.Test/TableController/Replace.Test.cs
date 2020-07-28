using Azure.Mobile.Server.Test.Helpers;
using Azure.Mobile.Server.Utils;
using E2EServer.DataObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Azure.Mobile.Server.Test.TableController
{
    [TestClass]
    public class Replace_Tests : Base_Test
    {
        [TestMethod]
        public async Task ReplaceItem_ValidId_Returns200()
        {
            var item = new HUnit() { Id = "hunit-6", Data = "Replaced" };
            var response = await SendRequestToServer<HUnit>(HttpMethod.Put, $"/tables/hunits/{item.Id}", item);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var actual = await GetValueFromResponse<HUnit>(response);
            Assert.AreEqual(item.Id, actual.Id);
            Assert.AreEqual(item.Data, actual.Data);
            Assert.IsTrue(actual.Version.Length > 0);
            HttpAssert.IsWithin(DateTimeOffset.UtcNow, actual.UpdatedAt, 1000);

            HttpAssert.AreEqual(actual.Version, response.Headers.ETag);
            HttpAssert.Match(actual.UpdatedAt, response.Content.Headers.LastModified);

            var dbItem = GetItemFromDb<HUnit>(item.Id);
            Assert.IsNotNull(dbItem);
            Assert.AreEqual(item.Data, dbItem.Data);
        }

        [TestMethod]
        public async Task ReplaceItem_MissingItem_Returns404()
        {
            var item = new HUnit() { Id = "missing-item", Data = "Replaced" };
            var response = await SendRequestToServer<HUnit>(HttpMethod.Put, $"/tables/hunits/{item.Id}", item);
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task ReplaceItem_MismatchedId_Returns400()
        {
            var item = new HUnit() { Id = "hunit-6", Data = "Replaced" };
            var response = await SendRequestToServer<HUnit>(HttpMethod.Put, $"/tables/hunits/missing-item", item);
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [TestMethod]
        public async Task ReplaceItem_SoftDelete_DeletedItem_Returns404()
        {
            var item = new SUnit() { Id = "sunit-7", Data = "Replaced" };
            var deleteResponse = await SendRequestToServer<SUnit>(HttpMethod.Delete, $"/tables/sunits/{item.Id}", null);
            Assert.AreEqual(HttpStatusCode.NoContent, deleteResponse.StatusCode);

            var response = await SendRequestToServer<SUnit>(HttpMethod.Put, $"/tables/sunits/{item.Id}", item);
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task ReplaceItem_Unauthorized_Returns404()
        {
            var item = GetItemFromDb<E2EServer.DataObjects.Movie>("movie-8");
            var response = await SendRequestToServer<E2EServer.DataObjects.Movie>(HttpMethod.Put, $"/tables/unauthorized/{item.Id}", item);
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task ReplaceItem_PreconditionsFail_Returns412()
        {
            var item = GetItemFromDb<HUnit>("hunit-8");
            var originalData = item.Data; item.Data = "Replaced";
            var response = await SendRequestToServer<HUnit>(HttpMethod.Put, $"/tables/hunits/{item.Id}", item, new Dictionary<string, string>
            {
                { "If-None-Match", ETag.FromByteArray(item.Version) }
            });
            Assert.AreEqual(HttpStatusCode.PreconditionFailed, response.StatusCode);

            var actual = await GetValueFromResponse<HUnit>(response);
            Assert.AreEqual(item.Id, actual.Id);
            Assert.AreEqual(originalData, actual.Data);

            HttpAssert.AreEqual(actual.Version, response.Headers.ETag);
            HttpAssert.Match(actual.UpdatedAt, response.Content.Headers.LastModified);

            var dbItem = GetItemFromDb<HUnit>(item.Id);
            Assert.IsNotNull(dbItem);
            Assert.AreEqual(originalData, dbItem.Data);
        }

        [TestMethod]
        public async Task ReplaceItem_PreconditionsSucceed_Returns200()
        {
            var item = GetItemFromDb<HUnit>("hunit-8");
            var originalData = item.Data; item.Data = "Replaced";
            var response = await SendRequestToServer<HUnit>(HttpMethod.Put, $"/tables/hunits/{item.Id}", item, new Dictionary<string, string>
            {
                { "If-Match", ETag.FromByteArray(item.Version) }
            });

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var actual = await GetValueFromResponse<HUnit>(response);
            Assert.AreEqual(item.Id, actual.Id);
            Assert.AreEqual(item.Data, actual.Data);
            Assert.IsTrue(actual.Version.Length > 0);
            HttpAssert.IsWithin(DateTimeOffset.UtcNow, actual.UpdatedAt, 1000);

            HttpAssert.AreEqual(actual.Version, response.Headers.ETag);
            HttpAssert.Match(actual.UpdatedAt, response.Content.Headers.LastModified);

            var dbItem = GetItemFromDb<HUnit>(item.Id);
            Assert.IsNotNull(dbItem);
            Assert.AreEqual(item.Data, dbItem.Data);
        }
    }
}
