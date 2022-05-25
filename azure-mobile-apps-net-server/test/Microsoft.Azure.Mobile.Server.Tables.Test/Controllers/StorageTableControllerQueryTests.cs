// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Azure.Mobile.Mocks;
using Microsoft.Azure.Mobile.Server.Config;
using Microsoft.Azure.Mobile.Server.Tables.Config;
using TestUtilities;
using Xunit;

namespace Microsoft.Azure.Mobile.Server
{
    public class StorageTableControllerQueryTests : IClassFixture<StorageTableControllerQueryTests.TestContext>
    {
        private HttpClient testClient;

        public StorageTableControllerQueryTests(TestContext data)
        {
            this.testClient = data.TestClient;
        }

        [Fact]
        public async Task QueryAsync_Succeeds_WithCamelCaseMembersInFilter()
        {
            HttpResponseMessage response = await this.testClient.GetAsync("person?$filter=age eq 20&$select=firstName");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        public class TestContext
        {
            private const string ConnectionStringEnvVar = "CONNECTION_STRING_STORAGE_TEST";
            private const string ConnectionStringName = "storage";

            public TestContext()
            {
                TestHelper.ResetTestDatabase();

                var httpConfig = new HttpConfiguration();
                new MobileAppConfiguration()
                    .AddTables(
                        new MobileAppTableConfiguration()
                        .MapTableControllers()
                        .AddEntityFramework())
                    .ApplyTo(httpConfig);

                IMobileAppSettingsProvider settingsProvider = httpConfig.GetMobileAppSettingsProvider();
                var settings = settingsProvider.GetMobileAppSettings();
                var connectionString = GetStorageConnectionString();
                settings.Connections.Add(ConnectionStringName, new ConnectionSettings(ConnectionStringName, connectionString));

                HttpServer server = new HttpServer(httpConfig);
                this.TestClient = new HttpClient(new HostMockHandler(server));
                this.TestClient.BaseAddress = new Uri("http://localhost/tables/");
            }

            public HttpClient TestClient { get; set; }

            private static string GetStorageConnectionString()
            {
                string connectionString = string.Empty;
                string connectionStringEnvVar = Environment.GetEnvironmentVariable(ConnectionStringEnvVar);
                if (!string.IsNullOrEmpty(connectionStringEnvVar))
                {
                    connectionString = Environment.ExpandEnvironmentVariables(connectionStringEnvVar);
                }

                if (string.IsNullOrWhiteSpace(connectionString))
                {
                    connectionString = "UseDevelopmentStorage=true";
                }

                return connectionString;
            }
        }
    }
}