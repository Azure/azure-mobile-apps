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
using Microsoft.Azure.Mobile.Server.Tables.Config;
using Microsoft.Azure.Mobile.Server.TestModels;
using Microsoft.Owin.Testing;
using Newtonsoft.Json.Linq;
using Owin;
using Xunit;

namespace Microsoft.Azure.Mobile.Server
{
    public class MappedEntityTableControllerQueryTests : IClassFixture<MappedEntityTableControllerQueryTests.TestContext>
    {
        private const string Address = "MappedMovies";
        private HttpClient testClient;

        public MappedEntityTableControllerQueryTests(MappedEntityTableControllerQueryTests.TestContext data)
        {
            this.testClient = data.TestClient;
        }

        [Fact]
        public async Task NoFilter_AllEntitiesReturned()
        {
            HttpResponseMessage response = await this.testClient.GetAsync(Address);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            JArray results = await response.Content.ReadAsAsync<JArray>();

            Assert.Equal(TestData.Movies.Count, results.Count);
        }

        [Fact]
        public async Task WithFilter_FilteredResultsReturned()
        {
            HttpResponseMessage response = await this.testClient.GetAsync(Address + "?$filter=Category eq 'Family'");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            JArray results = await response.Content.ReadAsAsync<JArray>();

            Assert.Equal(TestData.Movies.Count(p => p.Category == "Family"), results.Count);
        }

        [Fact]
        public async Task PagingWithoutInlineCount_PagedResultsReturned()
        {
            HttpResponseMessage response = await this.testClient.GetAsync(Address + "?$skip=5&$top=5&$orderby=Name");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            Movie[] results = await response.Content.ReadAsAsync<Movie[]>();

            Assert.Equal(5, results.Length);
            Movie[] expectedResults = TestData.Movies.OrderBy(p => p.Name).Skip(5).Take(5).ToArray();
            Assert.True(expectedResults.Select(p => p.Name).SequenceEqual(results.Select(p => p.Name)));
        }

        [Fact]
        public async Task PagingWithInlineCount_WrappedResultReturned()
        {
            HttpResponseMessage response = await this.testClient.GetAsync(Address + "?$orderby=Name&$skip=5&$top=5&$inlinecount=allpages");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            JObject result = await response.Content.ReadAsAsync<JObject>();
            Assert.Equal((int)result["count"], TestData.Movies.Count);

            JArray results = (JArray)result["results"];
            Assert.Equal(5, results.Count);

            string[] expectedResults = TestData.Movies.OrderBy(p => p.Name).Select(p => p.Name).Skip(5).Take(5).ToArray();
            Assert.True(expectedResults.SequenceEqual(results.Select(p => (string)p["name"])));
        }

        [Fact]
        public async Task Select_SystemPropertiesNotRequested_OnlyBasePropertiesAreReturned()
        {
            HttpResponseMessage response = await this.testClient.GetAsync(Address);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            JArray results = await response.Content.ReadAsAsync<JArray>();

            Assert.Equal(results.Count, TestData.Movies.Count);
            foreach (JObject result in results)
            {
                TableHttpRequestMessageExtensions.SystemProperties.Keys.All(p => result[p] == null);
                TableHttpRequestMessageExtensions.SystemProperties.Values.All(p => result[p] == null);
            }
        }

        [Fact]
        public async Task Select_AllPropertiesAreReturned()
        {
            HttpResponseMessage response = await this.testClient.GetAsync(Address);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            JArray results = await response.Content.ReadAsAsync<JArray>();

            Assert.Equal(results.Count, TestData.Movies.Count);
            foreach (JObject result in results)
            {
                TableHttpRequestMessageExtensions.SystemProperties.Keys.All(p => result[p] != null);
                TableHttpRequestMessageExtensions.SystemProperties.Values.All(p => result[p] == null);
            }
        }

        [Fact]
        public async Task Select_SystemPropertiesRequested_SpecificReturnsSpecificPropertyOnly()
        {
            HttpResponseMessage response = await this.testClient.GetAsync(Address + "?$select=Name,createdAt");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            JArray results = await response.Content.ReadAsAsync<JArray>();

            Assert.Equal(results.Count, TestData.Movies.Count);
            foreach (JObject result in results)
            {
                Assert.NotNull(result["createdAt"]);
                Assert.NotNull(result["name"]);

                Assert.Null(result["CreatedAt"]);
                Assert.Null(result["version"]);
                Assert.Null(result["rating"]);
            }
        }

        [Fact]
        public async Task Select_AllBasePropertiesAreReturned_Singleton()
        {
            HttpResponseMessage response = await this.testClient.GetAsync(Address + "/7");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            JObject result = await response.Content.ReadAsAsync<JObject>();

            Assert.True(TableHttpRequestMessageExtensions.SystemProperties.Keys.All(p => result[p] != null));
            Assert.NotNull(result["name"]);
        }

        [Fact]
        public async Task Select_SpecifiedPropertiesReturned_Singleton()
        {
            HttpResponseMessage response = await this.testClient.GetAsync(Address + "/7?$select=version,updatedAt,createdAt,deleted");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            JObject result = await response.Content.ReadAsAsync<JObject>();

            Assert.True(TableHttpRequestMessageExtensions.SystemProperties.Keys.All(p => result[p] != null));
            Assert.Null(result["name"]);
        }

        [Fact]
        public async Task FilterWithNoSelect_UriRewriteEscapesProperly()
        {
            string filter = Uri.EscapeDataString("Name eq 'Bill & Ted''s Excellent Adventure'");
            string uri = string.Format(Address + "?$filter={0}", filter);

            HttpResponseMessage response = await this.testClient.GetAsync(uri);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            JArray results = await response.Content.ReadAsAsync<JArray>();
            JToken result = results[0];

            Assert.Equal("Bill & Ted's Excellent Adventure", result["name"]);
        }

        public class TestContext
        {
            public TestContext()
            {
                var httpConfig = new HttpConfiguration();
                new MobileAppConfiguration()
                    .AddTables(
                        new MobileAppTableConfiguration()
                        .MapTableControllers()
                        .AddEntityFramework())
                    .ApplyTo(httpConfig);

                var server = TestServer.Create(app =>
                    {
                        app.UseWebApi(httpConfig);
                    });
                this.TestClient = server.HttpClient;
                this.TestClient.BaseAddress = new Uri("http://localhost/tables/");
            }

            public HttpClient TestClient { get; set; }
        }
    }
}