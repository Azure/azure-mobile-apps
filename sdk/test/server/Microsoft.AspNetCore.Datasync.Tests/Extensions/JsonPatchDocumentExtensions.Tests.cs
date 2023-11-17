// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Datasync.Extensions;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Newtonsoft.Json.Linq;
using System.IO;

namespace Microsoft.AspNetCore.Datasync.Tests.Extensions;

[ExcludeFromCodeCoverage]
public class JsonPatchDocumentExtensions_Tests
{
    [Theory]
    [InlineData("replace", "/id", "changed", true)]
    [InlineData("replace", "/id", "test", false)]
    [InlineData("replace", "/updatedAt", "2018-12-31T05:00:00.000Z", true)]
    [InlineData("replace", "/updatedAt", "2021-12-31T05:30:00.000Z", false)]
    [InlineData("replace", "/updatedAt", "some-string", false)]
    [InlineData("replace", "/version", "AQBCIkeP", false)]
    [InlineData("replace", "/version", "dGVzdA==", true)]
    [InlineData("test", "/id", "changed", false)]
    [InlineData("test", "/updatedAt", "2018-12-31T05:00:00.000Z", false)]
    [InlineData("test", "/version", "dGVzdA==", false)]
    public void ModifiesSystemProperties_BasicTests(string op, string path, string value, bool expected)
    {
        JsonPatchDocument<EntityMovie> patchDoc = new();
        if (path.Equals("/updatedAt", StringComparison.OrdinalIgnoreCase) && value.EndsWith(".000Z"))
            patchDoc.Operations.Add(new Operation<EntityMovie>(op, path, null, DateTime.Parse(value)));
        else
            patchDoc.Operations.Add(new Operation<EntityMovie>(op, path, null, value));

        EntityMovie entity = new()
        {
            Id = "test",
            UpdatedAt = DateTimeOffset.Parse("2021-12-31T05:30:00.000Z"),
            Version = new byte[] { 0x01, 0x00, 0x42, 0x22, 0x47, 0x8F }
        };

        bool actual = patchDoc.ModifiesSystemProperties(entity, out Dictionary<string, string[]> validationErrors);

        actual.Should().Be(expected);
        validationErrors.Count.Should().Be(expected ? 2 : 0);
    }

    [Theory]
    [InlineData("replace", "/updatedAt", "2018-12-31T05:00:00.000Z")]
    public void ModifiesSystemProperties_NullUpdatedAt(string op, string path, string value)
    {
        JsonPatchDocument<EntityMovie> patchDoc = new();
        if (path.Equals("/updatedAt", StringComparison.OrdinalIgnoreCase) && value.EndsWith(".000Z"))
            patchDoc.Operations.Add(new Operation<EntityMovie>(op, path, null, DateTime.Parse(value)));
        else
            patchDoc.Operations.Add(new Operation<EntityMovie>(op, path, null, value));

        EntityMovie entity = new()
        {
            Id = "test",
            UpdatedAt = null,
            Version = new byte[] { 0x01, 0x00, 0x42, 0x22, 0x47, 0x8F }
        };

        bool actual = patchDoc.ModifiesSystemProperties(entity, out Dictionary<string, string[]> validationErrors);

        actual.Should().Be(true);
        validationErrors.Count.Should().Be(2);
    }

    [Theory]
    [InlineData("replace", "/id", "test", false)]
    [InlineData("replace", "/updatedAt", "2021-12-31T05:30:00.000Z", false)]
    [InlineData("replace", "/version", "AQBCIkeP", false)]
    [InlineData("add", "/deleted", false, false)]
    [InlineData("replace", "/deleted", true, false)]
    [InlineData("replace", "/deleted", false, true)]
    public void Contains_BasicTests(string op, string path, object value, bool expected)
    {
        JsonPatchDocument<EntityMovie> patchDoc = new();
        patchDoc.Operations.Add(new Operation<EntityMovie>(op, path, null, value));

        bool actual = patchDoc.Contains("replace", "/deleted", false);

        actual.Should().Be(expected);
    }
}
