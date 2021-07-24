// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Commands;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Microsoft.Datasync.Client
{
    /// <summary>
    /// The external definition of the <see cref="InternalLazyObservableCollection{T}"/> that
    /// does lazy loading of a collection to conform to the standard provided by the Xamarin
    /// ListView (as an example).
    /// </summary>
    /// <typeparam name="T">The type of entity being loaded</typeparam>
    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "This is a deliberate choice for API concerns")]
    public interface LazyObservableCollection<T> : ICollection<T>, INotifyPropertyChanged
    {
        /// <summary>
        /// A <see cref="ICommand"/> that can be triggered to load more items into the collection
        /// </summary>
        ICommand LoadMoreCommand { get; }

        /// <summary>
        /// An observable boolean value that is true when network communication is happening.
        /// </summary>
        bool IsBusy { get; }
    }
}

namespace Microsoft.Datasync.Client.Utils
{
    /// <summary>
    /// The internal implementation of the <see cref="LazyObservableCollection{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of the entity being loaded</typeparam>
    internal class InternalLazyObservableCollection<T> : ObservableCollection<T>, LazyObservableCollection<T>
    {
        private bool _isBusy, _hasMoreItems = true;
        private readonly IAsyncEnumerable<T> _enumerable;
        private readonly IAsyncEnumerator<T> _enumerator;
        private readonly int _count;
        private readonly object busyLock = new();


        /// <summary>
        /// The default number of items to load each time <see cref="LoadMoreCommand"/> is executed.
        /// </summary>
        private const int DefaultPageCount = 20;

        /// <summary>
        /// Creates a new <see cref="LazyObservableCollection{T}"/> object based on the provided
        /// <see cref="IAsyncEnumerable{T}"/>
        /// </summary>
        /// <param name="enumerable">The <see cref="IAsyncEnumerable{T}"/> to use to get items</param>
        /// <param name="pageCount">The number of items to load each time</param>
        /// <param name="handler">An error handler for async exceptions</param>
        internal InternalLazyObservableCollection(IAsyncEnumerable<T> enumerable, int pageCount = DefaultPageCount, IAsyncExceptionHandler handler = null)
        {
            Validate.IsNotNull(enumerable, nameof(enumerable));

            _enumerable = enumerable;
            _enumerator = _enumerable.GetAsyncEnumerator();
            _count = pageCount;

            LoadMoreCommand = new AsyncCommand(LoadMoreItemsAsync, () => HasMoreItems, handler);
            LoadMoreItemsAsync().FireAndForgetSafeAsync(handler); // Load the first batch of items
        }

        /// <summary>
        /// Creates a new <see cref="LazyObservableCollection{T}"/> object based on the provided
        /// <see cref="IAsyncEnumerable{T}"/>
        /// </summary>
        /// <param name="enumerable">The <see cref="IAsyncEnumerable{T}"/> to use to get items</param>
        /// <param name="handler">An error handler for async exceptions</param>
        internal InternalLazyObservableCollection(IAsyncEnumerable<T> enumerable, IAsyncExceptionHandler handler)
            : this(enumerable, DefaultPageCount, handler)
        {
        }

        /// <summary>
        /// A <see cref="ICommand"/> that can be triggered to load more items into the collection
        /// </summary>
        public ICommand LoadMoreCommand { get; }

        /// <summary>
        /// An observable boolean value that is true when network communication is happening.
        /// </summary>
        public bool IsBusy
        {
            get => _isBusy;
            private set => SetField(ref _isBusy, value, nameof(IsBusy));
        }

        /// <summary>
        /// An observable boolean value that is true if there are more items to retrieve
        /// </summary>
        public bool HasMoreItems
        {
            get => _hasMoreItems;
            private set => SetField(ref _hasMoreItems, value, nameof(HasMoreItems));
        }

        /// <summary>
        /// Sets an observable property
        /// </summary>
        /// <typeparam name="P">The property type</typeparam>
        /// <param name="field">A reference to the backing field</param>
        /// <param name="value">The new value</param>
        /// <param name="propertyName">The name of the property</param>
        /// <returns>True if the field is set</returns>
        private bool SetField<P>(ref P field, P value, string propertyName)
        {
            field = value;
            base.OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
            return true;
        }

        /// <summary>
        /// Async task to load more items from the <see cref="IAsyncEnumerable{T}"/>
        /// object.
        /// </summary>
        /// <returns>A task that completes when the items are loaded.</returns>
        private async Task LoadMoreItemsAsync()
        {
            lock (busyLock)
            {
                if (IsBusy)
                    return;
                IsBusy = true;
            }

            for (var i = 0; i < _count; i++)
            {
                if (await _enumerator.MoveNextAsync().ConfigureAwait(false))
                {
                    Add(_enumerator.Current);
                }
                else
                {
                    HasMoreItems = false;
                    break;
                }
            }

            lock (busyLock)
            {
                IsBusy = false;
            }
        }
    }
}
