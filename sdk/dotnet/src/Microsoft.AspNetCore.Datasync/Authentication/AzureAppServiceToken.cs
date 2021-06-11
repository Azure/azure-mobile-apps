// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.AspNetCore.Authentication
{
    /// <summary>
    /// Model for the Azure App Service authentication token.
    /// </summary>
    internal class AzureAppServiceToken
    {
        /// <summary>
        /// Creates a new <see cref="AppServiceToken"/> from the encoded string.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public static AzureAppServiceToken FromString(string token)
        {
            if (token == null)
            {
                throw new ArgumentNullException(nameof(token));
            }
            if (string.IsNullOrEmpty(token))
            {
                throw new ArgumentException("Empty string", nameof(token));
            }
            try
            {
                var bytes = Convert.FromBase64String(token);
                var json = Encoding.UTF8.GetString(bytes);
                return JsonConvert.DeserializeObject<AzureAppServiceToken>(json);
            }
            catch (Exception ex)
            {
                throw new ArgumentException("Invalid token", nameof(token), ex);
            }
        }

        /// <summary>
        /// The provider (auth type)
        /// </summary>
        [JsonProperty("auth_typ")]
        public string Provider { get; set; }

        /// <summary>
        /// The list of claims
        /// </summary>
        [JsonProperty("claims")]
        public IEnumerable<UserClaim> Claims { get; set; }

        /// <summary>
        /// The schema type for the name within the claims
        /// </summary>
        [JsonProperty("name_typ")]
        public string NameType { get; set; }

        /// <summary>
        /// The schema type for the roles in the claims
        /// </summary>
        [JsonProperty("role_typ")]
        public string RoleType { get; set; }

        /// <summary>
        /// Basic form of a user claim
        /// </summary>
        public class UserClaim
        {
            [JsonProperty("typ")]
            public string Type { get; set; }

            [JsonProperty("val")]
            public string Value { get; set; }
        }
    }
}
