// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using Microsoft.WindowsAzure.MobileServices;
using System;

namespace DeviceTests.Shared.Helpers.Models
{
    [DataTable("RoundTripTable")]
    public class ToDoWithStringId
    {
        public string Id { get; set; }

        public string Name { get; set; }
    }

    [DataTable("IntIdRoundTripTable")]
    public class ToDoWithStringIdAgainstIntIdTable
    {
        public string Id { get; set; }

        public string Name { get; set; }
    }

    [DataTable("IntIdRoundTripTable")]
    public class ToDoWithIntId
    {
        public long Id { get; set; }

        public string Name { get; set; }
    }

    [DataTable("RoundTripTable")]
    public class RoundTripTableItemWithSystemPropertiesType
    {
        public string Id { get; set; }

        public string Name { get; set; }

        [CreatedAt]
        public DateTimeOffset CreatedAt { get; set; }

        [UpdatedAt]
        public DateTimeOffset UpdatedAt { get; set; }

        [Version]
        public String Version { get; set; }
    }
}
