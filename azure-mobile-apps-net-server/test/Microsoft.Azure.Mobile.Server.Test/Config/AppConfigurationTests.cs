// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------------------- 

using System;
using System.Web.Http;
using Xunit;

namespace Microsoft.Azure.Mobile.Server.Config
{
    public class AppConfigurationTests
    {
        [Fact]
        public void RegisterConfigProvider_Throws_IfProviderAlreadyExists()
        {
            var testAppConfig = new TestAppConfiguration();
            testAppConfig.RegisterConfigProvider(new TestExtensionConfigProvider1());
            testAppConfig.RegisterConfigProvider(new TestExtensionConfigProvider2());

            Assert.Throws<ArgumentException>(() => testAppConfig.RegisterConfigProvider(new TestExtensionConfigProvider1()));
        }

        [Fact]
        public void ApplyTo_Throws_IfCalledTwiceOnSameConfig()
        {
            var mobileAppConfig = new MobileAppConfiguration();
            var config = new HttpConfiguration();
            mobileAppConfig.ApplyTo(config);

            Assert.Throws<InvalidOperationException>(() => mobileAppConfig.ApplyTo(config));
        }

        private class TestAppConfiguration : AppConfiguration
        {
        }

        private class TestExtensionConfigProvider1 : IMobileAppExtensionConfigProvider
        {
            public void Initialize(HttpConfiguration config)
            {
            }
        }

        private class TestExtensionConfigProvider2 : IMobileAppExtensionConfigProvider
        {
            public void Initialize(HttpConfiguration config)
            {
            }
        }
    }
}
