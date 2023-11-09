// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

namespace Microsoft.AspNetCore.Datasync.EFCore.Tests;

[ExcludeFromCodeCoverage]
public class EntityTableData_Tests
{
    [Theory, ClassData(typeof(ITableData_TestData))]
    public void CosmosEntityTableData_Equals(ITableData a, ITableData b, bool expected)
    {
        CosmosEntityTableData entity_a = a.ToTableEntity<CosmosEntityTableData>();
        CosmosEntityTableData entity_b = b.ToTableEntity<CosmosEntityTableData>();

        entity_a.Equals(entity_b).Should().Be(expected);
        entity_b.Equals(entity_a).Should().Be(expected);

        entity_a.Equals(null).Should().BeFalse();
        entity_b.Equals(null).Should().BeFalse();
    }

    [Fact]
    public void CosmosEntityTableData_MetadataRoundtrips()
    {
        DateTimeOffset testTime = DateTimeOffset.Now;

        CosmosEntityTableData sut1 = new() { Id = "t1", Deleted = false, UpdatedAt = testTime, Version = new byte[] { 0x61, 0x62, 0x63, 0x64, 0x65 } };
        sut1.EntityTag.Should().BeEquivalentTo("abcde");
        sut1.UpdatedAt.Should().Be(testTime);

        CosmosEntityTableData sut2 = new() { Id = "t1", Deleted = false, UpdatedAt = testTime, EntityTag = "abcde" };
        sut2.Version.Should().BeEquivalentTo(new byte[] { 0x61, 0x62, 0x63, 0x64, 0x65 });
        sut2.UpdatedAt.Should().Be(testTime);
    }

    [Theory, ClassData(typeof(ITableData_TestData))]
    public void EntityTableData_Equals(ITableData a, ITableData b, bool expected)
    {
        EntityTableData entity_a = a.ToTableEntity<EntityTableData>();
        EntityTableData entity_b = b.ToTableEntity<EntityTableData>();

        entity_a.Equals(entity_b).Should().Be(expected);
        entity_b.Equals(entity_a).Should().Be(expected);

        entity_a.Equals(null).Should().BeFalse();
        entity_b.Equals(null).Should().BeFalse();
    }

    [Fact]
    public void EntityTableData_MetadataRoundtrips()
    {
        DateTimeOffset testTime = DateTimeOffset.Now;

        EntityTableData sut1 = new() { Id = "t1", Deleted = false, UpdatedAt = testTime, Version = new byte[] { 0x61, 0x62, 0x63, 0x64, 0x65 } };
        sut1.Version.Should().BeEquivalentTo(new byte[] { 0x61, 0x62, 0x63, 0x64, 0x65 });
        sut1.UpdatedAt.Should().Be(testTime);
    }
}