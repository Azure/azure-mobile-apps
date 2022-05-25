// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System.Collections.Generic;

namespace Microsoft.WindowsAzure.MobileServices
{
    internal static class IDictionaryExtensions
    {
        public static IDictionary<TKey, TValue> Add<TKey,TValue>(this IDictionary<TKey, TValue> thisDictionary, IDictionary<TKey, TValue> values)
        {
            foreach (var value in values)
            {
                thisDictionary.Add(value);
            }
            return thisDictionary;
        }
    }
}
