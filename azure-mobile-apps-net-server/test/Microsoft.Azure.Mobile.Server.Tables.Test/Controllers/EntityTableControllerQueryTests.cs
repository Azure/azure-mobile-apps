// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Azure.Mobile.Server.Config;
using Microsoft.Azure.Mobile.Server.TestModels;
using Microsoft.Owin.Testing;
using Newtonsoft.Json.Linq;
using Owin;
using TestUtilities;
using Xunit;

namespace Microsoft.Azure.Mobile.Server
{
    public class EntityTableControllerQueryTests : IClassFixture<EntityTableControllerQueryTests.TestContext>
    {
        private HttpClient testClient;

        public EntityTableControllerQueryTests(EntityTableControllerQueryTests.TestContext data)
        {
            this.testClient = data.TestClient;
        }

        [Fact]
        public async Task NoFilter_AllEntitiesReturned()
        {
            HttpResponseMessage response = await this.testClient.GetAsync("movies");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            JArray results = await response.Content.ReadAsAsync<JArray>();

            Assert.Equal(TestData.Movies.Count, results.Count);
            Assert.True(response.Headers.CacheControl.NoCache);

            foreach (JObject result in results)
            {
                ValidateAllSystemProperties(result);
            }
        }

        [Fact]
        public async Task WithFilter_FilteredResultsReturned()
        {
            HttpResponseMessage response = await this.testClient.GetAsync("movies?$filter=Category eq 'Family'");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            JArray results = await response.Content.ReadAsAsync<JArray>();

            Assert.Equal(TestData.Movies.Count(p => p.Category == "Family"), results.Count);

            foreach (JObject result in results)
            {
                ValidateAllSystemProperties(result);
            }
        }

        [Fact]
        public async Task PagingWithoutInlineCount_PagedResultsReturned()
        {
            HttpResponseMessage response = await this.testClient.GetAsync("movies?$skip=5&$top=5&$orderby=Name");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            Movie[] results = await response.Content.ReadAsAsync<Movie[]>();

            Assert.Equal(5, results.Length);
            Movie[] expectedResults = TestData.Movies.OrderBy(p => p.Name).Skip(5).Take(5).ToArray();
            Assert.True(expectedResults.Select(p => p.Name).SequenceEqual(results.Select(p => p.Name)));
        }

        [Fact]
        public async Task PagingWithInlineCount_WrappedResultReturned()
        {
            HttpResponseMessage response = await this.testClient.GetAsync("movies?$orderby=Name&$skip=5&$top=5&$inlinecount=allpages");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            JObject result = await response.Content.ReadAsAsync<JObject>();
            Assert.Equal((int)result["count"], TestData.Movies.Count);

            JArray results = (JArray)result["results"];
            Assert.Equal(5, results.Count);

            string[] expectedResults = TestData.Movies.OrderBy(p => p.Name).Select(p => p.Name).Skip(5).Take(5).ToArray();
            Assert.True(expectedResults.SequenceEqual(results.Select(p => (string)p["name"])));

            foreach (JObject r in results)
            {
                ValidateAllSystemProperties(r);
            }
        }

        [Fact]
        public async Task Select_SystemPropertiesNotRequested_AllPropertiesAreReturned()
        {
            HttpResponseMessage response = await this.testClient.GetAsync("movies");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            JArray results = await response.Content.ReadAsAsync<JArray>();

            Assert.Equal(results.Count, TestData.Movies.Count);
            foreach (JObject result in results)
            {
                ValidateAllSystemProperties(result);
            }
        }

        [Fact]
        public async Task Select_SystemPropertiesNotRequested_NoSystemPropertiesAreReturned()
        {
            HttpResponseMessage response = await this.testClient.GetAsync("movies?$select=Name,Category,ReleaseDate,Rating");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            JArray results = await response.Content.ReadAsAsync<JArray>();

            Assert.Equal(results.Count, TestData.Movies.Count);
            foreach (JObject result in results)
            {
                ValidateNoSystemProperties(result);
            }
        }

        [Fact]
        public async Task Select_SystemPropertiesRequested_DefaultAllProperties()
        {
            HttpResponseMessage response = await this.testClient.GetAsync("movies");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            string json = await response.Content.ReadAsStringAsync();
            JArray results = JArray.Parse(json);

            Assert.Equal(results.Count, TestData.Movies.Count);

            foreach (JObject result in results)
            {
                ValidateAllSystemProperties(result);
            }
        }

        [Fact]
        public async Task Select_SystemPropertiesRequested_SpecificReturnsAllProperties()
        {
            HttpResponseMessage response = await this.testClient.GetAsync("movies?$select=Name,Category,ReleaseDate,Rating,createdAt");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            JArray results = await response.Content.ReadAsAsync<JArray>();

            Assert.Equal(results.Count, TestData.Movies.Count);
            foreach (JObject result in results)
            {
                Assert.NotNull(result["createdAt"]);
                Assert.Null(result["CreatedAt"]);
                Assert.Null(result["version"]);
                Assert.Null(result["updatedAt"]);
                Assert.Null(result["deleted"]);
            }
        }

        [Fact]
        public async Task Select_AllSystemPropertiesAreReturned_Singleton()
        {
            HttpResponseMessage response = await this.testClient.GetAsync("movies/7");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            JObject result = await response.Content.ReadAsAsync<JObject>();
            ValidateAllSystemProperties(result);
        }

        [Fact]
        public async Task Select_SystemPropertiesRequested_AllPropertiesReturned_Singleton()
        {
            HttpResponseMessage response = await this.testClient.GetAsync("movies/7?$select=createdAt,updatedAt,deleted,version");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            JObject result = await response.Content.ReadAsAsync<JObject>();

            ValidateAllSystemProperties(result);

            Assert.Null(result["name"]);
        }

        [Fact]
        public async Task FilterWithNoSelect_UriRewriteEscapesProperly()
        {
            string filter = Uri.EscapeDataString("Name eq 'Bill & Ted''s Excellent Adventure'");
            string uri = string.Format("movies?$filter={0}", filter);

            HttpResponseMessage response = await this.testClient.GetAsync(uri);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            JArray results = await response.Content.ReadAsAsync<JArray>();
            JToken result = results[0];

            Assert.Equal("Bill & Ted's Excellent Adventure", result["name"]);
        }
        
        public static void ValidateAllSystemProperties(JObject result)
        {
            foreach (var sysProp in TableHttpRequestMessageExtensions.SystemProperties)
            {
                Assert.NotNull(result[sysProp.Key]);
                Assert.Null(result[sysProp.Value]);
            }
        }

        public static void ValidateNoSystemProperties(JObject result)
        {
            foreach (var sysProp in TableHttpRequestMessageExtensions.SystemProperties)
            {
                Assert.Null(result[sysProp.Key]);
                Assert.Null(result[sysProp.Value]);
            }
        }

        public class TestContext
        {
            public TestContext()
            {
                TestHelper.ResetTestDatabase();

                var config = new HttpConfiguration();
                new MobileAppConfiguration()
                    .AddTablesWithEntityFramework()
                    .MapApiControllers()
                    .ApplyTo(config);

                var server = TestServer.Create(app =>
                    {
                        app.UseWebApi(config);
                    });

                this.TestClient = server.HttpClient;
                this.TestClient.BaseAddress = new Uri("http://localhost/tables/");
            }

            public HttpClient TestClient { get; set; }
        }
    }
}