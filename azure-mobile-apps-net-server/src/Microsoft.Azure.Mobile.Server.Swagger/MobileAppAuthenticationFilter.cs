// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Description;
using Swashbuckle.Swagger;

namespace Microsoft.Azure.Mobile.Server.Swagger
{
    public class MobileAppAuthenticationFilter : IOperationFilter
    {
        private string provider;

        public MobileAppAuthenticationFilter(string provider)
        {
            this.provider = provider;
        }

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

            // Correspond each "Authorize" action to an oauth2 scope
            var authorized = apiDescription.ActionDescriptor.GetFilterPipeline()
                .Select(filterInfo => filterInfo.Instance)
                .OfType<AuthorizeAttribute>()
                .Distinct();

            if (authorized.Any())
            {
                if (operation.security == null)
                {
                    operation.security = new List<IDictionary<string, IEnumerable<string>>>();
                }

                var requirements = new Dictionary<string, IEnumerable<string>>
                {
                    { this.provider, new string[] { } }
                };

                operation.security.Add(requirements);
            }
        }
    }
}