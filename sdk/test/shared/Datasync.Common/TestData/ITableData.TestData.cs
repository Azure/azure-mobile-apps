// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Xunit;

namespace Datasync.Common.TestData;

/// <summary>
/// A set of test data for testing equality for ITableData based entities.
/// </summary>
[ExcludeFromCodeCoverage]
public class ITableData_TestData : TheoryData<TableData, TableData, bool>
{
    private readonly string[] ids = new string[]
    {
        "",
        "t1",
        Guid.NewGuid().ToString()
    };

    private readonly DateTimeOffset[] dates = new DateTimeOffset[]
    {
        DateTimeOffset.MinValue,
        DateTimeOffset.UnixEpoch,
        DateTimeOffset.Now.AddDays(-1),
        DateTimeOffset.UtcNow,
        DateTimeOffset.MaxValue
    };

    // Note: PgEntityTableData uses a uint32 for the RowVersion, which is decoded
    // into the Version byte array.  This means that, for testing purposes, we can
    // only test 4-byte arrays with certainty.
    private readonly byte[][] versions = new byte[][]
    {
        Array.Empty<byte>(),
        new byte[] { 0x01, 0x02, 0x03, 0x04 },
        new byte[] { 0x60, 0x50, 0x40, 0x30 }
    };

    public ITableData_TestData()
    {
        List<TableData> sourceMaterial = new();
        foreach (var id in ids)
        {
            foreach (var date in dates)
            {
                foreach (var version in versions)
                {
                    sourceMaterial.Add(new TableData { Id = id, Deleted = false, UpdatedAt = date, Version = version.ToArray() });
                    sourceMaterial.Add(new TableData { Id = id, Deleted = true, UpdatedAt = date, Version = version.ToArray() });
                }
            }
        }

        for (int a = 0; a < sourceMaterial.Count; a++)
        {
            for (int b = 0; b < sourceMaterial.Count; b++)
            {
                Add(sourceMaterial[a], sourceMaterial[b], sourceMaterial[a].Id == sourceMaterial[b].Id && sourceMaterial[a].Version.SequenceEqual(sourceMaterial[b].Version));
            }
        }
    }
}
