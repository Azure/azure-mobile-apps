// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Newtonsoft.Json.Linq;

namespace Microsoft.Datasync.Client.SQLiteStore.Test.Utils;

[ExcludeFromCodeCoverage]
public class ColumnDefinition_Tests : BaseStoreTest
{
    [Fact]
    public void DeserializeValue_Null_ReturnsNull()
    {
        var sut = new ColumnDefinition("test", JTokenType.String, "MISMATCH");

        Assert.Null(sut.DeserializeValue(null));
        Assert.Null(sut.DeserializeValue(new object()));
    }

    [Fact]
    public void Equals_Works()
    {
        var expected = new ColumnDefinition("test", JTokenType.String, "string");
        var equal = new ColumnDefinition("test", JTokenType.String, "string");
        var nTable = new ColumnDefinition("movies", JTokenType.String, "string");
        var nJsonType = new ColumnDefinition("movies", JTokenType.Bytes, "string");
        var nStoreType = new ColumnDefinition("movies", JTokenType.String, "uri");

        Assert.True(expected.Equals(equal));
        Assert.False(expected.Equals(nTable));
        Assert.False(expected.Equals(nJsonType));
        Assert.False(expected.Equals(nStoreType));
    }

    [Fact]
    public void Equals_Object_Works()
    {
        var expected = new ColumnDefinition("test", JTokenType.String, "string");
        object equal = new ColumnDefinition("test", JTokenType.String, "string");
        object nTable = new ColumnDefinition("movies", JTokenType.String, "string");
        object nJsonType = new ColumnDefinition("movies", JTokenType.Bytes, "string");
        object nStoreType = new ColumnDefinition("movies", JTokenType.String, "uri");
        object notColDef = new();

        Assert.True(expected.Equals(equal));
        Assert.False(expected.Equals(nTable));
        Assert.False(expected.Equals(nJsonType));
        Assert.False(expected.Equals(nStoreType));
        Assert.False(expected.Equals(notColDef));
    }

    [Fact]
    public void GetHashCode_Works()
    {
        var expected = new ColumnDefinition("test", JTokenType.String, "string");
        var equal = new ColumnDefinition("test", JTokenType.String, "string");
        var nTable = new ColumnDefinition("movies", JTokenType.String, "string");

        Assert.Equal(expected.GetHashCode(), equal.GetHashCode());
        Assert.NotEqual(expected.GetHashCode(), nTable.GetHashCode());
    }

    [Fact]
    public void ToString_Works()
    {
        var expected = new ColumnDefinition("test", JTokenType.String, "string");

        Assert.NotEmpty(expected.ToString());
        Assert.StartsWith("ColumnDef", expected.ToString());
    }
}
