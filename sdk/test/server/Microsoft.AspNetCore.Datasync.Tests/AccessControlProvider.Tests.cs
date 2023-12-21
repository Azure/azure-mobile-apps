// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using AnyClone;
using Microsoft.AspNetCore.Datasync.Abstractions;

namespace Microsoft.AspNetCore.Datasync.Tests;

[ExcludeFromCodeCoverage]
public class AccessControlProvider_Tests
{
    [Theory]
    [InlineData(TableOperation.Create)]
    [InlineData(TableOperation.Delete)]
    [InlineData(TableOperation.Read)]
    [InlineData(TableOperation.Query)]
    [InlineData(TableOperation.Update)]
    [SuppressMessage("Performance", "CA1859:Use concrete types when possible for improved performance", Justification = "Part of the test.")]
    public async Task Defaults_DontChange(TableOperation operation)
    {
        IAccessControlProvider<TableData> sut = new AccessControlProvider<TableData>();

        sut.GetDataView().Should().BeNull();

        TableData entity = new() { Id = "abc" };
        TableData clone = CloneExtensions.Clone(entity);

        (await sut.IsAuthorizedAsync(operation, entity)).Should().BeTrue();

        await sut.PreCommitHookAsync(operation, entity);
        entity.Should().BeEquivalentTo(clone);
        await sut.PostCommitHookAsync(operation, entity);
        entity.Should().BeEquivalentTo(clone);
    }
}
