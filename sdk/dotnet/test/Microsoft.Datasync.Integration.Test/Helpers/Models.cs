// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client;
using System;
using System.Diagnostics.CodeAnalysis;

#nullable disable

namespace Microsoft.Datasync.Integration.Test.Helpers
{
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

        // String
        public char CharValue { get; set; }
        public string StringValue { get; set; }

        // Complex types
        public DateTime? DateTimeValue { get; set; }
        public DateTimeOffset? DateTimeOffsetValue { get; set; }
        public Guid? GuidValue { get; set; }
    }
}
