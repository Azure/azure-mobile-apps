// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.IO;

namespace Microsoft.WindowsAzure.MobileServices
{
    /// <summary>
    /// Provides access to platform specific functionality for the current client platform.
    /// </summary>
    public class CurrentPlatform : IPlatform
    {
        /// <summary>
        /// You must call this method from your application in order to ensure
        /// that this platform specific assembly is included in your app.
        /// </summary>
        public static void Init()
        {
        }

        /// <summary>
        /// Returns a platform-specific implemention of application storage.
        /// </summary>
        IApplicationStorage IPlatform.ApplicationStorage => ApplicationStorage.Instance;

        /// <summary>
        /// Returns a platform-specific implemention of platform information.
        /// </summary>
        IPlatformInformation IPlatform.PlatformInformation => PlatformInformation.Instance;

        /// <summary>
        /// Returns a platform-specific implementation of a utility class
        /// that provides functionality for manipulating
        /// <see cref="System.Linq.Expressions.Expression"/> instances.
        /// </summary>
        IExpressionUtility IPlatform.ExpressionUtility => ExpressionUtility.Instance;

        /// <summary>
        /// Returns a platform-specific implementation of a utility class
        /// that provides functionality for platform-specifc push capabilities.
        /// </summary>
        IPushUtility IPlatform.PushUtility => PushUtility.Instance;

        /// <summary>
        /// Returns a platform-specific path for storing offline databases
        /// that are not fully-qualified.
        /// </summary>
        string IPlatform.DefaultDatabasePath => Environment.GetFolderPath(Environment.SpecialFolder.Personal);

        /// <summary>
        /// Retrieves an ApplicationStorage where all items stored are segmented from other stored items
        /// </summary>
        /// <param name="name">The name of the segemented area in application storage</param>
        /// <returns>The specific instance of that segment</returns>
        IApplicationStorage IPlatform.GetNamedApplicationStorage(string name) => new ApplicationStorage(name);

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