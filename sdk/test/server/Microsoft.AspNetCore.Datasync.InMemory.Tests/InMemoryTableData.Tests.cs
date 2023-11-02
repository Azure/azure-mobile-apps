// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

namespace Microsoft.AspNetCore.Datasync.InMemory.Tests;

[ExcludeFromCodeCoverage]
public class InMemoryTableData_Tests
{
    [Theory, ClassData(typeof(ITableData_TestData))]
    public void Equals_Works(ITableData a, ITableData b, bool expected)
    {
        InMemoryTableData entity_a = a.ToTableEntity<InMemoryTableData>();
        InMemoryTableData entity_b = b.ToTableEntity<InMemoryTableData>();

        entity_a.Equals(entity_b).Should().Be(expected);
        entity_b.Equals(entity_a).Should().Be(expected);

        entity_a.Equals(null).Should().BeFalse();
        entity_b.Equals(null).Should().BeFalse();
    }
}