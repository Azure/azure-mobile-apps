// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Datasync.Client.Extensions
{
    public static class HttpContentExtensions
    {
        /// <summary>
        /// A version of the <see cref="HttpContent.ReadAsByteArrayAsync"/> that takes a <see cref="CancellationToken"/>
        /// </summary>
        /// <param name="content">The <see cref="HttpContent"/> to read</param>
        /// <param name="token">A <see cref="CancellationToken"/></param>
        /// <returns>The byte array (asynchronously)</returns>
        public static Task<byte[]> ReadAsByteArrayAsync(this HttpContent content, CancellationToken token)
            => Task.Run<byte[]>(() => content.ReadAsByteArrayAsync(), token);
    }
}
