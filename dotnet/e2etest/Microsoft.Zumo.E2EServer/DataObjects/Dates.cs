// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Zumo.Server.Entity;
using System;

namespace Microsoft.Zumo.E2EServer.DataObjects
{
    public class Dates : EntityTableData
    {
        public DateTime Date { get; set; }
        public DateTimeOffset DateOffset { get; set; }
    }
}
