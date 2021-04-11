// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.AzureMobile.Common.Test.Models
{
    /// <summary>
    /// The client representation of <see cref="ITableData"/>.
    /// </summary>
    [ExcludeFromCodeCoverage(Justification = "Test suite")]
    public abstract class ClientTableData
    {
        public string Id { get; set; }
        public string Version { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
        public bool Deleted { get; set; }
    }
}
