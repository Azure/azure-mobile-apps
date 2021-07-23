// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Commands;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Datasync.Client.Test.Commands
{
    [ExcludeFromCodeCoverage]
    public class TaskExtensions_Test : BaseTest
    {
        private class ErrorHandler : IAsyncExceptionHandler
        {
            public List<Exception> Received = new();

            public void OnAsyncException(Exception ex)
            {
                Received.Add(ex);
            }
        }

        [Fact]
        [Trait("Method", "FireAndForgetSafeAsync")]
        public async Task FireAndForget_RunsTask_NoErrorHandler()
        {
            int count = 0;
            Task.Run(() => count++).FireAndForgetSafeAsync();

            // Wait with a timeout
            Assert.True(await WaitUntil(() => count == 1).ConfigureAwait(false), "Timeout waiting for IsBusy to settle");


            Assert.Equal(1, count);
        }

        [Fact]
        [Trait("Method", "FireAndForgetSafeAsync")]
        public async Task FireAndForget_RunsTask_WithErrorHandler()
        {
            int count = 0;
            var errorHandler = new ErrorHandler();

            Task.Run(() => count++).FireAndForgetSafeAsync(errorHandler);

            // Wait with a timeout
            Assert.True(await WaitUntil(() => count == 1).ConfigureAwait(false), "Timeout waiting for IsBusy to settle");

            Assert.Equal(1, count);
            Assert.Empty(errorHandler.Received);
        }

        [Fact]
        [Trait("Method", "FireAndForgetSafeAsync")]
        public void FireAndForget_CallsErrorHandler()
        {
            var errorHandler = new ErrorHandler();

            Task.Run(() => throw new NotSupportedException()).FireAndForgetSafeAsync(errorHandler);

            // Sleep some time because async is hard
            Thread.Sleep(2500);

            Assert.Single(errorHandler.Received);
            Assert.IsAssignableFrom<NotSupportedException>(errorHandler.Received[0]);
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
