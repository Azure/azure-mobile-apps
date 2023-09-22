// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Offline;
using Microsoft.Datasync.Client.Offline.Queue;
using System.Collections.Concurrent;

namespace Microsoft.Datasync.Client.Test.Offline;

[ExcludeFromCodeCoverage]
public class QueueHandler_Tests
{
    [Theory(Timeout = 30000)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(4)]
    [InlineData(8)]
    public async Task QueueHandler_WithThreads(int nThreads)
    {
        ConcurrentQueue<string> accId = new();
        ConcurrentQueue<int> accTh = new();
        QueueHandler sut = new(nThreads, (operation) =>
        {
            accId.Enqueue(operation.Id);
            accTh.Enqueue(Environment.CurrentManagedThreadId);
            Thread.Sleep(1000);
            return Task.CompletedTask;
        });
        DateTimeOffset startTime = DateTimeOffset.Now;

        // Add in some elements
        int nElements = nThreads * 5;
        for (int i = 0; i < nElements; i++)
        {
            TableOperation op = new DeleteOperation("table", "abc123");
            sut.Enqueue(op);
        }

        // Now wait for completion
        await sut.WhenComplete();
        DateTimeOffset endTime = DateTimeOffset.Now;

        // We should have 10 items in accId
        Assert.Equal(nElements, accId.Count);

        // We should have nThreads unique Ids in accTh
        Assert.Equal(nElements, accTh.Count);
        Dictionary<int, int> pairs = new();
        foreach (int threadId in accTh)
        {
            if (pairs.ContainsKey(threadId))
            {
                pairs[threadId]++;
            }
            else
            {
                pairs[threadId] = 1;
            }
        }
        Assert.Equal(nThreads, pairs.Count);

        // Roughly speaking, the test should take nElements / 2 seconds
        double nSeconds = (endTime - startTime).TotalSeconds;
        double eSeconds = (nElements / nThreads) + 2;
        Assert.True(nSeconds < eSeconds);
    }
}
