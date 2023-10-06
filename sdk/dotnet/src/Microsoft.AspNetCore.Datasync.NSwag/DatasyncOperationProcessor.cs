// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using NJsonSchema;
using NSwag.Generation.Processors.Contexts;
using NSwag.Generation.Processors;
using System.Net;
using System;
using System.Linq;
using Microsoft.AspNetCore.Datasync.Filters;
using System.Reflection;

namespace Microsoft.AspNetCore.Datasync.NSwag
{
    /// <summary>
    /// Implements an <see cref="IOperationProcessor"/> for handling datasync table controllers.
    /// </summary>
    public class DatasyncOperationProcessor : IOperationProcessor
    {
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
        /// <returns><c>true</c> if the type is a datasync table controller.</returns>
        private static bool IsTableController(Type type)
            => type.BaseType?.IsGenericType == true && !type.IsAbstract
            && type.GetCustomAttributes().Any(s => s.GetType() == typeof(DatasyncControllerAttribute));

        private static void ProcessDatasyncOperation(OperationProcessorContext context)
        {
            var operation = context.OperationDescription.Operation;
            var method = context.OperationDescription.Method;
            var path = context.OperationDescription.Path;
            Type entityType = context.ControllerType.BaseType?.GetGenericArguments().FirstOrDefault()
                ?? throw new ArgumentException("Cannot process a non-generic table controller");
            JsonSchema entitySchemaRef = GetEntityReference(context, entityType);

            operation.AddDatasyncRequestHeaders();
            if (method.Equals("DELETE", StringComparison.InvariantCultureIgnoreCase))
            {
                operation.AddConditionalRequestSupport(entitySchemaRef);
                operation.SetResponse(HttpStatusCode.NoContent);
                operation.SetResponse(HttpStatusCode.NotFound);
                operation.SetResponse(HttpStatusCode.Gone);
            }

            if (method.Equals("GET", StringComparison.InvariantCultureIgnoreCase) && path.EndsWith("/{id}"))
            {
                operation.AddConditionalRequestSupport(entitySchemaRef, true);
                operation.SetResponse(HttpStatusCode.OK, entitySchemaRef);
                operation.SetResponse(HttpStatusCode.NotFound);
            }

            if (method.Equals("GET", StringComparison.InvariantCultureIgnoreCase) && !path.EndsWith("/{id}"))
            {
                operation.AddODataQueryParameters();
                operation.SetResponse(HttpStatusCode.OK, CreateListSchema(entitySchemaRef, entityType.Name), false);
                operation.SetResponse(HttpStatusCode.BadRequest);
            }

            if (method.Equals("PATCH", StringComparison.InvariantCultureIgnoreCase))
            {
                operation.AddConditionalRequestSupport(entitySchemaRef);
                operation.SetResponse(HttpStatusCode.OK, entitySchemaRef);
                operation.SetResponse(HttpStatusCode.BadRequest);
                operation.SetResponse(HttpStatusCode.NotFound);
                operation.SetResponse(HttpStatusCode.Gone);
            }

            if (method.Equals("POST", StringComparison.InvariantCultureIgnoreCase))
            {
                operation.AddConditionalRequestSupport(entitySchemaRef, true);
                operation.SetResponse(HttpStatusCode.Created, entitySchemaRef);
                operation.SetResponse(HttpStatusCode.BadRequest);
            }

            if (method.Equals("PUT", StringComparison.InvariantCultureIgnoreCase))
            {
                operation.AddConditionalRequestSupport(entitySchemaRef);
                operation.SetResponse(HttpStatusCode.OK, entitySchemaRef);
                operation.SetResponse(HttpStatusCode.BadRequest);
                operation.SetResponse(HttpStatusCode.NotFound);
                operation.SetResponse(HttpStatusCode.Gone);
            }
        }

        /// <summary>
        /// Either reads or generates the required entity type schema.
        /// </summary>
        /// <param name="context">The context for the operation processor.</param>
        /// <param name="entityType">The entity type needed.</param>
        /// <returns>A reference to the entity schema.</returns>
        private static JsonSchema GetEntityReference(OperationProcessorContext context, Type entityType)
        {
            var schemaName = context.SchemaGenerator.Settings.SchemaNameGenerator.Generate(entityType);
            if (!context.Document.Definitions.ContainsKey(schemaName))
            {
                var newSchema = context.SchemaGenerator.Generate(entityType);
                context.Document.Definitions.Add(schemaName, newSchema);
            }

            var actualSchema = context.Document.Definitions[schemaName];
            return new JsonSchema { Reference = actualSchema };
        }

        /// <summary>
        /// Creates the paged item schema reference.
        /// </summary>
        /// <param name="entitySchema">The entity schema reference.</param>
        /// <returns>The list schema reference</returns>
        private static JsonSchema CreateListSchema(JsonSchema entitySchema, string entityName)
        {
            var listSchemaRef = new JsonSchema
            {
                Description = $"A page of {entityName} entities",
                Type = JsonObjectType.Object
            };
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
                Description = "The count of all entities in the result set",
                Type = JsonObjectType.Integer,
                IsReadOnly = true,
                IsNullableRaw = true
            };
            listSchemaRef.Properties["nextLink"] = new JsonSchemaProperty
            {
                Description = "The URI to the next page of entities",
                Type = JsonObjectType.String,
                Format = "uri",
                IsReadOnly = true,
                IsNullableRaw = true
            };
            return listSchemaRef;
        }
    }
}
