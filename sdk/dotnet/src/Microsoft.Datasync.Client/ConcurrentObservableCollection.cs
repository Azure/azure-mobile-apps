// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading;

namespace Microsoft.Datasync.Client
{
    /// <summary>
    /// A thread-safe implementation of the <see cref="ObservableCollection{T}"/> that allows us
    /// to add/replace/remove ranges without notifying more than once.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ConcurrentObservableCollection<T> : ObservableCollection<T>
    {
        private readonly SynchronizationContext context = SynchronizationContext.Current;
        private bool suppressNotification = false;

        /// <summary>
        /// Creates a new (empty) collection.
        /// </summary>
        public ConcurrentObservableCollection() : base()
        {
        }

        /// <summary>
        /// Creates a new collection seeded with the provided information.
        /// </summary>
        /// <param name="list">The information to be used for seeding the collection.</param>
        public ConcurrentObservableCollection(IEnumerable<T> list) : base(list)
        {
        }

        /// <summary>
        /// Replaces the contents of the observable collection with new contents.
        /// </summary>
        /// <param name="collection">The new collection.</param>
        public void ReplaceAll(IEnumerable<T> collection)
        {
            Arguments.IsNotNull(collection, nameof(collection));
            
            suppressNotification = true;
            this.Clear();
            foreach (var item in collection)
            {
                this.Add(item);
            }
            suppressNotification = false;
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        /// <summary>
        /// Adds a collection to the existing collection.
        /// </summary>
        /// <param name="collection">The collection of records to add.</param>
        /// <returns><c>true</c> if any records were added; <c>false</c> otherwise.</returns>
        public bool AddRange(IEnumerable<T> collection)
        {
            Arguments.IsNotNull(collection, nameof(collection));
            suppressNotification = true;
            bool changed = false;
            foreach (var item in collection)
            {
                this.Add(item);
                changed = true;
            }
            suppressNotification = false;
            if (changed)
            {
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
            return changed;
        }

        /// <summary>
        /// Adds an item within a collection only if there are no items identified by the match function.
        /// </summary>
        /// <param name="match">The match function.</param>
        /// <param name="item">The item to add.</param>
        /// <returns><c>true</c> if the item was added, <c>false</c> otherwise.</returns>
        public bool AddIfMissing(Func<T, bool> match, T item)
        {
            Arguments.IsNotNull(match, nameof(match));
            Arguments.IsNotNull(item, nameof(item));
            if (!this.Any(match))
            {
                this.Add(item);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Removes items within the collection based on a match function.
        /// </summary>
        /// <param name="match">The match predicate.</param>
        /// <returns><c>true</c> if an item was removed, <c>false</c> otherwise.</returns>
        public bool RemoveIf(Func<T, bool> match)
        {
            Arguments.IsNotNull(match, nameof(match));
            var itemsToRemove = this.Where(match).ToArray();
            foreach (var item in itemsToRemove)
            {
                var idx = this.IndexOf(item);
                this.RemoveAt(idx);
            }
            return itemsToRemove.Length > 0;
        }

        /// <summary>
        /// Replaced items within the collection with a (single) replacement based on a match function.
        /// </summary>
        /// <param name="match">The match predicate.</param>
        /// <param name="replacement">The replacement item.</param>
        /// <returns><c>true</c> if an item was replaced, <c>false</c> otherwise.</returns>
        public bool ReplaceIf(Func<T, bool> match, T replacement)
        {
            Arguments.IsNotNull(match, nameof(match));
            Arguments.IsNotNull(replacement, nameof(replacement));
            var itemsToReplace = this.Where(match).ToArray();
            foreach (var item in itemsToReplace)
            {
                var idx = this.IndexOf(item);
                this[idx] = replacement;
            }
            return itemsToReplace.Length > 0;
        }

        /// <summary>
        /// Event trigger to indicate that the collection has changed in a thread-safe way.
        /// </summary>
        /// <param name="e">The event arguments</param>
        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (SynchronizationContext.Current == context)
            {
                RaiseCollectionChanged(e);
            }
            else
            {
                context.Send(RaiseCollectionChanged, e);
            }
        }

        /// <summary>
        /// Event trigger to indicate that a property has changed in a thread-safe way.
        /// </summary>
        /// <param name="e">The event arguments</param>
        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (SynchronizationContext.Current == context)
            {
                RaisePropertyChanged(e);
            }
            else
            {
                context.Send(RaisePropertyChanged, e);
            }
        }

        private void RaiseCollectionChanged(object param)
        {
            if (!suppressNotification)
            {
                base.OnCollectionChanged((NotifyCollectionChangedEventArgs)param);
            }
        }

        private void RaisePropertyChanged(object param)
        {
            base.OnPropertyChanged((PropertyChangedEventArgs)param);
        }
    }
}
