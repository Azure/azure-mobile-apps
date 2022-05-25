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
using System.Web.Http.OData;
using System.Web.Http.OData.Extensions;
using System.Web.Http.OData.Query;
using Microsoft.Azure.Mobile.Server.Config;
using Microsoft.Azure.Mobile.Server.Mocks;
using Microsoft.Azure.Mobile.Server.Tables;
using Microsoft.Azure.Mobile.Server.TestModels;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Moq;
using TestUtilities;
using Xunit;

namespace Microsoft.Azure.Mobile.Server
{
    public class StorageDomainManagerTests : IDisposable
    {
        private const string ConnectionStringEnvVar = "CONNECTION_STRING_STORAGE_TEST";
        private const string ConnectionStringName = "storage";
        private const string Table = "persons";

        private static readonly string[] ComputedSystemProperties = new string[]
        {
            "Id",
            "Version",
            "UpdatedAt",
        };

        private bool disposed;
        private string connectionString;
        private HttpConfiguration config;
        private MobileAppSettingsDictionary settings;
        private HttpRequestMessage request;
        private StorageDomainManagerMock manager;
        private ODataQuerySettings querySettings;
        private ODataValidationSettings validationSettings;

        public StorageDomainManagerTests()
        {
            string connectionStringEnvVar = Environment.GetEnvironmentVariable(ConnectionStringEnvVar);
            if (!string.IsNullOrEmpty(connectionStringEnvVar))
            {
                this.connectionString = Environment.ExpandEnvironmentVariables(connectionStringEnvVar);
            }

            if (string.IsNullOrWhiteSpace(this.connectionString))
            {
                this.connectionString = "UseDevelopmentStorage=true";
            }

            this.config = new HttpConfiguration();
            this.settings = new MobileAppSettingsDictionary();
            this.settings.Connections.Add(ConnectionStringName, new ConnectionSettings(ConnectionStringName, this.connectionString));

            var providerMock = new Mock<IMobileAppSettingsProvider>();
            providerMock.Setup(p => p.GetMobileAppSettings()).Returns(this.settings);
            this.config.SetMobileAppSettingsProvider(providerMock.Object);

            this.request = new HttpRequestMessage(HttpMethod.Get, new Uri("http://localhost"));
            this.request.SetConfiguration(this.config);
            this.querySettings = StorageDomainManager<Person>.GetDefaultQuerySettings();
            this.validationSettings = StorageDomainManager<Person>.GetDefaultValidationSettings();
            this.manager = new StorageDomainManagerMock(this);
        }

        public static TheoryDataCollection<string, TableContinuationToken, string> NextPageData
        {
            get
            {
                return new TheoryDataCollection<string, TableContinuationToken, string>
                {
                    { "http://localhost/", new TableContinuationToken { NextPartitionKey = "你好世界" }, "?NextPartitionKey=%E4%BD%A0%E5%A5%BD%E4%B8%96%E7%95%8C" },
                    { "http://localhost/?foo=bar", new TableContinuationToken { NextPartitionKey = "next" }, "&NextPartitionKey=next" },

                    { "http://localhost/", new TableContinuationToken { NextRowKey = "你好世界" }, "?NextRowKey=%E4%BD%A0%E5%A5%BD%E4%B8%96%E7%95%8C" },
                    { "http://localhost/?foo=bar", new TableContinuationToken { NextRowKey = "next" }, "&NextRowKey=next" },

                    { "http://localhost/", new TableContinuationToken { NextTableName = "你好世界" }, "?NextTableName=%E4%BD%A0%E5%A5%BD%E4%B8%96%E7%95%8C" },
                    { "http://localhost/?foo=bar", new TableContinuationToken { NextTableName = "next" }, "&NextTableName=next" },

                    { "http://localhost/", new TableContinuationToken { NextPartitionKey = "par", NextRowKey = "row", NextTableName = "你好世界" }, "?NextPartitionKey=par&NextRowKey=row&NextTableName=%E4%BD%A0%E5%A5%BD%E4%B8%96%E7%95%8C" },
                    { "http://localhost/?foo=bar", new TableContinuationToken { NextPartitionKey = "par", NextRowKey = "row", NextTableName = "你好世界" }, "&NextPartitionKey=par&NextRowKey=row&NextTableName=%E4%BD%A0%E5%A5%BD%E4%B8%96%E7%95%8C" },
                };
            }
        }

        public static TheoryDataCollection<string, string> DateTimeOffsetFilterData
        {
            get
            {
                return new TheoryDataCollection<string, string>
                {
                    // valid datetimeoffsets are replced with datetime
                    { "CreatedAt gt datetimeoffset'1970-01-01T00:00:00Z'", "CreatedAt gt datetime'1970-01-01T00:00:00Z'" },
                    { "__updatedAt gt datetimeoffset'2014-08-27T22:48:24.3255367Z' and __createdAt gt datetimeoffset'2014-08-27T22:48:24.3255367Z'", "__updatedAt gt datetime'2014-08-27T22:48:24.3255367Z' and __createdAt gt datetime'2014-08-27T22:48:24.3255367Z'" },
                    { "__updatedAt gt datetimeoffset'2014-08-27T22:48:24Z'", "__updatedAt gt datetime'2014-08-27T22:48:24Z'" },
                    { "__updatedAt gt datetimeoffset'2014-08-27T22:48:24.325Z'", "__updatedAt gt datetime'2014-08-27T22:48:24.325Z'" },
                    { "__updatedAt gt datetimeoffset'2014-08-27T16:06:55.7387986-07:00'", "__updatedAt gt datetime'2014-08-27T16:06:55.7387986-07:00'" },
                    { "__updatedAt gt datetimeoffset'2014-08-27T16:06:55.7387986+07:00'", "__updatedAt gt datetime'2014-08-27T16:06:55.7387986+07:00'" },

                    // invalid datetimeoffsets are not replaced
                    { String.Empty, String.Empty },
                    { "__updatedAt gt datetimeoffset'2014-08-27T22:48:24.2Z'", "__updatedAt gt datetimeoffset'2014-08-27T22:48:24.2Z'" },
                    { "__updatedAt gt datetimeoffset'2014/08/27T22:48:24Z'", "__updatedAt gt datetimeoffset'2014/08/27T22:48:24Z'" },
                };
            }
        }

        public static TheoryDataCollection<string, int> SystemPropertyFilterData
        {
            get
            {
                return new TheoryDataCollection<string, int>
                {
                    { "updatedAt gt datetimeoffset'1970-01-01T00:00:00Z'", 5 },
                    { "createdAt gt datetimeoffset'1970-01-01T00:00:00Z'", 5 },
                    { "version eq 'asdf'", 0 }
                };
            }
        }

        [Fact]
        public void Request_Roundtrips()
        {
            HttpRequestMessage roundtrips = new HttpRequestMessage();
            PropertyAssert.Roundtrips(this.manager, m => m.Request, PropertySetter.NullThrows, defaultValue: this.request, roundtripValue: roundtrips);
        }

        [Fact]
        public void ValidationSettings_Roundtrips()
        {
            ODataValidationSettings roundtrips = new ODataValidationSettings();
            PropertyAssert.Roundtrips(this.manager, m => m.ValidationSettings, PropertySetter.NullThrows, defaultValue: this.validationSettings, roundtripValue: roundtrips);
        }

        [Fact]
        public void QuerySettings_Roundtrips()
        {
            ODataQuerySettings roundtrips = new ODataQuerySettings();
            PropertyAssert.Roundtrips(this.manager, m => m.QuerySettings, PropertySetter.NullThrows, defaultValue: this.querySettings, roundtripValue: roundtrips);
        }

        [Fact]
        public void Context_IsCached()
        {
            // Act
            StorageDomainManagerMock dm1 = new StorageDomainManagerMock(this);
            StorageDomainManagerMock dm2 = new StorageDomainManagerMock(this);

            // Assert
            Assert.Same(dm1.StorageAccount, dm2.StorageAccount);
        }

        [Fact]
        public void Query_ThrowsNotImplemented()
        {
            NotImplementedException ex = Assert.Throws<NotImplementedException>(() => ((IDomainManager<Person>)this.manager).Query());
            Assert.Contains("The 'StorageDomainManagerMock' domain manager does not support 'IQueryable' for querying data. Please use the 'QueryAsync' method instead'.", ex.Message);
        }

        [Fact]
        public void Lookup_ThrowsNotImplemented()
        {
            NotImplementedException ex = Assert.Throws<NotImplementedException>(() => ((IDomainManager<Person>)this.manager).Lookup((string)null));
            Assert.Contains("The 'StorageDomainManagerMock' domain manager does not support 'IQueryable' for looking up data. Please use the 'LookupAsync' method instead'.", ex.Message);
        }

        [Fact]
        public void GetCloudStorageAccount_Throws_IfConnectionStringNotFound()
        {
            // Act/Assert
            ArgumentException ex = Assert.Throws<ArgumentException>(() => this.manager.GetCloudStorageAccount("unknown"));
            Assert.Contains("No connection string named 'unknown'", ex.Message);
        }

        [Fact]
        public async Task QueryAsync_All_ReturnsData()
        {
            // Arrange
            Collection<Person> persons = TestData.Persons;

            await this.InsertPersons(persons);

            // Act
            ODataQueryContext context = new ODataQueryContext(EdmModelMock.Create<Person>("Persons"), typeof(Person));
            ODataQueryOptions query = new ODataQueryOptions(context, this.request);

            IEnumerable<Person> actual = await this.manager.QueryAsync(query);

            // Assert
            Assert.Equal(persons.Count, actual.Count());
            foreach (Person person in persons)
            {
                actual.Single(i => i.FirstName == person.FirstName);
            }
        }

        [Fact]
        public async Task QueryAsync_DoesNotReturnsSoftDeletedRows_IfIncludeDeletedIsFalse()
        {
            // Arrange
            List<Person> persons = TestData.Persons.Take(2).ToList();

            persons[0].PartitionKey = persons[0].LastName;
            persons[0].RowKey = persons[0].FirstName;
            await this.manager.InsertAsync(persons[0]);

            persons[1].PartitionKey = persons[1].LastName;
            persons[1].RowKey = persons[1].FirstName;
            persons[1].Deleted = true;
            await this.manager.InsertAsync(persons[1]);

            this.manager.IncludeDeleted = false;

            // Act
            ODataQueryContext context = new ODataQueryContext(EdmModelMock.Create<Person>("Persons"), typeof(Person));
            ODataQueryOptions query = new ODataQueryOptions(context, this.request);
            IEnumerable<Person> actual = await this.manager.QueryAsync(query);

            // Assert
            Assert.Equal(1, actual.Count());
            Assert.Equal(actual.First().Id, persons[0].Id);
        }

        [Fact]
        public async Task QueryAsync_Top_ReturnsData()
        {
            // Arrange
            this.request.RequestUri = new Uri("http://localhost/?$top=4");
            Collection<Person> persons = TestData.Persons;

            await this.InsertPersons(persons);

            // Act
            ODataQueryContext context = new ODataQueryContext(EdmModelMock.Create<Person>("Persons"), typeof(Person));
            ODataQueryOptions query = new ODataQueryOptions(context, this.request);

            IEnumerable<Person> actual = await this.manager.QueryAsync(query);

            // Assert
            Assert.Equal(4, actual.Count());
            foreach (Person a in actual)
            {
                persons.Single(i => i.FirstName == a.FirstName);
            }
        }

        [Fact]
        public async Task QueryAsync_Succeeds_WithCamelCaseMembersInFilter()
        {
            // Arrange
            this.request.RequestUri = new Uri("http://localhost/?$filter=age eq 20&$select=firstName");
            Collection<Person> persons = TestData.Persons;

            await this.InsertPersons(persons);

            // Act
            ODataQueryContext context = new ODataQueryContext(EdmModelMock.Create<Person>("Persons"), typeof(Person));
            ODataQueryOptions query = new ODataQueryOptions(context, this.request);

            IEnumerable<Person> actual = await this.manager.QueryAsync(query);

            // Assert
            Assert.Equal(1, actual.Count());
            persons.Single(i => i.FirstName == actual.Single().FirstName);
        }

        [Theory]
        [MemberData("SystemPropertyFilterData")]
        public async Task QueryAsync_Succeeds_WithSystemPropertiesInFilter(string filter, int expectedCount)
        {
            // Arrange
            this.request.RequestUri = new Uri("http://localhost/?$filter=" + filter + "&$select=firstName");
            Collection<Person> persons = TestData.Persons;

            await this.InsertPersons(persons);

            // Act
            ODataQueryContext context = new ODataQueryContext(EdmModelMock.Create<Person>("Persons"), typeof(Person));
            ODataQueryOptions query = new ODataQueryOptions(context, this.request);

            IEnumerable<Person> actual = await this.manager.QueryAsync(query);

            // Assert
            Assert.Equal(expectedCount, actual.Count());
        }

        [Theory]
        [MemberData("DateTimeOffsetFilterData")]
        public void ReplaceDateTimeOffset_Succeeds_WithDateTimeOffsetInFilter(string inputFilter, string outputFilter)
        {
            string actualOutput = StorageDomainManager<Person>.ReplaceDateTimeOffsetWithDateTime(inputFilter);
            Assert.Equal(outputFilter, actualOutput);
        }

        [Fact]
        public async Task QueryAsync_Succeeds_WithDateTimeOffsetInFilter()
        {
            // Arrange
            this.request.RequestUri = new Uri("http://localhost/?$filter=createdAt gt datetimeoffset'2014-08-28T14:22:48.8650825-07:00'&$select=firstName");
            Collection<Person> persons = TestData.Persons;

            await this.InsertPersons(persons);

            // Act
            ODataQueryContext context = new ODataQueryContext(EdmModelMock.Create<Person>("Persons"), typeof(Person));
            ODataQueryOptions query = new ODataQueryOptions(context, this.request);

            IEnumerable<Person> actual = await this.manager.QueryAsync(query);

            // Assert
            Assert.Equal(5, actual.Count());
        }

        [Fact]
        public async Task QueryAsync_Filter_ReturnsData()
        {
            // Arrange
            this.request.RequestUri = new Uri("http://localhost/?$filter=Age eq 20");
            Collection<Person> persons = TestData.Persons;

            await this.InsertPersons(persons);

            // Act
            ODataQueryContext context = new ODataQueryContext(EdmModelMock.Create<Person>("Persons"), typeof(Person));
            ODataQueryOptions query = new ODataQueryOptions(context, this.request);

            IEnumerable<Person> actual = await this.manager.QueryAsync(query);

            // Assert
            Assert.Equal(1, actual.Count());
            foreach (Person a in actual)
            {
                persons.Single(i => i.FirstName == a.FirstName);
            }
        }

        [Fact]
        public async Task QueryAsync_WithPaging_ReturnsAllData()
        {
            // Arrange
            int pageSize = 5;
            int numPersons = (pageSize * 3) + 2;  // 4 pages of data
            int numPages = (int)Math.Ceiling((double)numPersons / pageSize);
            Collection<Person> persons = TestData.CreatePersons(numPersons);

            await this.InsertPersons(persons);

            // Act
            ODataQueryContext context = new ODataQueryContext(EdmModelMock.Create<Person>("Persons"), typeof(Person));
            this.manager.QuerySettings.PageSize = pageSize;
            List<Person> allResults = new List<Person>();
            string rootUri = "https://localhost/tables/persons";
            Uri nextLinkUri = new Uri(rootUri);
            int actualPageCount = 0;
            do
            {
                // query for the next page and append the results
                HttpRequestMessage currRequest = new HttpRequestMessage(HttpMethod.Get, nextLinkUri);
                ODataQueryOptions query = new ODataQueryOptions(context, currRequest);
                this.manager.Request = currRequest;
                IEnumerable<Person> results = await this.manager.QueryAsync(query);
                allResults.AddRange(results);
                actualPageCount++;

                // follow the next link
                nextLinkUri = currRequest.ODataProperties().NextLink;

                if (nextLinkUri != null)
                {
                    // ensure that the root path has been maintained
                    Assert.True(nextLinkUri.ToString().StartsWith(rootUri));
                }
            }
            while (nextLinkUri != null);

            // Assert
            Assert.Equal(numPages, actualPageCount);
            Assert.Equal(persons.Count, allResults.Count());
            foreach (Person person in persons)
            {
                allResults.Single(i => i.FirstName == person.FirstName);
            }
        }

        [Fact]
        public async Task QueryAsync_ThrowsOnBadQuery()
        {
            // Arrange
            this.request.RequestUri = new Uri("http://localhost?$top=top");

            // Act
            ODataQueryContext context = new ODataQueryContext(EdmModelMock.Create<Person>("Persons"), typeof(Person));
            ODataQueryOptions query = new ODataQueryOptions(context, this.request);
            HttpResponseException ex = await AssertEx.ThrowsAsync<HttpResponseException>(async () => await this.manager.QueryAsync(query));

            HttpError error;
            ex.Response.TryGetContentValue(out error);
            Assert.Equal(HttpStatusCode.BadRequest, ex.Response.StatusCode);
            Assert.Equal("The query specified in the URI is not valid: 'Failed to convert 'top' to an integer.'.", error.Message);
        }

        [Fact]
        public async Task LookupAsync_ReturnsData()
        {
            // Arrange
            Collection<Person> persons = TestData.Persons;

            await this.InsertPersons(persons);

            CompositeTableKey id = new CompositeTableKey("Nielsen", "Henrik");

            // Act
            SingleResult<Person> actual = await this.manager.LookupAsync(id.ToString());

            // Assert
            Assert.Equal("Henrik", actual.Queryable.First().FirstName);
        }

        [Fact]
        public async Task LookupAsync_ReturnsSoftDeletedRecord_IfIncludeDeletedIsTrue()
        {
            // Arrange
            Person person = TestData.Persons.First();
            person.PartitionKey = person.LastName;
            person.RowKey = person.FirstName;
            person.CreatedAt = null;
            person.UpdatedAt = null;

            await this.manager.InsertAsync(person);
            this.manager.EnableSoftDelete = true;
            bool result = await this.manager.DeleteAsync(person.Id);

            // Act
            this.manager.IncludeDeleted = true;
            CompositeTableKey id = new CompositeTableKey(person.LastName, person.FirstName);
            Person lookedup = (await this.manager.LookupAsync(id.ToString())).Queryable.First();

            // Assert
            Assert.Equal(person.Id, lookedup.Id);
            Assert.Equal(true, lookedup.Deleted);
            Assert.Equal(person.FirstName, lookedup.FirstName);
            Assert.Equal(person.LastName, lookedup.LastName);
        }

        [Fact]
        public async Task LookupAsync_DoesNotReturnSoftDeletedRecord_IfIncludeDeletedIsFalse()
        {
            // Arrange
            Person person = TestData.Persons.First();
            person.PartitionKey = person.LastName;
            person.RowKey = person.FirstName;
            person.CreatedAt = null;
            person.UpdatedAt = null;

            await this.manager.InsertAsync(person);
            this.manager.EnableSoftDelete = true;
            bool result = await this.manager.DeleteAsync(person.Id);

            // Act
            this.manager.IncludeDeleted = false;
            CompositeTableKey id = new CompositeTableKey(person.LastName, person.FirstName);
            HttpResponseException ex = await AssertEx.ThrowsAsync<HttpResponseException>(async () => await this.manager.LookupAsync(id.ToString()));

            Assert.Equal(HttpStatusCode.NotFound, ex.Response.StatusCode);
        }

        [Fact]
        public async Task InsertAsync_DoesNotMap_ComputedSystemProperties()
        {
            // Arrange
            Collection<Person> persons = TestData.Persons;

            await this.InsertPersons(persons);

            // Act
            TableQuery query = new TableQuery();
            IEnumerable<DynamicTableEntity> queryResult = this.manager.Table.ExecuteQuery(query);

            // Assert
            bool isChecked = false;
            foreach (DynamicTableEntity result in queryResult)
            {
                foreach (string propertyName in ComputedSystemProperties)
                {
                    EntityProperty property;
                    bool actual = result.Properties.TryGetValue(propertyName, out property);
                    isChecked = true;
                    Assert.False(actual);
                }
            }

            Assert.True(isChecked);
        }

        [Fact]
        public async Task InsertAsync_Throws_Conflict_IfDuplicateKeys()
        {
            // Arrange
            Person person = TestData.Persons[0];
            person.PartitionKey = person.LastName;
            person.RowKey = person.FirstName;

            Person result = await this.manager.InsertAsync(person);

            // Act
            HttpResponseException ex = await AssertEx.ThrowsAsync<HttpResponseException>(async () => await this.manager.InsertAsync(person));
            HttpError error;
            ex.Response.TryGetContentValue(out error);

            Assert.Equal(HttpStatusCode.Conflict, ex.Response.StatusCode);
            Assert.Contains(@"The operation failed due to a conflict: 'The specified entity already exists.", error.Message);
        }

        [Fact]
        public async Task InsertAsync_ReturnsBadRequest_IfValidExceptionAndUnsupportedError()
        {
            // Arrange (Don't set partition key)
            Person person = TestData.Persons[0];
            person.RowKey = person.FirstName;

            // Act
            HttpResponseException ex = await AssertEx.ThrowsAsync<HttpResponseException>(async () => await this.manager.InsertAsync(person));
            HttpError error;
            ex.Response.TryGetContentValue(out error);

            Assert.Equal(HttpStatusCode.BadRequest, ex.Response.StatusCode);
            Assert.Contains(@"The remote server returned an error: (400) Bad Request", error.Message);
        }

        [Fact]
        public async Task UpdateAsync_Throws_PreconditionFailed_IfDifferentVersions()
        {
            const int Age = 1024;

            // Arrange
            Person person = TestData.Persons[0];
            person.PartitionKey = person.LastName;
            person.RowKey = person.FirstName;

            Delta<Person> patch = new Delta<Person>();

            // Act
            Person result = await this.manager.InsertAsync(person);

            patch.TrySetPropertyValue("Age", Age);
            patch.TrySetPropertyValue("Version", result.Version);
            await this.manager.UpdateAsync(result.Id, patch);

            HttpResponseException ex = await AssertEx.ThrowsAsync<HttpResponseException>(async () => await this.manager.UpdateAsync(result.Id, patch));

            // Assert
            Assert.Equal(HttpStatusCode.PreconditionFailed, ex.Response.StatusCode);
            Assert.Equal(result.Age, person.Age);
        }

        [Fact]
        public async Task UpdateAsync_UpdatesCurrentValues()
        {
            // Arrange
            const int Age = 1024;
            Collection<Person> persons = TestData.Persons;

            foreach (Person person in persons)
            {
                person.PartitionKey = person.LastName;
                person.RowKey = person.FirstName;
                person.CreatedAt = null;
                person.UpdatedAt = null;

                await this.manager.InsertAsync(person);

                Delta<Person> patch = new Delta<Person>();
                patch.TrySetPropertyValue("Age", Age);
                Person result = await this.manager.UpdateAsync(person.Id, patch);

                Assert.Equal(Age, result.Age);
                Assert.NotNull(result.UpdatedAt);
            }
        }

        [Fact]
        public async Task UpdateAsync_UpdatesCurrentValues_WhenItemIsSoftDeleted_AndIncludeDeletedIsTrue()
        {
            // Arrange
            const int Age = 1024;
            Person person = TestData.Persons.First();
            person.PartitionKey = person.LastName;
            person.RowKey = person.FirstName;
            person.CreatedAt = null;
            person.UpdatedAt = null;
            person.Deleted = true;
            Person insertedPerson = await this.manager.InsertAsync(person);

            // Act
            var patch = new Delta<Person>();
            this.manager.IncludeDeleted = true;
            patch.TrySetPropertyValue("Age", Age);
            Person result = await this.manager.UpdateAsync(person.Id, patch);

            // Assert
            Assert.Equal(Age, result.Age);
            Assert.NotNull(result.UpdatedAt);
            Assert.Equal(result.Deleted, true);
        }

        [Fact]
        public async Task UpdateAsync_Throws_NotFoundIfIdNotFound()
        {
            // Arrange
            string id = new CompositeTableKey("unknown", "item").ToString();
            Delta<Person> patch = new Delta<Person>();

            // Act/Assert
            this.manager.IncludeDeleted = false;
            HttpResponseException ex = await AssertEx.ThrowsAsync<HttpResponseException>(async () => await this.manager.UpdateAsync(id, patch));
            Assert.Equal(HttpStatusCode.NotFound, ex.Response.StatusCode);

            this.manager.IncludeDeleted = true;
            ex = await AssertEx.ThrowsAsync<HttpResponseException>(async () => await this.manager.UpdateAsync(id, patch));
            Assert.Equal(HttpStatusCode.NotFound, ex.Response.StatusCode);
        }

        [Fact]
        public async Task UpdateAsync_Throws_NotFound_IfItemIsSoftDeleted()
        {
            // Arrange
            Person person = TestData.Persons.First();
            person.PartitionKey = person.LastName;
            person.RowKey = person.FirstName;
            person.CreatedAt = null;
            person.UpdatedAt = null;
            person.Deleted = true;
            Person insertedPerson = await this.manager.InsertAsync(person);

            var patch = new Delta<Person>();
            this.manager.IncludeDeleted = false;

            // Act/Assert
            HttpResponseException ex = await AssertEx.ThrowsAsync<HttpResponseException>(async () => await this.manager.UpdateAsync(person.Id, patch));
            Assert.Equal(HttpStatusCode.NotFound, ex.Response.StatusCode);
        }

        [Fact]
        public async Task UpdateAsync_Throws_BadRequestIfInvalidId()
        {
            // Arrange
            Delta<Person> patch = new Delta<Person>();

            // Act/Assert
            HttpResponseException ex = await AssertEx.ThrowsAsync<HttpResponseException>(async () => await this.manager.UpdateAsync("invalid", patch));
            Assert.Equal(HttpStatusCode.BadRequest, ex.Response.StatusCode);
        }

        [Fact]
        public async Task ReplaceAsync_ReplacesData()
        {
            // Arrange
            const int Age = 1024;
            Collection<Person> persons = TestData.Persons;

            foreach (Person person in persons)
            {
                person.PartitionKey = person.LastName;
                person.RowKey = person.FirstName;
                person.CreatedAt = null;
                person.UpdatedAt = null;

                await this.manager.InsertAsync(person);

                person.Age = Age;
                Person result = await this.manager.ReplaceAsync(person.Id, person);

                Assert.Equal(person.Id, result.Id);
                Assert.Equal(Age, result.Age);
                Assert.NotNull(result.CreatedAt);
            }
        }

        [Fact]
        public async Task ReplaceAsync_Throws_BadRequestIfIdNotFound()
        {
            // Arrange
            string id = new CompositeTableKey("unknown", "item").ToString();
            Person person = new Person();

            // Act/Assert
            HttpResponseException ex = await AssertEx.ThrowsAsync<HttpResponseException>(async () => await this.manager.ReplaceAsync(id, person));
            Assert.Equal(HttpStatusCode.BadRequest, ex.Response.StatusCode);
        }

        [Fact]
        public async Task ReplaceAsync_Throws_BadRequestIfInvalidId()
        {
            // Arrange
            Person person = new Person();

            // Act/Assert
            HttpResponseException ex = await AssertEx.ThrowsAsync<HttpResponseException>(async () => await this.manager.ReplaceAsync("invalid", person));
            Assert.Equal(HttpStatusCode.BadRequest, ex.Response.StatusCode);
        }

        [Fact]
        public async Task DeleteAsync_DeletesData()
        {
            // Arrange
            Collection<Person> persons = TestData.Persons;

            foreach (Person person in persons)
            {
                person.PartitionKey = person.LastName;
                person.RowKey = person.FirstName;
                person.CreatedAt = null;
                person.UpdatedAt = null;

                await this.manager.InsertAsync(person);

                bool result = await this.manager.DeleteAsync(person.Id);

                Assert.True(result);
            }
        }

        [Fact]
        public async Task DeleteAsync_MarksAsDeleted_IfSoftDeleteIsTrue()
        {
            // Arrange
            Person person = TestData.Persons.First();
            person.PartitionKey = person.LastName;
            person.RowKey = person.FirstName;
            person.CreatedAt = null;
            person.UpdatedAt = null;
            person.Deleted = true;

            Person insertedPerson = await this.manager.InsertAsync(person);
            this.manager.IncludeDeleted = false;
            this.manager.EnableSoftDelete = true;

            // Act
            bool result = await this.manager.DeleteAsync(insertedPerson.Id);

            // Assert
            Assert.True(result);

            HttpResponseException ex = await AssertEx.ThrowsAsync<HttpResponseException>(async () => await this.manager.LookupAsync(insertedPerson.Id.ToString()));
            Assert.Equal(HttpStatusCode.NotFound, ex.Response.StatusCode);

            this.manager.IncludeDeleted = true;
            Person lookedup = (await this.manager.LookupAsync(insertedPerson.Id)).Queryable.First();

            Assert.Equal(person.Id, lookedup.Id);
            Assert.Equal(true, lookedup.Deleted);
            Assert.Equal(person.FirstName, lookedup.FirstName);
            Assert.Equal(person.LastName, lookedup.LastName);
        }

        [Fact]
        public async Task DeleteAsync_Throws_BadRequestIfIdNotFound()
        {
            // Assert
            string id = new CompositeTableKey("unknown", "item").ToString();

            // Act/Assert
            HttpResponseException ex = await AssertEx.ThrowsAsync<HttpResponseException>(async () => await this.manager.DeleteAsync(id));
            Assert.Equal(HttpStatusCode.NotFound, ex.Response.StatusCode);
        }

        [Fact]
        public async Task DeleteAsync_Throws_BadRequestIfInvalidId()
        {
            // Act/Assert
            HttpResponseException ex = await AssertEx.ThrowsAsync<HttpResponseException>(() => this.manager.DeleteAsync("invalid"));
            Assert.Equal(HttpStatusCode.BadRequest, ex.Response.StatusCode);
        }

        [Fact]
        public async Task UndeleteAsync_SetsDeletedToFalse()
        {
            // Arrange
            Person person = TestData.Persons.First();
            person.PartitionKey = person.LastName;
            person.RowKey = person.FirstName;
            person.CreatedAt = null;
            person.UpdatedAt = null;
            person.Deleted = true;

            person = await this.manager.InsertAsync(person);
            Assert.True(person.Deleted);

            // Act
            person = await this.manager.UndeleteAsync(person.Id, null);

            // Assert
            Assert.False(person.Deleted);
        }

        [Fact]
        public async Task UndeleteAsync_UpdatesAndUndeletes()
        {
            // Arrange
            Person person = TestData.Persons.First();
            person.PartitionKey = person.LastName;
            person.RowKey = person.FirstName;
            person.CreatedAt = null;
            person.UpdatedAt = null;
            person.Deleted = true;

            person = await this.manager.InsertAsync(person);
            Assert.True(person.Deleted);

            // Act
            var delta = new Delta<Person>();
            delta.TrySetPropertyValue("FirstName", "abc");
            person = await this.manager.UndeleteAsync(person.Id, delta);

            // Assert
            Assert.False(person.Deleted);
            Assert.Equal("abc", person.FirstName);
        }

        [Fact]
        public async Task UndeleteAsync_ThrowsNotFound_IfIdNotFound()
        {
            string id = new CompositeTableKey("unknown", "item").ToString();

            // Act/Assert
            HttpResponseException ex = await AssertEx.ThrowsAsync<HttpResponseException>(async () => await this.manager.UndeleteAsync(id, null));
            Assert.Equal(HttpStatusCode.NotFound, ex.Response.StatusCode);
        }

        [Fact]
        public async Task InsertAsync_CreatesStorageTable()
        {
            var isolatedManager = new StorageDomainManagerMock(this, "insertTableNotFound");
            try
            {
                await isolatedManager.Table.DeleteIfExistsAsync();
                Assert.False(isolatedManager.Table.Exists());

                var person = TestData.Persons.First();
                var personClone = new Person()
                {
                    FirstName = person.FirstName,
                    LastName = person.LastName,
                    Age = person.Age,
                    PartitionKey = person.LastName,
                    RowKey = person.FirstName
                };

                await isolatedManager.InsertAsync(personClone);
                Assert.True(isolatedManager.Table.Exists());
            }
            finally
            {
                isolatedManager.Dispose();
            }
        }

        [Fact]
        public async Task ReplaceAsync_CreatesStorageTable()
        {
            var isolatedManager = new StorageDomainManagerMock(this, "replaceTableNotFound");
            try
            {
                await isolatedManager.Table.DeleteIfExistsAsync();
                Assert.False(isolatedManager.Table.Exists());

                var person = TestData.Persons.First();
                var personClone = new Person()
                {
                    FirstName = person.FirstName,
                    LastName = person.LastName,
                    Age = person.Age,
                    PartitionKey = person.LastName,
                    RowKey = person.FirstName,
                    ETag = "*"
                };

                // First replace attempt will fail due to table not found (404) exception. ExecuteOperationAsync will create
                // the table on retry, but the attempt will fail again due to resource not found (404). The consecutive 404s
                // verify that ExecuteOperationAsync does not continue to recurse.
                HttpResponseException ex = await AssertEx.ThrowsAsync<HttpResponseException>(async () => await isolatedManager.ReplaceAsync(personClone.Id, personClone));
                Assert.Equal(HttpStatusCode.BadRequest, ex.Response.StatusCode);

                Assert.True(isolatedManager.Table.Exists());
            }
            finally
            {
                isolatedManager.Dispose();
            }
        }

        [Fact]
        public async Task LookupAsync_DoesNotCreateStorageTable()
        {
            // Behavior is same for Delete, Undelete, Update and Lookup which each depend on GetCurrentItem
            var isolatedManager = new StorageDomainManagerMock(this, "lookupTableNotFound");
            try
            {
                await isolatedManager.Table.DeleteIfExistsAsync();
                Assert.False(isolatedManager.Table.Exists());

                string id = new CompositeTableKey("unknown", "item").ToString();
                HttpResponseException ex = await AssertEx.ThrowsAsync<HttpResponseException>(async () => await isolatedManager.LookupAsync(id));
                Assert.Equal(HttpStatusCode.NotFound, ex.Response.StatusCode);

                // Table.ExecuteAsync for retrieve returns TableResult with 404 status code when table is not found
                // instead of throwing exception. As a result, the lookup fails fast and we do not create the table.
                Assert.False(isolatedManager.Table.Exists());
            }
            finally
            {
                isolatedManager.Dispose();
            }
        }

        [Fact]
        public async Task QueryAsync_CreatesStorageTable()
        {
            var isolatedManager = new StorageDomainManagerMock(this, "queryTableNotFound");
            try
            {
                await isolatedManager.Table.DeleteIfExistsAsync();
                Assert.False(isolatedManager.Table.Exists());

                ODataQueryContext context = new ODataQueryContext(EdmModelMock.Create<Person>("Persons"), typeof(Person));
                ODataQueryOptions query = new ODataQueryOptions(context, this.request);

                IEnumerable<Person> actual = await isolatedManager.QueryAsync(query);
                Assert.Equal(0, actual.Count());

                Assert.True(isolatedManager.Table.Exists());
            }
            finally
            {
                isolatedManager.Dispose();
            }
        }

        [Theory]
        [MemberData("NextPageData")]
        public void SetNextPageLink_CreatesCorrectUri(string address, TableContinuationToken continuation, string expected)
        {
            // Arrange
            this.request.RequestUri = new Uri(address);

            // Act
            this.manager.SetNextPageLink(continuation);
            Uri actual = this.manager.Request.ODataProperties().NextLink;

            // Assert
            Assert.Equal(address + expected, actual.AbsoluteUri);
        }

        [Fact]
        public void GetContinuationToken_ReturnsToken_WhenSpecifiedInRequest()
        {
            HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Get, "http://localhost?NextPartitionKey=PartitionKeyValue&NextRowKey=RowKeyValue&NextTableName=TableNameValue");
            TableContinuationToken token = StorageDomainManager<Person>.GetContinuationToken(req);

            Assert.Equal("PartitionKeyValue", token.NextPartitionKey);
            Assert.Equal("RowKeyValue", token.NextRowKey);
            Assert.Equal("TableNameValue", token.NextTableName);

            req = new HttpRequestMessage(HttpMethod.Get, "http://localhost?NextPartitionKey=PartitionKeyValue&NextRowKey=RowKeyValue");
            token = StorageDomainManager<Person>.GetContinuationToken(req);

            Assert.Equal("PartitionKeyValue", token.NextPartitionKey);
            Assert.Equal("RowKeyValue", token.NextRowKey);
            Assert.Equal(null, token.NextTableName);

            req = new HttpRequestMessage(HttpMethod.Get, "http://localhost?NextPartitionKey=PartitionKeyValue");
            token = StorageDomainManager<Person>.GetContinuationToken(req);

            Assert.Equal("PartitionKeyValue", token.NextPartitionKey);
            Assert.Equal(null, token.NextRowKey);
            Assert.Equal(null, token.NextTableName);

            req = new HttpRequestMessage(HttpMethod.Get, "http://localhost?");
            token = StorageDomainManager<Person>.GetContinuationToken(req);
            Assert.Null(token);
        }

        [Fact]
        public async Task ConvertStorageException_BadRequest_IfInvalidErrorType()
        {
            CompositeTableKey key = new CompositeTableKey("KeyPart1", "KeyPart2");
            StorageException storageException = new StorageException("ConvertStorageException invalid error type test", null);

            HttpResponseException ex = await this.manager.ConvertStorageException(storageException, key);

            Assert.Equal(HttpStatusCode.BadRequest, ex.Response.StatusCode);
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                this.disposed = true;
                if (disposing)
                {
                    if (this.manager != null)
                    {
                        this.manager.Dispose();
                    }

                    if (this.request != null)
                    {
                        this.request.Dispose();
                    }

                    if (this.config != null)
                    {
                        this.config.Dispose();
                    }
                }
            }
        }

        private async Task InsertPersons(IEnumerable<Person> persons)
        {
            foreach (Person person in persons)
            {
                person.PartitionKey = person.LastName;
                person.RowKey = person.FirstName;
                person.CreatedAt = null;
                person.UpdatedAt = null;

                Person result = await this.manager.InsertAsync(person);

                Assert.Equal(person.FirstName, result.FirstName);
                Assert.Equal(person.LastName, result.LastName);
                Assert.Equal(person.PartitionKey, result.PartitionKey);
                Assert.Equal(person.RowKey, result.RowKey);

                Assert.NotNull(result.CreatedAt);
                Assert.NotNull(result.UpdatedAt);
            }
        }

        private class StorageDomainManagerMock : StorageDomainManager<Person>, IDisposable
        {
            private bool disposed;

            public StorageDomainManagerMock(StorageDomainManagerTests parent)
                : this(parent, StorageDomainManagerTests.Table)
            {
            }

            public StorageDomainManagerMock(StorageDomainManagerTests parent, string tableName)
                : base(StorageDomainManagerTests.ConnectionStringName,
                    "{0}{1}".FormatInvariant(tableName, Guid.NewGuid().ToString("N")),
                    parent.request, parent.validationSettings, parent.querySettings)
            {
            }

            public new CloudStorageAccount StorageAccount
            {
                get
                {
                    return base.StorageAccount;
                }
            }

            public new CloudTable Table
            {
                get
                {
                    return base.Table;
                }
            }

            public new CloudStorageAccount GetCloudStorageAccount(string connectionStringName)
            {
                return base.GetCloudStorageAccount(connectionStringName);
            }

            public new void SetNextPageLink(TableContinuationToken continuationToken)
            {
                base.SetNextPageLink(continuationToken);
            }

            public void Dispose()
            {
                this.Dispose(true);
                GC.SuppressFinalize(this);
            }

            protected virtual void Dispose(bool disposing)
            {
                if (!this.disposed)
                {
                    this.disposed = true;
                    if (disposing)
                    {
                        base.Table.DeleteIfExists();
                    }
                }
            }
        }
    }
}