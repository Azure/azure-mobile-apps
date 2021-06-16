// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Internal;
using System.Collections.Generic;

namespace Microsoft.Datasync.Client.Extensions
{
    /// <summary>
    /// A set of extension methods for processing bulk-requests to dictionary collections.
    /// </summary>
    internal static class IDictionaryExtensions
    {
        /// <summary>
        /// Adds a list of key value pairs to the specified dictionary, returning the dictionary for fluent chaining.
        /// </summary>
        /// <typeparam name="TKey">The key type</typeparam>
        /// <typeparam name="TValue">The value type</typeparam>
        /// <param name="dict">The dictionary to modify</param>
        /// <param name="keyValuePairs">The list of updates</param>
        /// <returns>The modified dictionary</returns>
        internal static IDictionary<TKey, TValue> AddRange<TKey, TValue>(this IDictionary<TKey, TValue> dict, IEnumerable<KeyValuePair<TKey, TValue>> keyValuePairs)
        {
            Validate.IsNotNull(keyValuePairs, nameof(keyValuePairs));

            foreach (var kv in keyValuePairs)
            {
                if (dict.ContainsKey(kv.Key))
                {
                    dict.Remove(kv.Key);
                }
                dict.Add(kv.Key, kv.Value);
            }
            return dict;
        }
    }
}
