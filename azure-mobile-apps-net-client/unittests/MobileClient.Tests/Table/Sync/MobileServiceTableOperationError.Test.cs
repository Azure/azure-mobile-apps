// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using Microsoft.WindowsAzure.MobileServices;
using Microsoft.WindowsAzure.MobileServices.Sync;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace MobileClient.Tests.Table.Sync
{
    public class MobileServiceTableOperationErrorTests
    {
        private MobileServiceSerializer serializer;

        public MobileServiceTableOperationErrorTests()
        {
            this.serializer = new MobileServiceSerializer();
        }

        [Fact]
        public void Deserialize_Succeeds()
        {
            var serializedError = JObject.Parse(@"
            {""id"":""70cf6cc2-5981-4a32-ae6c-249572917a46"",
            ""httpStatus"": 200,
            ""operationVersion"":123,
            ""operationKind"":0,
            ""tableName"":""test"",
            ""tableKind"":1,
            ""item"":""{\""id\"":\""abc\"",\""text\"":\""example\""}"",
            ""rawResult"":""{\""id\"":\""abc\"",\""text\"":\""example\""}""
            }");
            var operation = MobileServiceTableOperationError.Deserialize(serializedError, this.serializer.SerializerSettings);

            Assert.Equal(serializedError["id"], operation.Id);
            Assert.Equal(serializedError["operationVersion"], operation.OperationVersion);
            Assert.Equal(serializedError["operationKind"], (int)operation.OperationKind);
            Assert.Equal(serializedError["httpStatus"], (int)operation.Status);
            Assert.Equal(serializedError["tableName"], operation.TableName);
            Assert.Equal(serializedError["tableKind"], (int)operation.TableKind);
            Assert.Equal(serializedError["item"], operation.Item.ToString(Formatting.None));
            Assert.Equal(serializedError["rawResult"], operation.RawResult);
        }

        [Fact]
        public void Deserialize_Succeeds_WhenOperationVersionIsNull()
        {
            var serializedError = JObject.Parse(@"
            {""id"":""70cf6cc2-5981-4a32-ae6c-249572917a46"",
            ""httpStatus"": 200,
            ""operationVersion"":null,
            ""operationKind"":0,
            ""tableName"":""test"",
            ""tableKind"":1,
            ""item"":""{\""id\"":\""abc\"",\""text\"":\""example\""}"",
            ""rawResult"":""{\""id\"":\""abc\"",\""text\"":\""example\""}""
            }");
            var operation = MobileServiceTableOperationError.Deserialize(serializedError, this.serializer.SerializerSettings);

            Assert.Equal(serializedError["id"], operation.Id);
            Assert.Equal(0, operation.OperationVersion);
            Assert.Equal(serializedError["operationKind"], (int)operation.OperationKind);
            Assert.Equal(serializedError["httpStatus"], (int)operation.Status);
            Assert.Equal(serializedError["tableName"], operation.TableName);
            Assert.Equal(serializedError["tableKind"], (int)operation.TableKind);
            Assert.Equal(serializedError["item"], operation.Item.ToString(Formatting.None));
            Assert.Equal(serializedError["rawResult"], operation.RawResult);
        }
    }
}