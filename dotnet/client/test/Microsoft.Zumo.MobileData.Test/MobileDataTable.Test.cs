// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Azure;
using Azure.Core.Pipeline;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Zumo.MobileData.Test.Helpers;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Microsoft.Zumo.MobileData.Test
{
    [TestClass]
    public class MobileDataTable : BaseTest
    {
        #region TestData
        class Entity : TableData { public string Data { get; set; } }

        private MobileTable<Entity> GetTableReference(MockTransport transport)
        {
            var client = new MobileTableClient(new Uri("https://localhost"), new MobileTableClientOptions
            {
                Transport = new HttpClientTransport(transport.Client)
            });
            return client.GetTable<Entity>();
        }

        private readonly string returnedEntity = "{\"id\":\"retid\",\"data\":\"retdata\",\"deleted\":false,\"updatedAt\":\"2020-02-27T03:22:05.000Z\",\"version\":\"abcdef\"}";
        
        private HttpResponseMessage CreateEntityResponse(HttpStatusCode code)
        {
            var response = new HttpResponseMessage
            {
                Content = new StringContent(returnedEntity),
                StatusCode = code
            };

            response.Headers.Add("ETag", "\"abcdef\"");
            response.Content.Headers.Add("Last-Modified", "Thu, 27 Feb 2020 03:22:05 GMT");
            if (code == HttpStatusCode.Created)
            {
                response.Headers.Add("Location", "https://localhost/tables/entity/retid");
            }

            return response;
        }
        #endregion

        #region CreateItemAsync
        // Item is null
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task CreateItemAsync_NullItem_Throws()
        {
            var mock = new MockTransport();
            var table = GetTableReference(mock);

            _ = await table.CreateItemAsync(null);
        }

        // Item is valid, Id is null, 201 response
        [TestMethod]
        public async Task CreateItemAsync_NullId_Returns201()
        {
            var mock = new MockTransport();
            var table = GetTableReference(mock);
            Entity sentEntity = new Entity { Data = "retdata", Deleted = false, UpdatedAt = DateTimeOffset.Parse("2020-02-27T03:22:05.000Z"), Version = "abcdef" };

            mock.Response = CreateEntityResponse(HttpStatusCode.Created);

            Response<Entity> response = await table.CreateItemAsync(sentEntity);

            // Check the response
            Assert.IsNotNull(response);
            var raw = response.GetRawResponse();
            Assert.AreEqual(201, raw.Status);
            HttpAssert.HeaderIsEqual("ETag", "\"abcdef\"", raw.Headers);
            HttpAssert.HeaderIsEqual("Last-Modified", "Thu, 27 Feb 2020 03:22:05 GMT", raw.Headers);
            HttpAssert.HeaderIsEqual("Location", "https://localhost/tables/entity/retid", raw.Headers);
            Assert.AreEqual("retid", response.Value.Id);
        }

        // Item is valid, Id is null, 403 response
        [TestMethod]
        public async Task CreateItemAsync_NullId_Unauthorized_Throws_RequestFailedException_WithInfo()
        {
            var mock = new MockTransport();
            var table = GetTableReference(mock);
            Entity sentEntity = new Entity { Data = "retdata", Deleted = false, UpdatedAt = DateTimeOffset.Parse("2020-02-27T03:22:05.000Z"), Version = "abcdef" };
            mock.Response = new HttpResponseMessage { StatusCode = HttpStatusCode.Forbidden };
            try
            {
                Response<Entity> response = await table.CreateItemAsync(sentEntity);

                Assert.Fail("RequestFailedException expected");
            }
            catch (RequestFailedException ex)
            {
                Assert.AreEqual(403, ex.Status);
            }
        }

        // Item is valid, Id is not null, 201 response
        [TestMethod]
        public async Task CreateItemAsync_ItemWithId_Returns201()
        {
            var mock = new MockTransport();
            var table = GetTableReference(mock);
            Entity sentEntity = new Entity { Id = "retid", Data = "retdata", Deleted = false, UpdatedAt = DateTimeOffset.Parse("2020-02-27T03:22:05.000Z"), Version = "abcdef" };

            mock.Response = CreateEntityResponse(HttpStatusCode.Created);

            Response<Entity> response = await table.CreateItemAsync(sentEntity);

            // Check the response
            Assert.IsNotNull(response);
            var raw = response.GetRawResponse();
            Assert.AreEqual(201, raw.Status);
            HttpAssert.HeaderIsEqual("ETag", "\"abcdef\"", raw.Headers);
            HttpAssert.HeaderIsEqual("Last-Modified", "Thu, 27 Feb 2020 03:22:05 GMT", raw.Headers);
            HttpAssert.HeaderIsEqual("Location", "https://localhost/tables/entity/retid", raw.Headers);
            Assert.AreEqual("retid", response.Value.Id);
        }

        // Item is valid, Id is not null, 403 response
        [TestMethod]
        public async Task CreateItemAsync_ItemWithId_Unauthorized_Throws_RequestFailedException_WithInfo()
        {
            var mock = new MockTransport();
            var table = GetTableReference(mock);
            Entity sentEntity = new Entity { Id = "retid", Data = "retdata", Deleted = false, UpdatedAt = DateTimeOffset.Parse("2020-02-27T03:22:05.000Z"), Version = "abcdef" };
            mock.Response = new HttpResponseMessage { StatusCode = HttpStatusCode.Forbidden };

            try
            {
                Response<Entity> response = await table.CreateItemAsync(sentEntity);

                Assert.Fail("RequestFailedException expected");
            }
            catch (RequestFailedException ex)
            {
                Assert.AreEqual(403, ex.Status);
            }
        }

        // Item is valid, Id is not null, 409 response with Conflict
        [TestMethod]
        public async Task CreateItemAsync_ItemWithId_Conflict_ThrowsConflictException_WithInfo()
        {
            var mock = new MockTransport();
            var table = GetTableReference(mock);
            Entity sentEntity = new Entity { Id = "retid", Data = "retdata", Deleted = false, UpdatedAt = DateTimeOffset.Parse("2020-02-27T03:22:05.000Z"), Version = "abcdef" };
            mock.Response = CreateEntityResponse(HttpStatusCode.Conflict);

            try
            {
                Response<Entity> response = await table.CreateItemAsync(sentEntity);

                Assert.Fail("ConflictException expected");
            }
            catch (ConflictException<Entity> ex)
            {
                Assert.AreEqual(409, ex.Status);
                Assert.IsNotNull(ex.Value);
                Assert.AreEqual("abcdef", ex.ETag.ToString());
                Assert.IsNotNull(ex.LastModified);
            }
        }

        // Item is valid, Id is not null, 412 response with Conflict
        [TestMethod]
        public async Task CreateItemAsync_ItemWithId_PreconditionsFail_ThrowsConflictException_WithInfo()
        {
            var mock = new MockTransport();
            var table = GetTableReference(mock);
            Entity sentEntity = new Entity { Id = "retid", Data = "retdata", Deleted = false, UpdatedAt = DateTimeOffset.Parse("2020-02-27T03:22:05.000Z"), Version = "abcdef" };
            mock.Response = CreateEntityResponse(HttpStatusCode.PreconditionFailed);

            try
            {
                Response<Entity> response = await table.CreateItemAsync(sentEntity);

                Assert.Fail("ConflictException expected");
            }
            catch (ConflictException<Entity> ex)
            {
                Assert.AreEqual(412, ex.Status);
                Assert.IsNotNull(ex.Value);
                Assert.AreEqual("abcdef", ex.ETag.ToString());
                Assert.IsNotNull(ex.LastModified);
            }
        }

        // Item is not valid, 400 response
        [TestMethod]
        public async Task CreateItemAsync_InvalidItem_ThrowsRequestFailedException_WithInfo()
        {
            var mock = new MockTransport();
            var table = GetTableReference(mock);
            Entity sentEntity = new Entity { Id = "retid", Data = "retdata", Deleted = false, UpdatedAt = DateTimeOffset.Parse("2020-02-27T03:22:05.000Z"), Version = "abcdef" };
            mock.Response = new HttpResponseMessage { StatusCode = HttpStatusCode.BadRequest };

            try
            {
                Response<Entity> response = await table.CreateItemAsync(sentEntity);

                Assert.Fail("RequestFailedException expected");
            }
            catch (RequestFailedException ex)
            {
                Assert.AreEqual(400, ex.Status);
            }
        }
        #endregion

        #region CreateItem
        // Item is null
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CreateItem_NullItem_Throws()
        {
            var mock = new MockTransport();
            var table = GetTableReference(mock);

            _ = table.CreateItem(null);
        }

        // Item is valid, Id is null, 201 response
        [TestMethod]
        public void CreateItem_NullId_Returns201()
        {
            var mock = new MockTransport();
            var table = GetTableReference(mock);
            Entity sentEntity = new Entity { Data = "retdata", Deleted = false, UpdatedAt = DateTimeOffset.Parse("2020-02-27T03:22:05.000Z"), Version = "abcdef" };

            mock.Response = CreateEntityResponse(HttpStatusCode.Created);

            Response<Entity> response = table.CreateItem(sentEntity);

            // Check the response
            Assert.IsNotNull(response);
            var raw = response.GetRawResponse();
            Assert.AreEqual(201, raw.Status);
            HttpAssert.HeaderIsEqual("ETag", "\"abcdef\"", raw.Headers);
            HttpAssert.HeaderIsEqual("Last-Modified", "Thu, 27 Feb 2020 03:22:05 GMT", raw.Headers);
            HttpAssert.HeaderIsEqual("Location", "https://localhost/tables/entity/retid", raw.Headers);
            Assert.AreEqual("retid", response.Value.Id);
        }

        // Item is valid, Id is null, 403 response
        [TestMethod]
        public void CreateItem_NullId_Unauthorized_Throws_RequestFailedException_WithInfo()
        {
            var mock = new MockTransport();
            var table = GetTableReference(mock);
            Entity sentEntity = new Entity { Data = "retdata", Deleted = false, UpdatedAt = DateTimeOffset.Parse("2020-02-27T03:22:05.000Z"), Version = "abcdef" };
            mock.Response = new HttpResponseMessage { StatusCode = HttpStatusCode.Forbidden };
            try
            {
                Response<Entity> response = table.CreateItem(sentEntity);

                Assert.Fail("RequestFailedException expected");
            }
            catch (RequestFailedException ex)
            {
                Assert.AreEqual(403, ex.Status);
            }
        }

        // Item is valid, Id is not null, 201 response
        [TestMethod]
        public void CreateItem_ItemWithId_Returns201()
        {
            var mock = new MockTransport();
            var table = GetTableReference(mock);
            Entity sentEntity = new Entity { Id = "retid", Data = "retdata", Deleted = false, UpdatedAt = DateTimeOffset.Parse("2020-02-27T03:22:05.000Z"), Version = "abcdef" };

            mock.Response = CreateEntityResponse(HttpStatusCode.Created);

            Response<Entity> response = table.CreateItem(sentEntity);

            // Check the response
            Assert.IsNotNull(response);
            var raw = response.GetRawResponse();
            Assert.AreEqual(201, raw.Status);
            HttpAssert.HeaderIsEqual("ETag", "\"abcdef\"", raw.Headers);
            HttpAssert.HeaderIsEqual("Last-Modified", "Thu, 27 Feb 2020 03:22:05 GMT", raw.Headers);
            HttpAssert.HeaderIsEqual("Location", "https://localhost/tables/entity/retid", raw.Headers);
            Assert.AreEqual("retid", response.Value.Id);
        }

        // Item is valid, Id is not null, 403 response
        [TestMethod]
        public void CreateItem_ItemWithId_Unauthorized_Throws_RequestFailedException_WithInfo()
        {
            var mock = new MockTransport();
            var table = GetTableReference(mock);
            Entity sentEntity = new Entity { Id = "retid", Data = "retdata", Deleted = false, UpdatedAt = DateTimeOffset.Parse("2020-02-27T03:22:05.000Z"), Version = "abcdef" };
            mock.Response = new HttpResponseMessage { StatusCode = HttpStatusCode.Forbidden };

            try
            {
                Response<Entity> response = table.CreateItem(sentEntity);

                Assert.Fail("RequestFailedException expected");
            }
            catch (RequestFailedException ex)
            {
                Assert.AreEqual(403, ex.Status);
            }
        }

        // Item is valid, Id is not null, 409 response with Conflict
        [TestMethod]
        public void CreateItem_ItemWithId_Conflict_ThrowsConflictException_WithInfo()
        {
            var mock = new MockTransport();
            var table = GetTableReference(mock);
            Entity sentEntity = new Entity { Id = "retid", Data = "retdata", Deleted = false, UpdatedAt = DateTimeOffset.Parse("2020-02-27T03:22:05.000Z"), Version = "abcdef" };
            mock.Response = CreateEntityResponse(HttpStatusCode.Conflict);

            try
            {
                Response<Entity> response = table.CreateItem(sentEntity);

                Assert.Fail("ConflictException expected");
            }
            catch (ConflictException<Entity> ex)
            {
                Assert.AreEqual(409, ex.Status);
                Assert.IsNotNull(ex.Value);
                Assert.AreEqual("abcdef", ex.ETag.ToString());
                Assert.IsNotNull(ex.LastModified);
            }
        }

        // Item is valid, Id is not null, 412 response with Conflict
        [TestMethod]
        public void CreateItem_ItemWithId_PreconditionsFail_ThrowsConflictException_WithInfo()
        {
            var mock = new MockTransport();
            var table = GetTableReference(mock);
            Entity sentEntity = new Entity { Id = "retid", Data = "retdata", Deleted = false, UpdatedAt = DateTimeOffset.Parse("2020-02-27T03:22:05.000Z"), Version = "abcdef" };
            mock.Response = CreateEntityResponse(HttpStatusCode.PreconditionFailed);

            try
            {
                Response<Entity> response = table.CreateItem(sentEntity);

                Assert.Fail("ConflictException expected");
            }
            catch (ConflictException<Entity> ex)
            {
                Assert.AreEqual(412, ex.Status);
                Assert.IsNotNull(ex.Value);
                Assert.AreEqual("abcdef", ex.ETag.ToString());
                Assert.IsNotNull(ex.LastModified);
            }
        }

        // Item is not valid, 400 response
        [TestMethod]
        public void CreateItem_InvalidItem_ThrowsRequestFailedException_WithInfo()
        {
            var mock = new MockTransport();
            var table = GetTableReference(mock);
            Entity sentEntity = new Entity { Id = "retid", Data = "retdata", Deleted = false, UpdatedAt = DateTimeOffset.Parse("2020-02-27T03:22:05.000Z"), Version = "abcdef" };
            mock.Response = new HttpResponseMessage { StatusCode = HttpStatusCode.BadRequest };

            try
            {
                Response<Entity> response = table.CreateItem(sentEntity);

                Assert.Fail("RequestFailedException expected");
            }
            catch (RequestFailedException ex)
            {
                Assert.AreEqual(400, ex.Status);
            }
        }
        #endregion

        #region DeleteItemAsync
        // Item is null
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task DeleteItemAsync_NullItem_Throws()
        {
            var mock = new MockTransport();
            var table = GetTableReference(mock);

            _ = await table.DeleteItemAsync(null);
        }

        // Item.Id is null
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task DeleteItemAsync_NullId_Throws()
        {
            var mock = new MockTransport();
            var table = GetTableReference(mock);

            _ = await table.DeleteItemAsync(new Entity { Id = null });
        }

        // Item is valid, no MatchConditions, 200 response
        [TestMethod]
        public async Task DeleteItemAsync_ValidItem_NoMatchConditions_Returns200()
        {
            var mock = new MockTransport();
            var table = GetTableReference(mock);
            var entity = new Entity { Id = "foo", Version = "bar" };
            mock.Response = new HttpResponseMessage { StatusCode = HttpStatusCode.OK };

            var response = await table.DeleteItemAsync(entity);

            Assert.AreEqual(200, response.Status);
        }

        // Item is valid, no MatchConditions, 204 response
        [TestMethod]
        public async Task DeleteItemAsync_ValidItem_NoMatchConditions_Returns204()
        {
            var mock = new MockTransport();
            var table = GetTableReference(mock);
            var entity = new Entity { Id = "foo", Version = "bar" };
            mock.Response = new HttpResponseMessage { StatusCode = HttpStatusCode.NoContent };

            var response = await table.DeleteItemAsync(entity);

            Assert.AreEqual(204, response.Status);
        }

        // Item is valid, no MatchConditions, 403 response
        [TestMethod]
        public async Task DeleteItemAsync_ValidItem_NoMatchConditions_Returns403()
        {
            var mock = new MockTransport();
            var table = GetTableReference(mock);
            var entity = new Entity { Id = "foo", Version = "bar" };
            mock.Response = new HttpResponseMessage { StatusCode = HttpStatusCode.Forbidden };

            try
            {
                var response = await table.DeleteItemAsync(entity);
                Assert.Fail("RequestFailedException expected");
            }
            catch (RequestFailedException ex)
            {
                Assert.AreEqual(403, ex.Status);
            }
        }

        // Item is valid, no MatchConditions, 404 response
        [TestMethod]
        public async Task DeleteItemAsync_ValidItem_NoMatchConditions_Returns404()
        {
            var mock = new MockTransport();
            var table = GetTableReference(mock);
            var entity = new Entity { Id = "foo", Version = "bar" };
            mock.Response = new HttpResponseMessage { StatusCode = HttpStatusCode.NotFound };

            try
            {
                var response = await table.DeleteItemAsync(entity);
                Assert.Fail("RequestFailedException expected");
            }
            catch (RequestFailedException ex)
            {
                Assert.AreEqual(404, ex.Status);
            }
        }

        // Item is valid, no MatchConditions, 412 response with Conflict
        [TestMethod]
        public async Task DeleteItemAsync_ValidItem_NoMatchConditions_Returns412_WithConflict()
        {
            var mock = new MockTransport();
            var table = GetTableReference(mock);
            var entity = new Entity { Id = "foo", Version = "bar" };
            mock.Response = new HttpResponseMessage { StatusCode = HttpStatusCode.NotFound };

            try
            {
                var response = await table.DeleteItemAsync(entity, new MatchConditions { IfMatch = ETag.All });
                Assert.Fail("RequestFailedException expected");
            }
            catch (RequestFailedException ex)
            {
                Assert.AreEqual(404, ex.Status);
            }
        }

        // Item is valid, MatchConditions blank, 204 response
        [TestMethod]
        public async Task DeleteItemAsync_ValidItem_BlankMatchConditions_Returns204()
        {
            var mock = new MockTransport();
            var table = GetTableReference(mock);
            var entity = new Entity { Id = "foo", Version = "bar" };
            mock.Response = new HttpResponseMessage { StatusCode = HttpStatusCode.NoContent };

            var response = await table.DeleteItemAsync(entity, new MatchConditions());

            Assert.AreEqual(204, response.Status);
        }

        // Item is valid, MatchConditions blank, 404 response
        [TestMethod]
        public async Task DeleteItemAsync_ValidItem_BlankMatchConditions_Returns404()
        {
            var mock = new MockTransport();
            var table = GetTableReference(mock);
            var entity = new Entity { Id = "foo", Version = "bar" };
            mock.Response = new HttpResponseMessage { StatusCode = HttpStatusCode.NotFound };

            try
            {
                var response = await table.DeleteItemAsync(entity, new MatchConditions());
                Assert.Fail("RequestFailedException expected");
            }
            catch (RequestFailedException ex)
            {
                Assert.AreEqual(404, ex.Status);
            }
        }

        // Item is valid, MatchConditions, If-Match *, 204 response
        [TestMethod]
        public async Task DeleteItemAsync_ValidItem_IfMatchAny_Returns204()
        {
            var mock = new MockTransport();
            var table = GetTableReference(mock);
            var entity = new Entity { Id = "foo", Version = "bar" };
            mock.Response = new HttpResponseMessage { StatusCode = HttpStatusCode.NoContent };

            var response = await table.DeleteItemAsync(entity, new MatchConditions { IfMatch = ETag.All });

            Assert.AreEqual(204, response.Status);
        }

        // Item is valid, MatchConditions, If-Match *, 404 response
        [TestMethod]
        public async Task DeleteItemAsync_ValidItem_IfMatchAny_Returns404()
        {
            var mock = new MockTransport();
            var table = GetTableReference(mock);
            var entity = new Entity { Id = "foo", Version = "bar" };
            mock.Response = new HttpResponseMessage { StatusCode = HttpStatusCode.NotFound };

            try
            {
                var response = await table.DeleteItemAsync(entity, new MatchConditions { IfMatch = ETag.All });
                Assert.Fail("RequestFailedException expected");
            }
            catch (RequestFailedException ex)
            {
                Assert.AreEqual(404, ex.Status);
            }
        }

        // Item is valid, MatchConditions, If-Match *, 412 response with conflict
        [TestMethod]
        public async Task DeleteItemAsync_ValidItem_IfMatchAny_Returns412_WithConflict()
        {
            var mock = new MockTransport();
            var table = GetTableReference(mock);
            var entity = new Entity { Id = "foo", Version = "bar" };
            mock.Response = CreateEntityResponse(HttpStatusCode.PreconditionFailed);

            try
            {
                var response = await table.DeleteItemAsync(entity, new MatchConditions());
                Assert.Fail("RequestFailedException expected");
            }
            catch (ConflictException<Entity> ex)
            {
                Assert.AreEqual(412, ex.Status);
                Assert.IsNotNull(ex.ETag);
                Assert.IsNotNull(ex.LastModified);
                Assert.IsNotNull(ex.Value);
            }
        }

        // Item is valid, MatchConditions, If-Match version, 204 response
        [TestMethod]
        public async Task DeleteItemAsync_ValidItem_IfMatchVersion_Returns204()
        {
            var mock = new MockTransport();
            var table = GetTableReference(mock);
            var entity = new Entity { Id = "foo", Version = "bar" };
            mock.Response = new HttpResponseMessage { StatusCode = HttpStatusCode.NoContent };

            var response = await table.DeleteItemAsync(entity, new MatchConditions { IfMatch = new ETag("bar") });

            Assert.AreEqual(204, response.Status);
        }

        // Item is valid, MatchConditions, If-Match version, 404 response
        [TestMethod]
        public async Task DeleteItemAsync_ValidItem_IfMatchVersion_Returns404()
        {
            var mock = new MockTransport();
            var table = GetTableReference(mock);
            var entity = new Entity { Id = "foo", Version = "bar" };
            mock.Response = new HttpResponseMessage { StatusCode = HttpStatusCode.NotFound };

            try
            {
                var response = await table.DeleteItemAsync(entity, new MatchConditions { IfMatch = new ETag("bar") });
                Assert.Fail("RequestFailedException expected");
            }
            catch (RequestFailedException ex)
            {
                Assert.AreEqual(404, ex.Status);
            }
        }

        // Item is valid, MatchConditions, If-Match version, 412 response with conflict
        [TestMethod]
        public async Task DeleteItemAsync_ValidItem_IfMatchVersion_Returns412()
        {
            var mock = new MockTransport();
            var table = GetTableReference(mock);
            var entity = new Entity { Id = "foo", Version = "bar" };
            mock.Response = CreateEntityResponse(HttpStatusCode.PreconditionFailed);

            try
            {
                var response = await table.DeleteItemAsync(entity, new MatchConditions { IfMatch = new ETag("bar") });
                Assert.Fail("RequestFailedException expected");
            }
            catch (ConflictException<Entity> ex)
            {
                Assert.AreEqual(412, ex.Status);
                Assert.IsNotNull(ex.ETag);
                Assert.IsNotNull(ex.LastModified);
                Assert.IsNotNull(ex.Value);
            }
        }
        #endregion

        #region DeleteItem
        // Item is null
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void DeleteItem_NullItem_Throws()
        {
            var mock = new MockTransport();
            var table = GetTableReference(mock);

            _ = table.DeleteItem(null);
        }

        // Item.Id is null
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void DeleteItem_NullId_Throws()
        {
            var mock = new MockTransport();
            var table = GetTableReference(mock);

            _ = table.DeleteItem(new Entity { Id = null });
        }

        // Item is valid, no MatchConditions, 200 response
        [TestMethod]
        public void DeleteItem_ValidItem_NoMatchConditions_Returns200()
        {
            var mock = new MockTransport();
            var table = GetTableReference(mock);
            var entity = new Entity { Id = "foo", Version = "bar" };
            mock.Response = new HttpResponseMessage { StatusCode = HttpStatusCode.OK };

            var response = table.DeleteItem(entity);

            Assert.AreEqual(200, response.Status);
        }

        // Item is valid, no MatchConditions, 204 response
        [TestMethod]
        public void DeleteItem_ValidItem_NoMatchConditions_Returns204()
        {
            var mock = new MockTransport();
            var table = GetTableReference(mock);
            var entity = new Entity { Id = "foo", Version = "bar" };
            mock.Response = new HttpResponseMessage { StatusCode = HttpStatusCode.NoContent };

            var response = table.DeleteItem(entity);

            Assert.AreEqual(204, response.Status);
        }

        // Item is valid, no MatchConditions, 403 response
        [TestMethod]
        public void DeleteItem_ValidItem_NoMatchConditions_Returns403()
        {
            var mock = new MockTransport();
            var table = GetTableReference(mock);
            var entity = new Entity { Id = "foo", Version = "bar" };
            mock.Response = new HttpResponseMessage { StatusCode = HttpStatusCode.Forbidden };

            try
            {
                var response = table.DeleteItem(entity);
                Assert.Fail("RequestFailedException expected");
            }
            catch (RequestFailedException ex)
            {
                Assert.AreEqual(403, ex.Status);
            }
        }

        // Item is valid, no MatchConditions, 404 response
        [TestMethod]
        public void DeleteItem_ValidItem_NoMatchConditions_Returns404()
        {
            var mock = new MockTransport();
            var table = GetTableReference(mock);
            var entity = new Entity { Id = "foo", Version = "bar" };
            mock.Response = new HttpResponseMessage { StatusCode = HttpStatusCode.NotFound };

            try
            {
                var response = table.DeleteItem(entity);
                Assert.Fail("RequestFailedException expected");
            }
            catch (RequestFailedException ex)
            {
                Assert.AreEqual(404, ex.Status);
            }
        }

        // Item is valid, no MatchConditions, 412 response with Conflict
        [TestMethod]
        public void DeleteItem_ValidItem_NoMatchConditions_Returns412_WithConflict()
        {
            var mock = new MockTransport();
            var table = GetTableReference(mock);
            var entity = new Entity { Id = "foo", Version = "bar" };
            mock.Response = new HttpResponseMessage { StatusCode = HttpStatusCode.NotFound };

            try
            {
                var response = table.DeleteItem(entity, new MatchConditions { IfMatch = ETag.All });
                Assert.Fail("RequestFailedException expected");
            }
            catch (RequestFailedException ex)
            {
                Assert.AreEqual(404, ex.Status);
            }
        }

        // Item is valid, MatchConditions blank, 204 response
        [TestMethod]
        public void DeleteItem_ValidItem_BlankMatchConditions_Returns204()
        {
            var mock = new MockTransport();
            var table = GetTableReference(mock);
            var entity = new Entity { Id = "foo", Version = "bar" };
            mock.Response = new HttpResponseMessage { StatusCode = HttpStatusCode.NoContent };

            var response = table.DeleteItem(entity, new MatchConditions());

            Assert.AreEqual(204, response.Status);
        }

        // Item is valid, MatchConditions blank, 404 response
        [TestMethod]
        public void DeleteItem_ValidItem_BlankMatchConditions_Returns404()
        {
            var mock = new MockTransport();
            var table = GetTableReference(mock);
            var entity = new Entity { Id = "foo", Version = "bar" };
            mock.Response = new HttpResponseMessage { StatusCode = HttpStatusCode.NotFound };

            try
            {
                var response = table.DeleteItem(entity, new MatchConditions());
                Assert.Fail("RequestFailedException expected");
            }
            catch (RequestFailedException ex)
            {
                Assert.AreEqual(404, ex.Status);
            }
        }

        // Item is valid, MatchConditions, If-Match *, 204 response
        [TestMethod]
        public void DeleteItem_ValidItem_IfMatchAny_Returns204()
        {
            var mock = new MockTransport();
            var table = GetTableReference(mock);
            var entity = new Entity { Id = "foo", Version = "bar" };
            mock.Response = new HttpResponseMessage { StatusCode = HttpStatusCode.NoContent };

            var response = table.DeleteItem(entity, new MatchConditions { IfMatch = ETag.All });

            Assert.AreEqual(204, response.Status);
        }

        // Item is valid, MatchConditions, If-Match *, 404 response
        [TestMethod]
        public void DeleteItem_ValidItem_IfMatchAny_Returns404()
        {
            var mock = new MockTransport();
            var table = GetTableReference(mock);
            var entity = new Entity { Id = "foo", Version = "bar" };
            mock.Response = new HttpResponseMessage { StatusCode = HttpStatusCode.NotFound };

            try
            {
                var response = table.DeleteItem(entity, new MatchConditions { IfMatch = ETag.All });
                Assert.Fail("RequestFailedException expected");
            }
            catch (RequestFailedException ex)
            {
                Assert.AreEqual(404, ex.Status);
            }
        }

        // Item is valid, MatchConditions, If-Match *, 412 response with conflict
        [TestMethod]
        public void DeleteItem_ValidItem_IfMatchAny_Returns412_WithConflict()
        {
            var mock = new MockTransport();
            var table = GetTableReference(mock);
            var entity = new Entity { Id = "foo", Version = "bar" };
            mock.Response = CreateEntityResponse(HttpStatusCode.PreconditionFailed);

            try
            {
                var response = table.DeleteItem(entity, new MatchConditions());
                Assert.Fail("RequestFailedException expected");
            }
            catch (ConflictException<Entity> ex)
            {
                Assert.AreEqual(412, ex.Status);
                Assert.IsNotNull(ex.ETag);
                Assert.IsNotNull(ex.LastModified);
                Assert.IsNotNull(ex.Value);
            }
        }

        // Item is valid, MatchConditions, If-Match version, 204 response
        [TestMethod]
        public void DeleteItem_ValidItem_IfMatchVersion_Returns204()
        {
            var mock = new MockTransport();
            var table = GetTableReference(mock);
            var entity = new Entity { Id = "foo", Version = "bar" };
            mock.Response = new HttpResponseMessage { StatusCode = HttpStatusCode.NoContent };

            var response = table.DeleteItem(entity, new MatchConditions { IfMatch = new ETag("bar") });

            Assert.AreEqual(204, response.Status);
        }

        // Item is valid, MatchConditions, If-Match version, 404 response
        [TestMethod]
        public void DeleteItem_ValidItem_IfMatchVersion_Returns404()
        {
            var mock = new MockTransport();
            var table = GetTableReference(mock);
            var entity = new Entity { Id = "foo", Version = "bar" };
            mock.Response = new HttpResponseMessage { StatusCode = HttpStatusCode.NotFound };

            try
            {
                var response = table.DeleteItem(entity, new MatchConditions { IfMatch = new ETag("bar") });
                Assert.Fail("RequestFailedException expected");
            }
            catch (RequestFailedException ex)
            {
                Assert.AreEqual(404, ex.Status);
            }
        }

        // Item is valid, MatchConditions, If-Match version, 412 response with conflict
        [TestMethod]
        public void DeleteItem_ValidItem_IfMatchVersion_Returns412()
        {
            var mock = new MockTransport();
            var table = GetTableReference(mock);
            var entity = new Entity { Id = "foo", Version = "bar" };
            mock.Response = CreateEntityResponse(HttpStatusCode.PreconditionFailed);

            try
            {
                var response = table.DeleteItem(entity, new MatchConditions { IfMatch = new ETag("bar") });
                Assert.Fail("RequestFailedException expected");
            }
            catch (ConflictException<Entity> ex)
            {
                Assert.AreEqual(412, ex.Status);
                Assert.IsNotNull(ex.ETag);
                Assert.IsNotNull(ex.LastModified);
                Assert.IsNotNull(ex.Value);
            }
        }
        #endregion

        #region GetItemAsync
        // Item is null
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetItemAsync_NullId_Throws()
        {
            var mock = new MockTransport();
            var table = GetTableReference(mock);

            _ = await table.GetItemAsync(null);
        }

        // Item is empty
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task GetItemAsync_EmptyId_Throws()
        {
            var mock = new MockTransport();
            var table = GetTableReference(mock);

            _ = await table.GetItemAsync("");
        }

        // Item is valid, 200 response
        [TestMethod]
        public async Task GetItemAsync_ValidItem_Returns200()
        {
            var mock = new MockTransport();
            var table = GetTableReference(mock);
            mock.Response = CreateEntityResponse(HttpStatusCode.OK);

            Response<Entity> response = await table.GetItemAsync("retid");

            // Check the response
            Assert.IsNotNull(response);
            var raw = response.GetRawResponse();
            Assert.AreEqual(200, raw.Status);
            HttpAssert.HeaderIsEqual("ETag", "\"abcdef\"", raw.Headers);
            HttpAssert.HeaderIsEqual("Last-Modified", "Thu, 27 Feb 2020 03:22:05 GMT", raw.Headers);
            Assert.AreEqual("retid", response.Value.Id);
        }

        // Item is valid, 403 response
        [TestMethod]
        public async Task GetItemAsync_ValidItem_Returns403()
        {
            var mock = new MockTransport();
            var table = GetTableReference(mock);
            mock.Response = new HttpResponseMessage { StatusCode = HttpStatusCode.Forbidden };

            try
            {
                Response<Entity> response = await table.GetItemAsync("retid");

                Assert.Fail("RequestFailedException expected");
            }
            catch (RequestFailedException ex)
            {
                Assert.AreEqual(403, ex.Status);
            }
        }

        // Item is valid, 404 response
        [TestMethod]
        public async Task GetItemAsync_ValidItem_Returns404()
        {
            var mock = new MockTransport();
            var table = GetTableReference(mock);
            mock.Response = new HttpResponseMessage { StatusCode = HttpStatusCode.NotFound };

            try
            {
                Response<Entity> response = await table.GetItemAsync("retid");

                Assert.Fail("RequestFailedException expected");
            }
            catch (RequestFailedException ex)
            {
                Assert.AreEqual(404, ex.Status);
            }
        }
        #endregion

        #region GetItem
        // Item is null
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetItem_NullId_Throws()
        {
            var mock = new MockTransport();
            var table = GetTableReference(mock);

            _ = table.GetItem(null);
        }

        // Item is empty
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void GetItem_EmptyId_Throws()
        {
            var mock = new MockTransport();
            var table = GetTableReference(mock);

            _ = table.GetItem("");
        }

        // Item is valid, 200 response
        [TestMethod]
        public void GetItem_ValidItem_Returns200()
        {
            var mock = new MockTransport();
            var table = GetTableReference(mock);
            mock.Response = CreateEntityResponse(HttpStatusCode.OK);

            Response<Entity> response = table.GetItem("retid");

            // Check the response
            Assert.IsNotNull(response);
            var raw = response.GetRawResponse();
            Assert.AreEqual(200, raw.Status);
            HttpAssert.HeaderIsEqual("ETag", "\"abcdef\"", raw.Headers);
            HttpAssert.HeaderIsEqual("Last-Modified", "Thu, 27 Feb 2020 03:22:05 GMT", raw.Headers);
            Assert.AreEqual("retid", response.Value.Id);
        }

        // Item is valid, 403 response
        [TestMethod]
        public void GetItem_ValidItem_Returns403()
        {
            var mock = new MockTransport();
            var table = GetTableReference(mock);
            mock.Response = new HttpResponseMessage { StatusCode = HttpStatusCode.Forbidden };

            try
            {
                Response<Entity> response = table.GetItem("retid");

                Assert.Fail("RequestFailedException expected");
            }
            catch (RequestFailedException ex)
            {
                Assert.AreEqual(403, ex.Status);
            }
        }

        // Item is valid, 404 response
        [TestMethod]
        public void GetItem_ValidItem_Returns404()
        {
            var mock = new MockTransport();
            var table = GetTableReference(mock);
            mock.Response = new HttpResponseMessage { StatusCode = HttpStatusCode.NotFound };

            try
            {
                Response<Entity> response = table.GetItem("retid");

                Assert.Fail("RequestFailedException expected");
            }
            catch (RequestFailedException ex)
            {
                Assert.AreEqual(404, ex.Status);
            }
        }
        #endregion

        #region GetItemsAsync
        #endregion

        #region GetItems
        #endregion

        #region ReplaceItemAsync
        // Item is null
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ReplaceItemAsync_NullItem_Throws()
        {
            var mock = new MockTransport();
            var table = GetTableReference(mock);

            _ = await table.ReplaceItemAsync(null);
        }

        // Item.Id is null
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ReplaceItemAsync_NullId_Throws()
        {
            var mock = new MockTransport();
            var table = GetTableReference(mock);

            _ = await table.ReplaceItemAsync(new Entity { Id = null });
        }

        // Item is valid, no match conditions, 200 response
        [TestMethod]
        public async Task ReplaceItemAsync_ValidItem_NoMatchConditions_Returns200()
        {
            var mock = new MockTransport();
            var table = GetTableReference(mock);
            Entity sentEntity = new Entity { Id = "retid", Data = "retdata", Deleted = false, UpdatedAt = DateTimeOffset.Parse("2020-02-27T03:22:05.000Z"), Version = "abcdef" };
            mock.Response = CreateEntityResponse(HttpStatusCode.OK);

            Response<Entity> response = await table.ReplaceItemAsync(sentEntity);

            // Check the response
            Assert.IsNotNull(response);
            var raw = response.GetRawResponse();
            Assert.AreEqual(200, raw.Status);
            HttpAssert.HeaderIsEqual("ETag", "\"abcdef\"", raw.Headers);
            HttpAssert.HeaderIsEqual("Last-Modified", "Thu, 27 Feb 2020 03:22:05 GMT", raw.Headers);
            Assert.AreEqual("retid", response.Value.Id);
        }

        // Item is valid, no match conditions, 403 response
        [TestMethod]
        public async Task ReplaceItemAsync_ValidItem_NoMatchConditions_Returns403()
        {
            var mock = new MockTransport();
            var table = GetTableReference(mock);
            Entity sentEntity = new Entity { Id = "retid", Data = "retdata", Deleted = false, UpdatedAt = DateTimeOffset.Parse("2020-02-27T03:22:05.000Z"), Version = "abcdef" };
            mock.Response = new HttpResponseMessage { StatusCode = HttpStatusCode.Forbidden };

            try
            {
                Response<Entity> response = await table.ReplaceItemAsync(sentEntity);
                Assert.Fail("RequestFailedException expected");
            }
            catch (RequestFailedException ex)
            {
                Assert.AreEqual(403, ex.Status);
            }
        }

        // Item is valid, no match conditions, 404 response
        [TestMethod]
        public async Task ReplaceItemAsync_ValidItem_NoMatchConditions_Returns404()
        {
            var mock = new MockTransport();
            var table = GetTableReference(mock);
            Entity sentEntity = new Entity { Id = "retid", Data = "retdata", Deleted = false, UpdatedAt = DateTimeOffset.Parse("2020-02-27T03:22:05.000Z"), Version = "abcdef" };
            mock.Response = new HttpResponseMessage { StatusCode = HttpStatusCode.NotFound };

            try
            {
                Response<Entity> response = await table.ReplaceItemAsync(sentEntity);
                Assert.Fail("RequestFailedException expected");
            }
            catch (RequestFailedException ex)
            {
                Assert.AreEqual(404, ex.Status);
            }
        }

        // Item is valid, no match conditions, 409 response with conflict
        [TestMethod]
        public async Task ReplaceItemAsync_ValidItem_NoMatchConditions_Returns409_WithConflict()
        {
            var mock = new MockTransport();
            var table = GetTableReference(mock);
            Entity sentEntity = new Entity { Id = "retid", Data = "retdata", Deleted = false, UpdatedAt = DateTimeOffset.Parse("2020-02-27T03:22:05.000Z"), Version = "abcdef" };
            mock.Response = CreateEntityResponse(HttpStatusCode.Conflict);

            try
            {
                Response<Entity> response = await table.ReplaceItemAsync(sentEntity);
                Assert.Fail("RequestFailedException expected");
            }
            catch (ConflictException<Entity> ex)
            {
                Assert.AreEqual(409, ex.Status);
                Assert.IsNotNull(ex.ETag);
                Assert.IsNotNull(ex.LastModified);
                Assert.IsNotNull(ex.Value);
            }
        }

        // Item is valid, no match conditions, 412 response with conflict
        [TestMethod]
        public async Task ReplaceItemAsync_ValidItem_NoMatchConditions_Returns412_WithConflict()
        {
            var mock = new MockTransport();
            var table = GetTableReference(mock);
            Entity sentEntity = new Entity { Id = "retid", Data = "retdata", Deleted = false, UpdatedAt = DateTimeOffset.Parse("2020-02-27T03:22:05.000Z"), Version = "abcdef" };
            mock.Response = CreateEntityResponse(HttpStatusCode.PreconditionFailed);

            try
            {
                Response<Entity> response = await table.ReplaceItemAsync(sentEntity);
                Assert.Fail("RequestFailedException expected");
            }
            catch (ConflictException<Entity> ex)
            {
                Assert.AreEqual(412, ex.Status);
                Assert.IsNotNull(ex.ETag);
                Assert.IsNotNull(ex.LastModified);
                Assert.IsNotNull(ex.Value);
            }
        }

        // Item is valid, match if-match *, 200 response
        [TestMethod]
        public async Task ReplaceItemAsync_ValidItem_IfMatchAny_Returns200()
        {
            var mock = new MockTransport();
            var table = GetTableReference(mock);
            Entity sentEntity = new Entity { Id = "retid", Data = "retdata", Deleted = false, UpdatedAt = DateTimeOffset.Parse("2020-02-27T03:22:05.000Z"), Version = "abcdef" };
            mock.Response = CreateEntityResponse(HttpStatusCode.OK);

            Response<Entity> response = await table.ReplaceItemAsync(sentEntity, new MatchConditions { IfMatch = ETag.All });

            // Check the response
            Assert.IsNotNull(response);
            var raw = response.GetRawResponse();
            Assert.AreEqual(200, raw.Status);
            HttpAssert.HeaderIsEqual("ETag", "\"abcdef\"", raw.Headers);
            HttpAssert.HeaderIsEqual("Last-Modified", "Thu, 27 Feb 2020 03:22:05 GMT", raw.Headers);
            Assert.AreEqual("retid", response.Value.Id);
        }

        // Item is valid, match if-match *, 404 response
        [TestMethod]
        public async Task ReplaceItemAsync_ValidItem_IfMatchAny_Returns404()
        {
            var mock = new MockTransport();
            var table = GetTableReference(mock);
            Entity sentEntity = new Entity { Id = "retid", Data = "retdata", Deleted = false, UpdatedAt = DateTimeOffset.Parse("2020-02-27T03:22:05.000Z"), Version = "abcdef" };
            mock.Response = new HttpResponseMessage { StatusCode = HttpStatusCode.NotFound };

            try
            {
                Response<Entity> response = await table.ReplaceItemAsync(sentEntity, new MatchConditions { IfMatch = ETag.All });
                Assert.Fail("RequestFailedException expected");
            }
            catch (RequestFailedException ex)
            {
                Assert.AreEqual(404, ex.Status);
            }
        }

        // Item is valid, match if-match *, 409 response with conflict
        [TestMethod]
        public async Task ReplaceItemAsync_ValidItem_IfMatchAny_Returns409_WithConflict()
        {
            var mock = new MockTransport();
            var table = GetTableReference(mock);
            Entity sentEntity = new Entity { Id = "retid", Data = "retdata", Deleted = false, UpdatedAt = DateTimeOffset.Parse("2020-02-27T03:22:05.000Z"), Version = "abcdef" };
            mock.Response = CreateEntityResponse(HttpStatusCode.Conflict);

            try
            {
                Response<Entity> response = await table.ReplaceItemAsync(sentEntity, new MatchConditions { IfMatch = ETag.All });
                Assert.Fail("RequestFailedException expected");
            }
            catch (ConflictException<Entity> ex)
            {
                Assert.AreEqual(409, ex.Status);
                Assert.IsNotNull(ex.ETag);
                Assert.IsNotNull(ex.LastModified);
                Assert.IsNotNull(ex.Value);
            }
        }

        // Item is valid, match if-match *, 412 response with conflict
        [TestMethod]
        public async Task ReplaceItemAsync_ValidItem_IfMatchAny_Returns412_WithConflict()
        {
            var mock = new MockTransport();
            var table = GetTableReference(mock);
            Entity sentEntity = new Entity { Id = "retid", Data = "retdata", Deleted = false, UpdatedAt = DateTimeOffset.Parse("2020-02-27T03:22:05.000Z"), Version = "abcdef" };
            mock.Response = CreateEntityResponse(HttpStatusCode.PreconditionFailed);

            try
            {
                Response<Entity> response = await table.ReplaceItemAsync(sentEntity, new MatchConditions { IfMatch = ETag.All });
                Assert.Fail("RequestFailedException expected");
            }
            catch (ConflictException<Entity> ex)
            {
                Assert.AreEqual(412, ex.Status);
                Assert.IsNotNull(ex.ETag);
                Assert.IsNotNull(ex.LastModified);
                Assert.IsNotNull(ex.Value);
            }
        }

        // Item is valid, match if-match version, 200 response
        [TestMethod]
        public async Task ReplaceItemAsync_ValidItem_IfMatchVersion_Returns200()
        {
            var mock = new MockTransport();
            var table = GetTableReference(mock);
            Entity sentEntity = new Entity { Id = "retid", Data = "retdata", Deleted = false, UpdatedAt = DateTimeOffset.Parse("2020-02-27T03:22:05.000Z"), Version = "abcdef" };
            mock.Response = CreateEntityResponse(HttpStatusCode.OK);

            Response<Entity> response = await table.ReplaceItemAsync(sentEntity, new MatchConditions { IfMatch = new ETag("abcdef") });

            // Check the response
            Assert.IsNotNull(response);
            var raw = response.GetRawResponse();
            Assert.AreEqual(200, raw.Status);
            HttpAssert.HeaderIsEqual("ETag", "\"abcdef\"", raw.Headers);
            HttpAssert.HeaderIsEqual("Last-Modified", "Thu, 27 Feb 2020 03:22:05 GMT", raw.Headers);
            Assert.AreEqual("retid", response.Value.Id);
        }

        // Item is valid, match if-match version, 404 response
        [TestMethod]
        public async Task ReplaceItemAsync_ValidItem_IfMatchVersion_Returns404()
        {
            var mock = new MockTransport();
            var table = GetTableReference(mock);
            Entity sentEntity = new Entity { Id = "retid", Data = "retdata", Deleted = false, UpdatedAt = DateTimeOffset.Parse("2020-02-27T03:22:05.000Z"), Version = "abcdef" };
            mock.Response = new HttpResponseMessage { StatusCode = HttpStatusCode.NotFound };

            try
            {
                Response<Entity> response = await table.ReplaceItemAsync(sentEntity, new MatchConditions { IfMatch = new ETag("abcdef") });
                Assert.Fail("RequestFailedException expected");
            }
            catch (RequestFailedException ex)
            {
                Assert.AreEqual(404, ex.Status);
            }
        }

        // Item is valid, match if-match version, 409 response with conflict
        [TestMethod]
        public async Task ReplaceItemAsync_ValidItem_IfMatchVersion_Returns409_WithConflict()
        {
            var mock = new MockTransport();
            var table = GetTableReference(mock);
            Entity sentEntity = new Entity { Id = "retid", Data = "retdata", Deleted = false, UpdatedAt = DateTimeOffset.Parse("2020-02-27T03:22:05.000Z"), Version = "abcdef" };
            mock.Response = CreateEntityResponse(HttpStatusCode.Conflict);

            try
            {
                Response<Entity> response = await table.ReplaceItemAsync(sentEntity, new MatchConditions { IfMatch = new ETag("abcdef") });
                Assert.Fail("RequestFailedException expected");
            }
            catch (ConflictException<Entity> ex)
            {
                Assert.AreEqual(409, ex.Status);
                Assert.IsNotNull(ex.ETag);
                Assert.IsNotNull(ex.LastModified);
                Assert.IsNotNull(ex.Value);
            }
        }

        // Item is valid, match if-match version, 412 response with conflict
        [TestMethod]
        public async Task ReplaceItemAsync_ValidItem_IfMatchVersion_Returns412_WithConflict()
        {
            var mock = new MockTransport();
            var table = GetTableReference(mock);
            Entity sentEntity = new Entity { Id = "retid", Data = "retdata", Deleted = false, UpdatedAt = DateTimeOffset.Parse("2020-02-27T03:22:05.000Z"), Version = "abcdef" };
            mock.Response = CreateEntityResponse(HttpStatusCode.PreconditionFailed);

            try
            {
                Response<Entity> response = await table.ReplaceItemAsync(sentEntity, new MatchConditions { IfMatch = new ETag("abcdef") });
                Assert.Fail("RequestFailedException expected");
            }
            catch (ConflictException<Entity> ex)
            {
                Assert.AreEqual(412, ex.Status);
                Assert.IsNotNull(ex.ETag);
                Assert.IsNotNull(ex.LastModified);
                Assert.IsNotNull(ex.Value);
            }
        }
        #endregion

        #region ReplaceItem
        // Item is null
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ReplaceItem_NullItem_Throws()
        {
            var mock = new MockTransport();
            var table = GetTableReference(mock);

            _ = table.ReplaceItem(null);
        }

        // Item.Id is null
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ReplaceItem_NullId_Throws()
        {
            var mock = new MockTransport();
            var table = GetTableReference(mock);

            _ = table.ReplaceItem(new Entity { Id = null });
        }

        // Item is valid, no match conditions, 200 response
        [TestMethod]
        public void ReplaceItem_ValidItem_NoMatchConditions_Returns200()
        {
            var mock = new MockTransport();
            var table = GetTableReference(mock);
            Entity sentEntity = new Entity { Id = "retid", Data = "retdata", Deleted = false, UpdatedAt = DateTimeOffset.Parse("2020-02-27T03:22:05.000Z"), Version = "abcdef" };
            mock.Response = CreateEntityResponse(HttpStatusCode.OK);

            Response<Entity> response = table.ReplaceItem(sentEntity);

            // Check the response
            Assert.IsNotNull(response);
            var raw = response.GetRawResponse();
            Assert.AreEqual(200, raw.Status);
            HttpAssert.HeaderIsEqual("ETag", "\"abcdef\"", raw.Headers);
            HttpAssert.HeaderIsEqual("Last-Modified", "Thu, 27 Feb 2020 03:22:05 GMT", raw.Headers);
            Assert.AreEqual("retid", response.Value.Id);
        }

        // Item is valid, no match conditions, 403 response
        [TestMethod]
        public void ReplaceItem_ValidItem_NoMatchConditions_Returns403()
        {
            var mock = new MockTransport();
            var table = GetTableReference(mock);
            Entity sentEntity = new Entity { Id = "retid", Data = "retdata", Deleted = false, UpdatedAt = DateTimeOffset.Parse("2020-02-27T03:22:05.000Z"), Version = "abcdef" };
            mock.Response = new HttpResponseMessage { StatusCode = HttpStatusCode.Forbidden };

            try
            {
                Response<Entity> response = table.ReplaceItem(sentEntity);
                Assert.Fail("RequestFailedException expected");
            }
            catch (RequestFailedException ex)
            {
                Assert.AreEqual(403, ex.Status);
            }
        }

        // Item is valid, no match conditions, 404 response
        [TestMethod]
        public void ReplaceItem_ValidItem_NoMatchConditions_Returns404()
        {
            var mock = new MockTransport();
            var table = GetTableReference(mock);
            Entity sentEntity = new Entity { Id = "retid", Data = "retdata", Deleted = false, UpdatedAt = DateTimeOffset.Parse("2020-02-27T03:22:05.000Z"), Version = "abcdef" };
            mock.Response = new HttpResponseMessage { StatusCode = HttpStatusCode.NotFound };

            try
            {
                Response<Entity> response = table.ReplaceItem(sentEntity);
                Assert.Fail("RequestFailedException expected");
            }
            catch (RequestFailedException ex)
            {
                Assert.AreEqual(404, ex.Status);
            }
        }

        // Item is valid, no match conditions, 409 response with conflict
        [TestMethod]
        public void ReplaceItem_ValidItem_NoMatchConditions_Returns409_WithConflict()
        {
            var mock = new MockTransport();
            var table = GetTableReference(mock);
            Entity sentEntity = new Entity { Id = "retid", Data = "retdata", Deleted = false, UpdatedAt = DateTimeOffset.Parse("2020-02-27T03:22:05.000Z"), Version = "abcdef" };
            mock.Response = CreateEntityResponse(HttpStatusCode.Conflict);

            try
            {
                Response<Entity> response = table.ReplaceItem(sentEntity);
                Assert.Fail("RequestFailedException expected");
            }
            catch (ConflictException<Entity> ex)
            {
                Assert.AreEqual(409, ex.Status);
                Assert.IsNotNull(ex.ETag);
                Assert.IsNotNull(ex.LastModified);
                Assert.IsNotNull(ex.Value);
            }
        }

        // Item is valid, no match conditions, 412 response with conflict
        [TestMethod]
        public void ReplaceItem_ValidItem_NoMatchConditions_Returns412_WithConflict()
        {
            var mock = new MockTransport();
            var table = GetTableReference(mock);
            Entity sentEntity = new Entity { Id = "retid", Data = "retdata", Deleted = false, UpdatedAt = DateTimeOffset.Parse("2020-02-27T03:22:05.000Z"), Version = "abcdef" };
            mock.Response = CreateEntityResponse(HttpStatusCode.PreconditionFailed);

            try
            {
                Response<Entity> response = table.ReplaceItem(sentEntity);
                Assert.Fail("RequestFailedException expected");
            }
            catch (ConflictException<Entity> ex)
            {
                Assert.AreEqual(412, ex.Status);
                Assert.IsNotNull(ex.ETag);
                Assert.IsNotNull(ex.LastModified);
                Assert.IsNotNull(ex.Value);
            }
        }

        // Item is valid, match if-match *, 200 response
        [TestMethod]
        public void ReplaceItem_ValidItem_IfMatchAny_Returns200()
        {
            var mock = new MockTransport();
            var table = GetTableReference(mock);
            Entity sentEntity = new Entity { Id = "retid", Data = "retdata", Deleted = false, UpdatedAt = DateTimeOffset.Parse("2020-02-27T03:22:05.000Z"), Version = "abcdef" };
            mock.Response = CreateEntityResponse(HttpStatusCode.OK);

            Response<Entity> response = table.ReplaceItem(sentEntity, new MatchConditions { IfMatch = ETag.All });

            // Check the response
            Assert.IsNotNull(response);
            var raw = response.GetRawResponse();
            Assert.AreEqual(200, raw.Status);
            HttpAssert.HeaderIsEqual("ETag", "\"abcdef\"", raw.Headers);
            HttpAssert.HeaderIsEqual("Last-Modified", "Thu, 27 Feb 2020 03:22:05 GMT", raw.Headers);
            Assert.AreEqual("retid", response.Value.Id);
        }

        // Item is valid, match if-match *, 404 response
        [TestMethod]
        public void ReplaceItem_ValidItem_IfMatchAny_Returns404()
        {
            var mock = new MockTransport();
            var table = GetTableReference(mock);
            Entity sentEntity = new Entity { Id = "retid", Data = "retdata", Deleted = false, UpdatedAt = DateTimeOffset.Parse("2020-02-27T03:22:05.000Z"), Version = "abcdef" };
            mock.Response = new HttpResponseMessage { StatusCode = HttpStatusCode.NotFound };

            try
            {
                Response<Entity> response = table.ReplaceItem(sentEntity, new MatchConditions { IfMatch = ETag.All });
                Assert.Fail("RequestFailedException expected");
            }
            catch (RequestFailedException ex)
            {
                Assert.AreEqual(404, ex.Status);
            }
        }

        // Item is valid, match if-match *, 409 response with conflict
        [TestMethod]
        public void ReplaceItem_ValidItem_IfMatchAny_Returns409_WithConflict()
        {
            var mock = new MockTransport();
            var table = GetTableReference(mock);
            Entity sentEntity = new Entity { Id = "retid", Data = "retdata", Deleted = false, UpdatedAt = DateTimeOffset.Parse("2020-02-27T03:22:05.000Z"), Version = "abcdef" };
            mock.Response = CreateEntityResponse(HttpStatusCode.Conflict);

            try
            {
                Response<Entity> response = table.ReplaceItem(sentEntity, new MatchConditions { IfMatch = ETag.All });
                Assert.Fail("RequestFailedException expected");
            }
            catch (ConflictException<Entity> ex)
            {
                Assert.AreEqual(409, ex.Status);
                Assert.IsNotNull(ex.ETag);
                Assert.IsNotNull(ex.LastModified);
                Assert.IsNotNull(ex.Value);
            }
        }

        // Item is valid, match if-match *, 412 response with conflict
        [TestMethod]
        public void ReplaceItem_ValidItem_IfMatchAny_Returns412_WithConflict()
        {
            var mock = new MockTransport();
            var table = GetTableReference(mock);
            Entity sentEntity = new Entity { Id = "retid", Data = "retdata", Deleted = false, UpdatedAt = DateTimeOffset.Parse("2020-02-27T03:22:05.000Z"), Version = "abcdef" };
            mock.Response = CreateEntityResponse(HttpStatusCode.PreconditionFailed);

            try
            {
                Response<Entity> response = table.ReplaceItem(sentEntity, new MatchConditions { IfMatch = ETag.All });
                Assert.Fail("RequestFailedException expected");
            }
            catch (ConflictException<Entity> ex)
            {
                Assert.AreEqual(412, ex.Status);
                Assert.IsNotNull(ex.ETag);
                Assert.IsNotNull(ex.LastModified);
                Assert.IsNotNull(ex.Value);
            }
        }

        // Item is valid, match if-match version, 200 response
        [TestMethod]
        public void ReplaceItem_ValidItem_IfMatchVersion_Returns200()
        {
            var mock = new MockTransport();
            var table = GetTableReference(mock);
            Entity sentEntity = new Entity { Id = "retid", Data = "retdata", Deleted = false, UpdatedAt = DateTimeOffset.Parse("2020-02-27T03:22:05.000Z"), Version = "abcdef" };
            mock.Response = CreateEntityResponse(HttpStatusCode.OK);

            Response<Entity> response = table.ReplaceItem(sentEntity, new MatchConditions { IfMatch = new ETag("abcdef") });

            // Check the response
            Assert.IsNotNull(response);
            var raw = response.GetRawResponse();
            Assert.AreEqual(200, raw.Status);
            HttpAssert.HeaderIsEqual("ETag", "\"abcdef\"", raw.Headers);
            HttpAssert.HeaderIsEqual("Last-Modified", "Thu, 27 Feb 2020 03:22:05 GMT", raw.Headers);
            Assert.AreEqual("retid", response.Value.Id);
        }

        // Item is valid, match if-match version, 404 response
        [TestMethod]
        public void ReplaceItem_ValidItem_IfMatchVersion_Returns404()
        {
            var mock = new MockTransport();
            var table = GetTableReference(mock);
            Entity sentEntity = new Entity { Id = "retid", Data = "retdata", Deleted = false, UpdatedAt = DateTimeOffset.Parse("2020-02-27T03:22:05.000Z"), Version = "abcdef" };
            mock.Response = new HttpResponseMessage { StatusCode = HttpStatusCode.NotFound };

            try
            {
                Response<Entity> response = table.ReplaceItem(sentEntity, new MatchConditions { IfMatch = new ETag("abcdef") });
                Assert.Fail("RequestFailedException expected");
            }
            catch (RequestFailedException ex)
            {
                Assert.AreEqual(404, ex.Status);
            }
        }

        // Item is valid, match if-match version, 409 response with conflict
        [TestMethod]
        public void ReplaceItem_ValidItem_IfMatchVersion_Returns409_WithConflict()
        {
            var mock = new MockTransport();
            var table = GetTableReference(mock);
            Entity sentEntity = new Entity { Id = "retid", Data = "retdata", Deleted = false, UpdatedAt = DateTimeOffset.Parse("2020-02-27T03:22:05.000Z"), Version = "abcdef" };
            mock.Response = CreateEntityResponse(HttpStatusCode.Conflict);

            try
            {
                Response<Entity> response = table.ReplaceItem(sentEntity, new MatchConditions { IfMatch = new ETag("abcdef") });
                Assert.Fail("RequestFailedException expected");
            }
            catch (ConflictException<Entity> ex)
            {
                Assert.AreEqual(409, ex.Status);
                Assert.IsNotNull(ex.ETag);
                Assert.IsNotNull(ex.LastModified);
                Assert.IsNotNull(ex.Value);
            }
        }

        // Item is valid, match if-match version, 412 response with conflict
        [TestMethod]
        public void ReplaceItem_ValidItem_IfMatchVersion_Returns412_WithConflict()
        {
            var mock = new MockTransport();
            var table = GetTableReference(mock);
            Entity sentEntity = new Entity { Id = "retid", Data = "retdata", Deleted = false, UpdatedAt = DateTimeOffset.Parse("2020-02-27T03:22:05.000Z"), Version = "abcdef" };
            mock.Response = CreateEntityResponse(HttpStatusCode.PreconditionFailed);

            try
            {
                Response<Entity> response = table.ReplaceItem(sentEntity, new MatchConditions { IfMatch = new ETag("abcdef") });
                Assert.Fail("RequestFailedException expected");
            }
            catch (ConflictException<Entity> ex)
            {
                Assert.AreEqual(412, ex.Status);
                Assert.IsNotNull(ex.ETag);
                Assert.IsNotNull(ex.LastModified);
                Assert.IsNotNull(ex.Value);
            }
        }
        #endregion
    }
}
