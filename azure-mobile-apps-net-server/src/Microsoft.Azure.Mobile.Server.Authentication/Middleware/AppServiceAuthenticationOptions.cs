// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Microsoft.Owin.Security;

namespace Microsoft.Azure.Mobile.Server.Authentication
{
    /// <summary>
    /// The <see cref="AppServiceAuthenticationOptions"/> provides options for the OWIN <see cref="AppServiceAuthenticationMiddleware"/> class.
    /// </summary>
    public class AppServiceAuthenticationOptions : AuthenticationOptions
    {
        public const string AuthenticationName = "MobileApp";

        public AppServiceAuthenticationOptions()
            : base(AuthenticationName)
        {
            this.AuthenticationMode = AuthenticationMode.Active;
        }

        public IEnumerable<string> ValidAudiences { get; set; }

        public IEnumerable<string> ValidIssuers { get; set; }

        public IAppServiceTokenHandler TokenHandler { get; set; }

        /// <summary>
        /// Gets or sets the application signing key.
        /// </summary>
        public string SigningKey { get; set; }
    }
}