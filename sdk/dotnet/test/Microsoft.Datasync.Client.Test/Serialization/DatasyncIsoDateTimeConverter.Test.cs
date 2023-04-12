// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Test;
using Microsoft.Datasync.Client.Serialization;
using Newtonsoft.Json;
using System.Text;

namespace Microsoft.Datasync.Client.Test.Serialization;

[ExcludeFromCodeCoverage]
public class DatasyncIsoDateTimeConverter_Tests : BaseTest
{
    private readonly DatasyncIsoDateTimeConverter converter = new();
    private const string jsontestdate = "\"2021-02-24T11:45:20.345Z\"";
    private readonly DateTime dttestdate = new(2021, 2, 24, 11, 45, 20, 345, DateTimeKind.Utc);
    private readonly DateTimeOffset dtotestdate = new(2021, 2, 24, 11, 45, 20, 345, TimeSpan.Zero);

    #region Helpers
    /// <summary>
    /// Create a <see cref="JsonReader"/> to read some JSON.
    /// </summary>
    /// <param name="json">The JSON string to read.</param>
    /// <returns>The <see cref="JsonReader"/></returns>
    private static JsonReader CreateReader(string json)
    {
        var reader = new JsonTextReader(new StringReader(json));
        while (reader.TokenType == JsonToken.None)
        {
            if (!reader.Read())
            {
                break;
            }
        }
        return reader;
    }
    #endregion

    [Fact]
    [Trait("Method", "ReadJson")]
    public void ReadJson_CanReadDateTime()
    {
        var reader = CreateReader(jsontestdate);
        var obj = converter.ReadJson(reader, typeof(DateTime), null, JsonSerializer.CreateDefault());
        Assert.IsAssignableFrom<DateTime>(obj);
        var actual = (DateTime)obj;
        Assert.Equal(dttestdate.ToUniversalTime().Ticks, actual.ToUniversalTime().Ticks);
        Assert.Equal(DateTimeKind.Local, actual.Kind);
    }

    [Fact]
    [Trait("Method", "ReadJson")]
    public void ReadJson_CanReadDateTimeOffset()
    {
        var reader = CreateReader(jsontestdate);
        var obj = converter.ReadJson(reader, typeof(DateTimeOffset), null, JsonSerializer.CreateDefault());
        Assert.IsAssignableFrom<DateTimeOffset>(obj);
        var actual = (DateTimeOffset)obj;
        Assert.Equal(dtotestdate.ToUniversalTime().Ticks, actual.ToUniversalTime().Ticks);
    }

    [Fact]
    [Trait("Method", "WriteJson")]
    public void WriteJson_CanWriteDateTime()
    {
        var sb = new StringBuilder();
        var sw = new StringWriter(sb);
        using (var writer = new JsonTextWriter(sw))
        {
            converter.WriteJson(writer, dttestdate, JsonSerializer.CreateDefault());
        }
        sw.Close();
        Assert.Equal(jsontestdate, sb.ToString());
    }

    [Fact]
    [Trait("Method", "WriteJson")]
    public void WriteJson_CanWriteDateTimeOffset()
    {
        var sb = new StringBuilder();
        var sw = new StringWriter(sb);
        using (var writer = new JsonTextWriter(sw))
        {
            converter.WriteJson(writer, dtotestdate, JsonSerializer.CreateDefault());
        }
        sw.Close();
        Assert.Equal(jsontestdate, sb.ToString());
    }
}
