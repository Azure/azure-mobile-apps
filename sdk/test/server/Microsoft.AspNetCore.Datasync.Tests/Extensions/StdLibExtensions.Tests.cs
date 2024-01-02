// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Datasync.Extensions;
using System.Text.Json;

namespace Microsoft.AspNetCore.Datasync.Tests.Extensions;

[ExcludeFromCodeCoverage]
public class StdLibExtensions_Tests
{
    [Fact]
    public void ToJsonString_Null_Works()
    {
        object sut = null;
        string actual = sut.ToJsonString();
        actual.Should().Be("null");
    }

    [Fact]
    public void ToJsonString_ITableData_Works()
    {
        TableData sut = new()
        {
            Id = "0f89592b-6e41-4fe1-abf2-ceee073a6d53",
            Deleted = false,
            Version = new byte[] { 0x61, 0x62, 0x63, 0x64 },
            UpdatedAt = new DateTimeOffset(2022, 10, 21, 7, 28, 0, TimeSpan.Zero)
        };
        const string expected = "{\"$id\":\"1\",\"Id\":\"0f89592b-6e41-4fe1-abf2-ceee073a6d53\",\"Deleted\":false,\"UpdatedAt\":\"2022-10-21T07:28:00.000Z\",\"Version\":\"YWJjZA==\"}";
        string actual = sut.ToJsonString();
        actual.Should().Be(expected);
    }

    [Fact]
    public void ToJsonString_Catches_UnserializableObjects()
    {
        JsonSerializerOptions options = Datasync.Extensions.StdLibExtensions.GetSerializerOptions();
        options.ReferenceHandler = null;
        TestObject sut = new(); sut.Arg = sut;  // Circular references *should* cause an exception.
        string actual = sut.ToJsonString(options);
        actual.Should().Be("unserializable object");
    }

    class TestObject
    {
        public TestObject Arg { get; set; }
    }
}
