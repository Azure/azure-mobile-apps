// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Datasync.InMemory;

namespace Datasync.Common.Test.Models;

/// <summary>
/// A model for handling dateonly and timeonly tests.
/// </summary>
[ExcludeFromCodeCoverage]
public class DateTimeModel : InMemoryTableData
{
    public DateOnly DateOnly { get; set; }
    public TimeOnly TimeOnly { get; set; }

    /// <summary>
    /// The tests use an in-memory table, so need some seed data.
    /// </summary>
    /// <returns>The seed data</returns>
    public static IEnumerable<DateTimeModel> GetSeedData()
    {
        var SourceDate = new DateOnly(2022, 1, 1);
        var list = new List<DateTimeModel>();

        for (int i = 0; i < 365; i++)
        {
            var date = SourceDate.AddDays(i);
            var model = new DateTimeModel
            {
                Id = string.Format("dtm-{0:000}", i),
                Version = Guid.NewGuid().ToByteArray(),
                UpdatedAt = DateTimeOffset.UtcNow,
                Deleted = false,
                DateOnly = date,
                TimeOnly = new TimeOnly(date.Month, date.Day)
            };
            list.Add(model);
        }
        return list;
    }
}
