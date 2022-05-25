// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------------------- 

using System;

namespace Microsoft.Azure.Mobile.Server.Cache
{
    /// <summary>
    /// Defines a cache policy describing which HTTP cache response headers to include in an <see cref="System.Net.Http.HttpResponseMessage"/>. 
    /// The <see cref="CachePolicy"/> is used by the <see cref="CachePolicyProvider"/> which defines a default caching policy if no HTTP 
    /// caching headers have been applied to a given <see cref="System.Net.Http.HttpResponseMessage"/>.
    /// </summary>
    public class CachePolicy
    {
        private CacheOptions options = CacheOptions.NoCache;

        /// <summary>
        /// Gets or sets the <see cref="CacheOptions"/> to include with the <c>Cache-Control</c> HTTP header field.
        /// </summary>
        public CacheOptions Options
        {
            get
            {
                return this.options;
            }

            set
            {
                CacheOptionsHelper.Instance.Validate(value, "value");
                this.options = value;
            }
        }

        /// <summary>
        /// Gets or sets the timespan after which the response no longer can be considered fresh. If set then include a 
        /// <c>Cache-Control</c> header field with a <c>Max-Age</c> property set to the value. For backwards compatibility 
        /// with HTTP/1.0 caches, the <c>Expires</c> header field is also set to an equivalent absolute time stamp.
        /// </summary>
        public TimeSpan? MaxAge { get; set; }
    }
}
