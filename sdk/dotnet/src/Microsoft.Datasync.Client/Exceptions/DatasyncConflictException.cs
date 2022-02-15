// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Http;
using Microsoft.Datasync.Client.Utils;
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
            Validate.IsNotNull(request, nameof(request));
            Validate.IsNotNull(response, nameof(response));
            Validate.IsNotNull(deserializerOptions, nameof(deserializerOptions));

            var content = await response.Content.ReadAsByteArrayAsync(token).ConfigureAwait(false);
            return new DatasyncConflictException<T>(request, response)
            {
                Content = content,
                ServerItem = content.Length > 0 ? JsonSerializer.Deserialize<T>(content, deserializerOptions) : default
            };
        }
        /// <summary>
        /// The content from the response.
        /// </summary>
        public new byte[] Content { get; private set; }

        /// <summary>
        /// The deserialized content of the payload.
        /// </summary>
        public T ServerItem { get; private set; }
    }
}
