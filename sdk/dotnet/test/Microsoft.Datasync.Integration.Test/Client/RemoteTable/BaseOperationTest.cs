// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Test;
using Datasync.Common.Test.Models;
using Microsoft.Datasync.Client;
using Microsoft.Datasync.Client.Serialization;
using Newtonsoft.Json.Linq;
using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace Microsoft.Datasync.Integration.Test.Client.RemoteTable
{
    [ExcludeFromCodeCoverage]
    public abstract class BaseOperationTest : BaseTest
    {
        protected readonly DatasyncClient client;
        protected readonly IRemoteTable soft, table;

        protected BaseOperationTest()
        {
            client = GetMovieClient();
            table = client.GetRemoteTable("movies");
            soft = client.GetRemoteTable("soft");
        }

        protected void AssertJsonDocumentMatches(EFMovie entity, JToken actual)
        {
            var serializer = new ServiceSerializer();
            var expected = (JObject)serializer.Serialize(entity);
            Assert.IsAssignableFrom<JObject>(actual);
            Assert.Equal(expected, (JObject)actual);
        }
    }
}
