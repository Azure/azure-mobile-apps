// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------------------- 

namespace Microsoft.Azure.Mobile.Server.Config
{
    /// <summary>
    /// Provides an abstraction for getting the <see cref="MobileAppSettingsDictionary"/> for
    /// a service. The <see cref="MobileAppSettingsDictionary"/> contains settings such
    /// as the name and other parameters for the service.
    /// </summary>
    public interface IMobileAppSettingsProvider
    {
        /// <summary>
        /// Gets the <see cref="MobileAppSettingsDictionary"/> for this service.
        /// </summary>
        /// <returns>An initialized <see cref="MobileAppSettingsDictionary"/> instance.</returns>
        MobileAppSettingsDictionary GetMobileAppSettings();
    }
}
