// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using DeviceTests.Shared.Helpers.Models;
using DeviceTests.Shared.TestPlatform;
using Microsoft.WindowsAzure.MobileServices;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Xunit;

namespace DeviceTests.Shared.Tests
{
    [Collection(nameof(SingleThreadedCollection))]
    public class Date_Tests : E2ETestBase
    {
        [Fact]
        public async Task InsertAndQuery()
        {
            IMobileServiceTable<Dates> table = GetClient().GetTable<Dates>();

            DateTime date = new DateTime(2009, 10, 21, 14, 22, 59, 860, DateTimeKind.Local);
            Dates instance = new Dates { Date = date };
            await table.InsertAsync(instance);
            Assert.Equal(date, instance.Date);

            List<Dates> items = await table.Where(i => i.Date == date).Where(i => i.Id == instance.Id).ToListAsync();
            Assert.Single(items);
            Assert.Equal(date, items[0].Date);
        }

        [Fact]
        public async Task InsertAndQueryOffset()
        {
            IMobileServiceTable<Dates> table = GetClient().GetTable<Dates>();

            DateTimeOffset date = new DateTimeOffset(
                new DateTime(2009, 10, 21, 14, 22, 59, 860, DateTimeKind.Utc).ToLocalTime());

            Dates instance = new Dates { DateOffset = date };
            await table.InsertAsync(instance);
            Assert.Equal(date, instance.DateOffset);

            List<Dates> items = await table.Where(i => i.DateOffset == date).Where(i => i.Id == instance.Id).ToListAsync();
            Assert.Single(items);
            Assert.Equal(date, items[0].DateOffset);
        }

        [Fact]
        public async Task DateKinds()
        {
            IMobileServiceTable<Dates> table = GetClient().GetTable<Dates>();

            DateTime original = new DateTime(2009, 10, 21, 14, 22, 59, 860, DateTimeKind.Local);
            Dates instance = new Dates { Date = original };

            await table.InsertAsync(instance);
            Assert.Equal(DateTimeKind.Local, instance.Date.Kind);
            Assert.Equal(original, instance.Date);

            instance.Date = new DateTime(2010, 5, 21, 0, 0, 0, 0, DateTimeKind.Utc);
            await table.UpdateAsync(instance);
            Assert.Equal(DateTimeKind.Local, instance.Date.Kind);

            instance.Date = new DateTime(2010, 5, 21, 0, 0, 0, 0, DateTimeKind.Local);
            await table.UpdateAsync(instance);
            Assert.Equal(DateTimeKind.Local, instance.Date.Kind);
        }

        [Fact]
        public async Task ChangeCulture()
        {
            IMobileServiceTable<Dates> table = GetClient().GetTable<Dates>();

            CultureInfo threadCulture = CultureInfo.DefaultThreadCurrentCulture;
            CultureInfo threadUICulture = CultureInfo.DefaultThreadCurrentUICulture;

            DateTime original = new DateTime(2009, 10, 21, 14, 22, 59, 860, DateTimeKind.Local);
            Dates instance = new Dates { Date = original };
            await table.InsertAsync(instance);

            CultureInfo arabic = new CultureInfo("ar-EG");
            CultureInfo.DefaultThreadCurrentCulture = arabic;
            CultureInfo.DefaultThreadCurrentUICulture = arabic;
            Dates arInstance = await table.LookupAsync(instance.Id);
            Assert.Equal(original, arInstance.Date);

            CultureInfo chinese = new CultureInfo("zh-CN");
            CultureInfo.DefaultThreadCurrentCulture = chinese;
            CultureInfo.DefaultThreadCurrentUICulture = chinese;
            Dates zhInstance = await table.LookupAsync(instance.Id);
            Assert.Equal(original, zhInstance.Date);

            CultureInfo russian = new CultureInfo("ru-RU");
            CultureInfo.DefaultThreadCurrentCulture = russian;
            CultureInfo.DefaultThreadCurrentUICulture = russian;
            Dates ruInstance = await table.LookupAsync(instance.Id);
            Assert.Equal(original, ruInstance.Date);

            CultureInfo.DefaultThreadCurrentCulture = threadCulture;
            CultureInfo.DefaultThreadCurrentUICulture = threadUICulture;
        }
    }
}
