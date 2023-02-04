// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Utils;
using System;
using Windows.Foundation.Collections;
using Windows.Storage;

namespace Microsoft.Datasync.Client.Platforms
{
    /// <summary>
    /// An implementation of the <see cref="IApplicationStorage"/> interface for
    /// UWP/UAP/Windows Storage platform.
    /// </summary>
    internal class ApplicationStorage : IApplicationStorage
    {
        internal ApplicationStorage(string containerName = "")
        {
            try
            {
                SharedContainerName = string.IsNullOrWhiteSpace(containerName) ? "ms-datasync-client" : containerName;
                Preferences = ApplicationData.Current.LocalSettings.CreateContainer(SharedContainerName, ApplicationDataCreateDisposition.Always).Values;
                IsPackaged = true;
            }
            catch (InvalidOperationException)
            {
                IsPackaged = false;
                Preferences = null;
            }
        }

        /// <summary>
        /// The name of the shared container for preferences.
        /// </summary>
        private string SharedContainerName { get; }

        /// <summary>
        /// The set of properties for the preference storage.
        /// </summary>
        private IPropertySet Preferences { get; }

        private bool IsPackaged { get; } = false;

        /// <summary>
        /// Clear all the values within the store.
        /// </summary>
        public void ClearValues()
        {
            Preferences?.Clear();
        }

        /// <summary>
        /// Remove a provided key from the store, if it exists.
        /// </summary>
        /// <param name="key">The key</param>
        public void RemoveValue(string key)
        {
            if (IsPackaged && Preferences.ContainsKey(key))
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
            if (IsPackaged)
            {
                Preferences[key] = value;
            }
        }

        /// <summary>
        /// Try to get a value from the key-value application storage.
        /// </summary>
        /// <param name="key">The key</param>
        /// <param name="value">The retrieved value</param>
        /// <returns>True if the key was found</returns>
        public bool TryGetValue(string key, out string value)
        {
            if (IsPackaged && Preferences.TryGetValue(key, out object prefValue))
            {
                value = prefValue.ToString();
                return value != null;
            }

            value = null;
            return false;
        }
    }
}
