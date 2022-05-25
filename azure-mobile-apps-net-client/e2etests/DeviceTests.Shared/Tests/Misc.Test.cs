// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using DeviceTests.Shared.Helpers;
using DeviceTests.Shared.Helpers.Models;
using DeviceTests.Shared.TestPlatform;
using Microsoft.WindowsAzure.MobileServices;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms.Internals;
using Xunit;

namespace DeviceTests.Shared.Tests
{
    [Collection(nameof(SingleThreadedCollection))]
    public class Misc_Tests : E2ETestBase
    {
        [DataTable("ParamsTestTable")]
        public class ParamsTestTableItem
        {
            public int Id { get; set; }
            public string parameters { get; set; }
        }

        [DataTable("RoundTripTable")]
        class VersionedType
        {
            public string Id { get; set; }

            [JsonProperty(PropertyName = "name")]
            public string Name { get; set; }

            [JsonProperty(PropertyName = "number")]
            public double Number { get; set; }

            [Version]
            public string Version { get; set; }

            [CreatedAt]
            public DateTime CreatedAt { get; set; }

            [UpdatedAt]
            public DateTime UpdatedAt { get; set; }

            public VersionedType() { }
            public VersionedType(Random rndGen)
            {
                this.Name = Utilities.CreateSimpleRandomString(rndGen, 20);
                this.Number = rndGen.Next(10000);
            }

            private VersionedType(VersionedType other)
            {
                this.Id = other.Id;
                this.Name = other.Name;
                this.Number = other.Number;
                this.Version = other.Version;
                this.CreatedAt = other.CreatedAt;
                this.UpdatedAt = other.UpdatedAt;
            }

            public override string ToString()
            {
                return string.Format("Versioned[Id={0},Name={1},Number={2},Version={3},CreatedAt={4},UpdatedAt={5}]",
                    Id, Name, Number, Version,
                    CreatedAt.ToString("yyyy-MM-dd'T'HH:mm:ss.fff'Z'", CultureInfo.InvariantCulture),
                    UpdatedAt.ToString("yyyy-MM-dd'T'HH:mm:ss.fff'Z'", CultureInfo.InvariantCulture));
            }

            public override int GetHashCode()
            {
                int result = 0;
                if (Name != null) result ^= Name.GetHashCode();
                result ^= Number.GetHashCode();
                return result;
            }

            public override bool Equals(object obj)
            {
                var other = obj as VersionedType;
                if (other == null) return false;
                if (this.Name != other.Name) return false;
                if (this.Number != other.Number) return false;
                return true;
            }

            public VersionedType Clone()
            {
                return new VersionedType(this);
            }
        }

        [Fact]
        public async Task CreateFilterTestWithMultipleRequests_WithTypedTable()
        {
            var client = this.GetClient();
            int numberOfRequests = new Random().Next(2, 5);
            var handler = new HandlerWithMultipleRequests(this, numberOfRequests);
            var filteredClient = new MobileServiceClient(client.MobileAppUri, handler);

            var typedTable = filteredClient.GetTable<RoundTripTableItem>();
            var untypedTable = filteredClient.GetTable("RoundTripTable");
            var uniqueId = Guid.NewGuid().ToString("N");
            var item = new RoundTripTableItem { Name = uniqueId };
            await typedTable.InsertAsync(item);

            Assert.False(handler.TestFailed);

            // Cleanup
            handler.NumberOfRequests = 1; // no need to send it multiple times anymore
            var items = await untypedTable.ReadAsync("$select=name,id&$filter=name eq '" + uniqueId + "'") as JArray;
            items.ForEach(async t => await untypedTable.DeleteAsync(t as JObject));

            Assert.True(items.Count == numberOfRequests);
        }

        [Fact]
        public async Task CreateFilterTestWithMultipleRequests_WithUntypedTable()
        {
            var client = this.GetClient();
            int numberOfRequests = new Random().Next(2, 5);
            var handler = new HandlerWithMultipleRequests(this, numberOfRequests);
            var filteredClient = new MobileServiceClient(client.MobileAppUri, handler);

            var untypedTable = filteredClient.GetTable("RoundTripTable");
            var uniqueId = Guid.NewGuid().ToString("N");
            
            var item = new JObject(new JProperty("name", uniqueId));
            await untypedTable.InsertAsync(item);

            Assert.False(handler.TestFailed);

            // Cleanup
            handler.NumberOfRequests = 1; // no need to send it multiple times anymore
            var items = await untypedTable.ReadAsync("$select=name,id&$filter=name eq '" + uniqueId + "'") as JArray;
            items.ForEach(async t => await untypedTable.DeleteAsync(t as JObject));

            Assert.True(items.Count == numberOfRequests);
        }

        [Fact]
        public Task ValidateUserAgent()
         => CreateUserAgentValidationTest();

        [Fact]
        public Task ParameterPassingTests_WithTypedTable()
            => CreateParameterPassingTest(true);

        [Fact]
        public Task ParameterPassingTests_WithUntypedTable()
            => CreateParameterPassingTest(false);

        [Fact]
        public async Task OptimisticConcurrency_ClientSide()
        {

            await CreateOptimisticConcurrencyTest("Conflicts (client side) - client wins", (clientItem, serverItem) =>
            {
                var mergeResult = clientItem.Clone();
                mergeResult.Version = serverItem.Version;
                return mergeResult;
            });
            await CreateOptimisticConcurrencyTest("Conflicts (client side) - server wins", (clientItem, serverItem) =>
            {
                return serverItem;
            });
            await CreateOptimisticConcurrencyTest("Conflicts (client side) - Name from client, Number from server", (clientItem, serverItem) =>
            {
                var mergeResult = serverItem.Clone();
                mergeResult.Name = clientItem.Name;
                return mergeResult;
            });
        }

        [Fact]
        public Task OptimisticConcurrency_ServerSide_ClientWins()
            => CreateOptimisticConcurrencyWithServerConflictsTest("Conflicts (server side) - client wins", true);

        [Fact]
        public Task OptimisticConcurrency_ServerSide_ServerWins()
            => CreateOptimisticConcurrencyWithServerConflictsTest("Conflicts (server side) - server wins", false);

        [Fact]
        public Task SystemPropertiesTests_WithTypedTable()
            => CreateSystemPropertiesTest(true);

        [Fact]
        public Task SystemPropertiesTests_WithUntypedTable()
            => CreateSystemPropertiesTest(false);

        private async Task CreateSystemPropertiesTest(bool useTypedTable)
        {
            var client = this.GetClient();
            var typedTable = client.GetTable<VersionedType>();
            var untypedTable = client.GetTable("RoundTripTable");

            DateTime now = DateTime.UtcNow;
            int seed = now.Year * 10000 + now.Month * 100 + now.Day;
            Random rndGen = new Random(seed);
            VersionedType item = null;
            JObject untypedItem = null;
            DateTime createdAt, updatedAt;
            string id;
            if (useTypedTable)
            {
                item = new VersionedType(rndGen);
                await typedTable.InsertAsync(item);
                id = item.Id;
                createdAt = item.CreatedAt;
                updatedAt = item.UpdatedAt;
            }
            else
            {
                untypedItem = new JObject();
                untypedItem.Add("name", "unused");
                untypedItem = (JObject)(await untypedTable.InsertAsync(untypedItem));
                id = (string)untypedItem["id"];
                createdAt = untypedItem["createdAt"].ToObject<DateTime>();
                updatedAt = untypedItem["updatedAt"].ToObject<DateTime>();
            }

            DateTime otherCreatedAt, otherUpdatedAt;
            string otherId;
            if (useTypedTable)
            {
                item = new VersionedType(rndGen);
                await typedTable.InsertAsync(item);
                otherId = item.Id;
                otherCreatedAt = item.CreatedAt;
                otherUpdatedAt = item.UpdatedAt;
            }
            else
            {
                untypedItem = new JObject();
                untypedItem.Add("name", "unused");
                untypedItem = (JObject)(await untypedTable.InsertAsync(untypedItem));
                otherId = (string)untypedItem["id"];
                otherCreatedAt = untypedItem["createdAt"].ToObject<DateTime>();
                otherUpdatedAt = untypedItem["updatedAt"].ToObject<DateTime>();
            }

            Assert.True(createdAt < otherCreatedAt);
            Assert.True(updatedAt < otherUpdatedAt);

            createdAt = otherCreatedAt;
            updatedAt = otherUpdatedAt;

            if (useTypedTable)
            {
                item = new VersionedType(rndGen) { Id = otherId };
                await typedTable.UpdateAsync(item);
                otherUpdatedAt = item.UpdatedAt;
                otherCreatedAt = item.CreatedAt;
            }
            else
            {
                untypedItem = new JObject(new JProperty("id", otherId), new JProperty("name", "other name"));
                untypedItem = (JObject)(await untypedTable.UpdateAsync(untypedItem));
                otherCreatedAt = untypedItem["createdAt"].ToObject<DateTime>();
                otherUpdatedAt = untypedItem["updatedAt"].ToObject<DateTime>();
            }

            Assert.Equal(createdAt, otherCreatedAt);
            Assert.False(otherUpdatedAt <= updatedAt);

            await untypedTable.DeleteAsync(new JObject(new JProperty("id", id)));
            await untypedTable.DeleteAsync(new JObject(new JProperty("id", otherId)));
        }

        private async Task CreateOptimisticConcurrencyTest(string testName, Func<VersionedType, VersionedType, VersionedType> mergingPolicy)
        {
            var client = this.GetClient();
            var table = client.GetTable<VersionedType>();
            DateTime now = DateTime.UtcNow;
            int seed = now.Year * 10000 + now.Month * 100 + now.Day;
            Random rndGen = new Random(seed);
            var item = new VersionedType(rndGen);
            await table.InsertAsync(item);

            var client2 = new MobileServiceClient(client.MobileAppUri);
            var table2 = client.GetTable<VersionedType>();
            var item2 = await table2.LookupAsync(item.Id);
            item2.Name = Utilities.CreateSimpleRandomString(rndGen, 20);
            item2.Number = rndGen.Next(100000);
            await table2.UpdateAsync(item2);

            var ex = await Assert.ThrowsAsync<MobileServicePreconditionFailedException<VersionedType>>(async () =>
            {
                item.Name = Utilities.CreateSimpleRandomString(rndGen, 20);
                await table.UpdateAsync(item);
            });

            var serverItem = ex.Item;
            Assert.Equal(serverItem.Version, item2.Version);

            var cachedMergedItem = mergingPolicy(item, serverItem);
            var mergedItem = mergingPolicy(item, serverItem);

            await table.UpdateAsync(mergedItem);
            Assert.Equal(cachedMergedItem, mergedItem);
            await table2.RefreshAsync(item2);
            Assert.Equal(item2, mergedItem);
        }

        private async Task CreateOptimisticConcurrencyWithServerConflictsTest(string testName, bool clientWins)
        {
            var client = this.GetClient();
            var table = client.GetTable<VersionedType>();
            DateTime now = DateTime.UtcNow;
            int seed = now.Year * 10000 + now.Month * 100 + now.Day;
            Random rndGen = new Random(seed);
            var item = new VersionedType(rndGen);
            await table.InsertAsync(item);

            var client2 = new MobileServiceClient(client.MobileAppUri);
            var table2 = client.GetTable<VersionedType>();
            var item2 = await table2.LookupAsync(item.Id);
            item2.Name = Utilities.CreateSimpleRandomString(rndGen, 20);
            item2.Number = rndGen.Next(100000);
            await table2.UpdateAsync(item2);

            string oldName = item2.Name;
            string newName = Utilities.CreateSimpleRandomString(rndGen, 20);
            item.Name = newName;
            await table.UpdateAsync(item, new Dictionary<string, string> { { "conflictPolicy", clientWins ? "clientWins" : "serverWins" } });

            await table2.RefreshAsync(item2);
            if (clientWins)
            {
                Assert.False(item.Name != newName || item2.Name != newName);
            }
            else
            {
                Assert.False(item.Name != oldName || item2.Name != oldName);
            }

            await table.DeleteAsync(item);
        }

        private async Task CreateParameterPassingTest(bool useTypedTable)
        {
            var client = this.GetClient();
            var typed = client.GetTable<ParamsTestTableItem>();
            var untyped = client.GetTable("ParamsTestTable");
            var dict = new Dictionary<string, string>
                {
                    { "item", "simple" },
                    { "empty", "" },
                    { "spaces", "with spaces" },
                    { "specialChars", "`!@#$%^&*()-=[]\\;',./~_+{}|:\"<>?" },
                    { "latin", "ãéìôü ÇñÑ" },
                    { "arabic", "الكتاب على الطاولة" },
                    { "chinese", "这本书在桌子上" },
                    { "japanese", "本は机の上に" },
                    { "hebrew", "הספר הוא על השולחן" },
                    { "name+with special&chars", "should just work" }
                };

            var expectedParameters = new JObject();
            foreach (var key in dict.Keys)
            {
                expectedParameters.Add(key, dict[key]);
            }

            bool testPassed = true;

            ParamsTestTableItem typedItem = new ParamsTestTableItem();
            var untypedItem = new JObject();
            JObject actualParameters;

            dict["operation"] = "insert";
            expectedParameters.Add("operation", "insert");
            if (useTypedTable)
            {
                await typed.InsertAsync(typedItem, dict);
                actualParameters = JObject.Parse(typedItem.parameters);
            }
            else
            {
                var inserted = await untyped.InsertAsync(untypedItem, dict);
                untypedItem = inserted as JObject;
                actualParameters = JObject.Parse(untypedItem["parameters"].Value<string>());
            }

            testPassed = testPassed && ValidateParameters("insert", expectedParameters, actualParameters);

            dict["operation"] = "update";
            expectedParameters["operation"] = "update";
            if (useTypedTable)
            {
                await typed.UpdateAsync(typedItem, dict);
                actualParameters = JObject.Parse(typedItem.parameters);
            }
            else
            {
                var updated = await untyped.UpdateAsync(untypedItem, dict);
                actualParameters = JObject.Parse(updated["parameters"].Value<string>());
            }

            testPassed = testPassed && ValidateParameters("update", expectedParameters, actualParameters);

            dict["operation"] = "lookup";
            expectedParameters["operation"] = "lookup";
            if (useTypedTable)
            {
                var temp = await typed.LookupAsync(1, dict);
                actualParameters = JObject.Parse(temp.parameters);
            }
            else
            {
                var temp = await untyped.LookupAsync(1, dict);
                actualParameters = JObject.Parse(temp["parameters"].Value<string>());
            }

            testPassed = testPassed && ValidateParameters("lookup", expectedParameters, actualParameters);

            dict["operation"] = "read";
            expectedParameters["operation"] = "read";
            if (useTypedTable)
            {
                var temp = await typed.Where(t => t.Id >= 1).WithParameters(dict).ToListAsync();
                actualParameters = JObject.Parse(temp[0].parameters);
            }
            else
            {
                var temp = await untyped.ReadAsync("$filter=id ge 1", dict);
                actualParameters = JObject.Parse(temp[0]["parameters"].Value<string>());
            }

            testPassed = testPassed && ValidateParameters("read", expectedParameters, actualParameters);

            if (useTypedTable)
            {
                // Refresh operation only exists for typed tables
                dict["operation"] = "read";
                expectedParameters["operation"] = "read";
                typedItem.Id = 1;
                typedItem.parameters = "";
                await typed.RefreshAsync(typedItem, dict);
                actualParameters = JObject.Parse(typedItem.parameters);
                testPassed = testPassed && ValidateParameters("refresh", expectedParameters, actualParameters);
            }

            // Delete operation doesn't populate the object with the response, so we'll use a filter to capture that
            var handler = new HandlerToCaptureHttpTraffic();
            var filteredClient = new MobileServiceClient(client.MobileAppUri, handler);
            typed = filteredClient.GetTable<ParamsTestTableItem>();
            untyped = filteredClient.GetTable("ParamsTestTable");

            dict["operation"] = "delete";
            expectedParameters["operation"] = "delete";
            if (useTypedTable)
            {
                await typed.DeleteAsync(typedItem, dict);
            }
            else
            {
                await untyped.DeleteAsync(untypedItem, dict);
            }

            JObject response = JObject.Parse(handler.ResponseBody);
            actualParameters = JObject.Parse(response["parameters"].Value<string>());
            Assert.True(testPassed && ValidateParameters("delete", expectedParameters, actualParameters));
        }

        private bool ValidateParameters(string operation, JObject expected, JObject actual)
        {
            List<string> errors = new List<string>();
            return Utilities.CompareJson(expected, actual, errors);
        }

        private async Task CreateUserAgentValidationTest()
        {
            var handler = new HandlerToCaptureHttpTraffic();
            MobileServiceClient client = new MobileServiceClient(MobileServiceRuntimeUrl, handler);
            var table = client.GetTable<RoundTripTableItem>();
            var item = new RoundTripTableItem { Name = "hello" };
            await table.InsertAsync(item);
            Action<string> dumpAndValidateHeaders = delegate (string operation)
            {
                if (!handler.RequestHeaders.TryGetValue("User-Agent", out string userAgent))
                {
                    throw new InvalidOperationException("This will fail the test");
                }
                else
                {
                    Regex expected = new Regex(@"^ZUMO\/\d.\d");
                    if (!expected.IsMatch(userAgent))
                    {
                        throw new InvalidOperationException("This will fail the test");
                    }
                }
            };

            dumpAndValidateHeaders("Insert");

            item.Number = 123;
            await table.UpdateAsync(item);
            dumpAndValidateHeaders("Update");

            var item2 = await table.LookupAsync(item.Id);
            dumpAndValidateHeaders("Read");

            await table.DeleteAsync(item);
            dumpAndValidateHeaders("Delete");
        }

        private async Task CreateFilterTestWithMultipleRequests(bool typed)
        {
            var client = this.GetClient();
            int numberOfRequests = new Random().Next(2, 5);
            var handler = new HandlerWithMultipleRequests(this, numberOfRequests);
            var filteredClient = new MobileServiceClient(client.MobileAppUri, handler);

            var typedTable = filteredClient.GetTable<RoundTripTableItem>();
            var untypedTable = filteredClient.GetTable("RoundTripTable");
            var uniqueId = Guid.NewGuid().ToString("N");
            if (typed)
            {
                var item = new RoundTripTableItem { Name = uniqueId };
                await typedTable.InsertAsync(item);
            }
            else
            {
                var item = new JObject(new JProperty("name", uniqueId));
                await untypedTable.InsertAsync(item);
            }
            Assert.False(handler.TestFailed);
            handler.NumberOfRequests = 1; // no need to send it multiple times anymore

            var items = await untypedTable.ReadAsync("$select=name,id&$filter=name eq '" + uniqueId + "'");
            var array = (JArray)items;
            bool passed = (array.Count == numberOfRequests);

            // Cleanup
            foreach (var item in array)
            {
                await untypedTable.DeleteAsync(item as JObject);
            }

            Assert.True(passed);
        }

        class HandlerWhichThrows : DelegatingHandler
        {
            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }
        }

        class HandlerToBypassService : DelegatingHandler
        {
            HttpStatusCode statusCode;
            string contentType;
            string content;

            public HandlerToBypassService(int statusCode, string contentType, string content)
            {
                this.statusCode = (HttpStatusCode)statusCode;
                this.contentType = contentType;
                this.content = content;
            }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                TaskCompletionSource<HttpResponseMessage> tcs = new TaskCompletionSource<HttpResponseMessage>();
                HttpResponseMessage result = new HttpResponseMessage(this.statusCode);
                result.Content = new StringContent(this.content, Encoding.UTF8, this.contentType);
                tcs.SetResult(result);
                return tcs.Task;
            }
        }

        class HandlerToCaptureHttpTraffic : DelegatingHandler
        {
            public Dictionary<string, string> RequestHeaders { get; private set; }
            public Dictionary<string, string> ResponseHeaders { get; private set; }
            public string ResponseBody { get; set; }

            public HandlerToCaptureHttpTraffic()
            {
                this.RequestHeaders = new Dictionary<string, string>();
                this.ResponseHeaders = new Dictionary<string, string>();
            }

            protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                this.RequestHeaders.Clear();
                foreach (var header in request.Headers)
                {
                    this.RequestHeaders.Add(header.Key, string.Join(", ", header.Value));
                    if (header.Key.Equals("user-agent", StringComparison.OrdinalIgnoreCase))
                    {
                        string userAgent = this.RequestHeaders[header.Key];
                        userAgent.TrimEnd(')');
                        int equalsIndex = userAgent.LastIndexOf('=');
                        if (equalsIndex >= 0)
                        {
                            //var clientVersion = userAgent.Substring(equalsIndex + 1);
                            //ZumoTestGlobals.Instance.GlobalTestParams[ZumoTestGlobals.ClientVersionKeyName] = clientVersion;
                        }
                    }
                }

                var response = await base.SendAsync(request, cancellationToken);
                this.ResponseHeaders.Clear();
                foreach (var header in response.Headers)
                {
                    this.ResponseHeaders.Add(header.Key, string.Join(", ", header.Value));
                    if (header.Key.Equals("x-zumo-version", StringComparison.OrdinalIgnoreCase))
                    {
                        //ZumoTestGlobals.Instance.GlobalTestParams[ZumoTestGlobals.RuntimeVersionKeyName] = this.ResponseHeaders[header.Key];
                    }
                }

                this.ResponseBody = await response.Content.ReadAsStringAsync();
                return response;
            }
        }

        class HandlerWithMultipleRequests : DelegatingHandler
        {
            private E2ETestBase Parent { get; set; }
            public bool TestFailed { get; private set; }
            public int NumberOfRequests { get; set; }

            public HandlerWithMultipleRequests(E2ETestBase parent, int numberOfRequests)
            {
                this.Parent = parent;
                this.NumberOfRequests = numberOfRequests;
                this.TestFailed = false;

                if (numberOfRequests < 1)
                {
                    throw new ArgumentOutOfRangeException("numberOfRequests", "Number of requests must be at least 1.");
                }
            }

            private static async Task<HttpRequestMessage> CloneRequestAsync(HttpRequestMessage request)
            {
                var clone = new HttpRequestMessage(request.Method, request.RequestUri);

                // Copy the request's content into the cloned object
                if (request.Content != null)
                {
                    var content = await request.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
                    clone.Content = new ByteArrayContent(content);

                    // Copy the content headers
                    if (request.Content.Headers != null)
                    {
                        foreach (var h in request.Content.Headers)
                        {
                            clone.Content.Headers.Add(h.Key, h.Value);
                        }
                    }
                }

                clone.Version = request.Version;

                foreach (var prop in request.Properties)
                {
                    clone.Properties.Add(prop);
                }

                foreach (var header in request.Headers)
                {
                    clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }

                return clone;
            }

            protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                HttpResponseMessage response = null;
                try
                {
                    for (int i = 0; i < this.NumberOfRequests; i++)
                    {
                        HttpRequestMessage clonedRequest = await CloneRequestAsync(request);
                        response = await base.SendAsync(clonedRequest, cancellationToken);
                        if (i < this.NumberOfRequests - 1)
                        {
                            response.Dispose();
                            response = null;
                        }
                    }
                }
                catch (Exception)
                {
                    this.TestFailed = true;
                    throw;
                }

                return response;
            }
        }
    }
}
