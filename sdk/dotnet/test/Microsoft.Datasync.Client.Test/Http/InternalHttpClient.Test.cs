﻿// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Http;
using Microsoft.Datasync.Client.Test.Helpers;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Datasync.Client.Test.Http
{
    [ExcludeFromCodeCoverage]
    public class InternalHttpClient_Test : BaseTest
    {
        /// <summary>
        /// Test version of <see cref="InternalHttpClient"/> that exposes the protected elements.
        /// </summary>
        internal class IntHttpClient : InternalHttpClient
        {
            internal IntHttpClient(Uri endpoint, DatasyncClientOptions options) : base(endpoint, options)
            {
            }
            internal string Endpoint { get => applicationUri.ToString(); }
            internal HttpMessageHandler HttpHandler { get => httpHandler; }
            internal HttpClient HttpClient { get => httpClient; }
            internal void IntDispose(bool dispose) => base.Dispose(dispose);
            internal void IntDispose() => base.Dispose();
            internal Task<HttpResponseMessage> IntSendAsync(HttpRequestMessage request, CancellationToken token = default)
                => base.SendAsync(request, token);
        }

        #region Ctor
        [Fact]
        [Trait("Method", "Ctor")]
        public void Ctor_NullEndpoint_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new IntHttpClient(null, ClientOptions));
        }

        [Fact]
        [Trait("Method", "Ctor")]
        public void Ctor_NullOptions_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new IntHttpClient(Endpoint, null));
        }

        [Theory, ClassData(typeof(TestCases.Invalid_Endpoints))]
        [Trait("Method", "Ctor")]
        public void Ctor_InvalidEndpoint_Throws(string endpoint, bool isRelative = false)
        {
            Assert.Throws<UriFormatException>(() => new IntHttpClient(isRelative ? new Uri(endpoint, UriKind.Relative) : new Uri(endpoint), ClientOptions));
        }

        [Theory, ClassData(typeof(TestCases.Valid_Endpoints))]
        [Trait("Method", "Ctor")]
        [SuppressMessage("Usage", "xUnit1026:Theory methods should use all of their parameters", Justification = "Test case does not check for endpoint")]
        [SuppressMessage("Redundancy", "RCS1163:Unused parameter.", Justification = "Test case does not check for endpoint")]
        [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Test case does not check for endpoint")]
        public void Ctor_ValidEndpoint_CreatesClient(string endpoint, string normalized)
        {
            var client = new IntHttpClient(new Uri(normalized), ClientOptions);
            Assert.Equal(normalized, client.Endpoint);
            Assert.NotNull(client.HttpHandler);
            Assert.NotNull(client.HttpClient);

            // DefaultRequestHeaders must contain the list of headers
            Assert.Equal(ClientOptions.UserAgent, client.HttpClient.DefaultRequestHeaders.UserAgent.ToString());
            AssertEx.Contains("X-ZUMO-VERSION", ClientOptions.UserAgent, client.HttpClient.DefaultRequestHeaders);
            AssertEx.Contains("ZUMO-API-VERSION", "3.0.0", client.HttpClient.DefaultRequestHeaders);
            AssertEx.Contains("X-ZUMO-INSTALLATION-ID", ClientOptions.InstallationId, client.HttpClient.DefaultRequestHeaders);
            Assert.Equal(normalized, client.HttpClient.BaseAddress.ToString());
        }

        [Theory, ClassData(typeof(TestCases.Valid_Endpoints))]
        [Trait("Method", "Ctor")]
        [SuppressMessage("Usage", "xUnit1026:Theory methods should use all of their parameters", Justification = "Test case does not check for endpoint")]
        [SuppressMessage("Redundancy", "RCS1163:Unused parameter.", Justification = "Test case does not check for endpoint")]
        [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Test case does not check for endpoint")]
        public void Ctor_ValidEndpoint_CustomOptions_CreatesClient(string endpoint, string normalized)
        {
            var options = new DatasyncClientOptions { HttpPipeline = null, InstallationId = "test-int-id", UserAgent = "test-user-agent" };
            var client = new IntHttpClient(new Uri(normalized), options);
            Assert.Equal(normalized, client.Endpoint);
            Assert.NotNull(client.HttpHandler);
            Assert.NotNull(client.HttpClient);

            // DefaultRequestHeaders must contain the list of headers
            Assert.Equal("test-user-agent", client.HttpClient.DefaultRequestHeaders.UserAgent.ToString());
            AssertEx.Contains("X-ZUMO-VERSION", "test-user-agent", client.HttpClient.DefaultRequestHeaders);
            AssertEx.Contains("ZUMO-API-VERSION", "3.0.0", client.HttpClient.DefaultRequestHeaders);
            AssertEx.Contains("X-ZUMO-INSTALLATION-ID", "test-int-id", client.HttpClient.DefaultRequestHeaders);
            Assert.Equal(normalized, client.HttpClient.BaseAddress.ToString());
        }
        #endregion

        #region CreatePipeline
        [Fact]
        [Trait("Method", "CreatePipeline")]
        public void CreatePipeline_NoHandlers_CreatesPipeline()
        {
            var options = new DatasyncClientOptions { HttpPipeline = Array.Empty<HttpMessageHandler>() };

            // Act
            var client = new IntHttpClient(Endpoint, options);

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
            var client = new IntHttpClient(Endpoint, options);

            // Assert
            Assert.Same(c, client.HttpHandler);
        }

        [Fact]
        [Trait("Method", "CreatePipeline")]
        public void CreatePipeline_B_CreatesPipeline()
        {
            var b = new TestDelegatingHandler();

            var options = new DatasyncClientOptions { HttpPipeline = new HttpMessageHandler[] { b } };

            // Act
            var client = new IntHttpClient(Endpoint, options);

            // Assert
            Assert.Same(b, client.HttpHandler);
            Assert.IsAssignableFrom<HttpClientHandler>(b.InnerHandler);
        }

        [Fact]
        [Trait("Method", "CreatePipeline")]
        public void CreatePipeline_BC_CreatesPipeline()
        {
            var b = new TestDelegatingHandler();
            var c = new HttpClientHandler();

            var options = new DatasyncClientOptions { HttpPipeline = new HttpMessageHandler[] { b, c } };

            // Act
            var client = new IntHttpClient(Endpoint, options);

            // Assert
            Assert.Same(b, client.HttpHandler);
            Assert.Same(c, b.InnerHandler);
        }

        [Fact]
        [Trait("Method", "CreatePipeline")]
        public void CreatePipeline_AB_CreatesPipeline()
        {
            var a = new TestDelegatingHandler();
            var b = new TestDelegatingHandler();

            var options = new DatasyncClientOptions { HttpPipeline = new HttpMessageHandler[] { a, b } };

            // Act
            var client = new IntHttpClient(Endpoint, options);

            // Assert
            Assert.Same(a, client.HttpHandler);
            Assert.Same(b, a.InnerHandler);
            Assert.IsAssignableFrom<HttpClientHandler>(b.InnerHandler);
        }

        [Fact]
        [Trait("Method", "CreatePipeline")]
        public void CreatePipeline_ABC_CreatesPipeline()
        {
            var a = new TestDelegatingHandler();
            var b = new TestDelegatingHandler();
            var c = new HttpClientHandler();

            var options = new DatasyncClientOptions { HttpPipeline = new HttpMessageHandler[] { a, b, c } };

            // Act
            var client = new IntHttpClient(Endpoint, options);

            // Assert
            Assert.Same(a, client.HttpHandler);
            Assert.Same(b, a.InnerHandler);
            Assert.Same(c, b.InnerHandler);
        }

        [Fact]
        [Trait("Method", "CreatePipeline")]
        public void CreatePipeline_CB_ThrowsArgumentException()
        {
            var b = new TestDelegatingHandler();
            var c = new HttpClientHandler();

            var options = new DatasyncClientOptions { HttpPipeline = new HttpMessageHandler[] { c, b } };

            // Act
            Assert.Throws<ArgumentException>(() => _ = new IntHttpClient(Endpoint, options));
        }

        [Fact]
        [Trait("Method", "CreatePipeline")]
        public void CreatePipeline_CAB_ThrowsArgumentException()
        {
            var a = new TestDelegatingHandler();
            var b = new TestDelegatingHandler();
            var c = new HttpClientHandler();

            var options = new DatasyncClientOptions { HttpPipeline = new HttpMessageHandler[] { c, a, b } };

            // Act
            Assert.Throws<ArgumentException>(() => _ = new IntHttpClient(Endpoint, options));
        }

        [Fact]
        [Trait("Method", "CreatePipeline")]
        public void CreatePipeline_ACB_ThrowsArgumentException()
        {
            var a = new TestDelegatingHandler();
            var b = new TestDelegatingHandler();
            var c = new HttpClientHandler();

            var options = new DatasyncClientOptions { HttpPipeline = new HttpMessageHandler[] { a, c, b } };

            // Act
            Assert.Throws<ArgumentException>(() => _ = new IntHttpClient(Endpoint, options));
        }
        #endregion

        #region IDisposable
        [Fact]
        [Trait("Method", "Dispose(bool)")]
        public void Dispose_True_Disposes()
        {
            var client = new IntHttpClient(Endpoint, ClientOptions);
            Assert.NotNull(client.HttpHandler);
            Assert.NotNull(client.HttpClient);

            client.IntDispose(true);
            Assert.Null(client.HttpHandler);
            Assert.Null(client.HttpClient);
        }

        [Fact]
        [Trait("Method", "Dispose(bool)")]
        public void Dispose_False_Disposes()
        {
            var client = new IntHttpClient(Endpoint, ClientOptions);
            Assert.NotNull(client.HttpHandler);
            Assert.NotNull(client.HttpClient);

            client.IntDispose(false);
            Assert.NotNull(client.HttpHandler);
            Assert.NotNull(client.HttpClient);
        }

        [Fact]
        [Trait("Method", "Dispose")]
        public void Dispose_Disposes()
        {
            var client = new IntHttpClient(Endpoint, ClientOptions);
            Assert.NotNull(client.HttpHandler);
            Assert.NotNull(client.HttpClient);

            client.IntDispose();
            Assert.Null(client.HttpHandler);
            Assert.Null(client.HttpClient);
        }
        #endregion

        #region SendAsync
        [Fact]
        [Trait("Method", "SendAsync")]
        public async Task SendAsync_NullRequest_Throws()
        {
            var client = new IntHttpClient(Endpoint, ClientOptions);
            await Assert.ThrowsAsync<ArgumentNullException>(() => client.IntSendAsync(null)).ConfigureAwait(false);
        }

        [Fact]
        [Trait("Method", "SendAsync")]
        public async Task SendAsync_Throws_OnBadRequest()
        {
            var handler = new TestDelegatingHandler();
            var options = new DatasyncClientOptions { HttpPipeline = new HttpMessageHandler[] { handler } };
            var client = new IntHttpClient(Endpoint, options);
            var response = new HttpResponseMessage(HttpStatusCode.BadRequest);
            handler.Responses.Add(response);
            var request = new HttpRequestMessage(HttpMethod.Get, "");

            var exception = await Assert.ThrowsAsync<DatasyncOperationException>(() => client.IntSendAsync(request)).ConfigureAwait(false);

            Assert.Same(request, exception.Request);
            Assert.Same(response, exception.Response);
        }

        [Fact]
        [Trait("Method", "SendAsync")]
        public async Task SendAsync_SendsProperRequest()
        {
            var handler = new TestDelegatingHandler();
            var options = new DatasyncClientOptions { HttpPipeline = new HttpMessageHandler[] { handler } };
            var client = new IntHttpClient(Endpoint, options);
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            handler.Responses.Add(response);
            var request = new HttpRequestMessage(HttpMethod.Get, "");

            var actualResponse = await client.IntSendAsync(request).ConfigureAwait(false);

            Assert.Same(response, actualResponse);      // We get the provided response back.

            // Check that the right headers were applied to the request
            Assert.Single(handler.Requests);
            var actual = handler.Requests[0];

            Assert.Equal(options.UserAgent, actual.Headers.UserAgent.ToString());
            AssertEx.Contains("X-ZUMO-VERSION", options.UserAgent, actual.Headers);
            AssertEx.Contains("ZUMO-API-VERSION", "3.0.0", actual.Headers);
            AssertEx.Contains("X-ZUMO-INSTALLATION-ID", options.InstallationId, actual.Headers);
        }
        #endregion
    }
}
