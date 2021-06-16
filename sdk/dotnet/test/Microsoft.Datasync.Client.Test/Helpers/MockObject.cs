// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Datasync.Client.Test.Helpers
{
    [ExcludeFromCodeCoverage]
    public class MockObject
    {
        public bool BooleanValue { get; set; }
        public char CharValue { get; set; }
        public DateTime DateTimeValue { get; set; }
        public DateTimeOffset DTOValue { get; set; }
        public decimal DecimalValue { get; set; }
        public double DoubleValue { get; set; }
        public float FloatValue { get; set; }
        public Guid GuidValue { get; set; }
        public int IntValue { get; set; }
        public long LongValue { get; set; }
        public string StringValue { get; set; }
    }
}
