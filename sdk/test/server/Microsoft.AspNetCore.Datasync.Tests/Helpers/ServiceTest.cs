// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Models;
using Datasync.Common.TestData;
using Microsoft.AspNetCore.Datasync.Abstractions;
using Microsoft.Spatial;
using System.Text.Json;

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
        => new DatasyncServiceOptions().JsonSerializerOptions;

    protected void SeedKitchenSinkWithDateTimeData()
    {
        factory.RunWithRepository<InMemoryKitchenSink>(repository =>
        {
            repository.Clear();
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
                repository.StoreEntity(model);
            }
        });
    }

    protected void SeedKitchenSinkWithCountryData()
    {
        factory.RunWithRepository<InMemoryKitchenSink>(repository =>
        {
            repository.Clear();
            foreach (Country countryRecord in CountryData.GetCountries())
            {
                InMemoryKitchenSink model = new()
                {
                    Id = countryRecord.IsoCode,
                    Version = Guid.NewGuid().ToByteArray(),
                    UpdatedAt = DateTimeOffset.UtcNow,
                    Deleted = false,
                    PointValue = GeographyPoint.Create(countryRecord.Latitude, countryRecord.Longitude),
                    StringValue = countryRecord.CountryName
                };
                repository.StoreEntity(model);
            }
        });
    }
}
