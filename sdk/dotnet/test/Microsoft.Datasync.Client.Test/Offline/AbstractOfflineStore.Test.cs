// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Offline;
using Microsoft.Datasync.Client.Query;
using Microsoft.Datasync.Client.Table;
using Newtonsoft.Json.Linq;

namespace Microsoft.Datasync.Client.Test.Offline;

[ExcludeFromCodeCoverage]
public class AbstractOfflineStore_Tests
{
    [Fact]
    public async Task Initialize_DefinesSystemTables()
    {
        var store = new ConcreteOfflineStore();
        await store.InitializeAsync();

        Assert.True(store.isInitialized);
        foreach (var tableName in SystemTables.AllTables)
        {
            Assert.True(store.tableDefinitions.ContainsKey(tableName));
        }
    }

    //[Fact]
    //public async Task EnsureInitializedAsync_Throws_WhenNotInitialized()
    //{
    //    var store = new ConcreteOfflineStore();
    //    await Assert.ThrowsAnyAsync<InvalidOperationException>(() => store.C_EnsureInitializedAsync());
    //}

    [Fact]
    public async Task EnsureInitializedAsync_DoesntThrow_WhenInitialized()
    {
        var store = new ConcreteOfflineStore();
        await store.InitializeAsync();
        await store.C_EnsureInitializedAsync();
    }

    [Fact]
    public async Task AbstractOfflineStore_CanDispose()
    {
        var store = new ConcreteOfflineStore();
        await store.InitializeAsync();
        store.Dispose();
    }
}

/// <summary>
/// This is the implementation of the <see cref="AbstractOfflineStore"/> for testing.
/// </summary>
/// <remarks>
/// Methods starting with <c>C_</c> are concrete overlaps of internal methods.
/// </remarks>
[ExcludeFromCodeCoverage]
public class ConcreteOfflineStore : AbstractOfflineStore
{
    public Dictionary<string, JObject> tableDefinitions = new();
    public bool isInitialized = false;

    #region IOfflineStore
    public override Task DeleteAsync(QueryDescription query, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public override Task DeleteAsync(string tableName, IEnumerable<string> ids, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public override Task<JObject> GetItemAsync(string tableName, string id, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public override Task<Page<JObject>> GetPageAsync(QueryDescription query, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public override Task<IList<string>> GetTablesAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public override Task UpsertAsync(string tableName, IEnumerable<JObject> items, bool ignoreMissingColumns, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
    #endregion

    protected override Task InitializeOfflineStoreAsync(CancellationToken cancellationToken)
    {
        isInitialized = true;
        return Task.CompletedTask;
    }

    public override void DefineTable(string tableName, JObject tableDefinition)
    {
        tableDefinitions[tableName] = tableDefinition;
    }

    public override bool TableIsDefined(string tableName)
        => tableDefinitions.ContainsKey(tableName);

    internal Task C_EnsureInitializedAsync() => base.EnsureInitializedAsync(default);
}
