// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Datasync.Client.Http
{
    /// <summary>
    /// Methods to extend HttpContent methods to support async with cancellation
    /// </summary>
    internal static class HttpContentExtensions
    {
        /// <summary>
        /// Reads the content of the <see cref="HttpContent"/> object as a byte array, with cancellation.
        /// </summary>
        /// <param name="content">The <see cref="HttpContent"/> object to process</param>
        /// <param name="token">A <see cref="CancellationToken"/></param>
        /// <returns>The content as a byte array, asynchronously</returns>
        internal static Task<byte[]> ReadAsByteArrayAsync(this HttpContent content, CancellationToken token)
            => Task.Run(() => content.ReadAsByteArrayAsync(), token);
    }
}
