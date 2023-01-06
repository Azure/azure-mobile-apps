// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Utils;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

#nullable enable

namespace Microsoft.Datasync.Client.Extensions
{
    /// <summary>
    /// A set of extensions for the <see cref="System.Linq"/> library.  These
    /// mirror the <c>System.Linq.Async</c> functionality where required.  
    /// </summary>
    /// <see href="https://github.com/dotnet/reactive/blob/main/Ix.NET/Source/System.Linq.Async/System/Linq/Operators"/>
    internal static class LinqExtensions
    {
        /// <summary>
        /// Creates an array from an async-enumerable sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <param name="source">The source async-enumerable sequence to get an array of elements for.</param>
        /// <param name="cancellationToken">The optional cancellation token to be used for cancelling the sequence at any time.</param>
        /// <returns>An async-enumerable sequence containing a single element with an array containing all the elements of the source sequence.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is null.</exception>
        /// <remarks>The return type of this operator differs from the corresponding operator on IEnumerable in order to retain asynchronous behavior.</remarks>
        internal static async ValueTask<TSource[]> ToZumoArrayAsync<TSource>(this IAsyncEnumerable<TSource> source, CancellationToken cancellationToken = default)
        {
            Arguments.IsNotNull(source, nameof(source));
            var list = await source.ToZumoListAsync(cancellationToken).ConfigureAwait(false);
            return list.ToArray();
        }

        /// <summary>
        /// Creates a dictionary from an async-enumerable sequence according to a specified key selector function.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <typeparam name="TKey">The type of the dictionary key computed for each element in the source sequence.</typeparam>
        /// <param name="source">An async-enumerable sequence to create a dictionary for.</param>
        /// <param name="keySelector">A function to extract a key from each element.</param>
        /// <param name="cancellationToken">The optional cancellation token to be used for cancelling the sequence at any time.</param>
        /// <returns>An async-enumerable sequence containing a single element with a dictionary mapping unique key values onto the corresponding source sequence's element.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="keySelector"/> is null.</exception>
        /// <remarks>The return type of this operator differs from the corresponding operator on IEnumerable in order to retain asynchronous behavior.</remarks>
        internal static ValueTask<Dictionary<TKey, TSource>> ToZumoDictionaryAsync<TSource, TKey>(this IAsyncEnumerable<TSource> source, Func<TSource, TKey> keySelector, CancellationToken cancellationToken = default) where TKey : notnull 
            => ToZumoDictionaryAsync(source, keySelector, comparer: null, cancellationToken);

        /// <summary>
        /// Creates a dictionary from an async-enumerable sequence according to a specified key selector function, and a comparer.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <typeparam name="TKey">The type of the dictionary key computed for each element in the source sequence.</typeparam>
        /// <param name="source">An async-enumerable sequence to create a dictionary for.</param>
        /// <param name="keySelector">A function to extract a key from each element.</param>
        /// <param name="comparer">An equality comparer to compare keys.</param>
        /// <param name="cancellationToken">The optional cancellation token to be used for cancelling the sequence at any time.</param>
        /// <returns>An async-enumerable sequence containing a single element with a dictionary mapping unique key values onto the corresponding source sequence's element.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="keySelector"/> or <paramref name="comparer"/> is null.</exception>
        /// <remarks>The return type of this operator differs from the corresponding operator on IEnumerable in order to retain asynchronous behavior.</remarks>
        internal static async ValueTask<Dictionary<TKey, TSource>> ToZumoDictionaryAsync<TSource, TKey>(this IAsyncEnumerable<TSource> source, Func<TSource, TKey> keySelector, IEqualityComparer<TKey>? comparer, CancellationToken cancellationToken = default) where TKey : notnull
        {
            Arguments.IsNotNull(source, nameof(source));
            Arguments.IsNotNull(keySelector, nameof(keySelector));
            var d = new Dictionary<TKey, TSource>(comparer);
            await foreach (var item in source.WithCancellation(cancellationToken).ConfigureAwait(false))
            {
                var key = keySelector(item);
                d.Add(key, item);
            }
            return d;
        }

        /// <summary>
        /// Converts an async-enumerable sequence to an enumerable sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <param name="source">An async-enumerable sequence to convert to an enumerable sequence.</param>
        /// <returns>The enumerable sequence containing the elements in the async-enumerable sequence.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is null.</exception>
        internal static IEnumerable<TSource> ToZumoEnumerable<TSource>(this IAsyncEnumerable<TSource> source)
        {
            Arguments.IsNotNull(source, nameof(source));
            return Core(source);

            static IEnumerable<TSource> Core(IAsyncEnumerable<TSource> source)
            {
                var e = source.GetAsyncEnumerator(default);
                try
                {
                    while (true)
                    {
                        if (!ZumoWait(e.MoveNextAsync()))
                            break;
                        yield return e.Current;
                    }
                }
                finally
                {
                    ZumoWait(e.DisposeAsync());
                }
            }
        }

        /// <summary>
        /// Creates a hash set from an async-enumerable sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <param name="source">The source async-enumerable sequence to get a hash set of elements for.</param>
        /// <param name="cancellationToken">The optional cancellation token to be used for cancelling the sequence at any time.</param>
        /// <returns>An async-enumerable sequence containing a single element with a hash set containing all the elements of the source sequence.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is null.</exception>
        /// <remarks>The return type of this operator differs from the corresponding operator on IEnumerable in order to retain asynchronous behavior.</remarks>
        internal static ValueTask<HashSet<TSource>> ToZumoHashSetAsync<TSource>(this IAsyncEnumerable<TSource> source, CancellationToken cancellationToken = default) 
            => ToZumoHashSetAsync(source, comparer: null, cancellationToken);

        /// <summary>
        /// Creates a hash set from an async-enumerable sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <param name="source">The source async-enumerable sequence to get a hash set of elements for.</param>
        /// <param name="comparer">An equality comparer to compare elements of the sequence.</param>
        /// <param name="cancellationToken">The optional cancellation token to be used for cancelling the sequence at any time.</param>
        /// <returns>An async-enumerable sequence containing a single element with a hash set containing all the elements of the source sequence.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is null.</exception>
        /// <remarks>The return type of this operator differs from the corresponding operator on IEnumerable in order to retain asynchronous behavior.</remarks>
        internal static async ValueTask<HashSet<TSource>> ToZumoHashSetAsync<TSource>(this IAsyncEnumerable<TSource> source, IEqualityComparer<TSource>? comparer, CancellationToken cancellationToken = default)
        {
            Arguments.IsNotNull(source, nameof(source));
            var set = new HashSet<TSource>(comparer);
            await foreach (var item in source.WithCancellation(cancellationToken).ConfigureAwait(false))
            {
                set.Add(item);
            }
            return set;
        }

        /// <summary>
        /// Creates a list from an async-enumerable sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <param name="source">The source async-enumerable sequence to get a list of elements for.</param>
        /// <param name="cancellationToken">The optional cancellation token to be used for cancelling the sequence at any time.</param>
        /// <returns>A list containing all the elements of the source sequence.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is null.</exception>
        internal static async ValueTask<List<TSource>> ToZumoListAsync<TSource>(this IAsyncEnumerable<TSource> source, CancellationToken cancellationToken = default)
        {
            Arguments.IsNotNull(source, nameof(source));
            var list = new List<TSource>();
            await foreach (var item in source.WithCancellation(cancellationToken).ConfigureAwait(false))
            {
                list.Add(item);
            }
            return list;
        }

        /// <summary>
        /// Creates a <see cref="ConcurrentObservableCollection{T}"/> from an async-enumerable sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <param name="source">The source async-enumerable sequence to get a list of elements for.</param>
        /// <param name="cancellationToken">The optional cancellation token to be used for cancelling the sequence at any time.</param>
        /// <returns>An observable collection containing all the elements of the source sequence.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is null.</exception>
        internal static ValueTask<ConcurrentObservableCollection<TSource>> ToZumoObservableCollectionAsync<TSource>(this IAsyncEnumerable<TSource> source, CancellationToken cancellationToken = default)
            => source.ToZumoObservableCollectionAsync(new ConcurrentObservableCollection<TSource>(), cancellationToken);

        /// <summary>
        /// Updates a <see cref="ConcurrentObservableCollection{T}"/> from an async-enumerable sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <param name="source">The source async-enumerable sequence to get a list of elements for.</param>
        /// <param name="existingCollection">The existing observable collection object.</param>
        /// <param name="cancellationToken">The optional cancellation token to be used for cancelling the sequence at any time.</param>
        /// <returns>The updated observable collection containing all the elements of the source sequence.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is null.</exception>
        internal static async ValueTask<ConcurrentObservableCollection<TSource>> ToZumoObservableCollectionAsync<TSource>(this IAsyncEnumerable<TSource> source, ConcurrentObservableCollection<TSource> existingCollection, CancellationToken cancellationToken = default)
        {
            Arguments.IsNotNull(source, nameof(source));
            var list = await source.ToZumoListAsync(cancellationToken).ConfigureAwait(false);
            existingCollection.ReplaceAll(list);
            return existingCollection;
        }

        // NB: ValueTask and ValueTask<T> do not have to support blocking on a call to GetResult when backed by
        //     an IValueTaskSource or IValueTaskSource<T> implementation. Convert to a Task or Task<T> to do so
        //     in case the task hasn't completed yet.

        private static void ZumoWait(ValueTask task)
        {
            var awaiter = task.GetAwaiter();

            if (!awaiter.IsCompleted)
            {
                task.AsTask().GetAwaiter().GetResult();
                return;
            }

            awaiter.GetResult();
        }

        private static T ZumoWait<T>(ValueTask<T> task)
        {
            var awaiter = task.GetAwaiter();

            if (!awaiter.IsCompleted)
            {
                return task.AsTask().GetAwaiter().GetResult();
            }

            return awaiter.GetResult();
        }
    }
}
