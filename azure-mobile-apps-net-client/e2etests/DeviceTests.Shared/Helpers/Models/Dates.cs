// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using Microsoft.WindowsAzure.MobileServices;
using System;

namespace DeviceTests.Shared.Helpers.Models
{
    [DataTable("dates")]
    public class Dates
    {
        public Dates()
        {
            this.Date = DateTime.UtcNow;
            this.DateOffset = DateTimeOffset.UtcNow;
        }

        public string Id { get; set; }
        public DateTime Date { get; set; }
        public DateTimeOffset DateOffset { get; set; }
    }
}
