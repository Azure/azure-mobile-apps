// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Test.Helpers;
using Microsoft.Datasync.Client.Utils;

namespace Microsoft.Datasync.Client.Test.Utils;

[ExcludeFromCodeCoverage]
public class Validate_Tests
{
    [Fact]
    [Trait("Method", "IsNotNull(object,string)")]
    public void IsNotNull_Null_Throws()
    {
        object sut = null;
        Assert.Throws<ArgumentNullException>(() => Arguments.IsNotNull(sut, nameof(sut)));
    }

    [Fact]
    [Trait("Method", "IsNotNull(object,string)")]
    public void IsNotNull_NotNull_Passes()
    {
        object sut = new();
        Arguments.IsNotNull(sut, nameof(sut));
    }

    [Fact]
    [Trait("Method", "IsNotNullOrWhitespace")]
    public void IsNotNullOrWhitespace_String_Null_Throws()
    {
        const string sut = null;
        Assert.Throws<ArgumentNullException>(() => Arguments.IsNotNullOrWhitespace(sut, nameof(sut)));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("    ")]
    [Trait("Method", "IsNotNullOrWhitespace")]
    public void IsNotNullOrWhitespace_String_Empty_Throws(string sut)
    {
        Assert.Throws<ArgumentException>(() => Arguments.IsNotNullOrWhitespace(sut, nameof(sut)));
    }

    [Fact]
    [Trait("Method", "IsNotNullOrWhitespace")]
    public void IsNotNullOrWhitespace_String_Filled_Passes()
    {
        string sut = Guid.NewGuid().ToString();
        Arguments.IsNotNullOrWhitespace(sut, nameof(sut));
        Assert.NotEmpty(sut);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-10)]
    [Trait("Method", "IsPositiveInteger")]
    public void IsPositiveInteger_Throws(int sut)
    {
        Assert.Throws<ArgumentException>(() => Arguments.IsPositiveInteger(sut, nameof(sut)));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(42)]
    [Trait("Method", "IsPositiveInteger")]
    public void IsPositiveInteger_Works(int sut)
    {
        Assert.Equal(sut, Arguments.IsPositiveInteger(sut, nameof(sut)));
    }

    [Fact]
    [Trait("Method", "IsValidEndpoint(Uri,string)")]
    public void IsValidEndpoint_Null_Throws()
    {
        Uri sut = null;
        Assert.Throws<ArgumentNullException>(() => Arguments.IsValidEndpoint(sut, nameof(sut)));
    }

    [Theory]
    [InlineData("", false)]
    [InlineData("", true)]
    [InlineData("http://", false)]
    [InlineData("http://", true)]
    [InlineData("file://localhost/foo", false)]
    [InlineData("http://foo.azurewebsites.net", false)]
    [InlineData("http://foo.azure-api.net", false)]
    [InlineData("http://[2001:db8:0:b:0:0:0:1A]", false)]
    [InlineData("http://[2001:db8:0:b:0:0:0:1A]:3000", false)]
    [InlineData("http://[2001:db8:0:b:0:0:0:1A]:3000/myapi", false)]
    [InlineData("http://10.0.0.8", false)]
    [InlineData("http://10.0.0.8:3000", false)]
    [InlineData("http://10.0.0.8:3000/myapi", false)]
    [InlineData("foo/bar", true)]
    [Trait("Method", "IsValidEndpoint(Uri,string)")]
    public void IsValidEndpoint_Invalid_Throws(string endpoint, bool isRelative = false)
    {
        Assert.Throws<UriFormatException>(() => Arguments.IsValidEndpoint(isRelative ? new Uri(endpoint, UriKind.Relative) : new Uri(endpoint), "sut"));
    }

    [Theory, ClassData(typeof(EndpointTestCases))]
    [Trait("Method", "IsValidEndpoint(Uri,string)")]
    public void IsValidEndpoint_Valid_Passes(EndpointTestCase testcase)
    {
        Uri sut = new(testcase.BaseEndpoint);
        Arguments.IsValidEndpoint(sut, nameof(sut));
    }

    [Fact]
    [Trait("Method", "IsValidId")]
    public void IsValidId_Null_Throws()
    {
        const string sut = null;
        Assert.Throws<ArgumentNullException>(() => Arguments.IsValidId(sut, nameof(sut)));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    [InlineData("abcdef gh")]
    [InlineData("?")]
    [InlineData(";")]
    [InlineData("{EA235ADF-9F38-44EA-8DA4-EF3D24755767}")]
    [InlineData("###")]
    [InlineData("!!!")]
    [Trait("Method", "IsValidId")]
    public void IsValidId_InvalidId_Throws(string sut)
    {
        Assert.Throws<ArgumentException>(() => Arguments.IsValidId(sut, nameof(sut)));
    }

    [Theory]
    [InlineData("db0ec08d-46a9-465d-9f5e-0066a3ee5b5f")]
    [InlineData("0123456789")]
    [InlineData("abcdefgh")]
    [InlineData("2023|05|01_120000")]
    [InlineData("db0ec08d_46a9_465d_9f5e_0066a3ee5b5f")]
    [InlineData("db0ec08d.46a9.465d.9f5e.0066a3ee5b5f")]
    [Trait("Method", "IsValidId")]
    public void IsValidId_Passes(string sut)
    {
        Arguments.IsValidId(sut, nameof(sut));
    }

    [Fact]
    [Trait("Method", "IsValidTableName")]
    public void IsValidTableName_Null_Throws()
    {
        const string sut = null;
        Assert.Throws<ArgumentNullException>(() => Arguments.IsValidTableName(sut, nameof(sut)));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    [InlineData("abcdef gh")]
    [InlineData("!!!")]
    [InlineData("?")]
    [InlineData(";")]
    [InlineData("{EA235ADF-9F38-44EA-8DA4-EF3D24755767}")]
    [InlineData("###")]
    [InlineData("1abcd")]
    [InlineData("true.false")]
    [InlineData("a-b-c-d")]
    [Trait("Method", "IsValidTableName")]
    public void IsValidTableName_InvalidTableName_Throws(string sut)
    {
        Assert.Throws<ArgumentException>(() => Arguments.IsValidTableName(sut, nameof(sut)));
    }

    [Theory]
    [InlineData("a")]
    [InlineData("movies")]
    [InlineData("movies1234_5678")]
    [Trait("Method", "IsValidTableName")]
    public void IsValidTableName_Passes(string sut)
    {
        Arguments.IsValidTableName(sut, nameof(sut));
    }
}