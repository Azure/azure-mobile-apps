// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------------------- 

using System.Net.Http;

namespace Microsoft.Azure.Mobile.Server.Cache
{
    /// <summary>
    /// Provides an abstraction for adding default HTTP caching headers to a given <see cref="HttpResponseMessage"/>.
    /// If no HTTP caching headers have been set on a response then this abstraction is called allowing for a default
    /// set of HTTP caching headers to be applied. By default, the <see cref="CachePolicyProvider"/> implementation is 
    /// registered with the dependency injection engine but other implementations can be registered as well. If
    /// no <see cref="ICachePolicyProvider"/> is registered then no default caching headers will be added to an
    /// <see cref="HttpResponseMessage"/>.
    /// </summary>
    public interface ICachePolicyProvider
    {
        /// <summary>
        /// Applies the default caching header policy to a given <paramref name="response"/>.
        /// </summary>
        /// <param name="response">The current <see cref="HttpResponseMessage"/>.</param>
        void SetCachePolicy(HttpResponseMessage response);
    }
}
