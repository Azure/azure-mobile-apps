﻿// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using Newtonsoft.Json;

namespace MobileClient.Tests.Helpers
{
    [JsonObject(Title = "NamedJsonContainerType")]
    public class JsonContainerType
    {
        public int Id { get; set; }

        [JsonProperty(PropertyName = "AnotherPublicProperty")]
        public int OtherThanPublicProperty { get; set; }

        public int PublicProperty { get; set; }
    }

    [JsonObject]
    public class UnnamedJsonContainerType
    {
        public int Id { get; set; }

        [JsonProperty(PropertyName = "AnotherPublicProperty")]
        public int OtherThanPublicProperty { get; set; }

        public int PublicProperty { get; set; }
    }
}