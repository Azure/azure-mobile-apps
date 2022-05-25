// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.IO;

namespace Microsoft.WindowsAzure.MobileServices
{
    internal class CurrentPlatform : IPlatform
    {
        /// <summary>
        /// Returns a platform-specific implemention of application storage.
        /// </summary>
        public IApplicationStorage ApplicationStorage { get { return MobileServices.ApplicationStorage.Instance; } }

        /// <summary>
        /// Returns a platform-specific implemention of platform information.
        /// </summary>
        public IPlatformInformation PlatformInformation { get { return MobileServices.PlatformInformation.Instance; } }

        /// <summary>
        /// Returns a platform-specific implementation of a utility class
        /// that provides functionality for manipulating
        /// <see cref="System.Linq.Expressions.Expression"/> instances.
        /// </summary>
        public IExpressionUtility ExpressionUtility { get { return MobileServices.ExpressionUtility.Instance; } }

        /// <summary>
        /// Returns a platform-specific implementation of a utility class
        /// that provides functionality for platform-specifc push capabilities.
        /// </summary>
        public IPushUtility PushUtility => null;

        /// <summary>
        /// Returns a platform-specific path for storing offline databases
        /// that are not fully-qualified.
        /// </summary>
        public string DefaultDatabasePath => Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

        /// <summary>
        /// Retrieves an ApplicationStorage where all items stored are segmented from other stored items
        /// </summary>
        /// <param name="name">The name of the segemented area in application storage</param>
        /// <returns>The specific instance of that segment</returns>
        public IApplicationStorage GetNamedApplicationStorage(string name) => new ApplicationStorage(name);

        /// <summary>
        /// Ensures that a file exists, creating it if necessary
        /// </summary>
        /// <param name="path">The fully-qualified pathname to check</param>
        public void EnsureFileExists(string path)
        {
            if (!File.Exists(path))
            {
                File.Create(path).Dispose();
            }
        }
    }
}