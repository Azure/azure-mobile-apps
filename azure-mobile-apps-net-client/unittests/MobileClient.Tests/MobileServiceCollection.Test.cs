// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using Microsoft.WindowsAzure.MobileServices;
using MobileClient.Tests.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace MobileClient.Tests
{
    public class MobileServiceCollection_Test
    {
        [Fact]
        public async Task MobileServiceCollectionMultipleLoadItemsAsyncShouldThrow()
        {
            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                MobileServiceTableQueryMock<Book> query = new MobileServiceTableQueryMock<Book>();
                MobileServiceCollection<Book> collection = new MobileServiceCollection<Book>(query);
                CancellationTokenSource tokenSource = new CancellationTokenSource();
                await Task.WhenAll(collection.LoadMoreItemsAsync(tokenSource.Token), collection.LoadMoreItemsAsync(tokenSource.Token));
            });
        }

        [Fact]
        public void MobileServiceCollectionItemsCanBeAddedAndNotifies()
        {
            // Get the Books table
            MobileServiceTableQueryMock<Book> query = new MobileServiceTableQueryMock<Book>();
            query.EnumerableAsyncThrowsException = true;

            MobileServiceCollection<Book, Book> collection = new MobileServiceCollection<Book, Book>(query);

            List<string> properties = new List<string>();
            List<string> expectedProperties = new List<string>() { "Count", "Item[]" };
            List<NotifyCollectionChangedAction> actions = new List<NotifyCollectionChangedAction>();
            List<NotifyCollectionChangedAction> expectedActions = new List<NotifyCollectionChangedAction>() { NotifyCollectionChangedAction.Add };

            Book book = new Book();

            ((INotifyPropertyChanged)collection).PropertyChanged += (s, e) => properties.Add(e.PropertyName);
            collection.CollectionChanged += (s, e) => actions.Add(e.Action);
            collection.Add(book);

            Assert.Single(collection);
            Assert.Equal(book, collection[0]);
            Assert.Equal(expectedProperties, properties);
            Assert.Equal(expectedActions, actions);
        }

        [Fact]
        public void MobileServiceCollectionCanClearAndNotifies()
        {
            // Get the Books table
            MobileServiceTableQueryMock<Book> query = new MobileServiceTableQueryMock<Book>();
            query.EnumerableAsyncThrowsException = true;

            MobileServiceCollection<Book> collection = new MobileServiceCollection<Book>(query);

            List<string> properties = new List<string>();
            List<string> expectedProperties = new List<string>() { "Count", "Item[]" };
            List<NotifyCollectionChangedAction> actions = new List<NotifyCollectionChangedAction>();
            List<NotifyCollectionChangedAction> expectedActions = new List<NotifyCollectionChangedAction>() { NotifyCollectionChangedAction.Reset };

            Book book = new Book();
            collection.Add(book);

            ((INotifyPropertyChanged)collection).PropertyChanged += (s, e) => properties.Add(e.PropertyName);
            collection.CollectionChanged += (s, e) => actions.Add(e.Action);
            collection.Clear();

            Assert.Empty(collection);
            Assert.Equal(expectedProperties, properties);
            Assert.Equal(expectedActions, actions);
        }

        [Fact]
        public void MobileServiceCollectionCanContainsAndNotNotifies()
        {
            // Get the Books table
            MobileServiceTableQueryMock<Book> query = new MobileServiceTableQueryMock<Book>();
            query.EnumerableAsyncThrowsException = true;

            MobileServiceCollection<Book, Book> collection = new MobileServiceCollection<Book, Book>(query);

            List<string> properties = new List<string>();
            List<string> expectedProperties = new List<string>() { };
            List<NotifyCollectionChangedAction> actions = new List<NotifyCollectionChangedAction>();
            List<NotifyCollectionChangedAction> expectedActions = new List<NotifyCollectionChangedAction>() { };

            Book book = new Book();
            collection.Add(book);

            ((INotifyPropertyChanged)collection).PropertyChanged += (s, e) => properties.Add(e.PropertyName);
            collection.CollectionChanged += (s, e) => actions.Add(e.Action);

            Assert.Contains(book, collection);
            Assert.Equal(expectedProperties, properties);
            Assert.Equal(expectedActions, actions);
        }

        [Fact]
        public void MobileServiceCollectionCanCopyToAndNotNotifies()
        {
            // Get the Books table
            MobileServiceTableQueryMock<Book> query = new MobileServiceTableQueryMock<Book>();
            query.EnumerableAsyncThrowsException = true;

            MobileServiceCollection<Book> collection = new MobileServiceCollection<Book>(query);

            List<string> properties = new List<string>();
            List<string> expectedProperties = new List<string>() { };
            List<NotifyCollectionChangedAction> actions = new List<NotifyCollectionChangedAction>();
            List<NotifyCollectionChangedAction> expectedActions = new List<NotifyCollectionChangedAction>() { };

            Book book = new Book();
            collection.Add(book);
            Book[] books = new Book[1];

            ((INotifyPropertyChanged)collection).PropertyChanged += (s, e) => properties.Add(e.PropertyName);
            collection.CollectionChanged += (s, e) => actions.Add(e.Action);
            collection.CopyTo(books, 0);

            Assert.Single(collection);
            Assert.Single(books);
            Assert.Equal(collection[0], books[0]);
            Assert.Equal(expectedProperties, properties);
            Assert.Equal(expectedActions, actions);
        }

        [Fact]
        public void MobileServiceCollectionCanIndexOfAndNotNotifies()
        {
            // Get the Books table
            MobileServiceTableQueryMock<Book> query = new MobileServiceTableQueryMock<Book>();
            query.EnumerableAsyncThrowsException = true;

            MobileServiceCollection<Book, Book> collection = new MobileServiceCollection<Book, Book>(query);

            List<string> properties = new List<string>();
            List<string> expectedProperties = new List<string>() { };
            List<NotifyCollectionChangedAction> actions = new List<NotifyCollectionChangedAction>();
            List<NotifyCollectionChangedAction> expectedActions = new List<NotifyCollectionChangedAction>() { };

            Book book = new Book();
            collection.Add(book);

            ((INotifyPropertyChanged)collection).PropertyChanged += (s, e) => properties.Add(e.PropertyName);
            collection.CollectionChanged += (s, e) => actions.Add(e.Action);

            Assert.Single(collection);
            Assert.Equal(0, collection.IndexOf(book));
            Assert.Equal(expectedProperties, properties);
            Assert.Equal(expectedActions, actions);
        }

        [Fact]
        public void MobileServiceCollectionCanInsertAndNotifies()
        {
            // Get the Books table
            MobileServiceTableQueryMock<Book> query = new MobileServiceTableQueryMock<Book>();
            query.EnumerableAsyncThrowsException = true;

            MobileServiceCollection<Book> collection = new MobileServiceCollection<Book>(query);

            List<string> properties = new List<string>();
            List<string> expectedProperties = new List<string>() { "Count", "Item[]" };
            List<NotifyCollectionChangedAction> actions = new List<NotifyCollectionChangedAction>();
            List<NotifyCollectionChangedAction> expectedActions = new List<NotifyCollectionChangedAction>() { NotifyCollectionChangedAction.Add };

            Book book = new Book();

            ((INotifyPropertyChanged)collection).PropertyChanged += (s, e) => properties.Add(e.PropertyName);
            collection.CollectionChanged += (s, e) => actions.Add(e.Action);
            collection.Insert(0, book);

            Assert.Single(collection);
            Assert.Equal(book, collection[0]);
            Assert.Equal(expectedProperties, properties);
            Assert.Equal(expectedActions, actions);
        }

        [Fact]
        public void MobileServiceCollectionCanRemoveAndNotifies()
        {
            // Get the Books table
            MobileServiceTableQueryMock<Book> query = new MobileServiceTableQueryMock<Book>();
            query.EnumerableAsyncThrowsException = true;

            MobileServiceCollection<Book, Book> collection = new MobileServiceCollection<Book, Book>(query);

            List<string> properties = new List<string>();
            List<string> expectedProperties = new List<string>() { "Count", "Item[]" };
            List<NotifyCollectionChangedAction> actions = new List<NotifyCollectionChangedAction>();
            List<NotifyCollectionChangedAction> expectedActions = new List<NotifyCollectionChangedAction>() { NotifyCollectionChangedAction.Remove };

            Book book = new Book();
            collection.Add(book);

            ((INotifyPropertyChanged)collection).PropertyChanged += (s, e) => properties.Add(e.PropertyName);
            collection.CollectionChanged += (s, e) => actions.Add(e.Action);
            collection.Remove(book);

            Assert.Empty(collection);
            Assert.Equal(expectedProperties, properties);
            Assert.Equal(expectedActions, actions);
        }

        [Fact]
        public void MobileServiceCollectionCanRemoveAtAndNotifies()
        {
            // Get the Books table
            MobileServiceTableQueryMock<Book> query = new MobileServiceTableQueryMock<Book>();
            query.EnumerableAsyncThrowsException = true;

            MobileServiceCollection<Book> collection = new MobileServiceCollection<Book>(query);

            List<string> properties = new List<string>();
            List<string> expectedProperties = new List<string>() { "Count", "Item[]" };
            List<NotifyCollectionChangedAction> actions = new List<NotifyCollectionChangedAction>();
            List<NotifyCollectionChangedAction> expectedActions = new List<NotifyCollectionChangedAction>() { NotifyCollectionChangedAction.Remove };

            Book book = new Book();
            collection.Add(book);

            ((INotifyPropertyChanged)collection).PropertyChanged += (s, e) => properties.Add(e.PropertyName);
            collection.CollectionChanged += (s, e) => actions.Add(e.Action);
            collection.RemoveAt(0);

            Assert.Empty(collection);
            Assert.Equal(expectedProperties, properties);
            Assert.Equal(expectedActions, actions);
        }

        [Fact]
        public void MobileServiceCollectionCanReplaceAndNotifies()
        {
            // Get the Books table
            MobileServiceTableQueryMock<Book> query = new MobileServiceTableQueryMock<Book>();
            query.EnumerableAsyncThrowsException = true;

            MobileServiceCollection<Book, Book> collection = new MobileServiceCollection<Book, Book>(query);

            List<string> properties = new List<string>();
            List<string> expectedProperties = new List<string>() { "Item[]" };
            List<NotifyCollectionChangedAction> actions = new List<NotifyCollectionChangedAction>();
            List<NotifyCollectionChangedAction> expectedActions = new List<NotifyCollectionChangedAction>() { NotifyCollectionChangedAction.Replace };

            Book book = new Book();
            Book book2 = new Book();
            collection.Add(book);

            ((INotifyPropertyChanged)collection).PropertyChanged += (s, e) => properties.Add(e.PropertyName);
            collection.CollectionChanged += (s, e) => actions.Add(e.Action);
            collection[0] = book2;

            Assert.Single(collection);
            Assert.Equal(book2, collection[0]);
            Assert.Equal(expectedProperties, properties);
            Assert.Equal(expectedActions, actions);
        }

        [Fact]
        public void MobileServiceCollectionHasMoreItemsInitiallyTrue()
        {
            // Get the Books table
            MobileServiceTableQueryMock<Book> query = new MobileServiceTableQueryMock<Book>();
            query.EnumerableAsyncThrowsException = true;

            MobileServiceCollection<Book> collection = new MobileServiceCollection<Book>(query);

            Assert.True(collection.HasMoreItems);
        }

        [Fact]
        public async Task MobileServiceCollectionTotalCountSet()
        {
            // Get the Books table
            MobileServiceTableQueryMock<Book> query = new MobileServiceTableQueryMock<Book>();

            List<string> properties = new List<string>();
            List<string> expectedProperties = new List<string>() { "HasMoreItems", "Count", "Item[]", "Count", "Item[]", "TotalCount" };
            List<NotifyCollectionChangedAction> actions = new List<NotifyCollectionChangedAction>();
            List<NotifyCollectionChangedAction> expectedActions = new List<NotifyCollectionChangedAction>() { NotifyCollectionChangedAction.Add, NotifyCollectionChangedAction.Add };

            MobileServiceCollection<Book, Book> collection = new MobileServiceCollection<Book, Book>(query);
            CancellationTokenSource tokenSource = new CancellationTokenSource();

            ((INotifyPropertyChanged)collection).PropertyChanged += (s, e) => properties.Add(e.PropertyName);
            collection.CollectionChanged += (s, e) => actions.Add(e.Action);
            int result = await collection.LoadMoreItemsAsync(tokenSource.Token);

            Assert.Equal(2, collection.TotalCount);
            Assert.Equal(expectedProperties, properties);
            Assert.Equal(expectedActions, actions);
        }

        [Fact]
        public async Task MobileServiceCollectionHasMoreItemsShouldBeFalseAfterRetrievingDataWhenNoPaging()
        {
            // Get the Books table
            MobileServiceTableQueryMock<Book> query = new MobileServiceTableQueryMock<Book>();

            MobileServiceCollection<Book> collection = new MobileServiceCollection<Book>(query);
            CancellationTokenSource tokenSource = new CancellationTokenSource();

            List<string> properties = new List<string>();
            List<string> expectedProperties = new List<string>() { "HasMoreItems", "Count", "Item[]", "Count", "Item[]", "TotalCount" };
            List<NotifyCollectionChangedAction> actions = new List<NotifyCollectionChangedAction>();
            List<NotifyCollectionChangedAction> expectedActions = new List<NotifyCollectionChangedAction>() { NotifyCollectionChangedAction.Add, NotifyCollectionChangedAction.Add };

            ((INotifyPropertyChanged)collection).PropertyChanged += (s, e) => properties.Add(e.PropertyName);
            collection.CollectionChanged += (s, e) => actions.Add(e.Action);
            int result = await collection.LoadMoreItemsAsync(tokenSource.Token);

            Assert.False(collection.HasMoreItems);
            Assert.Equal(expectedProperties, properties);
            Assert.Equal(expectedActions, actions);
        }

        [Fact]
        public async Task MobileServiceCollectionLoadMoreItemsAsyncFiresLoadingItemsEventBeforeReadingData()
        {
            // Get the Books table
            MobileServiceTableQueryMock<Book> query = new MobileServiceTableQueryMock<Book>();

            MobileServiceCollection<Book, Book> collection = new MobileServiceCollection<Book, Book>(query);
            CancellationTokenSource tokenSource = new CancellationTokenSource();

            bool firedLoading = false;

            collection.LoadingItems += (s, e) => firedLoading = true;
            _ = await collection.LoadMoreItemsAsync(tokenSource.Token);
            Assert.True(firedLoading);
        }

        [Fact]
        public async Task MobileServiceCollectionLoadMoreItemsAsyncFiresLoadingCompleteEventAfterReadingData()
        {
            // Get the Books table
            MobileServiceTableQueryMock<Book> query = new MobileServiceTableQueryMock<Book>();

            MobileServiceCollection<Book, Book> collection = new MobileServiceCollection<Book, Book>(query);
            CancellationTokenSource tokenSource = new CancellationTokenSource();

            int loadedEventArgsManyItemsLoaded = 0;
            collection.LoadingComplete += (s, e) => loadedEventArgsManyItemsLoaded = e.TotalItemsLoaded;
            _ = await collection.LoadMoreItemsAsync(tokenSource.Token);
            Assert.Equal(2, loadedEventArgsManyItemsLoaded);
        }
    }
}
