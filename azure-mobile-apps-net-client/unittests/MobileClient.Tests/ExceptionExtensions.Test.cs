// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using Microsoft.WindowsAzure.MobileServices;
using System;
using System.Net;
using System.Net.Http;
using Xunit;

namespace MobileClient.Tests
{
    public class ExceptionExtensionsTests
    {
        [Fact]
        public void IsNetworkError_ReturnsTrue_OnNetworkErrors()
        {
            Assert.True(ExceptionExtensions.IsNetworkError(new HttpRequestException()));
        }

        [Fact]
        public void IsNetworkError_ReturnsFalse_OnOtherErrors()
        {
            Assert.False(ExceptionExtensions.IsNetworkError(new Exception()));
            Assert.False(ExceptionExtensions.IsNetworkError(new MobileServiceInvalidOperationException(null, new HttpRequestMessage(), new HttpResponseMessage())));
        }

        [Fact]
        public void IsAuthenticationError_ReturnsTrue_OnAuthErrors()
        {
            Assert.True(ExceptionExtensions.IsAuthenticationError(new MobileServiceInvalidOperationException(null, new HttpRequestMessage(), new HttpResponseMessage(HttpStatusCode.Unauthorized))));
        }

        [Fact]
        public void IsAuthenticationError_ReturnsFalse_OnOtherErrors()
        {
            Assert.False(ExceptionExtensions.IsAuthenticationError(new Exception()));
            Assert.False(ExceptionExtensions.IsAuthenticationError(new MobileServiceInvalidOperationException(null, new HttpRequestMessage(), new HttpResponseMessage())));
        }
    }
}
