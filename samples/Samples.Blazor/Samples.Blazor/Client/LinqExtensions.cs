using System.Collections.ObjectModel;

namespace Samples.Blazor.Client
{
    public static class LinqExtensions
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
    }
}
