﻿using Azure.Mobile.Client.Auth;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Azure.Mobile.Client.Test.Auth
{
    [TestClass]
    public class PreauthorizedTokenCredential_Tests
    {
        [TestMethod]
        public void GetToken_ReturnsToken()
        {
            var token = "foo";
            var credential = new PreauthorizedTokenCredential(token);
            var actual = credential.GetToken(new Core.TokenRequestContext());
            Assert.AreEqual(token, actual.Token);
        }

        [TestMethod]
        public async Task GetTokenAsync_ReturnsToken()
        {
            var token = "foo";
            var credential = new PreauthorizedTokenCredential(token);
            var actual = await credential.GetTokenAsync(new Core.TokenRequestContext());
            Assert.AreEqual(token, actual.Token);
        }

        [TestMethod]
        public void GetToken_Modified_ReturnsModified()
        {
            var token = "foo";
            var credential = new PreauthorizedTokenCredential(token);
            var actual = credential.GetToken(new Core.TokenRequestContext());
            Assert.AreEqual(token, actual.Token);
            credential.AccessToken = "bar";
            var modified = credential.GetToken(new Core.TokenRequestContext());
            Assert.AreEqual("bar", modified.Token);
        }

        [TestMethod]
        public async Task GetTokenAsync_Modified_ReturnsModified()
        {
            var token = "foo";
            var credential = new PreauthorizedTokenCredential(token);
            var actual = await credential.GetTokenAsync(new Core.TokenRequestContext());
            Assert.AreEqual(token, actual.Token);
            credential.AccessToken = "bar";
            var modified = await credential.GetTokenAsync(new Core.TokenRequestContext());
            Assert.AreEqual("bar", modified.Token);
        }
    }
}
