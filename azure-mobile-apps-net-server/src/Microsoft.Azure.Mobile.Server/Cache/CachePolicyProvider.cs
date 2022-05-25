// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Microsoft.Azure.Mobile.Server.Cache
{
    /// <summary>
    /// Default implementation of <see cref="ICachePolicyProvider"/> with a set of possible default HTTP caching policies.
    /// If no HTTP caching headers have been set on a response then this abstraction is called allowing for a default
    /// set of HTTP caching headers to be applied.
    /// </summary>
    public class CachePolicyProvider : ICachePolicyProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CachePolicyProvider"/> class with a default <see cref="CachePolicy"/>.
        /// </summary>
        public CachePolicyProvider()
            : this(new CachePolicy())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CachePolicyProvider"/> class with the given <paramref name="policy"/>.
        /// </summary>
        /// <param name="policy">The <see cref="CachePolicy"/> to use if no HTTP caching response headers have been set on
        /// an <see cref="HttpResponseMessage"/>.</param>
        public CachePolicyProvider(CachePolicy policy)
        {
            if (policy == null)
            {
                throw new ArgumentNullException("policy");
            }

            this.Policy = policy;
        }

        /// <summary>
        /// Gets the <see cref="CachePolicy"/> being applied if no HTTP caching response headers have been set on
        /// an <see cref="HttpResponseMessage"/>.
        /// </summary>
        public CachePolicy Policy { get; private set; }

        /// <inheritdoc />
        public void SetCachePolicy(HttpResponseMessage response)
        {
            if (response == null)
            {
                throw new ArgumentNullException("response");
            }

            this.SetCacheOptions(response);
            this.SetMaxAge(response);
        }

        /// <summary>
        /// Applies the <see cref="CachePolicy"/> options to the given <paramref name="response"/>.
        /// </summary>
        /// <param name="response">The current <see cref="HttpResponseMessage"/>.</param>
        protected virtual void SetCacheOptions(HttpResponseMessage response)
        {
            if (response == null)
            {
                throw new ArgumentNullException("response");
            }

            CacheOptions options = this.Policy.Options;
            CacheControlHeaderValue cacheControl = response.Headers.CacheControl ?? new CacheControlHeaderValue();
            bool hasCacheOptions = false;
            if ((options & CacheOptions.NoStore) != 0)
            {
                hasCacheOptions = true;
                cacheControl.NoStore = true;
                ForceNoCache(response);
            }

            if ((options & CacheOptions.NoCache) != 0)
            {
                hasCacheOptions = true;
                cacheControl.NoCache = true;
                ForceNoCache(response);
            }

            if ((options & CacheOptions.MustRevalidate) != 0)
            {
                hasCacheOptions = true;
                cacheControl.MustRevalidate = true;
            }

            if ((options & CacheOptions.ProxyRevalidate) != 0)
            {
                hasCacheOptions = true;
                cacheControl.ProxyRevalidate = true;
            }

            if ((options & CacheOptions.NoTransform) != 0)
            {
                hasCacheOptions = true;
                cacheControl.NoTransform = true;
            }

            if ((options & CacheOptions.Private) != 0)
            {
                hasCacheOptions = true;
                cacheControl.Private = true;
            }

            if ((options & CacheOptions.Public) != 0)
            {
                hasCacheOptions = true;
                cacheControl.Public = true;
            }

            if (hasCacheOptions)
            {
                response.Headers.CacheControl = cacheControl;
            }
        }

        /// <summary>
        /// Applies the <see cref="CachePolicy"/> <c>MaxAge</c> value to the given <paramref name="response"/>.
        /// </summary>
        /// <param name="response">The current <see cref="HttpResponseMessage"/>.</param>
        protected virtual void SetMaxAge(HttpResponseMessage response)
        {
            if (response == null)
            {
                throw new ArgumentNullException("response");
            }

            if (this.Policy.MaxAge.HasValue)
            {
                CacheControlHeaderValue cacheControl = response.Headers.CacheControl ?? new CacheControlHeaderValue();
                cacheControl.MaxAge = this.Policy.MaxAge;
                response.Headers.CacheControl = cacheControl;

                // If there is an HTTP response content then set Expires header field
                if (response.Content != null)
                {
                    response.Content.Headers.Expires = DateTimeOffset.UtcNow + this.Policy.MaxAge.Value;
                }
            }
        }

        /// <summary>
        /// For backwards compatibility with HTTP/1.0 caches, set the Expires header and Pragma headers
        /// to force no cache.
        /// </summary>
        /// <param name="response">The current <see cref="HttpResponseMessage"/>.</param>
        internal static void ForceNoCache(HttpResponseMessage response)
        {
            response.Headers.Pragma.Add(new NameValueHeaderValue("no-cache"));
            if (response.Content != null)
            {
                response.Content.Headers.TryAddWithoutValidation("Expires", "0");
            }
        }
    }
}