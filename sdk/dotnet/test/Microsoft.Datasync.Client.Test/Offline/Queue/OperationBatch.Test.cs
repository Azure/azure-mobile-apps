// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Test;
using Datasync.Common.Test.Mocks;
using Microsoft.Datasync.Client.Offline;
using Microsoft.Datasync.Client.Offline.Queue;
using Microsoft.Datasync.Client.Table;
using Microsoft.Datasync.Client.Test.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Datasync.Client.Test.Offline.Queue;

[ExcludeFromCodeCoverage]
public class OperationBatch_Tests : BaseTest
{
    private readonly DatasyncClient client;
    private readonly MockOfflineStore store;
    private readonly SyncContext context;

    public OperationBatch_Tests()
    {
        client = GetMockClient();
        store = new MockOfflineStore();
        context = new SyncContext(client, store);
    }

    #region Helpers
    private async Task<OperationBatch> CreateBatch()
    {
        await context.InitializeAsync();
        return new OperationBatch(context);
    }

    private List<TableOperationError> CreateErrorItems(int count)
    {
        List<TableOperationError> result = new();

        for (int i = 0; i < count; i++)
        {
            TableOperation op = (i % 3) switch
            {
                0 => new DeleteOperation("movies", Guid.NewGuid().ToString()),
                1 => new InsertOperation("movies", Guid.NewGuid().ToString()),
                _ => new UpdateOperation("movies", Guid.NewGuid().ToString())
            };
            TableOperationError err = new(op, context, HttpStatusCode.BadRequest, null, null);
            result.Add(err);
        }

        return result;
    }
    #endregion

    [Fact]
    [Trait("Method", "Ctor")]
    public void Ctor_Throws_WithNullContext()
    {
        Assert.Throws<ArgumentNullException>(() => new OperationBatch(null));
    }

    [Fact]
    [Trait("Method", "Ctor")]
    public void Ctor_Sets_Context()
    {
        var batch = new OperationBatch(context);

        Assert.Same(context, batch.Context);
        Assert.Same(store, batch.OfflineStore);
        Assert.NotNull(batch.SerializerSettings);
        Assert.Empty(batch.OtherErrors);
        Assert.Null(batch.AbortReason);
    }

    [Fact]
    [Trait("Method", "Abort")]
    public void Abort_Complete_Throws()
    {
        var batch = new OperationBatch(context);

        Assert.Throws<ArgumentException>(() => batch.Abort(PushStatus.Complete));
    }

    [Fact]
    [Trait("Method", "Abort")]
    public void Abort_SetsReason()
    {
        var batch = new OperationBatch(context);

        batch.Abort(PushStatus.InternalError);

        Assert.Equal(PushStatus.InternalError, batch.AbortReason);
    }

    [Fact]
    [Trait("Method", "AddErrorAsync")]
    public async Task AddErrorAsync_Throws_OnNullOperation()
    {
        var batch = await CreateBatch();

        await Assert.ThrowsAsync<ArgumentNullException>(() => batch.AddErrorAsync(null, null, "", null));
    }

    [Fact]
    [Trait("Method", "AddErrorAsync")]
    public async Task AddErrorAsync_CreatesErrorFromOperation_NullResult()
    {
        var batch = await CreateBatch();
        var op = new InsertOperation("movies", Guid.NewGuid().ToString());
        var item = new IdEntity { Id = Guid.NewGuid().ToString(), StringValue = "test" };
        var obj = JObject.FromObject(item);

        await batch.AddErrorAsync(op, null, null, obj);

        Assert.Single(store.TableMap[SystemTables.SyncErrors]);
        var error = store.TableMap[SystemTables.SyncErrors].Values.First();

        Assert.NotEmpty(error.Value<string>("id"));
        Assert.Null(error.Value<int?>("status"));
        Assert.Equal("movies", error.Value<string>("tableName"));
        Assert.Equal(1, error.Value<int>("version"));
        Assert.Equal(2, error.Value<int>("kind"));
        Assert.Null(error.Value<string>("item"));
        Assert.Null(error.Value<string>("rawResult"));
    }

    [Fact]
    [Trait("Method", "AddErrorAsync")]
    public async Task AddErrorAsync_CreatesErrorFromOperation_WithResult()
    {
        var batch = await CreateBatch();
        var op = new InsertOperation("movies", Guid.NewGuid().ToString());
        var item = new IdEntity { Id = Guid.NewGuid().ToString(), StringValue = "test" };
        var obj = JObject.FromObject(item);
        var json = obj.ToString(Formatting.None);

        await batch.AddErrorAsync(op, null, json, obj);

        Assert.Single(store.TableMap[SystemTables.SyncErrors]);
        var error = store.TableMap[SystemTables.SyncErrors].Values.First();

        Assert.NotEmpty(error.Value<string>("id"));
        Assert.Null(error.Value<int?>("status"));
        Assert.Equal("movies", error.Value<string>("tableName"));
        Assert.Equal(1, error.Value<int>("version"));
        Assert.Equal(2, error.Value<int>("kind"));
        Assert.Null(error.Value<string>("item"));
        Assert.Equal(json, error.Value<string>("rawResult"));
    }

    [Fact]
    [Trait("Method", "AddErrorAsync")]
    public async Task AddErrorAsync_CreatesErrorFromOperation_NullResult_WithStatus()
    {
        var batch = await CreateBatch();
        var op = new InsertOperation("movies", Guid.NewGuid().ToString());
        var item = new IdEntity { Id = Guid.NewGuid().ToString(), StringValue = "test" };
        var obj = JObject.FromObject(item);

        await batch.AddErrorAsync(op, HttpStatusCode.BadRequest, null, obj);

        Assert.Single(store.TableMap[SystemTables.SyncErrors]);
        var error = store.TableMap[SystemTables.SyncErrors].Values.First();

        Assert.NotEmpty(error.Value<string>("id"));
        Assert.Equal(400, error.Value<int?>("status"));
        Assert.Equal("movies", error.Value<string>("tableName"));
        Assert.Equal(1, error.Value<int>("version"));
        Assert.Equal(2, error.Value<int>("kind"));
        Assert.Null(error.Value<string>("item"));
        Assert.Null(error.Value<string>("rawResult"));
    }

    [Fact]
    [Trait("Method", "AddErrorAsync")]
    public async Task AddErrorAsync_CreatesErrorFromOperation_WithResult_WithStatus()
    {
        var batch = await CreateBatch();
        var op = new InsertOperation("movies", Guid.NewGuid().ToString());
        var item = new IdEntity { Id = Guid.NewGuid().ToString(), StringValue = "test" };
        var obj = JObject.FromObject(item);
        var json = obj.ToString(Formatting.None);

        await batch.AddErrorAsync(op, HttpStatusCode.BadRequest, json, obj);

        Assert.Single(store.TableMap[SystemTables.SyncErrors]);
        var error = store.TableMap[SystemTables.SyncErrors].Values.First();

        Assert.NotEmpty(error.Value<string>("id"));
        Assert.Equal(400, error.Value<int?>("status"));
        Assert.Equal("movies", error.Value<string>("tableName"));
        Assert.Equal(1, error.Value<int>("version"));
        Assert.Equal(2, error.Value<int>("kind"));
        Assert.Null(error.Value<string>("item"));
        Assert.Equal(json, error.Value<string>("rawResult"));
    }

    [Fact]
    [Trait("Method", "AddErrorAsync")]
    public async Task AddErrorAsync_CreatesErrorFromOperation_WithResultAndItemAndStatus()
    {
        var batch = await CreateBatch();
        var item = new IdEntity { Id = Guid.NewGuid().ToString(), StringValue = "test" };
        var obj = JObject.FromObject(item);
        var json = obj.ToString(Formatting.None);
        var op = new DeleteOperation("movies", Guid.NewGuid().ToString()) { Item = obj };

        await batch.AddErrorAsync(op, HttpStatusCode.BadRequest, json, obj);

        Assert.Single(store.TableMap[SystemTables.SyncErrors]);
        var error = store.TableMap[SystemTables.SyncErrors].Values.First();

        Assert.NotEmpty(error.Value<string>("id"));
        Assert.Equal(400, error.Value<int?>("status"));
        Assert.Equal("movies", error.Value<string>("tableName"));
        Assert.Equal(1, error.Value<int>("version"));
        Assert.Equal(1, error.Value<int>("kind"));
        Assert.Equal(json, error.Value<string>("item"));
        Assert.Equal(json, error.Value<string>("rawResult"));
    }

    [Fact]
    [Trait("Method", "HasErrors")]
    public void HasErrors_False_WhenNoUnhandledErrors()
    {
        var batch = new OperationBatch(context);
        var errors = Array.Empty<TableOperationError>();

        Assert.False(batch.HasErrors(errors));
    }

    [Fact]
    [Trait("Method", "HasErrors")]
    public void HasErrors_True_WhenUnhandledErrors()
    {
        var batch = new OperationBatch(context);
        var op = new InsertOperation("movies", Guid.NewGuid().ToString());
        var errors = new TableOperationError[] { new TableOperationError(op, context, null, null, null) { Handled = false } };

        Assert.True(batch.HasErrors(errors));
    }

    [Fact]
    [Trait("Method", "HasErrors")]
    public void HasErrors_False_WhenHandledErrors()
    {
        var batch = new OperationBatch(context);
        var op = new InsertOperation("movies", Guid.NewGuid().ToString());
        var errors = new TableOperationError[] { new TableOperationError(op, context, null, null, null) { Handled = true } };

        Assert.False(batch.HasErrors(errors));
    }

    [Fact]
    [Trait("Method", "HasErrors")]
    public void HasErrors_False_WhenNullErrors()
    {
        var batch = new OperationBatch(context);

        Assert.False(batch.HasErrors(null));
    }

    [Fact]
    [Trait("Method", "HasErorrs")]
    public void HasErrors_True_WhenOtherErrors()
    {
        var batch = new OperationBatch(context);
        batch.OtherErrors.Add(new ApplicationException());
        var errors = Array.Empty<TableOperationError>();

        Assert.True(batch.HasErrors(errors));
    }

    [Fact]
    [Trait("Method", "LoadErrorsAsync")]
    public async Task LoadErrorsAsync_Works_WhenNoErrors()
    {
        store.ReadPageFunc = _ => new Page<JObject>() { Items = Array.Empty<JObject>() };
        var batch = await CreateBatch();

        var errors = await batch.LoadErrorsAsync();

        Assert.Empty(errors);
    }

    [Fact]
    [Trait("Method", "LoadErrorsAsync")]
    public async Task LoadErrorsAsync_Works_WithSinglePage()
    {
        const int count = 20;
        var inputList = CreateErrorItems(count);
        store.ReadPageFunc = _ => new Page<JObject>() { Items = inputList.Select(err => err.Serialize()).ToArray() };
        var batch = await CreateBatch();

        var errors = await batch.LoadErrorsAsync();

        Assert.Equal(count, errors.Count);
        for (int i = 0; i < count; i++)
        {
            Assert.True(inputList[i].Equals(errors[i]), $"Errors at index {i} mismatch");
        }
    }


    [Fact]
    [Trait("Method", "LoadErrorsAsync")]
    public async Task LoadErrorsAsync_Works_WithMultiplePages()
    {
        const int count = 20;
        var inputList = CreateErrorItems(count);
        store.ReadPageFunc = query =>
        {
            Assert.Equal(SystemTables.SyncErrors, query.TableName);
            int offset = query.Skip ?? 0;
            if (offset > count)
            {
                return new Page<JObject>();
            }
            else
            {
                return new Page<JObject>()
                {
                    Items = inputList.Skip(offset).Take(5).Select(err => err.Serialize()).ToArray(),
                    NextLink = new Uri($"https://inmemorystore.local/tables/__syncerrors?$skip={offset + 5}&$top=5")
                };
            }
        };
        var batch = await CreateBatch();

        var errors = await batch.LoadErrorsAsync();

        Assert.Equal(count, errors.Count);
        for (int i = 0; i < count; i++)
        {
            Assert.True(inputList[i].Equals(errors[i]), $"Errors at index {i} mismatch");
        }
    }
}
