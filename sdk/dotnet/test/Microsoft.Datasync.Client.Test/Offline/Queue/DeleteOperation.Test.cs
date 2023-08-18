// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Offline;
using Microsoft.Datasync.Client.Offline.Queue;
using Microsoft.Datasync.Client.Test.Helpers;
using Newtonsoft.Json.Linq;

namespace Microsoft.Datasync.Client.Test.Offline.Queue;

[ExcludeFromCodeCoverage]
public class DeleteOperation_Tests : BaseOperationTest
{
    private readonly JObject serializedObject = JObject.Parse("{\"kind\":1,\"itemId\":\"1234\",\"item\":\"{\\\"id\\\":\\\"1234\\\"}\", \"sequence\":0, \"state\": 0, \"tableName\":\"test\", \"version\":1}");
    private readonly JObject testObject = JObject.Parse("{\"id\":\"1234\"}");

    [Fact]
    public void Ctor_NullTable_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new DeleteOperation(null, "1234"));
    }

    [Fact]
    public void Ctor_NullItemId_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new DeleteOperation("test", null));
    }

    [Theory]
    [MemberData(nameof(BaseOperationTest.GetInvalidTableNames), MemberType = typeof(BaseOperationTest))]
    public void Ctor_InvalidTable_Throws(string tableName)
    {
        Assert.Throws<ArgumentException>(() => new DeleteOperation(tableName, "1234"));
    }

    [Theory]
    [MemberData(nameof(BaseOperationTest.GetInvalidIds), MemberType = typeof(BaseOperationTest))]
    public void Ctor_InvalidId_Throws(string itemId)
    {
        Assert.Throws<ArgumentException>(() => new DeleteOperation("test", itemId));
    }

    [Fact]
    public void Ctor_SetsContext()
    {
        var operation = new DeleteOperation("test", "1234");

        // Note - we check the int value for compatibility at the storage level.
        Assert.Equal(1, (int)operation.Kind);
        Assert.NotEmpty(operation.Id);
        Assert.Equal("1234", operation.ItemId);
        Assert.Equal("test", operation.TableName);
        Assert.Null(operation.Item);
        Assert.Equal(0, operation.Sequence);
        // Note - we check the int value for compatibility at the storage level.
        Assert.Equal(0, (int)operation.State);
        Assert.Equal(1, operation.Version);
        Assert.False(operation.CanWriteResultToStore);
        Assert.True(operation.SerializeItemToQueue);
    }

    [Fact]
    public void Equals_TableOperation_Works()
    {
        var sut = new DeleteOperation("test", "1234") { Item = testObject };
        var other = new DeleteOperation("test", "1234") { Item = testObject };
        var notother = new DeleteOperation("test", "1234");

        Assert.True(sut.Equals(other));
        Assert.False(sut.Equals(notother));
    }

    [Fact]
    public void Equals_Object_Works()
    {
        var sut = new DeleteOperation("test", "1234") { Item = testObject };
        var other = new DeleteOperation("test", "1234") { Item = testObject };
        var notother = new DeleteOperation("test", "1234");
        object notoperation = new();

        Assert.True(sut.Equals((object)other));
        Assert.False(sut.Equals((object)notother));
        Assert.False(sut.Equals(notoperation));
    }

    [Fact]
    public void GetHashCode_Works()
    {
        var sut = new DeleteOperation("test", "1234") { Item = testObject };
        var hash = "1234".GetHashCode();

        Assert.Equal(hash, sut.GetHashCode());
    }

    [Fact]
    public void Operation_CanAbort()
    {
        var operation = new DeleteOperation("test", "1234");
        Assert.Throws<PushAbortedException>(() => operation.AbortPush());
    }

    [Fact]
    public void Operation_CanBeCancelled()
    {
        var operation = new DeleteOperation("test", "1234");
        Assert.False(operation.IsCancelled);
        operation.Cancel();
        Assert.True(operation.IsCancelled);
    }

    [Fact]
    public void Operation_CanBeUpdated()
    {
        var operation = new DeleteOperation("test", "1234");
        Assert.False(operation.IsUpdated);
        operation.Update();
        Assert.Equal(2, operation.Version);
        Assert.True(operation.IsUpdated);
    }

    [Fact]
    public void Operation_CanSetItem()
    {
        var operation = new DeleteOperation("test", "1234") { Item = testObject };
        Assert.Equal(operation.Item, testObject);
    }

    [Fact]
    public void Serialize_Works_WithItem()
    {
        var operation = new DeleteOperation("test", "1234") { Item = testObject };

        JObject actual = operation.Serialize();
        actual.Remove("id");

        Assert.Equal(serializedObject, actual);
    }

    [Fact]
    public void Serialize_Works_WithoutItem()
    {
        var operation = new DeleteOperation("test", "1234");
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
        var expected = new DeleteOperation("test", "1234") { Item = testObject };
        var operation = TableOperation.Deserialize(serializedObject);
        Assert.IsAssignableFrom<DeleteOperation>(operation);
        Assert.Equal(expected, operation);
    }

    [Fact]
    public void Deserialize_Works_WithoutItem()
    {
        var expected = new DeleteOperation("test", "1234");
        serializedObject["item"] = null;    // We aren't expecting this.
        var operation = TableOperation.Deserialize(serializedObject);
        Assert.IsAssignableFrom<DeleteOperation>(operation);
        Assert.Equal(expected, operation);
    }

    [Theory, CombinatorialData]
    public void ValidateCollapse_Throws_OnMismatchedItemIds([CombinatorialRange(1, 3)] int kind)
    {
        var sut = new DeleteOperation("test", "1234");
        TableOperation newOperation = kind switch
        {
            1 => new DeleteOperation("test", "4321"),
            2 => new InsertOperation("test", "4321"),
            3 => new UpdateOperation("test", "4321"),
            _ => throw new NotImplementedException()
        };
        Assert.Throws<ArgumentException>(() => sut.ValidateOperationCanCollapse(newOperation));
    }

    [Theory, CombinatorialData]
    public void ValidateCollapse_Throws_OnMatchItems([CombinatorialRange(1, 3)] int kind)
    {
        var sut = new DeleteOperation("test", "1234");
        TableOperation newOperation = kind switch
        {
            1 => new DeleteOperation("test", "1234"),
            2 => new InsertOperation("test", "1234"),
            3 => new UpdateOperation("test", "1234"),
            _ => throw new NotImplementedException()
        };
        Assert.Throws<InvalidOperationException>(() => sut.ValidateOperationCanCollapse(newOperation));
    }

    [Theory, CombinatorialData]
    public void Collapse_DoesntCollapse([CombinatorialRange(1, 3)] int kind)
    {
        var sut = new DeleteOperation("test", "1234");
        TableOperation newOperation = kind switch
        {
            1 => new DeleteOperation("test", "1234"),
            2 => new InsertOperation("test", "1234"),
            3 => new UpdateOperation("test", "1234"),
            _ => throw new NotImplementedException()
        };
        sut.CollapseOperation(newOperation);
        Assert.False(sut.IsCancelled);
        Assert.Equal(1, sut.Version);
    }

    [Fact]
    public async Task ExecuteRemote_ReturnsNull_WhenCancelled()
    {
        var client = GetMockClient();
        MockHandler.AddResponse(HttpStatusCode.OK, new IdEntity { Id = "1234", StringValue = "foo" });

        var sut = new DeleteOperation("test", "1234") { Item = testObject };
        sut.Cancel();
        var actual = await sut.ExecuteOperationOnRemoteServiceAsync(client);
        Assert.Null(actual);
    }

    [Theory]
    [InlineData(HttpStatusCode.NoContent)]
    [InlineData(HttpStatusCode.NotFound)]
    [InlineData(HttpStatusCode.Gone)]
    public async Task ExecuteRemote_CallsRemoteServer_WithSuccess(HttpStatusCode statusCode)
    {
        var client = GetMockClient();
        MockHandler.AddResponse(statusCode);

        var sut = new DeleteOperation("test", "1234") { Item = testObject };
        var result = await sut.ExecuteOperationOnRemoteServiceAsync(client);

        Assert.Null(result);
        Assert.Single(MockHandler.Requests);
        Assert.Equal(HttpMethod.Delete, MockHandler.Requests[0].Method);
        Assert.Equal(new Uri(Endpoint, "/tables/test/1234"), MockHandler.Requests[0].RequestUri);
    }

    [Theory]
    [InlineData(HttpStatusCode.NoContent)]
    public async Task ExecuteRemote_ThrowsError_WithNoItem(HttpStatusCode statusCode)
    {
        var client = GetMockClient();
        MockHandler.AddResponse(statusCode);

        var sut = new DeleteOperation("test", "1234");
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

        var sut = new DeleteOperation("test", "1234") { Item = testObject };
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

    [Fact]
    public async Task ExecuteLocal_CallsLocalStore()
    {
        var store = Substitute.For<IOfflineStore>();
        store.DeleteAsync("test", Arg.Any<IEnumerable<string>>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);

        var sut = new DeleteOperation("test", "1234") { Item = testObject };
        await sut.ExecuteOperationOnOfflineStoreAsync(store, testObject);

        await store.Received(1).DeleteAsync("test", Arg.Is<IEnumerable<string>>(ids => ids.SingleOrDefault().Equals("1234")), default);
    }
}
