// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Azure.Mobile.Server.Config;
using Microsoft.Azure.Mobile.Server.Tables.Config;
using Microsoft.Azure.Mobile.Server.TestModels;
using Microsoft.Owin.Testing;
using Owin;
using TestUtilities;
using Xunit;

namespace Microsoft.Azure.Mobile.Server
{
    public class EntityTableControllerUpdateTests : IClassFixture<EntityTableControllerUpdateTests.TestContext>
    {
        private const string Address = "tables/testentity";
        private HttpConfiguration config;
        private HttpClient testClient;

        public EntityTableControllerUpdateTests(EntityTableControllerUpdateTests.TestContext data)
        {
            this.testClient = data.Server.HttpClient;
            this.config = data.Config;
        }

        [Fact]
        public async Task Post_EntityIsCreated_ReturnsCreatedWithLocationHeader()
        {
            TestEntitySimple entity = new TestEntitySimple
            {
                StringValue = "Apple",
                IntValue = 1
            };
            Uri location;

            // Post a new entity and verify entity returned
            using (HttpResponseMessage postResponse = await this.testClient.PostAsJsonAsync(Address, entity))
            {
                Assert.Equal(HttpStatusCode.Created, postResponse.StatusCode);
                location = postResponse.Headers.Location;
                Assert.NotNull(location);

                TestEntity result = await postResponse.Content.ReadAsAsync<TestEntity>();
                this.VerifySimpleEntitiesEqual(entity, result);
            }

            // Query the entity back using location header value to ensure it was inserted to the db
            UriBuilder queryUri = new UriBuilder(location) { };
            using (HttpResponseMessage queryResponse = await this.testClient.GetAsync(queryUri.Uri))
            {
                Assert.Equal(HttpStatusCode.OK, queryResponse.StatusCode);
                TestEntity result = await queryResponse.Content.ReadAsAsync<TestEntity>();
                this.VerifySimpleEntitiesEqual(entity, result);

                Assert.NotNull(result.CreatedAt);
                Assert.NotNull(result.UpdatedAt);
                Assert.NotNull(result.Version);
            }
        }

        [Fact]
        public async Task Post_Throws_ExtraMembers()
        {
            var entity = new TestInvalidEntity
            {
                StringValue = "Hello",
                UnknownProperty = "Value"
            };

            // Post a new entity and verify entity returned
            using (HttpResponseMessage postResponse = await this.testClient.PostAsJsonAsync(Address, entity))
            {
                Assert.Equal(HttpStatusCode.BadRequest, postResponse.StatusCode);
            }
        }

        [Fact]
        public async Task Patch_EntityIsUpdated_ReturnsOk()
        {
            TestEntity entity = CreateTestEntity();
            DateTimeOffset? createdAt;
            DateTimeOffset? updatedAt;
            string id;
            Uri location;

            // Create a new entity to update
            using (HttpResponseMessage postResponse = await this.testClient.PostAsJsonAsync(Address, entity))
            {
                Assert.Equal(HttpStatusCode.Created, postResponse.StatusCode);
                location = postResponse.Headers.Location;

                TestEntity result = await postResponse.Content.ReadAsAsync<TestEntity>();
                this.VerifyEntitiesEqual(entity, result);

                Assert.NotNull(result.CreatedAt);
                Assert.NotNull(result.UpdatedAt);
                Assert.NotNull(result.Version);

                createdAt = result.CreatedAt;
                updatedAt = result.UpdatedAt;
                id = result.Id;
            }

            // Delay slightly to avoid resolution conflict of less than 1 ms.
            await Task.Delay(50);

            // Update some properties
            var patchEntity = new TestEntitySimple
            {
                Id = entity.Id,
                StringValue = "Updated",
                IntValue = 84
            };

            HttpRequestMessage request = new HttpRequestMessage
            {
                RequestUri = location,
                Method = new HttpMethod("PATCH"),
                Content = (HttpContent)new ObjectContent<TestEntitySimple>(patchEntity, this.config.Formatters.JsonFormatter)
            };
            using (HttpResponseMessage patchResponse = await this.testClient.SendAsync(request))
            {
                Assert.Equal(HttpStatusCode.OK, patchResponse.StatusCode);

                TestEntity result = await patchResponse.Content.ReadAsAsync<TestEntity>();
                this.VerifySimpleEntitiesEqual(patchEntity, result);

                Assert.Equal(createdAt, result.CreatedAt);
                Assert.NotEqual(updatedAt, result.UpdatedAt);
                Assert.NotNull(result.Version);
            }

            // Query the entity back using location header value to ensure it was updated
            UriBuilder queryUri = new UriBuilder(location);
            using (HttpResponseMessage queryResponse = await this.testClient.GetAsync(queryUri.Uri))
            {
                Assert.Equal(HttpStatusCode.OK, queryResponse.StatusCode);
                TestEntity result = await queryResponse.Content.ReadAsAsync<TestEntity>();
                this.VerifySimpleEntitiesEqual(patchEntity, result);

                Assert.NotNull(result.CreatedAt);
                Assert.NotNull(result.UpdatedAt);
                Assert.NotNull(result.Version);
            }
        }

        [Fact]
        public async Task Patch_Returns_PreconditionFailed_WithOriginalValue_IfVersionMismatch()
        {
            TestEntity entity = CreateTestEntity();
            string stringValue = entity.StringValue;
            int intValue = entity.IntValue;
            Uri location;

            // create a new entity to update
            using (HttpResponseMessage postResponse = await this.testClient.PostAsJsonAsync(Address, entity))
            {
                Assert.Equal(HttpStatusCode.Created, postResponse.StatusCode);
                location = postResponse.Headers.Location;
                entity = await postResponse.Content.ReadAsAsync<TestEntity>();
                string s = await postResponse.Content.ReadAsStringAsync();
            }

            // Update some properties
            TestEntitySimple updatedEntity = new TestEntitySimple()
            {
                Id = entity.Id,
                StringValue = "Updated",
                IntValue = 84
            };

            HttpRequestMessage patchRequest = new HttpRequestMessage
            {
                RequestUri = location,
                Method = new HttpMethod("PATCH"),
                Content = (HttpContent)new ObjectContent<TestEntitySimple>(updatedEntity, this.config.Formatters.JsonFormatter)
            };
            patchRequest.Headers.IfMatch.Add(EntityTagHeaderValue.Parse("\"QUJDREVG\""));

            using (HttpResponseMessage patchResponse = await this.testClient.SendAsync(patchRequest))
            {
                TestEntity conflict = await patchResponse.Content.ReadAsAsync<TestEntity>();
                Assert.Equal(HttpStatusCode.PreconditionFailed, patchResponse.StatusCode);
                Assert.Equal(entity.CreatedAt, conflict.CreatedAt);
                Assert.Equal(entity.UpdatedAt, conflict.UpdatedAt);
            }

            // Query the entity back using location header value to ensure it was *not* updated
            UriBuilder queryUri = new UriBuilder(location) { };
            using (HttpResponseMessage response = await this.testClient.GetAsync(queryUri.Uri))
            {
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                TestEntity result = await response.Content.ReadAsAsync<TestEntity>();
                string s = await response.Content.ReadAsStringAsync();

                Assert.Equal(stringValue, result.StringValue);
                Assert.Equal(intValue, result.IntValue);
                Assert.NotNull(result.CreatedAt);
                Assert.NotNull(result.UpdatedAt);
            }
        }

        [Fact]
        public async Task Put_Throws_ExtraMembers()
        {
            TestEntity entity = CreateTestEntity();
            string stringValue = entity.StringValue;
            int intValue = entity.IntValue;

            // create a new entity to update
            using (HttpResponseMessage postResponse = await this.testClient.PostAsJsonAsync(Address, entity))
            {
                Assert.Equal(HttpStatusCode.Created, postResponse.StatusCode);
                entity = await postResponse.Content.ReadAsAsync<TestEntity>();
            }

            var patchEntity = new TestInvalidEntity
            {
                Id = entity.Id,
                StringValue = "Hello",
                UnknownProperty = "Value"
            };

            var location = new Uri(this.testClient.BaseAddress, Address + "/" + entity.Id);
            HttpRequestMessage putRequest = new HttpRequestMessage
            {
                RequestUri = location,
                Method = new HttpMethod("PUT"),
                Content = (HttpContent)new ObjectContent<TestInvalidEntity>(patchEntity, this.config.Formatters.JsonFormatter)
            };

            using (HttpResponseMessage putResponse = await this.testClient.SendAsync(putRequest))
            {
                Assert.Equal(HttpStatusCode.BadRequest, putResponse.StatusCode);
            }
        }

        [Fact]
        public async Task Replace_EntityIsUpdated_ReturnsOk()
        {
            TestEntity entity = CreateTestEntity();
            DateTimeOffset? createdAt;
            DateTimeOffset? updatedAt;
            string id;
            Uri location;

            // Create a new entity to update
            // TODO: When switching away from HttpServer, entity is no longer replaced on the
            //       call to PostAsJsonAsync. Perhaps b/c ContentLength is set?
            using (HttpResponseMessage postResponse = await this.testClient.PostAsJsonAsync(Address, entity))
            {
                Assert.Equal(HttpStatusCode.Created, postResponse.StatusCode);
                location = postResponse.Headers.Location;

                TestEntity result = await postResponse.Content.ReadAsAsync<TestEntity>();
                this.VerifyEntitiesEqual(entity, result);
                entity = result;

                Assert.NotNull(result.CreatedAt);
                Assert.NotNull(result.UpdatedAt);
                Assert.NotNull(result.Version);

                createdAt = result.CreatedAt;
                updatedAt = result.UpdatedAt;
                entity.Version = result.Version;
                id = result.Id;
            }

            // Delay slightly to avoid resolution conflict of less than 1 ms.
            await Task.Delay(50);

            // Update some properties
            entity.StringValue = "Updated";
            entity.IntValue = 84;
            using (HttpResponseMessage putResponse = await this.testClient.PutAsJsonAsync(location, entity))
            {
                Assert.Equal(HttpStatusCode.OK, putResponse.StatusCode);

                TestEntity result = await putResponse.Content.ReadAsAsync<TestEntity>();
                this.VerifyEntitiesEqual(entity, result);

                Assert.Equal(createdAt, result.CreatedAt);
                Assert.NotEqual(updatedAt, result.UpdatedAt);
                Assert.NotNull(result.Version);
            }

            // Query the entity back using location header value to ensure it was updated
            UriBuilder queryUri = new UriBuilder(location) { };
            using (HttpResponseMessage queryResponse = await this.testClient.GetAsync(queryUri.Uri))
            {
                Assert.Equal(HttpStatusCode.OK, queryResponse.StatusCode);
                TestEntity result = await queryResponse.Content.ReadAsAsync<TestEntity>();
                this.VerifyEntitiesEqual(entity, result);

                Assert.NotNull(result.CreatedAt);
                Assert.NotNull(result.UpdatedAt);
                Assert.NotNull(result.Version);
            }
        }

        [Fact]
        public async Task Replace_Returns_PreconditionFailed_WithOriginalValue_IfVersionMismatch()
        {
            TestEntity entity = CreateTestEntity();
            string stringValue = entity.StringValue;
            int intValue = entity.IntValue;
            DateTimeOffset? createdAt;
            DateTimeOffset? updatedAt;
            string id;
            Uri location;

            // Create a new entity to update
            using (HttpResponseMessage postResponse = await this.testClient.PostAsJsonAsync(Address, entity))
            {
                Assert.Equal(HttpStatusCode.Created, postResponse.StatusCode);
                location = postResponse.Headers.Location;

                TestEntity result = await postResponse.Content.ReadAsAsync<TestEntity>();
                this.VerifyEntitiesEqual(entity, result);

                Assert.NotNull(result.CreatedAt);
                Assert.NotNull(result.UpdatedAt);
                Assert.NotNull(result.Version);

                createdAt = result.CreatedAt;
                updatedAt = result.UpdatedAt;
                id = result.Id;
            }

            // Update some properties
            entity.StringValue = "Updated";
            entity.IntValue = 84;
            HttpRequestMessage putRequest = new HttpRequestMessage
            {
                RequestUri = location,
                Method = HttpMethod.Put,
                Content = (HttpContent)new ObjectContent<TestEntity>(entity, this.config.Formatters.JsonFormatter)
            };
            putRequest.Headers.IfMatch.Add(EntityTagHeaderValue.Parse("\"QUJDREVG\""));

            using (HttpResponseMessage patchResponse = await this.testClient.SendAsync(putRequest))
            {
                TestEntity conflict = await patchResponse.Content.ReadAsAsync<TestEntity>();
                Assert.Equal(HttpStatusCode.PreconditionFailed, patchResponse.StatusCode);
                Assert.Equal(createdAt, conflict.CreatedAt);
                Assert.Equal(updatedAt, conflict.UpdatedAt);
            }

            // Query the entity back using location header value to ensure it was *not* updated
            UriBuilder queryUri = new UriBuilder(location) { };
            using (HttpResponseMessage response = await this.testClient.GetAsync(queryUri.Uri))
            {
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                TestEntity result = await response.Content.ReadAsAsync<TestEntity>();

                Assert.Equal(stringValue, result.StringValue);
                Assert.Equal(intValue, result.IntValue);
                Assert.NotNull(result.CreatedAt);
                Assert.NotNull(result.UpdatedAt);
            }
        }

        [Fact]
        public async Task Replace_Throws_ExtraMembers()
        {
            var entity = new TestInvalidEntity
            {
                Id = "apple",
                StringValue = "Hello",
                UnknownProperty = "Value"
            };

            // Post a new entity and verify entity returned
            using (HttpResponseMessage putResponse = await this.testClient.PutAsJsonAsync(Address + "/apple", entity))
            {
                Assert.Equal(HttpStatusCode.BadRequest, putResponse.StatusCode);
            }
        }

        [Fact]
        public async Task Delete_EntityIsDeleted_ReturnsNoContent()
        {
            TestEntity entity = CreateTestEntity();
            Uri location;

            // Post a new entity and verify entity returned
            using (HttpResponseMessage postResponse = await this.testClient.PostAsJsonAsync(Address, entity))
            {
                Assert.Equal(HttpStatusCode.Created, postResponse.StatusCode);
                location = postResponse.Headers.Location;
            }

            // Delete the entity
            using (HttpResponseMessage deleteResponse = await this.testClient.DeleteAsync(location))
            {
                Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
            }

            // Try to query the entity back to ensure it was deleted
            using (HttpResponseMessage queryResponse = await this.testClient.GetAsync(location))
            {
                Assert.Equal(HttpStatusCode.NotFound, queryResponse.StatusCode);
            }
        }

        [Fact]
        public async Task Delete_Returns_PreconditionFailed_WithOriginalValue_IfVersionMismatch()
        {
            TestEntity entity = CreateTestEntity();
            DateTimeOffset? createdAt;
            DateTimeOffset? updatedAt;
            Uri location;

            // Post a new entity and verify entity returned
            using (HttpResponseMessage postResponse = await this.testClient.PostAsJsonAsync(Address, entity))
            {
                Assert.Equal(HttpStatusCode.Created, postResponse.StatusCode);
                location = postResponse.Headers.Location;

                TestEntity result = await postResponse.Content.ReadAsAsync<TestEntity>();
                this.VerifyEntitiesEqual(entity, result);

                Assert.NotNull(result.CreatedAt);
                Assert.NotNull(result.UpdatedAt);
                Assert.NotNull(result.Version);

                createdAt = result.CreatedAt;
                updatedAt = result.UpdatedAt;
            }

            // Try to delete the entity with wrong version
            HttpRequestMessage deleteRequest = new HttpRequestMessage(HttpMethod.Delete, location);
            deleteRequest.Headers.IfMatch.Add(EntityTagHeaderValue.Parse("\"QUJDREVG\""));
            using (HttpResponseMessage deleteResponse = await this.testClient.SendAsync(deleteRequest))
            {
                TestEntity conflict = await deleteResponse.Content.ReadAsAsync<TestEntity>();
                Assert.Equal(HttpStatusCode.PreconditionFailed, deleteResponse.StatusCode);
                Assert.Equal(createdAt, conflict.CreatedAt);
                Assert.Equal(updatedAt, conflict.UpdatedAt);
            }

            // Try to query the entity back to ensure it was *not* deleted
            using (HttpResponseMessage queryResponse = await this.testClient.GetAsync(location))
            {
                Assert.Equal(HttpStatusCode.OK, queryResponse.StatusCode);
            }
        }

        private static TestEntity CreateTestEntity()
        {
            return new TestEntity()
            {
                Id = Guid.NewGuid().ToString(),
                StringValue = "Testing",
                IntValue = 84,
                BooleanValue = true,
            };
        }

        private static TestEntitySimple CreateSimpleTestEntity()
        {
            return new TestEntitySimple()
            {
                Id = Guid.NewGuid().ToString(),
                StringValue = "Testing",
                IntValue = 84,
                BooleanValue = true,
            };
        }

        private void VerifyPostedEntity(TestEntity expected, TestEntity result)
        {
            // verify an ID was auto assigned
            Assert.False(string.IsNullOrEmpty(result.Id));
            Guid id = Guid.Parse(result.Id);
            Assert.NotEqual(Guid.Empty, id);

            // verify CreatedAt was set properly
            Assert.True(result.CreatedAt.HasValue);
            double delta = Math.Abs((result.CreatedAt.Value.ToUniversalTime() - DateTime.UtcNow).TotalSeconds);
            Assert.True(delta < 100);

            // verify the rest of the non-system properties
            this.VerifyEntitiesEqual(expected, result);
        }

        private void VerifyEntitiesEqual(TestEntity expected, TestEntity result)
        {
            Assert.Equal(expected.StringValue, result.StringValue);
            Assert.Equal(expected.IntValue, result.IntValue);
            Assert.Equal(expected.BooleanValue, result.BooleanValue);
        }

        private void VerifySimpleEntitiesEqual(TestEntitySimple expected, TestEntity result)
        {
            Assert.Equal(expected.StringValue, result.StringValue);
            Assert.Equal(expected.IntValue, result.IntValue);
            Assert.Equal(expected.BooleanValue, result.BooleanValue);
        }

        public class TestContext
        {
            public TestContext()
            {
                TestHelper.ResetTestDatabase();
                this.CreateTestServer();
            }

            public TestServer Server { get; set; }

            public HttpConfiguration Config { get; set; }

            public HttpClient TestClient { get; set; }

            private void CreateTestServer()
            {
                this.Config = new HttpConfiguration();
                new MobileAppConfiguration()
                    .AddTables(
                        new MobileAppTableConfiguration()
                        .MapTableControllers()
                        .AddEntityFramework())
                    .ApplyTo(this.Config);

                this.Server = TestServer.Create(appBuilder =>
                {
                    appBuilder.UseWebApi(this.Config);
                });
            }
        }
    }
}