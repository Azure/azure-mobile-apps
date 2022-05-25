// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using System.Web.Http.OData;
using System.Web.Http.Services;
using System.Web.Http.Tracing;
using Microsoft.Azure.Mobile;
using Microsoft.Azure.Mobile.Server;
using Microsoft.Azure.Mobile.Server.Tables;
using Moq;
using TestUtilities;
using Xunit;

namespace System.Web.Http
{
    public class TableHttpRequestMessageExtensionsTests
    {
        private HttpRequestMessage request;
        private Mock<HttpActionDescriptor> actionDescriptorMock;
        private HttpActionDescriptor actionDescriptor;

        public TableHttpRequestMessageExtensionsTests()
        {
            this.request = new HttpRequestMessage();
            this.actionDescriptorMock = new Mock<HttpActionDescriptor>();
            this.actionDescriptor = this.actionDescriptorMock.Object;
        }

        public static TheoryDataCollection<FilterInfo[], bool> QueryableFilterInfoData
        {
            get
            {
                return new TheoryDataCollection<FilterInfo[], bool>
                {
                    { new FilterInfo[] { new FilterInfo(new TestFilter1(), FilterScope.Global) }, false },
                    { new FilterInfo[] { new FilterInfo(new QueryableAttribute(), FilterScope.Action) }, true },
                    { new FilterInfo[] { new FilterInfo(new QueryableAttribute(), FilterScope.Controller) }, true },
                    { new FilterInfo[] { new FilterInfo(new QueryableAttribute(), FilterScope.Global) }, true },
                    { new FilterInfo[] { new FilterInfo(new EnableQueryAttribute(), FilterScope.Action) }, true },
                    { new FilterInfo[] { new FilterInfo(new EnableQueryAttribute(), FilterScope.Controller) }, true },
                    { new FilterInfo[] { new FilterInfo(new EnableQueryAttribute(), FilterScope.Global) }, true },
                    { new FilterInfo[] { new FilterInfo(new DerivedQueryableAttribute(), FilterScope.Action) }, false },
                    { new FilterInfo[] { new FilterInfo(new DerivedQueryableAttribute(), FilterScope.Controller) }, false },
                    { new FilterInfo[] { new FilterInfo(new DerivedQueryableAttribute(), FilterScope.Global) }, false },
                    { new FilterInfo[] { new FilterInfo(new DerivedEnableQueryAttribute(), FilterScope.Action) }, false },
                    { new FilterInfo[] { new FilterInfo(new DerivedEnableQueryAttribute(), FilterScope.Controller) }, false },
                    { new FilterInfo[] { new FilterInfo(new DerivedEnableQueryAttribute(), FilterScope.Global) }, false },
                    { new FilterInfo[] { new FilterInfo(new MockFilterTracerWithInnerQueryable(), FilterScope.Global) }, true },
                    { new FilterInfo[] { new FilterInfo(new MockFilterTracerWithoutInnerQueryable(), FilterScope.Global) }, false },
                };
            }
        }

        public static TheoryDataCollection<Type, string[]> TableModelTypes
        {
            get
            {
                return new TheoryDataCollection<Type, string[]>
                {
                    { typeof(ModelA), new string[] { "Id", "Name", "Age", "CreatedAt", "UpdatedAt", "Deleted", "Version" } },
                    { typeof(ModelB), new string[] { "Id", "Name", "Age", "CreatedAt", "UpdatedAt", "Deleted", "Version" } },
                    { typeof(ModelC), new string[] { "Name", "Age", "PartitionKey", "RowKey", "Timestamp", "ETag", "CreatedAt", "Deleted" } },                
                    { typeof(ModelD), new string[] { "Id", "你好世界", "CreatedAt", "UpdatedAt", "Deleted", "Version" } },
                };
            }
        }

        public static TheoryDataCollection<string, Type, string[], string, bool> SelectData
        {
            get
            {
                return new TheoryDataCollection<string, Type, string[], string, bool>
                {
                    { "?$filter=createdAt%20gt%20datetime'2013-12-07T21:55:45.8687306Z'%20or%20createdAt%20gt%20datetime'2013-12-07T21:55:45.8687306Z'", typeof(ModelB), new string[] { "Id", "Name", "Age", "CreatedAt", "UpdatedAt", "Version", "Deleted" }, "?$filter=CreatedAt%20gt%20datetime'2013-12-07T21:55:45.8687306Z'%20or%20CreatedAt%20gt%20datetime'2013-12-07T21:55:45.8687306Z'&$select=Name,Age,Id,Version,CreatedAt,UpdatedAt,Deleted", true },
                    { string.Empty, typeof(Uri), null, null, false },
                    { string.Empty, typeof(ModelA), new string[] { "Id", "Name", "Age", "Version", "CreatedAt", "UpdatedAt", "Deleted" }, "?$select=Id,Version,CreatedAt,UpdatedAt,Deleted,Name,Age", true },
                    { "?$filter=createdAt%20gt%20datetime'2013-12-07T21:55:45.8687306Z'", typeof(ModelB), new string[] { "Id", "Name", "Age", "Version", "CreatedAt", "UpdatedAt", "Deleted" }, "?$filter=CreatedAt%20gt%20datetime'2013-12-07T21:55:45.8687306Z'&$select=Name,Age,Id,Version,CreatedAt,UpdatedAt,Deleted", true },
                    { "?$filter=startswith(name, 'Sea')", typeof(ModelB), new string[] { "Id", "Name", "Age", "Version", "CreatedAt", "UpdatedAt", "Deleted" }, "?$filter=startswith(Name,%20'Sea')&$select=Name,Age,Id,Version,CreatedAt,UpdatedAt,Deleted", true },
                    { "?$filter=name%20eq%20'Name'&$orderby=name&$skip=10&$top=10", typeof(ModelB), new string[] { "Id", "Name", "Age", "Version", "CreatedAt", "UpdatedAt", "Deleted" }, "?$filter=Name%20eq%20'Name'&$orderby=Name&$skip=10&$top=10&$select=Name,Age,Id,Version,CreatedAt,UpdatedAt,Deleted", true },
                    { "?$select=%E4%BD%A0%E5%A5%BD%E4%B8%96%E7%95%8C", typeof(ModelA), new string[] { "你好世界" }, "?$select=%E4%BD%A0%E5%A5%BD%E4%B8%96%E7%95%8C", true },
                    { "?$filter=name%20eq%20'%E4%BD%A0%E5%A5%BD%E4%B8%96%E7%95%8C'", typeof(ModelA), new string[] { "Id", "Name", "Age", "Version", "CreatedAt", "UpdatedAt", "Deleted" }, "?$filter=Name%20eq%20'%E4%BD%A0%E5%A5%BD%E4%B8%96%E7%95%8C'&$select=Id,Version,CreatedAt,UpdatedAt,Deleted,Name,Age", true },
                    { "?$select=name", typeof(ModelA), new string[] { "Name" }, "?$select=Name", true },
                    { "?$select=name,createdAt", typeof(ModelA), new string[] { "Name", "CreatedAt" }, "?$select=Name,CreatedAt", true },
                    { "?$select=name,createdAt", typeof(ModelB), new string[] { "Name", "CreatedAt" }, "?$select=Name,CreatedAt", true },
                    { "?$select=name,createdAt", typeof(ModelC), new string[] { "Name", "CreatedAt" }, "?$select=Name,CreatedAt", true },
                    { "?$select=name,%63%72%65%61%74%65%64%41%74", typeof(ModelB), new string[] { "Name", "CreatedAt" }, "?$select=Name,CreatedAt", true },
                    { "?$select=createdAt", typeof(ModelC), new string[] { "CreatedAt" }, "?$select=CreatedAt", true },
                    { "?$select=createdAt,updatedAt", typeof(ModelC), new string[] { "CreatedAt", "updatedAt" }, "?$select=CreatedAt,updatedAt", true },
                    { "?$select=name,age,createdAt", typeof(ModelB), new string[] { "Name", "Age", "CreatedAt" }, "?$select=Name,Age,CreatedAt", true },
                    { string.Empty, typeof(ModelC), new string[] { "Name", "Age", "PartitionKey", "RowKey", "Timestamp", "ETag", "Deleted", "CreatedAt" }, "?$select=Name,Age,CreatedAt,Deleted,PartitionKey,RowKey,Timestamp,ETag", true },
                    { string.Empty, typeof(ModelD), new string[] { "Id", "你好世界", "CreatedAt", "UpdatedAt", "Version", "Deleted" }, "?$select=%E4%BD%A0%E5%A5%BD%E4%B8%96%E7%95%8C,Id,Version,CreatedAt,UpdatedAt,Deleted", true },
                    { "?$select=name,age,PartitionKey,RowKey,Timestamp,ETag,version,createdAt", typeof(ModelC), new string[] { "Name", "Age", "PartitionKey", "RowKey", "Timestamp", "ETag", "version", "CreatedAt" }, "?$select=Name,Age,PartitionKey,RowKey,Timestamp,ETag,version,CreatedAt", true },
                    { "?$select=name,age,PartitionKey,RowKey,Timestamp,ETag,version,createdAt", typeof(ModelC), new string[] { "Name", "Age", "PartitionKey", "RowKey", "Timestamp", "ETag", "version", "CreatedAt" }, "?$select=Name,Age,PartitionKey,RowKey,Timestamp,ETag,version,CreatedAt", true },
                };
            }
        }

        public static TheoryDataCollection<string, Type, string> QueryOptionData
        {
            get
            {
                return new TheoryDataCollection<string, Type, string>
                {
                    { "startswith(name,'name')", typeof(ModelB), "startswith(Name,'name')" },
                    { "startswith(name,%20'name')", typeof(ModelB), "startswith(Name,%20'name')" },
                    { "startswith(name,+'name')", typeof(ModelB), "startswith(Name,+'name')" },

                    { "(age%20add%204)%20eq%208", typeof(ModelB), "(Age%20add%204)%20eq%208" },
                    { "(age+add+4)+eq+8", typeof(ModelB), "(Age+add+4)+eq+8" },

                    { "(somename%20add%204)%20eq%208", typeof(ModelB), "(somename%20add%204)%20eq%208" },
                    { "(somename+add+4)+eq+8", typeof(ModelB), "(somename+add+4)+eq+8" },

                    { "(someage%20add%204)%20eq%208", typeof(ModelB), "(someage%20add%204)%20eq%208" },
                    { "(someage+add+4)+eq+8", typeof(ModelB), "(someage+add+4)+eq+8" },

                    { "name%20eq%20'Produce'", typeof(ModelB), "Name%20eq%20'Produce'" },
                    { "name+eq+'Produce'", typeof(ModelB), "Name+eq+'Produce'" },

                    { "name,age", typeof(ModelB), "Name,Age" },
                    { "name,+age", typeof(ModelB), "Name,+Age" },
                    { "name,%20age", typeof(ModelB), "Name,%20Age" },
                    { "%E4%BD%A0%E5%A5%BD%E4%B8%96%E7%95%8C", typeof(ModelA), "%E4%BD%A0%E5%A5%BD%E4%B8%96%E7%95%8C" },

                    { "name%20eq%20'%E4%BD%A0%E5%A5%BD%E4%B8%96%E7%95%8C'", typeof(ModelA), "Name%20eq%20'%E4%BD%A0%E5%A5%BD%E4%B8%96%E7%95%8C'" },
                    { "name+eq+'%E4%BD%A0%E5%A5%BD%E4%B8%96%E7%95%8C'", typeof(ModelA), "Name+eq+'%E4%BD%A0%E5%A5%BD%E4%B8%96%E7%95%8C'" },

                    { "name", typeof(ModelA), "Name" },
                    { "name,id,id,name", typeof(ModelE), "Name,Id,Id,Name" },
                    { "name,%20id,%20id,%20name", typeof(ModelE), "Name,%20Id,%20Id,%20Name" },
                    { "name,+id,+id,+name", typeof(ModelE), "Name,+Id,+Id,+Name" },

                    { "'1'%20eq%20id", typeof(ModelB), "'1'%20eq%20Id" },
                    { "'1'+eq+id", typeof(ModelB), "'1'+eq+Id" },

                    { "my%E4%BD%A0%E5%A5%BD%E4%B8%96%E7%95%8C", typeof(ModelE), "My%E4%BD%A0%E5%A5%BD%E4%B8%96%E7%95%8C" },
                    { "%E4%BD%A0%E5%A5%BD%E4%B8%96%E7%95%8Ce", typeof(ModelE), "%E4%BD%A0%E5%A5%BD%E4%B8%96%E7%95%8Ce" },

                    { "name,%E4%BD%A0%E5%A5%BD%E4%B8%96%E7%95%8Ce,age", typeof(ModelB), "Name,%E4%BD%A0%E5%A5%BD%E4%B8%96%E7%95%8Ce,Age" },
                    { "name,%E4%BD%A0%E5%A5%BD%E4%B8%96%E7%95%8Ce,+age", typeof(ModelB), "Name,%E4%BD%A0%E5%A5%BD%E4%B8%96%E7%95%8Ce,+Age" },
                };
            }
        }

        public static TheoryDataCollection<string, IDictionary<string, string>, string> SystemPropertiesQueryOptionData
        {
            get
            {
                return new TheoryDataCollection<string, IDictionary<string, string>, string>
                {
                    {
                        "createdAt%20gt%20datetime'2013-12-07T21:55:45.8687306Z'",
                        new Dictionary<string, string>() { { "createdAt", "MyOwnCreatedAt" } },
                        "MyOwnCreatedAt%20gt%20datetime'2013-12-07T21:55:45.8687306Z'"
                    },
                    {
                        "day(updatedAt)%20eq%208",
                        new Dictionary<string, string>() { { "updatedAt", "Timestamp" } },
                        "day(Timestamp)%20eq%208"
                    }
                };
            }
        }

        public static TheoryDataCollection<EntityTagHeaderValue, string> VersionETagData
        {
            get
            {
                return new TheoryDataCollection<EntityTagHeaderValue, string>
                {
                    { null, null },
                    { EntityTagHeaderValue.Any, null },
                    { EntityTagHeaderValue.Parse("\"QUJDREVG\""), "ABCDEF" },
                };
            }
        }

        [Theory]
        [MemberData("VersionETagData")]
        public void GetVersionFromIfMatch_ReturnsVersion(EntityTagHeaderValue entityTag, string expected)
        {
            // Arrange
            if (entityTag != null)
            {
                this.request.Headers.IfMatch.Add(entityTag);
            }

            byte[] expectedData = expected != null ? Encoding.UTF8.GetBytes(expected) : null;

            // Act
            byte[] actual = this.request.GetVersionFromIfMatch();

            // Assert
            Assert.Equal(expectedData, actual);
        }

        [Fact]
        public void GetVersionFromIfMatch_ThrowsOnWeakETag()
        {
            this.request.Headers.IfMatch.Add(EntityTagHeaderValue.Parse("W/\"abcdef\""));

            // Act
            HttpResponseException ex = Assert.Throws<HttpResponseException>(() => this.request.GetVersionFromIfMatch());
            HttpError error;
            ex.Response.TryGetContentValue<HttpError>(out error);

            // Assert
            Assert.Equal("The HTTP If-Match header is invalid: 'W/\"abcdef\"'. Updating an existing resource requires a single, strong ETag, or a wildcard ETag.", error.Message);
        }

        [Fact]
        public void GetVersionFromIfMatch_ThrowsOnMultipleETags()
        {
            this.request.Headers.IfMatch.Add(EntityTagHeaderValue.Parse("\"abcdef\""));
            this.request.Headers.IfMatch.Add(EntityTagHeaderValue.Parse("\"fedcba\""));

            // Act
            HttpResponseException ex = Assert.Throws<HttpResponseException>(() => this.request.GetVersionFromIfMatch());
            HttpError error;
            ex.Response.TryGetContentValue<HttpError>(out error);

            // Assert
            Assert.Equal("The HTTP If-Match header is invalid: '\"abcdef\", \"fedcba\"'. Updating an existing resource requires a single, strong ETag, or a wildcard ETag.", error.Message);
        }

        [Fact]
        public void GetVersionFromIfMatch_ThrowsOnInvalidEncodedETag()
        {
            HttpRequestMessage request = new HttpRequestMessage();
            request.Headers.IfMatch.Add(EntityTagHeaderValue.Parse("\"abcdef\""));

            // Act
            HttpResponseException ex = Assert.Throws<HttpResponseException>(() => request.GetVersionFromIfMatch());
            HttpError error;
            ex.Response.TryGetContentValue<HttpError>(out error);

            // Assert
            Assert.Equal("The HTTP If-Match header is invalid: '\"abcdef\"'. Updating an existing resource requires a single, strong ETag, or a wildcard ETag.", error.Message);
        }

        [Theory]
        [MemberData("QueryableFilterInfoData")]
        public void IsQueryableAction_DetectsQueryableActions(FilterInfo[] filterInfos, bool expected)
        {
            // Arrange
            this.actionDescriptorMock.Setup<Collection<FilterInfo>>(a => a.GetFilterPipeline())
                .Returns(new Collection<FilterInfo>(filterInfos))
                .Verifiable();

            // Act
            bool actual = this.request.IsQueryableAction(this.actionDescriptor);

            // Assert
            this.actionDescriptorMock.Verify();
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void IsQueryableAction_CachesResult()
        {
            // Arrange
            this.actionDescriptorMock.Setup<Collection<FilterInfo>>(a => a.GetFilterPipeline())
                .Returns(new Collection<FilterInfo> { new FilterInfo(new QueryableAttribute(), FilterScope.Action) });

            // Act
            bool actual1 = this.request.IsQueryableAction(this.actionDescriptor);
            bool actual2 = this.request.IsQueryableAction(this.actionDescriptor);

            // Assert
            Assert.Equal(actual1, actual2);
            this.actionDescriptorMock.Verify<Collection<FilterInfo>>(a => a.GetFilterPipeline(), Times.Once());
        }

        [Theory]
        [MemberData("TableModelTypes")]
        public void GetMappedModelProperties_ReturnsExpectedProperties(Type type, string[] expected)
        {
            // Act
            string[] actual = TableHttpRequestMessageExtensions.GetMappedModelProperties(this.request, type);

            // Assert
            VerifyMatch(expected, actual);
        }

        [Fact]
        public void GetMappedModelProperties_ReturnsEmptyOnExceptionThrown()
        {
            // Arrange
            Mock<Type> typeMock = new Mock<Type> { CallBase = true };
            typeMock.Setup(t => t.Name)
                .Returns("typeMock");
            typeMock.Setup(t => t.GetProperties(It.IsAny<BindingFlags>()))
                .Throws<InvalidOperationException>()
                .Verifiable();
            typeMock.Setup<int>(t => t.GetHashCode())
                .Returns(1024);

            Mock<ITraceWriter> tracerMock = new Mock<ITraceWriter>();
            tracerMock.Setup(t => t.Trace(this.request, ServiceLogCategories.TableControllers, TraceLevel.Error, It.IsAny<Action<TraceRecord>>()))
                .Callback<HttpRequestMessage, string, TraceLevel, Action<TraceRecord>>((req, cat, level, rec) =>
                {
                    TraceRecord record = new TraceRecord(req, cat, level);
                    rec(record);
                    Assert.Contains("Could not determine the properties for type 'typeMock'.", record.Message);
                })
                .Verifiable();
            HttpConfiguration config = new HttpConfiguration();
            config.Services.Replace(typeof(ITraceWriter), tracerMock.Object);
            this.request.SetConfiguration(config);

            // Act
            string[] actual = TableHttpRequestMessageExtensions.GetMappedModelProperties(this.request, typeMock.Object);

            // Assert
            typeMock.Verify();
            tracerMock.Verify();
            Assert.Empty(actual);
        }

        [Theory]
        [MemberData("SelectData")]
        public void SetSelectedProperties_SetsProperties(string originalQuery, Type type, ICollection<string> expectedProperties, string expectedQuery, bool expectedModified)
        {
            // Arrange
            this.request.RequestUri = new Uri("http://localhost" + originalQuery);

            // Act
            bool actualModified;
            IList<string> actual = this.request.SetSelectedProperties(type, out actualModified);

            // Assert
            VerifyMatch(expectedProperties, actual);
            if (expectedQuery != null)
            {
                string actualQuery = this.request.RequestUri.Query;
                Assert.Equal(expectedQuery, actualQuery);
            }
            else
            {
                string actualQuery = this.request.RequestUri.Query;
                Assert.Equal(originalQuery, actualQuery);
            }

            Assert.Equal(expectedModified, actualModified);
        }

        [Fact]
        public void SystemProperties_MatchITableDataDefinition()
        {
            // Arrange
            string[] expected = typeof(ITableData).GetProperties().Select(p => p.Name).ToArray();

            // Act
            List<string> actual = new List<string>(TableHttpRequestMessageExtensions.SystemProperties.Values)
            {
                "Id"
            };

            // Assert
            VerifyMatch(expected, actual);
        }

        [Theory]
        [MemberData("QueryOptionData")]
        public void GetMappedModelProperties_ReturnsPascalCasedQueryProperties(string option, Type type, string expected)
        {
            // Arrange
            string[] properties = TableHttpRequestMessageExtensions.GetMappedModelProperties(this.request, type);

            // Act
            string actual = TableHttpRequestMessageExtensions.PascalCasedQueryOption(option, properties);

            // Assert
            Assert.Equal(expected, actual);
        }

        [Theory]
        [MemberData("SystemPropertiesQueryOptionData")]
        public void TransformSystemProperties_TransformsSystemProperties(string option, IDictionary<string, string> map, string expected)
        {
            // Act
            string actual = TableHttpRequestMessageExtensions.TransformSystemProperties(option, map);

            // Assert
            Assert.Equal(expected, actual);
        }

        private static void VerifyMatch(ICollection<string> expected, ICollection<string> actual)
        {
            if (expected == null && actual == null)
            {
                return;
            }

            Assert.Equal(expected.Count, actual.Count);
            foreach (string value in actual)
            {
                Assert.True(expected.Contains(value));
            }
        }

        private class TestFilter1 : IFilter
        {
            public bool AllowMultiple
            {
                get
                {
                    return false;
                }
            }
        }

        private sealed class DerivedQueryableAttribute : QueryableAttribute
        {
        }

        private sealed class DerivedEnableQueryAttribute : EnableQueryAttribute
        {
        }

        private sealed class MockFilterTracerWithInnerQueryable : IFilter, IDecorator<IFilter>
        {
            public bool AllowMultiple
            {
                get { throw new NotImplementedException(); }
            }

            public IFilter Inner
            {
                get { return new QueryableAttribute(); }
            }
        }

        private sealed class MockFilterTracerWithoutInnerQueryable : IFilter, IDecorator<IFilter>
        {
            public bool AllowMultiple
            {
                get { throw new NotImplementedException(); }
            }

            public IFilter Inner
            {
                get { return new DerivedQueryableAttribute(); }
            }
        }

        private class ModelA : ITableData
        {
            public string Id { get; set; }

            public byte[] Version { get; set; }

            public DateTimeOffset? CreatedAt { get; set; }

            public DateTimeOffset? UpdatedAt { get; set; }

            public bool Deleted { get; set; }

            public string Name { get; set; }

            public int Age { get; set; }

            [NotMapped]
            public string Hidden { get; set; }
        }

        private class ModelB : EntityData
        {
            public string Name { get; set; }

            public int Age { get; set; }

            [NotMapped]
            public string Hidden { get; set; }
        }

        private class ModelC : StorageData
        {
            public string Name { get; set; }

            public int Age { get; set; }

            [NotMapped]
            public string Hidden { get; set; }
        }

        private class ModelD : EntityData
        {
            public string 你好世界 { get; set; }

            [NotMapped]
            public string Hidden { get; set; }
        }

        private class ModelE : EntityData
        {
            public string D { get; set; }

            public string E { get; set; }

            public string My你好世界 { get; set; }

            public string 你好世界e { get; set; }

            public string Name { get; set; }
        }
    }
}