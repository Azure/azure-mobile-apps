// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

namespace Microsoft.Datasync.Client.Utils
{
    /// <summary>
    /// Provides a key-value store for the storage of configuration data.
    /// </summary>
    internal interface IApplicationStorage
    {
        /// <summary>
        /// Clear all the values within the store.
        /// </summary>
        void ClearValues();

        /// <summary>
        /// Try to get a value from the key-value application storage.
        /// </summary>
        /// <param name="key">The key</param>
        /// <param name="value">The retrieved value</param>
        /// <returns>True if the key was found</returns>
        bool TryGetValue(string key, out string value);

        /// <summary>
        /// Stores the provided key-value pair into the application storage, overwriting
        /// the same key if needed.
        /// </summary>
        /// <param name="key">The key</param>
        /// <param name="value">The value to store</param>
        void SetValue(string key, string value);

        /// <summary>
        /// Remove a provided key from the store, if it exists.
        /// </summary>
        /// <param name="key">The key</param>
        void RemoveValue(string key);
    }
}