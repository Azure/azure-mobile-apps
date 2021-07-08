// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Http;
using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Datasync.Client
{
    /// <summary>
    /// An exception thrown when the requested operation is in conflict with the information
    /// already present on th service.
    /// </summary>
    /// <typeparam name="T">The type of the entity being transferred</typeparam>
    public sealed class DatasyncConflictException<T> : DatasyncOperationException
    {
        private DatasyncConflictException(HttpRequestMessage request, HttpResponseMessage response)
            : base(request, response)
        {
            Content = Array.Empty<byte>();
            ServerItem = default;
        }

        internal static async Task<DatasyncConflictException<T>> CreateAsync(HttpRequestMessage request, HttpResponseMessage response, JsonSerializerOptions deserializerOptions, CancellationToken token = default)
        {
            var content = await response.Content.ReadAsByteArrayAsync(token).ConfigureAwait(false);
            return new DatasyncConflictException<T>(request, response)
            {
                Content = content,
                ServerItem = JsonSerializer.Deserialize<T>(content, deserializerOptions)
            };
        }

        public byte[] Content { get; private set; }

        /// <summary>
        /// The deserialized content of the payload.
        /// </summary>
        public T ServerItem { get; private set; }
    }
}
