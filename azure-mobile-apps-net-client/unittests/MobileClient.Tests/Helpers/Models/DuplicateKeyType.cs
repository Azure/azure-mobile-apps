// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using Newtonsoft.Json;

namespace MobileClient.Tests.Helpers
{
    public class DuplicateKeyType
    {
        [JsonProperty(PropertyName = "PublicProperty")]
        public int OtherThanPublicProperty { get; set; }

        public int PublicProperty { get; set; }
    }
}