// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------------------- 

using System;

namespace Microsoft.Azure.Mobile.Server.Cache
{
    /// <summary>
    /// Defines the set of HTTP response cache control options that can be used with the HTTP Cache-Control header field for an 
    /// <see cref="System.Net.Http.HttpResponseMessage"/>. This is used by the <see cref="CachePolicyProvider"/> to define which HTTP caching 
    /// response headers to use if no HTTP caching response headers have been set on an <see cref="System.Net.Http.HttpResponseMessage"/>.
    /// </summary>
    [Flags]
    public enum CacheOptions
    {
        /// <summary>
        /// Don't include any cache control directives.
        /// </summary>
        None = 0x00,

        /// <summary>
        /// The "no-cache" response directive indicates that the response MUST NOT be used to satisfy a subsequent 
        /// request without successful validation on the origin server.
        /// </summary>
        NoCache = 0x01,

        /// <summary>
        /// The "no-store" response directive indicates that a cache MUST NOT store any part of either the immediate 
        /// request or response. This directive applies to both private and shared caches. 
        /// </summary>
        NoStore = 0x02,

        /// <summary>
        /// The "must-revalidate" response directive indicates that once it has become stale, a cache MUST NOT use 
        /// the response to satisfy subsequent requests without successful validation on the origin server.
        /// </summary>
        MustRevalidate = 0x04,

        /// <summary>
        /// The "proxy-revalidate" response directive has the same meaning as the must-revalidate response directive, 
        /// except that it does not apply to private caches.
        /// </summary>
        ProxyRevalidate = 0x08,

        /// <summary>
        /// The "no-transform" response directive indicates that an intermediary (regardless of whether it 
        /// implements a cache) MUST NOT transform the payload.
        /// </summary>
        NoTransform = 0x10,

        /// <summary>
        /// The "private" response directive indicates that the response message is intended for a single user and MUST 
        /// NOT be stored by a shared cache.  A private cache MAY store the response and reuse it for later
        /// requests, even if the response would normally be non-cacheable.
        /// </summary>
        Private = 0x20,

        /// <summary>
        /// The "public" response directive indicates that any cache MAY store the response, even if the response would 
        /// normally be non-cacheable or cacheable only within a private cache.
        /// </summary>
        Public = 0x40
    }
}
