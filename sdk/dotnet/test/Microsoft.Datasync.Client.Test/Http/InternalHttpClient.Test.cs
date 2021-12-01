﻿// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Test;
using Datasync.Common.Test.Mocks;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Datasync.Client.Test.Http
{
    [ExcludeFromCodeCoverage]
    public class InternalHttpClient_Test : BaseTest
    {
        [Fact]
        [Trait("Method", "Ctor")]
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

        [Theory, ClassData(typeof(TestCases.Invalid_Endpoints))]
        [Trait("Method", "Ctor")]
        public void Ctor_InvalidEndpoint_Throws(string endpoint, bool isRelative = false)
        {
            var options = new DatasyncClientOptions();
            Assert.Throws<UriFormatException>(() => new WrappedHttpClient(isRelative ? new Uri(endpoint, UriKind.Relative) : new Uri(endpoint), options));
        }

        [Theory, ClassData(typeof(TestCases.Valid_Endpoints))]
        [Trait("Method", "Ctor")]
        [SuppressMessage("Usage", "xUnit1026:Theory methods should use all of their parameters", Justification = "Test case does not check for endpoint")]
        [SuppressMessage("Redundancy", "RCS1163:Unused parameter.", Justification = "Test case does not check for endpoint")]
        public void Ctor_ValidEndpoint_CreatesClient(string endpoint, string normalized)
        {
            var options = new DatasyncClientOptions();
            var client = new WrappedHttpClient(new Uri(normalized), options);
            Assert.Equal(normalized, client.WrappedEndpoint);
            Assert.NotNull(client.HttpHandler);
            Assert.NotNull(client.HttpClient);

            // DefaultRequestHeaders must contain the list of headers
            Assert.Equal(options.UserAgent, client.HttpClient.DefaultRequestHeaders.UserAgent.ToString());
            AssertEx.HasHeader(client.HttpClient.DefaultRequestHeaders, "X-ZUMO-VERSION", options.UserAgent);
            AssertEx.HasHeader(client.HttpClient.DefaultRequestHeaders, "ZUMO-API-VERSION", "3.0.0");
            AssertEx.HasHeader(client.HttpClient.DefaultRequestHeaders, "X-ZUMO-INSTALLATION-ID", options.InstallationId);
            Assert.Equal(normalized, client.HttpClient.BaseAddress.ToString());
        }

        [Theory, ClassData(typeof(TestCases.Valid_Endpoints))]
        [Trait("Method", "Ctor")]
        [SuppressMessage("Usage", "xUnit1026:Theory methods should use all of their parameters", Justification = "Test case does not check for endpoint")]
        [SuppressMessage("Redundancy", "RCS1163:Unused parameter.", Justification = "Test case does not check for endpoint")]
        public void Ctor_ValidEndpoint_CustomOptions_CreatesClient(string endpoint, string normalized)
        {
            var options = new DatasyncClientOptions { HttpPipeline = null, InstallationId = "test-int-id", UserAgent = "test-user-agent" };
            var client = new WrappedHttpClient(new Uri(normalized), options);
            Assert.Equal(normalized, client.WrappedEndpoint);
            Assert.NotNull(client.HttpHandler);
            Assert.NotNull(client.HttpClient);

            // DefaultRequestHeaders must contain the list of headers
            Assert.Equal("test-user-agent", client.HttpClient.DefaultRequestHeaders.UserAgent.ToString());
            AssertEx.HasHeader(client.HttpClient.DefaultRequestHeaders, "X-ZUMO-VERSION", "test-user-agent");
            AssertEx.HasHeader(client.HttpClient.DefaultRequestHeaders, "ZUMO-API-VERSION", "3.0.0");
            AssertEx.HasHeader(client.HttpClient.DefaultRequestHeaders, "X-ZUMO-INSTALLATION-ID", "test-int-id");
            Assert.Equal(normalized, client.HttpClient.BaseAddress.ToString());
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
        [Trait("Method", "SendAsync")]
        public async Task SendAsync_NullRequest_Throws()
        {
            var client = new WrappedHttpClient(Endpoint, new DatasyncClientOptions());
            await Assert.ThrowsAsync<ArgumentNullException>(() => client.WrappedSendAsync(null)).ConfigureAwait(false);
        }

        [Fact]
        [Trait("Method", "SendAsync")]
        public async Task SendAsync_OnBadRequest()
        {
            var handler = new MockDelegatingHandler();
            var options = new DatasyncClientOptions { HttpPipeline = new HttpMessageHandler[] { handler } };
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
        [Trait("Method", "SendAsync")]
        public async Task SendAsync_OnSuccessfulRequest()
        {
            var handler = new MockDelegatingHandler();
            var options = new DatasyncClientOptions { HttpPipeline = new HttpMessageHandler[] { handler } };
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
        [Trait("Method", "SendAsync")]
        public async Task SendAsync_ThrowsTimeout_WhenOperationCanceled()
        {
            var handler = new TimeoutDelegatingHandler();
            var options = new DatasyncClientOptions { HttpPipeline = new HttpMessageHandler[] { handler } };
            var client = new WrappedHttpClient(Endpoint, options);
            var request = new HttpRequestMessage(HttpMethod.Get, "");

            await Assert.ThrowsAsync<TimeoutException>(() => client.WrappedSendAsync(request)).ConfigureAwait(false);
        }

        [Fact]
        [Trait("Method", "SendAsync")]
        public async Task SendAsync_OnSuccessfulRequest_WithAuth()
        {
            var handler = new MockDelegatingHandler();
            var options = new DatasyncClientOptions { HttpPipeline = new HttpMessageHandler[] { handler } };
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
    }
}
