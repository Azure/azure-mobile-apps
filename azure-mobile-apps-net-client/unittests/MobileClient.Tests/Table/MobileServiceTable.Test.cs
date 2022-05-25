// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using Newtonsoft.Json.Linq;
using MobileClient.Tests.Helpers;
using Microsoft.WindowsAzure.MobileServices;

namespace MobileClient.Tests.Table
{
    public class MobileServiceTable_Test
    {
        [Fact]
        public async Task ReadAsync_DoesNotwrapResult_WhenParameterIsFalse()
        {
            JToken result = await TestReadResponse(@"[{
                                            ""id"":""abc"",
                                            ""String"":""Hey""
                                         }]", null, wrapResult: false);
            JToken[] items = result.ToArray();
            Assert.Single(items);
            Assert.Equal("abc", (string)items[0]["id"]);
            Assert.Equal("Hey", (string)items[0]["String"]);

            result = await TestReadResponse(@"{
                                            ""id"":""abc"",
                                            ""String"":""Hey""
                                         }", null, wrapResult: false);

            var item = result as JObject;
            Assert.NotNull(item);
            Assert.Equal("abc", (string)item["id"]);
            Assert.Equal("Hey", (string)item["String"]);

            result = await TestReadResponse(@"{
                                                ""count"": 53,
                                                ""results"": [
                                                {
                                                    ""id"":""abc"",
                                                    ""String"":""Hey""
                                                }]}", "http://contoso.com/tables/Todo?$top=1&$skip=2; rel=next", wrapResult: false);

            AssertResult(result, 53, null);
        }

        [Fact]
        public async Task ReadAsync_FormatsResult_WhenParameterIsTrue()
        {
            JToken result = await TestReadResponse(@"[{
                                                ""id"":""abc"",
                                                ""String"":""Hey""
                                                }]",
                                                   link: null,
                                                   wrapResult: true);
            AssertResult(result, -1, null);

            result = await TestReadResponse(@"{
                                            ""id"":""abc"",
                                            ""String"":""Hey""
                                            }",
                                              link: null,
                                              wrapResult: true);

            AssertResult(result, -1, null);


            result = await TestReadResponse(@"{
                                                ""count"": 53,
                                                ""results"": [
                                                {
                                                    ""id"":""abc"",
                                                    ""String"":""Hey""
                                                }]}",
                                                    link: "http://contoso.com/tables/Todo?$top=1&$skip=2; rel=next",
                                                    wrapResult: true);

            AssertResult(result, 53, "http://contoso.com/tables/Todo?$top=1&$skip=2");
        }

        private static void AssertResult(JToken result, long count, string link)
        {
            var item = result as JObject;
            Assert.NotNull(item);
            Assert.Equal(count, item["count"].Value<long>());
            Assert.Equal(link, (string)item["nextLink"]);
            var items = result["results"].ToArray();
            Assert.Equal("abc", (string)items[0]["id"]);
            Assert.Equal("Hey", (string)items[0]["String"]);
        }

        private static async Task<JToken> TestReadResponse(string response, string link, bool wrapResult)
        {
            TestHttpHandler hijack = new TestHttpHandler();
            hijack.SetResponseContent(response);
            if (!String.IsNullOrEmpty(link))
            {
                hijack.Responses[0].Headers.Add("Link", link);
            }
            IMobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);
            IMobileServiceTable table = service.GetTable("someTable");
            JToken result = await table.ReadAsync("this is a query", null, wrapResult);
            return result;
        }

        [Fact]
        public async Task ReadAsyncWithStringIdResponseContent()
        {
            string[] testIdData = IdTestData.ValidStringIds
                                    .Concat(IdTestData.EmptyStringIds)
                                    .Concat(IdTestData.InvalidStringIds)
                                    .ToArray();

            foreach (string testId in testIdData)
            {
                TestHttpHandler hijack = new TestHttpHandler();

                // Make the testId JSON safe
                string jsonTestId = testId.Replace("\\", "\\\\").Replace("\"", "\\\"");

                hijack.SetResponseContent("[{\"id\":\"" + jsonTestId + "\",\"String\":\"Hey\"}]");
                IMobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);

                IMobileServiceTable table = service.GetTable("someTable");

                JToken results = await table.ReadAsync("");
                JToken[] items = results.ToArray();

                Assert.Single(items);
                Assert.Equal(testId, (string)items[0]["id"]);
                Assert.Equal("Hey", (string)items[0]["String"]);
            }
        }

        [Fact]
        public async Task ReadAsyncWithIntIdResponseContent()
        {
            long[] testIdData = IdTestData.ValidIntIds.Concat(
                                 IdTestData.InvalidIntIds).ToArray();

            foreach (long testId in testIdData)
            {
                string stringTestId = testId.ToString();

                TestHttpHandler hijack = new TestHttpHandler();
                hijack.SetResponseContent("[{\"id\":" + stringTestId + ",\"String\":\"Hey\"}]");
                IMobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);

                IMobileServiceTable table = service.GetTable("someTable");

                JToken results = await table.ReadAsync("");
                JToken[] items = results.ToArray();

                Assert.Single(items);
                Assert.Equal(testId, (long)items[0]["id"]);
                Assert.Equal("Hey", (string)items[0]["String"]);
            }
        }

        [Fact]
        public async Task ReadAsyncWithNonStringAndNonIntIdResponseContent()
        {
            object[] testIdData = IdTestData.NonStringNonIntValidJsonIds;

            foreach (object testId in testIdData)
            {
                string stringTestId = testId.ToString().ToLower();

                TestHttpHandler hijack = new TestHttpHandler();
                hijack.SetResponseContent("[{\"id\":" + stringTestId + ",\"String\":\"Hey\"}]");
                IMobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);

                IMobileServiceTable table = service.GetTable("someTable");

                JToken results = await table.ReadAsync("");
                JToken[] items = results.ToArray();

                Assert.Single(items);
                Assert.Equal(testId, items[0]["id"].ToObject(testId.GetType()));
                Assert.Equal("Hey", (string)items[0]["String"]);
            }
        }

        [Fact]
        public async Task ReadAsyncWithNullIdResponseContent()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            hijack.SetResponseContent("[{\"id\":null,\"String\":\"Hey\"}]");
            IMobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);

            IMobileServiceTable table = service.GetTable("someTable");

            JToken results = await table.ReadAsync("");
            JToken[] items = results.ToArray();
            JObject item0 = items[0] as JObject;

            Assert.Single(items);
            Assert.Null((string)items[0]["id"]);
            Assert.Equal("Hey", (string)items[0]["String"]);
        }

        [Fact]
        public async Task ReadAsyncWithNoIdResponseContent()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            hijack.SetResponseContent("[{\"String\":\"Hey\"}]");
            IMobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);

            IMobileServiceTable table = service.GetTable("someTable");

            JToken results = await table.ReadAsync("");
            JToken[] items = results.ToArray();
            JObject item0 = items[0] as JObject;

            Assert.Single(items);
            Assert.DoesNotContain(item0.Properties(), p => string.Equals(p.Name, "id", StringComparison.InvariantCultureIgnoreCase));
            Assert.Equal("Hey", (string)items[0]["String"]);
        }

        [Theory]
        [InlineData("http://www.test.com", "http://www.test.com/about?$filter=a eq b&$orderby=c")]
        [InlineData("http://www.test.com/", "http://www.test.com/about?$filter=a eq b&$orderby=c")]
        [InlineData("http://www.test.com/testmobileapp/", "http://www.test.com/testmobileapp/about?$filter=a eq b&$orderby=c")]
        [InlineData("http://www.test.com/testmobileapp", "http://www.test.com/testmobileapp/about?$filter=a eq b&$orderby=c")]
        public async Task ReadAsync_WithAbsoluteUri(string serviceUri, string queryUri)
        {
            var hijack = new TestHttpHandler();
            hijack.SetResponseContent("[{\"String\":\"Hey\"}]");
            IMobileServiceClient service = new MobileServiceClient(serviceUri, hijack);
            IMobileServiceTable table = service.GetTable("someTable");
            await table.ReadAsync(queryUri);

            Assert.Equal(queryUri, hijack.Request.RequestUri.ToString());
        }

        [Theory]
        [InlineData("http://www.test.com", "/about?$filter=a eq b&$orderby=c", "http://www.test.com/about?$filter=a eq b&$orderby=c")]
        [InlineData("http://www.test.com/", "/about?$filter=a eq b&$orderby=c", "http://www.test.com/about?$filter=a eq b&$orderby=c")]
        [InlineData("http://www.test.com/testmobileapp/", "/about?$filter=a eq b&$orderby=c", "http://www.test.com/testmobileapp/about?$filter=a eq b&$orderby=c")]
        [InlineData("http://www.test.com/testmobileapp", "/about?$filter=a eq b&$orderby=c", "http://www.test.com/testmobileapp/about?$filter=a eq b&$orderby=c")]
        public async Task ReadAsync_WithRelativeUri(string serviceUri, string queryUri, string requestUri)
        {
            var hijack = new TestHttpHandler();
            hijack.SetResponseContent("[{\"String\":\"Hey\"}]");
            IMobileServiceClient service = new MobileServiceClient(serviceUri, hijack);
            IMobileServiceTable table = service.GetTable("someTable");
            await table.ReadAsync(queryUri);

            Assert.Equal(requestUri, hijack.Request.RequestUri.ToString());
        }

        [Fact]
        public async Task ReadAsyncWithStringIdFilter()
        {
            string[] testIdData = IdTestData.ValidStringIds
                                    .Concat(IdTestData.EmptyStringIds)
                                    .Concat(IdTestData.InvalidStringIds)
                                    .ToArray();

            foreach (string testId in testIdData)
            {
                TestHttpHandler hijack = new TestHttpHandler();

                // Make the testId JSON safe
                string jsonTestId = testId.Replace("\\", "\\\\").Replace("\"", "\\\"");

                hijack.SetResponseContent("[{\"id\":\"" + jsonTestId + "\",\"String\":\"Hey\"}]");

                IMobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);
                MobileAppUriValidator mobileAppUriValidator = new MobileAppUriValidator(service);

                IMobileServiceTable table = service.GetTable("someTable");

                string idForOdataQuery = Uri.EscapeDataString(testId.Replace("'", "''"));
                JToken results = await table.ReadAsync(string.Format("$filter=id eq '{0}'", idForOdataQuery));
                JToken[] items = results.ToArray();
                JObject item0 = items[0] as JObject;

                Uri expectedUri = new Uri(string.Format(mobileAppUriValidator.GetTableUri("someTable?$filter=id eq '{0}'"), idForOdataQuery));

                Assert.Single(items);
                Assert.Equal(testId, (string)items[0]["id"]);
                Assert.Equal("Hey", (string)items[0]["String"]);
                Assert.Equal(hijack.Request.RequestUri.AbsoluteUri, expectedUri.AbsoluteUri);
            }
        }

        [Fact]
        public async Task ReadAsyncWithNullIdFilter()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            hijack.SetResponseContent("[{\"id\":null,\"String\":\"Hey\"}]");

            IMobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);
            MobileAppUriValidator mobileAppUriValidator = new MobileAppUriValidator(service);

            IMobileServiceTable table = service.GetTable("someTable");

            JToken results = await table.ReadAsync("$filter=id eq null");
            JToken[] items = results.ToArray();
            JObject item0 = items[0] as JObject;

            Uri expectedUri = new Uri(mobileAppUriValidator.GetTableUri("someTable?$filter=id eq null"));

            Assert.Single(items);
            Assert.Null((string)items[0]["id"]);
            Assert.Equal("Hey", (string)items[0]["String"]);
            Assert.Equal(hijack.Request.RequestUri.AbsoluteUri, expectedUri.AbsoluteUri);
        }

        [Fact]
        public async Task ReadAsyncWithUserParameters()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            hijack.SetResponseContent("{\"Count\":1, People: [{\"Id\":\"12\", \"String\":\"Hey\"}] }");
            IMobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);

            var userDefinedParameters = new Dictionary<string, string>() { { "tags", "#pizza #beer" } };

            IMobileServiceTable table = service.GetTable("tests");

            JToken people = await table.ReadAsync("$filter=id eq 12", userDefinedParameters);

            Assert.Contains("tests", hijack.Request.RequestUri.ToString());
            Assert.Contains("tags=%23pizza%20%23beer", hijack.Request.RequestUri.AbsoluteUri);
            Assert.Contains("$filter=id eq 12", hijack.Request.RequestUri.ToString());

            Assert.Equal(1, (int)people["Count"]);
            Assert.Equal(12, (int)people["People"][0]["Id"]);
            Assert.Equal("Hey", (string)people["People"][0]["String"]);
        }

        [Fact]
        public async Task ReadAsyncThrowsWithInvalidUserParameters()
        {
            var invalidUserDefinedParameters = new Dictionary<string, string>() { { "$this is invalid", "since it starts with a '$'" } };
            MobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp);
            IMobileServiceTable table = service.GetTable("test");

            await Assert.ThrowsAsync<ArgumentException>(() => table.ReadAsync("$filter=id eq 12", invalidUserDefinedParameters));
        }

        [Fact]
        public async Task LookupAsyncWithStringIdResponseContent()
        {
            string[] testIdData = IdTestData.ValidStringIds
                                    .Concat(IdTestData.EmptyStringIds)
                                    .Concat(IdTestData.InvalidStringIds)
                                    .ToArray();

            foreach (string testId in testIdData)
            {
                TestHttpHandler hijack = new TestHttpHandler();

                // Make the testId JSON safe
                string jsonTestId = testId.Replace("\\", "\\\\").Replace("\"", "\\\"");

                hijack.SetResponseContent("{\"id\":\"" + jsonTestId + "\",\"String\":\"Hey\"}");
                IMobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);

                IMobileServiceTable table = service.GetTable("someTable");

                JObject item = await table.LookupAsync("id") as JObject;

                Assert.Equal(testId, (string)item["id"]);
                Assert.Equal("Hey", (string)item["String"]);
            }
        }

        [Fact]
        public async Task LookupAsyncWithIntIdResponseContent()
        {
            long[] testIdData = IdTestData.ValidIntIds
                                    .Concat(IdTestData.InvalidIntIds)
                                    .ToArray();

            foreach (long testId in testIdData)
            {
                string stringTestId = testId.ToString();

                TestHttpHandler hijack = new TestHttpHandler();
                hijack.SetResponseContent("{\"id\":" + stringTestId + ",\"String\":\"Hey\"}");
                IMobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);

                IMobileServiceTable table = service.GetTable("someTable");

                JObject item = await table.LookupAsync("id") as JObject;

                Assert.Equal(testId, (long)item["id"]);
                Assert.Equal("Hey", (string)item["String"]);
            }
        }

        [Fact]
        public async Task LookupAsyncWithNonStringAndNonIntIdResponseContent()
        {
            object[] testIdData = IdTestData.NonStringNonIntValidJsonIds;

            foreach (object testId in testIdData)
            {
                string stringTestId = testId.ToString().ToLower();

                TestHttpHandler hijack = new TestHttpHandler();
                hijack.SetResponseContent("{\"id\":" + stringTestId + ",\"String\":\"Hey\"}");
                IMobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);

                IMobileServiceTable table = service.GetTable("someTable");

                JObject item = await table.LookupAsync("id") as JObject;

                object value = item["id"].ToObject(testId.GetType());

                Assert.Equal(testId, value);
                Assert.Equal("Hey", (string)item["String"]);
            }
        }

        [Fact]
        public async Task LookupAsyncWithNullIdResponseContent()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            hijack.SetResponseContent("{\"id\":null,\"String\":\"Hey\"}");
            IMobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);

            IMobileServiceTable table = service.GetTable("someTable");

            JObject item = await table.LookupAsync("id") as JObject;

            Assert.Null((string)item["id"]);
            Assert.Equal("Hey", (string)item["String"]);
        }

        [Fact]
        public async Task LookupAsyncWithNoIdResponseContent()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            hijack.SetResponseContent("{\"String\":\"Hey\"}");
            IMobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);

            IMobileServiceTable table = service.GetTable("someTable");

            JObject item = await table.LookupAsync("id") as JObject;

            Assert.DoesNotContain(item.Properties(), p => string.Equals(p.Name, "id", StringComparison.InvariantCultureIgnoreCase));
            Assert.Equal("Hey", (string)item["String"]);
        }

        [Fact]
        public async Task LookupAsyncWithStringIdParameter()
        {
            string[] testIdData = IdTestData.ValidStringIds;

            foreach (string testId in testIdData)
            {
                TestHttpHandler hijack = new TestHttpHandler();
                hijack.SetResponseContent("{\"id\":\"" + testId + "\",\"String\":\"Hey\"}");
                IMobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);
                MobileAppUriValidator mobileAppUriValidator = new MobileAppUriValidator(service);

                IMobileServiceTable table = service.GetTable("someTable");

                JToken item = await table.LookupAsync(testId);

                Uri expectedUri = new Uri(string.Format(mobileAppUriValidator.GetTableUri("someTable/{0}"), Uri.EscapeDataString(testId)));

                Assert.Equal(testId, (string)item["id"]);
                Assert.Equal("Hey", (string)item["String"]);
                Assert.Equal(hijack.Request.RequestUri.AbsoluteUri, expectedUri.AbsoluteUri);
            }
        }

        [Fact]
        public async Task LookupAsyncWithInvalidStringIdParameter()
        {
            string[] testIdData = IdTestData.EmptyStringIds.Concat(
                                  IdTestData.InvalidStringIds).ToArray();

            foreach (string testId in testIdData)
            {
                TestHttpHandler hijack = new TestHttpHandler();
                hijack.SetResponseContent("{\"id\":\"" + testId + "\",\"String\":\"Hey\"}");
                IMobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);

                IMobileServiceTable table = service.GetTable("someTable");
                Exception exception = await Assert.ThrowsAnyAsync<Exception>(async () => await table.LookupAsync(testId));

                Assert.True(exception.Message.Contains("The id can not be null or an empty string.") ||
                              exception.Message.Contains("An id must not contain any control characters or the characters") ||
                              exception.Message.Contains("is longer than the max string id length of 255 characters"));
            }
        }

        [Fact]
        public async Task LookupAsyncWithIntIdParameter()
        {
            long[] testIdData = IdTestData.ValidIntIds;

            foreach (long testId in testIdData)
            {
                TestHttpHandler hijack = new TestHttpHandler();
                hijack.SetResponseContent("{\"id\":" + testId.ToString() + ",\"String\":\"Hey\"}");
                IMobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);
                MobileAppUriValidator mobileAppUriValidator = new MobileAppUriValidator(service);

                IMobileServiceTable table = service.GetTable("someTable");

                JToken item = await table.LookupAsync(testId);

                Uri expectedUri = new Uri(string.Format(mobileAppUriValidator.GetTableUri("someTable/{0}"), testId));

                Assert.Equal(testId, (long)item["id"]);
                Assert.Equal("Hey", (string)item["String"]);
                Assert.Equal(hijack.Request.RequestUri.AbsoluteUri, expectedUri.AbsoluteUri);
            }
        }

        [Fact]
        public async Task LookupAsyncWithNullIdParameter()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            hijack.SetResponseContent("{\"id\":null,\"String\":\"Hey\"}");
            IMobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);

            IMobileServiceTable table = service.GetTable("someTable");
            Exception exception = await Assert.ThrowsAnyAsync<Exception>(async () => await table.LookupAsync(null));
            Assert.Contains("The id can not be null or an empty string.", exception.Message);
        }

        [Fact]
        public async Task LookupAsyncWithZeroIdParameter()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            hijack.SetResponseContent("{\"id\":null,\"String\":\"Hey\"}");
            IMobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);

            IMobileServiceTable table = service.GetTable("someTable");
            Exception exception = await Assert.ThrowsAnyAsync<Exception>(async () => await table.LookupAsync(0L));
            Assert.True(exception.Message.Contains("Specified argument was out of the range of valid values") || exception.Message.Contains(" is not a positive integer value"));
        }

        [Fact]
        public async Task LookupAsyncWithUserParameters()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            hijack.SetResponseContent("{\"Count\":1, People: [{\"Id\":\"12\", \"String\":\"Hey\"}] }");
            IMobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);

            var userDefinedParameters = new Dictionary<string, string>() { { "tags", "#pizza #beer" } };

            IMobileServiceTable table = service.GetTable("tests");

            JToken people = await table.LookupAsync("id ", userDefinedParameters);

            Assert.Contains("tests", hijack.Request.RequestUri.ToString());
            Assert.Contains("tags=%23pizza%20%23beer", hijack.Request.RequestUri.AbsoluteUri);

            Assert.Equal(1, (int)people["Count"]);
            Assert.Equal(12, (int)people["People"][0]["Id"]);
            Assert.Equal("Hey", (string)people["People"][0]["String"]);
        }

        [Fact]
        public async Task LookupAsyncThrowsWithInvalidUserParameters()
        {
            var invalidUserDefinedParameters = new Dictionary<string, string>() { { "$this is invalid", "since it starts with a '$'" } };
            MobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp);
            IMobileServiceTable table = service.GetTable("test");
            await Assert.ThrowsAsync<ArgumentException>(() => table.LookupAsync("$filter=id eq 12", invalidUserDefinedParameters));
        }

        [Fact]
        public async Task InsertAsyncWithStringIdResponseContent()
        {
            string[] testIdData = IdTestData.ValidStringIds
                                    .Concat(IdTestData.EmptyStringIds)
                                    .Concat(IdTestData.InvalidStringIds)
                                    .ToArray();

            foreach (string testId in testIdData)
            {
                TestHttpHandler hijack = new TestHttpHandler();

                // Make the testId JSON safe
                string jsonTestId = testId.Replace("\\", "\\\\").Replace("\"", "\\\"");

                hijack.SetResponseContent("{\"id\":\"" + jsonTestId + "\",\"String\":\"Hey\"}");
                IMobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);

                IMobileServiceTable table = service.GetTable("someTable");

                JObject obj = JToken.Parse("{\"value\":\"new\"}") as JObject;
                JObject item = await table.InsertAsync(obj) as JObject;

                Assert.Equal(testId, (string)item["id"]);
                Assert.Equal("Hey", (string)item["String"]);
            }
        }

        [Fact]
        public async Task InsertAsyncWithIntIdResponseContent()
        {
            long[] testIdData = IdTestData.ValidIntIds.Concat(IdTestData.InvalidIntIds).ToArray();

            foreach (long testId in testIdData)
            {
                string stringTestId = testId.ToString();

                TestHttpHandler hijack = new TestHttpHandler();
                hijack.SetResponseContent("{\"id\":" + stringTestId + ",\"String\":\"Hey\"}");
                IMobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);

                IMobileServiceTable table = service.GetTable("someTable");

                JObject obj = JToken.Parse("{\"value\":\"new\"}") as JObject;
                JObject item = await table.InsertAsync(obj) as JObject;

                Assert.Equal(testId, (long)item["id"]);
                Assert.Equal("Hey", (string)item["String"]);
            }
        }

        [Fact]
        public async Task InsertAsyncWithNonStringAndNonIntIdResponseContent()
        {
            object[] testIdData = IdTestData.NonStringNonIntValidJsonIds;

            foreach (object testId in testIdData)
            {
                string stringTestId = testId.ToString().ToLower();

                TestHttpHandler hijack = new TestHttpHandler();
                hijack.SetResponseContent("{\"id\":" + stringTestId + ",\"String\":\"Hey\"}");
                IMobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);

                IMobileServiceTable table = service.GetTable("someTable");

                JObject obj = JToken.Parse("{\"value\":\"new\"}") as JObject;
                JObject item = await table.InsertAsync(obj) as JObject;

                object value = item["id"].ToObject(testId.GetType());

                Assert.Equal(testId, value);
                Assert.Equal("Hey", (string)item["String"]);
            }
        }

        [Fact]
        public async Task InsertAsyncWithNullIdResponseContent()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            hijack.SetResponseContent("{\"id\":null,\"String\":\"Hey\"}");
            IMobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);

            IMobileServiceTable table = service.GetTable("someTable");

            JObject obj = JToken.Parse("{\"value\":\"new\"}") as JObject;
            JObject item = await table.InsertAsync(obj) as JObject;

            Assert.Null((string)item["id"]);
            Assert.Equal("Hey", (string)item["String"]);
        }

        [Fact]
        public async Task InsertAsyncWithNoIdResponseContent()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            hijack.SetResponseContent("{\"String\":\"Hey\"}");
            IMobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);

            IMobileServiceTable table = service.GetTable("someTable");

            JObject obj = JToken.Parse("{\"value\":\"new\"}") as JObject;
            JObject item = await table.InsertAsync(obj) as JObject;

            Assert.DoesNotContain(item.Properties(), p => string.Equals(p.Name, "id", StringComparison.InvariantCultureIgnoreCase));
            Assert.Equal("Hey", (string)item["String"]);
        }

        [Fact]
        public async Task InsertAsyncWithStringIdItem()
        {
            string[] testIdData = IdTestData.ValidStringIds;

            foreach (string testId in testIdData)
            {
                TestHttpHandler hijack = new TestHttpHandler();
                hijack.SetResponseContent("{\"id\":\"" + testId + "\",\"String\":\"Hey\"}");
                IMobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);

                IMobileServiceTable table = service.GetTable("someTable");

                JObject obj = JToken.Parse("{\"id\":\"" + testId + "\",\"String\":\"what?\"}") as JObject;
                JToken item = await table.InsertAsync(obj);

                Assert.Equal(testId, (string)item["id"]);
                Assert.Equal("Hey", (string)item["String"]);
            }
        }

        [Fact]
        public async Task InsertAsyncWithEmptyStringIdItem()
        {
            string[] testIdData = IdTestData.EmptyStringIds;

            foreach (string testId in testIdData)
            {
                TestHttpHandler hijack = new TestHttpHandler();
                hijack.SetResponseContent("{\"id\":\"an id\",\"String\":\"Hey\"}");
                IMobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);

                IMobileServiceTable table = service.GetTable("someTable");

                JObject obj = JToken.Parse("{\"id\":\"" + testId + "\",\"String\":\"what?\"}") as JObject;
                JToken item = await table.InsertAsync(obj);

                Assert.Equal("an id", (string)item["id"]);
                Assert.Equal("Hey", (string)item["String"]);
            }
        }

        [Fact]
        public async Task InsertAsyncWithInvalidStringIdItem()
        {
            string[] testIdData = IdTestData.InvalidStringIds;

            foreach (string testId in testIdData)
            {
                TestHttpHandler hijack = new TestHttpHandler();

                // Make the testId JSON safe
                string jsonTestId = testId.Replace("\\", "\\\\").Replace("\"", "\\\"");

                hijack.SetResponseContent("{\"id\":\"" + jsonTestId + "\",\"String\":\"Hey\"}");
                IMobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);

                IMobileServiceTable table = service.GetTable("someTable");
                Exception exception = await Assert.ThrowsAnyAsync<Exception>(async () => 
                {
                    JObject obj = JToken.Parse("{\"id\":\"" + jsonTestId + "\",\"String\":\"what?\"}") as JObject;
                    JToken item = await table.InsertAsync(obj);
                });
                Assert.True(exception.Message.Contains("is longer than the max string id length of 255 characters") ||
                             exception.Message.Contains("An id must not contain any control characters or the characters"));
            }
        }

        [Fact]
        public async Task InsertAsyncWithIntIdItem()
        {
            long[] testIdData = IdTestData.ValidIntIds;

            foreach (long testId in testIdData)
            {
                TestHttpHandler hijack = new TestHttpHandler();
                hijack.SetResponseContent("{\"id\":" + testId.ToString() + ",\"String\":\"Hey\"}");
                IMobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);

                IMobileServiceTable table = service.GetTable("someTable");
                Exception exception = await Assert.ThrowsAnyAsync<Exception>(async () =>
                {
                    JObject obj = JToken.Parse("{\"id\":" + testId.ToString() + ",\"String\":\"what?\"}") as JObject;
                    JToken item = await table.InsertAsync(obj);
                });

                Assert.True(exception.Message.Contains("Cannot insert if the id member is already set.") ||
                              exception.Message.Contains("for member id is outside the valid range for numeric columns"));
            }
        }

        [Fact]
        public async Task InsertAsyncWithNullIdItem()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            hijack.SetResponseContent("{\"id\":5,\"String\":\"Hey\"}");
            IMobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);

            IMobileServiceTable table = service.GetTable("someTable");

            JObject obj = JToken.Parse("{\"id\":null,\"String\":\"what?\"}") as JObject;
            JToken item = await table.InsertAsync(obj);

            Assert.Equal(5L, (long)item["id"]);
            Assert.Equal("Hey", (string)item["String"]);
        }

        [Fact]
        public async Task InsertAsyncWithNoIdItem()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            hijack.SetResponseContent("{\"id\":5,\"String\":\"Hey\"}");
            IMobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);

            IMobileServiceTable table = service.GetTable("someTable");

            JObject obj = JToken.Parse("{\"String\":\"what?\"}") as JObject;
            JToken item = await table.InsertAsync(obj);

            Assert.Equal(5L, (long)item["id"]);
            Assert.Equal("Hey", (string)item["String"]);
        }

        [Fact]
        public async Task InsertAsyncWithZeroIdItem()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            hijack.SetResponseContent("{\"id\":5,\"String\":\"Hey\"}");
            IMobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);

            IMobileServiceTable table = service.GetTable("someTable");

            JObject obj = JToken.Parse("{\"id\":0,\"String\":\"what?\"}") as JObject;
            JToken item = await table.InsertAsync(obj);

            Assert.Equal(5L, (long)item["id"]);
            Assert.Equal("Hey", (string)item["String"]);
        }

        [Fact]
        public async Task InsertAsyncWithUserParameters()
        {
            var userDefinedParameters = new Dictionary<string, string>() { { "state", "AL" } };

            TestHttpHandler hijack = new TestHttpHandler();
            IMobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);
            IMobileServiceTable table = service.GetTable("tests");

            JObject obj = JToken.Parse("{\"value\":\"new\"}") as JObject;
            hijack.SetResponseContent("{\"id\":12,\"value\":\"new\"}");
            JToken newObj = await table.InsertAsync(obj, userDefinedParameters);

            Assert.Equal(12, (int)newObj["id"]);
            Assert.NotEqual(newObj, obj);
            Assert.Contains("tests", hijack.Request.RequestUri.ToString());
            Assert.Contains("state=AL", hijack.Request.RequestUri.Query);
        }

        [Theory]
        [InlineData("ID")]
        [InlineData("Id")]
        [InlineData("iD")]
        public async Task InsertAsyncThrowsWhenIdIsWrongCase(string idField)
        {
            MobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp);
            IMobileServiceTable table = service.GetTable("tests");
            JObject obj = JToken.Parse($"{{\"{idField}\":\"an id\"}}") as JObject;
            ArgumentException expected = await Assert.ThrowsAsync<ArgumentException>(() => table.InsertAsync(obj));
            Assert.Contains("The casing of the 'id' property is invalid.", expected.Message);
        }

        [Fact]
        public async Task InsertAsyncWithStringId_KeepsSystemProperties()
        {
            var userDefinedParameters = new Dictionary<string, string>() { { "__systemproperties", "createdAt" } };

            TestHttpHandler hijack = new TestHttpHandler();
            IMobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);
            IMobileServiceTable table = service.GetTable("tests");

            JObject obj = JToken.Parse("{\"value\":\"new\"}") as JObject;
            hijack.SetResponseContent("{\"id\":\"A\",\"value\":\"new\", \"version\":\"XYZ\",\"__unknown\":12,\"CREATEDat\":\"12-02-02\"}");
            JToken newObj = await table.InsertAsync(obj);

            Assert.Equal("A", (string)newObj["id"]);
            Assert.NotEqual(newObj, obj);
            Assert.NotNull(newObj["version"]);
            Assert.NotNull(newObj["__unknown"]);
            Assert.NotNull(newObj["CREATEDat"]);
        }

        [Fact]
        public async Task InsertAsyncWithIntId_DoesNotStripSystemProperties()
        {
            var userDefinedParameters = new Dictionary<string, string>() { { "state", "AL" } };

            TestHttpHandler hijack = new TestHttpHandler();
            IMobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);
            IMobileServiceTable table = service.GetTable("tests");

            JObject obj = JToken.Parse("{\"value\":\"new\"}") as JObject;
            hijack.SetResponseContent("{\"id\":12,\"value\":\"new\", \"version\":\"XYZ\",\"__unknown\":12,\"CREATEDat\":\"12-02-02\"}");
            JToken newObj = await table.InsertAsync(obj, userDefinedParameters);

            Assert.Equal(12, (int)newObj["id"]);
            Assert.NotEqual(newObj, obj);
            Assert.NotNull(newObj["version"]);
            Assert.NotNull(newObj["__unknown"]);
            Assert.NotNull(newObj["CREATEDat"]);
        }

        [Fact]
        public async Task UpdateAsyncWithStringIdResponseContent()
        {
            string[] testIdData = IdTestData.ValidStringIds.Concat(
                                  IdTestData.EmptyStringIds).Concat(
                                  IdTestData.InvalidStringIds).ToArray();

            foreach (string testId in testIdData)
            {
                TestHttpHandler hijack = new TestHttpHandler();

                // Make the testId JSON safe
                string jsonTestId = testId.Replace("\\", "\\\\").Replace("\"", "\\\"");

                hijack.SetResponseContent("{\"id\":\"" + jsonTestId + "\",\"String\":\"Hey\"}");
                IMobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);

                IMobileServiceTable table = service.GetTable("someTable");

                JObject obj = JToken.Parse("{\"id\":\"id\",\"value\":\"new\"}") as JObject;
                JObject item = await table.UpdateAsync(obj) as JObject;

                Assert.Equal(testId, (string)item["id"]);
                Assert.Equal("Hey", (string)item["String"]);
            }
        }

        [Fact]
        public async Task UpdateAsyncWithIntIdResponseContent()
        {
            long[] testIdData = IdTestData.ValidIntIds.Concat(
                                 IdTestData.InvalidIntIds).ToArray();

            foreach (long testId in testIdData)
            {
                string stringTestId = testId.ToString();

                TestHttpHandler hijack = new TestHttpHandler();
                hijack.SetResponseContent("{\"id\":" + stringTestId + ",\"String\":\"Hey\"}");
                IMobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);

                IMobileServiceTable table = service.GetTable("someTable");

                JObject obj = JToken.Parse("{\"id\":\"id\",\"value\":\"new\"}") as JObject;
                JObject item = await table.UpdateAsync(obj) as JObject;

                Assert.Equal(testId, (long)item["id"]);
                Assert.Equal("Hey", (string)item["String"]);
            }
        }

        [Fact]
        public async Task UpdateAsyncWithNonStringAndNonIntIdResponseContent()
        {
            object[] testIdData = IdTestData.NonStringNonIntValidJsonIds;

            foreach (object testId in testIdData)
            {
                string stringTestId = testId.ToString().ToLower();

                TestHttpHandler hijack = new TestHttpHandler();
                hijack.SetResponseContent("{\"id\":" + stringTestId + ",\"String\":\"Hey\"}");
                IMobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);

                IMobileServiceTable table = service.GetTable("someTable");

                JObject obj = JToken.Parse("{\"id\":\"id\",\"value\":\"new\"}") as JObject;
                JObject item = await table.UpdateAsync(obj) as JObject;

                object value = item["id"].ToObject(testId.GetType());

                Assert.Equal(testId, value);
                Assert.Equal("Hey", (string)item["String"]);
            }
        }

        [Fact]
        public async Task UpdateAsyncWithNullIdResponseContent()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            hijack.SetResponseContent("{\"id\":null,\"String\":\"Hey\"}");
            IMobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);

            IMobileServiceTable table = service.GetTable("someTable");

            JObject obj = JToken.Parse("{\"id\":\"id\",\"value\":\"new\"}") as JObject;
            JObject item = await table.UpdateAsync(obj) as JObject;

            Assert.Null((string)item["id"]);
            Assert.Equal("Hey", (string)item["String"]);
        }

        [Fact]
        public async Task UpdateAsyncWithNoIdResponseContent()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            hijack.SetResponseContent("{\"String\":\"Hey\"}");
            IMobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);

            IMobileServiceTable table = service.GetTable("someTable");

            JObject obj = JToken.Parse("{\"id\":\"id\",\"value\":\"new\"}") as JObject;
            JObject item = await table.UpdateAsync(obj) as JObject;

            Assert.DoesNotContain(item.Properties(), p => string.Equals(p.Name, "id", StringComparison.InvariantCultureIgnoreCase));
            Assert.Equal("Hey", (string)item["String"]);
        }

        [Fact]
        public async Task UpdateAsyncWithStringIdItem()
        {
            string[] testIdData = IdTestData.ValidStringIds;

            foreach (string testId in testIdData)
            {
                TestHttpHandler hijack = new TestHttpHandler();
                hijack.SetResponseContent("{\"id\":\"" + testId + "\",\"String\":\"Hey\"}");
                IMobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);

                IMobileServiceTable table = service.GetTable("someTable");

                JObject obj = JToken.Parse("{\"id\":\"" + testId + "\",\"String\":\"what?\"}") as JObject;
                JToken item = await table.UpdateAsync(obj);

                Assert.Equal(testId, (string)item["id"]);
                Assert.Equal("Hey", (string)item["String"]);
            }
        }

        [Fact]
        public async Task UpdateAsyncWithEmptyStringIdItem()
        {
            string[] testIdData = IdTestData.EmptyStringIds;

            foreach (string testId in testIdData)
            {
                TestHttpHandler hijack = new TestHttpHandler();
                hijack.SetResponseContent("{\"id\":\"an id\",\"String\":\"Hey\"}");
                IMobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);

                IMobileServiceTable table = service.GetTable("someTable");
                Exception exception = await Assert.ThrowsAnyAsync<Exception>(async () =>
                {
                    JObject obj = JToken.Parse("{\"id\":\"" + testId + "\",\"String\":\"what?\"}") as JObject;
                    JToken item = await table.UpdateAsync(obj);
                });
                Assert.Contains("The id can not be null or an empty string.", exception.Message);
            }
        }

        [Fact]
        public async Task UpdateAsyncWithInvalidStringIdItem()
        {
            string[] testIdData = IdTestData.InvalidStringIds;

            foreach (string testId in testIdData)
            {
                TestHttpHandler hijack = new TestHttpHandler();

                // Make the testId JSON safe
                string jsonTestId = testId.Replace("\\", "\\\\").Replace("\"", "\\\"");

                hijack.SetResponseContent("{\"id\":\"" + jsonTestId + "\",\"String\":\"Hey\"}");
                IMobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);

                IMobileServiceTable table = service.GetTable("someTable");
                Exception exception = await Assert.ThrowsAnyAsync<Exception>(async () =>
                {
                    JObject obj = JToken.Parse("{\"id\":\"" + jsonTestId + "\",\"String\":\"what?\"}") as JObject;
                    JToken item = await table.UpdateAsync(obj);
                });
                Assert.True(exception.Message.Contains("is longer than the max string id length of 255 characters") ||
                              exception.Message.Contains("An id must not contain any control characters or the characters"));
            }
        }

        [Fact]
        public async Task UpdateAsyncWithIntIdItem()
        {
            long[] testIdData = IdTestData.ValidIntIds;

            foreach (long testId in testIdData)
            {
                TestHttpHandler hijack = new TestHttpHandler();
                hijack.SetResponseContent("{\"id\":" + testId.ToString() + ",\"String\":\"Hey\"}");
                IMobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);

                IMobileServiceTable table = service.GetTable("someTable");
                JObject obj = JToken.Parse("{\"id\":" + testId.ToString() + ",\"String\":\"what?\"}") as JObject;
                JToken item = await table.UpdateAsync(obj);

                Assert.Equal(testId, (long)item["id"]);
                Assert.Equal("Hey", (string)item["String"]);
            }
        }

        [Fact]
        public async Task UpdateAsyncWithNullIdItem()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            hijack.SetResponseContent("{\"id\":5,\"String\":\"Hey\"}");
            IMobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);

            IMobileServiceTable table = service.GetTable("someTable");
            Exception exception = await Assert.ThrowsAnyAsync<Exception>(async () =>
            {
                JObject obj = JToken.Parse("{\"id\":null,\"String\":\"what?\"}") as JObject;
                JToken item = await table.UpdateAsync(obj);
            });
            Assert.Contains("The id can not be null or an empty string", exception.Message);
        }

        [Fact]
        public async Task UpdateAsyncWithNoIdItem()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            hijack.SetResponseContent("{\"id\":5,\"String\":\"Hey\"}");
            IMobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);

            IMobileServiceTable table = service.GetTable("someTable");
            Exception exception = await Assert.ThrowsAnyAsync<Exception>(async () =>
            {
                JObject obj = JToken.Parse("{\"String\":\"what?\"}") as JObject;
                JToken item = await table.UpdateAsync(obj);
            });
            Assert.Contains("Expected id member not found.", exception.Message);
        }

        [Fact]
        public async Task UpdateAsyncWithZeroIdItem()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            hijack.SetResponseContent("{\"id\":5,\"String\":\"Hey\"}");
            IMobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);

            IMobileServiceTable table = service.GetTable("someTable");
            Exception exception = await Assert.ThrowsAnyAsync<Exception>(async () =>
            {
                JObject obj = JToken.Parse("{\"id\":0,\"String\":\"what?\"}") as JObject;
                JToken item = await table.UpdateAsync(obj);
            });
            Assert.Contains("The integer id '0' is not a positive integer value.", exception.Message);
        }

        [Fact]
        public async Task UpdateAsyncWithUserParameters()
        {
            var userDefinedParameters = new Dictionary<string, string>() { { "state", "FL" } };

            TestHttpHandler hijack = new TestHttpHandler();
            IMobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);
            IMobileServiceTable table = service.GetTable("tests");

            JObject obj = JToken.Parse("{\"id\":12,\"value\":\"new\"}") as JObject;
            hijack.SetResponseContent("{\"id\":12,\"value\":\"new\",\"other\":\"123\"}");
            JToken newObj = await table.UpdateAsync(obj, userDefinedParameters);

            Assert.Equal("123", (string)newObj["other"]);
            Assert.NotEqual(newObj, obj);
            Assert.Contains("tests/12", hijack.Request.RequestUri.ToString());
            Assert.Contains("state=FL", hijack.Request.RequestUri.Query);
        }

        [Theory]
        [InlineData("ID")]
        [InlineData("Id")]
        [InlineData("iD")]
        public async Task UpdateAsyncThrowsWhenIdIsWrongCase(string idField)
        {
            MobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp);
            IMobileServiceTable table = service.GetTable("tests");
            JObject obj = JToken.Parse($"{{\"{idField}\":5}}") as JObject;
            ArgumentException expected = await Assert.ThrowsAsync<ArgumentException>(() => table.UpdateAsync(obj));
            Assert.Contains("The casing of the 'id' property is invalid.", expected.Message);
        }

        [Fact]
        public async Task UpdateAsyncWithStringId_KeepsSystemProperties()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            IMobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);
            IMobileServiceTable table = service.GetTable("tests");

            JObject obj = JToken.Parse("{\"id\":\"A\",\"value\":\"new\"}") as JObject;
            hijack.SetResponseContent("{\"id\":\"A\",\"value\":\"new\", \"version\":\"XYZ\",\"__unknown\":12,\"CREATEDat\":\"12-02-02\"}");
            var newObj = await table.UpdateAsync(obj);

            Assert.Equal("A", (string)newObj["id"]);
            Assert.NotEqual(newObj, obj);
            Assert.NotNull(newObj["version"]);
            Assert.NotNull(newObj["__unknown"]);
            Assert.NotNull(newObj["CREATEDat"]);
        }

        [Fact]
        public async Task UpdateAsyncWithIntId_DoesNotStripSystemProperties()
        {
            var userDefinedParameters = new Dictionary<string, string>() { { "state", "AL" } };

            TestHttpHandler hijack = new TestHttpHandler();
            IMobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);
            IMobileServiceTable table = service.GetTable("tests");

            JObject obj = JToken.Parse("{\"id\":12,\"value\":\"new\"}") as JObject;
            hijack.SetResponseContent("{\"id\":12,\"value\":\"new\", \"version\":\"XYZ\",\"__unknown\":12,\"CREATEDat\":\"12-02-02\"}");
            JToken newObj = await table.UpdateAsync(obj, userDefinedParameters);

            Assert.Equal(12, (int)newObj["id"]);
            Assert.NotEqual(newObj, obj);
            Assert.NotNull(newObj["version"]);
            Assert.NotNull(newObj["__unknown"]);
            Assert.NotNull(newObj["CREATEDat"]);
        }

        [Fact]
        public async Task DeleteAsyncWithStringIdResponseContent()
        {
            string[] testIdData = IdTestData.ValidStringIds.Concat(
                                  IdTestData.EmptyStringIds).Concat(
                                  IdTestData.InvalidStringIds).ToArray();

            foreach (string testId in testIdData)
            {
                TestHttpHandler hijack = new TestHttpHandler();

                // Make the testId JSON safe
                string jsonTestId = testId.Replace("\\", "\\\\").Replace("\"", "\\\"");

                hijack.SetResponseContent("{\"id\":\"" + jsonTestId + "\",\"String\":\"Hey\"}");
                IMobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);

                IMobileServiceTable table = service.GetTable("someTable");

                JObject obj = JToken.Parse("{\"id\":\"id\",\"value\":\"new\"}") as JObject;
                JObject item = await table.DeleteAsync(obj) as JObject;

                Assert.Equal(testId, (string)item["id"]);
                Assert.Equal("Hey", (string)item["String"]);
            }
        }

        [Fact]
        public async Task DeleteAsyncWithIntIdResponseContent()
        {
            long[] testIdData = IdTestData.ValidIntIds.Concat(
                                 IdTestData.InvalidIntIds).ToArray();

            foreach (long testId in testIdData)
            {
                string stringTestId = testId.ToString();

                TestHttpHandler hijack = new TestHttpHandler();
                hijack.SetResponseContent("{\"id\":" + stringTestId + ",\"String\":\"Hey\"}");
                IMobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);

                IMobileServiceTable table = service.GetTable("someTable");

                JObject obj = JToken.Parse("{\"id\":\"id\",\"value\":\"new\"}") as JObject;
                JObject item = await table.DeleteAsync(obj) as JObject;

                Assert.Equal(testId, (long)item["id"]);
                Assert.Equal("Hey", (string)item["String"]);
            }
        }

        [Fact]
        public async Task DeleteAsyncWithNonStringAndNonIntIdResponseContent()
        {
            object[] testIdData = IdTestData.NonStringNonIntValidJsonIds;

            foreach (object testId in testIdData)
            {
                string stringTestId = testId.ToString().ToLower();

                TestHttpHandler hijack = new TestHttpHandler();
                hijack.SetResponseContent("{\"id\":" + stringTestId + ",\"String\":\"Hey\"}");
                IMobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);

                IMobileServiceTable table = service.GetTable("someTable");

                JObject obj = JToken.Parse("{\"id\":\"id\",\"value\":\"new\"}") as JObject;
                JObject item = await table.DeleteAsync(obj) as JObject;

                object value = item["id"].ToObject(testId.GetType());

                Assert.Equal(testId, value);
                Assert.Equal("Hey", (string)item["String"]);
            }
        }

        [Fact]
        public async Task DeleteAsyncWithNullIdResponseContent()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            hijack.SetResponseContent("{\"id\":null,\"String\":\"Hey\"}");
            IMobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);

            IMobileServiceTable table = service.GetTable("someTable");

            JObject obj = JToken.Parse("{\"id\":\"id\",\"value\":\"new\"}") as JObject;
            JObject item = await table.DeleteAsync(obj) as JObject;

            Assert.Null((string)item["id"]);
            Assert.Equal("Hey", (string)item["String"]);
        }

        [Fact]
        public async Task DeleteAsyncWithNoIdResponseContent()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            hijack.SetResponseContent("{\"String\":\"Hey\"}");
            IMobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);

            IMobileServiceTable table = service.GetTable("someTable");

            JObject obj = JToken.Parse("{\"id\":\"id\",\"value\":\"new\"}") as JObject;
            JObject item = await table.DeleteAsync(obj) as JObject;

            Assert.DoesNotContain(item.Properties(), p => string.Equals(p.Name, "id", StringComparison.InvariantCultureIgnoreCase));
            Assert.Equal("Hey", (string)item["String"]);
        }

        [Fact]
        public async Task DeleteAsyncWithStringIdItem()
        {
            string[] testIdData = IdTestData.ValidStringIds;

            foreach (string testId in testIdData)
            {
                TestHttpHandler hijack = new TestHttpHandler();
                hijack.SetResponseContent("{\"id\":\"" + testId + "\",\"String\":\"Hey\"}");
                IMobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);

                IMobileServiceTable table = service.GetTable("someTable");

                JObject obj = JToken.Parse("{\"id\":\"" + testId + "\",\"String\":\"what?\"}") as JObject;
                JToken item = await table.DeleteAsync(obj);

                Assert.Equal(testId, (string)item["id"]);
                Assert.Equal("Hey", (string)item["String"]);
            }
        }

        [Fact]
        public async Task DeleteAsyncWithEmptyStringIdItem()
        {
            string[] testIdData = IdTestData.EmptyStringIds;

            foreach (string testId in testIdData)
            {
                TestHttpHandler hijack = new TestHttpHandler();
                hijack.SetResponseContent("{\"id\":\"an id\",\"String\":\"Hey\"}");
                IMobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);

                IMobileServiceTable table = service.GetTable("someTable");
                Exception exception = await Assert.ThrowsAnyAsync<Exception>(async () => 
                {
                    JObject obj = JToken.Parse("{\"id\":\"" + testId + "\",\"String\":\"what?\"}") as JObject;
                    JToken item = await table.DeleteAsync(obj);
                });
                Assert.Contains("The id can not be null or an empty string.", exception.Message);
            }
        }

        [Fact]
        public async Task DeleteAsyncWithInvalidStringIdItem()
        {
            string[] testIdData = IdTestData.InvalidStringIds;

            foreach (string testId in testIdData)
            {
                TestHttpHandler hijack = new TestHttpHandler();

                // Make the testId JSON safe
                string jsonTestId = testId.Replace("\\", "\\\\").Replace("\"", "\\\"");

                hijack.SetResponseContent("{\"id\":\"" + jsonTestId + "\",\"String\":\"Hey\"}");
                IMobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);

                IMobileServiceTable table = service.GetTable("someTable");
                Exception exception = await Assert.ThrowsAnyAsync<Exception>(async () => 
                {
                    JObject obj = JToken.Parse("{\"id\":\"" + jsonTestId + "\",\"String\":\"what?\"}") as JObject;
                    JToken item = await table.DeleteAsync(obj);
                });
                Assert.True(exception.Message.Contains("is longer than the max string id length of 255 characters") ||
                              exception.Message.Contains("An id must not contain any control characters or the characters"));
            }
        }

        [Fact]
        public async Task DeleteAsyncWithIntIdItem()
        {
            long[] testIdData = IdTestData.ValidIntIds;

            foreach (long testId in testIdData)
            {
                TestHttpHandler hijack = new TestHttpHandler();
                hijack.SetResponseContent("{\"id\":" + testId.ToString() + ",\"String\":\"Hey\"}");
                IMobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);

                IMobileServiceTable table = service.GetTable("someTable");
                JObject obj = JToken.Parse("{\"id\":" + testId.ToString() + ",\"String\":\"what?\"}") as JObject;
                JToken item = await table.DeleteAsync(obj);

                Assert.Equal(testId, (long)item["id"]);
                Assert.Equal("Hey", (string)item["String"]);
            }
        }

        [Fact]
        public async Task DeleteAsyncWithNullIdItem()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            hijack.SetResponseContent("{\"id\":5,\"String\":\"Hey\"}");
            IMobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);

            IMobileServiceTable table = service.GetTable("someTable");
            Exception exception = await Assert.ThrowsAnyAsync<Exception>(async () =>
            {
                JObject obj = JToken.Parse("{\"id\":null,\"String\":\"what?\"}") as JObject;
                JToken item = await table.DeleteAsync(obj);
            });
            Assert.Contains("The id can not be null or an empty string.", exception.Message);
        }

        [Fact]
        public async Task DeleteAsyncWithNoIdItem()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            hijack.SetResponseContent("{\"id\":5,\"String\":\"Hey\"}");
            IMobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);

            IMobileServiceTable table = service.GetTable("someTable");
            Exception exception = await Assert.ThrowsAnyAsync<Exception>(async () =>
            {
                JObject obj = JToken.Parse("{\"String\":\"what?\"}") as JObject;
                JToken item = await table.DeleteAsync(obj);
            });
            Assert.Contains("Expected id member not found.", exception.Message);
        }

        [Fact]
        public async Task DeleteAsyncWithZeroIdItem()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            hijack.SetResponseContent("{\"id\":5,\"String\":\"Hey\"}");
            IMobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);

            IMobileServiceTable table = service.GetTable("someTable");
            Exception exception = await Assert.ThrowsAnyAsync<Exception>(async () =>
            {
                JObject obj = JToken.Parse("{\"id\":0,\"String\":\"what?\"}") as JObject;
                JToken item = await table.DeleteAsync(obj);
            });
            Assert.Contains("The integer id '0' is not a positive integer value", exception.Message);
        }

        [Fact]
        public async Task DeleteAsyncWithUserParameters()
        {
            var userDefinedParameters = new Dictionary<string, string>() { { "state", "FL" } };

            TestHttpHandler hijack = new TestHttpHandler();
            IMobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);
            IMobileServiceTable table = service.GetTable("tests");

            JObject obj = JToken.Parse("{\"id\":12,\"value\":\"new\"}") as JObject;
            hijack.SetResponseContent("{\"id\":12,\"value\":\"new\",\"other\":\"123\"}");
            JToken newObj = await table.DeleteAsync(obj, userDefinedParameters);

            Assert.Equal("123", (string)newObj["other"]);
            Assert.NotEqual(newObj, obj);
            Assert.Contains("tests/12", hijack.Request.RequestUri.ToString());
            Assert.Contains("state=FL", hijack.Request.RequestUri.Query);
        }

        [Theory]
        [InlineData("ID")]
        [InlineData("Id")]
        [InlineData("iD")]
        public async Task DeleteAsyncThrowsWhenIdIsWrongCase(string idField)
        {
            MobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp);
            IMobileServiceTable table = service.GetTable("tests");
            JObject obj = JToken.Parse($"{{\"{idField}\":5}}") as JObject;
            ArgumentException expected = await Assert.ThrowsAsync<ArgumentException>(() => table.DeleteAsync(obj));
            Assert.Contains("The casing of the 'id' property is invalid.", expected.Message);
        }

        [Fact]
        public async Task UndeleteAsync()
        {
            await TestUndeleteAsync("", null);
        }

        [Fact]
        public async Task UndeleteAsyncWithParameters()
        {
            await TestUndeleteAsync("?custom=value", new Dictionary<string, string>() { { "custom", "value" } });
        }

        private static async Task TestUndeleteAsync(string query, IDictionary<string, string> parameters)
        {
            TestHttpHandler hijack = new TestHttpHandler();

            hijack.SetResponseContent("{\"id\":\"an id\",\"String\":\"Hey\"}");
            hijack.OnSendingRequest = req =>
            {
                Assert.Equal(req.RequestUri.Query, query);
                Assert.Equal(req.Method, HttpMethod.Post);

                // only id and version should be sent
                Assert.Null(req.Content);
                Assert.Equal("\"abc\"", req.Headers.IfMatch.First().Tag);
                return Task.FromResult(req);
            };
            IMobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);

            IMobileServiceTable table = service.GetTable("someTable");

            var obj = JToken.Parse("{\"id\":\"id\",\"value\":\"new\", \"blah\":\"doh\", \"version\": \"abc\"}") as JObject;
            JObject item = await table.UndeleteAsync(obj, parameters) as JObject;

            Assert.Equal("an id", (string)item["id"]);
            Assert.Equal("Hey", (string)item["String"]);
        }

        [Fact]
        public async Task DeleteAsyncWithStringId_KeepsSystemProperties()
        {
            var userDefinedParameters = new Dictionary<string, string>() { { "__systemproperties", "createdAt" } };

            TestHttpHandler hijack = new TestHttpHandler();
            IMobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);
            IMobileServiceTable table = service.GetTable("tests");

            JObject obj = JToken.Parse("{\"id\":\"A\",\"value\":\"new\"}") as JObject;
            hijack.SetResponseContent("{\"id\":\"A\",\"value\":\"new\", \"version\":\"XYZ\",\"__unknown\":12,\"CREATEDat\":\"12-02-02\"}");
            JToken newObj = await table.DeleteAsync(obj);

            Assert.Equal("A", (string)newObj["id"]);
            Assert.NotEqual(newObj, obj);
            Assert.NotNull(newObj["version"]);
            Assert.NotNull(newObj["__unknown"]);
            Assert.NotNull(newObj["CREATEDat"]);
        }

        [Fact]
        public async Task DeleteAsyncWithIntId_DoesNotStripSystemProperties()
        {
            var userDefinedParameters = new Dictionary<string, string>() { { "state", "AL" } };

            TestHttpHandler hijack = new TestHttpHandler();
            IMobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);
            IMobileServiceTable table = service.GetTable("tests");

            JObject obj = JToken.Parse("{\"id\":12,\"value\":\"new\"}") as JObject;
            hijack.SetResponseContent("{\"id\":12,\"value\":\"new\", \"version\":\"XYZ\",\"__unknown\":12,\"CREATEDat\":\"12-02-02\"}");
            JToken newObj = await table.DeleteAsync(obj, userDefinedParameters);

            Assert.Equal(12, (int)newObj["id"]);
            Assert.NotEqual(newObj, obj);
            Assert.NotNull(newObj["version"]);
            Assert.NotNull(newObj["__unknown"]);
            Assert.NotNull(newObj["CREATEDat"]);
        }

        [Fact]
        public async Task InsertAsync_RemovesSystemProperties_WhenIdIsString_Generic()
        {
            string[] testSystemProperties = SystemPropertiesTestData.ValidSystemProperties;

            foreach (string testSystemProperty in testSystemProperties)
            {
                TestHttpHandler hijack = new TestHttpHandler();
                hijack.SetResponseContent("{\"id\":\"an id\",\"String\":\"Hey\"}");
                var service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);
                hijack.OnSendingRequest = async (request) =>
                {
                    string content = await request.Content.ReadAsStringAsync();
                    JObject obj = JToken.Parse(content) as JObject;
                    Assert.True(obj.Properties().Where(p => p.Name == "id").Any());
                    Assert.True(obj.Properties().Where(p => p.Name == "String").Any());
                    Assert.False(obj.Properties().Where(p => p.Name == testSystemProperty).Any());
                    return request;
                };
                var table = service.GetTable<ToDoWithSystemPropertiesType>();

                JObject itemToInsert = new JObject();
                if (testSystemProperty.Contains("deleted"))
                {
                    itemToInsert = JToken.Parse("{\"id\":\"an id\",\"String\":\"what?\",\"" + testSystemProperty + "\":\"false\"}") as JObject;
                }
                else
                {
                    itemToInsert = JToken.Parse("{\"id\":\"an id\",\"String\":\"what?\",\"" + testSystemProperty + "\":\"2015-09-26\"}") as JObject;
                }
                var typedItemToInsert = itemToInsert.ToObject<ToDoWithSystemPropertiesType>();
                await table.InsertAsync(typedItemToInsert);
            }
        }

        [Fact]
        public async Task InsertAsync_DoesNotRemoveSystemProperties_WhenIdIsString()
        {
            await InsertAsync_DoesNotRemoveSystemPropertiesTest(client => client.GetTable("some"));
        }

        [Fact]
        public async Task InsertAsync_JToken_DoesNotRemoveSystemProperties_WhenIdIsString_Generic()
        {
            await InsertAsync_DoesNotRemoveSystemPropertiesTest(client => client.GetTable<ToDoWithSystemPropertiesType>());
        }

        private static async Task InsertAsync_DoesNotRemoveSystemPropertiesTest(Func<IMobileServiceClient, IMobileServiceTable> getTable)
        {
            string[] testSystemProperties = SystemPropertiesTestData.ValidSystemProperties;

            foreach (string testSystemProperty in testSystemProperties)
            {
                TestHttpHandler hijack = new TestHttpHandler();
                hijack.SetResponseContent("{\"id\":\"an id\",\"String\":\"Hey\"}");
                IMobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);
                IMobileServiceTable table = getTable(service);
                hijack.OnSendingRequest = async (request) =>
                {
                    string content = await request.Content.ReadAsStringAsync();
                    JObject obj = JToken.Parse(content) as JObject;
                    Assert.True(obj.Properties().Where(p => p.Name == "id").Any());
                    Assert.True(obj.Properties().Where(p => p.Name == "String").Any());
                    Assert.True(obj.Properties().Where(p => p.Name == testSystemProperty).Any());
                    return request;
                };

                JObject itemToInsert = JToken.Parse("{\"id\":\"an id\",\"String\":\"what?\",\"" + testSystemProperty + "\":\"a value\"}") as JObject;
                await table.InsertAsync(itemToInsert);
            }
        }

        [Fact]
        public async Task InsertAsyncStringIdNonSystemPropertiesNotRemovedFromRequest()
        {
            string[] testSystemProperties = SystemPropertiesTestData.NonSystemProperties;

            foreach (string testSystemProperty in testSystemProperties)
            {
                TestHttpHandler hijack = new TestHttpHandler();
                hijack.SetResponseContent("{\"id\":\"an id\",\"String\":\"Hey\",\"" + testSystemProperty + "\":\"a value\"}");
                IMobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);

                IMobileServiceTable table = service.GetTable("someTable");

                hijack.OnSendingRequest = async (request) =>
                {
                    string content = await request.Content.ReadAsStringAsync();
                    JObject obj = JToken.Parse(content) as JObject;
                    Assert.True(obj.Properties().Where(p => p.Name == "id").Any());
                    Assert.True(obj.Properties().Where(p => p.Name == "String").Any());
                    Assert.True(obj.Properties().Where(p => p.Name == testSystemProperty).Any());
                    return request;
                };

                JObject itemToInsert = JToken.Parse("{\"id\":\"an id\",\"String\":\"what?\",\"" + testSystemProperty + "\":\"a value\"}") as JObject;
                await table.InsertAsync(itemToInsert);
            }
        }

        [Fact]
        public async Task InsertAsync_DoesNotRemoveSystemProperties_WhenIdIsNull()
        {
            string[] testSystemProperties = SystemPropertiesTestData.ValidSystemProperties;

            foreach (string testSystemProperty in testSystemProperties)
            {
                TestHttpHandler hijack = new TestHttpHandler();
                hijack.SetResponseContent("{\"id\":\"an id\",\"String\":\"Hey\"}");
                IMobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);

                IMobileServiceTable table = service.GetTable("someTable");

                hijack.OnSendingRequest = async (request) =>
                {
                    string content = await request.Content.ReadAsStringAsync();
                    JObject obj = JToken.Parse(content) as JObject;
                    Assert.True(obj.Properties().Where(p => p.Name == "id").Any());
                    Assert.True(obj.Properties().Where(p => p.Name == "String").Any());
                    Assert.True(obj.Properties().Where(p => p.Name == testSystemProperty).Any());
                    return request;
                };

                JObject itemToInsert = JToken.Parse("{\"id\":null,\"String\":\"what?\",\"" + testSystemProperty + "\":\"a value\"}") as JObject;
                await table.InsertAsync(itemToInsert);
            }
        }

        [Fact]
        public async Task InsertAsyncNullIdNonSystemPropertiesNotRemovedFromRequest()
        {
            string[] testSystemProperties = SystemPropertiesTestData.NonSystemProperties;

            foreach (string testSystemProperty in testSystemProperties)
            {
                TestHttpHandler hijack = new TestHttpHandler();
                hijack.SetResponseContent("{\"id\":\"an id\",\"String\":\"Hey\",\"" + testSystemProperty + "\":\"a value\"}");
                IMobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);

                IMobileServiceTable table = service.GetTable("someTable");

                hijack.OnSendingRequest = async (request) =>
                {
                    string content = await request.Content.ReadAsStringAsync();
                    JObject obj = JToken.Parse(content) as JObject;
                    Assert.True(obj.Properties().Where(p => p.Name == "id").Any());
                    Assert.True(obj.Properties().Where(p => p.Name == "String").Any());
                    Assert.True(obj.Properties().Where(p => p.Name == testSystemProperty).Any());
                    return request;
                };

                JObject itemToInsert = JToken.Parse("{\"id\":null,\"String\":\"what?\",\"" + testSystemProperty + "\":\"a value\"}") as JObject;
                await table.InsertAsync(itemToInsert);
            }
        }

        [Fact]
        public async Task UpdateAsyncStringIdSystemPropertiesRemovedFromRequest()
        {
            string[] testSystemProperties = SystemPropertiesTestData.ValidSystemProperties;

            foreach (string testSystemProperty in testSystemProperties)
            {
                TestHttpHandler hijack = new TestHttpHandler();
                hijack.SetResponseContent("{\"id\":\"an id\",\"String\":\"Hey\"}");
                IMobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);

                IMobileServiceTable table = service.GetTable("someTable");

                hijack.OnSendingRequest = async (request) =>
                {
                    string content = await request.Content.ReadAsStringAsync();
                    JObject obj = JToken.Parse(content) as JObject;
                    Assert.True(obj.Properties().Where(p => p.Name == "id").Any());
                    Assert.True(obj.Properties().Where(p => p.Name == "String").Any());
                    Assert.False(obj.Properties().Where(p => p.Name == testSystemProperty).Any());
                    return request;
                };

                JObject itemToUpdate = JToken.Parse("{\"id\":\"an id\",\"String\":\"what?\",\"" + testSystemProperty + "\":\"a value\"}") as JObject;
                await table.UpdateAsync(itemToUpdate);
            }
        }

        [Fact]
        public async Task UpdateAsyncStringIdNonSystemPropertiesNotRemovedFromRequest()
        {
            string[] testSystemProperties = SystemPropertiesTestData.NonSystemProperties;

            foreach (string testSystemProperty in testSystemProperties)
            {
                TestHttpHandler hijack = new TestHttpHandler();
                hijack.SetResponseContent("{\"id\":\"an id\",\"String\":\"Hey\",\"" + testSystemProperty + "\":\"a value\"}");
                IMobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);

                IMobileServiceTable table = service.GetTable("someTable");

                hijack.OnSendingRequest = async (request) =>
                {
                    string content = await request.Content.ReadAsStringAsync();
                    JObject obj = JToken.Parse(content) as JObject;
                    Assert.True(obj.Properties().Where(p => p.Name == "id").Any());
                    Assert.True(obj.Properties().Where(p => p.Name == "String").Any());
                    Assert.True(obj.Properties().Where(p => p.Name == testSystemProperty).Any());
                    return request;
                };

                JObject itemToUpdate = JToken.Parse("{\"id\":\"an id\",\"String\":\"what?\",\"" + testSystemProperty + "\":\"a value\"}") as JObject;
                await table.UpdateAsync(itemToUpdate);
            }
        }

        [Fact]
        public Task FeatureHeaderValidation_UntypedTableRead()
        {
            return this.ValidateFeaturesHeader("TU", true, t => t.ReadAsync(""));
        }

        [Fact]
        public Task FeatureHeaderValidation_UntypedTableLookup()
        {
            return this.ValidateFeaturesHeader("TU", false, t => t.LookupAsync("id"));
        }

        [Fact]
        public Task FeatureHeaderValidation_UntypedTableInsert()
        {
            JObject obj = JObject.Parse("{\"id\":\"the id\",\"value\":\"new\"}");
            return this.ValidateFeaturesHeader("TU", false, t => t.InsertAsync(obj));
        }

        [Fact]
        public Task FeatureHeaderValidation_UntypedTableUpdate()
        {
            JObject obj = JObject.Parse("{\"id\":\"the id\",\"value\":\"new\"}");
            return this.ValidateFeaturesHeader("TU", false, t => t.UpdateAsync(obj));
        }

        [Fact]
        public Task FeatureHeaderValidation_UntypedTableDelete()
        {
            JObject obj = JObject.Parse("{\"id\":\"the id\",\"value\":\"new\"}");
            return this.ValidateFeaturesHeader("TU", false, t => t.DeleteAsync(obj));
        }

        [Fact]
        public Task FeatureHeaderValidation_UntypedTableReadWithQuery()
        {
            return this.ValidateFeaturesHeader("TU,QS", true, t => t.ReadAsync("", new Dictionary<string, string> { { "a", "b" } }));
        }

        [Fact]
        public Task FeatureHeaderValidation_UntypedTableLookupWithQuery_FeaturesHeaderIsCorrect()
        {
            return this.ValidateFeaturesHeader("TU,QS", false, t => t.LookupAsync("id", new Dictionary<string, string> { { "a", "b" } }));
        }

        [Fact]
        public Task FeatureHeaderValidation_UntypedTableInsertWithQuery()
        {
            JObject obj = JObject.Parse("{\"id\":\"the id\",\"value\":\"new\"}");
            return this.ValidateFeaturesHeader("TU,QS", false, t => t.InsertAsync(obj, new Dictionary<string, string> { { "a", "b" } }));
        }

        [Fact]
        public Task FeatureHeaderValidation_UntypedTableUpdateWithQuery()
        {
            JObject obj = JObject.Parse("{\"id\":\"the id\",\"value\":\"new\"}");
            return this.ValidateFeaturesHeader("TU,QS", false, t => t.UpdateAsync(obj, new Dictionary<string, string> { { "a", "b" } }));
        }

        [Fact]
        public Task FeatureHeaderValidation_UntypedTableDeleteWithQuery()
        {
            JObject obj = JObject.Parse("{\"id\":\"the id\",\"value\":\"new\"}");
            return this.ValidateFeaturesHeader("TU,QS", false, t => t.DeleteAsync(obj, new Dictionary<string, string> { { "a", "b" } }));
        }

        private async Task ValidateFeaturesHeader(string expectedFeaturesHeader, bool arrayResponse, Func<IMobileServiceTable, Task> operation)
        {
            TestHttpHandler hijack = new TestHttpHandler();
            bool validationDone = false;
            hijack.OnSendingRequest = (request) =>
            {
                Assert.Equal(expectedFeaturesHeader, request.Headers.GetValues("X-ZUMO-FEATURES").First());
                validationDone = true;
                return Task.FromResult(request);
            };

            IMobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);
            IMobileServiceTable table = service.GetTable("someTable");

            var responseContent = "{\"id\":\"the id\",\"String\":\"Hey\"}";
            if (arrayResponse)
            {
                responseContent = "[" + responseContent + "]";
            }

            hijack.SetResponseContent(responseContent);
            await operation(table);
            Assert.True(validationDone);
        }
    }
}
