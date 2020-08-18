// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Zumo.Server.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

// This is a synchronous store, so disable all async warnings.
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously

namespace Microsoft.Zumo.Server.Test.Helpers
{
    /// <summary>
    /// A mock table repository that only stores data for the lifetime of the mock.
    /// </summary>
    /// <typeparam name="T">The type of entity</typeparam>
    public class MockTableRepository<T> : ITableRepository<T> where T : class, ITableData
    {
        public Dictionary<string, T> Data = new Dictionary<string, T>();

        // The number of times that the table repository has been called.
        public int CallCount { get; private set; } = 0;

        // The number of times the repository has been modified
        public int Modifications { get; private set; } = 0;

        // The last call to be made
        public string CallData { get; private set; } = "";

        private void Call(string v)
        {
            CallCount++;
            CallData = v;
        }

        public IQueryable<T> AsQueryable()
        {
            Call("AsQueryable");
            return Data.Values.AsQueryable();
        }

        public Task<T> CreateAsync(T item, CancellationToken cancellationToken = default)
        {
            Call("CreateAsync");
            if (item == null || string.IsNullOrEmpty(item.Id))
            {
                throw new ArgumentNullException(nameof(item));
            }

            if (Data.ContainsKey(item.Id))
            {
                throw new EntityExistsException();
            }

            UpdateVersionFields(item);
            Data.Add(item.Id, item);
            Modifications++;
            return Task.FromResult(item);
        }

        public async Task DeleteAsync(string id, CancellationToken cancellationToken = default)
        {
            Call("DeleteAsync");
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            if (!Data.ContainsKey(id))
            {
                throw new EntityDoesNotExistException();
            }

            Data.Remove(id);
            Modifications++;
        }

        public async ValueTask<T> LookupAsync(string id, CancellationToken cancellationToken = default)
        {
            Call("LookupAsync");
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException(nameof(id));
            }
            return Data.ContainsKey(id) ? Data[id] : null;
        }

        public Task<T> ReplaceAsync(T item, CancellationToken cancellationToken = default)
        {
            Call("ReplaceAsync");
            if (item == null || string.IsNullOrEmpty(item.Id))
            {
                throw new ArgumentNullException(nameof(item));
            }

            if (!Data.ContainsKey(item.Id))
            {
                throw new EntityDoesNotExistException();
            }

            UpdateVersionFields(item);
            Data[item.Id] = item;
            Modifications++;
            return Task.FromResult(item);
        }

        private void UpdateVersionFields(T item)
        {
            item.Version = Guid.NewGuid().ToByteArray();
            item.UpdatedAt = DateTimeOffset.UtcNow;
        }
    }
}
