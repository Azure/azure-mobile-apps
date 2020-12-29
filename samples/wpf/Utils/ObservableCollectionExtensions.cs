using System;
using System.Linq;
using System.Collections.ObjectModel;

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
            try
            {
                items.Remove(items.Single(whereClause));
                return true;
            }
            catch (InvalidOperationException)
            {
                return false;
            }
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
            try
            {
                var idx = items.IndexOf(items.Single(whereClause));
                items[idx] = item;
                return true;
            }
            catch (InvalidOperationException)
            {
                return false;
            }
        }
    }
}
