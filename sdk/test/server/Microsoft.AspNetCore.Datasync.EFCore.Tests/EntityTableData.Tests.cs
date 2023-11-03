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

    [Theory, ClassData(typeof(ITableData_TestData))]
    public void PgEntityTableData_Equals(ITableData a, ITableData b, bool expected)
    {
        PgEntityTableData entity_a = a.ToTableEntity<PgEntityTableData>();
        PgEntityTableData entity_b = b.ToTableEntity<PgEntityTableData>();

        entity_a.Equals(entity_b).Should().Be(expected);
        entity_b.Equals(entity_a).Should().Be(expected);

        entity_a.Equals(null).Should().BeFalse();
        entity_b.Equals(null).Should().BeFalse();
    }

    [Fact]
    public void PgEntityTableData_Version_Roundtrips()
    {
        DateTimeOffset testTime = DateTimeOffset.Now;

        PgEntityTableData sut1 = new() { Id = "t1", Deleted = false, UpdatedAt = testTime, Version = new byte[] { 0xFE, 0x80, 0x05, 0x42 } };
        sut1.RowVersion.Should().Be(0x420580FE);
        sut1.UpdatedAt.Should().Be(testTime);

        PgEntityTableData sut2 = new() { Id = "t1", Deleted = false, UpdatedAt = testTime, RowVersion = 0xFE800542 };
        sut2.Version.Should().BeEquivalentTo(new byte[] { 0x42, 0x05, 0x80, 0xFE });
        sut2.UpdatedAt.Should().Be(testTime);
    }

    [Theory, ClassData(typeof(ITableData_TestData))]
    public void SqliteEntityTableData_Equals(ITableData a, ITableData b, bool expected)
    {
        SqliteEntityTableData entity_a = a.ToTableEntity<SqliteEntityTableData>();
        SqliteEntityTableData entity_b = b.ToTableEntity<SqliteEntityTableData>();

        entity_a.Equals(entity_b).Should().Be(expected);
        entity_b.Equals(entity_a).Should().Be(expected);

        entity_a.Equals(null).Should().BeFalse();
        entity_b.Equals(null).Should().BeFalse();
    }

    [Fact]
    public void SqliteEntityTableData_MetadataRoundtrips()
    {
        DateTimeOffset testTime = DateTimeOffset.Now;

        SqliteEntityTableData sut1 = new() { Id = "t1", Deleted = false, UpdatedAt = testTime, Version = new byte[] { 0x61, 0x62, 0x63, 0x64, 0x65 } };
        sut1.Version.Should().BeEquivalentTo(new byte[] { 0x61, 0x62, 0x63, 0x64, 0x65 });
        sut1.UpdatedAt.Should().Be(testTime);
    }
}