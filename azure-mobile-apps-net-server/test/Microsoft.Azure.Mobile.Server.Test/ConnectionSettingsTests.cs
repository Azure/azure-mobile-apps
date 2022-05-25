// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------------------- 

using TestUtilities;
using Xunit;

namespace Microsoft.Azure.Mobile.Server
{
    public class ConnectionSettingsTests
    {
        private const string ConnectionName = "ConName";
        private const string ConnectionString = "ConString";

        private ConnectionSettings settings = new ConnectionSettings(ConnectionName, ConnectionString);

        [Fact]
        public void Name_Roundtrips()
        {
            PropertyAssert.Roundtrips(this.settings, s => s.Name, PropertySetter.NullThrows, defaultValue: ConnectionName, roundtripValue: "Value");
        }

        [Fact]
        public void ConnectionString_Roundtrips()
        {
            PropertyAssert.Roundtrips(this.settings, s => s.ConnectionString, PropertySetter.NullThrows, defaultValue: ConnectionString, roundtripValue: "Value");
        }

        [Fact]
        public void Provider_Roundtrips()
        {
            PropertyAssert.Roundtrips(this.settings, s => s.Provider, PropertySetter.NullRoundtrips, roundtripValue: "Value");
        }
    }
}
