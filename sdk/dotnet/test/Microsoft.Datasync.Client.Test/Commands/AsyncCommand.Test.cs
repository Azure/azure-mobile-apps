// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Commands;
using System;
using System.Collections.Generic;
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
        private class IntAsyncCommand : AsyncCommand
        {
            public IntAsyncCommand(Func<Task> execute, Func<bool> canExecute = null, IAsyncExceptionHandler handler = null)
                : base(execute, canExecute, handler)
            {
            }

            public Func<Task> IntExecuteFunc { get => base.ExecuteFunc; }
            public Func<bool> IntCanExecuteFunc { get => base.CanExecuteFunc; }
            public IAsyncExceptionHandler IntExceptionHandler { get => base.ExceptionHandler; }
        }

        private class ExceptionHandler : IAsyncExceptionHandler
        {
            internal List<Exception> Received = new();

            public void OnAsyncException(Exception ex)
            {
                Received.Add(ex);
            }
        }

        private int executionCount = 0;

        private Task DoWorkAsync()
        {
            executionCount++;
            return Task.CompletedTask;
        }

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
            var handler = new ExceptionHandler();
            Func<Task> dowork = DoWorkAsync;
            Func<bool> canexecute = () => true;
            var command = new IntAsyncCommand(dowork, canexecute, handler);

            Assert.Same(dowork, command.IntExecuteFunc);
            Assert.Same(canexecute, command.IntCanExecuteFunc);
            Assert.Same(handler, command.IntExceptionHandler);
        }

        [Fact]
        [Trait("Method", "ExecuteAsync")]
        public async Task ExecuteAsync_Executes_NoCanExecute()
        {
            var command = new AsyncCommand(DoWorkAsync);

            await command.ExecuteAsync().ConfigureAwait(false);

            Assert.Equal(1, executionCount);
        }

        [Fact]
        [Trait("Method", "ExecuteAsync")]
        public async Task ExecuteAsync_Executes_CanExecute_True()
        {
            var command = new AsyncCommand(DoWorkAsync, () => true);

            await command.ExecuteAsync().ConfigureAwait(false);

            Assert.Equal(1, executionCount);
        }

        [Fact]
        [Trait("Method", "ExecuteAsync")]
        public async Task ExecuteAsync_NotExecute_CanExecute_False()
        {
            var command = new AsyncCommand(DoWorkAsync, () => false);

            await command.ExecuteAsync().ConfigureAwait(false);

            Assert.Equal(0, executionCount);
        }

        [Fact]
        [Trait("Method", "ExecuteAsync")]
        public async Task ExecuteAsync_CallsEventHandler()
        {
            int count = 0;
            var command = new AsyncCommand(DoWorkAsync, () => true);
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
            var command = new IntAsyncCommand(DoWorkAsync, CanExecuteFunc);

            Assert.True(command.CanExecute());
            Assert.Equal(1, count);
        }

        [Fact]
        [Trait("Method", "CanExecute")]
        public void ICommand_CanExecute_ReturnsValue()
        {
            int count = 0;
            bool CanExecuteFunc() { count++; return true; }
            var command = new IntAsyncCommand(DoWorkAsync, CanExecuteFunc);
            var icommand = command as ICommand;

            Assert.True(icommand.CanExecute(new object()));
            Assert.Equal(1, count);
        }

        [Fact]
        [Trait("Method", "Execute")]
        public void ICommand_Execute_CallsExceptionHandler()
        {
            var handler = new ExceptionHandler();
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
