// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using Microsoft.WindowsAzure.MobileServices;
using Microsoft.WindowsAzure.MobileServices.Sync;
using MobileClient.Tests.Helpers;
using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace MobileClient.Tests.Table
{
    public class MobileServiceSyncTable_Test
    {
        [Theory]
        [InlineData("|myitems")]
        [InlineData("s|myitems")]
        [InlineData("nnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnn")]
        public void ValidateQueryId_Throws_OnInvalidId(string queryId)
            => Assert.Throws<ArgumentException>(() => MobileServiceSyncTable.ValidateQueryId(queryId));

        [Theory]
        [InlineData("myitems1")]
        [InlineData("myItems_yourItems1")]
        [InlineData("my-items123")]
        [InlineData("-myitems")]
        [InlineData("_myitems")]
        [InlineData("asdf@#$!/:^")]
        public void ValidateQueryId_Succeeds_OnValidId(string queryId)
            => MobileServiceSyncTable.ValidateQueryId(queryId);

        [Fact]
        public async Task InsertAsync_GeneratesId_WhenNull()
        {
            IMobileServiceClient service = new MobileServiceClient(MobileAppUriValidator.DummyMobileApp);
            await service.SyncContext.InitializeAsync(new MobileServiceLocalStoreMock(), new MobileServiceSyncHandler());

            var item = new JObject();
            JObject inserted = await service.GetSyncTable("someTable").InsertAsync(item);
            Assert.NotNull(inserted.Value<string>("id")); 

            item = new JObject() { { "id", null } };
            inserted = await service.GetSyncTable("someTable").InsertAsync(item);
            Assert.NotNull(inserted.Value<string>("id"));
        }
    }
}
