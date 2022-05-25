// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices.Internal;
using Newtonsoft.Json.Linq;

namespace Microsoft.WindowsAzure.MobileServices
{
    /// <summary>
    ///  Provides extension methods on <see cref="MobileServiceClient"/>.
    /// </summary>
    public static partial class MobileServiceClientExtensions
    {
        /// <summary>
        /// Name of the  JSON member in the token object that stores the
        /// authentication token fo Microsoft Account.
        /// </summary>
        private const string MicrosoftAccountLoginAsyncAuthenticationTokenKey = "authenticationToken";

        /// <summary>
        /// Log a user into a Mobile Services application given a Microsoft
        /// Account authentication token.
        /// </summary>
        /// <param name="thisClient">
        /// The client with which to login.
        /// </param>
        /// <param name="authenticationToken">
        /// Live SDK session authentication token.
        /// </param>
        /// <returns>
        /// Task that will complete when the user has finished authentication.
        /// </returns>
        public static Task<MobileServiceUser> LoginWithMicrosoftAccountAsync(this MobileServiceClient thisClient, string authenticationToken)
        {
            JObject token = new JObject
            {
                [MicrosoftAccountLoginAsyncAuthenticationTokenKey] = authenticationToken
            };
            MobileServiceTokenAuthentication tokenAuth = new MobileServiceTokenAuthentication(thisClient,
                MobileServiceAuthenticationProvider.MicrosoftAccount.ToString(),
                token, parameters: null);

            return tokenAuth.LoginAsync();
        }
    }
}
