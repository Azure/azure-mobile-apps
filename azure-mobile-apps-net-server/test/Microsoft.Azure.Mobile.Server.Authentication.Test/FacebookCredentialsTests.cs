// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------------------- 

using Microsoft.Azure.Mobile.Server.Authentication;
using TestUtilities;
using Xunit;

namespace Microsoft.Azure.Mobile.Server.Security
{
    public class FacebookCredentialsTests
    {
        private FacebookCredentials creds = new FacebookCredentials();

        [Fact]
        public void Provider_Roundtrips()
        {
            PropertyAssert.Roundtrips(this.creds, c => c.Provider, PropertySetter.NullRoundtrips, defaultValue: "Facebook", roundtripValue: "testProvider");
        }

        [Fact]
        public void AccessToken_Roundtrips()
        {
            PropertyAssert.Roundtrips(this.creds, c => c.AccessToken, PropertySetter.NullRoundtrips, roundtripValue: "accessToken");
        }
    }
}
