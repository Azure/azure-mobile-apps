// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Test;
using Datasync.Common.Test.Mocks;
using Datasync.Common.Test.Models;
using Microsoft.Datasync.Client.Commands;
using Microsoft.Datasync.Client.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Xunit;

using TestUtils = Datasync.Common.Test.Utils;

namespace Microsoft.Datasync.Client.Test.Utils
{
    [ExcludeFromCodeCoverage(Justification = "Test suite")]
    public class LazyObservableCollection_Tests : BaseTest
    {
        [Fact]
        [Trait("Method", "Ctor")]
        public void Ctor_NullEnumerable_Throws()
        {
            IAsyncEnumerable<ClientMovie> enumerable = null;
            Assert.Throws<ArgumentNullException>(() => new InternalLazyObservableCollection<ClientMovie>(enumerable));
        }

        [Fact]
        [Trait("Method", "Ctor")]
        public async Task Ctor_Creates_Structure()
        {
            IAsyncEnumerable<int> enumerable = TestUtils.RangeAsync(1, 100);
            var sut = new InternalLazyObservableCollection<int>(enumerable);

            // There is a LoadMoreCommand and it's an AsyncCommand
            Assert.NotNull(sut.LoadMoreCommand);
            Assert.IsAssignableFrom<IAsyncCommand>(sut.LoadMoreCommand);

            Assert.True(await WaitUntil(() => !sut.IsBusy).ConfigureAwait(false), "Timeout waiting for IsBusy to settle");

            // Basic properties are set
            Assert.False(sut.IsBusy);
            Assert.True(sut.HasMoreItems);

            // First page is loaded
            Assert.Equal(20, sut.Count);
        }

        [Fact]
        [Trait("Method", "LoadMoreCommand")]
        public async Task LoadMore_Works()
        {
            IAsyncEnumerable<int> enumerable = TestUtils.RangeAsync(1, 100);
            var sut = new InternalLazyObservableCollection<int>(enumerable);
            var loadMore = sut.LoadMoreCommand as IAsyncCommand;

            // Wait a little bit for the first page to be loaded.
            await Task.Delay(250).ConfigureAwait(false);
            Assert.True(await WaitUntil(() => !sut.IsBusy).ConfigureAwait(false), "Timeout waiting for IsBusy to settle");

            int loop = 1;
            while (sut.HasMoreItems && loop != 5)
            {
                loop++;

                await loadMore.ExecuteAsync().ConfigureAwait(false);
                Assert.Equal(loop * 20, sut.Count);
                Assert.False(sut.IsBusy);
            }

            await loadMore.ExecuteAsync().ConfigureAwait(false);
            Assert.False(sut.HasMoreItems);
        }

        [Fact]
        [Trait("Method", "LoadMoreCommand")]
        public async Task LoadMore_Works_WithPageSize()
        {
            IAsyncEnumerable<int> enumerable = TestUtils.RangeAsync(1, 100);
            var sut = new InternalLazyObservableCollection<int>(enumerable, 25);
            var loadMore = sut.LoadMoreCommand as IAsyncCommand;

            // Wait a little bit for the first page to be loaded.
            Assert.True(await WaitUntil(() => !sut.IsBusy).ConfigureAwait(false), "Timeout waiting for IsBusy to settle");

            int loop = 1;
            while (sut.HasMoreItems && loop != 4)
            {
                loop++;

                await loadMore.ExecuteAsync().ConfigureAwait(false);
                Assert.Equal(loop * 25, sut.Count);
                Assert.False(sut.IsBusy);
            }

            await loadMore.ExecuteAsync().ConfigureAwait(false);
            Assert.False(sut.HasMoreItems);
        }

        [Fact]
        [Trait("Method", "Ctor")]
        public async Task Ctor_CallsErrorHandler_OnError()
        {
            IAsyncEnumerable<int> enumerable = TestUtils.ThrowAsync();
            var handler = new MockExceptionHandler();
            var sut = new InternalLazyObservableCollection<int>(enumerable, handler);
            await WaitUntil(() => !sut.IsBusy).ConfigureAwait(false);

            Assert.Single(handler.Received);
        }
    }
}
