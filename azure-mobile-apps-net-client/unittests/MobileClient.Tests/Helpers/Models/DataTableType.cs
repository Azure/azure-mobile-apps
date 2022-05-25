// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using Microsoft.WindowsAzure.MobileServices;
using Newtonsoft.Json;

namespace MobileClient.Tests.Helpers
{
    [DataTable("NamedDataTableType")]
    public class DataTableType
    {
        public int Id { get; set; }

        [JsonProperty(PropertyName = "AnotherPublicProperty")]
        public int OtherThanPublicProperty { get; set; }

        public int PublicProperty { get; set; }
    }
}