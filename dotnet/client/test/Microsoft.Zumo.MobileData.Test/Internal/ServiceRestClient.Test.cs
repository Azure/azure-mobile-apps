// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Azure;
using Azure.Core;
using Azure.Core.Pipeline;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Zumo.MobileData.Internal;
using Microsoft.Zumo.MobileData.Test.Helpers;
using System;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;

namespace Microsoft.Zumo.MobileData.Test.Internal
{
    [TestClass]
    public class ServiceRestClient_Tests : BaseTest
    {
        #region Test Data
        class Entity : TableData
        {
            public string Data { get; set; }
        }

        private readonly string pagedResponse = "{\"results\":[{\"id\":\"1\",\"data\":\"f1\"},{\"id\":\"2\",\"data\":\"f2\"},{\"id\":\"3\",\"data\":\"f3\"},{\"id\":\"4\",\"data\":\"f4\"},{\"id\":\"5\",\"data\":\"f5\"}],\"count\":248}";
        #endregion

        #region Constructor
        [TestMethod]
        public void Ctor_NullCredential_SetsProperties()
        {
            var uri = new Uri("https://localhost");
            var options = new MobileTableClientOptions();
            var actual = new ServiceRestClient<Movie>(uri, null, options);

            Assert.IsNotNull(actual);
            Assert.AreEqual(uri, actual.Endpoint);
            Assert.AreEqual(options.JsonSerializerOptions.PropertyNamingPolicy, actual.SerializerOptions.PropertyNamingPolicy);
            Assert.IsNotNull(actual.Pipeline);
        }

        [TestMethod]
        public void Ctor_WithCredential_SetsProperties()
        {
            var uri = new Uri("https://localhost");
            var credential = new SimpleTokenCredential("foo");
            var options = new MobileTableClientOptions();
            var actual = new ServiceRestClient<Movie>(uri, credential, options);

            Assert.IsNotNull(actual);
            Assert.AreEqual(uri, actual.Endpoint);
            Assert.AreEqual(options.JsonSerializerOptions.PropertyNamingPolicy, actual.SerializerOptions.PropertyNamingPolicy);
            Assert.IsNotNull(actual.Pipeline);
        }
        #endregion

        #region CreateDeleteRequest
        [TestMethod]
        public void CreateDeleteRequest_ItemOnly_ProducesRightRequest()
        {
            var uri = new Uri("https://localhost/tables/movies");
            var credential = new SimpleTokenCredential("foo");
            var options = new MobileTableClientOptions();
            var client = new ServiceRestClient<Movie>(uri, credential, options);
            var movie = new Movie
            {
                BestPictureWinner = false,
                Deleted = false,
                Duration = 60,
                Id = "movie-5",
                MpaaRating = "G",
                ReleaseDate = new DateTime(2019, 12, 24),
                UpdatedAt = DateTimeOffset.Parse("2020-05-21T03:00:00.000Z"),
                Title = "Home Video",
                Version = "AQBCIkeP",
                Year = 2019
            };

            var actual = client.CreateDeleteRequest(movie, null);

            Assert.AreEqual(RequestMethod.Delete, actual.Method);
            Assert.AreEqual("https://localhost/tables/movies/movie-5", actual.Uri.ToString());
            HttpAssert.HeaderIsEqual("If-Match", $"\"{movie.Version}\"", actual);
        }

        [TestMethod]
        public void CreateDeleteRequest_Item_IfMatch_ProducesRightRequest()
        {
            var uri = new Uri("https://localhost/tables/movies");
            var credential = new SimpleTokenCredential("foo");
            var options = new MobileTableClientOptions();
            var client = new ServiceRestClient<Movie>(uri, credential, options);
            var movie = new Movie
            {
                BestPictureWinner = false,
                Deleted = false,
                Duration = 60,
                Id = "movie-5",
                MpaaRating = "G",
                ReleaseDate = new DateTime(2019, 12, 24),
                UpdatedAt = DateTimeOffset.Parse("2020-05-21T03:00:00.000Z"),
                Title = "Home Video",
                Version = "AQBCIkeP",
                Year = 2019
            };

            var actual = client.CreateDeleteRequest(movie, new MatchConditions { IfMatch = ETag.All });

            Assert.AreEqual(RequestMethod.Delete, actual.Method);
            Assert.AreEqual("https://localhost/tables/movies/movie-5", actual.Uri.ToString());
            HttpAssert.HeaderIsEqual("If-Match", "*", actual);
        }

        [TestMethod]
        public void CreateDeleteRequest_Item_IfModifiedSince_ProducesRightRequest()
        {
            var uri = new Uri("https://localhost/tables/movies");
            var credential = new SimpleTokenCredential("foo");
            var options = new MobileTableClientOptions();
            var client = new ServiceRestClient<Movie>(uri, credential, options);
            var movie = new Movie
            {
                BestPictureWinner = false,
                Deleted = false,
                Duration = 60,
                Id = "movie-5",
                MpaaRating = "G",
                ReleaseDate = new DateTime(2019, 12, 24),
                UpdatedAt = DateTimeOffset.Parse("2020-05-21T03:00:00.000Z"),
                Title = "Home Video",
                Version = "AQBCIkeP",
                Year = 2019
            };

            var actual = client.CreateDeleteRequest(movie, new RequestConditions { IfModifiedSince = movie.UpdatedAt });

            Assert.AreEqual(RequestMethod.Delete, actual.Method);
            Assert.AreEqual("https://localhost/tables/movies/movie-5", actual.Uri.ToString());
            HttpAssert.HeaderIsEqual("If-Modified-Since", movie.UpdatedAt.ToString("r"), actual);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CreateDeleteRequest_NullItem_Throws()
        {
            var uri = new Uri("https://localhost/tables/movies");
            var credential = new SimpleTokenCredential("foo");
            var options = new MobileTableClientOptions();
            var client = new ServiceRestClient<Movie>(uri, credential, options);

            _ = client.CreateDeleteRequest(null, null);
            Assert.Fail("ArgumentNullException expected");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CreateDeleteRequest_NullId_Throws()
        {
            var uri = new Uri("https://localhost/tables/movies");
            var credential = new SimpleTokenCredential("foo");
            var options = new MobileTableClientOptions();
            var client = new ServiceRestClient<Movie>(uri, credential, options);

            _ = client.CreateDeleteRequest(new Movie { Id = null }, null);
            Assert.Fail("ArgumentNullException expected");
        }

        [TestMethod]
        public void CreateDeleteRequest_ItemWithNoVersion_ProducesRightRequest()
        {
            var uri = new Uri("https://localhost/tables/movies");
            var credential = new SimpleTokenCredential("foo");
            var options = new MobileTableClientOptions();
            var client = new ServiceRestClient<Movie>(uri, credential, options);
            var movie = new Movie
            {
                BestPictureWinner = false,
                Deleted = false,
                Duration = 60,
                Id = "movie-5",
                MpaaRating = "G",
                ReleaseDate = new DateTime(2019, 12, 24),
                UpdatedAt = DateTimeOffset.Parse("2020-05-21T03:00:00.000Z"),
                Title = "Home Video",
                Year = 2019
            };

            var actual = client.CreateDeleteRequest(movie, null);

            Assert.AreEqual(RequestMethod.Delete, actual.Method);
            Assert.AreEqual("https://localhost/tables/movies/movie-5", actual.Uri.ToString());
            HttpAssert.HeaderIsNotPresent("If-Match", actual);
            HttpAssert.HeaderIsNotPresent("If-None-Match", actual);
            HttpAssert.HeaderIsNotPresent("If-Modified-Since", actual);
            HttpAssert.HeaderIsNotPresent("If-Unmodified-Since", actual);
        }
        #endregion

        #region CreateGetRequest
        [TestMethod]
        public void CreateGetRequest_ItemOnly_ProducesRightRequest()
        {
            var uri = new Uri("https://localhost/tables/movies");
            var credential = new SimpleTokenCredential("foo");
            var options = new MobileTableClientOptions();
            var client = new ServiceRestClient<Movie>(uri, credential, options);

            var actual = client.CreateGetRequest("movie-5", null);

            Assert.AreEqual(RequestMethod.Get, actual.Method);
            Assert.AreEqual("https://localhost/tables/movies/movie-5", actual.Uri.ToString());
            HttpAssert.HeaderIsNotPresent("If-Match", actual);
            HttpAssert.HeaderIsNotPresent("If-None-Match", actual);
            HttpAssert.HeaderIsNotPresent("If-Modified-Since", actual);
            HttpAssert.HeaderIsNotPresent("If-Unmodified-Since", actual);
        }

        [TestMethod]
        public void CreateGetRequest_Item_IfMatch_ProducesRightRequest()
        {
            var uri = new Uri("https://localhost/tables/movies");
            var credential = new SimpleTokenCredential("foo");
            var options = new MobileTableClientOptions();
            var client = new ServiceRestClient<Movie>(uri, credential, options);

            var actual = client.CreateGetRequest("movie-5", new MatchConditions { IfMatch = ETag.All });

            Assert.AreEqual(RequestMethod.Get, actual.Method);
            Assert.AreEqual("https://localhost/tables/movies/movie-5", actual.Uri.ToString());
            HttpAssert.HeaderIsEqual("If-Match", "*", actual);
        }

        [TestMethod]
        public void CreateGetRequest_Item_IfModifiedSince_ProducesRightRequest()
        {
            var uri = new Uri("https://localhost/tables/movies");
            var credential = new SimpleTokenCredential("foo");
            var options = new MobileTableClientOptions();
            var client = new ServiceRestClient<Movie>(uri, credential, options);
            var updatedAt = DateTimeOffset.Parse("2020-05-21T03:00:00.000Z");

            var actual = client.CreateGetRequest("movie-5", new RequestConditions { IfModifiedSince = updatedAt });

            Assert.AreEqual(RequestMethod.Get, actual.Method);
            Assert.AreEqual("https://localhost/tables/movies/movie-5", actual.Uri.ToString());
            HttpAssert.HeaderIsEqual("If-Modified-Since", updatedAt.ToString("r"), actual);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CreateGetRequest_NullItem_Throws()
        {
            var uri = new Uri("https://localhost/tables/movies");
            var credential = new SimpleTokenCredential("foo");
            var options = new MobileTableClientOptions();
            var client = new ServiceRestClient<Movie>(uri, credential, options);

            _ = client.CreateGetRequest(null, null);
            Assert.Fail("ArgumentNullException expected");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CreateGetRequest_EmptyItem_Throws()
        {
            var uri = new Uri("https://localhost/tables/movies");
            var credential = new SimpleTokenCredential("foo");
            var options = new MobileTableClientOptions();
            var client = new ServiceRestClient<Movie>(uri, credential, options);

            _ = client.CreateGetRequest(null, null);
            Assert.Fail("ArgumentNullException expected");
        }
        #endregion

        #region CreateInsertRequest
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CreateInsertRequest_NullItem_Throws()
        {
            var uri = new Uri("https://localhost/tables/movies");
            var credential = new SimpleTokenCredential("foo");
            var options = new MobileTableClientOptions();
            var client = new ServiceRestClient<Movie>(uri, credential, options);

            _ = client.CreateGetRequest(null, null);
            Assert.Fail("ArgumentNullException expected");
        }

        [TestMethod]
        public void CreateInsertRequest_RegularItem_WorksAsExpected()
        {
            var uri = new Uri("https://localhost/tables/movies");
            var credential = new SimpleTokenCredential("foo");
            var options = new MobileTableClientOptions();
            var client = new ServiceRestClient<Movie>(uri, credential, options);
            var movie = new Movie
            {
                BestPictureWinner = false,
                Deleted = false,
                Duration = 60,
                Id = "movie-5",
                MpaaRating = "G",
                ReleaseDate = new DateTime(2019, 12, 24),
                UpdatedAt = DateTimeOffset.Parse("2020-05-21T03:00:00.000Z"),
                Title = "Home Video",
                Version = "AQBCIkeP",
                Year = 2019
            };

            var actual = client.CreateInsertRequest(movie);

            Assert.AreEqual(RequestMethod.Post, actual.Method);
            Assert.AreEqual("https://localhost/tables/movies", actual.Uri.ToString());
            HttpAssert.HeaderIsEqual("If-None-Match", "*", actual);
            HttpAssert.HeaderIsNotPresent("If-Match", actual);
            HttpAssert.HeaderIsEqual("Content-Type", "application/json", actual);
            var hasLength = actual.Content.TryComputeLength(out long length);
            Assert.IsTrue(hasLength);
            Assert.IsTrue(length > 0);
        }
        #endregion

        #region CreateListRequest
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CreateListRequest_NullItemAndPageLink_Throws()
        {
            var uri = new Uri("https://localhost/tables/movies");
            var credential = new SimpleTokenCredential("foo");
            var options = new MobileTableClientOptions();
            var client = new ServiceRestClient<Movie>(uri, credential, options);

            _ = client.CreateListRequest(null, null);
            Assert.Fail("ArgumentNullException expected");
        }

        [TestMethod]
        [ExpectedException(typeof(UriFormatException))]
        public void CreateListRequest_InvalidLink_Throws()
        {
            var uri = new Uri("https://localhost/tables/movies");
            var credential = new SimpleTokenCredential("foo");
            var options = new MobileTableClientOptions();
            var client = new ServiceRestClient<Movie>(uri, credential, options);

            _ = client.CreateListRequest(null, "foo");
            Assert.Fail("UriFormatException expected");
        }

        [TestMethod]
        public void CreateListRequest_ValidLink_WorksAsExpected()
        {
            var uri = new Uri("https://localhost/tables/movies");
            var credential = new SimpleTokenCredential("foo");
            var options = new MobileTableClientOptions();
            var client = new ServiceRestClient<Movie>(uri, credential, options);

            var actual = client.CreateListRequest(null, "https://localhost/foo");

            Assert.AreEqual(RequestMethod.Get, actual.Method);
            Assert.AreEqual("https://localhost/foo", actual.Uri.ToString());
        }

        [TestMethod]
        public void CreateListRequest_ValidEmptyOptions_WorksAsExpected()
        {
            var uri = new Uri("https://localhost/tables/movies");
            var credential = new SimpleTokenCredential("foo");
            var options = new MobileTableClientOptions();
            var client = new ServiceRestClient<Movie>(uri, credential, options);
            var query = new MobileTableQueryOptions();

            var actual = client.CreateListRequest(query);

            Assert.AreEqual(RequestMethod.Get, actual.Method);
            Assert.AreEqual("https://localhost/tables/movies?$count=true", actual.Uri.ToString());
        }

        [TestMethod]
        public void CreateListRequest_IncludeDeleted_WorksAsExpected()
        {
            var uri = new Uri("https://localhost/tables/movies");
            var credential = new SimpleTokenCredential("foo");
            var options = new MobileTableClientOptions();
            var client = new ServiceRestClient<Movie>(uri, credential, options);
            var query = new MobileTableQueryOptions { IncludeDeleted = true };

            var actual = client.CreateListRequest(query);

            Assert.AreEqual(RequestMethod.Get, actual.Method);
            Assert.AreEqual("https://localhost/tables/movies?__includedeleted=true&$count=true", actual.Uri.ToString());
        }

        [TestMethod]
        public void CreateListRequest_SkipTop_WorksAsExpected()
        {
            var uri = new Uri("https://localhost/tables/movies");
            var credential = new SimpleTokenCredential("foo");
            var options = new MobileTableClientOptions();
            var client = new ServiceRestClient<Movie>(uri, credential, options);
            var query = new MobileTableQueryOptions { Skip = 5, Size = 10 };

            var actual = client.CreateListRequest(query);

            Assert.AreEqual(RequestMethod.Get, actual.Method);
            Assert.AreEqual("https://localhost/tables/movies?$skip=5&$top=10&$count=true", actual.Uri.ToString());
        }

        [TestMethod]
        public void CreateListRequest_FilterOrderBy_WorksAsExpected()
        {
            var uri = new Uri("https://localhost/tables/movies");
            var credential = new SimpleTokenCredential("foo");
            var options = new MobileTableClientOptions();
            var client = new ServiceRestClient<Movie>(uri, credential, options);
            var query = new MobileTableQueryOptions { Filter = "mpaaRating eq 'R'", OrderBy = "Year" };

            var actual = client.CreateListRequest(query);

            Assert.AreEqual(RequestMethod.Get, actual.Method);
            Assert.AreEqual("https://localhost/tables/movies?$filter=mpaaRating%20eq%20%27R%27&$orderBy=Year&$count=true", actual.Uri.ToString());
        }
        #endregion

        #region CreateReplaceRequest
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CreateReplaceRequest_NullItem_Throws()
        {
            var uri = new Uri("https://localhost/tables/movies");
            var credential = new SimpleTokenCredential("foo");
            var options = new MobileTableClientOptions();
            var client = new ServiceRestClient<Movie>(uri, credential, options);

            _ = client.CreateReplaceRequest(null, null);
            Assert.Fail("ArgumentNullException expected");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CreateReplaceRequest_NullId_Throws()
        {
            var uri = new Uri("https://localhost/tables/movies");
            var credential = new SimpleTokenCredential("foo");
            var options = new MobileTableClientOptions();
            var client = new ServiceRestClient<Movie>(uri, credential, options);

            _ = client.CreateReplaceRequest(new Movie { Id = null }, null);
            Assert.Fail("ArgumentNullException expected");
        }

        [TestMethod]
        public void CreateReplaceRequest_ItemOnly_ProducesRightRequest()
        {
            var uri = new Uri("https://localhost/tables/movies");
            var credential = new SimpleTokenCredential("foo");
            var options = new MobileTableClientOptions();
            var client = new ServiceRestClient<Movie>(uri, credential, options);
            var movie = new Movie
            {
                BestPictureWinner = false,
                Deleted = false,
                Duration = 60,
                Id = "movie-5",
                MpaaRating = "G",
                ReleaseDate = new DateTime(2019, 12, 24),
                UpdatedAt = DateTimeOffset.Parse("2020-05-21T03:00:00.000Z"),
                Title = "Home Video",
                Version = "AQBCIkeP",
                Year = 2019
            };

            var actual = client.CreateReplaceRequest(movie, null);

            Assert.AreEqual(RequestMethod.Put, actual.Method);
            Assert.AreEqual("https://localhost/tables/movies/movie-5", actual.Uri.ToString());
            HttpAssert.HeaderIsEqual("If-Match", $"\"{movie.Version}\"", actual);
            HttpAssert.HeaderIsEqual("Content-Type", "application/json", actual);
            var hasLength = actual.Content.TryComputeLength(out long length);
            Assert.IsTrue(hasLength);
            Assert.IsTrue(length > 0);
        }

        [TestMethod]
        public void CreateReplaceRequest_IfMatchStar_ProducesRightRequest()
        {
            var uri = new Uri("https://localhost/tables/movies");
            var credential = new SimpleTokenCredential("foo");
            var options = new MobileTableClientOptions();
            var client = new ServiceRestClient<Movie>(uri, credential, options);
            var movie = new Movie
            {
                BestPictureWinner = false,
                Deleted = false,
                Duration = 60,
                Id = "movie-5",
                MpaaRating = "G",
                ReleaseDate = new DateTime(2019, 12, 24),
                UpdatedAt = DateTimeOffset.Parse("2020-05-21T03:00:00.000Z"),
                Title = "Home Video",
                Version = "AQBCIkeP",
                Year = 2019
            };

            var actual = client.CreateReplaceRequest(movie, new MatchConditions { IfMatch = ETag.All });

            Assert.AreEqual(RequestMethod.Put, actual.Method);
            Assert.AreEqual("https://localhost/tables/movies/movie-5", actual.Uri.ToString());
            HttpAssert.HeaderIsEqual("If-Match", "*", actual);
            HttpAssert.HeaderIsEqual("Content-Type", "application/json", actual);
            var hasLength = actual.Content.TryComputeLength(out long length);
            Assert.IsTrue(hasLength);
            Assert.IsTrue(length > 0);
        }

        [TestMethod]
        public void CreateReplaceRequest_NoVersion_ProducesRightRequest()
        {
            var uri = new Uri("https://localhost/tables/movies");
            var credential = new SimpleTokenCredential("foo");
            var options = new MobileTableClientOptions();
            var client = new ServiceRestClient<Movie>(uri, credential, options);
            var movie = new Movie
            {
                BestPictureWinner = false,
                Deleted = false,
                Duration = 60,
                Id = "movie-5",
                MpaaRating = "G",
                ReleaseDate = new DateTime(2019, 12, 24),
                UpdatedAt = DateTimeOffset.Parse("2020-05-21T03:00:00.000Z"),
                Title = "Home Video",
                Version = null,
                Year = 2019
            };

            var actual = client.CreateReplaceRequest(movie, null);

            Assert.AreEqual(RequestMethod.Put, actual.Method);
            Assert.AreEqual("https://localhost/tables/movies/movie-5", actual.Uri.ToString());
            HttpAssert.HeaderIsNotPresent("If-Match", actual);
            HttpAssert.HeaderIsNotPresent("If-None-Match", actual);
            HttpAssert.HeaderIsNotPresent("If-Modified-Since", actual);
            HttpAssert.HeaderIsNotPresent("If-Unmodified-Since", actual);
            HttpAssert.HeaderIsEqual("Content-Type", "application/json", actual);
            var hasLength = actual.Content.TryComputeLength(out long length);
            Assert.IsTrue(hasLength);
            Assert.IsTrue(length > 0);
        }

        [TestMethod]
        public void CreateReplaceRequest_ExplicitNoConditions_ProducesRightRequest()
        {
            var uri = new Uri("https://localhost/tables/movies");
            var credential = new SimpleTokenCredential("foo");
            var options = new MobileTableClientOptions();
            var client = new ServiceRestClient<Movie>(uri, credential, options);
            var movie = new Movie
            {
                BestPictureWinner = false,
                Deleted = false,
                Duration = 60,
                Id = "movie-5",
                MpaaRating = "G",
                ReleaseDate = new DateTime(2019, 12, 24),
                UpdatedAt = DateTimeOffset.Parse("2020-05-21T03:00:00.000Z"),
                Title = "Home Video",
                Version = "abc123",
                Year = 2019
            };

            var actual = client.CreateReplaceRequest(movie, new MatchConditions());

            Assert.AreEqual(RequestMethod.Put, actual.Method);
            Assert.AreEqual("https://localhost/tables/movies/movie-5", actual.Uri.ToString());
            HttpAssert.HeaderIsNotPresent("If-Match", actual);
            HttpAssert.HeaderIsNotPresent("If-None-Match", actual);
            HttpAssert.HeaderIsNotPresent("If-Modified-Since", actual);
            HttpAssert.HeaderIsNotPresent("If-Unmodified-Since", actual);
            HttpAssert.HeaderIsEqual("Content-Type", "application/json", actual);
            var hasLength = actual.Content.TryComputeLength(out long length);
            Assert.IsTrue(hasLength);
            Assert.IsTrue(length > 0);
        }
        #endregion

        #region SendRequest
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SendRequest_NullRequest_Throws()
        {
            var uri = new Uri("https://localhost");
            var mockTransport = new MockTransport();
            var options = new MobileTableClientOptions
            {
                Transport = new HttpClientTransport(mockTransport.Client)
            };

            var client = new ServiceRestClient<Movie>(uri, null, options);

            _ = client.SendRequest(null, default);
        }

        [TestMethod]
        public void SendRequest_NoCredential_SendsRequest()
        {
            var uri = new Uri("https://localhost");
            var content = "{}";
            var mockTransport = new MockTransport();
            var options = new MobileTableClientOptions
            {
                Transport = new HttpClientTransport(mockTransport.Client)
            };

            var client = new ServiceRestClient<Movie>(uri, null, options);

            // Mock the response
            mockTransport.Response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(content)
            };

            // Create a basic request
            var request = client.Pipeline.CreateRequest();
            request.Method = RequestMethod.Get;
            request.BuildUri(uri);

            // This is the actual test
            var actual = client.SendRequest(request, default);

            // Check the request out
            Assert.AreEqual(HttpMethod.Get, mockTransport.Request.Method);
            Assert.AreEqual(uri, mockTransport.Request.RequestUri);

            // Ensure the User-Agent is set
            HttpAssert.HeaderStartsWith("azsdk-net-Microsoft.Zumo.MobileData/", "User-Agent", mockTransport.Request);

            // Ensure no Authorization header is present
            HttpAssert.HeaderIsNotPresent("Authorization", mockTransport.Request);

            // Check the response out
            Assert.AreEqual(200, actual.Status);
            Assert.AreEqual(2, actual.ContentStream.Length);
        }

        [TestMethod]
        public void SendRequest_WithCredential_SendsRequest()
        {
            var uri = new Uri("https://localhost");
            var credential = new SimpleTokenCredential("accessToken");
            var content = "{}";
            var mockTransport = new MockTransport();
            var options = new MobileTableClientOptions
            {
                Transport = new HttpClientTransport(mockTransport.Client)
            };

            var client = new ServiceRestClient<Movie>(uri, credential, options);

            // Mock the response
            mockTransport.Response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(content)
            };

            // Create a basic request
            var request = client.Pipeline.CreateRequest();
            request.Method = RequestMethod.Get;
            request.BuildUri(uri);

            // This is the actual test
            var actual = client.SendRequest(request, default);

            // Check the request out
            Assert.AreEqual(HttpMethod.Get, mockTransport.Request.Method);
            Assert.AreEqual(uri, mockTransport.Request.RequestUri);

            // Ensure the User-Agent is set
            HttpAssert.HeaderStartsWith("azsdk-net-Microsoft.Zumo.MobileData/", "User-Agent", mockTransport.Request);
            HttpAssert.HeaderIsEqual("Authorization", "Bearer accessToken", mockTransport.Request);

            // Check the response out
            Assert.AreEqual(200, actual.Status);
            Assert.AreEqual(2, actual.ContentStream.Length);
        }
        #endregion

        #region SendRequestAsync
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task SendRequestAsync_NullRequest_Throws()
        {
            var uri = new Uri("https://localhost");
            var mockTransport = new MockTransport();
            var options = new MobileTableClientOptions
            {
                Transport = new HttpClientTransport(mockTransport.Client)
            };

            var client = new ServiceRestClient<Movie>(uri, null, options);

            _ = await client.SendRequestAsync(null, default);
        }

        [TestMethod]
        public async Task SendRequestAsync_NoCredential_SendsRequest()
        {
            var uri = new Uri("https://localhost");
            var content = "{}";
            var mockTransport = new MockTransport();
            var options = new MobileTableClientOptions
            {
                Transport = new HttpClientTransport(mockTransport.Client)
            };

            var client = new ServiceRestClient<Movie>(uri, null, options);

            // Mock the response
            mockTransport.Response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(content)
            };

            // Create a basic request
            var request = client.Pipeline.CreateRequest();
            request.Method = RequestMethod.Get;
            request.BuildUri(uri);

            // This is the actual test
            var actual = await client.SendRequestAsync(request, default);

            // Check the request out
            Assert.AreEqual(HttpMethod.Get, mockTransport.Request.Method);
            Assert.AreEqual(uri, mockTransport.Request.RequestUri);

            // Ensure the User-Agent is set
            HttpAssert.HeaderStartsWith("azsdk-net-Microsoft.Zumo.MobileData/", "User-Agent", mockTransport.Request);

            // Ensure no Authorization header is present
            HttpAssert.HeaderIsNotPresent("Authorization", mockTransport.Request);

            // Check the response out
            Assert.AreEqual(200, actual.Status);
            Assert.AreEqual(2, actual.ContentStream.Length);
        }

        [TestMethod]
        public async Task SendRequestAsync_WithCredential_SendsRequest()
        {
            var uri = new Uri("https://localhost");
            var credential = new SimpleTokenCredential("accessToken");
            var content = "{}";
            var mockTransport = new MockTransport();
            var options = new MobileTableClientOptions
            {
                Transport = new HttpClientTransport(mockTransport.Client)
            };

            var client = new ServiceRestClient<Movie>(uri, credential, options);

            // Mock the response
            mockTransport.Response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(content)
            };

            // Create a basic request
            var request = client.Pipeline.CreateRequest();
            request.Method = RequestMethod.Get;
            request.BuildUri(uri);

            // This is the actual test
            var actual = await client.SendRequestAsync(request, default);

            // Check the request out
            Assert.AreEqual(HttpMethod.Get, mockTransport.Request.Method);
            Assert.AreEqual(uri, mockTransport.Request.RequestUri);

            // Ensure the User-Agent is set
            HttpAssert.HeaderStartsWith("azsdk-net-Microsoft.Zumo.MobileData/", "User-Agent", mockTransport.Request);
            HttpAssert.HeaderIsEqual("Authorization", "Bearer accessToken", mockTransport.Request);

            // Check the response out
            Assert.AreEqual(200, actual.Status);
            Assert.AreEqual(2, actual.ContentStream.Length);
        }
        #endregion

        #region CreateResponseAsync<T>
        [TestMethod]
        public async Task CreateResponseAsync_Deserializes_EmptyBody()
        {
            var uri = new Uri("https://localhost");
            var content = "{}";
            var mockTransport = new MockTransport();
            var options = new MobileTableClientOptions
            {
                Transport = new HttpClientTransport(mockTransport.Client)
            };

            var client = new ServiceRestClient<Entity>(uri, null, options);

            // Mock the response
            mockTransport.Response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(content)
            };

            // Create a basic request
            var request = client.Pipeline.CreateRequest();
            request.Method = RequestMethod.Get;
            request.BuildUri(uri);

            // This is the actual test
            var response = await client.SendRequestAsync(request, default);
            var actual = await client.CreateResponseAsync(response, default);

            // Check that the response created is the right type
            Assert.IsInstanceOfType(actual, typeof(Response<Entity>));
            Assert.IsNotNull(actual);
            Assert.IsNotNull(actual.Value);
            Assert.IsInstanceOfType(actual.Value, typeof(Entity));
        }

        [TestMethod]
        public async Task CreateResponseAsync_Deserializes_FullBody()
        {
            var uri = new Uri("https://localhost");
            var content = "{\"id\":\"f1\",\"deleted\":false,\"updatedAt\":\"2020-04-20T03:00:00.000Z\",\"data\":\"f2\"}";
            var mockTransport = new MockTransport();
            var options = new MobileTableClientOptions
            {
                Transport = new HttpClientTransport(mockTransport.Client)
            };

            var client = new ServiceRestClient<Entity>(uri, null, options);

            // Mock the response
            mockTransport.Response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(content)
            };

            // Create a basic request
            var request = client.Pipeline.CreateRequest();
            request.Method = RequestMethod.Get;
            request.BuildUri(uri);

            // This is the actual test
            var response = await client.SendRequestAsync(request, default);
            Response<Entity> actual = await client.CreateResponseAsync(response, default);

            // Check that the response created is the right type
            Assert.IsNotNull(actual);
            Assert.IsNotNull(actual.Value);
            Assert.IsInstanceOfType(actual.Value, typeof(Entity));
            Assert.AreEqual("f1", actual.Value.Id);
            Assert.AreEqual("f2", actual.Value.Data);
        }
        #endregion

        #region CreateResponse<T>
        [TestMethod]
        public async Task CreateResponse_Deserializes_EmptyBody()
        {
            var uri = new Uri("https://localhost");
            var content = "{}";
            var mockTransport = new MockTransport();
            var options = new MobileTableClientOptions
            {
                Transport = new HttpClientTransport(mockTransport.Client)
            };

            var client = new ServiceRestClient<Entity>(uri, null, options);

            // Mock the response
            mockTransport.Response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(content)
            };

            // Create a basic request
            var request = client.Pipeline.CreateRequest();
            request.Method = RequestMethod.Get;
            request.BuildUri(uri);

            // This is the actual test
            var response = await client.SendRequestAsync(request, default);
            var actual = client.CreateResponse(response, default);

            // Check that the response created is the right type
            Assert.IsInstanceOfType(actual, typeof(Response<Entity>));
            Assert.IsNotNull(actual);
            Assert.IsNotNull(actual.Value);
            Assert.IsInstanceOfType(actual.Value, typeof(Entity));
        }

        [TestMethod]
        public async Task CreateResponse_Deserializes_FullBody()
        {
            var uri = new Uri("https://localhost");
            var content = "{\"id\":\"f1\",\"deleted\":false,\"updatedAt\":\"2020-04-20T03:00:00.000Z\",\"data\":\"f2\"}";
            var mockTransport = new MockTransport();
            var options = new MobileTableClientOptions
            {
                Transport = new HttpClientTransport(mockTransport.Client)
            };

            var client = new ServiceRestClient<Entity>(uri, null, options);

            // Mock the response
            mockTransport.Response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(content)
            };

            // Create a basic request
            var request = client.Pipeline.CreateRequest();
            request.Method = RequestMethod.Get;
            request.BuildUri(uri);

            // This is the actual test
            var response = await client.SendRequestAsync(request, default);
            Response<Entity> actual = client.CreateResponse(response, default);

            // Check that the response created is the right type
            Assert.IsNotNull(actual);
            Assert.IsNotNull(actual.Value);
            Assert.IsInstanceOfType(actual.Value, typeof(Entity));
            Assert.AreEqual("f1", actual.Value.Id);
            Assert.AreEqual("f2", actual.Value.Data);
        }
        #endregion

        #region CreatePagedResultAsync
        [TestMethod]
        public async Task CreatePagedResultAsync_Deserializes_FullBody()
        {
            var uri = new Uri("https://localhost");
            var mockTransport = new MockTransport();
            var options = new MobileTableClientOptions
            {
                Transport = new HttpClientTransport(mockTransport.Client)
            };

            var client = new ServiceRestClient<Entity>(uri, null, options);

            // Mock the response
            mockTransport.Response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(pagedResponse)
            };

            // Create a basic request
            var request = client.Pipeline.CreateRequest();
            request.Method = RequestMethod.Get;
            request.BuildUri(uri);

            // This is the actual test
            var response = await client.SendRequestAsync(request, default);
            PagedResult<Entity> actual = await client.CreatePagedResultAsync(response, default);

            // Check that the response created is the right type
            Assert.IsNotNull(actual);
            Assert.AreEqual(5, actual.Results.Length);
            Assert.AreEqual(248, actual.Count);
        }
        #endregion

        #region CreatedPagedResult
        [TestMethod]
        public async Task CreatePagedResult_Deserializes_FullBody()
        {
            var uri = new Uri("https://localhost");
            var mockTransport = new MockTransport();
            var options = new MobileTableClientOptions
            {
                Transport = new HttpClientTransport(mockTransport.Client)
            };

            var client = new ServiceRestClient<Entity>(uri, null, options);

            // Mock the response
            mockTransport.Response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(pagedResponse)
            };

            // Create a basic request
            var request = client.Pipeline.CreateRequest();
            request.Method = RequestMethod.Get;
            request.BuildUri(uri);

            // This is the actual test
            var response = await client.SendRequestAsync(request, default);
            PagedResult<Entity> actual = client.CreatePagedResult(response, default);

            // Check that the response created is the right type
            Assert.IsNotNull(actual);
            Assert.AreEqual(5, actual.Results.Length);
            Assert.AreEqual(248, actual.Count);
        }
        #endregion
    }
}
