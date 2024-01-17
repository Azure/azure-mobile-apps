// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System.Collections.Concurrent;

namespace Microsoft.Datasync.Client.Http;

/// <summary>
/// Provides a client cache with the important "get or add" method.
/// </summary>
/// <typeparam name="TKey">The type of the key in the cache.</typeparam>
/// <typeparam name="TValue">The type of the value in the cache.</typeparam>
internal class ClientCache<TKey, TValue> : IDisposable where TKey : notnull
{
    /// <summary>
    /// The internal cache structure.
    /// </summary>
    internal ConcurrentDictionary<TKey, TValue> Cache { get; } = new();

    /// <summary>
    /// The timeout to use when waiting for the lock.
    /// </summary>
    internal TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(10);

    /// <summary>
    /// If <c>true</c>, the cache has been disposed and cannot be used.
    /// </summary>
    internal bool CacheIsDisposed = false;

    /// <summary>
    /// An internal lock obj
    /// </summary>
    internal object _lock = new();

    /// <summary>
    /// Retrieves a value from the client cache.  If the key does not exist, the factory is
    /// invoked to create a new value, which is then added to the cache and returned.
    /// </summary>
    /// <param name="key">The key to check or store.</param>
    /// <param name="factory">A factory method for creating a new value.</param>
    /// <returns>The value from the client cache.</returns>
    /// <exception cref="TimeoutException">If the factory method takes a long time.</exception>
    internal TValue GetOrAdd(TKey key, Func<TValue> factory)
    {
        // Quick bypass check to see if the cache has the key already.
        if (Cache.TryGetValue(key, out TValue? value))
        {
            return value;
        }
        return Add(key, factory);
    }

    /// <summary>
    /// Adds a value to the client cache.  If the key already exists, the existing value is returned.
    /// </summary>
    /// <param name="key">The key to check or store.</param>
    /// <param name="factory">A factory method for creating a new value.</param>
    /// <returns>The value from the client cache.</returns>
    /// <exception cref="TimeoutException"></exception>
    internal TValue Add(TKey key, Func<TValue> factory)
    {
        if (CacheIsDisposed)
        {
            throw new InvalidOperationException("Cache has been disposed");
        }

        if (Monitor.TryEnter(_lock, Timeout))
        {
            try
            {
                // Check again, in case there was a creation during the lock wait.
                if (Cache.TryGetValue(key, out TValue? value))
                {
                    return value;
                }

                // Create a new value, add it to the cache, and return it.
                value = factory.Invoke();
                Cache.TryAdd(key, value);
                return value;
            }
            finally
            {
                Monitor.Exit(_lock);
            }
        }
        else
        {
            throw new TimeoutException("Unable to obtain lock on client cache.");
        }
    }

    /// <summary>
    /// Implementation of the <see cref="IDisposable"/> pattern for derived classes to use.
    /// </summary>
    /// <param name="disposing"><c>true</c> if calling from <see cref="Dispose()"/> or the finalizer.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            // Cycle through each element in the cache and dispose of it.
            foreach (KeyValuePair<TKey, TValue> kvp in Cache)
            {
                if (kvp.Value is IDisposable disposable)
                {
                    disposable.Dispose();
                }
                Cache.TryRemove(kvp.Key, out _);
            }
            // Irrespective of the TryRemove() above, the cache is invalid - clear it.
            Cache.Clear();
            CacheIsDisposed = true;
        }
    }

    /// <summary>
    /// Implementation of the <see cref="IDisposable"/> pattern.
    /// </summary>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
