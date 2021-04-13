// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Http;
using Microsoft.AzureMobile.Server.Tables;
using Xunit;

namespace Microsoft.AzureMobile.Server.Test.Models
{
    [ExcludeFromCodeCoverage(Justification = "Test suite")]
    public class ZumoVersion_Tests
    {
        [Theory]
        [InlineData(null, ZumoVersion.V2, false)]
        [InlineData("somestring", ZumoVersion.V2, false)]
        [InlineData(ZumoVersion.Invalid, ZumoVersion.V2, false)]
        [InlineData(1, ZumoVersion.V2, false)]
        [InlineData(ZumoVersion.V2, ZumoVersion.V2, true)]
        [InlineData(ZumoVersion.V3, ZumoVersion.V2, false)]
        public void IsZumoVersionTests(object contents, ZumoVersion comparison, bool expected)
        {
            // Arrange
            var context = new DefaultHttpContext();
            if (contents != null) context.Items["ZumoVersion"] = contents;

            // Act
            var actual = context.Request.IsZumoVersion(comparison);

            // Assert
            Assert.Equal(expected, actual);
        }
    }
}
