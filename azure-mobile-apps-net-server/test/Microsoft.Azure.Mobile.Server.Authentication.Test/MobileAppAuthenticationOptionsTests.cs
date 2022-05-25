// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------------------- 

using System;
using Microsoft.Azure.Mobile.Server.Authentication;
using Microsoft.Owin.Security;
using TestUtilities;
using Xunit;

namespace Microsoft.Azure.Mobile.Server.Security
{
    public class MobileAppAuthenticationOptionsTests
    {
        private const string SigningKey = "$$SigningKey";

        private AppServiceAuthenticationOptions options;

        public MobileAppAuthenticationOptionsTests()
        {
            this.options = new AppServiceAuthenticationOptions();
            this.options.SigningKey = SigningKey;
        }

        [Fact]
        public void AuthenticationMode_IsActive()
        {
            Assert.Equal(AuthenticationMode.Active, this.options.AuthenticationMode);
        }

        [Fact]
        public void SigningKey_Roundtrips()
        {
            PropertyAssert.Roundtrips(this.options, o => o.SigningKey, PropertySetter.NullRoundtrips, defaultValue: SigningKey, roundtripValue: "roundtrips");
        }
    }
}
