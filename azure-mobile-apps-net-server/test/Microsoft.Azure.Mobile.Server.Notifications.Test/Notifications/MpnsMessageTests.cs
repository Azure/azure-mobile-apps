// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------------------- 

using TestUtilities;
using Xunit;

namespace Microsoft.Azure.Mobile.Server.Notifications
{
    public class MpnsMessageTests
    {
        private MpnsMessageMock message;

        public MpnsMessageTests()
        {
            this.message = new MpnsMessageMock();
        }

        [Fact]
        public void Template_Roundtrips()
        {
            PropertyAssert.Roundtrips(this.message, m => m.Template, PropertySetter.NullRoundtrips, defaultValue: "template", roundtripValue: "roundtrips");
        }

        [Fact]
        public void Version_Roundtrips()
        {
            PropertyAssert.Roundtrips(this.message, m => m.Version, PropertySetter.NullRoundtrips, defaultValue: "version", roundtripValue: "roundtrips");
        }

        public class MpnsMessageMock : MpnsMessage
        {
            public MpnsMessageMock()
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
