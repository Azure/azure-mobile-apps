// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Test;
using Datasync.Common.Test.Mocks;
using Microsoft.Datasync.Client.Http;
using Microsoft.Datasync.Client.Test.Helpers;
using Microsoft.Datasync.Client.Utils;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Datasync.Client.Test.Http
{
    [ExcludeFromCodeCoverage]
    public class ServiceHttpClient_Tests : BaseTest
    {
        [Fact]
        [Trait("Method", "Ctor(Uri, ")]
        public void Ctor_NullEndpoint_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new WrappedHttpClient(null, new DatasyncClientOptions()));
        }

        [Fact]
        [Trait("Method", "Ctor")]
        public void Ctor_NullOptions_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new WrappedHttpClient(Endpoint, null));
        }

        [Theory]
        [InlineData("", false)]
        [InlineData("", true)]
        [InlineData("http://", false)]
        [InlineData("http://", true)]
        [InlineData("file://localhost/foo", false)]
        [InlineData("http://foo.azurewebsites.net", false)]
        [InlineData("http://foo.azure-api.net", false)]
        [InlineData("http://[2001:db8:0:b:0:0:0:1A]", false)]
        [InlineData("http://[2001:db8:0:b:0:0:0:1A]:3000", false)]
        [InlineData("http://[2001:db8:0:b:0:0:0:1A]:3000/myapi", false)]
        [InlineData("http://10.0.0.8", false)]
        [InlineData("http://10.0.0.8:3000", false)]
        [InlineData("http://10.0.0.8:3000/myapi", false)]
        [InlineData("foo/bar", true)]
        [Trait("Method", "Ctor")]
        public void Ctor_InvalidEndpoint_Throws(string endpoint, bool isRelative = false)
        {
            var options = new DatasyncClientOptions();
            Assert.Throws<UriFormatException>(() => new WrappedHttpClient(isRelative ? new Uri(endpoint, UriKind.Relative) : new Uri(endpoint), options));
        }

        [Theory, ClassData(typeof(EndpointTestCases))]
        [Trait("Method", "Ctor")]
        public void Ctor_ValidEndpoint_CreatesClient(EndpointTestCase testcase)
        {
            var options = new DatasyncClientOptions();
            var client = new WrappedHttpClient(new Uri(testcase.NormalizedEndpoint), options);
            Assert.Equal(testcase.NormalizedEndpoint, client.WrappedEndpoint);
            Assert.NotNull(client.HttpHandler);
            Assert.NotNull(client.HttpClient);

            // DefaultRequestHeaders must contain the list of headers
            Assert.StartsWith("Datasync/", client.HttpClient.DefaultRequestHeaders.UserAgent.ToString());
            Assert.Equal(client.HttpClient.DefaultRequestHeaders.UserAgent.ToString(),
                client.HttpClient.DefaultRequestHeaders.GetValues("X-ZUMO-VERSION").FirstOrDefault());
            AssertEx.HasHeader(client.HttpClient.DefaultRequestHeaders, "ZUMO-API-VERSION", "3.0.0");
            AssertEx.HasHeader(client.HttpClient.DefaultRequestHeaders, "X-ZUMO-INSTALLATION-ID", Platform.InstallationId);
            Assert.Equal(testcase.NormalizedEndpoint, client.HttpClient.BaseAddress.ToString());
        }

        [Theory, ClassData(typeof(EndpointTestCases))]
        [Trait("Method", "Ctor")]
        public void Ctor_ValidEndpoint_CustomOptions_CreatesClient(EndpointTestCase testcase)
        {
            var options = new DatasyncClientOptions { HttpPipeline = null, InstallationId = "test-int-id", UserAgent = "test-user-agent" };
            var client = new WrappedHttpClient(new Uri(testcase.NormalizedEndpoint), options);
            Assert.Equal(testcase.NormalizedEndpoint, client.WrappedEndpoint);
            Assert.NotNull(client.HttpHandler);
            Assert.NotNull(client.HttpClient);

            // DefaultRequestHeaders must contain the list of headers
            Assert.Equal("test-user-agent", client.HttpClient.DefaultRequestHeaders.UserAgent.ToString());
            AssertEx.HasHeader(client.HttpClient.DefaultRequestHeaders, "X-ZUMO-VERSION", "test-user-agent");
            AssertEx.HasHeader(client.HttpClient.DefaultRequestHeaders, "ZUMO-API-VERSION", "3.0.0");
            AssertEx.HasHeader(client.HttpClient.DefaultRequestHeaders, "X-ZUMO-INSTALLATION-ID", "test-int-id");
            Assert.Equal(testcase.NormalizedEndpoint, client.HttpClient.BaseAddress.ToString());
        }

        [Fact]
        [Trait("Method", "Ctor")]
        public void Ctor_WithoutTimeout_SetsDefaultTimeout()
        {
            var options = new DatasyncClientOptions();
            var client = new WrappedHttpClient(new Uri("http://localhost:8000"), options);
            Assert.NotNull(client.HttpClient);
            Assert.Equal(100000, client.HttpClient.Timeout.TotalMilliseconds);
        }

        [Fact]
        [Trait("Method", "Ctor")]
        public void Ctor_WithTimeout_SetsTimeout()
        {
            var options = new DatasyncClientOptions { HttpTimeout = TimeSpan.FromMilliseconds(15000) };
            var client = new WrappedHttpClient(new Uri("http://localhost:8000"), options);
            Assert.NotNull(client.HttpClient);
            Assert.Equal(15000, client.HttpClient.Timeout.TotalMilliseconds);
        }

        [Fact]
        [Trait("Method", "CreatePipeline")]
        public void CreatePipeline_NoHandlers_CreatesPipeline()
        {
            var options = new DatasyncClientOptions { HttpPipeline = Array.Empty<HttpMessageHandler>() };

            // Act
            var client = new WrappedHttpClient(Endpoint, options);

            // Assert
            Assert.IsAssignableFrom<HttpClientHandler>(client.HttpHandler);
        }

        [Fact]
        [Trait("Method", "CreatePipeline")]
        public void CreatePipeline_C_CreatesPipeline()
        {
            var c = new HttpClientHandler();

            var options = new DatasyncClientOptions { HttpPipeline = new HttpMessageHandler[] { c } };

            // Act
            var client = new WrappedHttpClient(Endpoint, options);

            // Assert
            Assert.Same(c, client.HttpHandler);
        }

        [Fact]
        [Trait("Method", "CreatePipeline")]
        public void CreatePipeline_B_CreatesPipeline()
        {
            var b = new MockDelegatingHandler();

            var options = new DatasyncClientOptions { HttpPipeline = new HttpMessageHandler[] { b } };

            // Act
            var client = new WrappedHttpClient(Endpoint, options);

            // Assert
            Assert.Same(b, client.HttpHandler);
            Assert.IsAssignableFrom<HttpClientHandler>(b.InnerHandler);
        }

        [Fact]
        [Trait("Method", "CreatePipeline")]
        public void CreatePipeline_BC_CreatesPipeline()
        {
            var b = new MockDelegatingHandler();
            var c = new HttpClientHandler();

            var options = new DatasyncClientOptions { HttpPipeline = new HttpMessageHandler[] { b, c } };

            // Act
            var client = new WrappedHttpClient(Endpoint, options);

            // Assert
            Assert.Same(b, client.HttpHandler);
            Assert.Same(c, b.InnerHandler);
        }

        [Fact]
        [Trait("Method", "CreatePipeline")]
        public void CreatePipeline_AB_CreatesPipeline()
        {
            var a = new MockDelegatingHandler();
            var b = new MockDelegatingHandler();

            var options = new DatasyncClientOptions { HttpPipeline = new HttpMessageHandler[] { a, b } };

            // Act
            var client = new WrappedHttpClient(Endpoint, options);

            // Assert
            Assert.Same(a, client.HttpHandler);
            Assert.Same(b, a.InnerHandler);
            Assert.IsAssignableFrom<HttpClientHandler>(b.InnerHandler);
        }

        [Fact]
        [Trait("Method", "CreatePipeline")]
        public void CreatePipeline_ABC_CreatesPipeline()
        {
            var a = new MockDelegatingHandler();
            var b = new MockDelegatingHandler();
            var c = new HttpClientHandler();

            var options = new DatasyncClientOptions { HttpPipeline = new HttpMessageHandler[] { a, b, c } };

            // Act
            var client = new WrappedHttpClient(Endpoint, options);

            // Assert
            Assert.Same(a, client.HttpHandler);
            Assert.Same(b, a.InnerHandler);
            Assert.Same(c, b.InnerHandler);
        }

        [Fact]
        [Trait("Method", "CreatePipeline")]
        public void CreatePipeline_CB_ThrowsArgumentException()
        {
            var b = new MockDelegatingHandler();
            var c = new HttpClientHandler();

            var options = new DatasyncClientOptions { HttpPipeline = new HttpMessageHandler[] { c, b } };

            // Act
            Assert.Throws<ArgumentException>(() => _ = new WrappedHttpClient(Endpoint, options));
        }

        [Fact]
        [Trait("Method", "CreatePipeline")]
        public void CreatePipeline_CAB_ThrowsArgumentException()
        {
            var a = new MockDelegatingHandler();
            var b = new MockDelegatingHandler();
            var c = new HttpClientHandler();

            var options = new DatasyncClientOptions { HttpPipeline = new HttpMessageHandler[] { c, a, b } };

            // Act
            Assert.Throws<ArgumentException>(() => _ = new WrappedHttpClient(Endpoint, options));
        }

        [Fact]
        [Trait("Method", "CreatePipeline")]
        public void CreatePipeline_ACB_ThrowsArgumentException()
        {
            var a = new MockDelegatingHandler();
            var b = new MockDelegatingHandler();
            var c = new HttpClientHandler();

            var options = new DatasyncClientOptions { HttpPipeline = new HttpMessageHandler[] { a, c, b } };

            // Act
            Assert.Throws<ArgumentException>(() => _ = new WrappedHttpClient(Endpoint, options));
        }

        [Fact]
        [Trait("Method", "CreatePipeline")]
        public void CreatePipeline_NoHandlersWithAuth_CreatesPipeline()
        {
            var options = new DatasyncClientOptions { HttpPipeline = Array.Empty<HttpMessageHandler>() };

            // Act
            var authProvider = new GenericAuthenticationProvider(() => Task.FromResult(ValidAuthenticationToken), "X-ZUMO-AUTH");
            var client = new WrappedHttpClient(Endpoint, authProvider, options);

            // Assert
            Assert.Same(authProvider, client.HttpHandler);
            Assert.IsAssignableFrom<HttpClientHandler>(authProvider.InnerHandler);
        }

        [Fact]
        [Trait("Method", "CreatePipeline")]
        public void CreatePipeline_CWithAuth_CreatesPipeline()
        {
            var c = new HttpClientHandler();

            var options = new DatasyncClientOptions { HttpPipeline = new HttpMessageHandler[] { c } };

            // Act
            var authProvider = new GenericAuthenticationProvider(() => Task.FromResult(ValidAuthenticationToken), "X-ZUMO-AUTH");
            var client = new WrappedHttpClient(Endpoint, authProvider, options);

            // Assert
            Assert.Same(authProvider, client.HttpHandler);
            Assert.Same(c, authProvider.InnerHandler);
        }

        [Fact]
        [Trait("Method", "CreatePipeline")]
        public void CreatePipeline_BWithAuth_CreatesPipeline()
        {
            var b = new MockDelegatingHandler();

            var options = new DatasyncClientOptions { HttpPipeline = new HttpMessageHandler[] { b } };

            // Act
            var authProvider = new GenericAuthenticationProvider(() => Task.FromResult(ValidAuthenticationToken), "X-ZUMO-AUTH");
            var client = new WrappedHttpClient(Endpoint, authProvider, options);

            // Assert
            Assert.Same(authProvider, client.HttpHandler);
            Assert.Same(b, authProvider.InnerHandler);
            Assert.IsAssignableFrom<HttpClientHandler>(b.InnerHandler);
        }

        [Fact]
        [Trait("Method", "CreatePipeline")]
        public void CreatePipeline_BCWithAuth_CreatesPipeline()
        {
            var b = new MockDelegatingHandler();
            var c = new HttpClientHandler();

            var options = new DatasyncClientOptions { HttpPipeline = new HttpMessageHandler[] { b, c } };

            // Act
            var authProvider = new GenericAuthenticationProvider(() => Task.FromResult(ValidAuthenticationToken), "X-ZUMO-AUTH");
            var client = new WrappedHttpClient(Endpoint, authProvider, options);

            // Assert
            Assert.Same(authProvider, client.HttpHandler);
            Assert.Same(b, authProvider.InnerHandler);
            Assert.Same(c, b.InnerHandler);
        }

        [Fact]
        [Trait("Method", "CreatePipeline")]
        public void CreatePipeline_ABWithAuth_CreatesPipeline()
        {
            var a = new MockDelegatingHandler();
            var b = new MockDelegatingHandler();

            var options = new DatasyncClientOptions { HttpPipeline = new HttpMessageHandler[] { a, b } };

            // Act
            var authProvider = new GenericAuthenticationProvider(() => Task.FromResult(ValidAuthenticationToken), "X-ZUMO-AUTH");
            var client = new WrappedHttpClient(Endpoint, authProvider, options);

            // Assert
            Assert.Same(authProvider, client.HttpHandler);
            Assert.Same(a, authProvider.InnerHandler);
            Assert.Same(b, a.InnerHandler);
            Assert.IsAssignableFrom<HttpClientHandler>(b.InnerHandler);
        }

        [Fact]
        [Trait("Method", "CreatePipeline")]
        public void CreatePipeline_ABCWithAuth_CreatesPipeline()
        {
            var a = new MockDelegatingHandler();
            var b = new MockDelegatingHandler();
            var c = new HttpClientHandler();

            var options = new DatasyncClientOptions { HttpPipeline = new HttpMessageHandler[] { a, b, c } };

            // Act
            var authProvider = new GenericAuthenticationProvider(() => Task.FromResult(ValidAuthenticationToken), "X-ZUMO-AUTH");
            var client = new WrappedHttpClient(Endpoint, authProvider, options);

            // Assert
            Assert.Same(authProvider, client.HttpHandler);
            Assert.Same(a, authProvider.InnerHandler);
            Assert.Same(b, a.InnerHandler);
            Assert.Same(c, b.InnerHandler);
        }

        [Fact]
        [Trait("Method", "CreatePipeline")]
        public void CreatePipeline_CBWithAuth_ThrowsArgumentException()
        {
            var b = new MockDelegatingHandler();
            var c = new HttpClientHandler();

            var options = new DatasyncClientOptions { HttpPipeline = new HttpMessageHandler[] { c, b } };

            // Act
            var authProvider = new GenericAuthenticationProvider(() => Task.FromResult(ValidAuthenticationToken), "X-ZUMO-AUTH");
            Assert.Throws<ArgumentException>(() => _ = new WrappedHttpClient(Endpoint, authProvider, options));
        }

        [Fact]
        [Trait("Method", "CreatePipeline")]
        public void CreatePipeline_CABWithAuth_ThrowsArgumentException()
        {
            var a = new MockDelegatingHandler();
            var b = new MockDelegatingHandler();
            var c = new HttpClientHandler();

            var options = new DatasyncClientOptions { HttpPipeline = new HttpMessageHandler[] { c, a, b } };

            // Act
            var authProvider = new GenericAuthenticationProvider(() => Task.FromResult(ValidAuthenticationToken), "X-ZUMO-AUTH");
            Assert.Throws<ArgumentException>(() => _ = new WrappedHttpClient(Endpoint, authProvider, options));
        }

        [Fact]
        [Trait("Method", "CreatePipeline")]
        public void CreatePipeline_ACBWithAuth_ThrowsArgumentException()
        {
            var a = new MockDelegatingHandler();
            var b = new MockDelegatingHandler();
            var c = new HttpClientHandler();

            var options = new DatasyncClientOptions { HttpPipeline = new HttpMessageHandler[] { a, c, b } };

            // Act
            var authProvider = new GenericAuthenticationProvider(() => Task.FromResult(ValidAuthenticationToken), "X-ZUMO-AUTH");
            Assert.Throws<ArgumentException>(() => _ = new WrappedHttpClient(Endpoint, authProvider, options));
        }

        [Fact]
        [Trait("Method", "SendAsync(HttpRequestMessage)")]
        public async Task SendHttpAsync_NullRequest_Throws()
        {
            HttpRequestMessage sut = null;
            var client = new WrappedHttpClient(Endpoint, new DatasyncClientOptions());
            await Assert.ThrowsAsync<ArgumentNullException>(() => client.WrappedSendAsync(sut)).ConfigureAwait(false);
        }

        [Fact]
        [Trait("Method", "SendAsync(HttpRequestMessage)")]
        public async Task SendHttpAsync_OnBadRequest()
        {
            var handler = new MockDelegatingHandler();
            var options = new DatasyncClientOptions
            {
                HttpPipeline = new HttpMessageHandler[] { handler },
                InstallationId = "hijack",
                UserAgent = "hijack"
            };
            var client = new WrappedHttpClient(Endpoint, options);
            var response = new HttpResponseMessage(HttpStatusCode.BadRequest);
            handler.Responses.Add(response);
            var request = new HttpRequestMessage(HttpMethod.Get, "");

            var actualResponse = await client.WrappedSendAsync(request).ConfigureAwait(false);

            Assert.Same(response, actualResponse);      // We get the provided response back.

            // Check that the right headers were applied to the request
            Assert.Single(handler.Requests);
            var actual = handler.Requests[0];

            Assert.Equal(options.UserAgent, actual.Headers.UserAgent.ToString());
            AssertEx.HasHeader(actual.Headers, "X-ZUMO-VERSION", options.UserAgent);
            AssertEx.HasHeader(actual.Headers, "ZUMO-API-VERSION", "3.0.0");
            AssertEx.HasHeader(actual.Headers, "X-ZUMO-INSTALLATION-ID", options.InstallationId);
        }

        [Fact]
        [Trait("Method", "SendAsync(HttpRequestMessage)")]
        public async Task SendHttpAsync_OnSuccessfulRequest()
        {
            var handler = new MockDelegatingHandler();
            var options = new DatasyncClientOptions
            {
                HttpPipeline = new HttpMessageHandler[] { handler },
                InstallationId = "hijack",
                UserAgent = "hijack"
            };
            var client = new WrappedHttpClient(Endpoint, options);
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            handler.Responses.Add(response);
            var request = new HttpRequestMessage(HttpMethod.Get, "");

            var actualResponse = await client.WrappedSendAsync(request).ConfigureAwait(false);

            Assert.Same(response, actualResponse);      // We get the provided response back.

            // Check that the right headers were applied to the request
            Assert.Single(handler.Requests);
            var actual = handler.Requests[0];

            Assert.Equal(options.UserAgent, actual.Headers.UserAgent.ToString());
            AssertEx.HasHeader(actual.Headers, "X-ZUMO-VERSION", options.UserAgent);
            AssertEx.HasHeader(actual.Headers, "ZUMO-API-VERSION", "3.0.0");
            AssertEx.HasHeader(actual.Headers, "X-ZUMO-INSTALLATION-ID", options.InstallationId);
        }

        [Fact]
        [Trait("Method", "SendAsync(HttpRequestMessage)")]
        public async Task SendHttpAsync_ThrowsTimeout_WhenOperationCanceled()
        {
            var handler = new TimeoutDelegatingHandler();
            var options = new DatasyncClientOptions { HttpPipeline = new HttpMessageHandler[] { handler } };
            var client = new WrappedHttpClient(Endpoint, options);
            var request = new HttpRequestMessage(HttpMethod.Get, "");

            await Assert.ThrowsAsync<TimeoutException>(() => client.WrappedSendAsync(request)).ConfigureAwait(false);
        }

        [Fact]
        [Trait("Method", "SendAsync(HttpRequestMessage)")]
        public async Task SendHttpAsync_OnSuccessfulRequest_WithAuth()
        {
            var handler = new MockDelegatingHandler();
            var options = new DatasyncClientOptions
            {
                HttpPipeline = new HttpMessageHandler[] { handler },
                InstallationId = "hijack",
                UserAgent = "hijack"
            };
            var requestor = () => Task.FromResult(ValidAuthenticationToken);
            var client = new WrappedHttpClient(Endpoint, new GenericAuthenticationProvider(requestor, "X-ZUMO-AUTH"), options);
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            handler.Responses.Add(response);
            var request = new HttpRequestMessage(HttpMethod.Get, "");

            var actualResponse = await client.WrappedSendAsync(request).ConfigureAwait(false);

            Assert.Same(response, actualResponse);      // We get the provided response back.

            // Check that the right headers were applied to the request
            Assert.Single(handler.Requests);
            var actual = handler.Requests[0];

            Assert.Equal(options.UserAgent, actual.Headers.UserAgent.ToString());
            AssertEx.HasHeader(actual.Headers, "X-ZUMO-VERSION", options.UserAgent);
            AssertEx.HasHeader(actual.Headers, "ZUMO-API-VERSION", "3.0.0");
            AssertEx.HasHeader(actual.Headers, "X-ZUMO-AUTH", ValidAuthenticationToken.Token);
            AssertEx.HasHeader(actual.Headers, "X-ZUMO-INSTALLATION-ID", options.InstallationId);
        }

        [Fact]
        [Trait("Method", "SendAsync(ServiceMessage)")]
        public async Task SendServiceAsync_NullRequest_Throws()
        {
            ServiceRequest sut = null;
            var client = new WrappedHttpClient(Endpoint, new DatasyncClientOptions());
            await Assert.ThrowsAsync<ArgumentNullException>(() => client.WrappedSendAsync(sut)).ConfigureAwait(false);
        }

        [Theory]
        [InlineData(null, "The request could not be completed")]
        [InlineData("", "The request could not be completed")]
        [InlineData("Bad Request", "Bad Request")]
        [InlineData("{this-is-bad-json", "The request could not be completed")]
        [InlineData("{\"error\":\"some error\"}", "some error")]
        [InlineData("{\"description\":\"some other error\"}", "some other error")]
        [InlineData("{}", "The request could not be completed")]
        [Trait("Method", "SendAsync(ServiceMessage)")]
        public async Task SendServiceAsync_OnBadRequest(string content, string expectedMessage)
        {
            var handler = new MockDelegatingHandler();
            var response = new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = content == null ? null : new StringContent(content, Encoding.UTF8, "application/json")
            };
            handler.Responses.Add(response);

            var options = new DatasyncClientOptions
            {
                HttpPipeline = new HttpMessageHandler[] { handler },
                InstallationId = "hijack",
                UserAgent = "hijack"
            };
            var client = new WrappedHttpClient(Endpoint, options);
            var request = new ServiceRequest { Method = HttpMethod.Get, UriPathAndQuery = "/tables/movies/" };
            var exception = await Assert.ThrowsAsync<DatasyncInvalidOperationException>(() => client.WrappedSendAsync(request));
            Assert.Same(response, exception.Response);
            Assert.StartsWith(expectedMessage, exception.Message);

            // Check that the right headers were applied to the request
            Assert.Single(handler.Requests);
            var actual = handler.Requests[0];

            Assert.Equal(options.UserAgent, actual.Headers.UserAgent.ToString());
            AssertEx.HasHeader(actual.Headers, "X-ZUMO-VERSION", options.UserAgent);
            AssertEx.HasHeader(actual.Headers, "ZUMO-API-VERSION", "3.0.0");
            AssertEx.HasHeader(actual.Headers, "X-ZUMO-INSTALLATION-ID", options.InstallationId);
        }

        [Fact]
        [Trait("Method", "SendAsync(ServiceMessage)")]
        public async Task SendServiceAsync_OnSuccessfulRequest()
        {
            var handler = new MockDelegatingHandler();
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            handler.Responses.Add(response);

            var options = new DatasyncClientOptions
            {
                HttpPipeline = new HttpMessageHandler[] { handler },
                InstallationId = "hijack",
                UserAgent = "hijack"
            };
            var client = new WrappedHttpClient(Endpoint, options);

            var request = new ServiceRequest { Method = HttpMethod.Get, UriPathAndQuery = "/tables/movies/", EnsureResponseContent = false };
            var actualResponse = await client.WrappedSendAsync(request).ConfigureAwait(false);
            Assert.Equal(200, actualResponse.StatusCode);

            // Check that the right headers were applied to the request
            Assert.Single(handler.Requests);
            var actual = handler.Requests[0];

            Assert.Equal(options.UserAgent, actual.Headers.UserAgent.ToString());
            AssertEx.HasHeader(actual.Headers, "X-ZUMO-VERSION", options.UserAgent);
            AssertEx.HasHeader(actual.Headers, "ZUMO-API-VERSION", "3.0.0");
            AssertEx.HasHeader(actual.Headers, "X-ZUMO-INSTALLATION-ID", options.InstallationId);
        }

        [Fact]
        [Trait("Method", "SendAsync(ServiceMessage)")]
        public async Task SendServiceAsync_Throws_OnFailedRequest()
        {
            var handler = new MockDelegatingHandler();
            var response = new HttpResponseMessage(HttpStatusCode.BadRequest);
            handler.Responses.Add(response);

            var options = new DatasyncClientOptions
            {
                HttpPipeline = new HttpMessageHandler[] { handler },
                InstallationId = "hijack",
                UserAgent = "hijack"
            };
            var client = new WrappedHttpClient(Endpoint, options);

            var request = new ServiceRequest { Method = HttpMethod.Get, UriPathAndQuery = "/tables/movies/", EnsureResponseContent = false };
            var exception = await Assert.ThrowsAsync<DatasyncInvalidOperationException>(() => client.WrappedSendAsync(request)).ConfigureAwait(false);
            Assert.NotNull(exception.Request);
            Assert.NotNull(exception.Response);
            Assert.Equal(HttpStatusCode.BadRequest, exception.Response.StatusCode);
        }

        [Fact]
        [Trait("Method", "SendAsync(ServiceMessage)")]
        public async Task SendServiceAsync_ThrowsTimeout_WhenOperationCanceled()
        {
            var handler = new TimeoutDelegatingHandler();
            var options = new DatasyncClientOptions { HttpPipeline = new HttpMessageHandler[] { handler } };
            var client = new WrappedHttpClient(Endpoint, options);
            var request = new ServiceRequest { Method = HttpMethod.Get, UriPathAndQuery = "/tables/movies/" };
            await Assert.ThrowsAsync<TimeoutException>(() => client.WrappedSendAsync(request)).ConfigureAwait(false);
        }

        [Fact]
        [Trait("Method", "SendAsync(ServiceMessage)")]
        public async Task SendServiceAsync_OnSuccessfulRequest_WithAuth()
        {
            var handler = new MockDelegatingHandler();
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{}", Encoding.UTF8, "application/json")
            };
            handler.Responses.Add(response);

            var options = new DatasyncClientOptions
            {
                HttpPipeline = new HttpMessageHandler[] { handler },
                InstallationId = "hijack",
                UserAgent = "hijack"
            };
            var requestor = () => Task.FromResult(ValidAuthenticationToken);
            var client = new WrappedHttpClient(Endpoint, new GenericAuthenticationProvider(requestor, "X-ZUMO-AUTH"), options);
            var request = new ServiceRequest { Method = HttpMethod.Get, UriPathAndQuery = "/tables/movies/" };

            var actualResponse = await client.WrappedSendAsync(request).ConfigureAwait(false);
            Assert.Equal(200, actualResponse.StatusCode);
            Assert.Equal("{}", actualResponse.Content);
            Assert.True(actualResponse.HasContent);

            // Check that the right headers were applied to the request
            Assert.Single(handler.Requests);
            var actual = handler.Requests[0];

            Assert.Equal(options.UserAgent, actual.Headers.UserAgent.ToString());
            AssertEx.HasHeader(actual.Headers, "X-ZUMO-VERSION", options.UserAgent);
            AssertEx.HasHeader(actual.Headers, "ZUMO-API-VERSION", "3.0.0");
            AssertEx.HasHeader(actual.Headers, "X-ZUMO-AUTH", ValidAuthenticationToken.Token);
            AssertEx.HasHeader(actual.Headers, "X-ZUMO-INSTALLATION-ID", options.InstallationId);
        }

        [Fact]
        [Trait("Method", "Dispose(boolean)")]
        public void Dispose_True_Disposes()
        {
            var client = new WrappedHttpClient(Endpoint, new DatasyncClientOptions());
            Assert.NotNull(client.HttpHandler);
            Assert.NotNull(client.HttpClient);

            client.WrappedDispose(true);
            Assert.Null(client.HttpHandler);
            Assert.Null(client.HttpClient);
        }

        [Fact]
        [Trait("Method", "Dispose(boolean)")]
        public void Dispose_False_Disposes()
        {
            var client = new WrappedHttpClient(Endpoint, new DatasyncClientOptions());
            Assert.NotNull(client.HttpHandler);
            Assert.NotNull(client.HttpClient);

            client.WrappedDispose(false);
            Assert.NotNull(client.HttpHandler);
            Assert.NotNull(client.HttpClient);
        }

        [Fact]
        [Trait("Method", "Dispose")]
        public void Dispose_Disposes()
        {
            var client = new WrappedHttpClient(Endpoint, new DatasyncClientOptions());
            Assert.NotNull(client.HttpHandler);
            Assert.NotNull(client.HttpClient);

            client.WrappedDispose();
            Assert.Null(client.HttpHandler);
            Assert.Null(client.HttpClient);
        }

        [Fact]
        [Trait("Method", "Dispose(boolean)")]
        public void Dispose_CanDisposeTwice()
        {
            var client = new WrappedHttpClient(Endpoint, new DatasyncClientOptions());
            Assert.NotNull(client.HttpHandler);
            Assert.NotNull(client.HttpClient);

            client.WrappedDispose();
            Assert.Null(client.HttpHandler);
            Assert.Null(client.HttpClient);

            client.WrappedDispose();
            Assert.Null(client.HttpHandler);
            Assert.Null(client.HttpClient);
        }
    }
}
