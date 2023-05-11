// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

#nullable disable

namespace Microsoft.Datasync.Integration.Test.Helpers;

[ExcludeFromCodeCoverage]
public class IdEntity : IEquatable<IdEntity>
{
    public string Id { get; set; }
    public string Version { get; set; }
    public string StringValue { get; set; }

    public string StringField;

    public bool Equals(IdEntity other)
        => Id == other.Id && StringValue == other.StringValue;

    public override bool Equals(object obj)
        => obj is IdEntity ide && Equals(ide);

    public override int GetHashCode()
        => Id.GetHashCode() + StringValue.GetHashCode();
}

[ExcludeFromCodeCoverage]
public class IdOnly
{
    public string Id { get; set; }
}

public enum KitchenSinkDtoState
{
    None,
    Completed,
    Failed
}

[ExcludeFromCodeCoverage]
public class KitchenSinkDto : DatasyncClientData
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
    public string StringValue { get; set; }

    // Complex types
    public DateTime? DateTimeValue { get; set; }
    public DateTimeOffset? DateTimeOffsetValue { get; set; }
    public Guid? GuidValue { get; set; }

    // Enums
    public KitchenSinkDtoState EnumValue { get; set; }

    // Nullable Enums
    public KitchenSinkDtoState? EnumOfNullableValue { get; set; } // Nullable<KitchenSinkDtoState>
}

[ExcludeFromCodeCoverage]
public class DateTimeDto : DatasyncClientData
{
    public string DateOnly { get; set; }
    public string TimeOnly { get; set; }
}
