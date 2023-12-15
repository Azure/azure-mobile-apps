using Microsoft.AspNetCore.Datasync.InMemory;

#nullable enable

namespace Datasync.Common;

public enum KitchenSinkState
{
    None,
    Completed,
    Failed
}

[ExcludeFromCodeCoverage]
public class ClientKitchenSink
{
    #region ClientTableData
    public string? Id { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public string? Version { get; set; }
    public bool Deleted { get; set; }
    #endregion

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
public class InMemoryKitchenSink : InMemoryTableData
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
