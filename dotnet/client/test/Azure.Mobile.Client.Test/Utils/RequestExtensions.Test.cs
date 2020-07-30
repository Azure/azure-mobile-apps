using Azure.Core;
using Azure.Mobile.Client.Test.Helpers;
using Azure.Mobile.Client.Test.Table;
using Azure.Mobile.Client.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Azure.Mobile.Client.Test.Utils
{
    [TestClass]
    public class RequestExtensions_Tests
    {
        [TestMethod]
        public void IfMatch_Added()
        {
            var client = new MobileDataClient(new Uri("https://localhost:5001"));
            var table = client.GetTable<Movie>();
            var request = table.Pipeline.CreateRequest();
            var conditions = new MatchConditions { IfMatch = new ETag("foo") };
            request.ApplyConditionalHeaders(conditions);
            HttpAssert.HeaderIsEqual("If-Match", "\"foo\"", request);
        }

        [TestMethod]
        public void IfNoneMatch_Added()
        {
            var client = new MobileDataClient(new Uri("https://localhost:5001"));
            var table = client.GetTable<Movie>();
            var request = table.Pipeline.CreateRequest();
            var conditions = new MatchConditions { IfNoneMatch = new ETag("foo") };
            request.ApplyConditionalHeaders(conditions);
            HttpAssert.HeaderIsEqual("If-None-Match", "\"foo\"", request);
        }

        [TestMethod]
        public void IfMatch_All_Added()
        {
            var client = new MobileDataClient(new Uri("https://localhost:5001"));
            var table = client.GetTable<Movie>();
            var request = table.Pipeline.CreateRequest();
            var conditions = new MatchConditions { IfMatch = ETag.All };
            request.ApplyConditionalHeaders(conditions);
            HttpAssert.HeaderIsEqual("If-Match", "*", request);
        }

        [TestMethod]
        public void IfNoneMatch_All_Added()
        {
            var client = new MobileDataClient(new Uri("https://localhost:5001"));
            var table = client.GetTable<Movie>();
            var request = table.Pipeline.CreateRequest();
            var conditions = new MatchConditions { IfNoneMatch = ETag.All };
            request.ApplyConditionalHeaders(conditions);
            HttpAssert.HeaderIsEqual("If-None-Match", "*", request);
        }

        [TestMethod]
        public void IfMatch_And_IfNoneMatch_Added()
        {
            var client = new MobileDataClient(new Uri("https://localhost:5001"));
            var table = client.GetTable<Movie>();
            var request = table.Pipeline.CreateRequest();
            var conditions = new MatchConditions { IfMatch = new ETag("foo"), IfNoneMatch = new ETag("bar") };
            request.ApplyConditionalHeaders(conditions);
            HttpAssert.HeaderIsEqual("If-Match", "\"foo\"", request);
            HttpAssert.HeaderIsEqual("If-None-Match", "\"bar\"", request);
        }

        [TestMethod]
        public void IfModifiedSince_Added()
        {
            var client = new MobileDataClient(new Uri("https://localhost:5001"));
            var table = client.GetTable<Movie>();
            var request = table.Pipeline.CreateRequest();
            var conditions = new RequestConditions { IfModifiedSince = DateTimeOffset.Parse("2020-01-01T07:30:06Z") };
            request.ApplyConditionalHeaders(conditions);
            HttpAssert.HeaderIsEqual("If-Modified-Since", "Wed, 01 Jan 2020 07:30:06 GMT", request);
        }

        [TestMethod]
        public void IfUnmodifiedSince_Added()
        {
            var client = new MobileDataClient(new Uri("https://localhost:5001"));
            var table = client.GetTable<Movie>();
            var request = table.Pipeline.CreateRequest();
            var conditions = new RequestConditions { IfUnmodifiedSince = DateTimeOffset.Parse("2020-01-01T07:30:06Z") };
            request.ApplyConditionalHeaders(conditions);
            HttpAssert.HeaderIsEqual("If-Unmodified-Since", "Wed, 01 Jan 2020 07:30:06 GMT", request);
        }
    }
}
