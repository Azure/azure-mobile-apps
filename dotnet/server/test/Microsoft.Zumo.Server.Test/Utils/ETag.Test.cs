// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Zumo.Server.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Headers;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace Microsoft.Zumo.Server.Test.Utils
{
    [TestClass]
    public class ETag_Test
    {
        #region Test Data
        class Entity : ITableData
        {
            public string Id { get; set; }
            public byte[] Version { get; set; }
            public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
            public DateTimeOffset UpdatedAt { get; set; }
            public bool Deleted { get; set; }
        }

        private readonly Entity testEntity = new Entity()
        {
            Version = new byte[] { 0x01, 0x00, 0x42, 0x22, 0x47, 0x8F },
            UpdatedAt = DateTimeOffset.Parse("Wed, 30 Jan 2019 13:30:15 GMT"),
            Id = "test1"
        };

        private readonly string earlierTestDate = "Tue, 29 Jan 2019 13:30:15 GMT";
        private readonly string laterTestDate = "Thu, 31 Jan 2019 13:30:15 GMT";
        private readonly string matchingETag = "\"AQBCIkeP\"";
        private readonly string nonMatchingETag = "\"Foo\"";

        private RequestHeaders GetRequestHeaders(string headerName, string headerValue)
        {
            HeaderDictionary dict = new HeaderDictionary(new Dictionary<string, StringValues>()
            {
                { headerName, headerValue }
            });
            return new RequestHeaders(dict);
        }

        private RequestHeaders GetEmptyRequestHeaders()
            => new RequestHeaders(new HeaderDictionary());
        #endregion

        #region ETag.Matches
        [TestMethod]
        public void Matches_AisWeak_False()
        {
            var a = new EntityTagHeaderValue("\"tag-a\"", true);
            var b = new EntityTagHeaderValue("\"tag-a\"", false);
            Assert.IsFalse(ETag.Matches(a, b));
        }

        [TestMethod]
        public void Matches_BisWeak_False()
        {
            var a = new EntityTagHeaderValue("\"tag-a\"", false);
            var b = new EntityTagHeaderValue("\"tag-a\"", true);
            Assert.IsFalse(ETag.Matches(a, b));
        }

        [TestMethod]
        public void Matches_AStar_True()
        {
            var a = new EntityTagHeaderValue("*");
            var b = new EntityTagHeaderValue("\"tag-a\"");
            Assert.IsTrue(ETag.Matches(a, b));
        }

        [TestMethod]
        public void Matches_BStar_True()
        {
            var b = new EntityTagHeaderValue("*");
            var a = new EntityTagHeaderValue("\"tag-a\"");
            Assert.IsTrue(ETag.Matches(a, b));
        }

        [TestMethod]
        public void Matches_AB_True()
        {
            var a = new EntityTagHeaderValue("\"tag-a\"");
            var b = new EntityTagHeaderValue("\"tag-a\"");
            Assert.IsTrue(ETag.Matches(a, b));
        }

        [TestMethod]
        public void Matches_AnotB_True()
        {
            var a = new EntityTagHeaderValue("\"tag-a\"");
            var b = new EntityTagHeaderValue("\"tag-b\"");
            Assert.IsFalse(ETag.Matches(a, b));
        }
        #endregion ETag.Matches

        #region ETag.FromByteArray
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void FromByteArray_Null_ThrowsArgumentNullException()
        {
            _ = ETag.FromByteArray(null);
            Assert.Fail("ArgumentNullException expected");
        }

        [TestMethod]
        public void FromByteArray_Empty_ReturnsEmptyString()
        {
            var actual = ETag.FromByteArray(new byte[] { });
            var expected = "\"\"";
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void FromByteArray_ValidData_ReturnsExpected()
        {
            var testData = new byte[] { 0x01, 0x00, 0x42, 0x22, 0x47, 0x8F };
            var actual = ETag.FromByteArray(testData);
            var expected = "\"AQBCIkeP\"";
            Assert.AreEqual(expected, actual);
        }
        #endregion

        #region ETag.EvaluationPreconditions
        [TestMethod]
        public void EvaluatePreoonditions_NoHeaders_NullItem_200()
        {
            var actual = ETag.EvaluatePreconditions<Entity>(null, GetEmptyRequestHeaders(), false);
            Assert.AreEqual(200, actual);
        }

        [TestMethod]
        public void EvaluatePreconditions_NoHeaders_TestItem_200()
        {
            var actual = ETag.EvaluatePreconditions<Entity>(testEntity, GetEmptyRequestHeaders(), false);
            Assert.AreEqual(200, actual);
        }

        [TestMethod]
        public void EvaluatePreconditions_IfMatchStar_NullItem_412()
        {
            var actual = ETag.EvaluatePreconditions<Entity>(null, GetRequestHeaders("If-Match", "*"), false);
            Assert.AreEqual(412, actual);
        }

        [TestMethod]
        public void EvaluatePreconditions_IfNoneMatchStar_NullItem_200()
        {
            var actual = ETag.EvaluatePreconditions<Entity>(null, GetRequestHeaders("If-None-Match", "*"), false);
            Assert.AreEqual(200, actual);
        }

        [TestMethod]
        public void EvaluatePreconditions_IfMatch_NullItem_412()
        {
            var actual = ETag.EvaluatePreconditions<Entity>(null, GetRequestHeaders("If-Match", matchingETag), false);
            Assert.AreEqual(412, actual);
        }

        [TestMethod]
        public void EvaluatePreconditions_IfNoneMatch_NullItem_200()
        {
            var actual = ETag.EvaluatePreconditions<Entity>(null, GetRequestHeaders("If-None-Match", matchingETag), false);
            Assert.AreEqual(200, actual);
        }

        [TestMethod]
        public void EvaluatePreconditions_IfUnmodifiedSince_NullItem_412()
        {
            var actual = ETag.EvaluatePreconditions<Entity>(null, GetRequestHeaders("If-Unmodified-Since", earlierTestDate), false);
            Assert.AreEqual(412, actual);
        }

        [TestMethod]
        public void EvaluatePreconditions_IfMatchStar_TestItem_200()
        {
            var actual = ETag.EvaluatePreconditions<Entity>(testEntity, GetRequestHeaders("If-Match", "*"), false);
            Assert.AreEqual(200, actual);
        }

        [TestMethod]
        public void EvaluatePreconditions_GET_IfNoneMatchStar_TestItem_304()
        {
            var actual = ETag.EvaluatePreconditions<Entity>(testEntity, GetRequestHeaders("If-None-Match", "*"), true);
            Assert.AreEqual(304, actual);
        }

        [TestMethod]
        public void EvaluatePreconditions_POST_IfNoneMatchStar_TestItem_412()
        {
            var actual = ETag.EvaluatePreconditions<Entity>(testEntity, GetRequestHeaders("If-None-Match", "*"), false);
            Assert.AreEqual(412, actual);
        }

        [TestMethod]
        public void EvaluatePreconditions_IfMatch_TestItem_200()
        {
            var actual = ETag.EvaluatePreconditions<Entity>(testEntity, GetRequestHeaders("If-Match", matchingETag), false);
            Assert.AreEqual(200, actual);
        }

        [TestMethod]
        public void EvaluatePreconditions_IfMatch_NonMatchingItem_412()
        {
            var actual = ETag.EvaluatePreconditions<Entity>(testEntity, GetRequestHeaders("If-Match", nonMatchingETag), false);
            Assert.AreEqual(412, actual);
        }

        [TestMethod]
        public void EvaluatePreconditions_POST_IfNoneMatch_TestItem_412()
        {
            var actual = ETag.EvaluatePreconditions<Entity>(testEntity, GetRequestHeaders("If-None-Match", matchingETag), false);
            Assert.AreEqual(412, actual);
        }

        [TestMethod]
        public void EvaluatePreconditions_GET_IfNoneMatch_TestItem_304()
        {
            var actual = ETag.EvaluatePreconditions<Entity>(testEntity, GetRequestHeaders("If-None-Match", matchingETag), true);
            Assert.AreEqual(304, actual);
        }

        [TestMethod]
        public void EvaluatePreconditions_IfNoneMatch_NonMatchingItem_200()
        {
            var actual = ETag.EvaluatePreconditions<Entity>(testEntity, GetRequestHeaders("If-None-Match", nonMatchingETag), false);
            Assert.AreEqual(200, actual);
        }

        [TestMethod]
        public void EvaluatePreconditions_IfModifiedSinceEarlier_GET_NullItem_200()
        {
            var actual = ETag.EvaluatePreconditions<Entity>(null, GetRequestHeaders("If-Modified-Since", earlierTestDate), true);
            Assert.AreEqual(304, actual);
        }

        [TestMethod]
        public void EvaluatePreconditions_IfModifiedSinceEarlier_GET_TestItem_200()
        {
            var actual = ETag.EvaluatePreconditions<Entity>(testEntity, GetRequestHeaders("If-Modified-Since", earlierTestDate), true);
            Assert.AreEqual(200, actual);
        }

        [TestMethod]
        public void EvaluatePreconditions_IfModifiedSinceLater_GET_NullItem_304()
        {
            var actual = ETag.EvaluatePreconditions<Entity>(testEntity, GetRequestHeaders("If-Modified-Since", laterTestDate), true);
            Assert.AreEqual(304, actual);
        }

        [TestMethod]
        public void EvaluatePreconditions_IfModifiedSinceLater_GET_TestItem_304()
        {
            var actual = ETag.EvaluatePreconditions<Entity>(testEntity, GetRequestHeaders("If-Modified-Since", laterTestDate), true);
            Assert.AreEqual(304, actual);
        }

        [TestMethod]
        public void EvaluatePreconditions_IfModifiedSinceEarlier_POST_TestItem_200()
        {
            var actual = ETag.EvaluatePreconditions<Entity>(testEntity, GetRequestHeaders("If-Modified-Since", earlierTestDate), false);
            Assert.AreEqual(200, actual);
        }

        [TestMethod]
        public void EvaluatePreconditions_IfModifiedSinceLater_POST_TestItem_200()
        {
            var actual = ETag.EvaluatePreconditions<Entity>(testEntity, GetRequestHeaders("If-Modified-SInce", laterTestDate), false);
            Assert.AreEqual(200, actual);
        }

        [TestMethod]
        public void EvaluatePreconditions_IfUnmodifiedSinceEarlier_GET_TestItem_412()
        {
            var actual = ETag.EvaluatePreconditions<Entity>(testEntity, GetRequestHeaders("If-Unmodified-Since", earlierTestDate), true);
            Assert.AreEqual(412, actual);
        }

        [TestMethod]
        public void EvaluatePreconditions_IfUnmodifiedSinceLater_GET_TestItem_200()
        {
            var actual = ETag.EvaluatePreconditions<Entity>(testEntity, GetRequestHeaders("If-Unmodified-Since", laterTestDate), true);
            Assert.AreEqual(200, actual);
        }

        [TestMethod]
        public void EvaluatePreconditions_IfUnmodifiedSInceEarlier_POST_TestItem_412()
        {
            var actual = ETag.EvaluatePreconditions<Entity>(testEntity, GetRequestHeaders("If-Unmodified-Since", earlierTestDate), false);
            Assert.AreEqual(412, actual);
        }

        [TestMethod]
        public void EvaluatePreconditions_IfUnmodifiedSinceLater_POST_TestItem_200()
        {
            var actual = ETag.EvaluatePreconditions<Entity>(testEntity, GetRequestHeaders("If-Unmodified-Since", laterTestDate), false);
            Assert.AreEqual(200, actual);
        }
        #endregion

        #region ETag.IsValidETag
        [TestMethod]
        public void IsValidETag_Validity()
        {
            Assert.IsTrue(ETag.IsValidETag(Guid.NewGuid().ToByteArray()));
            Assert.IsFalse(ETag.IsValidETag(null));
            Assert.IsFalse(ETag.IsValidETag(new byte[] { }));
        }
        #endregion
    }
}
