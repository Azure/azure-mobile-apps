// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------------------- 

using System;
using System.Globalization;
using System.Text;

namespace Microsoft.Azure.Mobile
{
    /// <summary>
    /// Provides various string utilities.
    /// </summary>
    internal static class StringUtils
    {
        /// <summary>
        /// Converts a string to a camel case representation, e.g. <c>thisIsCamelCasing</c> with
        /// an initial lower case character.
        /// </summary>
        /// <param name="value">The string to convert.</param>
        /// <returns>The camel cased string; or the original string if no modifications were necessary.</returns>
        public static string ToCamelCase(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }

            if (char.IsLower(value[0]))
            {
                return value;
            }

            StringBuilder stringBuilder = new StringBuilder();
            for (int i = 0; i < value.Length; i++)
            {
                bool flag = i + 1 < value.Length;
                if (i != 0 && flag && char.IsLower(value[i + 1]))
                {
                    stringBuilder.Append(value.Substring(i));
                    break;
                }

                char lower = char.ToLower(value[i], CultureInfo.InvariantCulture);
                stringBuilder.Append(lower);
            }

            return stringBuilder.ToString();
        }

        /// <summary>
        /// Converts a string to a Pascal case representation, e.g. <c>ThisIsPascalCasing</c> with 
        /// an initial upper case character.
        /// </summary>
        /// <param name="value">The string to convert.</param>
        /// <returns>The Pascal cased string; or the original string if no modifications were necessary.</returns>
        public static string ToPascalCase(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }

            if (char.IsUpper(value[0]))
            {
                return value;
            }

            StringBuilder pascal = new StringBuilder(value);
            pascal[0] = Char.ToUpperInvariant(value[0]);
            return pascal.ToString();
        }
    }
}
