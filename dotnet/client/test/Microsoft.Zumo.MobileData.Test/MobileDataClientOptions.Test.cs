// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text.Json;

namespace Microsoft.Zumo.MobileData.Test
{
    [TestClass]
    public class MobileDataClientOptions_Tests
    {
        [TestMethod]
        public void SetJsonSerializerOptions_RoundTrips()
        {
            var actual = new MobileTableClientOptions
            {
                JsonSerializerOptions = new JsonSerializerOptions
                {
                    IgnoreNullValues = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                }
            };

            Assert.IsTrue(actual.JsonSerializerOptions.IgnoreNullValues);
        }
    }
}
