// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using System.Web.Http.OData;
using Microsoft.Azure.Mobile.Mocks;
using Microsoft.Azure.Mobile.Server.Config;
using Microsoft.Azure.Mobile.Server.Tables.Config;
using Microsoft.Azure.Mobile.Server.TestModels;
using TestUtilities;
using Xunit;

namespace Microsoft.Azure.Mobile.Server.Tables
{
    public class TableFilterProviderTests : IClassFixture<TableFilterProviderTests.TestContext>
    {
        private TestContext testContext = null;

        public TableFilterProviderTests(TestContext data)
        {
            this.testContext = data;
            this.testContext.TestConfig.Filters.Clear();
        }

        public static TheoryDataCollection<string> ControllerAddresses
        {
            get
            {
                return new TheoryDataCollection<string>
                {
                    "Nofilter",
                    "QueryableControllerFilter",
                    "EnableQueryControllerFilter",
                    "QueryableActionFilter",
                    "EnableQueryActionFilter",
                };
            }
        }

        [Theory]
        [MemberData("ControllerAddresses")]
        public async Task GetFilters_AddsTableFilterBeforeQueryFilter_OnGetActions(string address)
        {
            // Act
            HttpResponseMessage response = await this.testContext.TestClient.GetAsync(address);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Theory]
        [MemberData("ControllerAddresses")]
        public async Task GetFilters_AddsTableFilterBeforeQueryFilter_OnPostActions(string address)
        {
            // Act
            HttpResponseMessage response = await this.testContext.TestClient.PostAsJsonAsync(address, "Hello");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task GetFilters_AddsTableFilterBeforeQueryableFilter_ForGlobalFilters()
        {
            // Arrange
            this.testContext.TestConfig.Filters.Add(new QueryableAttribute());

            // Act
            HttpResponseMessage response = await this.testContext.TestClient.GetAsync("GlobalFilter");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task GetFilters_AddsTableFilterBeforeEnableQueryFilter_ForGlobalFilters()
        {
            // Arrange
            this.testContext.TestConfig.Filters.Add(new EnableQueryAttribute());

            // Act
            HttpResponseMessage response = await this.testContext.TestClient.GetAsync("GlobalFilter");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        public static void ValidateFilterPipeline(HttpActionDescriptor actionDescriptor, bool isQueryable)
        {
            Collection<FilterInfo> filters = actionDescriptor.GetFilterPipeline();
            if (isQueryable)
            {
                string order = string.Empty;

                // The queryable attributes need to come after the TableQueryFilter, but our other
                // filter may sneak in somewhere.
                foreach (FilterInfo filter in filters)
                {
                    if (filter.Instance is TableQueryFilter)
                    {
                        order += "1";
                    }

                    if (filter.Instance is EnableQueryAttribute || filter.Instance is QueryableAttribute)
                    {
                        order += "2";
                    }
                }

                Assert.Equal(3, filters.Count);
                Assert.Equal("12", order);
            }
            else
            {
                Assert.Equal(2, filters.Count);
                filters.Where(f => f.Instance is TableQueryFilter).Single();
            }
        }

        public class TestContext
        {
            public TestContext()
            {
                TestHelper.ResetTestDatabase();

                this.TestConfig = new HttpConfiguration();

                new MobileAppConfiguration()
                    .AddTables(
                        new MobileAppTableConfiguration()
                        .MapTableControllers()
                        .AddEntityFramework())
                    .ApplyTo(this.TestConfig);

                HttpServer server = new HttpServer(this.TestConfig);
                this.TestClient = new HttpClient(new HostMockHandler(server));
                this.TestClient.BaseAddress = new Uri("http://localhost/tables/");
            }

            public HttpConfiguration TestConfig { get; set; }

            public HttpClient TestClient { get; set; }
        }

        public class NoFilterController : TableController<Movie>
        {
            public IQueryable<string> Get()
            {
                TableFilterProviderTests.ValidateFilterPipeline(this.ActionContext.ActionDescriptor, isQueryable: true);
                List<string> result = new List<string>() { "Hello" };
                return result.AsQueryable();
            }

            public bool Post([FromBody]string input)
            {
                TableFilterProviderTests.ValidateFilterPipeline(this.ActionContext.ActionDescriptor, isQueryable: false);
                return !String.IsNullOrEmpty(input);
            }
        }

        [Queryable]
        public class QueryableControllerFilterController : TableController<Movie>
        {
            public IQueryable<string> Get()
            {
                TableFilterProviderTests.ValidateFilterPipeline(this.ActionContext.ActionDescriptor, isQueryable: true);
                List<string> result = new List<string>() { "Hello" };
                return result.AsQueryable();
            }

            public bool Post([FromBody]string input)
            {
                TableFilterProviderTests.ValidateFilterPipeline(this.ActionContext.ActionDescriptor, isQueryable: true);
                return !String.IsNullOrEmpty(input);
            }
        }

        [EnableQuery]
        public class EnableQueryControllerFilterController : TableController<Movie>
        {
            public IQueryable<string> Get()
            {
                TableFilterProviderTests.ValidateFilterPipeline(this.ActionContext.ActionDescriptor, isQueryable: true);
                List<string> result = new List<string>() { "Hello" };
                return result.AsQueryable();
            }

            public bool Post([FromBody]string input)
            {
                TableFilterProviderTests.ValidateFilterPipeline(this.ActionContext.ActionDescriptor, isQueryable: true);
                return !String.IsNullOrEmpty(input);
            }
        }

        public class QueryableActionFilterController : TableController<Movie>
        {
            [Queryable]
            public IQueryable<string> Get()
            {
                TableFilterProviderTests.ValidateFilterPipeline(this.ActionContext.ActionDescriptor, isQueryable: true);
                List<string> result = new List<string>() { "Hello" };
                return result.AsQueryable();
            }

            public bool Post([FromBody]string input)
            {
                TableFilterProviderTests.ValidateFilterPipeline(this.ActionContext.ActionDescriptor, isQueryable: false);
                return !String.IsNullOrEmpty(input);
            }
        }

        public class EnableQueryActionFilterController : TableController<Movie>
        {
            [EnableQuery]
            public IQueryable<string> Get()
            {
                TableFilterProviderTests.ValidateFilterPipeline(this.ActionContext.ActionDescriptor, isQueryable: true);
                List<string> result = new List<string>() { "Hello" };
                return result.AsQueryable();
            }

            public bool Post([FromBody]string input)
            {
                TableFilterProviderTests.ValidateFilterPipeline(this.ActionContext.ActionDescriptor, isQueryable: false);
                return !String.IsNullOrEmpty(input);
            }
        }

        public class GlobalFilterController : TableController<Movie>
        {
            public IQueryable<string> Get()
            {
                TableFilterProviderTests.ValidateFilterPipeline(this.ActionContext.ActionDescriptor, isQueryable: true);
                List<string> result = new List<string>() { "Hello" };
                return result.AsQueryable();
            }
        }
    }
}