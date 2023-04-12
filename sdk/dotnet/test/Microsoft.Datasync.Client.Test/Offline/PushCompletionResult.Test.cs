// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Test;
using Datasync.Common.Test.Mocks;
using Microsoft.Datasync.Client.Offline;
using Microsoft.Datasync.Client.Offline.Queue;
using Newtonsoft.Json.Linq;

namespace Microsoft.Datasync.Client.Test.Offline;

[ExcludeFromCodeCoverage]
public class PushCompletionResult_Tests : BaseTest
{
    [Fact]
    public void Ctor_NullItems_CreatesEmptyList()
    {
        var sut = new PushCompletionResult(null, PushStatus.InternalError);
        Assert.Empty(sut.Errors);
        Assert.Equal(PushStatus.InternalError, sut.Status);
    }

    [Fact]
    public void Ctor_LoadsErrors_FromList()
    {
        var errors = new List<TableOperationError>();
        var context = new SyncContext(GetMockClient(), new MockOfflineStore());
        for (int i = 0; i < 5; i++)
        {
            var op = new InsertOperation("movies", Guid.NewGuid().ToString());
            var error = new TableOperationError(op, context, null, null, JObject.Parse("{}"));
            errors.Add(error);
        }

        var sut = new PushCompletionResult(errors.ToArray(), PushStatus.Complete);

        Assert.Equal(5, sut.Errors.Count);
        Assert.Equal(PushStatus.Complete, sut.Status);
    }
}
