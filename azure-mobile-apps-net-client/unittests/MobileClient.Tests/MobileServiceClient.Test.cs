// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using Microsoft.WindowsAzure.MobileServices;
using Microsoft.WindowsAzure.MobileServices.Sync;
using Newtonsoft.Json.Linq;

using MobileClient.Tests.Helpers;
using Xunit;

namespace MobileClient.Tests
{
    public class MobileServiceClient_Tests
    {
        [Fact]
        public void Constructor_Basic()
        {
            var service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp);
            Assert.Equal(MobileAppUriValidator.DummyMobileApp, service.MobileAppUri.ToString());
        }

        [Fact]
        public void Constructor_NullThrows()
            => Assert.Throws<ArgumentNullException>(() => new MobileServiceClient(mobileAppUri: (string)null));

        [Fact]
        public void Constructor_NullUriThrows()
            => Assert.Throws<ArgumentNullException>(() => new MobileServiceClient(mobileAppUri: (Uri)null));

        [Fact]
        public void Constructor_Basic_NoTrailignSlash()
        {
            var service = new MobileServiceClient(MobileAppUriValidator.DummyMobileAppUriWithFolderWithoutTrailingSlash);
            Assert.NotNull(service.MobileAppUri);
            Assert.True(service.MobileAppUri.IsAbsoluteUri);
            Assert.EndsWith("/", service.MobileAppUri.AbsoluteUri);
            Assert.NotNull(service.HttpClient);
        }

        [Fact]
        public void InstallationId_IsAvailable()
        {
            var service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp);
            Assert.NotNull(service.InstallationId);
        }

        [Fact]
        public async Task Constructor_SingleHttpHandler()
        {
            var hijack = new TestHttpHandler();
            var service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, handlers: hijack);
            var validator = new MobileAppUriValidator(service);

            hijack.SetResponseContent("[]");
            _ = await service.GetTable("foo").ReadAsync("bar");
            Assert.StartsWith(validator.TableBaseUri, hijack.Request.RequestUri.ToString());
        }

        [Fact]
        public void Constructor_DoesNotRewireHandlers()
        {
            var innerHandler = new TestHttpHandler();
            var wiredHandler = new TestHttpHandler { InnerHandler = innerHandler };
            _ = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, handlers: wiredHandler);
            Assert.Equal(innerHandler, wiredHandler.InnerHandler);
        }

        [Fact]
        public async Task Constructor_MultipleHttpHandlers()
        {
            var hijack = new TestHttpHandler();
            string[] expectedMessages = { "before#1", "before#2", "after#2", "after#1" };
            var firstHandler = new ComplexDelegatingHandler(expectedMessages[0], expectedMessages[3]);
            var secondHandler = new ComplexDelegatingHandler(expectedMessages[1], expectedMessages[2]);
            var service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, new HttpMessageHandler[] { firstHandler, secondHandler, hijack });
            Assert.Same(hijack, secondHandler.InnerHandler);
            Assert.Same(secondHandler, firstHandler.InnerHandler);

            ComplexDelegatingHandler.ClearStoredMessages();
            hijack.SetResponseContent("[]");
            _ = await service.GetTable("foo").ReadAsync("bar");
            var actualMessages = new List<string>(ComplexDelegatingHandler.AllMessages);

            Assert.Equal(4, actualMessages.Count);
            Assert.Equal(expectedMessages, actualMessages.ToArray());
        }

        [Fact]
        public async Task Logout_Basic()
        {
            var service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp);
            service.CurrentUser = new MobileServiceUser("123456");
            Assert.NotNull(service.CurrentUser);
            await service.LogoutAsync();
            Assert.Null(service.CurrentUser);
        }

        [Fact]
        public async Task StandardRequestFormat()
        {
            string appUrl = MobileAppUriValidator.DummyMobileApp;
            string collection = "tests";
            string query = "$filter=id eq 12";

            TestHttpHandler hijack = new TestHttpHandler();
            IMobileServiceClient service = new MobileServiceClient(appUrl, hijack);
            service.CurrentUser = new MobileServiceUser("someUser");
            service.CurrentUser.MobileServiceAuthenticationToken = "Not rhubarb";

            hijack.SetResponseContent("[{\"id\":12,\"value\":\"test\"}]");
            JToken response = await service.GetTable(collection).ReadAsync(query);

            Assert.NotNull(hijack.Request.Headers.GetValues("X-ZUMO-INSTALLATION-ID").First());
            Assert.Equal("application/json", hijack.Request.Headers.Accept.First().MediaType);
            Assert.Equal("Not rhubarb", hijack.Request.Headers.GetValues("X-ZUMO-AUTH").First());
            Assert.NotNull(hijack.Request.Headers.GetValues("ZUMO-API-VERSION").First());

            string userAgent = hijack.Request.Headers.UserAgent.ToString();
            Assert.Contains("ZUMO/4.", userAgent);
            Assert.Contains("version=4.", userAgent);
        }

        [Fact]
        public async Task ErrorMessageConstruction()
        {
            string appUrl = MobileAppUriValidator.DummyMobileApp;
            string collection = "tests";
            string query = "$filter=id eq 12";

            TestHttpHandler hijack = new TestHttpHandler();
            IMobileServiceClient service = new MobileServiceClient(appUrl, hijack);
            MobileAppUriValidator mobileAppUriValidator = new MobileAppUriValidator(service);

            // Verify the error message is correctly pulled out
            hijack.SetResponseContent("{\"error\":\"error message\",\"other\":\"donkey\"}");
            hijack.Response.StatusCode = HttpStatusCode.Unauthorized;
            hijack.Response.ReasonPhrase = "YOU SHALL NOT PASS.";
            try
            {
                JToken response = await service.GetTable(collection).ReadAsync(query);
            }
            catch (InvalidOperationException ex)
            {
                Assert.Equal("error message", ex.Message);
            }

            // Verify all of the exception parameters
            hijack.Response = new HttpResponseMessage(HttpStatusCode.Unauthorized);
            hijack.Response.Content = new StringContent("{\"error\":\"error message\",\"other\":\"donkey\"}", Encoding.UTF8, "application/json");
            hijack.Response.ReasonPhrase = "YOU SHALL NOT PASS.";
            try
            {
                JToken response = await service.GetTable(collection).ReadAsync(query);
            }
            catch (MobileServiceInvalidOperationException ex)
            {
                Assert.Equal("error message", ex.Message);
                Assert.Equal(HttpStatusCode.Unauthorized, ex.Response.StatusCode);
                Assert.Contains("donkey", ex.Response.Content.ReadAsStringAsync().Result);
                Assert.StartsWith(mobileAppUriValidator.TableBaseUri, ex.Request.RequestUri.ToString());
                Assert.Equal("YOU SHALL NOT PASS.", ex.Response.ReasonPhrase);
            }

            // If no error message in the response, we'll use the
            // StatusDescription instead
            hijack.Response = new HttpResponseMessage(HttpStatusCode.Unauthorized);
            hijack.Response.Content = new StringContent("{\"error\":\"error message\",\"other\":\"donkey\"}", Encoding.UTF8, "application/json");
            hijack.Response.ReasonPhrase = "YOU SHALL NOT PASS.";
            hijack.SetResponseContent("{\"other\":\"donkey\"}");

            try
            {
                JToken response = await service.GetTable(collection).ReadAsync(query);
            }
            catch (InvalidOperationException ex)
            {
                Assert.Equal("The request could not be completed.  (YOU SHALL NOT PASS.)", ex.Message);
            }
        }

        [Fact]
        public async Task DeleteAsync_Throws_WhenSyncContextIsNotInitialized()
        {
            var service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp);
            var table = service.GetSyncTable<ToDoWithSystemPropertiesType>();
            await Assert.ThrowsAsync<InvalidOperationException>(() => table.DeleteAsync(new ToDoWithSystemPropertiesType("abc")));
        }

        [Fact]
        public async Task InsertAsync_Throws_WhenSyncContextIsNotInitialized()
        {
            var service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp);
            var table = service.GetSyncTable<ToDoWithSystemPropertiesType>();
            await Assert.ThrowsAsync<InvalidOperationException>(() => table.InsertAsync(new ToDoWithSystemPropertiesType("abc")));
        }

        [Fact]
        public async Task UpdateAsync_Throws_WhenSyncContextIsNotInitialized()
        {
            var service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp);
            var table = service.GetSyncTable<ToDoWithSystemPropertiesType>();
            await Assert.ThrowsAsync<InvalidOperationException>(() => table.UpdateAsync(new ToDoWithSystemPropertiesType("abc")));
        }

        [Fact]
        public async Task LookupAsync_Throws_WhenSyncContextIsNotInitialized()
        {
            var service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp);
            var table = service.GetSyncTable<ToDoWithSystemPropertiesType>();
            await Assert.ThrowsAsync<InvalidOperationException>(() => table.LookupAsync("abc"));
        }

        [Fact]
        public async Task ReadAsync_Throws_WhenSyncContextIsNotInitialized()
        {
            var service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp);
            var table = service.GetSyncTable<ToDoWithSystemPropertiesType>();
            await Assert.ThrowsAsync<InvalidOperationException>(() => table.Where(t => t.String == "abc").ToListAsync());
        }

        [Fact]
        public async Task PurgeAsync_Throws_WhenSyncContextIsNotInitialized()
        {
            var service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp);
            var table = service.GetSyncTable<ToDoWithSystemPropertiesType>();
            await Assert.ThrowsAsync<InvalidOperationException>(() => table.PurgeAsync(table.Where(t => t.String == "abc")));
        }

        [Fact]
        public async Task PullAsync_Throws_WhenSyncContextIsNotInitialized()
        {
            var service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp);
            var table = service.GetSyncTable<ToDoWithSystemPropertiesType>();
            await Assert.ThrowsAsync<InvalidOperationException>(() => table.PullAsync(null, table.Where(t => t.String == "abc")));
        }

        [Fact]
        public void GetTableThrowsWithNullTable()
        {
            MobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp);
            Assert.Throws<ArgumentNullException>(() => service.GetTable(null));
        }

        [Fact]
        public void GetTableThrowsWithEmptyStringTable()
        {
            MobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp);
            Assert.Throws<ArgumentException>(() => service.GetTable(""));
        }

        [Fact]
        public async Task InvokeCustomApiThrowsForNullApiName()
        {
            MobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp);
            await Assert.ThrowsAsync<ArgumentNullException>(() => service.InvokeApiAsync(null, null));
        }

        [Fact]
        public async Task InvokeCustomApiThrowsForEmptyApiName()
        {
            MobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp);
            await Assert.ThrowsAsync<ArgumentException>(() => service.InvokeApiAsync("", null));
        }

        [Fact]
        public async Task InvokeCustomAPISimple()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            MobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);
            MobileAppUriValidator mobileAppUriValidator = new MobileAppUriValidator(service);

            hijack.SetResponseContent("{\"id\":3}");

            IntType expected = await service.InvokeApiAsync<IntType>("calculator/add?a=1&b=2");

            Assert.Equal(mobileAppUriValidator.GetApiUriPath("calculator/add"), hijack.Request.RequestUri.LocalPath);
            Assert.Equal("?a=1&b=2", hijack.Request.RequestUri.Query);
            Assert.Equal(3, expected.Id);
        }

        [Fact]
        public async Task InvokeCustomAPISimpleJToken()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            hijack.SetResponseContent("{\"id\":3}");
            MobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);
            MobileAppUriValidator mobileAppUriValidator = new MobileAppUriValidator(service);

            JToken expected = await service.InvokeApiAsync("calculator/add?a=1&b=2");

            Assert.Equal(mobileAppUriValidator.GetApiUriPath("calculator/add"), hijack.Request.RequestUri.LocalPath);
            Assert.Equal("?a=1&b=2", hijack.Request.RequestUri.Query);
            Assert.Equal(3, (int)expected["id"]);
        }

        [Fact]
        public async Task InvokeCustomAPIPost()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            hijack.SetResponseContent("{\"id\":3}");
            MobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);

            var body = "{\"test\" : \"one\"}";
            IntType expected = await service.InvokeApiAsync<string, IntType>("calculator/add", body);
            Assert.NotNull(hijack.Request.Content);
        }

        [Fact]
        public async Task InvokeCustomAPIPostJToken()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            hijack.SetResponseContent("{\"id\":3}");
            MobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);

            JObject body = JToken.Parse("{\"test\":\"one\"}") as JObject;
            JToken expected = await service.InvokeApiAsync("calculator/add", body);
            Assert.NotNull(hijack.Request.Content);
        }

        [Fact]
        public async Task InvokeCustomAPIPostJTokenBooleanPrimative()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            hijack.SetResponseContent("{\"id\":3}");
            MobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);
            hijack.OnSendingRequest = async request =>
            {
                string content = await request.Content.ReadAsStringAsync();
                Assert.Equal("true", content);
                return request;
            };

            JToken expected = await service.InvokeApiAsync("calculator/add", new JValue(true));
        }

        [Fact]
        public async Task InvokeCustomAPIPostJTokenNullPrimative()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            hijack.SetResponseContent("{\"id\":3}");
            MobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);
            hijack.OnSendingRequest = async request =>
            {
                string content = await request.Content.ReadAsStringAsync();
                Assert.Equal("null", content);
                return request;
            };

            JToken expected = await service.InvokeApiAsync("calculator/add", new JValue((object)null));
        }


        [Fact]
        public async Task InvokeCustomAPIGet()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            MobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);
            MobileAppUriValidator mobileAppUriValidator = new MobileAppUriValidator(service);

            hijack.SetResponseContent("{\"id\":3}");

            IntType expected = await service.InvokeApiAsync<IntType>("calculator/add?a=1&b=2", HttpMethod.Get, null);

            Assert.Equal(mobileAppUriValidator.GetApiUriPath("calculator/add"), hijack.Request.RequestUri.LocalPath);
            Assert.Equal("?a=1&b=2", hijack.Request.RequestUri.Query);
            Assert.Equal(3, expected.Id);
        }

        [Fact]
        public async Task InvokeApiAsync_DoesNotAppendApiPath_IfApiStartsWithSlash()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            var service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);
            hijack.SetResponseContent("{\"id\":3}");

            await service.InvokeApiAsync<IntType>("/calculator/add?a=1&b=2", HttpMethod.Get, null);
            Assert.Equal("?a=1&b=2", hijack.Request.RequestUri.Query);
        }

        [Fact]
        public async Task InvokeCustomAPIGetJToken()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            hijack.SetResponseContent("{\"id\":3}");
            MobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);
            MobileAppUriValidator mobileAppUriValidator = new MobileAppUriValidator(service);

            JToken expected = await service.InvokeApiAsync("calculator/add?a=1&b=2", HttpMethod.Get, null);

            Assert.Equal(mobileAppUriValidator.GetApiUriPath("calculator/add"), hijack.Request.RequestUri.LocalPath);
            Assert.Equal("?a=1&b=2", hijack.Request.RequestUri.Query);
            Assert.Equal(3, (int)expected["id"]);
        }

        [Fact]
        public async Task InvokeCustomAPIGetWithParams()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            hijack.SetResponseContent("{\"id\":3}");
            MobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);

            var myParams = new Dictionary<string, string>() { { "a", "1" }, { "b", "2" } };
            IntType expected = await service.InvokeApiAsync<IntType>("calculator/add", HttpMethod.Get, myParams);
            Assert.Equal("?a=1&b=2", hijack.Request.RequestUri.Query);
        }

        [Fact]
        public async Task InvokeCustomAPIGetWithParamsJToken()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            hijack.SetResponseContent("{\"id\":3}");
            MobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);

            var myParams = new Dictionary<string, string>() { { "a", "1" }, { "b", "2" } };
            JToken expected = await service.InvokeApiAsync("calculator/add", HttpMethod.Get, myParams);
            Assert.Equal("?a=1&b=2", hijack.Request.RequestUri.Query);
        }

        [Fact]
        public async Task InvokeCustomAPIGetWithODataParams()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            hijack.SetResponseContent("{\"id\":3}");
            MobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);

            var myParams = new Dictionary<string, string>() { { "$select", "one,two" }, { "$take", "1" } };
            IntType expected = await service.InvokeApiAsync<IntType>("calculator/add", HttpMethod.Get, myParams);

            Assert.Contains("?%24select=one%2Ctwo&%24take=1", hijack.Request.RequestUri.Query);
        }

        [Fact]
        public async Task InvokeCustomAPIGetWithODataParamsJToken()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            hijack.SetResponseContent("{\"id\":3}");
            MobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);

            var myParams = new Dictionary<string, string>() { { "$select", "one,two" } };
            JToken expected = await service.InvokeApiAsync("calculator/add", HttpMethod.Get, myParams);
            Assert.Contains("?%24select=one%2Ctwo", hijack.Request.RequestUri.Query);
        }

        [Fact]
        public async Task InvokeCustomAPIPostWithBody()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            hijack.SetResponseContent("{\"id\":3}");
            MobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);

            var myParams = new Dictionary<string, string>() { { "a", "1" }, { "b", "2" } };
            var body = "{\"test\" : \"one\"}";
            IntType expected = await service.InvokeApiAsync<string, IntType>("calculator/add", body, HttpMethod.Post, myParams);

            Assert.Equal("?a=1&b=2", hijack.Request.RequestUri.Query);
            Assert.NotNull(hijack.Request.Content);
        }

        [Fact]
        public async Task InvokeCustomAPIPostWithBodyJToken()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            hijack.SetResponseContent("{\"id\":3}");
            MobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);

            var myParams = new Dictionary<string, string>() { { "a", "1" }, { "b", "2" } };
            JObject body = JToken.Parse("{\"test\":\"one\"}") as JObject;
            JToken expected = await service.InvokeApiAsync("calculator/add", body, HttpMethod.Post, myParams);

            Assert.Equal("?a=1&b=2", hijack.Request.RequestUri.Query);
            Assert.NotNull(hijack.Request.Content);
        }

        [Fact]
        public async Task InvokeCustomAPIResponse()
        {
            TestHttpHandler hijack = new TestHttpHandler();

            hijack.Response = new HttpResponseMessage(System.Net.HttpStatusCode.Accepted);
            hijack.Response.Content = new StringContent("{\"id\":\"2\"}", Encoding.UTF8, "application/json");

            MobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);
            MobileAppUriValidator mobileAppUriValidator = new MobileAppUriValidator(service);
            HttpResponseMessage response = await service.InvokeApiAsync("calculator/add?a=1&b=2", null, HttpMethod.Post, null, null);

            Assert.Equal(mobileAppUriValidator.GetApiUriPath("calculator/add"), hijack.Request.RequestUri.LocalPath);
            Assert.Equal("?a=1&b=2", hijack.Request.RequestUri.Query);
            Assert.Contains("{\"id\":\"2\"}", response.Content.ReadAsStringAsync().Result);
        }

        [Fact]
        public async Task InvokeCustomAPIResponseWithParams()
        {
            TestHttpHandler hijack = new TestHttpHandler();

            hijack.Response = new HttpResponseMessage(System.Net.HttpStatusCode.Accepted);
            hijack.Response.Content = new StringContent("{\"id\":\"2\"}", Encoding.UTF8, "application/json");
            var myParams = new Dictionary<string, string>() { { "a", "1" }, { "b", "2" } };

            MobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);
            MobileAppUriValidator mobileAppUriValidator = new MobileAppUriValidator(service);

            HttpResponseMessage response = await service.InvokeApiAsync("calculator/add", null, HttpMethod.Post, null, myParams);

            Assert.Equal(mobileAppUriValidator.GetApiUriPath("calculator/add"), hijack.Request.RequestUri.LocalPath);
            Assert.Equal("?a=1&b=2", hijack.Request.RequestUri.Query);
            Assert.Null(hijack.Request.Content);
            Assert.Contains("{\"id\":\"2\"}", response.Content.ReadAsStringAsync().Result);
        }

        [Fact]
        public async Task InvokeCustomAPIResponseWithParamsBodyAndHeader()
        {
            TestHttpHandler hijack = new TestHttpHandler();

            hijack.Response = new HttpResponseMessage(System.Net.HttpStatusCode.Accepted);
            hijack.Response.Content = new StringContent("{\"id\":\"2\"}", Encoding.UTF8, "application/json");
            var myParams = new Dictionary<string, string>() { { "a", "1" }, { "b", "2" } };

            HttpContent content = new StringContent("{\"test\" : \"one\"}", Encoding.UTF8, "application/json");
            MobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);
            MobileAppUriValidator mobileAppUriValidator = new MobileAppUriValidator(service);

            var myHeaders = new Dictionary<string, string>() { { "x-zumo-test", "test" } };

            HttpResponseMessage response = await service.InvokeApiAsync("calculator/add", content, HttpMethod.Post, myHeaders, myParams);

            Assert.Single(myHeaders);
            Assert.Equal("test", myHeaders["x-zumo-test"]);
            Assert.Equal(mobileAppUriValidator.GetApiUriPath("calculator/add"), hijack.Request.RequestUri.LocalPath);
            Assert.Equal("test", hijack.Request.Headers.GetValues("x-zumo-test").First());
            Assert.NotNull(hijack.Request.Content);
            Assert.Equal("?a=1&b=2", hijack.Request.RequestUri.Query);
            Assert.Contains("{\"id\":\"2\"}", response.Content.ReadAsStringAsync().Result);
        }

        [Fact]
        public async Task InvokeCustomAPIWithEmptyStringResponse_Success()
        {
            TestHttpHandler hijack = new TestHttpHandler();

            hijack.Response = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
            hijack.Response.Content = new StringContent("", Encoding.UTF8, "application/json");

            MobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);
            MobileAppUriValidator mobileAppUriValidator = new MobileAppUriValidator(service);

            JToken expected = await service.InvokeApiAsync("testapi");
            Assert.Equal(mobileAppUriValidator.GetApiUriPath("testapi"), hijack.Request.RequestUri.LocalPath);
            Assert.Null(expected);
        }

        [Fact]
        public async Task InvokeGenericCustomAPIWithNullResponse_Success()
        {
            TestHttpHandler hijack = new TestHttpHandler();

            hijack.Response = new HttpResponseMessage(HttpStatusCode.OK);
            hijack.Response.Content = null;

            MobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);
            MobileAppUriValidator mobileAppUriValidator = new MobileAppUriValidator(service);

            IntType expected = await service.InvokeApiAsync<IntType>("testapi");
            Assert.Equal(mobileAppUriValidator.GetApiUriPath("testapi"), hijack.Request.RequestUri.LocalPath);
            Assert.Null(expected);
        }

        [Fact]
        public async Task InvokeCustomAPI_ErrorWithJsonObject()
        {
            TestHttpHandler hijack = new TestHttpHandler();

            hijack.Response = new HttpResponseMessage(HttpStatusCode.BadRequest);
            hijack.Response.Content = new StringContent("{ error: \"message\"}", Encoding.UTF8, "application/json");

            MobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);
            Exception exception = await Assert.ThrowsAnyAsync<Exception>(async () => await service.InvokeApiAsync("testapi"));
            Assert.Equal("message", exception.Message);
        }

        [Fact]
        public async Task InvokeCustomAPI_ErrorWithJsonString()
        {
            TestHttpHandler hijack = new TestHttpHandler();

            hijack.Response = new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest);
            hijack.Response.Content = new StringContent("\"message\"", Encoding.UTF8, "application/json");

            MobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);
            Exception exception = await Assert.ThrowsAnyAsync<Exception>(async () => await service.InvokeApiAsync("testapi"));
            Assert.Equal("message", exception.Message);
        }

        [Fact]
        public async Task InvokeCustomAPI_ErrorWithString()
        {
            TestHttpHandler hijack = new TestHttpHandler();

            hijack.Response = new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest);
            hijack.Response.Content = new StringContent("message", Encoding.UTF8, "text/html");

            MobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);
            Exception exception = await Assert.ThrowsAnyAsync<Exception>(async () => await service.InvokeApiAsync("testapi"));
            Assert.Equal("message", exception.Message);
        }

        [Fact]
        public async Task InvokeCustomAPI_ErrorStringAndNoContentType()
        {
            TestHttpHandler hijack = new TestHttpHandler();

            hijack.Response = new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest);
            hijack.Response.Content = new StringContent("message", Encoding.UTF8, null);
            hijack.Response.Content.Headers.ContentType = null;

            MobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);
            await Assert.ThrowsAnyAsync<Exception>(() => service.InvokeApiAsync("testapi"));
        }

        [Fact]
        public async Task InvokeApiJsonOverloads_HasCorrectFeaturesHeader()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            hijack.OnSendingRequest = (request) =>
            {
                Assert.Equal("AJ", request.Headers.GetValues("X-ZUMO-FEATURES").First());
                return Task.FromResult(request);
            };

            MobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);

            hijack.SetResponseContent("{\"hello\":\"world\"}");
            await service.InvokeApiAsync("apiName");

            hijack.SetResponseContent("{\"hello\":\"world\"}");
            await service.InvokeApiAsync("apiName", JObject.Parse("{\"a\":1}"));

            hijack.OnSendingRequest = (request) =>
            {
                Assert.Equal("AJ,QS", request.Headers.GetValues("X-ZUMO-FEATURES").First());
                return Task.FromResult(request);
            };

            var dic = new Dictionary<string, string> { { "a", "b" } };
            hijack.SetResponseContent("{\"hello\":\"world\"}");
            await service.InvokeApiAsync("apiName", HttpMethod.Get, dic);

            hijack.SetResponseContent("{\"hello\":\"world\"}");
            await service.InvokeApiAsync("apiName", null, HttpMethod.Delete, dic);
        }

        [Fact]
        public async Task InvokeApiTypedOverloads_HasCorrectFeaturesHeader()
        {
            TestHttpHandler hijack = new TestHttpHandler();
            hijack.OnSendingRequest = (request) =>
            {
                Assert.Equal("AT", request.Headers.GetValues("X-ZUMO-FEATURES").First());
                return Task.FromResult(request);
            };

            MobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp, hijack);

            hijack.SetResponseContent("{\"id\":3}");
            await service.InvokeApiAsync<IntType>("apiName");

            hijack.SetResponseContent("{\"id\":3}");
            await service.InvokeApiAsync<IntType, IntType>("apiName", new IntType { Id = 1 });

            hijack.OnSendingRequest = (request) =>
            {
                Assert.Equal("AT,QS", request.Headers.GetValues("X-ZUMO-FEATURES").First());
                return Task.FromResult(request);
            };

            var dic = new Dictionary<string, string> { { "a", "b" } };
            hijack.SetResponseContent("{\"id\":3}");
            await service.InvokeApiAsync<IntType>("apiName", HttpMethod.Get, dic);

            hijack.SetResponseContent("{\"hello\":\"world\"}");
            await service.InvokeApiAsync<IntType, IntType>("apiName", new IntType { Id = 1 }, HttpMethod.Put, dic);
        }

        [Fact]
        public Task FeatureHeaderValidation_InvokeApi_String()
            => ValidateFeaturesHeader("AJ", c => c.InvokeApiAsync("apiName"));

        [Fact]
        public Task FeatureHeaderValidation_InvokeApi_String_JToken()
            => ValidateFeaturesHeader("AJ", c => c.InvokeApiAsync("apiName", JObject.Parse("{\"id\":1}")));

        [Fact]
        public Task FeatureHeaderValidation_InvokeApi_String_HttpMethod_Dict()
            => ValidateFeaturesHeader("AJ,QS", c => c.InvokeApiAsync("apiName", null, HttpMethod.Get, new Dictionary<string, string> { { "a", "b" } }));

        [Fact]
        public Task FeatureHeaderValidation_InvokeApi_String_JToken_HttpMethod_Dict()
            => ValidateFeaturesHeader("AJ,QS", c => c.InvokeApiAsync("apiName", JObject.Parse("{\"id\":1}"), HttpMethod.Put, new Dictionary<string, string> { { "a", "b" } }));

        [Fact]
        public Task FeatureHeaderValidation_InvokeApi_String_HttpContent_NoQueryParams()
            => ValidateFeaturesHeader("AG", c => c.InvokeApiAsync("apiName", new StringContent("hello world", Encoding.UTF8, "text/plain"), HttpMethod.Post, null, null));

        [Fact]
        public Task FeatureHeaderValidation_InvokeApi_String_HttpContent_WithQueryParams()
            => ValidateFeaturesHeader("AG", c => c.InvokeApiAsync("apiName", new StringContent("hello world", Encoding.UTF8, "text/plain"), HttpMethod.Post, null, new Dictionary<string, string> { { "a", "b" } }));

        [Fact]
        public Task FeatureHeaderValidation_TypedInvokeApi_String()
            => ValidateFeaturesHeader("AT", c => c.InvokeApiAsync<IntType>("apiName"));

        [Fact]
        public Task FeatureHeaderValidation_TypedInvokeApi_String_HttpMethod_Dict()
            => ValidateFeaturesHeader("AT,QS", c => c.InvokeApiAsync<IntType>("apiName", HttpMethod.Get, new Dictionary<string, string> { { "a", "b" } }));

        [Fact]
        public Task FeatureHeaderValidation_TypedInvokeApi_String_T()
            => ValidateFeaturesHeader("AT", c => c.InvokeApiAsync<IntType, IntType>("apiName", new IntType { Id = 1 }));

        [Fact]
        public Task FeatureHeaderValidation_TypedInvokeApi_String_T_HttpMethod_Dict()
            => ValidateFeaturesHeader("AT,QS", c => c.InvokeApiAsync<IntType, IntType>("apiName", new IntType { Id = 1 }, HttpMethod.Get, new Dictionary<string, string> { { "a", "b" } }));

        [Fact]
        public async Task LoginAsync_UserEnumTypeProvider()
        {
            JObject token = new JObject();
            string appUrl = MobileAppUriValidator.DummyMobileApp;
            MobileServiceAuthenticationProvider provider = MobileServiceAuthenticationProvider.MicrosoftAccount;
            string expectedUri = appUrl + ".auth/login/microsoftaccount";

            TestHttpHandler hijack = new TestHttpHandler();
            hijack.Response = new HttpResponseMessage(HttpStatusCode.OK);

            MobileServiceHttpClient.DefaultHandlerFactory = () => hijack;
            MobileServiceClient client = new MobileServiceClient(appUrl, hijack);

            await client.LoginAsync(provider, token);

            Assert.Equal(expectedUri, hijack.Request.RequestUri.OriginalString);
        }

        [Fact]
        public async Task LoginAsync_UserStringTypeProvider()
        {
            JObject token = new JObject();
            string appUrl = MobileAppUriValidator.DummyMobileApp;
            MobileServiceAuthenticationProvider provider = MobileServiceAuthenticationProvider.MicrosoftAccount;
            string expectedUri = appUrl + ".auth/login/microsoftaccount";

            TestHttpHandler hijack = new TestHttpHandler();
            hijack.Response = new HttpResponseMessage(HttpStatusCode.OK);

            MobileServiceHttpClient.DefaultHandlerFactory = () => hijack;
            MobileServiceClient client = new MobileServiceClient(appUrl, hijack);

            await client.LoginAsync(provider.ToString(), token);

            Assert.Equal(expectedUri, hijack.Request.RequestUri.OriginalString);
        }

        [Fact]
        public async Task RefreshUserAsync_Throws_WhenMobileServiceUserIsNotSet()
        {
            string appUrl = MobileAppUriValidator.DummyMobileApp;

            TestHttpHandler hijack = new TestHttpHandler();

            MobileServiceHttpClient.DefaultHandlerFactory = () => hijack;
            MobileServiceClient client = new MobileServiceClient(appUrl, hijack);
            client.CurrentUser = null;
            await Assert.ThrowsAsync<InvalidOperationException>(() => client.RefreshUserAsync());
        }

        [Fact]
        public async Task RefreshUserAsync_Throws_WhenMobileServiceUserHasNoAuthToken()
        {
            string appUrl = MobileAppUriValidator.DummyMobileApp;
            string userId = "sid:xxxxxxxxxxxxxxxxx";

            TestHttpHandler hijack = new TestHttpHandler();

            MobileServiceHttpClient.DefaultHandlerFactory = () => hijack;
            MobileServiceClient client = new MobileServiceClient(appUrl, hijack);
            client.CurrentUser = new MobileServiceUser(userId);
            await Assert.ThrowsAsync<InvalidOperationException>(() => client.RefreshUserAsync());
        }

        [Fact]
        public async Task RefreshUserAsync_NonAlternateLoginUri()
        {
            await RefreshUserAsync_Setup();
        }

        [Fact]
        public async Task RefreshUserAsync_AlternateLoginUri()
        {
            await RefreshUserAsync_Setup(alternateLoginUri: "https://www.testalternatelogin.com/");
        }

        private static async Task RefreshUserAsync_Setup(string alternateLoginUri = null)
        {
            string appUrl = MobileAppUriValidator.DummyMobileApp;
            string newAuthToken = "new-auth-token";
            string userId = "sid:xxxxxxxxxxxxxxxxx";
            string responseContent = "{\"authenticationToken\":\"" + newAuthToken + "\",\"user\":{\"userId\":\"" + userId + "\"}}";

            TestHttpHandler hijack = new TestHttpHandler();
            hijack.Response = new HttpResponseMessage(HttpStatusCode.OK);
            hijack.Response.Content = new StringContent(responseContent);

            MobileServiceHttpClient.DefaultHandlerFactory = () => hijack;
            MobileServiceClient client = new MobileServiceClient(appUrl, hijack);
            client.CurrentUser = new MobileServiceUser(userId)
            {
                MobileServiceAuthenticationToken = "auth-token"
            };

            string refreshUrl;
            if (!string.IsNullOrEmpty(alternateLoginUri))
            {
                refreshUrl = alternateLoginUri + ".auth/refresh";
                client.AlternateLoginHost = new Uri(alternateLoginUri);
            }
            else
            {
                refreshUrl = appUrl + ".auth/refresh";
            }
            MobileServiceUser user = await client.RefreshUserAsync();

            Assert.Equal(EnumValueAttribute.GetValue(MobileServiceFeatures.RefreshToken),
                hijack.Request.Headers.GetValues(MobileServiceHttpClient.ZumoFeaturesHeader).FirstOrDefault());
            Assert.Equal(hijack.Request.RequestUri.OriginalString, refreshUrl);
            Assert.Equal(newAuthToken, user.MobileServiceAuthenticationToken);
            Assert.Equal(userId, user.UserId);
        }

        [Fact]
        public async Task RefreshUserAsync_Throws_On_BadRequest()
        {
            string appUrl = MobileAppUriValidator.DummyMobileApp;
            string refreshUrl = appUrl + ".auth/refresh";
            string userId = "sid:xxxxxxxxxxxxxxxxx";

            TestHttpHandler hijack = new TestHttpHandler();
            hijack.Response = TestHttpHandler.CreateResponse("error message from Mobile Apps refresh endpoint", HttpStatusCode.BadRequest);

            MobileServiceHttpClient.DefaultHandlerFactory = () => hijack;
            MobileServiceClient client = new MobileServiceClient(appUrl, hijack);
            client.CurrentUser = new MobileServiceUser(userId)
            {
                MobileServiceAuthenticationToken = "auth-token"
            };

            await Assert.ThrowsAsync<MobileServiceInvalidOperationException>(() => client.RefreshUserAsync());
        }

        [Fact]
        public async Task RefreshUserAsync_Throws_On_Unauthorized()
        {
            string appUrl = MobileAppUriValidator.DummyMobileApp;
            string refreshUrl = appUrl + ".auth/refresh";
            string userId = "sid:xxxxxxxxxxxxxxxxx";

            TestHttpHandler hijack = new TestHttpHandler();
            hijack.Response = TestHttpHandler.CreateResponse("error message from Mobile Apps refresh endpoint", HttpStatusCode.Unauthorized);

            MobileServiceHttpClient.DefaultHandlerFactory = () => hijack;
            MobileServiceClient client = new MobileServiceClient(appUrl, hijack);
            client.CurrentUser = new MobileServiceUser(userId)
            {
                MobileServiceAuthenticationToken = "auth-token"
            };

            await Assert.ThrowsAsync<MobileServiceInvalidOperationException>(() => client.RefreshUserAsync());
        }

        [Fact]
        public async Task RefreshUserAsync_Throws_On_Forbidden()
        {
            string appUrl = MobileAppUriValidator.DummyMobileApp;
            string refreshUrl = appUrl + ".auth/refresh";
            string userId = "sid:xxxxxxxxxxxxxxxxx";

            TestHttpHandler hijack = new TestHttpHandler();
            hijack.Response = TestHttpHandler.CreateResponse("error message from Mobile Apps refresh endpoint", HttpStatusCode.Forbidden);

            MobileServiceHttpClient.DefaultHandlerFactory = () => hijack;
            MobileServiceClient client = new MobileServiceClient(appUrl, hijack);
            client.CurrentUser = new MobileServiceUser(userId)
            {
                MobileServiceAuthenticationToken = "auth-token"
            };

            await Assert.ThrowsAsync<MobileServiceInvalidOperationException>(() => client.RefreshUserAsync());
        }

        [Fact]
        public async Task RefreshUserAsync_Throws_On_InternalServerError()
        {
            string appUrl = MobileAppUriValidator.DummyMobileApp;
            string refreshUrl = appUrl + ".auth/refresh";
            string userId = "sid:xxxxxxxxxxxxxxxxx";

            TestHttpHandler hijack = new TestHttpHandler();
            hijack.Response = TestHttpHandler.CreateResponse("error message from Mobile Apps refresh endpoint", HttpStatusCode.InternalServerError);

            MobileServiceHttpClient.DefaultHandlerFactory = () => hijack;
            MobileServiceClient client = new MobileServiceClient(appUrl, hijack);
            client.CurrentUser = new MobileServiceUser(userId)
            {
                MobileServiceAuthenticationToken = "auth-token"
            };

            await Assert.ThrowsAsync<MobileServiceInvalidOperationException>(() => client.RefreshUserAsync());
        }

        [Fact]
        public void AddFeaturesHeader_RequestHeadersIsNull()
        {
            IDictionary<string, string> requestedHeaders = null;
            requestedHeaders = MobileServiceHttpClient.FeaturesHelper.AddFeaturesHeader(requestHeaders: requestedHeaders, features: MobileServiceFeatures.None);
            Assert.Null(requestedHeaders);

            requestedHeaders = null;
            requestedHeaders = MobileServiceHttpClient.FeaturesHelper.AddFeaturesHeader(requestHeaders: requestedHeaders, features: MobileServiceFeatures.RefreshToken);
            Assert.Equal(EnumValueAttribute.GetValue(MobileServiceFeatures.RefreshToken), requestedHeaders[MobileServiceHttpClient.ZumoFeaturesHeader]);
        }

        [Fact]
        public void AddFeaturesHeader_RequestHeadersIsNotNull()
        {
            IDictionary<string, string> requestedHeaders = new Dictionary<string, string>();
            requestedHeaders.Add("key1", "value1");
            requestedHeaders.Add("key2", "value2");
            Assert.Equal(2, requestedHeaders.Count);

            requestedHeaders = MobileServiceHttpClient.FeaturesHelper.AddFeaturesHeader(requestHeaders: requestedHeaders, features: MobileServiceFeatures.None);
            Assert.Equal(2, requestedHeaders.Count);

            requestedHeaders = MobileServiceHttpClient.FeaturesHelper.AddFeaturesHeader(requestHeaders: requestedHeaders, features: MobileServiceFeatures.RefreshToken | MobileServiceFeatures.Offline);
            Assert.Equal(3, requestedHeaders.Count);
            Assert.Equal("OL,RT", requestedHeaders[MobileServiceHttpClient.ZumoFeaturesHeader]);
        }

        [Fact]
        public void AddFeaturesHeader_ZumoFeaturesHeaderAlreadyExists()
        {
            IDictionary<string, string> requestedHeaders = new Dictionary<string, string>();
            requestedHeaders.Add("key1", "value1");
            requestedHeaders.Add("key2", "value2");
            requestedHeaders.Add(MobileServiceHttpClient.ZumoFeaturesHeader, EnumValueAttribute.GetValue(MobileServiceFeatures.RefreshToken));
            Assert.Equal(3, requestedHeaders.Count);

            // AddFeaturesHeader won't add anything because ZumoFeaturesHeader already exists
            requestedHeaders = MobileServiceHttpClient.FeaturesHelper.AddFeaturesHeader(requestHeaders: requestedHeaders, features: MobileServiceFeatures.Offline);
            Assert.Equal(3, requestedHeaders.Count);
            Assert.Equal(EnumValueAttribute.GetValue(MobileServiceFeatures.RefreshToken), requestedHeaders[MobileServiceHttpClient.ZumoFeaturesHeader]);
        }

        private async Task ValidateFeaturesHeader(string expectedFeaturesHeader, Func<IMobileServiceClient, Task> operation)
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

            hijack.SetResponseContent("{\"id\":3}");
            await operation(service);
            Assert.True(validationDone);
        }

    }
}
