// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Datasync.Client.Http
{
    /// <summary>
    /// A representation of the HTTP response with an in-memory copy of the payload.
    /// </summary>
    internal class ServiceResponse
    {
        /// <summary>
        /// Creates a filled-in <see cref="ServiceResponse"/> without content.
        /// </summary>
        /// <param name="response">The <see cref="HttpResponseMessage"/> to use as the source.</param>
        protected ServiceResponse(HttpResponseMessage response)
        {
            Arguments.IsNotNull(response, nameof(response));

            var headers = new Dictionary<string, IEnumerable<string>>();
            response.Headers.ToList().ForEach(hdr => headers.Add(hdr.Key, hdr.Value.ToArray()));

            Content = string.Empty;
            Headers = headers;
            ReasonPhrase = response.ReasonPhrase;
            ETag = response.Headers.ETag;
            StatusCode = (int)response.StatusCode;
            Version = new Version(response.Version.ToString());
        }

        /// <summary>
        /// Creates a new <see cref="ServiceResponse"/> from an existing <see cref="HttpResponseMessage"/>.  After execution,
        /// the <see cref="HttpResponseMessage"/> can be disposed.
        /// </summary>
        /// <param name="message">The source message.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>The equivalent <see cref="ServiceResponse"/></returns>
        internal static async Task<ServiceResponse> CreateResponseAsync(HttpResponseMessage message, CancellationToken cancellationToken = default)
        {
            var response = new ServiceResponse(message);
            if (message.Content != null)
            {
                // Bug #431: On MAUI/iOS, certain headers can appear in both HttpResponseMessage.Headers and
                // HttpContent.Headers which should never happen.  We protect against that by skipping any
                // headers that have already been added.
                message.Content.Headers.ToList().ForEach(hdr =>
                {
                    if (!response.Headers.ContainsKey(hdr.Key))
                    {
                        response.Headers.Add(hdr.Key, hdr.Value.ToArray());
                    }
                });
                response.Content = await message.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            }
            return response;
        }

        /// <summary>
        /// If not <c>null</c>, the payload of the response.
        /// </summary>
        public string Content { get; protected set; }

        /// <summary>
        /// The value of the <c>ETag</c> header (quoted)
        /// </summary>
        public EntityTagHeaderValue ETag { get; }

        /// <summary>
        /// <c>true</c> if a payload was sent from the service.
        /// </summary>
        public bool HasContent { get => Content.Length > 0; }

        /// <summary>
        /// <c>true</c> if the status code indicates a conflict.
        /// </summary>
        public bool IsConflictStatusCode { get => StatusCode == 409 || StatusCode == 412; }

        /// <summary>
        /// <c>true</c> if the HTTP request was successful.
        /// </summary>
        public bool IsSuccessStatusCode { get => StatusCode >= 200 && StatusCode <= 299; }

        /// <summary>
        /// The consolidated list of all headers returned in the response.
        /// </summary>
        public IDictionary<string, IEnumerable<string>> Headers { get; }

        /// <summary>
        /// The reason phrase which typically is sent by servers together with the status code.
        /// </summary>
        public string ReasonPhrase { get; }

        /// <summary>
        /// The HTTP status code for the response.
        /// </summary>
        public int StatusCode { get; }

        /// <summary>
        /// The HTTP message version.
        /// </summary>
        public Version Version { get; }
    }
}
