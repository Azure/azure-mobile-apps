// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Datasync.Client.Test.Helpers
{
    [ExcludeFromCodeCoverage(Justification = "Test suite")]
    public class MockObject
    {
        #region ITableData
        public string Id { get; set; }
        public DateTimeOffset? UpdatedAt { get; set; }
        public string Version { get; set; }
        public bool Deleted { get; set; }
        #endregion

        #region Properties
        public bool BoolValue { get; set; }
        public char CharValue { get; set; }
        public DateTime? DateTimeValue { get; set; }
        public DateTimeOffset? DateTimeOffsetValue { get; set; }
        public decimal DecimalValue { get; set; }
        public double DoubleValue { get; set; }
        public float FloatValue { get; set; }
        public Guid GuidValue { get; set; }
        public string StringValue { get; set; }
        #endregion
    }
}
