using Azure.Mobile.Server.Test.Helpers;
using Azure.Mobile.Server.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Azure.Mobile.Server.Test.TableController
{
    [TestClass]
    public class Read_Test : Base_Test
    {
        Movie Movie4 = new Movie()
        {
            Id = "movie-4",
            BestPictureWinner = false,
            Duration = 161,
            MpaaRating = null,
            ReleaseDate = new DateTime(1967, 12, 29, 0, 0, 0, DateTimeKind.Utc),
            Title = "The Good, the Bad and the Ugly",
            Year = 1966
        };

        Movie Movie6 = new Movie()
        {
            Id = "movie-6",
            BestPictureWinner = false,
            Duration = 152,
            MpaaRating = "PG-13",
            ReleaseDate = new DateTime(2008, 07, 18, 0, 0, 0, DateTimeKind.Utc),
            Title = "The Dark Knight",
            Year = 2008
        };

        [TestMethod]
        public async Task ReadItemAsync_WithValidId_Returns200()
        {
            var response = await SendRequestToServer<Movie>(HttpMethod.Get, $"/tables/movies/{Movie4.Id}", null);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var actual = await GetValueFromResponse<Movie>(response);

            HttpAssert.AreEqual(actual.Version, response.Headers.ETag);
            HttpAssert.Match(actual.UpdatedAt, response.Content.Headers.LastModified);
            Assert.IsTrue(Movie4.Equals(actual));
        }
        
        [TestMethod]
        public async Task ReadItemAsync_NotAuthorized_Returns404()
        {
            var response = await SendRequestToServer<Movie>(HttpMethod.Get, $"/tables/unauthorized/{Movie4.Id}", null);

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task ReadItemAsync_WithInvalidId_Returns404()
        {
            var response = await SendRequestToServer<Movie>(HttpMethod.Get, $"/tables/movies/missing", null);

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task ReadItemAsync_SoftDelete_DeletedItem_Returns404()
        {
            var response = await SendRequestToServer<Movie>(HttpMethod.Get, $"/tables/rmovies/rmovie-0", null);

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task ReadItemAsync_SoftDelete_ValidItem_Returns200()
        {
            var response = await SendRequestToServer<Movie>(HttpMethod.Get, $"/tables/rmovies/rmovie-6", null);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var actual = await GetValueFromResponse<Movie>(response);

            HttpAssert.AreEqual(actual.Version, response.Headers.ETag);
            HttpAssert.Match(actual.UpdatedAt, response.Content.Headers.LastModified);
            Assert.IsTrue(Movie6.Equals(actual));
        }

        [TestMethod]
        public async Task ReadItemAsync_PreconditionsFailed_Returns304()
        {
            var firstResponse = await SendRequestToServer<Movie>(HttpMethod.Get, $"/tables/movies/{Movie4.Id}", null);
            Assert.AreEqual(HttpStatusCode.OK, firstResponse.StatusCode);
            var firstActual = await GetValueFromResponse<Movie>(firstResponse);

            var response = await SendRequestToServer<Movie>(HttpMethod.Get, $"/tables/movies/{Movie4.Id}", null, new Dictionary<string, string>
            {
                { "If-None-Match", ETag.FromByteArray(firstActual.Version) }
            });

            Assert.AreEqual(HttpStatusCode.NotModified, response.StatusCode);
        }

        [TestMethod]
        public async Task ReadItemAsync_PreconditionsSucceed_Returns200()
        {
            var firstResponse = await SendRequestToServer<Movie>(HttpMethod.Get, $"/tables/movies/{Movie4.Id}", null);
            Assert.AreEqual(HttpStatusCode.OK, firstResponse.StatusCode);
            var firstActual = await GetValueFromResponse<Movie>(firstResponse);

            var response = await SendRequestToServer<Movie>(HttpMethod.Get, $"/tables/movies/{Movie4.Id}", null, new Dictionary<string, string>
            {
                { "If-Match", ETag.FromByteArray(firstActual.Version) }
            });

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            var actual = await GetValueFromResponse<Movie>(response);

            HttpAssert.AreEqual(actual.Version, response.Headers.ETag);
            HttpAssert.Match(actual.UpdatedAt, response.Content.Headers.LastModified);
            Assert.IsTrue(Movie4.Equals(actual));
        }
    }
}
