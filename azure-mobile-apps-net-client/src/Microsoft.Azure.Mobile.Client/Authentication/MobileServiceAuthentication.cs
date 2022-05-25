// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.WindowsAzure.MobileServices.Internal
{
    /// <summary>
    /// Provides login functionality for the <see cref="MobileServiceClient"/>. 
    /// </summary>
    public abstract class MobileServiceAuthentication
    {
        /// <summary>
        /// Name of the  JSON member in the config setting that stores the
        /// authentication token.
        /// </summary>
        private const string LoginAsyncAuthenticationTokenKey = "authenticationToken";

        /// <summary>
        /// Relative URI fragment of the login endpoint.
        /// </summary>
        public const string LoginAsyncUriFragment = "/.auth/login";

        /// <summary>
        /// Relative URI fragment of the login/done endpoint.
        /// </summary>
        public const string LoginAsyncDoneUriFragment = "/.auth/login/done";

        /// <summary>
        /// Name of the authentication provider as expected by the service REST API.
        /// </summary>
        private string providerName;

        /// <summary>
        /// The name for the Azure Active Directory authentication provider as used by the
        /// service REST API.
        /// </summary>
        public const string WindowsAzureActiveDirectoryRestApiPathName = "aad";

        /// <summary>
        /// Initializes a new instance of the <see cref="MobileServiceAuthentication"/> class.
        /// </summary>
        /// <param name="client">
        /// The <see cref="MobileServiceClient"/> associated with this 
        /// MobileServiceLogin instance.
        /// </param>
        /// <param name="providerName">
        /// The <see cref="MobileServiceAuthenticationProvider"/> used to authenticate.
        /// </param>
        /// <param name="parameters">
        /// Provider specific extra parameters that are sent as query string parameters to login endpoint.
        /// </param>
        public MobileServiceAuthentication(IMobileServiceClient client, string providerName, IDictionary<string, string> parameters)
        {
            Arguments.IsNotNull(client, nameof(client));
            Arguments.IsNotNull(providerName, nameof(providerName));

            this.Client = client;
            this.Parameters = parameters;
            this.ProviderName = providerName;
            string path = MobileServiceUrlBuilder.CombinePaths(LoginAsyncUriFragment, this.ProviderName);
            string loginAsyncDoneUriFragment = MobileServiceAuthentication.LoginAsyncDoneUriFragment;
            if (!string.IsNullOrEmpty(this.Client.LoginUriPrefix))
            {
                path = MobileServiceUrlBuilder.CombinePaths(this.Client.LoginUriPrefix, this.ProviderName);
                loginAsyncDoneUriFragment = MobileServiceUrlBuilder.CombinePaths(this.Client.LoginUriPrefix, "done");
            }
            string queryString = MobileServiceUrlBuilder.GetQueryString(parameters, useTableAPIRules: false);
            string pathAndQuery = MobileServiceUrlBuilder.CombinePathAndQuery(path, queryString);

            this.StartUri = new Uri(this.Client.MobileAppUri, pathAndQuery);
            this.EndUri = new Uri(this.Client.MobileAppUri, loginAsyncDoneUriFragment);

            if (this.Client.AlternateLoginHost != null)
            {
                this.StartUri = new Uri(this.Client.AlternateLoginHost, pathAndQuery);
                this.EndUri = new Uri(this.Client.AlternateLoginHost, loginAsyncDoneUriFragment);
            }
        }

        /// <summary>
        /// The <see cref="MobileServiceClient"/> associated with this <see cref="MobileServiceAuthentication"/> instance.
        /// </summary>
        public IMobileServiceClient Client { get; private set; }

        /// <summary>
        /// The name of the authentication provider used by this <see cref="MobileServiceAuthentication"/> instance.
        /// </summary>
        public string ProviderName
        {
            get { return providerName; }
            private set
            {
                providerName = value.ToLowerInvariant();
                if (providerName.Equals(MobileServiceAuthenticationProvider.WindowsAzureActiveDirectory.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    providerName = WindowsAzureActiveDirectoryRestApiPathName;
                }
            }
        }

        /// <summary>
        /// Provider specific extra parameters that are sent as query string parameters to login endpoint.
        /// </summary>
        public IDictionary<string, string> Parameters { get; private set; }

        /// <summary>
        /// The start uri to use for authentication.
        /// The browser-based control should 
        /// first navigate to this Uri in order to start the authenication flow.
        /// </summary>
        public Uri StartUri { get; private set; }

        /// <summary>
        /// The end Uri to use for authentication.
        /// This Uri indicates that the authentication flow has 
        /// completed. Upon being redirected to any URL that starts with the 
        /// endUrl, the browser-based control must stop navigating and
        /// return the response data.
        /// </summary>
        public Uri EndUri { get; private set; }

        /// <summary>
        /// Log a user into a Mobile Services application with the provider name and
        /// optional token object from this instance.
        /// </summary>
        /// <returns>
        /// Task that will complete when the user has finished authentication.
        /// </returns>
        public async Task<MobileServiceUser> LoginAsync()
        {
            string response = await this.LoginAsyncOverride();
            if (!string.IsNullOrEmpty(response))
            {
                JToken authToken = JToken.Parse(response);

                // Get the Mobile Services auth token and user data
                this.Client.CurrentUser = new MobileServiceUser((string)authToken["user"]["userId"])
                {
                    MobileServiceAuthenticationToken = (string)authToken[LoginAsyncAuthenticationTokenKey]
                };
            }

            return this.Client.CurrentUser;
        }

        /// <summary>
        /// Provides Login logic.
        /// </summary>
        /// <returns>
        /// Task that will complete with the response string when the user has finished authentication.
        /// </returns>
        public abstract Task<string> LoginAsyncOverride();
    }
}
