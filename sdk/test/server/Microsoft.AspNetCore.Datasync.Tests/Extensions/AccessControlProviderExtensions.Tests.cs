// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Datasync.Abstractions;
using Microsoft.AspNetCore.Datasync.Extensions;
using System.Linq.Expressions;

namespace Microsoft.AspNetCore.Datasync.Tests.Extensions;

[ExcludeFromCodeCoverage]
public class AccessControlProviderExtensions_Tests
{
    [Fact]
    public void EntityIsInView_NullDataView_Works()
    {
        TableData entity = new() { Id = "1" };
        IAccessControlProvider<TableData> sut = Substitute.For<IAccessControlProvider<TableData>>();
        Expression<Func<TableData, bool>> expr = null;
        sut.GetDataView().Returns(expr);

        sut.EntityIsInView(entity).Should().BeTrue();
    }

    [Theory]
    [InlineData("inview", true)]
    [InlineData("notinview", false)]
    public void EntityIsInView_Works(string id, bool expected)
    {
        TableData entity = new() { Id = id };
        IAccessControlProvider<TableData> sut = Substitute.For<IAccessControlProvider<TableData>>();
        Expression<Func<TableData, bool>> expr = model => model.Id == "inview";
        sut.GetDataView().Returns(expr);

        sut.EntityIsInView(entity).Should().Be(expected);
    }
}
