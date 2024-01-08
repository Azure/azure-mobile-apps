// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Datasync.Tables;
using Microsoft.Spatial;

namespace Microsoft.AspNetCore.Datasync.Tests.Tables;

[ExcludeFromCodeCoverage]
public class ODataFunctions_Tests
{
    [Fact]
    public void GeoDistance_NullArg_ReturnsNaN()
    {
        GeographyPoint p0 = null;
        GeographyPoint p1 = GeographyPoint.Create(0, 0);

        ODataFunctions.GeoDistance(p0, p1).Should().Be(double.NaN);
        ODataFunctions.GeoDistance(p1, p0).Should().Be(double.NaN);
    }
}
