// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Query.OData;
using Microsoft.Datasync.Client.Test.Helpers;

namespace Microsoft.Datasync.Client.Test.Query;

[ExcludeFromCodeCoverage]
public class EdmTypeSupport_Tests
{
    [Fact]
    public void ToODataString_DateOnly()
    {
        var dateOnly = new DateOnly(2022, 2, 14);
        var expected = "cast(2022-02-14,Edm.Date)";
        var actual = EdmTypeSupport.ToODataString(dateOnly);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void ToODataString_TimeOnly()
    {
        var timeOnly = new TimeOnly(2, 14, 30);
        var expected = "cast(02:14:30,Edm.TimeOfDay)";
        var actual = EdmTypeSupport.ToODataString(timeOnly);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void ToODataString_Guid()
    {
        var guid = Guid.NewGuid();
        var expected = $"cast({guid:D},Edm.Guid)";
        var actual = EdmTypeSupport.ToODataString(guid);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void ToODataString_DateTime()
    {
        var dt = DateTime.Parse("2021-03-01T07:30:22.000+00:00");
        var expected = $"cast(2021-03-01T07:30:22.000Z,Edm.DateTimeOffset)";
        var actual = EdmTypeSupport.ToODataString(dt);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void ToODataString_DateTimeOffset()
    {
        var dt = DateTimeOffset.Parse("2021-03-01T07:30:22.000+00:00");
        var expected = $"cast(2021-03-01T07:30:22.000Z,Edm.DateTimeOffset)";
        var actual = EdmTypeSupport.ToODataString(dt);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void ToODataString_Unknown()
    {
        var sut = new IdEntity { Id = "1234" };
        var actual = EdmTypeSupport.ToODataString(sut);
        Assert.Null(actual);
    }
}
