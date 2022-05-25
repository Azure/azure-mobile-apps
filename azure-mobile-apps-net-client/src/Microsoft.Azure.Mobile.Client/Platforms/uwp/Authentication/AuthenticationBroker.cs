// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading.Tasks;
using Windows.Security.Authentication.Web;

namespace Microsoft.WindowsAzure.MobileServices
{
    /// <summary>
    /// AuthenticationBroker for the Windows Store Platform that uses the Windows Store
    /// <see cref="WebAuthenticationBroker"/> APIs.
    /// </summary>
    internal class AuthenticationBroker
    {
        /// <summary>
        /// Begins a server-side authentication flow by navigating the 
        /// <see cref="WebAuthenticationBroker"/> to the <paramref name="startUrl"/>.
        /// </summary>
        /// <param name="startUrl">The URL that the browser-based control should 
        /// first navigate to in order to start the authenication flow.
        /// </param>
        /// <param name="endUrl">The URL that indicates the authentication flow has 
        /// completed. Upon being redirected to any URL that starts with the 
        /// endUrl, the browser-based control must stop navigating and
        /// return the response data to the <see cref="AuthenticationBroker"/>.
        /// </param>
        /// <param name="useSingleSignOn">Indicates if single sign-on should be used so 
        /// that users do not have to re-enter his/her credentials every time.
        /// </param>
        /// <returns>
        /// The response data from the authentication flow that contains a string of JSON 
        /// that represents a Mobile Services authentication token.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the user cancels the authentication flow or an error occurs during
        /// the authentication flow.
        /// </exception>
        public async Task<string> AuthenticateAsync(Uri startUrl, Uri endUrl, bool useSingleSignOn)
        {
            WebAuthenticationResult result = await AuthenticateWithBroker(startUrl, endUrl, useSingleSignOn);

            if (result.ResponseStatus != WebAuthenticationStatus.Success)
            {
                string message;
                if (result.ResponseStatus == WebAuthenticationStatus.UserCancel)
                {
                    message = "Authentication was cancelled by the user.";
                }
                else
                {
                    message = $"Authentication failed with HTTP response code {result.ResponseErrorDetail}.";
                }

                throw new InvalidOperationException(message);
            }

            return GetTokenStringFromResult(result);
        }

        /// <summary>
        /// Begins a server-side authentication flow by navigating the 
        /// <see cref="WebAuthenticationBroker"/> to the <paramref name="startUrl"/>.
        /// Considers if the <paramref name="useSingleSignOn"/> is being used and calls the
        /// correct overload of the <see cref="WebAuthenticationBroker"/>.
        /// </summary>
        /// <param name="startUrl">The URL that the browser-based control should 
        /// first navigate to in order to start the authenication flow.
        /// </param>
        /// <param name="endUrl">The URL that indicates the authentication flow has 
        /// completed. Upon being redirected to any URL that starts with the 
        /// <paramref name="endUrl"/>, the browser-based control must stop navigating and
        /// return the response data to the <see cref="AuthenticationBroker"/>.
        /// </param>
        /// <param name="useSingleSignOn">Indicates if single sign-on should be used so 
        /// that users do not have to re-enter his/her credentials every time.
        /// </param>
        /// <returns>
        /// The <see cref="WebAuthenticationResult"/> returned by the 
        /// <see cref="WebAuthenticationBroker"/>.
        /// </returns>
        private async Task<WebAuthenticationResult> AuthenticateWithBroker(Uri startUrl, Uri endUrl, bool useSingleSignOn)
        {
            Arguments.IsNotNull(startUrl, nameof(startUrl));
            Arguments.IsNotNull(endUrl, nameof(endUrl));

            WebAuthenticationResult result = null;
            if (useSingleSignOn)
            {
                Uri ssoEndUri = WebAuthenticationBroker.GetCurrentApplicationCallbackUri();
                Uri ssoStartUrl = GetUrlWithQueryStringParameter(startUrl, "sso_end_uri", ssoEndUri.AbsoluteUri);
                result = await WebAuthenticationBroker.AuthenticateAsync(WebAuthenticationOptions.None, ssoStartUrl);
            }
            else
            {
                result = await WebAuthenticationBroker.AuthenticateAsync(WebAuthenticationOptions.None, startUrl, endUrl);
            }

            return result;
        }

        /// <summary>
        /// Gets the JSON string that represents the Mobile Service authentication token
        /// from the <see cref="WebAuthenticationResult"/>.
        /// </summary>
        /// <param name="result">The <see cref="WebAuthenticationResult"/> returned
        /// from the <see cref="WebAuthenticationBroker"/>.</param>
        /// <returns>
        /// A JSON string that represents a Mobile Service authentication token.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the authentication flow resulted in an error message or an invalid response.
        /// </exception>
        private string GetTokenStringFromResult(WebAuthenticationResult result)
        {
            Arguments.IsNotNull(result, nameof(result));
            if (result.ResponseStatus != WebAuthenticationStatus.Success)
            {
                throw new ArgumentException("Successful Web Authentication required", nameof(result));
            }

            string tokenString = null;

            string responseData = result.ResponseData;
            if (!string.IsNullOrEmpty(responseData))
            {
                tokenString = GetSubStringAfterMatch(responseData, "#token=");
            }

            if (string.IsNullOrEmpty(tokenString))
            {
                string errorString = GetSubStringAfterMatch(responseData, "#error=");
                string message = (string.IsNullOrEmpty(errorString))
                    ? "Invalid format of the authentication response."
                    : $"Login failed: {errorString}";

                throw new InvalidOperationException(message);
            }

            return tokenString;
        }

        /// <summary>
        /// Returns a substring from the <paramref name="stringToSearch"/> starting from
        /// the first character after the <paramref name="matchString"/> if the 
        /// <paramref name="stringToSearch"/> contains the <paramref name="matchString"/>;
        /// otherwise, returns <c>null</c>.
        /// </summary>
        /// <param name="stringToSearch">The string to search for the <paramref name="matchString"/>.
        /// </param>
        /// <param name="matchString">The string to look for in the <paramref name="stringToSearch"/>
        /// </param>
        /// <returns>The substring from <paramref name="stringToSearch"/> that follows the
        /// <paramref name="matchString"/> if the <paramref name="stringToSearch"/> contains 
        /// the <paramref name="matchString"/>; otherwise, returns <c>null</c>.
        /// </returns>
        private string GetSubStringAfterMatch(string stringToSearch, string matchString)
        {
            Arguments.IsNotNull(stringToSearch, nameof(stringToSearch));
            Arguments.IsNotNull(matchString, nameof(matchString));

            string value = null;
            int index = stringToSearch.IndexOf(matchString);
            if (index > 0)
            {
                value = Uri.UnescapeDataString(stringToSearch.Substring(index + matchString.Length));
            }

            return value;
        }

        /// <summary>
        /// Returns a URL that is equivalent to the <paramref name="url"/> provided but which
        /// includes in the query string of the URL the <paramref name="queryParameter"/>
        /// with the value given by <paramref name="queryValue"/>.
        /// </summary>
        /// <param name="url">The URL to add the query string parameter and value to.
        /// </param>
        /// <param name="queryParameter">The name of the query string parameter to add to 
        /// the URL.
        /// </param>
        /// <param name="queryValue">The value of the query string parameter to add to the URL.
        /// </param>
        /// <returns>
        /// A URL that is equivalent to the <paramref name="url"/> provided but which
        /// includes in the query string of the URL the <paramref name="queryParameter"/>
        /// with the value given by <paramref name="queryValue"/>.
        /// </returns>
        internal Uri GetUrlWithQueryStringParameter(Uri url, string queryParameter, string queryValue)
        {
            Arguments.IsNotNull(url, nameof(url));
            Arguments.IsNotNull(queryParameter, nameof(queryParameter));
            Arguments.IsNotNull(queryValue, nameof(queryValue));

            string queryParameterEscaped = Uri.EscapeDataString(queryParameter);
            string queryValueEscaped = Uri.EscapeDataString(queryValue);

            UriBuilder uriBuilder = new UriBuilder(url);

            string queryToAppend = string.Format(CultureInfo.InvariantCulture, "{0}={1}", queryParameterEscaped, queryValueEscaped);
            string query = uriBuilder.Query;

            // Must strip off "?" prefix of query before setting it back to avoid "??" in the query.
            // Because UriBuild.Query property (https://msdn.microsoft.com/en-us/library/system.uribuilder.query) 
            // getter starts with "?", but property setter starts without "?".
            if (!string.IsNullOrEmpty(query) && query.Length > 1)
            {
                query = query.Substring(1) + "&" + queryToAppend;
            }
            else
            {
                query = queryToAppend;
            }

            uriBuilder.Query = query;

            return uriBuilder.Uri;
        }
    }
}