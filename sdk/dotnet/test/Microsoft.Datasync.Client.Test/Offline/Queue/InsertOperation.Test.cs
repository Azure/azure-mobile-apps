// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Offline;
using Microsoft.Datasync.Client.Offline.Queue;
using Microsoft.Datasync.Client.Test.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Datasync.Client.Test.Offline.Queue;

[ExcludeFromCodeCoverage]
public class InsertOperation_Tests : BaseOperationTest
{
    private readonly JObject serializedObject = JObject.Parse("{\"kind\":2,\"itemId\":\"1234\",\"item\":null, \"sequence\":0, \"state\": 0, \"tableName\":\"test\", \"version\":1}");
    private readonly JObject testObject = JObject.Parse("{\"id\":\"1234\"}");

    [Fact]
    public void Ctor_NullTable_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new InsertOperation(null, "1234"));
    }

    [Fact]
    public void Ctor_NullItemId_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new InsertOperation("test", null));
    }

    [Theory]
    [MemberData(nameof(BaseOperationTest.GetInvalidTableNames), MemberType = typeof(BaseOperationTest))]
    public void Ctor_InvalidTable_Throws(string tableName)
    {
        Assert.Throws<ArgumentException>(() => new InsertOperation(tableName, "1234"));
    }

    [Theory]
    [MemberData(nameof(BaseOperationTest.GetInvalidIds), MemberType = typeof(BaseOperationTest))]
    public void Ctor_InvalidId_Throws(string itemId)
    {
        Assert.Throws<ArgumentException>(() => new InsertOperation("test", itemId));
    }

    [Fact]
    public void Ctor_SetsContext()
    {
        var operation = new InsertOperation("test", "1234");

        // Note - we check the int value for compatibility at the storage level.
        Assert.Equal(2, (int)operation.Kind);
        Assert.NotEmpty(operation.Id);
        Assert.Equal("1234", operation.ItemId);
        Assert.Equal("test", operation.TableName);
        Assert.Null(operation.Item);
        Assert.Equal(0, operation.Sequence);
        // Note - we check the int value for compatibility at the storage level.
        Assert.Equal(0, (int)operation.State);
        Assert.Equal(1, operation.Version);
        Assert.True(operation.CanWriteResultToStore);
        Assert.False(operation.SerializeItemToQueue);
    }

    [Fact]
    public void Equals_TableOperation_Works()
    {
        var sut = new InsertOperation("test", "1234") { Item = testObject };
        var other = new InsertOperation("test", "1234") { Item = testObject };
        var notother = new DeleteOperation("test", "1234");

        Assert.True(sut.Equals(other));
        Assert.False(sut.Equals(notother));
    }

    [Fact]
    public void Equals_Object_Works()
    {
        var sut = new InsertOperation("test", "1234") { Item = testObject };
        var other = new InsertOperation("test", "1234") { Item = testObject };
        var notother = new DeleteOperation("test", "1234");
        object notoperation = new();

        Assert.True(sut.Equals((object)other));
        Assert.False(sut.Equals((object)notother));
        Assert.False(sut.Equals(notoperation));
    }

    [Fact]
    public void GetHashCode_Works()
    {
        var sut = new InsertOperation("test", "1234") { Item = testObject };
        var hash = "1234".GetHashCode();

        Assert.Equal(hash, sut.GetHashCode());
    }

    [Fact]
    public void Operation_CanAbort()
    {
        var operation = new InsertOperation("test", "1234");
        Assert.Throws<PushAbortedException>(() => operation.AbortPush());
    }

    [Fact]
    public void Operation_CanBeCancelled()
    {
        var operation = new InsertOperation("test", "1234");
        Assert.False(operation.IsCancelled);
        operation.Cancel();
        Assert.True(operation.IsCancelled);
    }

    [Fact]
    public void Operation_CanBeUpdated()
    {
        var operation = new InsertOperation("test", "1234");
        Assert.False(operation.IsUpdated);
        operation.Update();
        Assert.Equal(2, operation.Version);
        Assert.True(operation.IsUpdated);
    }

    [Fact]
    public void Operation_CanSetItem()
    {
        var operation = new InsertOperation("test", "1234") { Item = testObject };
        Assert.Equal(operation.Item, testObject);
    }

    [Fact]
    public void Serialize_Works_WithItem()
    {
        var operation = new InsertOperation("test", "1234") { Item = testObject };

        JObject actual = operation.Serialize();
        actual.Remove("id");

        Assert.Equal(serializedObject, actual);
    }

    [Fact]
    public void Serialize_Works_WithoutItem()
    {
        var operation = new InsertOperation("test", "1234");
        serializedObject["item"] = null;   // We aren't expecting this.

        JObject actual = operation.Serialize();
        actual.Remove("id");

        Assert.Equal(serializedObject, actual);
    }

    [Fact]
    public void Deserialize_Null_ReturnsNull()
    {
        var operation = TableOperation.Deserialize(null);
        Assert.Null(operation);
    }

    [Fact]
    public void Deserialize_Works_WithItem()
    {
        var expected = new InsertOperation("test", "1234") { Item = testObject };
        var operation = TableOperation.Deserialize(serializedObject);
        Assert.IsAssignableFrom<InsertOperation>(operation);
        Assert.Equal(expected, operation);
    }

    [Fact]
    public void Deserialize_Works_WithoutItem()
    {
        var expected = new InsertOperation("test", "1234");
        serializedObject["item"] = null;    // We aren't expecting this.
        var operation = TableOperation.Deserialize(serializedObject);
        Assert.IsAssignableFrom<InsertOperation>(operation);
        Assert.Equal(expected, operation);
    }

    [Fact]
    public void ValidateCollapse_MismatchedItemId_Throws()
    {
        var sut = new InsertOperation("test", "1234");
        var newOp = new UpdateOperation("test", "4321");

        Assert.Throws<ArgumentException>(() => sut.ValidateOperationCanCollapse(newOp));
    }

    [Fact]
    public void ValidateCollapse_InsertOperation_Throws()
    {
        var sut = new InsertOperation("test", "1234");
        var newOp = new InsertOperation("test", "1234");

        Assert.Throws<InvalidOperationException>(() => sut.ValidateOperationCanCollapse(newOp));
    }

    [Fact]
    public void ValidateCollapse_PendingDeleteOperation_OK()
    {
        var sut = new InsertOperation("test", "1234");
        var newOp = new DeleteOperation("test", "1234") { State = TableOperationState.Pending };

        sut.ValidateOperationCanCollapse(newOp);
    }

    [Fact]
    public void ValidateCollapse_ActiveDeleteOperation_Throws()
    {
        var sut = new InsertOperation("test", "1234") { State = TableOperationState.Failed };
        var newOp = new DeleteOperation("test", "1234");

        Assert.Throws<InvalidOperationException>(() => sut.ValidateOperationCanCollapse(newOp));
    }

    [Fact]
    public void ValidateCollapse_UpdateOperation_OK()
    {
        var sut = new InsertOperation("test", "1234");
        var newOp = new UpdateOperation("test", "1234");

        sut.ValidateOperationCanCollapse(newOp);
    }

    [Fact]
    public void Collapse_MismatchedItemId_Throws()
    {
        var sut = new InsertOperation("test", "1234");
        var newOp = new UpdateOperation("test", "4321");

        Assert.Throws<ArgumentException>(() => sut.CollapseOperation(newOp));
    }

    [Fact]
    public void Collapse_DeleteOperation()
    {
        var sut = new InsertOperation("test", "1234");
        var newOp = new DeleteOperation("test", "1234");

        sut.CollapseOperation(newOp);

        Assert.True(sut.IsCancelled);
        Assert.True(newOp.IsCancelled);
    }

    [Fact]
    public void Collapse_UpdateOperation()
    {
        var sut = new InsertOperation("test", "1234");
        var newOp = new UpdateOperation("test", "1234");

        sut.CollapseOperation(newOp);

        Assert.False(sut.IsCancelled);
        Assert.True(sut.IsUpdated);
        Assert.True(newOp.IsCancelled);
    }

    [Fact]
    public async Task ExecuteOnOfflineStore_ExistingItem_Throws()
    {
        var store = Substitute.For<IOfflineStore>();
        store.GetItemAsync("test", "1234", Arg.Any<CancellationToken>()).Returns(Task.FromResult(testObject));
        store.UpsertAsync("test", Arg.Any<IEnumerable<JObject>>(), Arg.Any<bool>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        var sut = new InsertOperation("test", "1234");

        await Assert.ThrowsAsync<OfflineStoreException>(() => sut.ExecuteOperationOnOfflineStoreAsync(store, testObject));
    }

    [Fact]
    public async Task ExecuteOnOfflineStore_CallsUpsert()
    {
        var store = Substitute.For<IOfflineStore>();
        store.GetItemAsync("test", "1234", Arg.Any<CancellationToken>()).Returns(Task.FromResult((JObject)null));
        store.UpsertAsync("test", Arg.Any<IEnumerable<JObject>>(), Arg.Any<bool>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        var sut = new InsertOperation("test", "1234");

        await sut.ExecuteOperationOnOfflineStoreAsync(store, testObject);

        await store.Received(1).UpsertAsync("test", Arg.Is<IEnumerable<JObject>>(ops => ops.Count() == 1 && ops.First().Equals(testObject)), false, default);
        await store.Received(1).GetItemAsync("test", "1234", default);
    }

    [Fact]
    public async Task ExecuteRemote_ReturnsNull_WhenCancelled()
    {
        var client = GetMockClient();
        MockHandler.AddResponse(HttpStatusCode.OK, new IdEntity { Id = "1234", StringValue = "foo" });

        var sut = new InsertOperation("test", "1234") { Item = testObject };
        sut.Cancel();
        var actual = await sut.ExecuteOperationOnRemoteServiceAsync(client);
        Assert.Null(actual);
    }

    [Fact]
    public async Task ExecuteRemote_CallsRemoteServer_WithSuccess()
    {
        var client = GetMockClient();
        MockHandler.AddResponse(HttpStatusCode.OK, new IdEntity { Id = "1234", StringValue = "foo" });

        var sut = new InsertOperation("test", "1234") { Item = testObject };
        await sut.ExecuteOperationOnRemoteServiceAsync(client);

        Assert.Single(MockHandler.Requests);
        Assert.Equal(HttpMethod.Post, MockHandler.Requests[0].Method);
        Assert.Equal(new Uri(Endpoint, "/tables/test"), MockHandler.Requests[0].RequestUri);
        Assert.Equal(testObject.ToString(Formatting.None), await MockHandler.Requests[0].Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task ExecuteRemote_CallsRemoteServer_ThrowsWhenInvalidResponse()
    {
        var client = GetMockClient();
        MockHandler.AddResponse(HttpStatusCode.OK, new string[] { "test" });

        var sut = new InsertOperation("test", "1234") { Item = testObject };
        await Assert.ThrowsAsync<DatasyncInvalidOperationException>(() => sut.ExecuteOperationOnRemoteServiceAsync(client));
    }

    [Fact]
    public async Task ExecuteRemote_ThrowsError_WithNoItem()
    {
        var client = GetMockClient();
        MockHandler.AddResponse(HttpStatusCode.OK);

        var sut = new InsertOperation("test", "1234");
        var exception = await Assert.ThrowsAsync<DatasyncInvalidOperationException>(() => sut.ExecuteOperationOnRemoteServiceAsync(client));

        Assert.Empty(MockHandler.Requests);
        Assert.Contains("must have an item", exception.Message);
    }

    [Theory]
    [InlineData(HttpStatusCode.BadRequest)]
    [InlineData(HttpStatusCode.Conflict)]
    [InlineData(HttpStatusCode.PreconditionFailed)]
    public async Task ExecuteRemote_CallsRemoteServer_WithFailure(HttpStatusCode statusCode)
    {
        var client = GetMockClient();
        if (statusCode == HttpStatusCode.Conflict || statusCode == HttpStatusCode.PreconditionFailed)
        {
            MockHandler.AddResponse(statusCode, new IdEntity { Id = "1234", StringValue = "movie" });
        }
        else
        {
            MockHandler.AddResponse(statusCode);
        }

        var sut = new InsertOperation("test", "1234") { Item = testObject };
        var exception = await Assert.ThrowsAnyAsync<DatasyncInvalidOperationException>(() => sut.ExecuteOperationOnRemoteServiceAsync(client));

        if (statusCode == HttpStatusCode.Conflict || statusCode == HttpStatusCode.PreconditionFailed)
        {
            Assert.IsAssignableFrom<DatasyncConflictException>(exception);
            Assert.NotNull(exception.Value);
        }
        Assert.NotNull(exception.Request);
        Assert.NotNull(exception.Response);
        Assert.Equal(statusCode, exception.Response?.StatusCode);
    }
}
