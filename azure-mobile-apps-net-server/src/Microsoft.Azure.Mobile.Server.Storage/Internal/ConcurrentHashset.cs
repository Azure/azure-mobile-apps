// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------------------- 

using System.Collections.Generic;

namespace Microsoft.Azure.Mobile.Server.Internal
{
    /// <summary>
    /// A <see cref="HashSet{T}"/> collection implementation with a protective lock for concurrent access. 
    /// </summary>
    /// <typeparam name="T">The type of elements in the hash set.</typeparam>
    internal class ConcurrentHashset<T>
    {
        private readonly HashSet<T> index;
        private readonly object thisLock = new object();

        public ConcurrentHashset()
        {
            this.index = new HashSet<T>();
        }

        public ConcurrentHashset(IEqualityComparer<T> comparer)
        {
            this.index = new HashSet<T>(comparer);
        }

        public bool Add(T item)
        {
            lock (this.thisLock)
            {
                return this.index.Add(item);
            }
        }
    }
}
