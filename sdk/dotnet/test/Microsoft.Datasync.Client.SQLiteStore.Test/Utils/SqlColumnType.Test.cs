// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Newtonsoft.Json.Linq;

namespace Microsoft.Datasync.Client.SQLiteStore.Test.Utils;

[ExcludeFromCodeCoverage]
public class SqlColumnType_Tests : BaseStoreTest
{
    [Fact]
    public void Get_WithNull_Works()
    {
        Assert.Null(SqlColumnType.Get(JTokenType.Null, true));
        Assert.Throws<NotSupportedException>(() => SqlColumnType.Get(JTokenType.Null, false));
    }

    [Theory]
    [InlineData(typeof(bool), "NUMERIC")]
    [InlineData(typeof(DateTime), "NUMERIC")]
    [InlineData(typeof(decimal), "NUMERIC")]
    [InlineData(typeof(int), "INTEGER")]
    [InlineData(typeof(uint), "INTEGER")]
    [InlineData(typeof(long), "INTEGER")]
    [InlineData(typeof(ulong), "INTEGER")]
    [InlineData(typeof(short), "INTEGER")]
    [InlineData(typeof(ushort), "INTEGER")]
    [InlineData(typeof(byte), "INTEGER")]
    [InlineData(typeof(sbyte), "INTEGER")]
    [InlineData(typeof(float), "REAL")]
    [InlineData(typeof(double), "REAL")]
    [InlineData(typeof(string), "TEXT")]
    [InlineData(typeof(Guid), "TEXT")]
    [InlineData(typeof(byte[]), "TEXT")]
    [InlineData(typeof(Uri), "TEXT")]
    [InlineData(typeof(TimeSpan), "TEXT")]
    public void GetStoreCaseType_Works(Type sut, string expected)
    {
        Assert.Equal(expected, SqlColumnType.GetStoreCastType(sut));
    }

    [Theory]
    [InlineData(typeof(SqlColumnType))]
    [InlineData(typeof(BaseStoreTest))]
    public void GetStoreCaseType_Throws(Type sut)
    {
        Assert.Throws<NotSupportedException>(() => SqlColumnType.GetStoreCastType(sut));
    }

    [Theory]
    [InlineData("INTEGER", false)]
    [InlineData("TEXT", false)]
    [InlineData("NONE", false)]
    [InlineData("REAL", true)]
    [InlineData("NUMERIC", false)]
    [InlineData("BOOLEAN", false)]
    [InlineData("DATETIME", false)]
    [InlineData("FLOAT", true)]
    [InlineData("BLOB", false)]
    [InlineData("GUID", false)]
    [InlineData("JSON", false)]
    [InlineData("URI", false)]
    [InlineData("TIMESPAN", false)]
    public void IsFloatType_Works(string type, bool expected)
    {
        Assert.Equal(expected, SqlColumnType.IsFloatType(type));
    }

    [Fact]
    public void SerializeValue_Null_ReturnsNull()
    {
        Assert.Null(SqlColumnType.SerializeValue(null, "STRING", JTokenType.String));
        Assert.Null(SqlColumnType.SerializeValue(JValue.CreateNull(), "STRING", JTokenType.String));
    }

    [Fact]
    public void SerializeValue_ReturnsString_ByDefault()
    {
        var value = JValue.CreateString("foo");
        var actual = SqlColumnType.SerializeValue(value, "UNDEFINED", JTokenType.String);
        Assert.Equal("foo", actual);
    }
}
