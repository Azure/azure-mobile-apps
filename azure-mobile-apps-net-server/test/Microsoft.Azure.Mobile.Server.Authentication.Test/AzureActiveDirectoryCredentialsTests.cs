// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------------------- 

using Microsoft.Azure.Mobile.Server.Authentication;
using TestUtilities;
using Xunit;

namespace Microsoft.Azure.Mobile.Server.Security
{
    public class AzureActiveDirectoryCredentialsTests
    {
        private AzureActiveDirectoryCredentials creds = new AzureActiveDirectoryCredentials();

        [Fact]
        public void Provider_Roundtrips()
        {
            PropertyAssert.Roundtrips(this.creds, c => c.Provider, PropertySetter.NullRoundtrips, defaultValue: "Aad", roundtripValue: "testProvider");
        }

        [Fact]
        public void AccessToken_Roundtrips()
        {
            PropertyAssert.Roundtrips(this.creds, c => c.AccessToken, PropertySetter.NullRoundtrips, roundtripValue: "accessToken");
        }

        [Fact]
        public void TenantId_Roundtrips()
        {
            PropertyAssert.Roundtrips(this.creds, c => c.TenantId, PropertySetter.NullRoundtrips, roundtripValue: "tenantId");
        }

        [Fact]
        public void ObjectId_Roundtrips()
        {
            PropertyAssert.Roundtrips(this.creds, c => c.ObjectId, PropertySetter.NullRoundtrips, roundtripValue: "objectId");
        }
    }
}
