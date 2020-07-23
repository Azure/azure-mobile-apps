using Azure.Mobile.Server.Test.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Azure.Mobile.Server.Test.TableController
{
    [TestClass]
    public class Read_Test : Base_Test
    {
        Movie TestMovie = new Movie()
        {
            Id = "movie-4",
            BestPictureWinner = false,
            Duration = 161,
            MpaaRating = null,
            ReleaseDate = new DateTime(1967, 12, 29, 0, 0, 0, DateTimeKind.Utc),
            Title = "The Good, the Bad and the Ugly",
            Year = 1966
        };

        [TestMethod]
        public async Task ReadItemAsync_WithValidId_Returns200()
        {
            var server = E2EServer.Program.GetTestServer();
            var response = await SendRequestToServer<Movie>(server, HttpMethod.Get, $"/tables/movies/{TestMovie.Id}", null);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var actual = await GetValueFromResponse<Movie>(response);

            HttpAssert.AreEqual(actual.Version, response.Headers.ETag);
            HttpAssert.Match(actual.UpdatedAt, response.Content.Headers.LastModified);
            Assert.IsTrue(TestMovie.Equals(actual));
        }
    }
}
