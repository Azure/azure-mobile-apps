// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------------------- 

using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Azure.Mobile.Server.Properties;

namespace Microsoft.Azure.Mobile
{
    /// <summary>
    /// Utility class for various HTTP header manipulations
    /// </summary>
    internal static class HttpHeaderUtils
    {
        private static readonly bool[] TokenCharacters = new bool[128];

        [SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline", Justification = "Cannot initialize inline.")]
        static HttpHeaderUtils()
        {
            // Initialize valid token characters as per RFC 2616
            for (int i = 33; i < 127; i++)
            {
                TokenCharacters[i] = true;
            }

            TokenCharacters[(byte)'('] = false;
            TokenCharacters[(byte)')'] = false;
            TokenCharacters[(byte)'<'] = false;
            TokenCharacters[(byte)'>'] = false;
            TokenCharacters[(byte)'@'] = false;
            TokenCharacters[(byte)','] = false;
            TokenCharacters[(byte)';'] = false;
            TokenCharacters[(byte)':'] = false;
            TokenCharacters[(byte)'\\'] = false;
            TokenCharacters[(byte)'"'] = false;
            TokenCharacters[(byte)'/'] = false;
            TokenCharacters[(byte)'['] = false;
            TokenCharacters[(byte)']'] = false;
            TokenCharacters[(byte)'?'] = false;
            TokenCharacters[(byte)'='] = false;
            TokenCharacters[(byte)'{'] = false;
            TokenCharacters[(byte)'}'] = false;
        }

        /// <summary>
        /// Checks whether the given character is a valid HTTP header token character.
        /// </summary>
        /// <param name="character">The character to check.</param>
        /// <returns>true if the character is a token character.</returns>
        public static bool IsTokenChar(char character)
        {
            if (character > 127)
            {
                return false;
            }

            return TokenCharacters[character];
        }

        /// <summary>
        /// Validates whether a given <paramref name="token"/> is a valid HTTP header token.
        /// </summary>
        /// <param name="token">The <see cref="string"/> to validate.</param>
        public static void ValidateToken(string token)
        {
            if (token == null)
            {
                throw new ArgumentNullException("token");
            }

            foreach (char ch in token)
            {
                if (!IsTokenChar(ch))
                {
                    throw new FormatException(CommonResources.HttpHeaderToken_Invalid.FormatForUser(token, ch));
                }
            }
        }

        /// <summary>
        /// Creates a quoted string (starting and ending with '"').
        /// </summary>
        /// <remarks>This method does not validate whether there are additional quotes ('"') within the string. It only
        /// checks whether the string starts and ends with a quote, and if not then quotes it.</remarks>
        /// <param name="value">The string to quote.</param>
        /// <returns>The quoted string</returns>
        public static string GetQuotedString(string value)
        {
            if (string.IsNullOrEmpty(value) || (value.Length > 1 && value.StartsWith("\"", StringComparison.Ordinal) && value.EndsWith("\"", StringComparison.Ordinal)))
            {
                return value;
            }

            return "\"" + value + "\"";
        }

        /// <summary>
        /// Removes a quote pair from a string if present.
        /// </summary>
        /// <param name="value">The string to unquote.</param>
        /// <returns>The unquoted string.</returns>
        public static string GetUnquotedString(string value)
        {
            if (string.IsNullOrEmpty(value) || value.Length < 2 || !(value.StartsWith("\"", StringComparison.Ordinal) && value.EndsWith("\"", StringComparison.Ordinal)))
            {
                return value;
            }

            return value.Substring(1, value.Length - 2);
        }
    }
}
