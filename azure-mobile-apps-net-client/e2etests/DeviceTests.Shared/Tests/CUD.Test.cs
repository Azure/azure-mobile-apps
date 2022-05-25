// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using DeviceTests.Shared.Helpers;
using DeviceTests.Shared.Helpers.Models;
using DeviceTests.Shared.TestPlatform;
using Microsoft.WindowsAzure.MobileServices;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace DeviceTests.Shared.Tests
{
    [Collection(nameof(SingleThreadedCollection))]
    public class CUDTests : E2ETestBase
    {
        enum DeleteTestType { ValidDelete, NonExistingId, NoIdField }

        static Lazy<Random> s_Random = new Lazy<Random>(() =>
        {
            DateTime now = DateTime.UtcNow;
            int seed = now.Year * 10000 + now.Month * 100 + now.Day;
            return new Random(seed);
        });

        [Fact]
        private async Task CUD_TypedStringId()
        {
            Random rndGen = s_Random.Value;

            await CreateTypedUpdateTest("[string id] Update typed item",
                new RoundTripTableItem(rndGen), new RoundTripTableItem(rndGen));
            await CreateTypedUpdateTest(
                "[string id] Update typed item, setting values to null",
                new RoundTripTableItem(rndGen),
                new RoundTripTableItem(rndGen) { Name = null, Bool = null, Date = null });
            await CreateTypedUpdateTest(
                "[string id] Update typed item, setting values to 0",
                new RoundTripTableItem(rndGen),
                new RoundTripTableItem(rndGen) { Integer = 0, Number = 0.0 });

            await CreateTypedUpdateTest<RoundTripTableItem, MobileServiceInvalidOperationException>("[string id] (Neg) Update typed item, non-existing item id",
                new RoundTripTableItem(rndGen), new RoundTripTableItem(rndGen) { Id = "does not exist" }, false);
            await CreateTypedUpdateTest<RoundTripTableItem, ArgumentException>("[string id] (Neg) Update typed item, id = null",
                new RoundTripTableItem(rndGen), new RoundTripTableItem(rndGen) { Id = null }, false);
        }

        [Fact]
        public Task CUD_TypedIntId_UpdateTypedItem()
            => CreateTypedUpdateTest(
                "[int id] Update typed item", 
                new IntIdRoundTripTableItem(s_Random.Value), 
                new IntIdRoundTripTableItem(s_Random.Value));

        [Fact]
        public Task TypedIntId_SettingValuesToNull()
            => CreateTypedUpdateTest(
                "[int id] Update typed item, setting values to null",
                new IntIdRoundTripTableItem(s_Random.Value),
                new IntIdRoundTripTableItem(s_Random.Value) { Name = null, Bool = null, Date = null });

        [Fact]
        public Task TypedIntId_Neg_UpdateTypedItem_NonExistingItemId()
            => CreateTypedUpdateTest<IntIdRoundTripTableItem, MobileServiceInvalidOperationException>(
                "[int id] (Neg) Update typed item, non-existing item id",
                new IntIdRoundTripTableItem(s_Random.Value), 
                new IntIdRoundTripTableItem(s_Random.Value) { Id = 1000000000 }, false);

        [Fact]
        public Task TypedIntId_Neg_UpdatedTypedItem_IdZero()
            =>CreateTypedUpdateTest<IntIdRoundTripTableItem, ArgumentException>(
                "[int id] (Neg) Update typed item, id = 0",
                new IntIdRoundTripTableItem(s_Random.Value), 
                new IntIdRoundTripTableItem(s_Random.Value) { Id = 0 }, false);

        [Fact]
        private async Task CUD_UntypedStringId()
        {
            Random rndGen = s_Random.Value;

            string toInsertJsonString = JsonConvert.SerializeObject(new RoundTripTableItem(rndGen) { Id = null });
            string toUpdateJsonString = JsonConvert.SerializeObject(new RoundTripTableItem(rndGen) { Id = null });
            await CreateUntypedUpdateTest("[string id] Update untyped item", toInsertJsonString, toUpdateJsonString, true);

            JToken nullValue = JValue.Parse("null");
            JObject toUpdate = JObject.Parse(toUpdateJsonString);
            toUpdate["name"] = nullValue;
            toUpdate["date1"] = nullValue;
            toUpdate["bool"] = nullValue;
            toUpdate["number"] = nullValue;
            await CreateUntypedUpdateTest("[string id] Update untyped item, setting values to null", toInsertJsonString, toUpdate.ToString(), true);

            string idName = GetSerializedId<RoundTripTableItem>();

            toUpdate[idName] = Guid.NewGuid().ToString();
            await CreateUntypedUpdateTest<MobileServiceInvalidOperationException>("(Neg) [string id] Update untyped item, non-existing item id",
                toInsertJsonString, toUpdate.ToString(), false, true);

            toUpdate[idName] = nullValue;
            await CreateUntypedUpdateTest<InvalidOperationException>("[string id] (Neg) Update typed item, id = null",
                toInsertJsonString, toUpdateJsonString, false, true);

            // Delete tests
            await CreateDeleteTest<RoundTripTableItem>("[string id] Delete typed item", true, DeleteTestType.ValidDelete);
            await CreateDeleteTest<RoundTripTableItem>("[string id] (Neg) Delete typed item with non-existing id", true, DeleteTestType.NonExistingId);
            await CreateDeleteTest<RoundTripTableItem>("[string id] Delete untyped item", false, DeleteTestType.ValidDelete);
            await CreateDeleteTest<RoundTripTableItem>("[string id] (Neg) Delete untyped item with non-existing id", false, DeleteTestType.NonExistingId);
            await CreateDeleteTest<RoundTripTableItem>("[string id] (Neg) Delete untyped item without id field", false, DeleteTestType.NoIdField);
        }

        [Fact]
        private async Task CUD_UntypedIntId()
        {
            Random rndGen = s_Random.Value;

            string toInsertJsonString = @"{
                ""name"":""hello"",
                ""date1"":""2012-12-13T09:23:12.000Z"",
                ""bool"":true,
                ""integer"":-1234,
                ""number"":123.45
            }";

            string toUpdateJsonString = @"{
                ""name"":""world"",
                ""date1"":""1999-05-23T19:15:54.000Z"",
                ""bool"":false,
                ""integer"":9999,
                ""number"":888.88
            }";

            await CreateUntypedUpdateTest("[int id] Update untyped item", toInsertJsonString, toUpdateJsonString);

            JToken nullValue = JValue.Parse("null");
            JObject toUpdate = JObject.Parse(toUpdateJsonString);
            toUpdate["name"] = nullValue;
            toUpdate["bool"] = nullValue;
            toUpdate["integer"] = nullValue;
            await CreateUntypedUpdateTest("[int id] Update untyped item, setting values to null", toInsertJsonString, toUpdate.ToString());

            string idName = GetSerializedId<IntIdRoundTripTableItem>();

            toUpdate[idName] = 1000000000;
            await CreateUntypedUpdateTest<MobileServiceInvalidOperationException>("[int id] (Neg) Update untyped item, non-existing item id",
                toInsertJsonString, toUpdate.ToString(), false);

            toUpdate[idName] = 0;
            await CreateUntypedUpdateTest<ArgumentException>("[int id] (Neg) Update typed item, id = 0",
                toInsertJsonString, toUpdateJsonString, false);

            // Delete tests
            await CreateDeleteTest<IntIdRoundTripTableItem>("[int id] Delete typed item", true, DeleteTestType.ValidDelete);
            await CreateDeleteTest<IntIdRoundTripTableItem>("[int id] (Neg) Delete typed item with non-existing id", true, DeleteTestType.NonExistingId);
            await CreateDeleteTest<IntIdRoundTripTableItem>("[int id] Delete untyped item", false, DeleteTestType.ValidDelete);
            await CreateDeleteTest<IntIdRoundTripTableItem>("[int id] (Neg) Delete untyped item with non-existing id", false, DeleteTestType.NonExistingId);
            await CreateDeleteTest<IntIdRoundTripTableItem>("[int id] (Neg) Delete untyped item without id field", false, DeleteTestType.NoIdField);
        }


        private Task CreateTypedUpdateTest<TRoundTripType>(
                    string testName, TRoundTripType itemToInsert, TRoundTripType itemToUpdate) where TRoundTripType : ICloneableItem<TRoundTripType>
        {
            return CreateTypedUpdateTest<TRoundTripType, ExceptionTypeWhichWillNeverBeThrown>(testName, itemToInsert, itemToUpdate);
        }

        private async Task CreateTypedUpdateTest<TRoundTripType, TExpectedException>(
            string testName, TRoundTripType itemToInsert, TRoundTripType itemToUpdate, bool setUpdatedId = true)
            where TExpectedException : Exception
            where TRoundTripType : ICloneableItem<TRoundTripType>
        {
            var client = GetClient();

            var table = client.GetTable<TRoundTripType>();
            var toInsert = itemToInsert.Clone();
            var toUpdate = itemToUpdate.Clone();
            try
            {
                await table.InsertAsync(toInsert);

                if (setUpdatedId)
                {
                    toUpdate.Id = toInsert.Id;
                }

                var expectedItem = toUpdate.Clone();

                await table.UpdateAsync(toUpdate);

                var retrievedItem = await table.LookupAsync(toInsert.Id);
                Assert.Equal(expectedItem, retrievedItem);

                // cleanup
                await table.DeleteAsync(retrievedItem);

                if (typeof(TExpectedException) != typeof(ExceptionTypeWhichWillNeverBeThrown))
                {
                    Assert.False(true, "Error, test should have failed with " + typeof(TExpectedException).FullName + ", but succeeded.");
                }
            }
            catch (TExpectedException)
            {
                // Swallow the expected exception.
            }
        }

        private Task CreateUntypedUpdateTest(string testName, string itemToInsert, string itemToUpdate, bool useStringIdTable = false)
        {
            return CreateUntypedUpdateTest<ExceptionTypeWhichWillNeverBeThrown>(testName, itemToInsert, itemToUpdate, true, useStringIdTable);
        }

        private async Task CreateUntypedUpdateTest<TExpectedException>(
            string testName, string itemToInsertJson, string itemToUpdateJson, bool setUpdatedId = true, bool useStringIdTable = false)
            where TExpectedException : Exception
        {
            var itemToInsert = JObject.Parse(itemToInsertJson);
            var itemToUpdate = JObject.Parse(itemToUpdateJson);
            Utilities.CamelCaseProps(itemToUpdate);

            var client = GetClient();
            var table = client.GetTable(useStringIdTable ? "RoundTripTable" : "IntIdRoundTripTable");
            try
            {
                var inserted = await table.InsertAsync(itemToInsert);
                object id = useStringIdTable ?
                    (object)(string)inserted["id"] :
                    (object)(int)inserted["id"];

                if (setUpdatedId)
                {
                    itemToUpdate["id"] = new JValue(id);
                }

                var expectedItem = JObject.Parse(itemToUpdate.ToString());

                var updated = await table.UpdateAsync(itemToUpdate);
                var retrievedItem = await table.LookupAsync(id);

                List<string> errors = new List<string>();
                if (!Utilities.CompareJson(expectedItem, retrievedItem, errors))
                {
                    Assert.False(true, "Error, retrieved item is different than the expected value. Expected: " + expectedItem + "; actual:" + retrievedItem);
                }

                // cleanup
                await table.DeleteAsync(new JObject(new JProperty("id", id)));

                if (typeof(TExpectedException) != typeof(ExceptionTypeWhichWillNeverBeThrown))
                {
                    Assert.False(true, "Error, test should have failed with " + typeof(TExpectedException).FullName + " but succeeded.");
                }
            }
            catch (TExpectedException)
            {
                // Swallow expected exception
            }
        }

        private async Task CreateDeleteTest<TItemType>(string testName, bool useTypedTable, DeleteTestType testType) where TItemType : ICloneableItem<TItemType>
        {
            if (useTypedTable && testType == DeleteTestType.NoIdField)
            {
                throw new ArgumentException("Cannot send a delete request without an id field on a typed table.");
            }

            var client = GetClient();
            var typedTable = client.GetTable<TItemType>();
            var useStringIdTable = typeof(TItemType) == typeof(RoundTripTableItem);
            var untypedTable = client.GetTable(useStringIdTable ? "RoundTripTable" : "IntIdRoundTripTable");
            TItemType itemToInsert;
            if (useStringIdTable)
            {
                itemToInsert = (TItemType)(object)new RoundTripTableItem { Name = "will be deleted", Number = 123 };
            }
            else
            {
                itemToInsert = (TItemType)(object)new IntIdRoundTripTableItem { Name = "will be deleted", Number = 123 };
            }

            await typedTable.InsertAsync(itemToInsert);
            object id = itemToInsert.Id;
            switch (testType)
            {
                case DeleteTestType.ValidDelete:
                    if (useTypedTable)
                    {
                        await typedTable.DeleteAsync(itemToInsert);
                    }
                    else
                    {
                        await untypedTable.DeleteAsync(new JObject(new JProperty("id", id)));
                    }

                    var vsioe = await Assert.ThrowsAsync<MobileServiceInvalidOperationException>(() => untypedTable.LookupAsync(id));
                    Assert.True(Validate404Response(vsioe));
                    return;

                case DeleteTestType.NonExistingId:
                    var msioe = await Assert.ThrowsAsync<MobileServiceInvalidOperationException>(async () =>
                    {
                        object nonExistingId = useStringIdTable ? (object)Guid.NewGuid().ToString() : (object)1000000000;
                        if (useTypedTable)
                        {
                            itemToInsert.Id = nonExistingId;
                            await typedTable.DeleteAsync(itemToInsert);
                        }
                        else
                        {
                            JObject jo = new JObject(new JProperty("id", nonExistingId));
                            await untypedTable.DeleteAsync(jo);
                        }
                    });
                    Assert.True(Validate404Response(msioe));
                    return;

                default:
                    await Assert.ThrowsAsync<ArgumentException>(async () =>
                    {
                        JObject jo = new JObject(new JProperty("Name", "hello"));
                        await untypedTable.DeleteAsync(jo);
                    });
                    return;
            }
        }

        private bool Validate404Response(MobileServiceInvalidOperationException msioe)
        {
            var response = msioe.Response;
            return (response.StatusCode == HttpStatusCode.NotFound);
        }

        public static string GetSerializedId<T>()
        {
            var idName = typeof(T).GetTypeInfo()
                .DeclaredProperties
                .Where(p => p.Name.ToLowerInvariant() == "id")
                .Select(p =>
                {
                    var a = p.GetCustomAttribute<JsonPropertyAttribute>();
                    return a == null ? p.Name : a.PropertyName;
                })
                .First();
            return idName;
        }
    }
}
