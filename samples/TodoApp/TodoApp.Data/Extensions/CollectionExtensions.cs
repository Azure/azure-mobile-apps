// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace TodoApp.Data.Extensions
{
    public static class CollectionExtensions
    {
        /// <summary>
        /// Replace an item within a collection based on a match function.
        /// </summary>
        /// <typeparam name="T">The type of the collection model.</typeparam>
        /// <param name="collection">The collection.</param>
        /// <param name="match">The match function</param>
        /// <param name="replacement">The replacement model item.</param>
        public static bool ReplaceIf<T>(this Collection<T> collection, Func<T, bool> match, T replacement)
        {
            var itemsToReplace = collection.Where(match).ToArray();
            foreach (var item in itemsToReplace)
            {
                var idx = collection.IndexOf(item);
                collection[idx] = replacement;
            }
            return itemsToReplace.Length > 0;
        }

        /// <summary>
        /// Remove an item within a collection based on a match function.
        /// </summary>
        /// <typeparam name="T">The type of the collection model.</typeparam>
        /// <param name="collection">The collection.</param>
        /// <param name="match">The match function</param>
        public static bool RemoveIf<T>(this Collection<T> collection, Func<T, bool> match)
        {
            var itemsToRemove = collection.Where(match).ToArray();
            foreach (var item in itemsToRemove)
            {
                var idx = collection.IndexOf(item);
                collection.RemoveAt(idx);
            }
            return itemsToRemove.Length > 0;
        }

        /// <summary>
        /// Clears the collection and replaces the contents with the provided content.
        /// </summary>
        /// <typeparam name="T">The type of the collection model.</typeparam>
        /// <param name="collection">The collection.</param>
        /// <param name="items">The new list of items.</param>
        public static void ReplaceAllItems<T>(this Collection<T> collection, IEnumerable<T> items)
        {
            collection.Clear();
            foreach (var item in items)
            {
                collection.Add(item);
            }
        }
    }
}
