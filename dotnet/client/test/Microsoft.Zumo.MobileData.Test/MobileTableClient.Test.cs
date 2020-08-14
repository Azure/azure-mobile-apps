// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Zumo.MobileData.Test.Helpers;
using System;

namespace Microsoft.Zumo.MobileData.Test
{
    [TestClass]
    public class MobileTableClient_Test : BaseTest
    {
        [TestMethod]
        public void Ctor_WithEndpoint_CanCreate()
        {
            var endpoint = new Uri("https://localhost:5001");
            var actual = new MobileTableClient(endpoint);

            Assert.IsNotNull(actual);
            Assert.AreEqual(endpoint.ToString(), actual.Endpoint.ToString());
            Assert.IsNotNull(actual.ClientOptions);
            Assert.IsNull(actual.Credential);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Ctor_WithRelativeEndpoint_Throws()
        {
            var baseUri = new Uri("https://localhost:5001");
            var relUri = new Uri("https://localhost:5001/tables");
            _ = new MobileTableClient(relUri.MakeRelativeUri(baseUri));
            Assert.Fail("ArgumentException expected");
        }

        [TestMethod]
        public void Ctor_WithEndpointAndCredential_CanCreate()
        {
            var endpoint = new Uri("https://localhost:5001");
            var credential = new SimpleTokenCredential("token");
            var actual = new MobileTableClient(endpoint, credential);

            Assert.IsNotNull(actual);
            Assert.AreEqual(endpoint.ToString(), actual.Endpoint.ToString());
            Assert.IsNotNull(actual.ClientOptions);
            Assert.IsNotNull(actual.Credential);
            Assert.AreEqual("token", actual.Credential.GetToken(new Azure.Core.TokenRequestContext(), default).Token);
        }

        [TestMethod]
        public void Ctor_WithEndpointCredentialsAndOptions_CanCreate()
        {
            var endpoint = new Uri("https://localhost:5001");
            var credential = new SimpleTokenCredential("token");
            var options = new MobileTableClientOptions();
            options.Diagnostics.ApplicationId = "foo";

            var actual = new MobileTableClient(endpoint, credential, options);

            Assert.IsNotNull(actual);
            Assert.AreEqual(endpoint.ToString(), actual.Endpoint.ToString());
            Assert.IsNotNull(actual.ClientOptions);
            Assert.AreEqual("foo", actual.ClientOptions.Diagnostics.ApplicationId);
            Assert.IsNotNull(actual.Credential);
            Assert.AreEqual("token", actual.Credential.GetToken(new Azure.Core.TokenRequestContext(), default).Token);
        }

        [TestMethod]
        public void Ctor_WithEndpointAndOptions_CanCreate()
        {
            var endpoint = new Uri("https://localhost:5001");
            var options = new MobileTableClientOptions();
            options.Diagnostics.ApplicationId = "foo";

            var actual = new MobileTableClient(endpoint, options);

            Assert.IsNotNull(actual);
            Assert.AreEqual(endpoint.ToString(), actual.Endpoint.ToString());
            Assert.IsNotNull(actual.ClientOptions);
            Assert.AreEqual("foo", actual.ClientOptions.Diagnostics.ApplicationId);
            Assert.IsNull(actual.Credential);
        }

        [TestMethod]
        public void GetTable_ReturnsMobileTable()
        {
            var endpoint = new Uri("https://localhost:5001");
            var client = new MobileTableClient(endpoint);
            var actual = client.GetTable<Movie>();

            Assert.IsNotNull(actual);
            Assert.IsInstanceOfType(actual, typeof(MobileTable<Movie>));
            Assert.AreEqual("https://localhost:5001/tables/movies", actual.Client.Endpoint.ToString());
        }

        [TestMethod]
        public void GetTable_WithString_ReturnsMobileTable()
        {
            var endpoint = new Uri("https://localhost:5001");
            var client = new MobileTableClient(endpoint);
            var actual = client.GetTable<Movie>("foo");

            Assert.IsNotNull(actual);
            Assert.IsInstanceOfType(actual, typeof(MobileTable<Movie>));
            Assert.AreEqual("https://localhost:5001/foo", actual.Client.Endpoint.ToString());
        }

        [TestMethod]
        public void GetTable_WithNull_ReturnsMobileTable()
        {
            var endpoint = new Uri("https://localhost:5001");
            var client = new MobileTableClient(endpoint);
            var actual = client.GetTable<Movie>(null);

            Assert.IsNotNull(actual);
            Assert.IsInstanceOfType(actual, typeof(MobileTable<Movie>));
            Assert.AreEqual("https://localhost:5001/tables/movies", actual.Client.Endpoint.ToString());
        }

        [TestMethod]
        public void GetTable_WithEmptyString_ReturnsMobileTable()
        {
            var endpoint = new Uri("https://localhost:5001");
            var client = new MobileTableClient(endpoint);
            var actual = client.GetTable<Movie>("");

            Assert.IsNotNull(actual);
            Assert.IsInstanceOfType(actual, typeof(MobileTable<Movie>));
            Assert.AreEqual("https://localhost:5001/tables/movies", actual.Client.Endpoint.ToString());
        }
    }
}
