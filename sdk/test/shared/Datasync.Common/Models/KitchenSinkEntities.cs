// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Models;
using Microsoft.AspNetCore.Datasync.InMemory;

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
    bool BooleanValue { get; set; }

    int IntValue { get; set; }
    long LongValue { get; set; }

    decimal DecimalValue { get; set; }
    double DoubleValue { get; set; }
    float FloatValue { get; set; }
    double? NullableDouble { get; set; }

    char CharValue { get; set; }
    string? StringValue { get; set; }
    byte ByteValue { get; set; }
    byte[]? ByteArrayValue { get; set; }

    KitchenSinkState EnumValue { get; set; }
    KitchenSinkState? NullableEnumValue { get; set; }

    DateTime? DateTimeValue { get; set; }
    DateTimeOffset? DateTimeOffsetValue { get; set; }
    DateOnly? DateOnlyValue { get; set; }
    Guid? GuidValue { get; set; }
    TimeOnly? TimeOnlyValue { get; set; }
}

[ExcludeFromCodeCoverage]
public class ClientKitchenSink : ClientTableData, IKitchenSink
{
    public bool BooleanValue { get; set; }

    public int IntValue { get; set; }
    public long LongValue { get; set; }

    public decimal DecimalValue { get; set; }
    public double DoubleValue { get; set; }
    public float FloatValue { get; set; }
    public double? NullableDouble { get; set; }

    public char CharValue { get; set; }
    public string? StringValue { get; set; }
    public byte ByteValue { get; set; }
    public byte[]? ByteArrayValue { get; set; }

    public KitchenSinkState EnumValue { get; set; }
    public KitchenSinkState? NullableEnumValue { get; set; }

    public DateTime? DateTimeValue { get; set; }
    public DateTimeOffset? DateTimeOffsetValue { get; set; }
    public DateOnly? DateOnlyValue { get; set; }
    public Guid? GuidValue { get; set; }
    public TimeOnly? TimeOnlyValue { get; set; }
}

[ExcludeFromCodeCoverage]
public class InMemoryKitchenSink : InMemoryTableData, IKitchenSink
{
    public bool BooleanValue { get; set; }

    public int IntValue { get; set; }
    public long LongValue { get; set; }

    public decimal DecimalValue { get; set; }
    public double DoubleValue { get; set; }
    public float FloatValue { get; set; }
    public double? NullableDouble { get; set; }

    public char CharValue { get; set; }
    public string? StringValue { get; set; }
    public byte ByteValue { get; set; }
    public byte[]? ByteArrayValue { get; set; }

    public KitchenSinkState EnumValue { get; set; }
    public KitchenSinkState? NullableEnumValue { get; set; }

    public DateTime? DateTimeValue { get; set; }
    public DateTimeOffset? DateTimeOffsetValue { get; set; }
    public DateOnly? DateOnlyValue { get; set; }
    public Guid? GuidValue { get; set; }
    public TimeOnly? TimeOnlyValue { get; set; }
}
