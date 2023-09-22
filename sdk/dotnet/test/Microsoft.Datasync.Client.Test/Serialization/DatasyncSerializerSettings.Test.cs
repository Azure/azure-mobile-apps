// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Test;
using Microsoft.Datasync.Client.Serialization;

namespace Microsoft.Datasync.Client.Test.Serialization;

[ExcludeFromCodeCoverage]
public class DatasyncSerializerSettings_Tests : BaseTest
{
    private readonly DatasyncSerializerSettings settings;
    private readonly DatasyncContractResolver contractResolver;

    public DatasyncSerializerSettings_Tests()
    {
        settings = new();
        contractResolver = settings.ContractResolver;
    }

    [Fact]
    [Trait("Method", "CamelCasePropertyNames")]
    public void CamelCasePropertyNames_CanRoundTrip()
    {
        settings.CamelCasePropertyNames = true;
        Assert.True(settings.CamelCasePropertyNames);
        Assert.True(contractResolver.CamelCasePropertyNames);

        settings.CamelCasePropertyNames = false;
        Assert.False(settings.CamelCasePropertyNames);
        Assert.False(contractResolver.CamelCasePropertyNames);
    }

    [Fact]
    [Trait("Method", "GetSerializerFromSettings")]
    public void GetSerializerFromSettings_Works()
    {
        var serializer = settings.GetSerializerFromSettings();
        Assert.NotNull(serializer);
    }
}
