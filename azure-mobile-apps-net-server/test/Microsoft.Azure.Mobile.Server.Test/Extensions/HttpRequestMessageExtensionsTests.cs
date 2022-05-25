// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------------------- 

using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Web.Http;
using Microsoft.Owin;
using TestUtilities;
using Xunit;

namespace System.Net.Http
{
    public class HttpRequestMessageExtensionsTests
    {
        private static readonly Uri BaseAddr = new Uri("http://localhost");
        private HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, BaseAddr);

        public HttpRequestMessageExtensionsTests()
        {
            this.request.Headers.Add("name1", "value1");
            this.request.Headers.Host = "host1";
        }

        public static TheoryDataCollection<HttpMethod, Uri, bool> SchedulerJobRequests
        {
            get
            {
                return new TheoryDataCollection<HttpMethod, Uri, bool>
                {
                    { HttpMethod.Get, new Uri(BaseAddr, "/jobs"), false },
                    { HttpMethod.Get, new Uri(BaseAddr, "/other"), false },
                    { HttpMethod.Delete, new Uri(BaseAddr, "/jobs"), false },
                    { HttpMethod.Delete, new Uri(BaseAddr, "/other"), false },
                    { HttpMethod.Post, new Uri(BaseAddr, string.Empty), false },
                    { HttpMethod.Post, new Uri(BaseAddr, "/path"), false },
                    { HttpMethod.Post, new Uri(BaseAddr, "/jobs/job1"), true },
                    { HttpMethod.Post, new Uri(BaseAddr, "/jobs/job1?query=value"), true },
                    { HttpMethod.Post, new Uri(BaseAddr, "/jobs/job1#fragment"), true },
                    { HttpMethod.Post, new Uri(BaseAddr, "/jobs"), true },
                    { HttpMethod.Post, new Uri(BaseAddr, "/Jobs"), true },
                    { HttpMethod.Post, new Uri(BaseAddr, "/JOBS"), true },
                };
            }
        }

        public static TheoryDataCollection<EntityTagHeaderValue[], EntityTagHeaderValue, bool> IsIfNoneMatch
        {
            get
            {
                return new TheoryDataCollection<EntityTagHeaderValue[], EntityTagHeaderValue, bool>
                {
                    { new EntityTagHeaderValue[] { }, null, false }, 
                    { new EntityTagHeaderValue[] { }, ParseEtag("1234"), false }, 
                    { new EntityTagHeaderValue[] { ParseEtag("1") }, ParseEtag("1234"), false }, 
                    { new EntityTagHeaderValue[] { ParseEtag("1"), ParseEtag("2") }, ParseEtag("1234"), false }, 
                    { new EntityTagHeaderValue[] { ParseEtag("1234") }, ParseEtag("1234"), true }, 
                    { new EntityTagHeaderValue[] { ParseEtag("1"), ParseEtag("1234"), ParseEtag("2") }, ParseEtag("1234"), true }, 
                    { new EntityTagHeaderValue[] { EntityTagHeaderValue.Any }, ParseEtag("1234"), true }, 
                    { new EntityTagHeaderValue[] { ParseEtag("1"), EntityTagHeaderValue.Any }, ParseEtag("1234"), true }, 
                    { new EntityTagHeaderValue[] { EntityTagHeaderValue.Any }, null, false }, 
                    { new EntityTagHeaderValue[] { ParseEtag("1"), EntityTagHeaderValue.Any }, null, false }, 
                };
            }
        }

        public static TheoryDataCollection<string, object[]> ErrorMessages
        {
            get
            {
                return new TheoryDataCollection<string, object[]>
                {
                    { string.Empty, null },
                    { "{0}", null },
                    { "{0}", new object[] { } },
                    { "{0}", new object[] { 10 } },
                    { "{0}, {1}, {2}", new object[] { 10, 20, 30 } },
                };
            }
        }

        [Fact]
        public void GetAuthenticationManager_GetsAuthenticationManager()
        {
            // Arrange
            OwinContext context = new OwinContext();
            this.request.SetOwinContext(context);

            // Act
            IOwinContext actual = this.request.GetOwinContext();

            // Assert
            Assert.Same(context, actual);
        }

        [Fact]
        public void GetAuthenticationManager_ReturnsNull_IfAuthenticationManager_NotPresent()
        {
            // Act
            IOwinContext actual = this.request.GetOwinContext();

            // Assert
            Assert.Null(actual);
        }

        [Theory]
        [MemberData("IsIfNoneMatch")]
        public void IsIfNoneMatch_DetectsIfNonMatchRequests(IEnumerable<EntityTagHeaderValue> ifNoneMatchETags, EntityTagHeaderValue current, bool expected)
        {
            // Arrange
            foreach (EntityTagHeaderValue etag in ifNoneMatchETags)
            {
                this.request.Headers.IfNoneMatch.Add(etag);
            }

            // Act
            bool actual = this.request.IsIfNoneMatch(current);

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void CreateNotFound_CreatesDefaultResponse()
        {
            // Arrange
            HttpError error;

            // Act
            HttpResponseMessage response = this.request.CreateNotFoundResponse();
            response.TryGetContentValue(out error);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.Contains("No HTTP resource was found that matches", error.Message);
        }

        [Theory]
        [MemberData("ErrorMessages")]
        public void CreateNotFoundResponse_CreatesResponse(string format, object[] args)
        {
            // Arrange
            HttpError error;
            string expected = (args != null && args.Length > 0) ? string.Format(format, args) : format;

            // Act
            HttpResponseMessage response = this.request.CreateNotFoundResponse(format, args);
            response.TryGetContentValue(out error);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.Equal(expected, error.Message);
        }

        [Theory]
        [MemberData("ErrorMessages")]
        public void CreateUnauthorizedResponse_CreatesResponse(string format, object[] args)
        {
            // Arrange
            HttpError error;
            string expected = (args != null && args.Length > 0) ? string.Format(format, args) : format;

            // Act
            HttpResponseMessage response = this.request.CreateUnauthorizedResponse(format, args);
            response.TryGetContentValue(out error);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            Assert.Equal(expected, error.Message);
        }

        [Fact]
        public void CreateBadRequest_CreatesDefaultResponse()
        {
            // Arrange
            HttpError error;

            // Act
            HttpResponseMessage response = this.request.CreateBadRequestResponse();
            response.TryGetContentValue(out error);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Contains("The request is invalid as submitted", error.Message);
        }

        [Theory]
        [MemberData("ErrorMessages")]
        public void CreateBadRequestResponse_CreatesResponse(string format, object[] args)
        {
            // Arrange
            HttpError error;
            string expected = (args != null && args.Length > 0) ? string.Format(format, args) : format;

            // Act
            HttpResponseMessage response = this.request.CreateBadRequestResponse(format, args);
            response.TryGetContentValue(out error);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal(expected, error.Message);
        }

        private static EntityTagHeaderValue ParseEtag(string etag)
        {
            return EntityTagHeaderValue.Parse('\"' + etag + '\"');
        }
    }
}
