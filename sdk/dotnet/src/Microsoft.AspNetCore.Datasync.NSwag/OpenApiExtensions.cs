// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using NJsonSchema;
using NSwag.Generation.AspNetCore;
using NSwag;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;

namespace Microsoft.AspNetCore.Datasync.NSwag
{
    /// <summary>
    /// A set of extension methods for working with OpenApi types.
    /// </summary>
    public static class NSwagOpenApiDatasyncExtensions
    {
        private const string apiVersionHeader = "ZUMO-API-VERSION";
        private const string etagHeader = "ETag";
        private const string jsonMediaType = "application/json";
        private static readonly string[] versionStrings = new string[] { "3.0.0", "2.0.0" };

        /// <summary>
        /// Incorporates the operation and schema processors into the NSwag document processors.
        /// </summary>
        /// <param name="settings">The NSwag settings object.</param>
        public static void AddDatasyncProcessors(this AspNetCoreOpenApiDocumentGeneratorSettings settings)
        {
            settings.OperationProcessors.Add(new DatasyncOperationProcessor());
            settings.SchemaProcessors.Add(new DatasyncSchemaProcessor());
        }

        /// <summary>
        /// Determines if the operation contains the specified request header.
        /// </summary>
        /// <param name="operation">The operation to check.</param>
        /// <param name="headerName">The name of the header to check for.</param>
        /// <returns></returns>
        internal static bool ContainsRequestHeader(this OpenApiOperation operation, string headerName)
            => operation.Parameters.Any(parameter => parameter.Name == headerName && parameter.Kind == OpenApiParameterKind.Header);

        /// <summary>
        /// Adds the required request headers for a datasync table controller.
        /// </summary>
        /// <param name="operation">The operation to modify.</param>
        internal static void AddDatasyncRequestHeaders(this OpenApiOperation operation)
        {
            if (!operation.ContainsRequestHeader(apiVersionHeader))
            {
                var versionSchema = new JsonSchema { Type = JsonObjectType.String };
                versionStrings.ToList().ForEach(s => versionSchema.Enumeration.Add(s));

                operation.Parameters.Add(new OpenApiParameter
                {
                    Name = apiVersionHeader,
                    Kind = OpenApiParameterKind.Header,
                    Description = "Sets the Datasync API version to use.",
                    IsRequired = true,
                    Schema = versionSchema
                });
            }
        }

        /// <summary>
        /// Adds the specified response code to the operation.
        /// </summary>
        /// <param name="operation">The <see cref="OpenApiOperation"/> to edit.</param>
        /// <param name="statusCode">The HTTP status code to add.</param>
        /// <param name="schema">The <see cref="JsonSchema"/> for the response content.</param>
        internal static void SetResponse(this OpenApiOperation operation, HttpStatusCode statusCode, JsonSchema schema = null, bool includeETagHeader = true)
        {
            var statusId = (int)statusCode;
            var description = Regex.Replace(statusCode.ToString(), "(?<=[a-z])([A-Z])", " $1", RegexOptions.Compiled);
            var response = new OpenApiResponse
            {
                Description = description
            };
            if (schema != null)
            {
                response.Content.Add(jsonMediaType, new OpenApiMediaType { Schema = schema });
                if (includeETagHeader)
                {
                    response.Headers.Add("ETag", new OpenApiHeader
                    {
                        Description = "The version string of the server entity, per RFC 9110",
                        Schema = new JsonSchema { Type = JsonObjectType.String }
                    });
                }
            }
            operation.Responses[statusId.ToString()] = response;
        }

        /// <summary>
        /// Adds appropriate request headers and responses for conditional requests.
        /// </summary>
        /// <param name="operation">The <see cref="OpenApiOperation"/> to edit.</param>
        /// <param name="isRead">If true, it's a read operation.</param>
        /// <param name="schema">The <see cref="JsonSchema"/> of the entity.</param>
        internal static void AddConditionalRequestSupport(this OpenApiOperation operation, JsonSchema schema, bool isRead = false)
        {
            var headerName = isRead ? "If-None-Match" : "If-Match";
            var description = isRead
                ? "Conditionally execute only if the entity version does not match the provided string (RFC 9110 13.1.2)."
                : "Conditionally execute only if the entity version matches the provided string (RFC 9110 13.1.1).";
            if (!operation.ContainsRequestHeader(etagHeader))
            {
                operation.Parameters.Add(new OpenApiParameter
                {
                    Name = headerName,
                    Kind = OpenApiParameterKind.Header,
                    Description = description,
                    IsRequired = false,
                    Schema = new JsonSchema { Type = JsonObjectType.String }
                });
            }

            operation.SetResponse(HttpStatusCode.Conflict, schema);
            operation.SetResponse(HttpStatusCode.PreconditionFailed, schema);
        }
        /// <summary>
        /// Adds the OData query parameter to the operation.
        /// </summary>
        /// <param name="operation">The <see cref="OpenApiOperation"/> reference.</param>
        /// <param name="parameterName">The name of the query parameter.</param>
        /// <param name="parameterType">The OpenAPI type for the query parameter.</param>
        /// <param name="description">The OpenAPI description for the query parameter.</param>
        internal static void AddODataQueryParameter(this OpenApiOperation operation, string parameterName, JsonObjectType parameterType, string description)
        {
            operation.Parameters.Add(new OpenApiParameter
            {
                Name = parameterName,
                Description = description,
                Kind = OpenApiParameterKind.Query,
                IsRequired = false,
                Schema = new JsonSchema { Type = parameterType }
            });
        }

        /// <summary>
        /// Adds the OData query parameters for the <c>GET list</c> operation.
        /// </summary>
        /// <param name="operation">The <see cref="OpenApiOperation"/> reference.</param>
        internal static void AddODataQueryParameters(this OpenApiOperation operation)
        {
            operation.AddODataQueryParameter("$count", JsonObjectType.Boolean, "If true, return the total number of items matched by the filter");
            operation.AddODataQueryParameter("$filter", JsonObjectType.String, "An OData filter describing the entities to be returned");
            operation.AddODataQueryParameter("$orderby", JsonObjectType.String, "A comma-separated list of ordering instructions.  Each ordering instruction is a field name with an optional direction (asc or desc).");
            operation.AddODataQueryParameter("$select", JsonObjectType.String, "A comma-separated list of fields to be returned in the result set.");
            operation.AddODataQueryParameter("$skip", JsonObjectType.Integer, "The number of items in the list to skip for paging support.");
            operation.AddODataQueryParameter("$top", JsonObjectType.Integer, "The number of items in the list to return for paging support.");
            operation.AddODataQueryParameter("__includedeleted", JsonObjectType.Boolean, "If true, soft-deleted items are returned as well as non-deleted items.");
        }
    }
}