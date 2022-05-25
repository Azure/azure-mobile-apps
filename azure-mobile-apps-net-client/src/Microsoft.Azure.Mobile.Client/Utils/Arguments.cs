// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;

namespace Microsoft.WindowsAzure.MobileServices
{
    /// <summary>
    /// A set of static methods to enforce the contract of a method.
    /// </summary>
    internal static class Arguments
    {
        public static void IsNotNull(object param, string paramName)
        {
            if (param == null)
            {
                throw new ArgumentNullException(paramName);
            }
        }

        public static void IsNotNullOrWhiteSpace(string param, string paramName)
        {
            IsNotNull(param, paramName);
            if (string.IsNullOrWhiteSpace(param))
            {
                throw new ArgumentException($"'{param}' cannot have whitespace", paramName);
            }
        }

        public static void IsNotNullOrEmpty(string param, string paramName)
        {
            IsNotNull(param, paramName);
            if (string.IsNullOrEmpty(param))
            {
                throw new ArgumentException($"'{param}' cannot have whitespace", paramName);
            }
        }

        public static void IsPositiveInteger(int param, string paramName)
        {
            if (param < 0)
            {
                throw new ArgumentOutOfRangeException(paramName);
            }
        }
    }
}
