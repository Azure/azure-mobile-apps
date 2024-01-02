// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Datasync.Filters;
using NJsonSchema;
using NSwag;
using NSwag.Generation.Processors;
using NSwag.Generation.Processors.Contexts;
using System.Net;
using System.Text.RegularExpressions;

namespace Microsoft.AspNetCore.Datasync.NSwag;

/// <summary>
/// Implementation of the <see cref="IOperationProcessor"/> for handing Azure Mobile Apps
/// table controllers.
/// </summary>
public class DatasyncOperationProcessor : IOperationProcessor
{
    /// <summary>
    /// The name of the <c>ETag</c> header.
    /// </summary>
    private const string etagHeader = "ETag";

    /// <summary>
    /// The name of the JSON media type.
    /// </summary>
    private const string jsonMediaType = "application/json";

    /// <summary>
    /// The list of HTTP status codes that return a schema.
    /// </summary>
    private static readonly HttpStatusCode[] schemaCodes = new[] { HttpStatusCode.OK, HttpStatusCode.Created, HttpStatusCode.Conflict, HttpStatusCode.PreconditionFailed };

    private static readonly QueryParameter[] odataQueryParameters = new[]
    {
        new QueryParameter { Name = "$count", Description = "Include the total count of entities that match the query", Type = JsonObjectType.Boolean },
        new QueryParameter { Name = "$filter", Description = "Filter the results using an OData filter expression", Type = JsonObjectType.String },
        new QueryParameter { Name = "$orderby", Description = "Sort the results using an OData sort expression", Type = JsonObjectType.String },
        new QueryParameter { Name = "$select", Description = "Select only the specified comma-delimited properties", Type = JsonObjectType.String },
        new QueryParameter { Name = "$skip", Description = "Skip the first N results", Type = JsonObjectType.Integer },
        new QueryParameter { Name = "$top", Description = "Return only the first N results", Type = JsonObjectType.Integer },
        new QueryParameter { Name = "__includedeleted", Description = "Include deleted entities in the results", Type = JsonObjectType.Boolean }
    };

    /// <inheritdoc/>
    public bool Process(OperationProcessorContext context)
    {
        if (IsTableController(context.ControllerType))
        {
            ProcessDatasyncOperation(context);
        }
        return true;
    }

    /// <summary>
    /// Determines if the controller type provided is a datasync table controller.
    /// </summary>
    /// <param name="type">The type of the table controller.</param>
    /// <returns><c>true</c> if the provided controller is a table controller; <c>false</c> otherwise.</returns>
    internal static bool IsTableController(Type type)
       => type.BaseType?.IsGenericType == true && !type.IsAbstract && type.GetCustomAttributes(typeof(DatasyncControllerAttribute), true).Length != 0;

    /// <summary>
    /// Processes a single operation for a table controller.
    /// </summary>
    /// <param name="context">The operation processor context containing the current details about the operation.</param>
    internal static void ProcessDatasyncOperation(OperationProcessorContext context)
    {
        string method = context.OperationDescription.Method.ToUpperInvariant();
        string path = context.OperationDescription.Path;

        if (path.EndsWith("/{id}"))
        {
            switch (method)
            {
                case "DELETE":
                    AddConditionalRequestSupport(context);
                    AddExpectedResponses(context, new[] { HttpStatusCode.NoContent, HttpStatusCode.NotFound, HttpStatusCode.Gone });
                    break;
                case "GET":
                    AddConditionalRequestSupport(context);
                    AddExpectedResponses(context, new[] { HttpStatusCode.OK, HttpStatusCode.NotFound }, GetEntitySchemaReference(context));
                    break;
                case "PUT":
                    AddConditionalRequestSupport(context);
                    AddExpectedResponses(context, new[] { HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.NotFound, HttpStatusCode.Gone }, GetEntitySchemaReference(context));
                    break;
            }
        }
        else
        {
            switch (method)
            {
                case "GET":
                    AddODataQueryParameters(context);
                    AddExpectedResponses(context, new[] { HttpStatusCode.OK, HttpStatusCode.BadRequest }, GetEntityListSchema(context), includeETagHeader: false);
                    break;
                case "POST":
                    AddConditionalRequestSupport(context);
                    AddExpectedResponses(context, new[] { HttpStatusCode.OK, HttpStatusCode.BadRequest }, GetEntitySchemaReference(context));
                    break;
            }
        }
    }

    /// <summary>
    /// Adds support for the <c>If-Match</c>, <c>If-None-Match</c>, and related conditional request headers
    /// for this operation.
    /// </summary>
    /// <param name="context">The operation processor context</param>
    internal static void AddConditionalRequestSupport(OperationProcessorContext context)
    {
        string method = context.OperationDescription.Method.ToUpperInvariant();
        string headerName = method == "GET" ? "If-None-Match" : "If-Match";
        string description = method == "GET" ? "does not match" : "matches";
        JsonSchema entitySchema = GetEntitySchemaReference(context);

        if (!context.OperationDescription.Operation.Parameters.Any(p => IsHeader(p, headerName)))
        {
            context.OperationDescription.Operation.Parameters.Add(new OpenApiParameter
            {
                Name = headerName,
                Kind = OpenApiParameterKind.Header,
                Description = $"Conditionally execute only if the entity version {description} the provided string (RFC 9110 section 13.1).",
                Schema = new JsonSchema { Type = JsonObjectType.String },
                IsNullableRaw = true
            });
        }
        AddExpectedResponses(context, new[] { HttpStatusCode.Conflict, HttpStatusCode.PreconditionFailed }, entitySchema);
    }

    /// <summary>
    /// Determines if the <see cref="OpenApiParameter"/> is a header with the specified name.
    /// </summary>
    /// <param name="parameter">The parameter to check.</param>
    /// <param name="name">The expected name of the header.</param>
    /// <returns><c>true</c> if there is a match; <c>false</c> otherwise.</returns>
    internal static bool IsHeader(OpenApiParameter parameter, string name)
        => parameter.Name == name && parameter.Kind == OpenApiParameterKind.Header;

    /// <summary>
    /// Adds the list of expected responses to the operation.  If the operation response is OK or Created, the
    /// appropriate entity reference is added to the response.
    /// </summary>
    /// <param name="context">The operation processor context.</param>
    /// <param name="codes">The list of HTTP status codes to include.</param>
    /// <param name="schema">The schema to include for OK and Created status codes.</param>
    /// <param name="includeETagHeader">If <c>true</c>, the <c>ETag</c> header is included in the response.</param>
    internal static void AddExpectedResponses(OperationProcessorContext context, IEnumerable<HttpStatusCode> codes, JsonSchema? schema = null, bool includeETagHeader = true)
    {
        foreach (HttpStatusCode code in codes)
        {
            _ = AddExpectedResponse(context, code, schema, includeETagHeader);
        }
    }

    /// <summary>
    /// Adds the expected response to the operation.  If the operation response is OK or Created, the appropriate entity
    /// reference is added to the response.
    /// </summary>
    /// <param name="context">The operation processor context.</param>
    /// <param name="code">The HTTP status code to include.</param>
    /// <param name="schema">The schema to include for OK and Created status codes.</param>
    /// <param name="includeETagHeader">If <c>true</c>, the <c>ETag</c> header is included in the response.</param>
    internal static OpenApiResponse AddExpectedResponse(OperationProcessorContext context, HttpStatusCode code, JsonSchema? schema = null, bool includeETagHeader = true)
    {
        OpenApiResponse response = new()
        {
            Description = Regex.Replace(code.ToString(), "(?<=[a-z])([A-Z])", " $1", RegexOptions.Compiled)
        };
        if (schema != null && schemaCodes.Contains(code))
        {
            response.Content.Add(jsonMediaType, new OpenApiMediaType { Schema = schema });
            if (includeETagHeader)
            {
                response.Headers.Add(etagHeader, new OpenApiHeader { Type = JsonObjectType.String, Description = "The version string of the server entity, per RFC 9110", });
            }
        }
        context.OperationDescription.Operation.Responses[((int)code).ToString()] = response;
        return response;
    }

    /// <summary>
    /// Adds the OData query parameters supported by the GET table operation.
    /// </summary>
    /// <param name="context">The operation processor context</param>
    internal static void AddODataQueryParameters(OperationProcessorContext context)
    {
        var operation = context.OperationDescription.Operation;
        foreach (QueryParameter queryParameter in odataQueryParameters)
        {
            operation.Parameters.Add(new OpenApiParameter
            {
                Name = queryParameter.Name,
                Description = queryParameter.Description,
                Kind = OpenApiParameterKind.Query,
                IsRequired = false,
                Type = queryParameter.Type
            });
        }
    }

    /// <summary>
    /// Retrieves the <see cref="Type"/> for the entity being handled by the controller.
    /// </summary>
    /// <param name="controllerType">The <see cref="Type"/> for the controller.</param>
    /// <returns>The <see cref="Type"/> for the entity.</returns>
    /// <exception cref="ArgumentException">If the controller is not based on a generic controller.</exception>
    internal static Type GetEntityType(Type controllerType)
        => controllerType.BaseType?.GetGenericArguments().FirstOrDefault() ?? throw new ArgumentException("Cannot get the entity schema for a non-generic controller type.");

    /// <summary>
    /// Gets the schema for the entity as a reference schema.  If necessary, creates the schema first.
    /// </summary>
    /// <param name="context">The operation processor context.</param>
    /// <returns>The <see cref="JsonSchema"/> with a reference to the actual schema.</returns>
    /// <exception cref="ArgumentException">If the generic type cannot be determined.</exception>
    internal static JsonSchema GetEntitySchemaReference(OperationProcessorContext context)
    {
        Type entityType = GetEntityType(context.ControllerType);
        string schemaName = context.SchemaGenerator.Settings.SchemaNameGenerator.Generate(entityType);
        if (!context.Document.Definitions.TryGetValue(schemaName, out JsonSchema? value))
        {
            value = context.SchemaGenerator.Generate(entityType);
            context.Document.Definitions.Add(schemaName, value);
        }
        JsonSchema actualSchema = value;
        return new JsonSchema { Reference = actualSchema };
    }

    /// <summary>
    /// Generates the schema for a list of entities, according to the <see cref="PagedResult"/> model.
    /// </summary>
    /// <param name="context">The operation processor context.</param>
    /// <returns>The <see cref="JsonSchema"/> for the list of entities result.</returns>
    /// <exception cref="ArgumentException">If the generic type cannot be determined.</exception>
    internal static JsonSchema GetEntityListSchema(OperationProcessorContext context)
    {
        Type entityType = GetEntityType(context.ControllerType);
        string entityName = entityType.Name;
        JsonSchema entitySchema = GetEntitySchemaReference(context);

        JsonSchema listSchemaRef = new() { Type = JsonObjectType.Object, Description = $"A page of {entityName} entities" };
        listSchemaRef.Properties["items"] = new JsonSchemaProperty
        {
            Description = "The entities in this page of results",
            Type = JsonObjectType.Array,
            Item = entitySchema,
            IsReadOnly = true,
            IsNullableRaw = true
        };
        listSchemaRef.Properties["count"] = new JsonSchemaProperty
        {
            Description = "The total number of entities that match the query (without paging)",
            Type = JsonObjectType.Integer,
            IsReadOnly = true,
            IsNullableRaw = true
        };
        listSchemaRef.Properties["nextLink"] = new JsonSchemaProperty
        {
            Description = "The query to retrieve the next page of results.",
            Type = JsonObjectType.String,
            IsReadOnly = true,
            IsNullableRaw = true
        };
        return listSchemaRef;
    }

    /// <summary>
    /// The structure of the query parameters supported by the GET table operation.
    /// </summary>
    internal readonly struct QueryParameter
    {
        internal string Name { get; init; }
        internal string Description { get; init; }
        internal JsonObjectType Type { get; init; }
    }
}
