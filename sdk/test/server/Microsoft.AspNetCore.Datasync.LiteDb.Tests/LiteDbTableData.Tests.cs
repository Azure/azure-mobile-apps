// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

namespace Microsoft.AspNetCore.Datasync.LiteDb.Tests;

[ExcludeFromCodeCoverage]
public class LiteDbTableData_Tests
{
    [Theory, ClassData(typeof(ITableData_TestData))]
    public void Equals_Works(ITableData a, ITableData b, bool expected)
    {
        LiteDbTableData entity_a = a.ToTableEntity<LiteDbTableData>();
        LiteDbTableData entity_b = b.ToTableEntity<LiteDbTableData>();

        entity_a.Equals(entity_b).Should().Be(expected);
        entity_b.Equals(entity_a).Should().Be(expected);

        entity_a.Equals(null).Should().BeFalse();
        entity_b.Equals(null).Should().BeFalse();
    }
}