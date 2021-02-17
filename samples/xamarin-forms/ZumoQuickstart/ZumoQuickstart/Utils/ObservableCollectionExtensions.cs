using System;
using System.Collections.ObjectModel;
using Xamarin.Forms.Internals;

namespace ZumoQuickstart
{
    public static class ObservableCollectionExtensions
    {
        /// <summary>
        /// Remove the element identified by the whereClause.
        /// </summary>
        /// <param name="items">The items to modify</param>
        /// <param name="whereClause">The predicate to use</param>
        /// <returns>True if an item was removed</returns>
        public static bool RemoveIf(this ObservableCollection<TodoItem> items, Func<TodoItem, bool> whereClause)
        {
            var index = items.IndexOf(whereClause);
            if (index >= 0)
            {
                items.RemoveAt(index);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Replaces the item identified by the whereClause
        /// </summary>
        /// <param name="items">The items to modify</param>
        /// <param name="whereClause">The predicate to use</param>
        /// <param name="item">The new item</param>
        /// <returns>True if an item was updated</returns>
        public static bool ReplaceIf(this ObservableCollection<TodoItem> items, Func<TodoItem, bool> whereClause, TodoItem item)
        {
            var index = items.IndexOf(whereClause);
            if (index >= 0)
            {
                items[index] = item;
                return true;
            }
            return false;
        }
    }
}
