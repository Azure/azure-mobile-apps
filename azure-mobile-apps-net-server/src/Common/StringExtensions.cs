// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------------------- 

using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;

namespace System
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal static class StringExtensions
    {
        /// <summary>
        /// Formats the input string to be user visible using <see cref="CultureInfo.CurrentCulture"/>.
        /// </summary>
        public static string FormatForUser(this string format, params object[] args)
        {
            return string.Format(CultureInfo.CurrentCulture, format, args);
        }

        /// <summary>
        /// Formats the input string to be user visible using <see cref="CultureInfo.InvariantCulture"/>.
        /// </summary>
        public static string FormatInvariant(this string format, params object[] args)
        {
            return string.Format(CultureInfo.InvariantCulture, format, args);
        }

        /// <summary>
        /// Splits a string into segments based on a given <paramref name="separator"/>. The segments are 
        /// trimmed and empty segments are removed.
        /// </summary>
        /// <param name="input">The string to split.</param>
        /// <param name="separator">An array of Unicode characters that delimit the substrings in this instance, an empty array that contains no delimiters, or null.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> containing the resulting segments.</returns>
        public static IEnumerable<string> SplitAndTrim(this string input, params char[] separator)
        {
            if (input == null)
            {
                return Enumerable.Empty<string>();
            }

            return input.Split(separator).Select(x => x.Trim()).Where(x => !string.IsNullOrWhiteSpace(x));
        }
    }
}
