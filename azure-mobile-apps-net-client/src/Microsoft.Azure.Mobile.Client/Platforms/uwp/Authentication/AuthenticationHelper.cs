// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.WindowsAzure.MobileServices
{
    internal class AuthenticationHelper
    {

        private static readonly string EasyAuthCallbackUrlSegment = "easyauth.callback";

        /// <summary>
        /// Occurs when authentication has been successfully or unsuccessfully completed.
        /// </summary>
        public event EventHandler<AuthenticatorCompletedEventArgs> Completed;

        /// <summary>
        /// Occurs when there an error is encountered when authenticating.
        /// </summary>
        public event EventHandler<AuthenticatorErrorEventArgs> Error;

        internal void OnResponseReceived(Uri uri)
        {
            if (!IsValidUrl(uri))
            {
                Error?.Invoke(this, new AuthenticatorErrorEventArgs("Invalid redirect url returned by the server"));
                return;
            }

            try
            {
                var fragments = DecodeResponse(uri.Fragment);
                Completed?.Invoke(this, new AuthenticatorCompletedEventArgs(fragments["authorization_code"]));
            }
            catch (KeyNotFoundException)
            {
                Error?.Invoke(this, new AuthenticatorErrorEventArgs("authorization_code not found in server response"));
            }
            catch (Exception e)
            {
                Error?.Invoke(this, new AuthenticatorErrorEventArgs(e.Message));
            }
        }

        private bool IsValidUrl(Uri uri)
        {
            if (uri != null && uri.Scheme != null && uri.Host != null && uri.Host == EasyAuthCallbackUrlSegment)
            {
                return true;
            }
            return false;
        }

        private Dictionary<string, string> DecodeResponse(string encodedString)
        {
            var inputs = new Dictionary<string, string>();

            if (encodedString.Length > 0)
            {
                char firstChar = encodedString[0];
                if (firstChar == '#')
                {
                    encodedString = encodedString.Substring(1);
                }

                if (encodedString.Length > 0)
                {
                    var parts = encodedString.Split(new char[] { '&' }, StringSplitOptions.RemoveEmptyEntries);

                    foreach (var part in parts)
                    {
                        var equalsIndex = part.IndexOf('=');

                        string key;
                        string value;
                        if (equalsIndex >= 0)
                        {
                            key = Uri.UnescapeDataString(part.Substring(0, equalsIndex));
                            value = Uri.UnescapeDataString(part.Substring(equalsIndex + 1));
                        }
                        else
                        {
                            throw new InvalidOperationException($"Query parameter: {part} value is empty");
                        }

                        inputs[key] = value;
                    }
                }
            }
            return inputs;
        }
    }
}