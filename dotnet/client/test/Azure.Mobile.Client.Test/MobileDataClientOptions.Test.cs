using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text.Json;

namespace Azure.Mobile.Client.Test
{
    [TestClass]
    public class MobileDataClientOptions_Tests
    {
        [TestMethod]
        public void SetJsonSerializerOptions_RoundTrips()
        {
            var actual = new MobileDataClientOptions();
            actual.JsonSerializerOptions = new JsonSerializerOptions
            {
                IgnoreNullValues = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            Assert.IsTrue(actual.JsonSerializerOptions.IgnoreNullValues);
        }
    }
}
