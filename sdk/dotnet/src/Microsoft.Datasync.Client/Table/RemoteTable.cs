// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Http;
using Microsoft.Datasync.Client.Serialization;
using Microsoft.Datasync.Client.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Datasync.Client.Table
{
    /// <summary>
    /// Provides the operations that can be done against a remote table
    /// with untyped (JSON) object.
    /// </summary>
    internal class RemoteTable : IRemoteTable
    {
        /// <summary>
        /// Creates a new <see cref="RemoteTable"/> instance to perform
        /// untyped (JSON) requests to a remote table.
        /// </summary>
        /// <param name="tableName">The name of the table.</param>
        /// <param name="serviceClient">The service client that created this table.</param>
        internal RemoteTable(string tableName, DatasyncClient serviceClient)
        {
            Arguments.IsValidTableName(tableName, nameof(tableName));
            Arguments.IsNotNull(serviceClient, nameof(serviceClient));

            ServiceClient = serviceClient;
            TableName = tableName;
            TableEndpoint = ServiceClient.ClientOptions.TableEndpointResolver?.Invoke(tableName) ?? $"tables/{tableName.ToLowerInvariant()}";
        }

        #region IRemoteTable
        /// <summary>
        /// The service client being used for communication.
        /// </summary>
        public DatasyncClient ServiceClient { get; }

        /// <summary>
        /// The name of the table.
        /// </summary>
        public string TableName { get; }

        /// <summary>
        /// The endpoint to the table.
        /// </summary>
        public string TableEndpoint { get; }

        /// <summary>
        /// Deletes an item from the remote table.
        /// </summary>
        /// <param name="instance">The instance to delete from the table.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that returns the response when complete.</returns>
        public Task<JToken> DeleteItemAsync(JObject instance, CancellationToken cancellationToken = default)
        {
            string id = ServiceSerializer.GetId(instance);
            ServiceRequest request = new()
            {
                Method = HttpMethod.Delete,
                UriPathAndQuery = $"{TableEndpoint}/{id}",
                EnsureResponseContent = false,
                RequestHeaders = GetConditionalHeaders(instance)
            };
            return SendRequestAsync(request, cancellationToken);
        }

        /// <summary>
        /// Execute a query against a remote table.
        /// </summary>
        /// <param name="query">An OData query to execute.</param>
        /// <returns>A task that returns the results when the query finishes.</returns>
        public IAsyncEnumerable<JToken> GetAsyncItems(string query)
            => new FuncAsyncPageable<JToken>(nextLink => GetNextPageAsync(query, nextLink));

        /// <summary>
        /// Retrieve an item from the remote table.
        /// </summary>
        /// <param name="id">The ID of the item to retrieve.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that returns the item when complete.</returns>
        public Task<JToken> GetItemAsync(string id, CancellationToken cancellationToken = default)
        {
            Arguments.IsValidId(id, nameof(id));
            ServiceRequest request = new()
            {
                Method = HttpMethod.Get,
                UriPathAndQuery = $"{TableEndpoint}/{id}",
                EnsureResponseContent = true
            };
            return SendRequestAsync(request, cancellationToken);
        }

        /// <summary>
        /// Inserts an item into the remote table.
        /// </summary>
        /// <param name="instance">The instance to insert into the table.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that returns the inserted data when complete.</returns>
        public Task<JToken> InsertItemAsync(JObject instance, CancellationToken cancellationToken = default)
        {
            _ = ServiceSerializer.GetId(instance, allowDefault: true);
            ServiceRequest request = new()
            {
                Method = HttpMethod.Post,
                UriPathAndQuery = TableEndpoint,
                EnsureResponseContent = true,
                Content = instance.ToString(Formatting.None)
            };
            return SendRequestAsync(request, cancellationToken);
        }

        /// <summary>
        /// Replaces an item into the remote table.
        /// </summary>
        /// <param name="instance">The instance to replace into the table.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that returns the replaced data when complete.</returns>
        public Task<JToken> ReplaceItemAsync(JObject instance, CancellationToken cancellationToken = default)
        {
            string id = ServiceSerializer.GetId(instance);
            ServiceRequest request = new()
            {
                Method = HttpMethod.Put,
                UriPathAndQuery = $"{TableEndpoint}/{id}",
                EnsureResponseContent = true,
                Content = instance.ToString(Formatting.None),
                RequestHeaders = GetConditionalHeaders(instance)
            };
            return SendRequestAsync(request, cancellationToken);
        }

        /// <summary>
        /// Undeletes an item in the remote table.
        /// </summary>
        /// <remarks>
        /// This requires that the table supports soft-delete.
        /// </remarks>
        /// <param name="instance">The instance to undelete in the table.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that returns the response when complete.</returns>
        public Task<JToken> UndeleteItemAsync(JObject instance, CancellationToken cancellationToken = default)
        {
            string id = ServiceSerializer.GetId(instance);
            ServiceRequest request = new()
            {
                Method = ServiceRequest.PATCH,
                UriPathAndQuery = $"{TableEndpoint}/{id}",
                EnsureResponseContent = true,
                Content = "{\"deleted\":false}",
                RequestHeaders = GetConditionalHeaders(instance)
            };
            return SendRequestAsync(request, cancellationToken);
        }
        #endregion

        /// <summary>
        /// Gets a single page of items produced as a result of a query against the server.
        /// </summary>
        /// <param name="query">The query string to send to the service.</param>
        /// <param name="requestUri">The request URI to send (if we're on the second or future pages)</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A <see cref="Page{JToken}"/> containing the page of items.</returns>
        protected async Task<Page<JToken>> GetNextPageAsync(string query = "", string requestUri = null, CancellationToken cancellationToken = default)
        {
            string queryString = string.IsNullOrEmpty(query) ? string.Empty : $"?{query.TrimStart('?').TrimEnd()}";
            ServiceRequest request = new()
            {
                Method = HttpMethod.Get,
                UriPathAndQuery = requestUri ?? TableEndpoint + queryString,
                EnsureResponseContent = true
            };
            var response = await SendRequestAsync(request, cancellationToken).ConfigureAwait(false);
            var result = new Page<JToken>();
            if (response is JObject)
            {
                if (response[Page.JsonItemsProperty]?.Type == JTokenType.Array)
                {
                    result.Items = ((JArray)response[Page.JsonItemsProperty] as JArray).ToList();
                }
                if (response[Page.JsonCountProperty]?.Type == JTokenType.Integer)
                {
                    result.Count = response.Value<long>(Page.JsonCountProperty);
                }
                if (response[Page.JsonNextLinkProperty]?.Type == JTokenType.String)
                {
                    result.NextLink = new Uri(response.Value<string>(Page.JsonNextLinkProperty));
                }
            }
            return result;
        }

        /// <summary>
        /// Gets the conditional headers for the request to the remote service.
        /// </summary>
        /// <param name="instance">The instance being sent.</param>
        /// <returns>The conditional headers, or <c>null</c> if there are no conditional headers.</returns>
        protected static Dictionary<string, string> GetConditionalHeaders(JObject instance)
        {
            string version = ServiceSerializer.GetVersion(instance);
            return string.IsNullOrEmpty(version) ? null : new Dictionary<string, string> { [ServiceHeaders.IfMatch] = version.ToETagValue() };
        }

        /// <summary>
        /// Parses the response content into a <see cref="JToken"/> and adds the version system property
        /// if the <c>ETag</c> header was returned from the server.
        /// </summary>
        /// <param name="response">The response to parse.</param>
        /// <returns>The parsed <see cref="JToken"/>.</returns>
        protected JToken GetJTokenFromResponse(ServiceResponse response)
        {
            if (response.HasContent)
            {
                JToken token = JsonConvert.DeserializeObject<JToken>(response.Content, ServiceClient.Serializer.SerializerSettings);
                if (response.ETag != null)
                {
                    token[SystemProperties.JsonVersionProperty] = response.ETag.GetVersion();
                }
                return token;
            }
            return null;
        }

        /// <summary>
        /// Sends a request to the service.
        /// </summary>
        /// <param name="request">The service request.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>The response from the service.</returns>
        /// <exception cref="DatasyncConflictException"></exception>
        protected async Task<JToken> SendRequestAsync(ServiceRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await ServiceClient.HttpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
                return GetJTokenFromResponse(response);
            }
            catch (DatasyncInvalidOperationException ex) when (ex.IsConflictStatusCode())
            {
                string content = await ex.Response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                JToken token = string.IsNullOrEmpty(content) ? null : JsonConvert.DeserializeObject<JToken>(content, ServiceClient.Serializer.SerializerSettings);
                JObject value = ValidItemOrNull(token);
                if (value != null)
                {
                    throw new DatasyncConflictException(ex, ValidItemOrNull(token));
                }

                throw;
            }
        }

        /// <summary>
        /// Determines if the specified <see cref="JToken"/> is valid; if it is, then return the
        /// associated <see cref="JObject"/>; otherwise return <c>null</c>.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns>The <see cref="JObject"/>, or <c>null</c>.</returns>
        protected static JObject ValidItemOrNull(JToken item)
            => item is JObject obj && obj.Value<string>(SystemProperties.JsonIdProperty) != null ? (JObject)item : null;
    }
}
