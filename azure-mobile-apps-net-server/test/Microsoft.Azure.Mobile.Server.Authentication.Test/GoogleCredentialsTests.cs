// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------------------- 

using System;
using Microsoft.Azure.Mobile.Server.Authentication;
using TestUtilities;
using Xunit;

namespace Microsoft.Azure.Mobile.Server.Security
{
    public class GoogleCredentialsTests
    {
        private GoogleCredentials creds = new GoogleCredentials();

        [Fact]
        public void Provider_Roundtrips()
        {
            PropertyAssert.Roundtrips(this.creds, c => c.Provider, PropertySetter.NullRoundtrips, defaultValue: "Google", roundtripValue: "testProvider");
        }

        [Fact]
        public void AccessToken_Roundtrips()
        {
            PropertyAssert.Roundtrips(this.creds, c => c.AccessToken, PropertySetter.NullRoundtrips, roundtripValue: "accessToken");
        }

        [Fact]
        public void RefreshToken_Roundtrips()
        {
            PropertyAssert.Roundtrips(this.creds, c => c.RefreshToken, PropertySetter.NullRoundtrips, roundtripValue: "refreshToken");
        }

        [Fact]
        public void AccessTokenExpiration_Roundtrips()
        {
            PropertyAssert.Roundtrips<GoogleCredentials, DateTimeOffset>(this.creds, c => c.AccessTokenExpiration, PropertySetter.NullRoundtrips, roundtripValue: DateTimeOffset.UtcNow);
        }
    }
}
