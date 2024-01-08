// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Models;
using Microsoft.AspNetCore.Datasync.InMemory;
using Microsoft.Spatial;

#nullable enable

namespace Datasync.Common;

public enum KitchenSinkState
{
    None,
    Completed,
    Failed
}

public interface IKitchenSink
{
    // Boolean types
    bool BooleanValue { get; set; }

    // Number types
    int IntValue { get; set; }
    long LongValue { get; set; }
    decimal DecimalValue { get; set; }
    double DoubleValue { get; set; }
    float FloatValue { get; set; }
    double? NullableDouble { get; set; }

    // String types
    char CharValue { get; set; }
    string? StringValue { get; set; }
    byte ByteValue { get; set; }
    byte[]? ByteArrayValue { get; set; }

    // Enum types
    KitchenSinkState EnumValue { get; set; }
    KitchenSinkState? NullableEnumValue { get; set; }

    // Complex types
    Guid? GuidValue { get; set; }

    // Date/time types
    DateTime DateTimeValue { get; set; }
    DateTimeOffset DateTimeOffsetValue { get; set; }
    DateOnly DateOnlyValue { get; set; }
    TimeOnly TimeOnlyValue { get; set; }

    // Geospatial types
    GeographyPoint? PointValue { get; set; }
}

[ExcludeFromCodeCoverage]
public class ClientKitchenSink : ClientTableData, IKitchenSink
{
    // Boolean types
    public bool BooleanValue { get; set; }

    // Number types
    public int IntValue { get; set; }
    public long LongValue { get; set; }
    public decimal DecimalValue { get; set; }
    public double DoubleValue { get; set; }
    public float FloatValue { get; set; }
    public double? NullableDouble { get; set; }

    // String types
    public char CharValue { get; set; }
    public string? StringValue { get; set; }
    public byte ByteValue { get; set; }
    public byte[]? ByteArrayValue { get; set; }

    // Enum types
    public KitchenSinkState EnumValue { get; set; }
    public KitchenSinkState? NullableEnumValue { get; set; }

    // Complex types
    public Guid? GuidValue { get; set; }

    // Date/time types
    public DateTime DateTimeValue { get; set; }
    public DateTimeOffset DateTimeOffsetValue { get; set; }
    public DateOnly DateOnlyValue { get; set; }
    public TimeOnly TimeOnlyValue { get; set; }

    // Geospatial types
    public GeographyPoint? PointValue { get; set; }
}

[ExcludeFromCodeCoverage]
public class InMemoryKitchenSink : InMemoryTableData, IKitchenSink
{
    public bool BooleanValue { get; set; }
    public byte ByteValue { get; set; }
    public byte[]? ByteArrayValue { get; set; }
    public char CharValue { get; set; }
    public DateOnly DateOnlyValue { get; set; }
    public DateTime DateTimeValue { get; set; }
    public DateTimeOffset DateTimeOffsetValue { get; set; }
    public decimal DecimalValue { get; set; }
    public double DoubleValue { get; set; }
    public KitchenSinkState EnumValue { get; set; }
    public float FloatValue { get; set; }
    public Guid? GuidValue { get; set; }
    public int IntValue { get; set; }
    public long LongValue { get; set; }
    public double? NullableDouble { get; set; }
    public KitchenSinkState? NullableEnumValue { get; set; }
    public GeographyPoint? PointValue { get; set; }
    public string? StringValue { get; set; }
    public TimeOnly TimeOnlyValue { get; set; }
}
