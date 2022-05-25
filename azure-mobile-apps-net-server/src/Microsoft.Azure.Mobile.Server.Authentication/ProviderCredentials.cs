// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Security.Claims;
using Newtonsoft.Json;

namespace Microsoft.Azure.Mobile.Server.Authentication
{
    /// <summary>
    /// Base class for all provider specific credentials. Provider
    /// specific subclasses include add their own specific information,
    /// for example access tokens, token secrets, etc.
    /// </summary>
    public abstract class ProviderCredentials
    {
        /// <summary>
        /// Initializes a new instance with the name of the provider associated with this instance.
        /// </summary>
        /// <param name="providerName">The name of the provider.</param>
        protected ProviderCredentials(string providerName)
        {
            if (providerName == null)
            {
                throw new ArgumentNullException("providerName");
            }

            this.Provider = providerName;
        }

        /// <summary>
        /// Gets or sets the name of the provider these credentials are for.
        /// </summary>
        [JsonIgnore]
        public string Provider { get; set; }

        /// <summary>
        /// Gets or sets the provider id for the user. The value can be created and parsed using the methods provided by <see cref="IAppServiceTokenHandler"/>.
        /// </summary>
        [JsonIgnore]
        public string UserId { get; set; }

        /// <summary>
        /// Gets or sets the collection of additional claims associated with this instance. May
        /// be null or empty.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public IEnumerable<Claim> UserClaims { get; set; }
    }
}