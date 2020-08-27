// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Zumo.Server.Entity;
using System;

namespace Microsoft.Zumo.E2EServer.DataObjects
{
    public class OfflineReady : EntityTableData
    {
        public string Name { get; set; }
        public int Age { get; set; }
        public double Float { get; set; }
        public DateTimeOffset Date { get; set; }
        public bool Bool { get; set; }
    }
}
