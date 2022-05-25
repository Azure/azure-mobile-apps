// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------------------- 

using TestUtilities;
using Xunit;

namespace Microsoft.Azure.Mobile.Server.Notifications
{
    public class MpnsTileMessageTests
    {
        private MpnsTileMessageMock message;

        public MpnsTileMessageTests()
        {
            this.message = new MpnsTileMessageMock();
        }

        [Fact]
        public void Id_Roundtrips()
        {
            PropertyAssert.Roundtrips(this.message, m => m.Id, PropertySetter.NullRoundtrips, roundtripValue: "roundtrips");
        }

        public class MpnsTileMessageMock : MpnsTileMessage
        {
            public MpnsTileMessageMock()
                : base("version", "template")
            {
            }

            public new string Template
            {
                get
                {
                    return base.Template;
                }

                set
                {
                    base.Template = value;
                }
            }
        }
    }
}
