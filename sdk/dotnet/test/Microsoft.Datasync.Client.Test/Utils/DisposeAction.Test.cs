// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Utils;

namespace Microsoft.Datasync.Client.Test.Utils;

[ExcludeFromCodeCoverage]
public class DisposeAction_Tests
{
    [Fact]
    public void DisposeAction_CallsAction_OnDispose()
    {
        var isCalled = false;
        using (var sut = new DisposeAction(() => isCalled = true))
        {
            Assert.False(isCalled);
        }
        Assert.True(isCalled);
    }

    [Fact]
    public void DisposeAction_CanDisposeTwice()
    {
        int isCalled = 0;
        using (var sut = new DisposeAction(() => isCalled++))
        {
            Assert.Equal(0, isCalled);
            sut.Dispose();
            Assert.Equal(1, isCalled);
        }
        Assert.Equal(1, isCalled);
    }

    [Fact]
    public void AsyncLock_CanDispose()
    {
        var sut = new AsyncLock();
        sut.Dispose(); // should not throw
    }
}
