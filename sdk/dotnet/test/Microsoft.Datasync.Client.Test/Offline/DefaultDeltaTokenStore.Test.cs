// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Test.Mocks;
using Microsoft.Datasync.Client.Offline;
using Microsoft.Datasync.Client.Offline.DeltaToken;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Datasync.Client.Test.Offline;

[ExcludeFromCodeCoverage]
public class DefaultDeltaTokenStore_Tests
{
    private readonly MockOfflineStore store;
    private readonly DefaultDeltaTokenStore sut;
    private const string testTBL = "movies";
    private const string testQID = "qid-1234-test";
    private readonly DateTimeOffset testDTO = DateTimeOffset.Parse("2022-03-15T14:22:20.050000+00:00");
    private const string testKEY = "dt.movies.qid-1234-test";
    private readonly long testVAL;
    private readonly string json;

    public DefaultDeltaTokenStore_Tests()
    {
        store = new MockOfflineStore();
        sut = new DefaultDeltaTokenStore(store);
        testVAL = testDTO.ToUnixTimeMilliseconds();
        json = $"{{\"id\":\"{testKEY}\",\"value\":{testVAL}}}";
    }

    #region Helpers
    /// <summary>
    /// A set of invalid table names for testing
    /// </summary>
    /// <remarks>
    /// Do not allow the system tables.
    /// </remarks>
    public static IEnumerable<object[]> GetInvalidTableNames() => new List<object[]>
    {
        new object[] { "" },
        new object[] { " " },
        new object[] { "\t" },
        new object[] { "abcdef gh" },
        new object[] { "!!!" },
        new object[] { "?" },
        new object[] { ";" },
        new object[] { "{EA235ADF-9F38-44EA-8DA4-EF3D24755767}" },
        new object[] { "###" },
        new object[] { "1abcd" },
        new object[] { "true.false" },
        new object[] { "a-b-c-d" },
        new object[] { "__queue" },
        new object[] { "__errors" },
        new object[] { "__delta" }
    };

    /// <summary>
    /// A set of invalid query IDs for testing
    /// </summary>
    /// <remarks>
    /// Tests for this directory do not allow the system tables.
    /// </remarks>
    public static IEnumerable<object[]> GetInvalidQueryIds() => new List<object[]>
    {
        new object[] { "" },
        new object[] { " " },
        new object[] { "\t" },
    };
    #endregion

    [Fact]
    public void TableDefinition_Serializes()
    {
        var actual = DefaultDeltaTokenStore.TableDefinition.ToString(Formatting.None);
        const string expected = "{\"id\":\"\",\"value\":0}";
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Ctor_NullStore_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new DefaultDeltaTokenStore(null));
    }

    [Fact]
    public async Task GetDeltaTokenAsync_NullTable_Throws()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() => sut.GetDeltaTokenAsync(null, testQID));
    }

    [Theory]
    [MemberData(nameof(GetInvalidTableNames))]
    public async Task GetDeltaTokenAsync_InvalidTable_Throws(string tableName)
    {
        await Assert.ThrowsAsync<ArgumentException>(() => sut.GetDeltaTokenAsync(tableName, testQID));
    }

    [Fact]
    public async Task GetDeltaTokenAsync_NullQueryId_Throws()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() => sut.GetDeltaTokenAsync(testTBL, null));
    }

    [Theory]
    [MemberData(nameof(GetInvalidQueryIds))]
    public async Task GetDeltaTokenAsync_InvalidQueryId_Throws(string queryId)
    {
        await Assert.ThrowsAsync<ArgumentException>(() => sut.GetDeltaTokenAsync(testTBL, queryId));
    }

    [Fact]
    public async Task GetDeltaTokenAsync_DoesntExist_ReturnsEpoch()
    {
        var actual = await sut.GetDeltaTokenAsync(testTBL, testQID);
        Assert.Equal(DateTimeOffset.UnixEpoch, actual);
    }

    [Fact]
    public async Task GetDeltaTokenAsync_Exists_ReturnsData()
    {
        store.Upsert(SystemTables.Configuration, new[] { JObject.Parse(json) });
        var actual = await sut.GetDeltaTokenAsync(testTBL, testQID);
        Assert.Equal(testDTO, actual.ToUniversalTime());
    }

    [Fact]
    public async Task GetDeltaTokenAsync_Exists_ReturnsCachedData()
    {
        store.Upsert(SystemTables.Configuration, new[] { JObject.Parse(json) });
        var actual = await sut.GetDeltaTokenAsync(testTBL, testQID);
        Assert.Equal(testDTO, actual.ToUniversalTime());

        // This one returns the cached data - we don't see it's cached.
        actual = await sut.GetDeltaTokenAsync(testTBL, testQID);
        Assert.Equal(testDTO, actual.ToUniversalTime());
    }

    [Fact]
    public async Task GetDeltaTokenAsync_BadData_Throws()
    {
        store.Upsert(SystemTables.Configuration, new[] { JObject.Parse($"{{\"id\":\"{testKEY}\",\"value\":\"some-bad-date\"}}") });
        await Assert.ThrowsAnyAsync<Exception>(() => sut.GetDeltaTokenAsync(testTBL, testQID));
    }

    [Fact]
    public async Task ResetDeltaTokenAsync_NullTable_Throws()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() => sut.ResetDeltaTokenAsync(null, testQID));
    }

    [Theory]
    [MemberData(nameof(GetInvalidTableNames))]
    public async Task ResetDeltaTokenAsync_InvalidTable_Throws(string tableName)
    {
        await Assert.ThrowsAsync<ArgumentException>(() => sut.ResetDeltaTokenAsync(tableName, testQID));
    }

    [Fact]
    public async Task ResetDeltaTokenAsync_NullQueryId_Throws()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() => sut.ResetDeltaTokenAsync(testTBL, null));
    }

    [Theory]
    [MemberData(nameof(GetInvalidQueryIds))]
    public async Task ResetDeltaTokenAsync_InvalidQueryId_Throws(string queryId)
    {
        await Assert.ThrowsAsync<ArgumentException>(() => sut.ResetDeltaTokenAsync(testTBL, queryId));
    }

    [Fact]
    public async Task ResetDeltaTokenAsync_NotExist_Works()
    {
        await sut.ResetDeltaTokenAsync(testTBL, testQID);
        Assert.Empty(store.TableMap[SystemTables.Configuration]);
    }

    [Fact]
    public async Task ResetDeltaTokenAsync_ExistsNotInCache_Works()
    {
        store.Upsert(SystemTables.Configuration, new[] { JObject.Parse(json) });
        await sut.ResetDeltaTokenAsync(testTBL, testQID);
        Assert.Empty(store.TableMap[SystemTables.Configuration]);
    }

    [Fact]
    public async Task ResetDeltaTokenAsync_ExistsInCache_Works()
    {
        store.Upsert(SystemTables.Configuration, new[] { JObject.Parse(json) });
        _ = await sut.GetDeltaTokenAsync(testTBL, testQID);
        await sut.ResetDeltaTokenAsync(testTBL, testQID);
        Assert.Empty(store.TableMap[SystemTables.Configuration]);
    }

    [Fact]
    public async Task ResetDeltaTokenAsync_DoesntAffectOtherKeys()
    {
        store.Upsert(SystemTables.Configuration, new[] { JObject.Parse(json), JObject.Parse("{\"id\":\"1234\",\"value\":42}") });
        await sut.ResetDeltaTokenAsync(testTBL, testQID);
        Assert.True(store.TableMap[SystemTables.Configuration].ContainsKey("1234"));
        Assert.Equal(42, store.TableMap[SystemTables.Configuration]["1234"]?.Value<long>("value"));
    }

    [Fact]
    public async Task SetDeltaTokenAsync_NullTable_Throws()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() => sut.SetDeltaTokenAsync(null, testQID, testDTO));
    }

    [Theory]
    [MemberData(nameof(GetInvalidTableNames))]
    public async Task SetDeltaTokenAsync_InvalidTable_Throws(string tableName)
    {
        await Assert.ThrowsAsync<ArgumentException>(() => sut.SetDeltaTokenAsync(tableName, testQID, testDTO));
    }

    [Fact]
    public async Task SetDeltaTokenAsync_NullQueryId_Throws()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() => sut.SetDeltaTokenAsync(testTBL, null, testDTO));
    }

    [Theory]
    [MemberData(nameof(GetInvalidQueryIds))]
    public async Task SetDeltaTokenAsync_InvalidQueryId_Throws(string queryId)
    {
        await Assert.ThrowsAsync<ArgumentException>(() => sut.SetDeltaTokenAsync(testTBL, queryId, testDTO));
    }

    [Fact]
    public async Task SetDeltaTokenAsync_Overwrites_Existing()
    {
        store.Upsert(SystemTables.Configuration, new[] { JObject.Parse(json) });
        var dto = DateTimeOffset.UtcNow;
        await sut.SetDeltaTokenAsync(testTBL, testQID, dto);
        Assert.Equal(dto.ToUnixTimeMilliseconds(), store.TableMap[SystemTables.Configuration][testKEY]?.Value<long>("value"));
    }

    [Fact]
    public async Task SetDeltaTokenAsync_Creates_NotExisting()
    {
        var dto = DateTimeOffset.UtcNow;
        await sut.SetDeltaTokenAsync(testTBL, testQID, dto);
        Assert.Equal(dto.ToUnixTimeMilliseconds(), store.TableMap[SystemTables.Configuration][testKEY]?.Value<long>("value"));
    }

    [Fact]
    public async Task SetDeltaTokenAsync_DoesntAffectOtherKeys()
    {
        store.Upsert(SystemTables.Configuration, new[] { JObject.Parse(json), JObject.Parse("{\"id\":\"1234\",\"value\":42}") });
        var dto = DateTimeOffset.UtcNow;
        await sut.SetDeltaTokenAsync(testTBL, testQID, dto);
        Assert.True(store.TableMap[SystemTables.Configuration].ContainsKey("1234"));
        Assert.Equal(42, store.TableMap[SystemTables.Configuration]["1234"]?.Value<long>("value"));
    }
}
