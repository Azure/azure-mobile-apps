// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices.SQLiteStore;
using Microsoft.WindowsAzure.MobileServices.Sync;
using SQLiteStore.Tests.Helpers;
using Xunit;

namespace SQLiteStore.Tests
{
    public class SyncSettingsManager_Tests
    {
        public static string TestDbName = "syncsettingsmanager-test.db";
        private const string TestTable = "todoItem";
        private const string TestQueryId = "abc";


        [Fact]
        public async Task ResetDeltaTokenAsync_ResetsTheToken()
        {
            MobileServiceSyncSettingsManager settings = await GetSettingManager();

            DateTimeOffset saved = new DateTime(2014, 7, 24, 3, 4, 5, DateTimeKind.Local);
            await settings.SetDeltaTokenAsync(TestTable, TestQueryId, saved);

            DateTimeOffset read = await settings.GetDeltaTokenAsync(TestTable, TestQueryId);
            Assert.Equal(read, saved.ToUniversalTime());

            await settings.ResetDeltaTokenAsync(TestTable, TestQueryId);
            read = await settings.GetDeltaTokenAsync(TestTable, TestQueryId);
            Assert.Equal(read, new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero).ToUniversalTime());
        }

        [Fact]
        public async Task GetDeltaTokenAsync_ReturnsMinValue_WhenTokenDoesNotExist()
        {
            MobileServiceSyncSettingsManager settings = await GetSettingManager();

            DateTimeOffset token = await settings.GetDeltaTokenAsync(TestTable, TestQueryId);

            Assert.Equal(token, new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero).ToUniversalTime());
        }

        [Fact]
        public async Task SetDeltaTokenAsync_SavesTheSetting_AsUTCDate()
        {
            MobileServiceSyncSettingsManager settings = await GetSettingManager();

            DateTimeOffset saved = new DateTime(2014, 7, 24, 3, 4, 5, DateTimeKind.Local);
            await settings.SetDeltaTokenAsync(TestTable, TestQueryId, saved);

            // with cache
            DateTimeOffset read = await settings.GetDeltaTokenAsync(TestTable, TestQueryId);
            Assert.Equal(read, saved.ToUniversalTime());

            // without cache
            settings = await GetSettingManager(resetDb: false);
            read = await settings.GetDeltaTokenAsync(TestTable, TestQueryId);
            Assert.Equal(read, saved.ToUniversalTime());
        }

        [Fact]
        public async Task SetDeltaTokenAsync_SavesTheSetting()
        {
            MobileServiceSyncSettingsManager settings = await GetSettingManager();

            var saved = new DateTimeOffset(2014, 7, 24, 3, 4, 5, TimeSpan.Zero);
            await settings.SetDeltaTokenAsync(TestTable, TestQueryId, saved);

            // with cache
            DateTimeOffset read = await settings.GetDeltaTokenAsync(TestTable, TestQueryId);
            Assert.Equal(read, saved);

            // without cache
            settings = await GetSettingManager(resetDb: false);
            read = await settings.GetDeltaTokenAsync(TestTable, TestQueryId);
            Assert.Equal(read, saved);
        }

        [Fact]
        public async Task SetDeltaTokenAsync_UpdatesCacheAndDatabase()
        {
            MobileServiceSyncSettingsManager settings = await GetSettingManager();

            // first save
            var saved = new DateTimeOffset(2014, 7, 24, 3, 4, 5, TimeSpan.Zero);
            await settings.SetDeltaTokenAsync(TestTable, TestQueryId, saved);

            // then read and update
            DateTimeOffset read = await settings.GetDeltaTokenAsync(TestTable, TestQueryId);
            await settings.SetDeltaTokenAsync(TestTable, TestQueryId, read.AddDays(1));

            // then read again
            read = await settings.GetDeltaTokenAsync(TestTable, TestQueryId);
            Assert.Equal(read, saved.AddDays(1));

            // then read again in fresh instance
            settings = await GetSettingManager(resetDb: false);
            read = await settings.GetDeltaTokenAsync(TestTable, TestQueryId);
            Assert.Equal(read, saved.AddDays(1));
        }

        private static async Task<MobileServiceSyncSettingsManager> GetSettingManager(bool resetDb = true)
        {
            if (resetDb)
            {
                TestUtilities.ResetDatabase(TestDbName);
            }

            var store = new MobileServiceSQLiteStore(TestDbName);
            await store.InitializeAsync();

            var settings = new MobileServiceSyncSettingsManager(store);
            return settings;
        }

    }
}
