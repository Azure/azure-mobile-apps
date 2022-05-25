// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Security.Claims;
using Microsoft.Azure.Mobile.Server.Authentication;
using Moq;
using TestUtilities;
using Xunit;

namespace Microsoft.Azure.Mobile.Server.Security
{
    public class ProviderCredentialsTests
    {
        private Mock<ProviderCredentials> credsMock;
        private ProviderCredentials creds;

        public ProviderCredentialsTests()
        {
            this.credsMock = new Mock<ProviderCredentials>("TestProvider") { CallBase = true };
            this.creds = this.credsMock.Object;
        }

        [Fact]
        public void Provider_Roundtrips()
        {
            PropertyAssert.Roundtrips(this.creds, c => c.Provider, PropertySetter.NullRoundtrips, defaultValue: "TestProvider", roundtripValue: "你好世界");
        }

        [Fact]
        public void UserId_Roundtrips()
        {
            PropertyAssert.Roundtrips(this.creds, c => c.UserId, PropertySetter.NullRoundtrips, roundtripValue: "userId");
        }

        [Fact]
        public void UserClaims_Roundtrip()
        {
            Collection<Claim> claims = new Collection<Claim>();
            claims.Add(new Claim("foo", "value1"));
            claims.Add(new Claim("bar", "value2"));
            PropertyAssert.Roundtrips(this.creds, c => c.UserClaims, PropertySetter.NullRoundtrips, roundtripValue: claims);
        }
    }
}