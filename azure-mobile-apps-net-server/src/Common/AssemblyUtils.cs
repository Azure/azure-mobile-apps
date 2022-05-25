// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------------------- 

using System.Reflection;
using Microsoft.Azure.Mobile.Server.Properties;

namespace Microsoft.Azure.Mobile
{
    internal static class AssemblyUtils
    {
        private static string assemblyFileVersion;

        /// <summary>
        /// Gets a string containing the <see cref="AssemblyFileVersionAttribute"/> version information
        /// for the current assembly. 
        /// </summary>
        public static string AssemblyFileVersion
        {
            get
            {
                if (assemblyFileVersion == null)
                {
                    assemblyFileVersion = GetExecutingAssemblyFileVersionOrDefault();
                }

                return assemblyFileVersion;
            }
        }

        public static string GetExecutingAssemblyFileVersionOrDefault()
        {
            try
            {
                Assembly current = Assembly.GetExecutingAssembly();
                return GetExecutingAssemblyFileVersionOrDefault(current);
            }
            catch
            {
                return CommonResources.Assembly_UnknownFileVersion;
            }
        }

        internal static string GetExecutingAssemblyFileVersionOrDefault(Assembly assembly)
        {
            AssemblyFileVersionAttribute fileVersionAttr = assembly.GetCustomAttribute<AssemblyFileVersionAttribute>();
            return fileVersionAttr != null && fileVersionAttr.Version != null ? fileVersionAttr.Version : CommonResources.Assembly_UnknownFileVersion;
        }
    }
}
