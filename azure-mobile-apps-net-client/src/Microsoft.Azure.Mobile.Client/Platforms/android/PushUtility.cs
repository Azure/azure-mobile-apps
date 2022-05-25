// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

namespace Microsoft.WindowsAzure.MobileServices
{
    internal class PushUtility : IPushUtility
    {
        /// <summary>
        /// A singleton instance of the <see cref="PushUtility"/>.
        /// </summary>
        public static IPushUtility Instance { get; } = new PushUtility();

        public string GetPlatform()
        {
            return "gcm";
        }
    }
}
