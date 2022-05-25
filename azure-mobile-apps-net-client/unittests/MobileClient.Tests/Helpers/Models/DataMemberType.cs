// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System.Runtime.Serialization;

namespace MobileClient.Tests.Helpers
{
    public class DataMemberType
    {
        public long Id { get; set; }

        [DataMember]
        public string PublicProperty { get; set; }
    }
}