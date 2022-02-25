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
        /// Deletes an item from the remote table.
        /// </summary>
        /// <param name="instance">The instance to delete from the table.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that returns the response when complete.</returns>
        public Task<JToken> DeleteItemAsync(JObject instance, CancellationToken cancellationToken = default)
        {
            Arguments.IsNotNull(instance, nameof(instance));
            ObjectReader.GetSystemProperties(instance, out SystemProperties systemProperties);
            Arguments.IsValidId(systemProperties.Id, nameof(instance));

            ServiceRequest request = new()
            {
                Method = HttpMethod.Delete,
                UriPathAndQuery = CreateUriPath(TableName, systemProperties.Id),
                EnsureResponseContent = false
            };
            AddIfMatchVersionHeader(request, systemProperties.Version);
            return SendRequestAsync(request, cancellationToken);
        }

        /// <summary>
        /// Execute a query against a remote table.
        /// </summary>
        /// <param name="query">An OData query to execute.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
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

            throw new NotImplementedException();
        }

        /// <summary>
        /// Inserts an item into the remote table.
        /// </summary>
        /// <param name="instance">The instance to insert into the table.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A task that returns the inserted data when complete.</returns>
        public Task<JToken> InsertItemAsync(JObject instance, CancellationToken cancellationToken = default)
        {
            Arguments.IsNotNull(instance, nameof(instance));
            ObjectReader.GetSystemProperties(instance, out SystemProperties systemProperties);
            if (systemProperties.Id != null)
            {
                // If the Id is set, then it must be valid.
                Arguments.IsValidId(systemProperties.Id, nameof(instance));
            }
            ServiceRequest request = new()
            {
                Method = HttpMethod.Post,
                UriPathAndQuery = CreateUriPath(TableName),
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
            Arguments.IsNotNull(instance, nameof(instance));
            ObjectReader.GetSystemProperties(instance, out SystemProperties systemProperties);
            Arguments.IsValidId(systemProperties.Id, nameof(instance));

            ServiceRequest request = new()
            {
                Method = HttpMethod.Put,
                UriPathAndQuery = CreateUriPath(TableName, systemProperties.Id),
                EnsureResponseContent = true,
                Content = instance.ToString(Formatting.None)
            };
            AddIfMatchVersionHeader(request, systemProperties.Version);
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
            Arguments.IsNotNull(instance, nameof(instance));
            ObjectReader.GetSystemProperties(instance, out SystemProperties systemProperties);
            Arguments.IsValidId(systemProperties.Id, nameof(instance));

            ServiceRequest request = new()
            {
                Method = ServiceRequest.PATCH,
                UriPathAndQuery = CreateUriPath(TableName, systemProperties.Id),
                EnsureResponseContent = true,
                Content = "{\"deleted\":false}"
            };
            AddIfMatchVersionHeader(request, systemProperties.Version);
            return SendRequestAsync(request, cancellationToken);
        }
        #endregion

        protected Task<Page<JToken>> GetNextPageAsync(string query, string nextLink)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Adds an If-Match header matching the provided version to the request.
        /// </summary>
        /// <param name="request">The <see cref="ServiceRequest"/> to modify.</param>
        /// <param name="version">The version of the entity.</param>
        protected static void AddIfMatchVersionHeader(ServiceRequest request, string version)
        {
            if (!string.IsNullOrWhiteSpace(version))
            {
                (request.RequestHeaders ??= new Dictionary<string, string>()).Add(ServiceHeaders.IfMatch, version.ToETagValue());
            }
        }

        /// <summary>
        /// Creates the relevant URI path from the list of segments.
        /// </summary>
        /// <param name="segments">The list of segments comprising the path</param>
        /// <returns>The URI Path.</returns>
        protected static string CreateUriPath(params string[] segments)
            => "/tables/" + string.Join("/", segments.Select(segment => Uri.EscapeDataString(segment)).ToArray());

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

        protected async Task<JToken> SendRequestAsync(ServiceRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await ServiceClient.HttpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
                return GetJTokenFromResponse(response);
            }
            catch (DatasyncInvalidOperationException ex)
            {
                if (ex.Response == null || !ex.Response.IsConflictStatusCode())
                {
                    throw;
                }

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
