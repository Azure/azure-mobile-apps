// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Datasync.Converters;
using Microsoft.AspNetCore.OData.Query.Wrapper;
using Newtonsoft.Json;

namespace Microsoft.AspNetCore.Datasync.Test.Converters;

[ExcludeFromCodeCoverage]
public class JSelectExpandWrapperConverter_Tests
{
    private readonly JsonSerializerSettings _settings;

    public JSelectExpandWrapperConverter_Tests()
    {
        _settings = new JsonSerializerSettings()
        {
            NullValueHandling = NullValueHandling.Ignore
        };
        _settings.Converters.Add(new JSelectExpandWrapperConverter());
    }

    [Fact]
    public void JsonConverter_Basics()
    {
        var converter = new JSelectExpandWrapperConverter();

        Assert.False(converter.CanRead);
        Assert.True(converter.CanWrite);

        Assert.True(converter.CanConvert(typeof(ISelectExpandWrapper)));
        Assert.False(converter.CanConvert(typeof(string)));

        Assert.Throws<ArgumentNullException>(() => converter.CanConvert(null));
    }

    [Fact]
    public void Ctor_NullMapperProvider_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new JSelectExpandWrapperConverter(null));
    }
}
