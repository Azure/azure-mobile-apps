// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Datasync.Tables;
using Microsoft.AspNetCore.Http;
using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace Microsoft.AspNetCore.Datasync.Test.Models
{
    [ExcludeFromCodeCoverage(Justification = "Test suite")]
    public class ProtocolVersion_Tests
    {
        [Theory]
        [InlineData(null, ProtocolVersion.V2, false)]
        [InlineData("somestring", ProtocolVersion.V2, false)]
        [InlineData(ProtocolVersion.Invalid, ProtocolVersion.V2, false)]
        [InlineData(1, ProtocolVersion.V2, false)]
        [InlineData(ProtocolVersion.V2, ProtocolVersion.V2, true)]
        [InlineData(ProtocolVersion.V3, ProtocolVersion.V2, false)]
        public void IsProtocolVersionTests(object contents, ProtocolVersion comparison, bool expected)
        {
            // Arrange
            var context = new DefaultHttpContext();
            if (contents != null) context.Items["ProtocolVersion"] = contents;

            // Act
            var actual = context.Request.IsProtocolVersion(comparison);

            // Assert
            Assert.Equal(expected, actual);
        }
    }
}
