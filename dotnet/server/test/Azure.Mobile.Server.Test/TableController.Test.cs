using Azure.Mobile.Common.Test;
using Microsoft.Extensions.Primitives;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Net.Http;

namespace Azure.Mobile.Server.Test
{
    [TestClass]
    public class TableController_Tests
    {
        private Movie testMovie = new Movie()
        {
            Version = new byte[] { 0x01, 0x00, 0x42, 0x22, 0x47, 0x8F },
            UpdatedAt = DateTimeOffset.Parse("Wed, 30 Jan 2019 13:30:15 GMT"),
            Id = "test1",
            Title = "Test Data"
        };

        private string earlierTestDate = "Tue, 29 Jan 2019 13:30:15 GMT";
        private string laterTestDate = "Thu, 31 Jan 2019 13:30:15 GMT";
        private string matchingETag = "\"AQBCIkeP\"";
        private string nonMatchingETag = "\"Foo\"";

        #region EvaluationPreconditions
        [TestMethod]
        public void EvaluatePreoonditions_NoHeaders_NullItem_200()
        {
            var controller = new MoviesController();
            controller.SetRequest(HttpMethod.Post, "https://foo.com/tables/movies");

            var actual = controller.EvaluatePreconditions(null);
            Assert.AreEqual(200, actual);
        }

        [TestMethod]
        public void EvaluatePreconditions_NoHeaders_TestItem_200()
        {
            var controller = new MoviesController();
            controller.SetRequest(HttpMethod.Post, "https://foo.com/tables/movies");

            var actual = controller.EvaluatePreconditions(testMovie);
            Assert.AreEqual(200, actual);
        }

        [TestMethod]
        public void EvaluatePreconditions_IfMatchStar_NullItem_412()
        {
            var controller = new MoviesController();
            controller.SetRequest(HttpMethod.Post, "https://foo.com/tables/movies", new Dictionary<string, string>()
            {
                { "If-Match", "*" }
            });

            var actual = controller.EvaluatePreconditions(null);
            Assert.AreEqual(412, actual);
        }

        [TestMethod]
        public void EvaluatePreconditions_IfNoneMatchStar_NullItem_200()
        {
            var controller = new MoviesController();
            controller.SetRequest(HttpMethod.Post, "https://foo.com/tables/movies", new Dictionary<string, string>()
            {
                { "If-None-Match", "*" }
            });

            var actual = controller.EvaluatePreconditions(null);
            Assert.AreEqual(200, actual);
        }

        [TestMethod]
        public void EvaluatePreconditions_IfMatch_NullItem_412()
        {
            var controller = new MoviesController();
            controller.SetRequest(HttpMethod.Post, "https://foo.com/tables/movies", new Dictionary<string, string>()
            {
                { "If-Match", matchingETag }
            });

            var actual = controller.EvaluatePreconditions(null);
            Assert.AreEqual(412, actual);
        }

        [TestMethod]
        public void EvaluatePreconditions_IfNoneMatch_NullItem_200()
        {
            var controller = new MoviesController();
            controller.SetRequest(HttpMethod.Post, "https://foo.com/tables/movies", new Dictionary<string, string>()
            {
                { "If-None-Match", matchingETag }
            });

            var actual = controller.EvaluatePreconditions(null);
            Assert.AreEqual(200, actual);
        }

        [TestMethod]
        public void EvaluatePreconditions_IfUnmodifiedSince_NullItem_412()
        {
            var controller = new MoviesController();
            controller.SetRequest(HttpMethod.Post, "https://foo.com/tables/movies", new Dictionary<string, string>()
            {
                { "If-Unmodified-Since", earlierTestDate }
            });

            var actual = controller.EvaluatePreconditions(null);
            Assert.AreEqual(412, actual);
        }

        [TestMethod]
        public void EvaluatePreconditions_IfMatchStar_TestItem_200()
        {
            var controller = new MoviesController();
            controller.SetRequest(HttpMethod.Post, "https://foo.com/tables/movies", new Dictionary<string, string>()
            {
                { "If-Match", "*" }
            });

            var actual = controller.EvaluatePreconditions(testMovie);
            Assert.AreEqual(200, actual);
        }

        [TestMethod]
        public void EvaluatePreconditions_GET_IfNoneMatchStar_TestItem_304()
        {
            var controller = new MoviesController();
            controller.SetRequest(HttpMethod.Get, "https://foo.com/tables/movies", new Dictionary<string, string>()
            {
                { "If-None-Match", "*" }
            });

            var actual = controller.EvaluatePreconditions(testMovie);
            Assert.AreEqual(304, actual);
        }

        [TestMethod]
        public void EvaluatePreconditions_POST_IfNoneMatchStar_TestItem_412()
        {
            var controller = new MoviesController();
            controller.SetRequest(HttpMethod.Post, "https://foo.com/tables/movies", new Dictionary<string, string>()
            {
                { "If-None-Match", "*" }
            });

            var actual = controller.EvaluatePreconditions(testMovie);
            Assert.AreEqual(412, actual);
        }

        [TestMethod]
        public void EvaluatePreconditions_IfMatch_TestItem_200()
        {
            var controller = new MoviesController();
            controller.SetRequest(HttpMethod.Post, "https://foo.com/tables/movies", new Dictionary<string, string>()
            {
                { "If-Match", matchingETag }
            });

            var actual = controller.EvaluatePreconditions(testMovie);
            Assert.AreEqual(200, actual);
        }

        [TestMethod]
        public void EvaluatePreconditions_IfMatch_NonMatchingItem_412()
        {
            var controller = new MoviesController();
            controller.SetRequest(HttpMethod.Post, "https://foo.com/tables/movies", new Dictionary<string, string>()
            {
                { "If-Match", nonMatchingETag }
            });

            var actual = controller.EvaluatePreconditions(testMovie);
            Assert.AreEqual(412, actual);
        }

        [TestMethod]
        public void EvaluatePreconditions_POST_IfNoneMatch_TestItem_412()
        {
            var controller = new MoviesController();
            controller.SetRequest(HttpMethod.Post, "https://foo.com/tables/movies", new Dictionary<string, string>()
            {
                { "If-None-Match", matchingETag }
            });

            var actual = controller.EvaluatePreconditions(testMovie);
            Assert.AreEqual(412, actual);
        }

        [TestMethod]
        public void EvaluatePreconditions_GET_IfNoneMatch_TestItem_304()
        {
            var controller = new MoviesController();
            controller.SetRequest(HttpMethod.Get, "https://foo.com/tables/movies", new Dictionary<string, string>()
            {
                { "If-None-Match", matchingETag }
            });

            var actual = controller.EvaluatePreconditions(testMovie);
            Assert.AreEqual(304, actual);
        }

        [TestMethod]
        public void EvaluatePreconditions_IfNoneMatch_NonMatchingItem_200()
        {
            var controller = new MoviesController();
            controller.SetRequest(HttpMethod.Post, "https://foo.com/tables/movies", new Dictionary<string, string>()
            {
                { "If-None-Match", nonMatchingETag }
            });

            var actual = controller.EvaluatePreconditions(testMovie);
            Assert.AreEqual(200, actual);
        }

        [TestMethod]
        public void EvaluatePreconditions_IfModifiedSinceEarlier_GET_TestItem_200()
        {
            var controller = new MoviesController();
            controller.SetRequest(HttpMethod.Get, "https://foo.com/tables/movies", new Dictionary<string, string>()
            {
                { "If-Modified-Since", earlierTestDate }
            });

            var actual = controller.EvaluatePreconditions(testMovie);
            Assert.AreEqual(200, actual);
        }

        [TestMethod]
        public void EvaluatePreconditions_IfModifiedSinceLater_GET_NullItem_304()
        {
            var controller = new MoviesController();
            controller.SetRequest(HttpMethod.Get, "https://foo.com/tables/movies", new Dictionary<string, string>()
            {
                { "If-Modified-Since", laterTestDate }
            });

            var actual = controller.EvaluatePreconditions(null);
            Assert.AreEqual(304, actual);
        }

        [TestMethod]
        public void EvaluatePreconditions_IfModifiedSinceLater_GET_TestItem_304()
        {
            var controller = new MoviesController();
            controller.SetRequest(HttpMethod.Get, "https://foo.com/tables/movies", new Dictionary<string, string>()
            {
                { "If-Modified-Since", laterTestDate }
            });

            var actual = controller.EvaluatePreconditions(testMovie);
            Assert.AreEqual(304, actual);
        }

        [TestMethod]
        public void EvaluatePreconditions_IfModifiedSinceEarlier_POST_TestItem_200()
        {
            var controller = new MoviesController();
            controller.SetRequest(HttpMethod.Post, "https://foo.com/tables/movies", new Dictionary<string, string>()
            {
                { "If-Modified-Since", earlierTestDate }
            });

            var actual = controller.EvaluatePreconditions(testMovie);
            Assert.AreEqual(200, actual);
        }

        [TestMethod]
        public void EvaluatePreconditions_IfModifiedSinceLater_POST_TestItem_200()
        {
            var controller = new MoviesController();
            controller.SetRequest(HttpMethod.Post, "https://foo.com/tables/movies", new Dictionary<string, string>()
            {
                { "If-Modified-SInce", laterTestDate }
            });

            var actual = controller.EvaluatePreconditions(testMovie);
            Assert.AreEqual(200, actual);
        }

        [TestMethod]
        public void EvaluatePreconditions_IfUnmodifiedSinceEarlier_GET_TestItem_412()
        {
            var controller = new MoviesController();
            controller.SetRequest(HttpMethod.Get, "https://foo.com/tables/movies", new Dictionary<string, string>()
            {
                { "If-Unmodified-Since", earlierTestDate }
            });

            var actual = controller.EvaluatePreconditions(testMovie);
            Assert.AreEqual(412, actual);
        }

        [TestMethod]
        public void EvaluatePreconditions_IfUnmodifiedSinceLater_GET_TestItem_200()
        {
            var controller = new MoviesController();
            controller.SetRequest(HttpMethod.Get, "https://foo.com/tables/movies", new Dictionary<string, string>()
            {
                { "If-Unmodified-Since", laterTestDate }
            });

            var actual = controller.EvaluatePreconditions(testMovie);
            Assert.AreEqual(200, actual);
        }

        [TestMethod]
        public void EvaluatePreconditions_IfUnmodifiedSInceEarlier_POST_TestItem_412()
        {
            var controller = new MoviesController();
            controller.SetRequest(HttpMethod.Post, "https://foo.com/tables/movies", new Dictionary<string, string>()
            {
                { "If-Unmodified-Since", earlierTestDate }
            });

            var actual = controller.EvaluatePreconditions(testMovie);
            Assert.AreEqual(412, actual);
        }

        [TestMethod]
        public void EvaluatePreconditions_IfUnmodifiedSinceLater_POST_TestItem_200()
        {
            var controller = new MoviesController();
            controller.SetRequest(HttpMethod.Post, "https://foo.com/tables/movies", new Dictionary<string, string>()
            {
                { "If-Unmodified-Since", laterTestDate }
            });

            var actual = controller.EvaluatePreconditions(testMovie);
            Assert.AreEqual(200, actual);
        }
        #endregion
    }
}
