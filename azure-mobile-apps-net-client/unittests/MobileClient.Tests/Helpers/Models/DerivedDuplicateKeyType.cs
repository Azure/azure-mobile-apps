// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using Newtonsoft.Json;

namespace MobileClient.Tests.Helpers
{
    public class DerivedDuplicateKeyType : PocoType
    {
        [JsonProperty(PropertyName = "PublicProperty")]
        public string OtherThanPublicProperty { get; set; }
    }
}