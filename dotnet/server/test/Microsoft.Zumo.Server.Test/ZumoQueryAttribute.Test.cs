// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Net;

namespace Microsoft.Zumo.Server.Test
{
    [TestClass]
    public class ZumoQueryAttribute_Tests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void OnActionExecuted_Throws_OnNull()
        {
            var sut = new ZumoQueryAttribute();
            sut.OnActionExecuted(null);
            Assert.Fail("ArgumentNullException expected");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ValidateQuery_Throws_OnNullRequest()
        {
            var sut = new ZumoQueryAttribute();
            sut.ValidateQuery(null, null);
            Assert.Fail("ArgumentNullException expected");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ValidateQuery_Throws_OnNullQueryOptions()
        {
            var sut = new ZumoQueryAttribute();
            Mock<HttpRequest> mock = new Mock<HttpRequest>();
            sut.ValidateQuery(mock.Object, null);
            Assert.Fail("ArgumentNullException expected");
        }

        [DataTestMethod]
        [DataRow(StatusCodes.Status100Continue, false)]
        [DataRow(StatusCodes.Status101SwitchingProtocols, false)]
        [DataRow(StatusCodes.Status102Processing, false)]
        [DataRow(StatusCodes.Status200OK, true)]
        [DataRow(StatusCodes.Status201Created, true)]
        [DataRow(StatusCodes.Status202Accepted, true)]
        [DataRow(StatusCodes.Status203NonAuthoritative, true)]
        [DataRow(StatusCodes.Status204NoContent, true)]
        [DataRow(StatusCodes.Status205ResetContent, true)]
        [DataRow(StatusCodes.Status206PartialContent, true)]
        [DataRow(StatusCodes.Status207MultiStatus, true)]
        [DataRow(StatusCodes.Status208AlreadyReported, true)]
        [DataRow(StatusCodes.Status226IMUsed, true)]
        [DataRow(StatusCodes.Status300MultipleChoices, false)]
        [DataRow(StatusCodes.Status301MovedPermanently, false)]
        [DataRow(StatusCodes.Status302Found, false)]
        [DataRow(StatusCodes.Status303SeeOther, false)]
        [DataRow(StatusCodes.Status304NotModified, false)]
        [DataRow(StatusCodes.Status305UseProxy, false)]
        [DataRow(StatusCodes.Status306SwitchProxy, false)]
        [DataRow(StatusCodes.Status307TemporaryRedirect, false)]
        [DataRow(StatusCodes.Status308PermanentRedirect, false)]
        [DataRow(StatusCodes.Status400BadRequest, false)]
        [DataRow(StatusCodes.Status401Unauthorized, false)]
        [DataRow(StatusCodes.Status402PaymentRequired, false)]
        [DataRow(StatusCodes.Status403Forbidden, false)]
        [DataRow(StatusCodes.Status404NotFound, false)]
        [DataRow(StatusCodes.Status405MethodNotAllowed, false)]
        [DataRow(StatusCodes.Status406NotAcceptable, false)]
        [DataRow(StatusCodes.Status407ProxyAuthenticationRequired, false)]
        [DataRow(StatusCodes.Status408RequestTimeout, false)]
        [DataRow(StatusCodes.Status409Conflict, false)]
        [DataRow(StatusCodes.Status410Gone, false)]
        [DataRow(StatusCodes.Status411LengthRequired, false)]
        [DataRow(StatusCodes.Status412PreconditionFailed, false)]
        [DataRow(StatusCodes.Status413PayloadTooLarge, false)]
        [DataRow(StatusCodes.Status414RequestUriTooLong, false)]
        [DataRow(StatusCodes.Status415UnsupportedMediaType, false)]
        [DataRow(StatusCodes.Status423Locked, false)]
        [DataRow(StatusCodes.Status500InternalServerError, false)]
        public void IsSuccessStatusCode_Correct(int statusCode, bool expected)
        {
            var sut = new ZumoQueryAttribute();
            Assert.AreEqual(expected, sut.IsSuccessStatusCode(statusCode));
        }
    }
}
