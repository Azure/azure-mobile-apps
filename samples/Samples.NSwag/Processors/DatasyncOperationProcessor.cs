using Microsoft.AspNetCore.Datasync;
using NSwag;
using NSwag.Generation.Processors;
using NSwag.Generation.Processors.Contexts;
using System.Diagnostics;

namespace Samples.NSwag.Processors
{
    /// <summary>
    /// Implements an <see cref="IOperationProcessor"/> for handling datasync table controllers.
    /// </summary>
    public class DatasyncOperationProcessor : IOperationProcessor
    {
        private static readonly Type tableControllerType = typeof(TableController<>);

        public bool Process(OperationProcessorContext context)
        {
            var baseType = context.ControllerType.BaseType;
            if (baseType?.IsGenericType == true && baseType?.GetGenericTypeDefinition() == tableControllerType)
            {
                ProcessDatasyncOperation(context);
                return true;
            }
            return true;
        }

        private void ProcessDatasyncOperation(OperationProcessorContext context)
        {
            var operation = context.OperationDescription;
            Type entityType = context.ControllerType.BaseType?.GetGenericArguments().FirstOrDefault() ?? throw new ArgumentException("Cannot process a non-generic table controller");

            // operation.AddDatasyncRequestHeaders();
            if (operation.Method.Equals("DELETE", StringComparison.InvariantCultureIgnoreCase))
            {
                //operation.AddConditionalRequestSupport();
                //operation.SetResponse(StatusCodes.Status204NoContent);
                //operation.SetResponse(StatusCodes.Status404NotFound);
                //operation.SetResponse(StatusCodes.Status410Gone);
            }

            if (operation.Method.Equals("GET", StringComparison.InvariantCultureIgnoreCase) && operation.Path.EndsWith("/{id}"))
            {
                //operation.AddConditionalRequestSupport();
                //operation.SetResponse(StatusCodes.Status200OK, entityType);
                //operation.SetResponse(StatusCodes.Status404NotFound);
            }

            if (operation.Method.Equals("GET", StringComparison.InvariantCultureIgnoreCase) && !operation.Path.EndsWith("/{id}"))
            {
                //operation.AddODataQueryParameters();
                //operation.SetResponse(StatusCodes.Status200OK, listSchemaRef);
                //operation.SetResponse(StatusCodes.Status400BadRequest);
            }

            if (operation.Method.Equals("PATCH", StringComparison.InvariantCultureIgnoreCase))
            {
                //operation.AddConditionalRequestSupport();
                //operation.SetResponse(StatusCodes.Status200OK, entityType);
                //operation.SetResponse(StatusCodes.Status400BadRequest);
                //operation.SetResponse(StatusCodes.Status404NotFound);
                //operation.SetResponse(StatusCodes.Status410Gone);
            }

            if (operation.Method.Equals("POST", StringComparison.InvariantCultureIgnoreCase))
            {
                //operation.AddConditionalRequestSupport();
                //operation.SetResponse(StatusCodes.Status201Created, entityType);
                //operation.SetResponse(StatusCodes.Status400BadRequest);
            }

            if (operation.Method.Equals("PUT", StringComparison.InvariantCultureIgnoreCase))
            {
                //operation.AddConditionalRequestSupport();
                //operation.SetResponse(StatusCodes.Status200OK, entityType);
                //operation.SetResponse(StatusCodes.Status400BadRequest);
                //operation.SetResponse(StatusCodes.Status404NotFound);
                //operation.SetResponse(StatusCodes.Status410Gone);
            }
        }
    }
}
