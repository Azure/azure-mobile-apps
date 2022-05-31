// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Utils;
using Microsoft.Maui.Storage;

namespace Microsoft.Datasync.Client.Platforms
{
    /// <summary>
    /// Implementation of the <see cref="IApplicationStorage"/> interface for
    /// Android devices.
    /// </summary>
    internal class ApplicationStorage : IApplicationStorage
    {
        /// <summary>
        /// The storage prefix.
        /// </summary>
        private string SharedContainerName { get; }

        /// <summary>
        /// Creates a new <see cref="ApplicationStorage"/> instance.
        /// </summary>
        /// <param name="containerName">The optional storage prefix.</param>
        internal ApplicationStorage(string containerName = "")
        {
            SharedContainerName = string.IsNullOrWhiteSpace(containerName) ? "ms-datasync-client" : containerName;
        }

        /// <summary>
        /// Clear all the values within the store.
        /// </summary>
        public void ClearValues()
        {
            Preferences.Clear(SharedContainerName);
        }

        /// <summary>
        /// Remove a provided key from the store, if it exists.
        /// </summary>
        /// <param name="key">The key</param>
        public void RemoveValue(string key)
        {
            if (Preferences.ContainsKey(key, SharedContainerName))
            {
                Preferences.Remove(key);
            }
        }

        /// <summary>
        /// Stores the provided key-value pair into the application storage, overwriting
        /// the same key if needed.
        /// </summary>
        /// <param name="key">The key</param>
        /// <param name="value">The value to store</param>
        public void SetValue(string key, string value)
        {
            Preferences.Set(key, value, SharedContainerName);
        }

        /// <summary>
        /// Try to get a value from the key-value application storage.
        /// </summary>
        /// <param name="key">The key</param>
        /// <param name="value">The retrieved value</param>
        /// <returns>True if the key was found</returns>
        public bool TryGetValue(string key, out string value)
        {
            if (Preferences.ContainsKey(key, SharedContainerName))
            {
                value = Preferences.Get(key, null, SharedContainerName);
                return value != null;
            }
            value = null;
            return false;
        }
    }
}
