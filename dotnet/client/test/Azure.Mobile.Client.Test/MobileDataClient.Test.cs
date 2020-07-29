using Azure.Mobile.Client.Auth;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Security.Permissions;

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

        [TestMethod]
        public void CTor_EndpointWithCreds_CanCreate()
        {
            var endpoint = new Uri("https://localhost:5001");
            var credential = new PreauthorizedTokenCredential("token");
            var actual = new MobileDataClient(endpoint, credential);
            Assert.IsNotNull(actual);
            Assert.AreEqual(endpoint.ToString(), actual.Endpoint.ToString());
            Assert.IsNotNull(actual.ClientOptions);
            Assert.IsNotNull(actual.Credential);
            Assert.AreEqual("token", actual.Credential.GetToken(new Core.TokenRequestContext(), default).Token);
        }
    }
}
