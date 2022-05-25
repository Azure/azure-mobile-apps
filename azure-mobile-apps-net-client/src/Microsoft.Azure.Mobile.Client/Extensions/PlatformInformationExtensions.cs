// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System.Linq;
using System.Reflection;

namespace Microsoft.WindowsAzure.MobileServices
{
    /// <summary>
    /// A set of extensions to the PlatformInformation type
    /// </summary>
    public static class PlatformInformationExtensions
    {
        /// <summary>
        /// Returns the version set in the <see cref="AssemblyFileVersionAttribute"/> set in the assembly
        /// containing this <see cref="IPlatformInformation"/> implementation.
        /// </summary>
        /// <param name="platformInformation">The object being extended</param>
        /// <returns>The version set in the assembly.</returns>
        internal static string GetVersionFromAssemblyFileVersion(this IPlatformInformation platformInformation)
            => platformInformation.GetType()
                .GetTypeInfo().Assembly
                .GetCustomAttributes(typeof(AssemblyFileVersionAttribute))
                .FirstOrDefault() is AssemblyFileVersionAttribute attribute ? attribute.Version : string.Empty;
    }
}
