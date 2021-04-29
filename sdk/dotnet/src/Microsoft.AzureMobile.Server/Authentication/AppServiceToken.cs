// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.AzureMobile.Server.Authentication
{
    internal class AppServiceToken
    {
        /// <summary>
        /// Creates a new <see cref="AppServiceToken"/> from the encoded string.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public static AppServiceToken FromString(string token)
        {
            var bytes = Convert.FromBase64String(token);
            var json = Encoding.UTF8.GetString(bytes);
            return JsonConvert.DeserializeObject<AppServiceToken>(json);
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
