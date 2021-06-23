// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Http;
using Microsoft.Datasync.Client.Internal;
using Microsoft.Datasync.Client.Table;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Datasync.Client
{
    /// <summary>
    /// Additional CRUDL methods for the <see cref="IDatasyncTable{T}"/> that use
    /// the underlying data access methods.
    /// </summary>
    public static class IDatasyncTableExtensions
    {
        /// <summary>
        /// Delete an item, but only if unchanged.
        /// </summary>
        /// <typeparam name="T">The type of the item</typeparam>
        /// <param name="table">The table holding the item</param>
        /// <param name="item">The item to delete</param>
        /// <param name="token">A <see cref="CancellationToken"/></param>
        /// <returns>A response object (asynchronously)</returns>
        public static Task<HttpResponse> DeleteItemIfUnchangedAsync<T>(this IDatasyncTable<T> table, T item, CancellationToken token = default) where T : notnull
        {
            Validate.IsNotNull(item, nameof(item));
            var id = Utils.GetIdFromItem(item);
            Validate.IsValidId(id, nameof(item));
            string version = Utils.GetVersionFromItem(item);
            Validate.IsNotNullOrEmpty(version, nameof(item));

            return table.DeleteItemAsync(id, HttpCondition.IfMatch(version), token);
        }

        /// <summary>
        /// Retrieve an item, but only if it has changed.
        /// </summary>
        /// <typeparam name="T">The type of the item</typeparam>
        /// <param name="item">The item to be refreshed</param>
        /// <param name="token">A <see cref="CancellationToken"/></param>
        /// <returns>The response object (asynchronously)</returns>
        /// <exception cref="NotModifiedException">thrown if the item has not been modified</exception>
        public static Task<HttpResponse<T>> RefreshItemAsync<T>(this IDatasyncTable<T> table, T item, CancellationToken token = default) where T : notnull
        {
            Validate.IsNotNull(item, nameof(item));
            var id = Utils.GetIdFromItem(item);
            Validate.IsValidId(id, nameof(item));
            string version = Utils.GetVersionFromItem(item);
            Validate.IsNotNullOrEmpty(version, nameof(item));

            return table.GetItemAsync(id, HttpCondition.IfNotMatch(version), token);
        }

        /// <summary>
        /// Replace an item, but only if unchanged since the last time.
        /// </summary>
        /// <typeparam name="T">The type of the item</typeparam>
        /// <param name="table">The table holding the item</param>
        /// <param name="item">The item to delete</param>
        /// <param name="token">A <see cref="CancellationToken"/></param>
        /// <returns>A response object (asynchronously)</returns>
        public static Task<HttpResponse<T>> ReplaceItemIfUnchangedAsync<T>(this IDatasyncTable<T> table, T item, CancellationToken token = default) where T : notnull
        {
            Validate.IsNotNull(item, nameof(item));
            var id = Utils.GetIdFromItem(item);
            Validate.IsValidId(id, nameof(item));
            string version = Utils.GetVersionFromItem(item);
            Validate.IsNotNullOrEmpty(version, nameof(item));

            return table.ReplaceItemAsync(item, HttpCondition.IfMatch(version), token);
        }
    }
}
