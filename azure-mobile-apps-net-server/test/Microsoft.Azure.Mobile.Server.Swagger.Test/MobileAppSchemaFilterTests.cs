// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Swashbuckle.Swagger;
using Xunit;

namespace Microsoft.Azure.Mobile.Server.Swagger.Test
{
    public class MobileAppSchemaFilterTests
    {
        [Fact]
        public void SchemaFilter_SetsPropsToReadOnly()
        {
            // Arrange
            var filter = new MobileAppSchemaFilter();
            var schema = new Schema()
            {
                properties = new Dictionary<string, Schema>()
                {
                    { "ReadWrite", new Schema() },
                    { "ReadOnly", new Schema() }
                }
            };

            // Act
            filter.Apply(schema, null, typeof(TestType));

            // Assert
            Assert.Null(schema.properties["ReadWrite"].readOnly);
            Assert.True(schema.properties["ReadOnly"].readOnly);
        }

        private class TestType
        {
            public string ReadWrite { get; set; }

            [DatabaseGenerated(DatabaseGeneratedOption.None)]
            public string ReadOnly { get; set; }
        }
    }
}