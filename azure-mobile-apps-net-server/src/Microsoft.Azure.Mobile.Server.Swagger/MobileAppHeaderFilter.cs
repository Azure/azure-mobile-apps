// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Web.Http.Description;
using System.Web.Http.Filters;
using Microsoft.Azure.Mobile.Server.Config;
using Swashbuckle.Swagger;

namespace Microsoft.Azure.Mobile.Server
{
    public class MobileAppHeaderFilter : IOperationFilter
    {
        [CLSCompliant(false)]
        public void Apply(Operation operation, SchemaRegistry schemaRegistry, ApiDescription apiDescription)
        {
            if (operation == null)
            {
                throw new ArgumentNullException("operation");
            }

            if (apiDescription == null)
            {
                throw new ArgumentNullException("apiDescription");
            }

            Collection<IFilter> filters = apiDescription.ActionDescriptor.ControllerDescriptor.GetFilters();
            IEnumerable<IFilter> mobileAppFilter = filters.Where(f => typeof(MobileAppControllerAttribute).IsAssignableFrom(f.GetType()));

            if (mobileAppFilter.Any())
            {
                if (operation.parameters == null)
                {
                    operation.parameters = new List<Parameter>();
                }

                operation.parameters.Add(new Parameter
                {
                    name = "ZUMO-API-VERSION",
                    @in = "header",
                    type = "string",
                    required = true,
                    @default = "2.0.0"
                });
            }
        }
    }
}