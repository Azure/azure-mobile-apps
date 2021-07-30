// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Test.Helpers;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Datasync.Client.Test.Authentication
{
    [ExcludeFromCodeCoverage]
    public class GenericAuthenticationProvider_Tests : BaseTest
    {
        #region Test Artifacts
        private static readonly AuthenticationToken basicToken = new()
        {
            DisplayName = "John Smith",
            ExpiresOn = DateTimeOffset.Now.AddMinutes(5),
            Token = "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJpc3MiOiJkYXRhc3luYy1mcmFtZXdvcmstdGVzdHMiLCJpYXQiOjE2Mjc2NTk4MTMsImV4cCI6MTY1OTE5NTgxMywiYXVkIjoiZGF0YXN5bmMtZnJhbWV3b3JrLXRlc3RzLmNvbnRvc28uY29tIiwic3ViIjoidGhlX2RvY3RvckBjb250b3NvLmNvbSIsIkdpdmVuTmFtZSI6IkpvaG4iLCJTdXJuYW1lIjoiU21pdGgiLCJFbWFpbCI6InRoZV9kb2N0b3JAY29udG9zby5jb20ifQ.6Sm-ghJBKLB1vC4NuCqYKwL1mbRnJ9ziSHQT5VlNVEY",
            UserId = "the_doctor"
        };

        private static readonly AuthenticationToken expiredToken = new()
        {
            DisplayName = "John Smith",
            ExpiresOn = DateTimeOffset.Now.AddMinutes(-5),
            Token = "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJpc3MiOiJkYXRhc3luYy1mcmFtZXdvcmstdGVzdHMiLCJpYXQiOjE2Mjc2NTk4MTMsImV4cCI6MTY1OTE5NTgxMywiYXVkIjoiZGF0YXN5bmMtZnJhbWV3b3JrLXRlc3RzLmNvbnRvc28uY29tIiwic3ViIjoidGhlX2RvY3RvckBjb250b3NvLmNvbSIsIkdpdmVuTmFtZSI6IkpvaG4iLCJTdXJuYW1lIjoiU21pdGgiLCJFbWFpbCI6InRoZV9kb2N0b3JAY29udG9zby5jb20ifQ.6Sm-ghJBKLB1vC4NuCqYKwL1mbRnJ9ziSHQT5VlNVEY",
            UserId = "the_doctor"
        };

        private readonly Func<Task<AuthenticationToken>> requestor = () => Task.FromResult(basicToken);
        private readonly Func<Task<AuthenticationToken>> expiredRequestor = () => Task.FromResult(expiredToken);

        private class IntGap : GenericAuthenticationProvider
        {
            public IntGap(Func<Task<AuthenticationToken>> requestor, string header = "Authorization", string authType = null)
                : base(requestor, header, authType) { }

            public Task<HttpResponseMessage> IntSendAsync(HttpRequestMessage request, CancellationToken token = default)
                => base.SendAsync(request, token);
        }
        #endregion

        #region Ctor
        [Fact]
        [Trait("Method", "Ctor")]
        public void Ctor_NullTokenRequestor_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new GenericAuthenticationProvider(null));
        }

        [Fact]
        [Trait("Method", "Ctor")]
        public void Ctor_CanSetTokenRequestor()
        {
            var sut = new GenericAuthenticationProvider(requestor);
            Assert.Same(requestor, sut.TokenRequestorAsync);
            Assert.Equal("Authorization", sut.HeaderName);
            Assert.Equal("Bearer", sut.AuthenticationType);
            Assert.Null(sut.Current);
            Assert.True(sut.RefreshBufferTimeSpan.TotalMilliseconds > 0);
        }

        [Fact]
        [Trait("Method", "Ctor")]
        public void Ctor_NullHeader_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new GenericAuthenticationProvider(requestor, null));
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("\t")]
        [InlineData(" \t ")]
        [Trait("Method", "Ctor")]
        public void Ctor_WhitespaceHeader_Throws(string headerName)
        {
            Assert.Throws<ArgumentException>(() => new GenericAuthenticationProvider(requestor, headerName));
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("\t")]
        [InlineData(" \t ")]
        [Trait("Method", "Ctor")]
        public void Ctor_Authorization_RequiresType(string authType)
        {
            Assert.Throws<ArgumentException>(() => new GenericAuthenticationProvider(requestor, "Authorization", authType));
        }

        [Fact]
        [Trait("Method", "Ctor")]
        public void Ctor_CanDoXZumoAuth()
        {
            var sut = new GenericAuthenticationProvider(requestor, "X-ZUMO-AUTH", null);
            Assert.Same(requestor, sut.TokenRequestorAsync);
            Assert.Equal("X-ZUMO-AUTH", sut.HeaderName);
            Assert.Null(sut.AuthenticationType);
            Assert.Null(sut.Current);
            Assert.True(sut.RefreshBufferTimeSpan.TotalMilliseconds > 0);
        }

        [Fact]
        [Trait("Method", "Ctor")]
        public void Ctor_CanDoAuthBasic()
        {
            var sut = new GenericAuthenticationProvider(requestor, "Authorization", "Basic");
            Assert.Same(requestor, sut.TokenRequestorAsync);
            Assert.Equal("Authorization", sut.HeaderName);
            Assert.Equal("Basic", sut.AuthenticationType);
            Assert.Null(sut.Current);
            Assert.True(sut.RefreshBufferTimeSpan.TotalMilliseconds > 0);
        }

        [Fact]
        [Trait("Method", "Ctor")]
        public void Ctor_CanDoAuthBearer()
        {
            var sut = new GenericAuthenticationProvider(requestor, "Authorization");
            Assert.Same(requestor, sut.TokenRequestorAsync);
            Assert.Equal("Authorization", sut.HeaderName);
            Assert.Equal("Bearer", sut.AuthenticationType);
            Assert.Null(sut.Current);
            Assert.True(sut.RefreshBufferTimeSpan.TotalMilliseconds > 0);
        }
        #endregion

        #region RefreshBufferTimeSpan
        [Theory]
        [InlineData(0)]
        [InlineData(100)]
        [Trait("Method", "RefreshBufferTimeSpan")]
        public void RefreshBufferTimeSpan_CannotBeSmall(long ms)
        {
            var sut = new GenericAuthenticationProvider(requestor);
            Assert.Throws<ArgumentException>(() => sut.RefreshBufferTimeSpan = TimeSpan.FromMilliseconds(ms));
        }

        [Fact]
        [Trait("Method", "RefreshBufferTimeSpan")]
        public void RefreshBufferTimeSpan_Roundtrips()
        {
            var ts = TimeSpan.FromMinutes(1);
            var sut = new GenericAuthenticationProvider(requestor) { RefreshBufferTimeSpan = ts };
            Assert.Equal(ts, sut.RefreshBufferTimeSpan);
        }
        #endregion

        #region IsExpired
        [Fact]
        [Trait("Method", "IsExpired")]
        public void IsExpired_NullToken_ReturnsTrue()
        {
            var sut = new GenericAuthenticationProvider(requestor) { RefreshBufferTimeSpan = TimeSpan.FromMinutes(2) };
            Assert.True(sut.IsExpired(sut.Current));
        }

        [Fact]
        [Trait("Method", "IsExpired")]
        public void IsExpired_NotExpired_ReturnsFalse()
        {
            var sut = new GenericAuthenticationProvider(requestor) { RefreshBufferTimeSpan = TimeSpan.FromMinutes(2) };
            sut.Current = new AuthenticationToken { ExpiresOn = DateTimeOffset.Now.AddMinutes(4) };
            Assert.False(sut.IsExpired(sut.Current));
        }

        [Fact]
        [Trait("Method", "IsExpired")]
        public void IsExpired_InBuffer_ReturnsTrue()
        {
            var sut = new GenericAuthenticationProvider(requestor) { RefreshBufferTimeSpan = TimeSpan.FromMinutes(2) };
            sut.Current = new AuthenticationToken { ExpiresOn = DateTimeOffset.Now.AddMinutes(-1) };
            Assert.True(sut.IsExpired(sut.Current));
        }

        [Fact]
        [Trait("Method", "IsExpired")]
        public void IsExpired_Expired_ReturnsTrue()
        {
            var sut = new GenericAuthenticationProvider(requestor) { RefreshBufferTimeSpan = TimeSpan.FromMinutes(2) };
            sut.Current = new AuthenticationToken { ExpiresOn = DateTimeOffset.Now.AddMinutes(-3) };
            Assert.True(sut.IsExpired(sut.Current));
        }

        [Fact]
        [Trait("Method", "IsExpired")]
        public void IsExpired_ExpiredToken_ReturnsTrue()
        {
            var sut = new GenericAuthenticationProvider(requestor) { RefreshBufferTimeSpan = TimeSpan.FromMinutes(2) };
            Assert.True(sut.IsExpired(expiredToken));
        }


        [Fact]
        [Trait("Method", "IsExpired")]
        public void IsExpired_BasicToken_ReturnsFalse()
        {
            var sut = new GenericAuthenticationProvider(requestor) { RefreshBufferTimeSpan = TimeSpan.FromMinutes(2) };
            Assert.False(sut.IsExpired(basicToken));
        }
        #endregion

        #region GetTokenAsync
        [Fact]
        [Trait("Method", "GetTokenAsync")]
        public async Task GetTokenAsync_CallsOnFirstRun()
        {
            int requestCount = 0;
            Func<Task<AuthenticationToken>> countingRequestor = () =>
            {
                requestCount++;
                return Task.FromResult(basicToken);
            };

            var sut = new GenericAuthenticationProvider(countingRequestor);
            var actual = await sut.GetTokenAsync().ConfigureAwait(false);
            Assert.Equal(basicToken.Token, actual);
            Assert.Equal(1, requestCount);
        }

        [Fact]
        [Trait("Method", "GetTokenAsync")]
        public async Task GetTokenAsync_CachesResult()
        {
            int requestCount = 0;
            Func<Task<AuthenticationToken>> countingRequestor = () =>
            {
                requestCount++;
                return Task.FromResult(basicToken);
            };

            var sut = new GenericAuthenticationProvider(countingRequestor);
            var firstCall = await sut.GetTokenAsync().ConfigureAwait(false);
            var secondCall = await sut.GetTokenAsync().ConfigureAwait(false);
            Assert.Equal(basicToken.Token, firstCall);
            Assert.Equal(basicToken.Token, secondCall);
            Assert.Equal(1, requestCount);
        }

        [Fact]
        [Trait("Method", "GetTokenAsync")]
        public async Task GetTokenAsync_CallsOnForce()
        {
            int requestCount = 0;
            Func<Task<AuthenticationToken>> countingRequestor = () =>
            {
                requestCount++;
                return Task.FromResult(basicToken);
            };

            var sut = new GenericAuthenticationProvider(countingRequestor);
            var firstCall = await sut.GetTokenAsync().ConfigureAwait(false);
            Assert.Equal(basicToken.Token, firstCall);
            var secondCall = await sut.GetTokenAsync(true).ConfigureAwait(false);
            Assert.Equal(basicToken.Token, secondCall);
            Assert.Equal(2, requestCount);
        }

        [Fact]
        [Trait("Method", "GetTokenAsync")]
        public async Task GetTokenAsync_LogsOutWhenExpired()
        {
            var sut = new GenericAuthenticationProvider(requestor);
            var firstCall = await sut.GetTokenAsync().ConfigureAwait(false);
            Assert.Equal(basicToken.Token, firstCall);
            Assert.Equal(basicToken.DisplayName, sut.DisplayName);
            Assert.Equal(basicToken.UserId, sut.UserId);
            Assert.True(sut.IsLoggedIn);

            sut.TokenRequestorAsync = expiredRequestor;
            var secondCall = await sut.GetTokenAsync(true).ConfigureAwait(false);
            Assert.Null(secondCall);
            Assert.Null(sut.DisplayName);
            Assert.Null(sut.UserId);
            Assert.False(sut.IsLoggedIn);
        }
        #endregion

        #region LoginAsync
        [Fact]
        [Trait("Method", "LoginAsync")]
        public async Task LoginAsync_CallsTokenRequestor()
        {
            int requestCount = 0;
            Func<Task<AuthenticationToken>> countingRequestor = () =>
            {
                requestCount++;
                return Task.FromResult(basicToken);
            };

            var sut = new GenericAuthenticationProvider(countingRequestor);
            await sut.LoginAsync().ConfigureAwait(false);
            Assert.Equal(1, requestCount);
        }

        [Fact]
        [Trait("Method", "LoginAsync")]
        public async Task LoginAsync_ForcesTokenRequestor()
        {
            int requestCount = 0;
            Func<Task<AuthenticationToken>> countingRequestor = () =>
            {
                requestCount++;
                return Task.FromResult(basicToken);
            };

            var sut = new GenericAuthenticationProvider(countingRequestor);
            var firstCall = await sut.GetTokenAsync().ConfigureAwait(false);
            Assert.Equal(basicToken.Token, firstCall);
            await sut.LoginAsync().ConfigureAwait(false);
            Assert.Equal(2, requestCount);
        }
        #endregion

        #region SendAsync
        [Fact]
        [Trait("Method", "SendAsync")]
        public async Task SendAsync_AddsHeader_BearerAuth()
        {
            var handler = new TestDelegatingHandler();
            handler.Responses.Add(new HttpResponseMessage(HttpStatusCode.OK));
            var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/test");
            var sut = new IntGap(requestor);
            sut.InnerHandler = handler;

            var response = await sut.IntSendAsync(request);

            Assert.NotNull(response);
            Assert.Single(handler.Requests);
            var headers = handler.Requests[0].Headers;
            Assert.Equal(basicToken.Token, headers.Authorization.Parameter);
            Assert.Equal("Bearer", headers.Authorization.Scheme);
        }

        [Fact]
        [Trait("Method", "SendAsync")]
        public async Task SendAsync_AddsHeader_ZumoAuth()
        {
            var handler = new TestDelegatingHandler();
            handler.Responses.Add(new HttpResponseMessage(HttpStatusCode.OK));
            var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/test");
            var sut = new IntGap(requestor, "X-ZUMO-AUTH");
            sut.InnerHandler = handler;

            var response = await sut.IntSendAsync(request);

            Assert.NotNull(response);
            Assert.Single(handler.Requests);
            var headers = handler.Requests[0].Headers;
            Assert.Equal(basicToken.Token, headers.GetValues("X-ZUMO-AUTH").First());
        }

        [Fact]
        [Trait("Method", "SendAsync")]
        public async Task SendAsync_NoHeader_WhenExpired()
        {
            var handler = new TestDelegatingHandler();
            handler.Responses.Add(new HttpResponseMessage(HttpStatusCode.OK));
            var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/test");
            var sut = new IntGap(expiredRequestor, "X-ZUMO-AUTH");
            sut.InnerHandler = handler;

            var response = await sut.IntSendAsync(request);

            Assert.NotNull(response);
            Assert.Single(handler.Requests);
            var headers = handler.Requests[0].Headers;
            Assert.False(headers.Contains("X-ZUMO-AUTH"));
        }

        [Fact]
        [Trait("Method", "SendAsync")]
        public async Task SendAsync_RemoveHeader_WhenExpired()
        {
            var handler = new TestDelegatingHandler();
            handler.Responses.Add(new HttpResponseMessage(HttpStatusCode.OK));
            var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/test");
            request.Headers.Add("X-ZUMO-AUTH", "a-test-header");
            var sut = new IntGap(expiredRequestor, "X-ZUMO-AUTH");
            sut.InnerHandler = handler;

            var response = await sut.IntSendAsync(request);

            Assert.NotNull(response);
            Assert.Single(handler.Requests);
            var headers = handler.Requests[0].Headers;
            Assert.False(headers.Contains("X-ZUMO-AUTH"));
        }

        [Fact]
        [Trait("Method", "SendAsync")]
        public async Task SendAsync_OverwritesHeader_WhenNotExpired()
        {
            var handler = new TestDelegatingHandler();
            handler.Responses.Add(new HttpResponseMessage(HttpStatusCode.OK));
            var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/test");
            request.Headers.Add("X-ZUMO-AUTH", "a-test-header");
            var sut = new IntGap(requestor, "X-ZUMO-AUTH");
            sut.InnerHandler = handler;

            var response = await sut.IntSendAsync(request);

            Assert.NotNull(response);
            Assert.Single(handler.Requests);
            var headers = handler.Requests[0].Headers;
            Assert.Equal(basicToken.Token, headers.GetValues("X-ZUMO-AUTH").First());
        }
        #endregion
    }
}
