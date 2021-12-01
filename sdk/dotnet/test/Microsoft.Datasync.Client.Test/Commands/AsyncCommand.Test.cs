// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Test;
using Datasync.Common.Test.Mocks;
using Microsoft.Datasync.Client.Commands;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Xunit;

namespace Microsoft.Datasync.Client.Test.Commands
{
    [ExcludeFromCodeCoverage]
    public class AsyncCommand_Tests : BaseTest
    {
        [Fact]
        [Trait("Method", "Ctor")]
        public void Ctor_NullExecute_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new AsyncCommand(null));
        }

        [Fact]
        [Trait("Method", "Ctor")]
        public void Ctor_Stores_Values()
        {
            var handler = new MockExceptionHandler();
            Func<Task> dowork = () => Task.CompletedTask;
            Func<bool> canexecute = () => true;
            var command = new WrappedAsyncCommand(dowork, canexecute, handler);

            Assert.Same(dowork, command.WrappedExecuteFunc);
            Assert.Same(canexecute, command.WrappedCanExecuteFunc);
            Assert.Same(handler, command.WrappedExceptionHandler);
        }

        [Fact]
        [Trait("Method", "ExecuteAsync")]
        public async Task ExecuteAsync_Executes_NoCanExecute()
        {
            var count = 0;
            var command = new AsyncCommand(() => { count++; return Task.CompletedTask; });

            await command.ExecuteAsync().ConfigureAwait(false);

            Assert.Equal(1, count);
        }

        [Fact]
        [Trait("Method", "ExecuteAsync")]
        public async Task ExecuteAsync_Executes_CanExecute_True()
        {
            var count = 0;
            var command = new AsyncCommand(() => { count++; return Task.CompletedTask; }, () => true);

            await command.ExecuteAsync().ConfigureAwait(false);

            Assert.Equal(1, count);
        }

        [Fact]
        [Trait("Method", "ExecuteAsync")]
        public async Task ExecuteAsync_NotExecute_CanExecute_False()
        {
            var count = 0;
            var command = new AsyncCommand(() => { count++; return Task.CompletedTask; }, () => false);

            await command.ExecuteAsync().ConfigureAwait(false);

            Assert.Equal(0, count);
        }

        [Fact]
        [Trait("Method", "ExecuteAsync")]
        public async Task ExecuteAsync_CallsEventHandler()
        {
            int count = 0;
            var command = new AsyncCommand(() => { count++; return Task.CompletedTask; }, () => true);
            command.CanExecuteChanged += (_, _) => count++;

            await command.ExecuteAsync().ConfigureAwait(false);

            Assert.Equal(1, count);
        }

        [Fact]
        [Trait("Method", "CanExecute")]
        public void CanExecute_ReturnsValue()
        {
            int count = 0;
            bool CanExecuteFunc() { count++; return true; }

            var command = new WrappedAsyncCommand(() => { count++; return Task.CompletedTask; }, CanExecuteFunc);

            Assert.True(command.CanExecute());
            Assert.Equal(1, count);
        }

        [Fact]
        [Trait("Method", "CanExecute")]
        public void ICommand_CanExecute_ReturnsValue()
        {
            int count = 0;
            bool CanExecuteFunc() { count++; return true; }

            var command = new WrappedAsyncCommand(() => { count++; return Task.CompletedTask; }, CanExecuteFunc);
            var icommand = command as ICommand;

            Assert.True(icommand.CanExecute(new object()));
            Assert.Equal(1, count);
        }

        [Fact]
        [Trait("Method", "Execute")]
        public void ICommand_Execute_CallsExceptionHandler()
        {
            var handler = new MockExceptionHandler();
            var command = new AsyncCommand(() => throw new NotSupportedException(), () => true, handler);

            var icommand = command as ICommand;

            icommand.Execute(new object());
            Thread.Sleep(2500); // Because async is hard

            Assert.Single(handler.Received);
            Assert.IsAssignableFrom<NotSupportedException>(handler.Received[0]);
        }

        [Fact]
        [Trait("Method", "Execute")]
        public void ICommand_Execute_DoubleExecuteIgnored()
        {
            int count = 0;
            // This is a timing problem and may not always work
            Task SleepAsync() => Task.Run(() => { Thread.Sleep(1000);  count++; Thread.Sleep(500); });

            var command = new AsyncCommand(SleepAsync);
            var icommand = command as ICommand;

            icommand.Execute(new object());
            icommand.Execute(new object());
            Thread.Sleep(5000);

            Assert.Equal(1, count);
                        // May be 2 as well, if threading misbehaves
                        // Will never be anything other than 1 or 2.
        }

        [Fact]
        [Trait("Method", "Execute")]
        public void ICommand_Execute_DoubleExecuteIgnored_WithCanExecute()
        {
            int count = 0;
            // This is a timing problem and may not always work
            Task SleepAsync() => Task.Run(() => { Thread.Sleep(1000); count++; Thread.Sleep(500); });

            var command = new AsyncCommand(SleepAsync, () => true);
            var icommand = command as ICommand;

            icommand.Execute(new object());
            icommand.Execute(new object());
            Thread.Sleep(5000);

            Assert.Equal(1, count);
            // May be 2 as well, if threading misbehaves
            // Will never be anything other than 1 or 2.
        }
    }
}
