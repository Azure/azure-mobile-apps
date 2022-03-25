// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Utils;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.IsolatedStorage;

namespace Microsoft.Datasync.Client.Platforms
{
    /// <summary>
    /// An implementation of the <see cref="IApplicationStorage"/> interface for
    /// .NET Standard Storage platform.
    /// </summary>
    internal class ApplicationStorage : IApplicationStorage
    {
        internal ApplicationStorage(IsolatedStorageFile storageLocation, string containerName = "")
        {
            Arguments.IsNotNull(storageLocation, nameof(storageLocation));

            StorageLocation = storageLocation;
            SharedContainerName = string.IsNullOrWhiteSpace(containerName) ? "ms-datasync-client" : containerName;
            LoadPreferences();
        }

        /// <summary>
        /// The name of the shared container for preferences.
        /// </summary>
        private string SharedContainerName { get; }

        /// <summary>
        /// The storage location for preferences
        /// </summary>
        private IsolatedStorageFile StorageLocation { get; }

        /// <summary>
        /// Internal storage for the preferences file.
        /// </summary>
        private Dictionary<string, string> Preferences { get; set; }

        /// <summary>
        /// Clear all the values within the store.
        /// </summary>
        /// <remarks>We can't test the catch block, but rest of this method is covered.</remarks>
        [ExcludeFromCodeCoverage]
        public void ClearValues()
        {
            Preferences.Clear();
            try
            {
                StorageLocation.Remove();
            }
            catch
            {
                // Swallow any errors
            }
        }

        /// <summary>
        /// Remove a provided key from the store, if it exists.
        /// </summary>
        /// <param name="key">The key</param>
        public void RemoveValue(string key)
        {
            if (Preferences.ContainsKey(key))
            {
                Preferences.Remove(key);
                SavePreferences();
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
            Preferences[key] = value;
            SavePreferences();
        }

        /// <summary>
        /// Try to get a value from the key-value application storage.
        /// </summary>
        /// <param name="key">The key</param>
        /// <param name="value">The retrieved value</param>
        /// <returns>True if the key was found</returns>
        public bool TryGetValue(string key, out string value)
        {
            if (Preferences.TryGetValue(key, out string pref))
            {
                value = pref;
                return value != null;
            }

            value = null;
            return false;
        }

        /// <summary>
        /// Convert the <see cref="SharedContainerName"/> into a filename.
        /// </summary>
        private string Filename { get => $"{SharedContainerName}.json"; }

        /// <summary>
        /// Load the preferences (if they exist) from disk,
        /// </summary>
        private void LoadPreferences()
        {
            try
            {
                using var stream = StorageLocation.OpenFile(Filename, FileMode.Open, FileAccess.Read);
                using var reader = new StreamReader(stream);
                var jsonText = reader.ReadToEnd();
                Preferences = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonText) ?? new Dictionary<string, string>();
            }
            catch
            {
                Preferences = new Dictionary<string, string>();
            }
        }


        /// <summary>
        /// Save the preferences (if they exist) to disk.
        /// </summary>
        /// <remarks>
        /// There is no way to test the catch clause here, and plenty of tests cover the inner block
        /// of the SavePreferences() call.  As a result, we need to exclude from code coverage.
        /// </remarks>
        [ExcludeFromCodeCoverage]
        private void SavePreferences()
        {
            try
            {
                var jsonText = JsonConvert.SerializeObject(Preferences);
                using var stream = StorageLocation.OpenFile(Filename, FileMode.Create, FileAccess.Write);
                using var writer = new StreamWriter(stream);
                writer.WriteLine(jsonText);
            }
            catch
            {
                // Swallow errors here
            }
        }
    }
}
