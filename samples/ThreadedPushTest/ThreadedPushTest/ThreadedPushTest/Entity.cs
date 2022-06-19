// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client;
using System;

namespace ThreadedPushTest
{
    internal class Entity : DatasyncClientData
    {
        public bool BoolProp { get; set; }
        public int IntProp { get; set; }
        public double DoubleProp { get; set; }
        public string StringProp { get; set; }
        public DateTimeOffset TimestampProp { get; set; }
        public Guid GuidProp { get; set; }
    }
}
