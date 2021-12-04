// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Datasync.InMemory;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.AspNetCore.Datasync.Test
{
    /// <summary>
    /// A test entity.
    /// </summary>
    [ExcludeFromCodeCoverage(Justification = "Test suite")]
    internal class InMemoryEntity : InMemoryTableData
    {
    }
}
