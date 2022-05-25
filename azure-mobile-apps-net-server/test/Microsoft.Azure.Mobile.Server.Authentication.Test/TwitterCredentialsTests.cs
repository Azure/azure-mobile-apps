// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------------------- 

using Microsoft.Azure.Mobile.Server.Authentication;
using TestUtilities;
using Xunit;

namespace Microsoft.Azure.Mobile.Server.Security
{
    public class TwitterCredentialsTests
    {
        private TwitterCredentials creds = new TwitterCredentials();

        [Fact]
        public void Provider_Roundtrips()
        {
            PropertyAssert.Roundtrips(this.creds, c => c.Provider, PropertySetter.NullRoundtrips, defaultValue: "Twitter", roundtripValue: "testProvider");
        }

        [Fact]
        public void AccessToken_Roundtrips()
        {
            PropertyAssert.Roundtrips(this.creds, c => c.AccessToken, PropertySetter.NullRoundtrips, roundtripValue: "accessToken");
        }

        [Fact]
        public void AccessTokenSecret_Roundtrips()
        {
            PropertyAssert.Roundtrips(this.creds, c => c.AccessTokenSecret, PropertySetter.NullRoundtrips, roundtripValue: "accessTokenSecret");
        }
    }
}
