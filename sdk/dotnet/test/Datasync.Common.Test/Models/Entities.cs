// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Datasync.Common.Test.Models
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
    public class NoIdEntity
    {
        public string StringValue { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class NonStringIdEntity
    {
        public bool Id { get; set; }
        public bool Version { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class DTOIdEntity
    {
        public string Id { get; set; }
        public DateTimeOffset? UpdatedAt { get; set; }
        public long Number { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class KitchenSinkEntity
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

    [ExcludeFromCodeCoverage]
    public class SelectResult
    {
        public string Id { get; set; }
        public string Title { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class RenamedEntity
    {
        [JsonPropertyName("rating")]
        public string MpaaRating { get; set; }
    }
}
