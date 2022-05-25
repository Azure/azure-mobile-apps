// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using Microsoft.WindowsAzure.MobileServices;
using Newtonsoft.Json.Linq;
using Xunit;

namespace MobileClient.Tests
{
    public class JTokenExtensionTests
    {
        [Fact]
        public void IsValidItem_ReturnsFalse_IfObjectIsNull()
        {
            Assert.False(JTokenExtensions.IsValidItem(null));
        }

        [Fact]
        public void IsValidItem_ReturnsFalse_IfObjectIsNotJObject()
        {
            Assert.False(JTokenExtensions.IsValidItem(new JValue(true)));
            Assert.False(JTokenExtensions.IsValidItem(new JArray()));
        }

        [Fact]
        public void IsValidItem_ReturnsFalse_IfObjectIsJObjectWithoutId()
        {
            Assert.False(JTokenExtensions.IsValidItem(new JObject()));
        }

        [Fact]
        public void IsValidItem_ReturnsTrue_IfObjectIsJObjectWithId()
        {
            Assert.True(JTokenExtensions.IsValidItem(new JObject() { { "id", "abc" } }));
        }

        [Fact]
        public void ValidItemOrNull_ReturnsItem_IfItIsValid()
        {
            var item = new JObject() { { "id", "abc" } };
            Assert.Same(item, JTokenExtensions.ValidItemOrNull(item));
        }

        [Fact]
        public void ValidItemOrNull_ReturnsNull_IfItIsInValid()
        {
            var items = new JToken[] { null, new JArray(), new JValue(true), new JObject() };
            foreach (JToken item in items)
            {
                Assert.Null(JTokenExtensions.ValidItemOrNull(item));
            }
        }
    }
}
