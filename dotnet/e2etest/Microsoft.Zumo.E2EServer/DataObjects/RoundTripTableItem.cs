// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Zumo.Server.Entity;
using System;

namespace Microsoft.Zumo.E2EServer.DataObjects
{
    public class RoundTripTableItem : EntityTableData
    {
        public string Name { get; set; }
        public DateTimeOffset? Date1 { get; set; }
        public bool? Bool { get; set; }
        public int? Integer { get; set; }
        public double? Number { get; set; }
    }
}
