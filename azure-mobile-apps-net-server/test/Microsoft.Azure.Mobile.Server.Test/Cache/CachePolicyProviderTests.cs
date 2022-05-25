// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------------------- 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using TestUtilities;
using Xunit;

namespace Microsoft.Azure.Mobile.Server.Cache
{
    public class CachePolicyProviderTests
    {
        private HttpResponseMessage response;
        private CachePolicy policy;
        private CachePolicyProvider provider;

        public CachePolicyProviderTests()
        {
            this.response = new HttpResponseMessage();
            this.response.Content = new StringContent("Hello");
            this.policy = new CachePolicy();
            this.provider = new CachePolicyProvider(this.policy);
        }

        public static TheoryDataCollection<CacheOptions, Func<HttpResponseMessage, bool>> CacheControlOptions
        {
            get
            {
                return new TheoryDataCollection<CacheOptions, Func<HttpResponseMessage, bool>>
                {
                    { CacheOptions.NoStore, rsp => rsp.Headers.CacheControl.NoStore },
                    { CacheOptions.NoCache, rsp => rsp.Headers.CacheControl.NoCache },
                    { CacheOptions.MustRevalidate, rsp => rsp.Headers.CacheControl.MustRevalidate },
                    { CacheOptions.ProxyRevalidate, rsp => rsp.Headers.CacheControl.ProxyRevalidate },
                    { CacheOptions.NoTransform, rsp => rsp.Headers.CacheControl.NoTransform },
                    { CacheOptions.Private, rsp => rsp.Headers.CacheControl.Private },
                    { CacheOptions.Public, rsp => rsp.Headers.CacheControl.Public },
                };
            }
        }

        [Fact]
        public void Policy_Roundtrips()
        {
            CachePolicy roundtrip = new CachePolicy();
            PropertyAssert.Roundtrips(this.provider, p => p.Policy, PropertySetter.NullRoundtrips, defaultValue: this.policy, roundtripValue: roundtrip);
        }

        [Fact]
        public void SetCachePolicy_SetsMaxAgeCacheControlAndExpires()
        {
            // Arrange
            this.policy.Options = CacheOptions.None;
            this.policy.MaxAge = TimeSpan.FromHours(1);

            // Act
            this.provider.SetCachePolicy(this.response);

            // Assert
            Assert.Equal("max-age=3600", this.response.Headers.CacheControl.ToString());
            Assert.NotNull(this.response.Content.Headers.Expires);
        }

        [Theory]
        [InlineData(CacheOptions.NoCache)]
        [InlineData(CacheOptions.NoStore)]
        public void SetCachePolicy_SetsExpiresAndPragma_IfNoCacheOrNoStore(CacheOptions options)
        {
            // Arrange
            this.policy.Options = options;

            // Act
            this.provider.SetCachePolicy(this.response);

            // Assert
            Assert.Equal("no-cache", this.response.Headers.Pragma.ToString());
            IEnumerable<string> expires;
            this.response.Content.Headers.TryGetValues("Expires", out expires);
            Assert.Equal("0", expires.Single());
        }

        [Fact]
        public void SetCachePolicy_DoesNotSetCacheControlIfNoOptions()
        {
            this.policy.Options = CacheOptions.None;
            this.policy.MaxAge = null;

            // Act
            this.provider.SetCachePolicy(this.response);

            // Assert
            Assert.Null(this.response.Headers.CacheControl);
        }

        [Fact]
        public void SetCachePolicy_SetsCacheControlOnlyIfNoResponseContent()
        {
            // Arrange
            this.response.Content = null;
            this.policy.Options = CacheOptions.None;
            this.policy.MaxAge = TimeSpan.FromHours(1);

            // Act
            this.provider.SetCachePolicy(this.response);

            // Assert
            Assert.Equal("max-age=3600", this.response.Headers.CacheControl.ToString());
        }

        [Theory]
        [MemberData("CacheControlOptions")]
        public void SetCachePolicy_SetsCacheOptions(CacheOptions options, Func<HttpResponseMessage, bool> expected)
        {
            this.policy.Options = options;
            this.policy.MaxAge = null;

            // Act
            this.provider.SetCachePolicy(this.response);

            // Assert
            Assert.True(expected(this.response));
        }
    }
}
