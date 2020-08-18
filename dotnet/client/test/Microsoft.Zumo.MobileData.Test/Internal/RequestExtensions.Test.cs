// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Azure;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Zumo.MobileData.Internal;
using Microsoft.Zumo.MobileData.Test.Helpers;
using System;

namespace Microsoft.Zumo.MobileData.Test.Internal
{
    [TestClass]
    public class RequestExtensions_Tests
    {
        [TestMethod]
        public void ApplyConditionalHeader_IfMatch_Added()
        {
            var client = new MobileTableClient(new Uri("https://localhost:5001"));
            var table = client.GetTable<Movie>();
            var request = table.Client.Pipeline.CreateRequest();
            var conditions = new MatchConditions { IfMatch = new ETag("foo") };
            request.ApplyConditionalHeaders(conditions);
            HttpAssert.HeaderIsEqual("If-Match", "\"foo\"", request);
            HttpAssert.HeaderIsNotPresent("If-None-Match", request);
            HttpAssert.HeaderIsNotPresent("If-Modified-Since", request);
            HttpAssert.HeaderIsNotPresent("If-Unmodified-Since", request);
        }

        [TestMethod]
        public void ApplyConditionalHeader_IfNoneMatch_Added()
        {
            var client = new MobileTableClient(new Uri("https://localhost:5001"));
            var table = client.GetTable<Movie>();
            var request = table.Client.Pipeline.CreateRequest();
            var conditions = new MatchConditions { IfNoneMatch = new ETag("foo") };
            request.ApplyConditionalHeaders(conditions);
            HttpAssert.HeaderIsEqual("If-None-Match", "\"foo\"", request);
            HttpAssert.HeaderIsNotPresent("If-Match", request);
            HttpAssert.HeaderIsNotPresent("If-Modified-Since", request);
            HttpAssert.HeaderIsNotPresent("If-Unmodified-Since", request);
        }

        [TestMethod]
        public void ApplyConditionalHeader_IfMatch_All_Added()
        {
            var client = new MobileTableClient(new Uri("https://localhost:5001"));
            var table = client.GetTable<Movie>();
            var request = table.Client.Pipeline.CreateRequest();
            var conditions = new MatchConditions { IfMatch = ETag.All };
            request.ApplyConditionalHeaders(conditions);
            HttpAssert.HeaderIsEqual("If-Match", "*", request);
            HttpAssert.HeaderIsNotPresent("If-None-Match", request);
            HttpAssert.HeaderIsNotPresent("If-Modified-Since", request);
            HttpAssert.HeaderIsNotPresent("If-Unmodified-Since", request);
        }

        [TestMethod]
        public void ApplyConditionalHeader_IfNoneMatch_All_Added()
        {
            var client = new MobileTableClient(new Uri("https://localhost:5001"));
            var table = client.GetTable<Movie>();
            var request = table.Client.Pipeline.CreateRequest();
            var conditions = new MatchConditions { IfNoneMatch = ETag.All };
            request.ApplyConditionalHeaders(conditions);
            HttpAssert.HeaderIsEqual("If-None-Match", "*", request);
            HttpAssert.HeaderIsNotPresent("If-Match", request);
            HttpAssert.HeaderIsNotPresent("If-Modified-Since", request);
            HttpAssert.HeaderIsNotPresent("If-Unmodified-Since", request);
        }

        [TestMethod]
        public void ApplyConditionalHeader_IfMatch_And_IfNoneMatch_Added()
        {
            var client = new MobileTableClient(new Uri("https://localhost:5001"));
            var table = client.GetTable<Movie>();
            var request = table.Client.Pipeline.CreateRequest();
            var conditions = new MatchConditions { IfMatch = new ETag("foo"), IfNoneMatch = new ETag("bar") };
            request.ApplyConditionalHeaders(conditions);
            HttpAssert.HeaderIsEqual("If-Match", "\"foo\"", request);
            HttpAssert.HeaderIsEqual("If-None-Match", "\"bar\"", request);
            HttpAssert.HeaderIsNotPresent("If-Modified-Since", request);
            HttpAssert.HeaderIsNotPresent("If-Unmodified-Since", request);
        }

        [TestMethod]
        public void ApplyConditionalHeader_IfModifiedSince_Added()
        {
            var client = new MobileTableClient(new Uri("https://localhost:5001"));
            var table = client.GetTable<Movie>();
            var request = table.Client.Pipeline.CreateRequest();
            var conditions = new RequestConditions { IfModifiedSince = DateTimeOffset.Parse("2020-01-01T07:30:06Z") };
            request.ApplyConditionalHeaders(conditions);
            HttpAssert.HeaderIsEqual("If-Modified-Since", "Wed, 01 Jan 2020 07:30:06 GMT", request);
            HttpAssert.HeaderIsNotPresent("If-Match", request);
            HttpAssert.HeaderIsNotPresent("If-None-Match", request);
            HttpAssert.HeaderIsNotPresent("If-Unmodified-Since", request);
        }

        [TestMethod]
        public void ApplyConditionalHeader_IfUnmodifiedSince_Added()
        {
            var client = new MobileTableClient(new Uri("https://localhost:5001"));
            var table = client.GetTable<Movie>();
            var request = table.Client.Pipeline.CreateRequest();
            var conditions = new RequestConditions { IfUnmodifiedSince = DateTimeOffset.Parse("2020-01-01T07:30:06Z") };
            request.ApplyConditionalHeaders(conditions);
            HttpAssert.HeaderIsEqual("If-Unmodified-Since", "Wed, 01 Jan 2020 07:30:06 GMT", request);
            HttpAssert.HeaderIsNotPresent("If-Match", request);
            HttpAssert.HeaderIsNotPresent("If-None-Match", request);
            HttpAssert.HeaderIsNotPresent("If-Modified-Since", request);
        }

        [TestMethod]
        public void ApplyConditionalHeader_NoMatchConditions_Added()
        {
            var client = new MobileTableClient(new Uri("https://localhost:5001"));
            var table = client.GetTable<Movie>();
            var request = table.Client.Pipeline.CreateRequest();
            request.ApplyConditionalHeaders(new MatchConditions());
            HttpAssert.HeaderIsNotPresent("If-Match", request);
            HttpAssert.HeaderIsNotPresent("If-None-Match", request);
            HttpAssert.HeaderIsNotPresent("If-Modified-Since", request);
            HttpAssert.HeaderIsNotPresent("If-Unmodified-Since", request);
        }

        [TestMethod]
        public void ApplyConditionalHeader_NoRequestConditions_Added()
        {
            var client = new MobileTableClient(new Uri("https://localhost:5001"));
            var table = client.GetTable<Movie>();
            var request = table.Client.Pipeline.CreateRequest();
            request.ApplyConditionalHeaders(new RequestConditions());
            HttpAssert.HeaderIsNotPresent("If-Match", request);
            HttpAssert.HeaderIsNotPresent("If-None-Match", request);
            HttpAssert.HeaderIsNotPresent("If-Modified-Since", request);
            HttpAssert.HeaderIsNotPresent("If-Unmodified-Since", request);
        }

        [TestMethod]
        public void ApplyConditionalHeader_Null_Added()
        {
            var client = new MobileTableClient(new Uri("https://localhost:5001"));
            var table = client.GetTable<Movie>();
            var request = table.Client.Pipeline.CreateRequest();
            request.ApplyConditionalHeaders(null);
            HttpAssert.HeaderIsNotPresent("If-Match", request);
            HttpAssert.HeaderIsNotPresent("If-None-Match", request);
            HttpAssert.HeaderIsNotPresent("If-Modified-Since", request);
            HttpAssert.HeaderIsNotPresent("If-Unmodified-Since", request);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void BuildUri_NullEndpoint_Throws()
        {
            var client = new MobileTableClient(new Uri("https://localhost:5001"));
            var table = client.GetTable<Movie>();
            var request = table.Client.Pipeline.CreateRequest();
            request.BuildUri(null);
            Assert.Fail("ArgumentNullException expected");
        }

        [TestMethod]
        public void BuildUri_EndpointOnly_Works()
        {
            var uri = new Uri("https://localhost:5001");
            var client = new MobileTableClient(new Uri("https://localhost:5001"));
            var table = client.GetTable<Movie>();
            var request = table.Client.Pipeline.CreateRequest();
            request.BuildUri(new Uri(uri, "/tables/foo"));
            Assert.AreEqual("https://localhost:5001/tables/foo", request.Uri.ToString());
        }

        [TestMethod]
        public void BuildUri_EndpointAndRelativePath_Works()
        {
            var uri = new Uri("https://localhost:5001");
            var client = new MobileTableClient(new Uri("https://localhost:5001"));
            var table = client.GetTable<Movie>();
            var request = table.Client.Pipeline.CreateRequest();
            request.BuildUri(new Uri(uri, "/tables/foo"), "28");
            Assert.AreEqual("https://localhost:5001/tables/foo/28", request.Uri.ToString());
        }
    }
}
