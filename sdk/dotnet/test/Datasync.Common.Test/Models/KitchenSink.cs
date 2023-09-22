// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Datasync.EFCore;

#nullable enable

namespace Datasync.Common.Test.Models;

public enum KitchenSinkState
{
    None,
    Completed,
    Failed
}

/// <summary>
/// A model used for validation tests.  It contains one of
/// every single supported type in a nullable form.
/// </summary>
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
    public double? NullableDouble { get; set; }

    // String
    public char CharValue { get; set; }
    public string? StringValue { get; set; }

    // Enums
    public KitchenSinkState EnumValue { get; set; }

    // Nullable Enums
    public KitchenSinkState? EnumOfNullableValue { get; set; } // Nullable<KitchenSinkState>

    // Complex types
    public DateTime? DateTimeValue { get; set; }
    public DateTimeOffset? DateTimeOffsetValue { get; set; }
    public Guid? GuidValue { get; set; }

}
