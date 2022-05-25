// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using Swashbuckle.Swagger;

namespace Microsoft.Azure.Mobile.Server.Swagger
{
    public class MobileAppSchemaFilter : ISchemaFilter
    {
        [CLSCompliant(false)]
        public void Apply(Schema schema, SchemaRegistry schemaRegistry, Type type)
        {
            if (schema == null)
            {
                throw new ArgumentNullException("schema");
            }

            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            foreach (PropertyInfo prop in type.GetProperties())
            {
                foreach (var attr in prop.GetCustomAttributes(true))
                {
                    if (typeof(DatabaseGeneratedAttribute).IsAssignableFrom(attr.GetType()))
                    {
                        schema.properties[prop.Name].readOnly = true;
                    }
                }
            }
        }
    }
}