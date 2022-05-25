// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using DeviceTests.Shared;

namespace DeviceTests.Android.Services
{
    class PushTestUtility : IPushTestUtility
    {
        public string GetPushHandle() => GcmService.RegistrationId;
    }
}