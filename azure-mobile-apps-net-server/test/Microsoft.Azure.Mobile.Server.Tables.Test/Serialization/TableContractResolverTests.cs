// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http.Formatting;
using System.Text;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.OData;
using Microsoft.Azure.Mobile.Server.Tables;
using Moq;
using Moq.Protected;
using Newtonsoft.Json.Serialization;
using TestUtilities;
using Xunit;

namespace Microsoft.Azure.Mobile.Server.Serialization
{
    public class TableContractResolverTests
    {
        private JsonMediaTypeFormatter formatter;
        private Mock<TableContractResolver> resolverMock;
        private TableContractResolver resolver;
        private Encoding encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

        public TableContractResolverTests()
        {
            var config = new HttpConfiguration();

            // Formatter initialization happens in the TableControllerConfigAttribute
            TableControllerConfigAttribute tableConfig = new TableControllerConfigAttribute();
            var descriptor = new HttpControllerDescriptor { Configuration = config };
            var settings = new HttpControllerSettings(config);
            tableConfig.Initialize(settings, descriptor);

            this.formatter = settings.Formatters.JsonFormatter;
            this.resolverMock = new Mock<TableContractResolver>(this.formatter) { CallBase = true };
            this.resolver = this.resolverMock.Object;
        }

        public static TheoryDataCollection<Type> NonDeltaTypes
        {
            get
            {
                return new TheoryDataCollection<Type>
                {
                    typeof(int),
                    typeof(Guid),
                    typeof(List<string>),
                    typeof(List<int>),
                    typeof(Uri),
                };
            }
        }

        public static TheoryDataCollection<Type> DeltaTypes
        {
            get
            {
                return new TheoryDataCollection<Type>
                {
                    typeof(Delta<IDictionary<int, string>>),
                    typeof(Delta<List<string>>),
                    typeof(Delta<List<int>>),
                    typeof(Delta<Uri>),
                };
            }
        }

        public static TheoryDataCollection<string, Dictionary<string, object>> DeltaTestData
        {
            get
            {
                return new TheoryDataCollection<string, Dictionary<string, object>>
                {
                    { "{ \"string\":\"你好世界\" }", new Dictionary<string, object> { { "String", "你好世界" } } },
                    { "{ \"string\":null }", new Dictionary<string, object> { { "String", null } } },
                    { "{ \"bool\":true }", new Dictionary<string, object> { { "Bool", true } } },
                    { "{ \"bool\":false }", new Dictionary<string, object> { { "Bool", false } } },
                    { "{ \"byte\":97 }", new Dictionary<string, object> { { "Byte", Byte.Parse("97") } } },
                    { "{ \"bytes\":\"5L2g5aW95LiW55WM\" }", new Dictionary<string, object> { { "Bytes", Encoding.UTF8.GetBytes("你好世界") } } },
                    { "{ \"dateTime\":\"2013-12-07T21:55:45.8677285Z\" }", new Dictionary<string, object> { { "DateTime", DateTime.Parse("2013-12-07T21:55:45.8677285Z").ToUniversalTime() } } },
                    { "{ \"nullableDateTime\":\"2013-12-07T21:55:45.8687306Z\" }", new Dictionary<string, object> { { "NullableDateTime", DateTime.Parse("2013-12-07T21:55:45.8687306Z").ToUniversalTime() } } },
                    { "{ \"dateTimeOffset\":\"2013-12-07T21:55:45.8677285+00:00\" }", new Dictionary<string, object> { { "DateTimeOffset", DateTimeOffset.Parse("2013-12-07T21:55:45.8677285+00:00") } } },
                    { "{ \"nullableDateTimeOffset\":\"2013-12-07T21:55:45.8687306+00:00\" }", new Dictionary<string, object> { { "NullableDateTimeOffset", DateTimeOffset.Parse("2013-12-07T21:55:45.8687306+00:00") } } },
                    { "{ \"int\":1024 }", new Dictionary<string, object> { { "Int", 1024 } } },
                    { "{ \"nullableInt\":1024 }", new Dictionary<string, object> { { "NullableInt", 1024 } } },
                    { "{ \"long\":4000000000 }", new Dictionary<string, object> { { "Long", 4000000000L } } },
                    { "{ \"nullableLong\":4000000000 }", new Dictionary<string, object> { { "NullableLong", 4000000000L } } },
                    { "{ \"double\":1 }", new Dictionary<string, object> { { "Double", 1.0 } } },
                    { "{ \"double\":1.0 }", new Dictionary<string, object> { { "Double", 1.0 } } },
                    { "{ \"nullableDouble\":1 }", new Dictionary<string, object> { { "NullableDouble", 1.0 } } },
                    { "{ \"nullableDouble\":1.0 }", new Dictionary<string, object> { { "NullableDouble", 1.0 } } },
                    { "{ \"enum\": \"Saturday\" }", new Dictionary<string, object> { { "Enum", DayOfWeek.Saturday } } },
                    { "{ \"string\":\"你好世界\", \"bool\":true, \"bytes\":\"5L2g5aW95LiW55WM\" }", new Dictionary<string, object> { { "String", "你好世界" }, { "Bool", true }, { "Bytes", Encoding.UTF8.GetBytes("你好世界") } } },
                    { "{ \"bytes\":\"5L2g5aW95LiW55WM\", \"bool\":true, \"string\":\"你好世界\" }", new Dictionary<string, object> { { "String", "你好世界" }, { "Bool", true }, { "Bytes", Encoding.UTF8.GetBytes("你好世界") } } },
                    { "{ \"string\":null,\"nullableDateTime\":null,\"nullableBool\":null,\"double\":3839.892597829873,\"bytes\":null}",
                        new Dictionary<string, object> { { "String", null }, { "NullableDateTime", null }, { "NullableBool", null }, { "Double", 3839.892597829873 }, { "Bytes", null } } },
                    { "{ \"int\":0 }", new Dictionary<string, object> { { "Int", 0 } } },
                    { "{ \"nullableInt\":null }", new Dictionary<string, object> { { "NullableInt", null } } },
                    { "{ \"bool\":false }", new Dictionary<string, object> { { "Bool", false } } },
                    { "{ \"nullableBool\":null }", new Dictionary<string, object> { { "NullableBool", null } } },
                    { "{ \"string\":null }", new Dictionary<string, object> { { "String", null } } },
                    { "{ \"nullableDateTime\":null }", new Dictionary<string, object> { { "NullableDateTime", null } } },
                };
            }
        }

        public static TheoryDataCollection<string, string> DateTimeData
        {
            get
            {
                return new TheoryDataCollection<string, string>
                {
                    { "Thursday, Nov 01 2007 GMT", "\"2007-11-01T00:00:00Z\"" },
                    { "Thursday, Nov 01 2007 09:45:12 -07:00", "\"2007-11-01T16:45:12Z\"" },
                    { "2/22/2014 8 AM-08:00", "\"2014-02-22T16:00:00Z\"" },
                    { "2/22/2014 8:12:34 AM GMT", "\"2014-02-22T08:12:34Z\"" },
                    { "2/22/2014 8:12:34 AM+01:00", "\"2014-02-22T07:12:34Z\"" },
                    { "Sat, 22 Feb 2014 20:21:11 GMT", "\"2014-02-22T20:21:11Z\"" },
                    { "2014-02-22T20:23:22.6556474Z", "\"2014-02-22T20:23:22.655Z\"" },
                    { "2014-02-22T20:23:22.6555074+00:00", "\"2014-02-22T20:23:22.655Z\"" },
                    { "2014-02-22T12:24:26.1672087-08:00", "\"2014-02-22T20:24:26.167Z\"" },
                };
            }
        }

        [Theory]
        [MemberData("NonDeltaTypes")]
        public void CreateContract_DoesNotCallGetContractOnNonDeltaTypes(Type input)
        {
            // Act
            this.resolver.ResolveContract(input);

            // Assert
            this.resolverMock.Protected()
                .Verify<JsonContract>("GetDeltaContract", Times.Never(), input);
        }

        [Theory]
        [MemberData("DeltaTypes")]
        public void CreateContract_CallsGetContractOnDeltaTypes(Type input)
        {
            // Act
            this.resolver.ResolveContract(input);

            // Assert
            this.resolverMock.Protected()
                .Verify<JsonContract>("GetDeltaContract", Times.Once(), input);
        }

        [Theory]
        [MemberData("DeltaTypes")]
        public void CreateContract_UsesCacheOnDeltaTypes(Type input)
        {
            // Act
            this.resolver.ResolveContract(input);
            this.resolver.ResolveContract(input);

            // Assert
            this.resolverMock.Protected()
                .Verify<JsonContract>("GetDeltaContract", Times.Once(), input);
        }

        [Theory]
        [MemberData("DeltaTestData")]
        public void DeltaOfT_Deserialization_Works(string input, IDictionary<string, object> expected)
        {
            // Arrange
            Stream readStream = new MemoryStream(Encoding.UTF8.GetBytes(input));

            // Act
            object result = this.formatter.ReadFromStream(typeof(Delta<DeltaTest>), readStream, this.encoding, null);
            Delta<DeltaTest> actual = result as Delta<DeltaTest>;

            // Assert
            Assert.IsType<Delta<DeltaTest>>(result);
            IEnumerable<string> changes = actual.GetChangedPropertyNames();
            foreach (string change in changes)
            {
                object actualValue;
                actual.TryGetPropertyValue(change, out actualValue);
                Assert.Equal(expected[change], actualValue);
            }
        }

        [Theory]
        [MemberData("DateTimeData")]
        public void DateTime_Serialization_Works(string input, string expected)
        {
            // Arrange
            DateTime dateTime = DateTime.Parse(input);
            string actual = null;

            // Act
            using (MemoryStream memStream = new MemoryStream())
            {
                this.formatter.WriteToStream(typeof(DateTime), dateTime, memStream, this.encoding);
                byte[] data = memStream.ToArray();
                actual = this.encoding.GetString(data);
            }

            // Assert
            Assert.Equal(expected, actual);
        }

        [Theory]
        [MemberData("DateTimeData")]
        public void DateTimeOffset_Serialization_Works(string input, string expected)
        {
            // Arrange
            DateTimeOffset dateTimeOffset = DateTimeOffset.Parse(input);
            string actual = null;

            // Act
            using (MemoryStream memStream = new MemoryStream())
            {
                this.formatter.WriteToStream(typeof(DateTimeOffset), dateTimeOffset, memStream, this.encoding);
                byte[] data = memStream.ToArray();
                actual = this.encoding.GetString(data);
            }

            // Assert
            Assert.Equal(expected, actual);
        }

        public class DeltaTest
        {
            public string String { get; set; }

            public bool Bool { get; set; }

            public bool? NullableBool { get; set; }

            public byte Byte { get; set; }

            public byte[] Bytes { get; set; }

            public DateTime DateTime { get; set; }

            public DateTime? NullableDateTime { get; set; }

            public DateTimeOffset DateTimeOffset { get; set; }

            public DateTimeOffset? NullableDateTimeOffset { get; set; }

            public int Int { get; set; }

            public int? NullableInt { get; set; }

            public long Long { get; set; }

            public long? NullableLong { get; set; }

            public double Double { get; set; }

            public double? NullableDouble { get; set; }

            public DayOfWeek Enum { get; set; }
        }
    }
}