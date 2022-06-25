using Microsoft.AspNetCore.Datasync.EFCore;
using System;
using System.Diagnostics.CodeAnalysis;

#nullable enable

namespace Datasync.Common.Test.Models
{
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

        // String
        public char CharValue { get; set; }
        public string? StringValue { get; set; }

        // Complex types
        public DateTime? DateTimeValue { get; set; }
        public DateTimeOffset? DateTimeOffsetValue { get; set; }
        public Guid? GuidValue { get; set; }
    }
}
