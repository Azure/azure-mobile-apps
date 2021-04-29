// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Authentication;

namespace Microsoft.AzureMobile.Server.Authentication
{
    /// <summary>
    /// The options for the App Service Authentication Handler.
    /// </summary>
    public class AppServiceAuthenticationOptions : AuthenticationSchemeOptions
    {
        public AppServiceAuthenticationOptions()
        {
            Events = new object();
        }
    }
}
