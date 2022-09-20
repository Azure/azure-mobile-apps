// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Test;
using Datasync.Common.Test.Mocks;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Datasync.Client.Test.Authentication
{
    [ExcludeFromCodeCoverage]
    public class GenericAuthenticationProvider_Tests : BaseTest
    {
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
            var sut = new GenericAuthenticationProvider(() => Task.FromResult(ValidAuthenticationToken));
            Assert.Equal("Authorization", sut.HeaderName);
            Assert.Equal("Bearer", sut.AuthenticationType);
            Assert.Null(sut.Current);
            Assert.True(sut.RefreshBufferTimeSpan.TotalMilliseconds > 0);
        }

        [Fact]
        [Trait("Method", "Ctor")]
        public void Ctor_NullHeader_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new GenericAuthenticationProvider(() => Task.FromResult(ValidAuthenticationToken), null));
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("\t")]
        [InlineData(" \t ")]
        [Trait("Method", "Ctor")]
        public void Ctor_WhitespaceHeader_Throws(string headerName)
        {
            Assert.Throws<ArgumentException>(() => new GenericAuthenticationProvider(() => Task.FromResult(ValidAuthenticationToken), headerName));
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("\t")]
        [InlineData(" \t ")]
        [Trait("Method", "Ctor")]
        public void Ctor_Authorization_RequiresType(string authType)
        {
            Assert.Throws<ArgumentException>(() => new GenericAuthenticationProvider(() => Task.FromResult(ValidAuthenticationToken), "Authorization", authType));
        }

        [Fact]
        [Trait("Method", "Ctor")]
        public void Ctor_CanDoXZumoAuth()
        {
            var sut = new GenericAuthenticationProvider(() => Task.FromResult(ValidAuthenticationToken), "X-ZUMO-AUTH", null);
            Assert.Equal("X-ZUMO-AUTH", sut.HeaderName);
            Assert.Null(sut.AuthenticationType);
            Assert.Null(sut.Current);
            Assert.True(sut.RefreshBufferTimeSpan.TotalMilliseconds > 0);
        }

        [Fact]
        [Trait("Method", "Ctor")]
        public void Ctor_CanDoAuthBasic()
        {
            var sut = new GenericAuthenticationProvider(() => Task.FromResult(ValidAuthenticationToken), "Authorization", "Basic");
            Assert.Equal("Authorization", sut.HeaderName);
            Assert.Equal("Basic", sut.AuthenticationType);
            Assert.Null(sut.Current);
            Assert.True(sut.RefreshBufferTimeSpan.TotalMilliseconds > 0);
        }

        [Fact]
        [Trait("Method", "Ctor")]
        public void Ctor_CanDoAuthBearer()
        {
            var sut = new GenericAuthenticationProvider(() => Task.FromResult(ValidAuthenticationToken), "Authorization");
            Assert.Equal("Authorization", sut.HeaderName);
            Assert.Equal("Bearer", sut.AuthenticationType);
            Assert.Null(sut.Current);
            Assert.True(sut.RefreshBufferTimeSpan.TotalMilliseconds > 0);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(100)]
        [Trait("Method", "RefreshBufferTimeSpan")]
        public void RefreshBufferTimeSpan_CannotBeSmall(long ms)
        {
            var sut = new GenericAuthenticationProvider(() => Task.FromResult(ValidAuthenticationToken));
            Assert.Throws<ArgumentException>(() => sut.RefreshBufferTimeSpan = TimeSpan.FromMilliseconds(ms));
        }

        [Fact]
        [Trait("Method", "RefreshBufferTimeSpan")]
        public void RefreshBufferTimeSpan_Roundtrips()
        {
            var ts = TimeSpan.FromMinutes(1);
            var sut = new GenericAuthenticationProvider(() => Task.FromResult(ValidAuthenticationToken)) { RefreshBufferTimeSpan = ts };
            Assert.Equal(ts, sut.RefreshBufferTimeSpan);
        }

        [Fact]
        [Trait("Method", "IsExpired")]
        public void IsExpired_NullToken_ReturnsTrue()
        {
            var sut = new GenericAuthenticationProvider(() => Task.FromResult(ValidAuthenticationToken))
            {
                RefreshBufferTimeSpan = TimeSpan.FromMinutes(2)
            };
            Assert.True(sut.IsExpired(sut.Current));
        }

        [Fact]
        [Trait("Method", "IsExpired")]
        public void IsExpired_NotExpired_ReturnsFalse()
        {
            var sut = new GenericAuthenticationProvider(() => Task.FromResult(ValidAuthenticationToken))
            {
                RefreshBufferTimeSpan = TimeSpan.FromMinutes(2)
            };
            sut.Current = new AuthenticationToken { ExpiresOn = DateTimeOffset.Now.AddMinutes(4) };
            Assert.False(sut.IsExpired(sut.Current));
        }

        [Fact]
        [Trait("Method", "IsExpired")]
        public void IsExpired_InBuffer_ReturnsTrue()
        {
            var sut = new GenericAuthenticationProvider(() => Task.FromResult(ValidAuthenticationToken))
            {
                RefreshBufferTimeSpan = TimeSpan.FromMinutes(2)
            };
            sut.Current = new AuthenticationToken { ExpiresOn = DateTimeOffset.Now.AddMinutes(-1) };
            Assert.True(sut.IsExpired(sut.Current));
        }

        [Fact]
        [Trait("Method", "IsExpired")]
        public void IsExpired_Expired_ReturnsTrue()
        {
            var sut = new GenericAuthenticationProvider(() => Task.FromResult(ValidAuthenticationToken))
            {
                RefreshBufferTimeSpan = TimeSpan.FromMinutes(2)
            };
            sut.Current = new AuthenticationToken { ExpiresOn = DateTimeOffset.Now.AddMinutes(-3) };
            Assert.True(sut.IsExpired(sut.Current));
        }

        [Fact]
        [Trait("Method", "IsExpired")]
        public void IsExpired_ExpiredToken_ReturnsTrue()
        {
            var sut = new GenericAuthenticationProvider(() => Task.FromResult(ValidAuthenticationToken))
            {
                RefreshBufferTimeSpan = TimeSpan.FromMinutes(2)
            };
            Assert.True(sut.IsExpired(ExpiredAuthenticationToken));
        }

        [Fact]
        [Trait("Method", "IsExpired")]
        public void IsExpired_BasicToken_ReturnsFalse()
        {
            var sut = new GenericAuthenticationProvider(() => Task.FromResult(ValidAuthenticationToken))
            {
                RefreshBufferTimeSpan = TimeSpan.FromMinutes(2)
            };
            Assert.False(sut.IsExpired(ValidAuthenticationToken));
        }

        [Fact]
        [Trait("Method", "IsExpired")]
        public void IsExpired_NoExpiration_ReturnsTrue()
        {
            var authtoken = new AuthenticationToken
            {
                DisplayName = ValidAuthenticationToken.DisplayName,
                Token = ValidAuthenticationToken.Token,
                UserId = ValidAuthenticationToken.UserId
            };
            var sut = new GenericAuthenticationProvider(() => Task.FromResult(authtoken))
            {
                RefreshBufferTimeSpan = TimeSpan.FromMinutes(2)
            };
            Assert.True(sut.IsExpired(authtoken));
        }

        [Fact]
        [Trait("Method", "GetTokenAsync")]
        public async Task GetTokenAsync_CallsOnFirstRun()
        {
            var count = 0;
            var requestor = () => { count++; return Task.FromResult(ValidAuthenticationToken); };
            var sut = new GenericAuthenticationProvider(requestor);
            var actual = await sut.GetTokenAsync().ConfigureAwait(false);
            Assert.Equal(ValidAuthenticationToken.Token, actual);
            Assert.Equal(1, count);
        }

        [Fact]
        [Trait("Method", "GetTokenAsync")]
        public async Task GetTokenAsync_CachesResult()
        {
            var count = 0;
            var requestor = () => { count++; return Task.FromResult(ValidAuthenticationToken); };
            var sut = new GenericAuthenticationProvider(requestor);
            var firstCall = await sut.GetTokenAsync().ConfigureAwait(false);
            var secondCall = await sut.GetTokenAsync().ConfigureAwait(false);
            Assert.Equal(ValidAuthenticationToken.Token, firstCall);
            Assert.Equal(ValidAuthenticationToken.Token, secondCall);
            Assert.Equal(1, count);
        }

        [Fact]
        [Trait("Method", "GetTokenAsync")]
        public async Task GetTokenAsync_CallsOnForce()
        {
            var count = 0;
            var requestor = () => { count++; return Task.FromResult(ValidAuthenticationToken); };
            var sut = new GenericAuthenticationProvider(requestor);
            var firstCall = await sut.GetTokenAsync().ConfigureAwait(false);
            Assert.Equal(ValidAuthenticationToken.Token, firstCall);
            var secondCall = await sut.GetTokenAsync(true).ConfigureAwait(false);
            Assert.Equal(ValidAuthenticationToken.Token, secondCall);
            Assert.Equal(2, count);
        }

        [Fact]
        [Trait("Method", "GetTokenAsync")]
        public async Task GetTokenAsync_LogsOutWhenExpired()
        {
            var sut = new GenericAuthenticationProvider(() => Task.FromResult(ValidAuthenticationToken));
            var firstCall = await sut.GetTokenAsync().ConfigureAwait(false);
            Assert.Equal(ValidAuthenticationToken.Token, firstCall);
            Assert.Equal(ValidAuthenticationToken.DisplayName, sut.DisplayName);
            Assert.Equal(ValidAuthenticationToken.UserId, sut.UserId);
            Assert.True(sut.IsLoggedIn);

            sut.TokenRequestorAsync = () => Task.FromResult(ExpiredAuthenticationToken);
            var secondCall = await sut.GetTokenAsync(true).ConfigureAwait(false);
            Assert.Null(secondCall);
            Assert.Null(sut.DisplayName);
            Assert.Null(sut.UserId);
            Assert.False(sut.IsLoggedIn);
        }

        [Fact]
        [Trait("Method", "LoginAsync")]
        public async Task LoginAsync_CallsTokenRequestor()
        {
            var count = 0;
            var requestor = () => { count++; return Task.FromResult(ValidAuthenticationToken); };
            var sut = new GenericAuthenticationProvider(requestor);
            await sut.LoginAsync().ConfigureAwait(false);
            Assert.Equal(1, count);
        }

        [Fact]
        [Trait("Method", "LoginAsync")]
        public async Task LoginAsync_ForcesTokenRequestor()
        {
            var count = 0;
            var requestor = () => { count++; return Task.FromResult(ValidAuthenticationToken); };
            var sut = new GenericAuthenticationProvider(requestor);
            var firstCall = await sut.GetTokenAsync().ConfigureAwait(false);
            Assert.Equal(ValidAuthenticationToken.Token, firstCall);
            await sut.LoginAsync().ConfigureAwait(false);
            Assert.Equal(2, count);
        }

        [Fact]
        [Trait("Method", "SendAsync")]
        public async Task SendAsync_AddsHeader_BearerAuth()
        {
            var handler = new MockDelegatingHandler();
            handler.Responses.Add(new HttpResponseMessage(HttpStatusCode.OK));
            var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/test");
            var sut = new WrappedAuthenticationProvider(() => Task.FromResult(ValidAuthenticationToken))
            {
                InnerHandler = handler
            };

            var response = await sut.WrappedSendAsync(request).ConfigureAwait(false);

            Assert.NotNull(response);
            Assert.Single(handler.Requests);
            var headers = handler.Requests[0].Headers;
            Assert.Equal(ValidAuthenticationToken.Token, headers.Authorization.Parameter);
            Assert.Equal("Bearer", headers.Authorization.Scheme);
        }

        [Fact]
        [Trait("Method", "SendAsync")]
        public async Task SendAsync_AddsHeader_ZumoAuth()
        {
            var handler = new MockDelegatingHandler();
            handler.Responses.Add(new HttpResponseMessage(HttpStatusCode.OK));
            var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/test");
            var sut = new WrappedAuthenticationProvider(() => Task.FromResult(ValidAuthenticationToken), "X-ZUMO-AUTH")
            {
                InnerHandler = handler
            };

            var response = await sut.WrappedSendAsync(request).ConfigureAwait(false);

            Assert.NotNull(response);
            Assert.Single(handler.Requests);
            var headers = handler.Requests[0].Headers;
            Assert.Equal(ValidAuthenticationToken.Token, headers.GetValues("X-ZUMO-AUTH").First());
        }

        [Fact]
        [Trait("Method", "SendAsync")]
        public async Task SendAsync_NoHeader_WhenExpired()
        {
            var handler = new MockDelegatingHandler();
            handler.Responses.Add(new HttpResponseMessage(HttpStatusCode.OK));
            var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/test");
            var sut = new WrappedAuthenticationProvider(() => Task.FromResult(ExpiredAuthenticationToken), "X-ZUMO-AUTH")
            {
                InnerHandler = handler
            };

            var response = await sut.WrappedSendAsync(request).ConfigureAwait(false);

            Assert.NotNull(response);
            Assert.Single(handler.Requests);
            var headers = handler.Requests[0].Headers;
            Assert.False(headers.Contains("X-ZUMO-AUTH"));
        }

        [Fact]
        [Trait("Method", "SendAsync")]
        public async Task SendAsync_RemoveHeader_WhenExpired()
        {
            var handler = new MockDelegatingHandler();
            handler.Responses.Add(new HttpResponseMessage(HttpStatusCode.OK));
            var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/test");
            request.Headers.Add("X-ZUMO-AUTH", "a-test-header");
            var sut = new WrappedAuthenticationProvider(() => Task.FromResult(ExpiredAuthenticationToken), "X-ZUMO-AUTH")
            {
                InnerHandler = handler
            };

            var response = await sut.WrappedSendAsync(request).ConfigureAwait(false);

            Assert.NotNull(response);
            Assert.Single(handler.Requests);
            var headers = handler.Requests[0].Headers;
            Assert.False(headers.Contains("X-ZUMO-AUTH"));
        }

        [Fact]
        [Trait("Method", "SendAsync")]
        public async Task SendAsync_OverwritesHeader_WhenNotExpired()
        {
            var handler = new MockDelegatingHandler();
            handler.Responses.Add(new HttpResponseMessage(HttpStatusCode.OK));
            var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/test");
            request.Headers.Add("X-ZUMO-AUTH", "a-test-header");
            var sut = new WrappedAuthenticationProvider(() => Task.FromResult(ValidAuthenticationToken), "X-ZUMO-AUTH")
            {
                InnerHandler = handler
            };

            var response = await sut.WrappedSendAsync(request).ConfigureAwait(false);

            Assert.NotNull(response);
            Assert.Single(handler.Requests);
            var headers = handler.Requests[0].Headers;
            Assert.Equal(ValidAuthenticationToken.Token, headers.GetValues("X-ZUMO-AUTH").First());
        }
    }
}
