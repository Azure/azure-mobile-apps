// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

namespace DeviceTests.Shared
{
    /// <summary>
    /// An interface for platform-specific assemblies to provide utility functions
    /// regarding Push capabilities.
    /// </summary>
    public interface IPushTestUtility
    {
        string GetPushHandle();
    }
}