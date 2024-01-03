using Microsoft.AspNetCore.Datasync.Filters;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Microsoft.AspNetCore.Datasync.Swashbuckle;

/// <summary>
/// An <see cref="IDocumentFilter"/> implementation that adds the relevant schema and
/// parameter definitions to generate an OpenAPI3 definition for an Azure Mobile Apps
/// table controller.
/// </summary>
public class DatasyncDocumentFilter : IDocumentFilter
{
    /// <summary>
    /// The assembly to query for <see cref="TableController{TEntity}"/> instances.
    /// </summary>
    private readonly Assembly assemblyToQuery;

    /// <summary>
    /// The name of the <c>ETag</c> header.
    /// </summary>
    private const string etagHeader = "ETag";

    /// <summary>
    /// The name of the JSON media type.
    /// </summary>
    private const string jsonMediaType = "application/json";

    /// <summary>
    /// The list of OData query parameters supported by the GET table operation.
    /// </summary>
    private readonly QueryParameter[] odataQueryParameters = new[]
    {
        new QueryParameter { Name = "$count", Description = "Include the total count of entities that match the query", Type = "boolean" },
        new QueryParameter { Name = "$filter", Description = "Filter the results using an OData filter expression", Type = "string" },
        new QueryParameter { Name = "$orderby", Description = "Sort the results using an OData sort expression", Type = "string" },
        new QueryParameter { Name = "$select", Description = "Select only the specified comma-delimited properties", Type = "string" },
        new QueryParameter { Name = "$skip", Description = "Skip the first N results", Type = "integer" },
        new QueryParameter { Name = "$top", Description = "Return only the first N results", Type = "integer" },
        new QueryParameter { Name = "__includedeleted", Description = "Include deleted entities in the results", Type = "boolean" }
    };

    /// <summary>
    /// The list of HTTP status codes that return a schema.
    /// </summary>
    private readonly HttpStatusCode[] schemaCodes = new[] { HttpStatusCode.OK, HttpStatusCode.Created, HttpStatusCode.Conflict, HttpStatusCode.PreconditionFailed };

    /// <summary>
    /// The list of system properties within the <see cref="ITableData"/> interface.
    /// </summary>
    private static readonly string[] systemProperties = new[] { "deleted", "updatedAt", "version" };

    /// <summary>
    /// Creates a new instance of the <see cref="DatasyncDocumentFilter"/> class.
    /// </summary>
    /// <param name="assemblyToQuery">The assembly to query for TableController instances.`</param>
    public DatasyncDocumentFilter(Assembly assemblyToQuery)
    {
        this.assemblyToQuery = assemblyToQuery;
    }

    /// <summary>
    /// Updates the OpenApiDocument with the necessary schema and parameter definitions for
    /// each Azure Mobile Apps <see cref="TableController{TEntity}"/>.
    /// </summary>
    /// <param name="document">The <see cref="OpenApiDocument"/> to edit.</param>
    /// <param name="context">The filter context.</param>
    public void Apply(OpenApiDocument document, DocumentFilterContext context)
    {
        foreach (Type controller in assemblyToQuery.GetTypes().Where(t => IsTableController(t)))
        {
            foreach (ApiDescription apiDescription in context.ApiDescriptions.Where(m => IsApiDescriptionForTableController(m, controller)))
            {
                ApplySchemaChangesForOperation(document, context, apiDescription, controller);
            }
        }
    }

    /// <summary>
    /// Processes the provided <see cref="ApiDescription"/> and updates the <see cref="OpenApiDocument"/>
    /// </summary>
    /// <param name="document">The <see cref="OpenApiDocument"/> to edit.</param>
    /// <param name="context">The filter context.</param>
    /// <param name="apiDescription">The <see cref="ApiDescription"/> for the operation.</param>
    /// <param name="controller">The type of the controller.</param>
    internal void ApplySchemaChangesForOperation(OpenApiDocument document, DocumentFilterContext context, ApiDescription apiDescription, Type controller)
    {
        string method = IsNotNull(apiDescription.HttpMethod, nameof(apiDescription));
        string path = IsNotNull(apiDescription.RelativePath, nameof(apiDescription));
        OpenApiOperation operation = document.Paths[$"/{path}"].Operations.First(m => m.Key.ToString().Equals(method, StringComparison.InvariantCultureIgnoreCase)).Value;

        if (path.EndsWith("/{id}"))
        {
            switch (method.ToUpperInvariant())
            {
                case "DELETE":
                    AddConditionalRequestSupport(apiDescription, operation);
                    AddExpectedResponses(operation, new[] { HttpStatusCode.NoContent, HttpStatusCode.NotFound, HttpStatusCode.Gone, HttpStatusCode.Conflict, HttpStatusCode.PreconditionFailed }, GetEntitySchema(context, controller));
                    break;
                case "GET":
                    AddConditionalRequestSupport(apiDescription, operation);
                    AddExpectedResponses(operation, new[] { HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.PreconditionFailed }, GetEntitySchema(context, controller));
                    break;
                case "PUT":
                    AddConditionalRequestSupport(apiDescription, operation);
                    AddExpectedResponses(operation, new[] { HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.NotFound, HttpStatusCode.Gone, HttpStatusCode.Conflict, HttpStatusCode.PreconditionFailed }, GetEntitySchema(context, controller));
                    break;
            }
        }
        else
        {
            switch (method.ToUpperInvariant())
            {
                case "GET":
                    AddODataQueryParameters(operation);
                    AddExpectedResponses(operation, new[] { HttpStatusCode.OK, HttpStatusCode.BadRequest }, GetEntityListSchema(context, controller), includeETagHeader: false);
                    break;
                case "POST":
                    AddConditionalRequestSupport(apiDescription, operation);
                    AddExpectedResponses(operation, new[] { HttpStatusCode.Created, HttpStatusCode.BadRequest, HttpStatusCode.Conflict, HttpStatusCode.PreconditionFailed }, GetEntitySchema(context, controller));
                    break;
            }
        }
    }

    /// <summary>
    /// Adds the conditional request support to the provided <see cref="OpenApiOperation"/>.
    /// </summary>
    /// <param name="apiDescription">The <see cref="ApiDescription"/> for the controller.</param>
    /// <param name="operation">The <see cref="OpenApiOperation"/> referencing the current operation.</param>
    internal static void AddConditionalRequestSupport(ApiDescription apiDescription, OpenApiOperation operation)
    {
        string headerName = IsGetOperation(apiDescription.HttpMethod) ? "If-None-Match" : "If-Match";
        string description = IsGetOperation(apiDescription.HttpMethod) ? "does not match" : "matches";
        operation.Parameters.Add(new OpenApiParameter
        {
            Name = headerName,
            Description = $"Conditionally execute only if the entity version {description} the provided string (RFC 9110 section 13.1).",
            In = ParameterLocation.Header,
            Required = false,
            Schema = new OpenApiSchema { Type = "string" }
        });
    }

    /// <summary>
    /// Adds the list of expected responses to the operation.  If the operation response is OK or Created, the
    /// appropriate entity reference is added to the response.
    /// </summary>
    /// <param name="operation">The <see cref="OpenApiOperation"/> referencing the current operation.</param>
    /// <param name="codes">The list of HTTP status codes to include.</param>
    /// <param name="schema">The schema to include for OK and Created status codes.</param>
    /// <param name="includeETagHeader">If <c>true</c>, the <c>ETag</c> header is included in the response.</param>
    internal void AddExpectedResponses(OpenApiOperation operation, IEnumerable<HttpStatusCode> codes, OpenApiSchema schema, bool includeETagHeader = true)
    {
        foreach (HttpStatusCode code in codes)
        {
            _ = AddExpectedResponse(operation, code, schema, includeETagHeader);
        }
    }

    /// <summary>
    /// Adds the expected response to the operation.  If the operation response is OK or Created, the
    /// appropriate entity reference is added to the response.
    /// </summary>
    /// <param name="operation">The <see cref="OpenApiOperation"/> referencing the current operation.</param>
    /// <param name="code">The HTTP status code to include.</param>
    /// <param name="schema">The schema to include for OK and Created status codes.</param>
    /// <param name="includeETagHeader">If <c>true</c>, the <c>ETag</c> header is included in the response.</param>
    internal OpenApiResponse AddExpectedResponse(OpenApiOperation operation, HttpStatusCode code, OpenApiSchema schema, bool includeETagHeader)
    {
        OpenApiResponse response = new()
        {
            Description = Regex.Replace(code.ToString(), "(?<=[a-z])([A-Z])", " $1", RegexOptions.Compiled)
        };
        if (schemaCodes.Contains(code))
        {
            response.Content = new Dictionary<string, OpenApiMediaType>
            {
                [jsonMediaType] = new OpenApiMediaType { Schema = schema }
            };
            if (includeETagHeader)
            {
                response.Headers.Add(etagHeader, new OpenApiHeader
                {
                    Schema = new OpenApiSchema { Type = "string" },
                    Description = "The version string of the server entity, per RFC 9110"
                });
            }
        }
        operation.Responses[((int)code).ToString()] = response;
        return response;
    }

    /// <summary>
    /// Adds the OData query parameters to the provided <see cref="OpenApiOperation"/>.
    /// </summary>
    /// <param name="operation">The <see cref="OpenApiOperation"/> to edit.</param>
    internal void AddODataQueryParameters(OpenApiOperation operation)
    {
        foreach (QueryParameter queryParameter in odataQueryParameters)
        {
            operation.Parameters.Add(new OpenApiParameter
            {
                Name = queryParameter.Name,
                Description = queryParameter.Description,
                In = ParameterLocation.Query,
                Required = false,
                Schema = new OpenApiSchema { Type = queryParameter.Type }
            });
        }
    }

    /// <summary>
    /// Returns the schema representing the entity for the controller.
    /// </summary>
    /// <param name="context">The Document filter context.</param>
    /// <param name="controller">The type of the controller.</param>
    /// <returns>The schema reference.</returns>
    internal static OpenApiSchema GetEntitySchema(DocumentFilterContext context, Type controller)
    {
        Type entityType = GetEntityType(controller);
        if (!context.SchemaRepository.Schemas.TryGetValue(entityType.Name, out OpenApiSchema? schema))
        {
            context.SchemaGenerator.GenerateSchema(entityType, context.SchemaRepository);
            schema = context.SchemaRepository.Schemas[entityType.Name];
        }

        foreach (KeyValuePair<string, OpenApiSchema> property in schema.Properties)
        {
            if (systemProperties.Contains(property.Key))
            {
                property.Value.ReadOnly = true;
            }
        }
        schema.UnresolvedReference = false;
        schema.Reference = new OpenApiReference { Id = entityType.Name, Type = ReferenceType.Schema };

        return schema;
    }

    internal static OpenApiSchema GetEntityListSchema(DocumentFilterContext context, Type controller)
    {
        Type entityType = GetEntityType(controller);
        Type listEntityType = typeof(Page<>).MakeGenericType(entityType);
        OpenApiSchema listSchemaRef = context.SchemaRepository.Schemas.GetValueOrDefault(listEntityType.Name)
            ?? context.SchemaGenerator.GenerateSchema(listEntityType, context.SchemaRepository);
        return listSchemaRef;
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
    /// Determines if the provided <see cref="ApiDescription"/> is for the provided table controller.  Used
    /// for filtering the list of api descriptions down to just the table controllers.
    /// </summary>
    /// <param name="description">The <see cref="ApiDescription"/> to test.</param>
    /// <param name="controllerType">The <see cref="TableController{TEntity}"/> type being processed.</param>
    /// <returns><c>true</c> if the ApiDescription is for the provided table controller; <c>false</c> otherwise.</returns>
    [ExcludeFromCodeCoverage(Justification = "TryGetMethodInfo always returns true in test environment")]
    internal static bool IsApiDescriptionForTableController(ApiDescription description, Type controllerType)
        => description.TryGetMethodInfo(out MethodInfo methodInfo) && methodInfo.ReflectedType == controllerType;

    /// <summary>
    /// Determines if the provided method string represents a GET operation.
    /// </summary>
    /// <param name="method">The HTTP method.</param>
    /// <returns><c>true</c> if the method is GET; <c>false</c> otherwise.</returns>
    internal static bool IsGetOperation(string? method)
        => method?.Equals("GET", StringComparison.InvariantCultureIgnoreCase) == true;

    /// <summary>
    /// Determines if the provided type is an Azure Mobile Apps table controller.
    /// </summary>
    /// <param name="type">The type to query.</param>
    /// <returns><c>true</c> if the type is a table controller; <c>false</c> otherwise.</returns>
    internal static bool IsTableController(Type type)
         => type.BaseType?.IsGenericType == true && !type.IsAbstract && type.GetCustomAttributes(typeof(DatasyncControllerAttribute), true).Length != 0;

    /// <summary>
    /// Convenience method to throw if the argument provided is null.
    /// </summary>
    internal static string IsNotNull(string? arg, string argName)
        => arg ?? throw new ArgumentNullException(argName);

    /// <summary>
    /// The structure of the query parameters supported by the GET table operation.
    /// </summary>
    internal readonly struct QueryParameter
    {
        internal string Name { get; init; }
        internal string Description { get; init; }
        internal string Type { get; init; }
    }

    /// <summary>
    /// A type representing a single page of entities.
    /// </summary>
    /// <typeparam name="T">The type of the entity.</typeparam>
    [ExcludeFromCodeCoverage(Justification = "Model class - coverage not needed")]
    internal class Page<T>
    {
        /// <summary>
        /// The list of entities in this page of the results.
        /// </summary>
        public IEnumerable<T> Items { get; } = new List<T>();

        /// <summary>
        /// The count of all the entities in the result set.
        /// </summary>
        public long? Count { get; }

        /// <summary>
        /// The URI to the next page of entities.
        /// </summary>
        public string? NextLink { get; }
    }
}
