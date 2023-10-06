// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Datasync.Filters;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

namespace Microsoft.AspNetCore.Datasync
{
    /// <summary>
    /// An <see cref="IDocumentFilter"/> that adds the relevant schema and paramter definitions
    /// to generate an OpenAPI v3.0.3 definition for Datasync <see cref="TableController{TEntity}"/>
    /// controllers.
    /// </summary>
    public class DatasyncDocumentFilter : IDocumentFilter
    {
        /// <summary>
        /// The list of operation types.
        /// </summary>
        private enum OpType
        {
            Create,
            Delete,
            GetById,
            List,
            Patch,
            Replace
        }

        /// <summary>
        /// The list of entity names that have already had their schema adjusted.
        /// </summary>
        private readonly List<string> processedEntityNames = new();

        /// <summary>
        /// The assembly to query for TableController instances, if any.  If none is provided, the calling assembly is queried.
        /// </summary>
        private readonly Assembly assemblyToQuery = null;

        /// <summary>
        /// Creates a new <see cref="DatasyncDocumentFilter"/>.
        /// </summary>
        /// <param name="assemblyToQuery">The assembly to query for TableController instances, if any.  If none is provided, the calling assembly is queried.</param>
        public DatasyncDocumentFilter(Assembly assemblyToQuery = null)
        {
            this.assemblyToQuery = assemblyToQuery;
        }

        /// <summary>
        /// Applies the necessary changes to the <see cref="OpenApiDocument"/>.
        /// </summary>
        /// <param name="document">The <see cref="OpenApiDocument"/> to edit.</param>
        /// <param name="context">The filter context.</param>
        public void Apply(OpenApiDocument document, DocumentFilterContext context)
        {
            var controllers = GetAllTableControllers(assemblyToQuery);

            foreach (Type controller in controllers)
            {
                if (controller == null)
                    continue;

                var entityType = controller.BaseType?.GenericTypeArguments[0];
                if (entityType == null)
                    continue;

                var routePath = context.ApiDescriptions.FirstOrDefault(m => IsApiDescriptionForController(m, controller))?.RelativePath;
                if (routePath == null)
                    continue;
                var allEntitiesPath = $"/{routePath}";
                var singleEntityPath = $"/{routePath}/{{id}}";

                // Get the various operations
                Dictionary<OpType, OpenApiOperation> operations = new()
                {
                    { OpType.Create, document.Paths[allEntitiesPath].Operations[OperationType.Post] },
                    { OpType.Delete, document.Paths[singleEntityPath].Operations[OperationType.Delete] },
                    { OpType.GetById, document.Paths[singleEntityPath].Operations[OperationType.Get] },
                    { OpType.List, document.Paths[allEntitiesPath].Operations[OperationType.Get] },
                    { OpType.Patch, document.Paths[singleEntityPath].Operations[OperationType.Patch] },
                    { OpType.Replace, document.Paths[singleEntityPath].Operations[OperationType.Put] }
                };

                // Make the system properties in the entity read-only
                if (!processedEntityNames.Contains(entityType.Name))
                {
                    context.SchemaRepository.Schemas[entityType.Name].MakeSystemPropertiesReadonly();
                    context.SchemaRepository.Schemas[entityType.Name].UnresolvedReference = false;
                    context.SchemaRepository.Schemas[entityType.Name].Reference = new OpenApiReference
                    {
                        Id = entityType.Name,
                        Type = ReferenceType.Schema
                    };
                }

                Type listEntityType = typeof(Page<>).MakeGenericType(entityType);
                OpenApiSchema listSchemaRef = context.SchemaRepository.Schemas.GetValueOrDefault(listEntityType.Name)
                    ?? context.SchemaGenerator.GenerateSchema(listEntityType, context.SchemaRepository);

                foreach (var operation in operations)
                {
                    // All Operations must have an ZUMO-API-VERSION header as a parameter
                    operation.Value.AddZumoApiVersionHeader();

                    // Each operation also has certain modifications.
                    switch (operation.Key)
                    {
                        case OpType.Create:
                            // Request Edits
                            operation.Value.AddConditionalHeader(true);

                            // Response Edits
                            operation.Value.AddResponseWithContent("201", "Created", context.SchemaRepository.Schemas[entityType.Name]);
                            operation.Value.Responses["400"] = new OpenApiResponse { Description = "Bad Request" };
                            operation.Value.AddConflictResponse(context.SchemaRepository.Schemas[entityType.Name]);
                            break;

                        case OpType.Delete:
                            // Request Edits
                            operation.Value.AddConditionalHeader();

                            // Response Edits
                            operation.Value.Responses["204"] = new OpenApiResponse { Description = "No Content" };
                            operation.Value.Responses["404"] = new OpenApiResponse { Description = "Not Found" };
                            operation.Value.Responses["410"] = new OpenApiResponse { Description = "Gone" };
                            operation.Value.AddConflictResponse(context.SchemaRepository.Schemas[entityType.Name]);
                            break;

                        case OpType.GetById:
                            // Request Edits
                            operation.Value.AddConditionalHeader(true);

                            // Response Edits
                            operation.Value.AddResponseWithContent("200", "OK", context.SchemaRepository.Schemas[entityType.Name]);
                            operation.Value.Responses["304"] = new OpenApiResponse { Description = "Not Modified" };
                            operation.Value.Responses["404"] = new OpenApiResponse { Description = "Not Found" };
                            break;

                        case OpType.List:
                            // Request Edits
                            operation.Value.AddODataQueryParameters();

                            // Response Edits
                            operation.Value.AddResponseWithContent("200", "OK", listSchemaRef);
                            operation.Value.Responses["400"] = new OpenApiResponse { Description = "Bad Request" };
                            break;

                        case OpType.Patch:
                            // Request Edits
                            operation.Value.AddConditionalHeader();

                            // Response Edits
                            operation.Value.AddResponseWithContent("200", "OK", context.SchemaRepository.Schemas[entityType.Name]);
                            operation.Value.Responses["400"] = new OpenApiResponse { Description = "Bad Request" };
                            operation.Value.Responses["404"] = new OpenApiResponse { Description = "Not Found" };
                            operation.Value.Responses["410"] = new OpenApiResponse { Description = "Gone" };
                            operation.Value.AddConflictResponse(context.SchemaRepository.Schemas[entityType.Name]);
                            break;

                        case OpType.Replace:
                            // Request Edits
                            operation.Value.AddConditionalHeader();

                            // Response Edits
                            operation.Value.AddResponseWithContent("200", "OK", context.SchemaRepository.Schemas[entityType.Name]);
                            operation.Value.Responses["400"] = new OpenApiResponse { Description = "Bad Request" };
                            operation.Value.Responses["404"] = new OpenApiResponse { Description = "Not Found" };
                            operation.Value.Responses["410"] = new OpenApiResponse { Description = "Gone" };
                            operation.Value.AddConflictResponse(context.SchemaRepository.Schemas[entityType.Name]);
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Determines if the controller type is represented by the API Description.
        /// </summary>
        /// <param name="description">The <see cref="ApiDescription"/> being handled.</param>
        /// <param name="controllerType">The type of the controller being used.</param>
        /// <returns><c>true</c> if the Api description represents the controller.</returns>
        internal static bool IsApiDescriptionForController(ApiDescription description, Type controllerType)
        {
            if (description.TryGetMethodInfo(out MethodInfo methodInfo))
            {
                return methodInfo.ReflectedType == controllerType && (methodInfo.Name.Equals("GetAsync") || methodInfo.Name.Equals("CreateAsync"));
            }
            return false;
        }
        
        /// <summary>
        /// Returns a list of all table controllers in the provided assembly.
        /// </summary>
        /// <param name="assembly">The assembly to query.  Be default, the calling assembly is queried.</param>
        /// <returns>The list of table controllers in the assembly.</returns>
        internal static List<Type> GetAllTableControllers(Assembly assembly = null)
            => (assembly ?? Assembly.GetCallingAssembly()).GetTypes().Where(t => IsTableController(t)).ToList();

        /// <summary>
        /// Determines if the provided type is a table controller.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <returns><c>true</c> if the type is a table controller; <c>false</c> otherwise.</returns>
        internal static bool IsTableController(Type type)
            => type.BaseType?.IsGenericType == true && !type.IsAbstract 
            && type.GetCustomAttributes().Any(s => s.GetType() == typeof(DatasyncControllerAttribute));

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
            public Uri NextLink { get; }
        }
    }
}
