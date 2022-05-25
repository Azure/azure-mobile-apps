// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using DeviceTests.Shared.TestPlatform;
using Microsoft.WindowsAzure.MobileServices;
using Xunit;

namespace DeviceTests.Shared.Tests
{
    // DO NOT REMOVE THIS CLASS
    // The Android/iOS platform code relies on its existance to find the right assembly to load
    public class Shared_Tests: E2ETestBase
    {
        [Fact]
        public void Shared_client_test()
        {
            var client = GetClient();
            Assert.IsType<MobileServiceClient>(client);
        }
    }
}
