// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Utils;
using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace Microsoft.Datasync.Client.Test.Utils
{
    [ExcludeFromCodeCoverage(Justification = "Test suite")]
    public class EnumValueAttribute_Test : OldBaseTest
    {
        [Theory]
        [InlineData(DatasyncFeatures.Offline, "OL")]
        [InlineData(DatasyncFeatures.TypedTable, "TT")]
        [InlineData(DatasyncFeatures.UntypedTable, "TU")]
        [Trait("Method", "GetValue")]
        internal void GetValue_ReturnsExpected(DatasyncFeatures feature, string expected)
        {
            Assert.Equal(expected, EnumValueAttribute.GetValue(feature));
        }

        [Theory]
        [InlineData(DatasyncFeatures.None)]
        [InlineData((DatasyncFeatures)0x8000)]
        [Trait("Method", "GetValue")]
        internal void GetValue_ReturnsNull_WithNoAttribute(DatasyncFeatures feature)
        {
            Assert.Null(EnumValueAttribute.GetValue(feature));
        }
    }
}
