using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace Azure.Mobile.Client.Test
{
    [TestClass]
    public class MobileDataClient_Tests
    {
        [TestMethod]
        public void Ctor_Endpoint_CanCreate()
        {
            var endpoint = new Uri("https://localhost:5001");
            var actual = new MobileDataClient(endpoint);
            Assert.IsNotNull(actual);
            Assert.AreEqual(endpoint.ToString(), actual.Endpoint.ToString());
            Assert.IsNotNull(actual.ClientOptions);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Ctor_RelativeEndpoint_Throws()
        {
            var baseUri = new Uri("https://localhost:5001");
            var relUri = new Uri("https://localhost:5001/tables");
            var actual = new MobileDataClient(relUri.MakeRelativeUri(baseUri));
            Assert.Fail("ArgumentException expected");
        }
    }
}
