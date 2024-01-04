// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Models;
using Datasync.Common.TestData;
using Microsoft.AspNetCore.Datasync.Abstractions.Converters;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO.Converters;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Microsoft.AspNetCore.Datasync.Tests.Helpers;

[ExcludeFromCodeCoverage(Justification = "Test suite")]
public abstract class ServiceTest
{
    protected readonly ServiceApplicationFactory factory;
    protected readonly HttpClient client;
    protected readonly JsonSerializerOptions serializerOptions;
    protected readonly DateTimeOffset StartTime = DateTimeOffset.UtcNow;

    protected ServiceTest(ServiceApplicationFactory factory)
    {
        this.factory = factory;
        this.client = factory.CreateClient();
        this.serializerOptions = GetSerializerOptions();
    }

    private static JsonSerializerOptions GetSerializerOptions()
    {
        JsonSerializerOptions options = new(JsonSerializerDefaults.Web)
        {
            Converters =
            {
                new JsonStringEnumConverter(),
                new DateTimeOffsetConverter(),
                new DateTimeConverter(),
                new TimeOnlyConverter(),
                new GeoJsonConverterFactory()
            },
        };
        options.Converters.Add(new JsonStringEnumConverter());
        options.Converters.Add(new DateTimeOffsetConverter());
        options.Converters.Add(new DateTimeConverter());
        return options;
    }

    protected void SeedKitchenSink()
    {
        DateOnly SourceDate = new(2022, 1, 1);
        for (int i = 0; i < 365; i++)
        {
            DateOnly date = SourceDate.AddDays(i);
            InMemoryKitchenSink model = new()
            {
                Id = string.Format("id-{0:000}", i),
                Version = Guid.NewGuid().ToByteArray(),
                UpdatedAt = DateTimeOffset.UtcNow,
                Deleted = false,
                DateOnlyValue = date,
                TimeOnlyValue = new TimeOnly(date.Month, date.Day)
            };
            factory.Store(model);
        }
    }

    protected void SeedKitchenSinkWithCountryData()
    {
        foreach (Country countryRecord in CountryData.GetCountries())
        {
            InMemoryKitchenSink model = new()
            {
                Id = countryRecord.IsoCode,
                Version = Guid.NewGuid().ToByteArray(),
                UpdatedAt = DateTimeOffset.UtcNow,
                Deleted = false,
                PointValue = new Point(countryRecord.Longitude, countryRecord.Latitude),
                StringValue = countryRecord.CountryName
            };
            factory.Store(model);
        }
    }
}
