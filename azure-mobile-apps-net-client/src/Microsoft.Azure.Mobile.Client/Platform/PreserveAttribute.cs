// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;

namespace Microsoft.WindowsAzure.MobileServices
{
    /// <summary>
    /// When applied to a target (e.g. assembly, class, member, etc.), instructs the Xamarin linker to preserve that target
    /// in the linking process result.
    /// </summary>
    [AttributeUsage(System.AttributeTargets.All)]
    public sealed class PreserveAttribute : System.Attribute
    {
        /// <summary>
        /// Preserve all members
        /// </summary>
        public bool AllMembers;

        /// <summary>
        /// Apply a condition to determine which members to preserve.
        /// </summary>
        public bool Conditional;
    }
}
