using Azure.Mobile.Server.Test.Helpers;
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
    public class Create_Tests : Base_Test
    {
        [TestMethod]
        public async Task CreateItem_MissingItem_Returns201()
        {
            var item = new HUnit
            {
                Id = Guid.NewGuid().ToString("N"),
                Data = "create-item-missing-item-returns-201"
            };
            var response = await SendRequestToServer<HUnit>(HttpMethod.Post, "tables/hunits", item);

            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
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
        public async Task CreateItem_NotAuthorized_Returns401()
        {
            var item = new HUnit
            {
                Id = Guid.NewGuid().ToString("N"),
                Data = "create-item-not-authorized-returns-401"
            };
            var response = await SendRequestToServer<HUnit>(HttpMethod.Post, "tables/unauthorized", item);

            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);

            var dbItem = GetItemFromDb<HUnit>(item.Id);
            Assert.IsNull(dbItem);
        }

        [TestMethod]
        public async Task CreateItem_DoubleCreate_Returns409()
        {
            var item = new HUnit
            {
                Id = Guid.NewGuid().ToString("N"),
                Data = "create-item-double-create-returns-409"
            };

            var firstResponse = await SendRequestToServer<HUnit>(HttpMethod.Post, "tables/hunits", item);
            Assert.AreEqual(HttpStatusCode.Created, firstResponse.StatusCode);

            var response = await SendRequestToServer<HUnit>(HttpMethod.Post, "tables/hunits", item);
            Assert.AreEqual(HttpStatusCode.Conflict, response.StatusCode);
        }

        [TestMethod]
        public async Task CreateItem_MissingId_Returns201_AndCreatesId()
        {
            var item = new HUnit
            {
                Data = "create-item-missing-id-returns-201"
            };
            var response = await SendRequestToServer<HUnit>(HttpMethod.Post, "tables/hunits", item);

            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
            var actual = await GetValueFromResponse<HUnit>(response);

            Assert.IsNotNull(actual.Id);                // Check existence of Id
            Assert.IsNotNull(new Guid(actual.Id));      // Make sure it is a real GUID
            Assert.AreEqual(item.Data, actual.Data);
            Assert.IsTrue(actual.Version.Length > 0);
            HttpAssert.IsWithin(DateTimeOffset.UtcNow, actual.UpdatedAt, 1000);

            HttpAssert.AreEqual(actual.Version, response.Headers.ETag);
            HttpAssert.Match(actual.UpdatedAt, response.Content.Headers.LastModified);

            var dbItem = GetItemFromDb<HUnit>(actual.Id);
            Assert.IsNotNull(dbItem);
            Assert.AreEqual(item.Data, dbItem.Data);
        }

        [TestMethod]
        public async Task CreateItem_PreconditionsFail_Returns412()
        {
            var item = new HUnit
            {
                Id = Guid.NewGuid().ToString("N"),
                Data = "create-item-preconditions-fail-returns-412"
            };
            var firstResponse = await SendRequestToServer<HUnit>(HttpMethod.Post, "tables/hunits", item);
            Assert.AreEqual(HttpStatusCode.Created, firstResponse.StatusCode);

            var response = await SendRequestToServer<HUnit>(HttpMethod.Post, "tables/hunits", item, new Dictionary<string,string>
            {
                { "If-None-Match", "*" }
            });
            Assert.AreEqual(HttpStatusCode.PreconditionFailed, response.StatusCode);
        }

        [TestMethod]
        public async Task CreateItem_PreconditionsSuccess_Returns201()
        {
            var item = new HUnit
            {
                Id = Guid.NewGuid().ToString("N"),
                Data = "create-item-preconditions-success-returns-201"
            };

            var response = await SendRequestToServer<HUnit>(HttpMethod.Post, "tables/hunits", item, new Dictionary<string, string>
            {
                { "If-None-Match", "*" }
            });
            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
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
