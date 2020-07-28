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
#if SUPPORTS_PATCH
    [TestClass]
    public class Patch_Tests : Base_Test
    {
        // Patch is disabled until Microsoft.AspNetCore.JsonPatch supports System.Text.Json

        [TestMethod]
        public async Task PatchItem_ValidId_Returns200()
        {
            var patchDoc = @"[{ ""op"": ""replace"", ""path"": ""/data"", ""value"": ""Replaced"" }]";
            var response = await SendRequestToServer(HttpMethod.Patch, $"/tables/hunits/hunit-10", patchDoc);
            var content = await response.Content.ReadAsStringAsync();
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            //var actual = await GetValueFromResponse<HUnit>(response);
            //Assert.AreEqual("hunit-10", actual.Id);
            //Assert.AreEqual("Replaced", actual.Data);
            //Assert.IsTrue(actual.Version.Length > 0);
            //HttpAssert.IsWithin(DateTimeOffset.UtcNow, actual.UpdatedAt, 1000);

            //HttpAssert.AreEqual(actual.Version, response.Headers.ETag);
            //HttpAssert.Match(actual.UpdatedAt, response.Content.Headers.LastModified);

            var dbItem = await GetItemFromDb<HUnit>("hunit-10");
            Assert.IsNotNull(dbItem);
            Assert.AreEqual("Replaced", dbItem.Data);
        }

        [TestMethod]
        public async Task PatchItem_MissingItem_Returns404()
        {
            var patchDoc = @"[{ ""op"": ""replace"", ""path"": ""/Data"", ""value"": ""Replaced"" }]";
            var response = await SendRequestToServer(HttpMethod.Patch, $"/tables/hunits/missing-item", patchDoc);
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task PatchItem_MismatchedId_Returns400()
        {
            var patchDoc = @"[
    { ""op"": ""replace"", ""path"": ""/Data"", ""value"": ""Replaced"" },
    { ""op"": ""replace"", ""path"": ""/Id"", ""value"" ""Replaced"" }
]";
            var response = await SendRequestToServer(HttpMethod.Patch, $"/tables/hunits/hunit-11", patchDoc);
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [TestMethod]
        public async Task PatchItem_SoftDelete_DeletedItem_Returns404()
        {
            var patchDoc = @"[{ ""op"": ""replace"", ""path"": ""/Data"", ""value"": ""Replaced"" }]";
            var deleteResponse = await SendRequestToServer<SUnit>(HttpMethod.Delete, $"/tables/sunits/sunit-12", null);
            Assert.AreEqual(HttpStatusCode.NoContent, deleteResponse.StatusCode);

            var response = await SendRequestToServer(HttpMethod.Patch, $"/tables/sunits/sunit-12", patchDoc);
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);

            var dbItem = await GetItemFromDb<HUnit>("sunit-12");
            Assert.IsNotNull(dbItem);
            Assert.AreNotEqual("Replaced", dbItem.Data);
        }

        [TestMethod]
        public async Task PatchItem_Unauthorized_Returns404()
        {
            var item = await GetItemFromDb<E2EServer.DataObjects.Movie>("movie-8");
            var patchDoc = @"[{ ""op"": ""replace"", ""path"": ""/Title"", ""value"": ""Replaced"" }]";
            var response = await SendRequestToServer(HttpMethod.Patch, $"/tables/unauthorized/{item.Id}", patchDoc);
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task PatchItem_PreconditionsFail_Returns412()
        {
            var item = await GetItemFromDb<HUnit>("hunit-13");
            var patchDoc = @"[{ ""op"": ""replace"", ""path"": ""/Data"", ""value"": ""Replaced"" }]";
            var response = await SendRequestToServer(HttpMethod.Patch, $"/tables/hunits/{item.Id}", patchDoc, new Dictionary<string, string>
            {
                { "If-None-Match", ETag.FromByteArray(item.Version) }
            });
            Assert.AreEqual(HttpStatusCode.PreconditionFailed, response.StatusCode);

            var actual = await GetValueFromResponse<HUnit>(response);
            Assert.AreEqual(item.Id, actual.Id);
            Assert.AreEqual(item.Data, actual.Data);

            HttpAssert.AreEqual(actual.Version, response.Headers.ETag);
            HttpAssert.Match(actual.UpdatedAt, response.Content.Headers.LastModified);

            var dbItem = await GetItemFromDb<HUnit>(item.Id);
            Assert.IsNotNull(dbItem);
            Assert.AreEqual(item.Data, dbItem.Data);
        }

        [TestMethod]
        public async Task PatchItem_PreconditionsSucceed_Returns200()
        {
            var item = await GetItemFromDb<HUnit>("hunit-14");
            var patchDoc = @"[{ ""op"": ""replace"", ""path"": ""/Data"", ""value"": ""Replaced"" }]";
            var response = await SendRequestToServer(HttpMethod.Patch, $"/tables/hunits/{item.Id}", patchDoc, new Dictionary<string, string>
            {
                { "If-Match", ETag.FromByteArray(item.Version) }
            });

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var actual = await GetValueFromResponse<HUnit>(response);
            Assert.AreEqual(item.Id, actual.Id);
            Assert.AreEqual("Replaced", actual.Data);
            Assert.IsTrue(actual.Version.Length > 0);
            HttpAssert.IsWithin(DateTimeOffset.UtcNow, actual.UpdatedAt, 1000);

            HttpAssert.AreEqual(actual.Version, response.Headers.ETag);
            HttpAssert.Match(actual.UpdatedAt, response.Content.Headers.LastModified);

            var dbItem = await GetItemFromDb<HUnit>(item.Id);
            Assert.IsNotNull(dbItem);
            Assert.AreEqual("Replaced", dbItem.Data);
        }
    }
#endif
}
