// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Datasync.EFCore;

#nullable enable

namespace Microsoft.AspNetCore.Datasync.Swashbuckle.Test.Service;

[ExcludeFromCodeCoverage]
public class TodoItem : EntityTableData
{
    public string? Title { get; set; }
}

[ExcludeFromCodeCoverage]
public class KitchenSink : EntityTableData
{
    // Boolean
    public bool BooleanValue { get; set; }

    // Integers
    public int IntValue { get; set; }
    public long LongValue { get; set; }

    // Floats
    public decimal DecimalValue { get; set; }
    public double DoubleValue { get; set; }
    public float FloatValue { get; set; }

    // String
    public char CharValue { get; set; }
    public string? StringValue { get; set; }

    // Complex types
    public DateTime? DateTimeValue { get; set; }
    public DateTimeOffset? DateTimeOffsetValue { get; set; }
    public Guid? GuidValue { get; set; }
}
