// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Test;
using Datasync.Common.Test.Mocks;
using Microsoft.Datasync.Client.Commands;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Datasync.Client.Test.Commands
{
    [ExcludeFromCodeCoverage]
    public class TaskExtensions_Test : BaseTest
    {
        [Fact]
        [Trait("Method", "FireAndForgetSafeAsync")]
        public async Task FireAndForget_RunsTask_NoTestExceptionHandler()
        {
            int count = 0;
            Task.Run(() => count++).FireAndForgetSafeAsync();

            // Wait with a timeout
            Assert.True(await WaitUntil(() => count == 1).ConfigureAwait(false), "Timeout waiting for IsBusy to settle");

            Assert.Equal(1, count);
        }

        [Fact]
        [Trait("Method", "FireAndForgetSafeAsync")]
        public async Task FireAndForget_RunsTask_WithTestExceptionHandler()
        {
            int count = 0;
            var TestExceptionHandler = new MockExceptionHandler();

            Task.Run(() => count++).FireAndForgetSafeAsync(TestExceptionHandler);

            // Wait with a timeout
            Assert.True(await WaitUntil(() => count == 1).ConfigureAwait(false), "Timeout waiting for IsBusy to settle");

            Assert.Equal(1, count);
            Assert.Empty(TestExceptionHandler.Received);
        }

        [Fact]
        [Trait("Method", "FireAndForgetSafeAsync")]
        public async Task FireAndForget_CallsTestExceptionHandler()
        {
            var TestExceptionHandler = new MockExceptionHandler();

            Task.Run(() => throw new NotSupportedException()).FireAndForgetSafeAsync(TestExceptionHandler);

            // Sleep some time because async is hard
            Assert.True(await WaitUntil(() => TestExceptionHandler.Received.Count > 0, 1000).ConfigureAwait(false), "Timeout waiting for error hanlder to be called");

            Assert.Single(TestExceptionHandler.Received);
            Assert.IsAssignableFrom<NotSupportedException>(TestExceptionHandler.Received[0]);
        }

        [Fact]
        [Trait("Method", "FireAndForgetSafeAsync")]
        public void FireAndForget_CanThrowWithoutError()
        {
            Task.Run(() => throw new NotSupportedException()).FireAndForgetSafeAsync();

            // Sleep some time because async is hard
            Thread.Sleep(2500);

            // If anything breaks, it will be in the thread.  The exception should be swallowed.
        }
    }
}
