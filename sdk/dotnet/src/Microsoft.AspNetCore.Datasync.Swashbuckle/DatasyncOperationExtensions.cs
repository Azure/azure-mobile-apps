// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.OpenApi.Any;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.OpenApi.Models
{
    /// <summary>
    /// A set of extension methods for working with the <see cref="OpenApiOperation"/> class.
    /// </summary>
    internal static class OpenApiDatasyncExtensions
    {
        private const string JsonMediaType = "application/json";
        private static string[] SystemProperties = { "updatedAt", "version", "deleted" };

        /// <summary>
        /// Adds a <c>ZUMO-API-VERSION</c> header parameter to the operation document.
        /// </summary>
        /// <param name="operation">The <see cref="OpenApiOperation"/> reference.</param>
        internal static void AddZumoApiVersionHeader(this OpenApiOperation operation)
        {
            var versionStrings = new string[] { "3.0.0", "2.0.0" };

            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "ZUMO-API-VERSION",
                Description = "Datasync protocol version to be used.",
                In = ParameterLocation.Header,
                Required = true,
                Schema = new OpenApiSchema
                {
                    Type = "string",
                    Enum = versionStrings.Select(p => new OpenApiString(p)).ToList<IOpenApiAny>()
                }
            });
        }

        /// <summary>
        /// Adds an appropriate conditional header (either <c>If-Match</c> or <c>If-None-Match</c> to the list of
        /// allowed headers.
        /// </summary>
        /// <param name="operation">The <see cref="OpenApiOperation"/> reference.</param>
        /// <param name="ifNoneMatch">If <c>true</c>, add a <c>If-None-Match</c> header.</param>
        internal static void AddConditionalHeader(this OpenApiOperation operation, bool ifNoneMatch = false)
        {
            var headerName = ifNoneMatch ? "If-None-Match" : "If-Match";
            var description = ifNoneMatch
                ? "Conditionally execute only if the entity version does not match the provided string (RFC 9110 13.1.2)."
                : "Conditionally execute only if the entity version matches the provided string (RFC 9110 13.1.1).";

            operation.Parameters.Add(new OpenApiParameter
            {
                Name = headerName,
                Description = description,
                In = ParameterLocation.Header,
                Required = false,
                Schema = new OpenApiSchema { Type = "string" }
            });
        }

        /// <summary>
        /// Adds the OData query parameter to the operation.
        /// </summary>
        /// <param name="operation">The <see cref="OpenApiOperation"/> reference.</param>
        /// <param name="parameterName">The name of the query parameter.</param>
        /// <param name="parameterType">The OpenAPI type for the query parameter.</param>
        /// <param name="description">The OpenAPI description for the query parameter.</param>
        internal static void AddODataQueryParameter(this OpenApiOperation operation, string parameterName, string parameterType, string description)
        {
            operation.Parameters.Add(new OpenApiParameter
            {
                Name = parameterName,
                Description = description,
                In = ParameterLocation.Query,
                Required = false,
                Schema = new OpenApiSchema { Type = parameterType }
            });
        }

        /// <summary>
        /// Adds the OData query parameters for the <c>GET list</c> operation.
        /// </summary>
        /// <param name="operation">The <see cref="OpenApiOperation"/> reference.</param>
        internal static void AddODataQueryParameters(this OpenApiOperation operation)
        {
            operation.AddODataQueryParameter("$count", "boolean", "If true, return the total number of items matched by the filter");
            operation.AddODataQueryParameter("$filter", "string", "An OData filter describing the entities to be returned");
            operation.AddODataQueryParameter("$orderby", "string", "A comma-separated list of ordering instructions.  Each ordering instruction is a field name with an optional direction (asc or desc).");
            operation.AddODataQueryParameter("$select", "string", "A comma-separated list of fields to be returned in the result set.");
            operation.AddODataQueryParameter("$skip", "integer", "The number of items in the list to skip for paging support.");
            operation.AddODataQueryParameter("$top", "integer", "The number of items in the list to return for paging support.");
            operation.AddODataQueryParameter("__includedeleted", "boolean", "If true, soft-deleted items are returned as well as non-deleted items.");
        }

        /// <summary>
        /// Adds or replaces a response with JSON content and an ETag.
        /// </summary>
        /// <param name="operation">The <see cref="OpenApiOperation"/> to modify.</param>
        /// <param name="statusCode">The HTTP status code to model.</param>
        /// <param name="description">The description of the HTTP status code.</param>
        /// <param name="schema">The schema of the entity to return.</param>
        internal static void AddResponseWithContent(this OpenApiOperation operation, string statusCode, string description, OpenApiSchema schema)
        {
            var response = new OpenApiResponse
            {
                Description = description,
                Content = new Dictionary<string, OpenApiMediaType>
                {
                    [JsonMediaType] = new OpenApiMediaType { Schema = schema }
                }
            };
            var etagDescription = statusCode == "409" || statusCode == "412"
                ? "The opaque versioning identifier of the conflicting entity"
                : "The opaque versioning identifier of the entity";
            response.Headers.Add("ETag", new OpenApiHeader
            {
                Schema = new OpenApiSchema { Type = "string" },
                Description = $"{etagDescription}, per RFC 9110 8.8.3."
            });
            operation.Responses[statusCode] = response;
        }

        /// <summary>
        /// Adds or replaces the 409/412 Conflict/Precondition Failed response.
        /// </summary>
        /// <param name="operation">The <see cref="OpenApiOperation"/> to modify.</param>
        /// <param name="schema">The schema of the entity to return.</param>
        internal static void AddConflictResponse(this OpenApiOperation operation, OpenApiSchema schema)
        {
            operation.AddResponseWithContent("409", "Conflict", schema);
            operation.AddResponseWithContent("412", "Precondition failed", schema);
        }

        /// <summary>
        /// Makes the system properties in the schema read-only.
        /// </summary>
        /// <param name="schema">The <see cref="OpenApiSchema"/> to edit.</param>
        public static void MakeSystemPropertiesReadonly(this OpenApiSchema schema)
        {
            foreach (var property in schema.Properties)
            {
                if (SystemProperties.Contains(property.Key))
                {
                    property.Value.ReadOnly = true;
                }
            }
        }
    }
}
