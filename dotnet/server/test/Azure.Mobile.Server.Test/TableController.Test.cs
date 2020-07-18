using Azure.Mobile.Common.Test;
using Azure.Mobile.Server.Entity;
using Azure.Mobile.Server.Utils;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

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
        
        // Test controller for testing null check.
        class TestTableController : TableController<Movie>
        {
            public TestTableController(): base(null)
            {
            }
        }

        #region CTOR
        [TestMethod]
        public void CtorWithRepository_SetsTableRepository()
        {
            var context = MovieDbContext.InMemoryContext();
            var controller = new MoviesController(context);
            Assert.IsNotNull(controller.TableRepository);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CtorWithRepository_NullThrows()
        {
            var controller = new TestTableController();
            Assert.Fail("ArgumentNullException expected");
        }
        #endregion

        #region TableRepository
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TableRepository_ThrowsIfNull()
        {
            var controller = new MoviesController();
            Assert.IsNull(controller.TableRepository);
            Assert.Fail("InvalidOperationException expected");
        }

        [TestMethod]
        public void TableRepository_Roundtrips()
        {
            var context = MovieDbContext.InMemoryContext();
            var controller = new MoviesController();
            controller.TableRepository = new EntityTableRepository<Movie>(context);
            Assert.IsNotNull(controller.TableRepository);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TableRepository_ThrowsIfSetTwice()
        {
            var context = MovieDbContext.InMemoryContext();
            var controller = new MoviesController(context);
            controller.TableRepository = new EntityTableRepository<Movie>(context);
            Assert.Fail("InvalidOperationException expected");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TableRepository_ThrowsIfSetNull()
        {
            var context = MovieDbContext.InMemoryContext();
            var controller = new MoviesController(context);
            controller.TableRepository = null;
            Assert.Fail("ArgumentNullException expected");
        }
        #endregion

        #region Overridable Modifiers
        [TestMethod]
        public void IsAuthorized_ReturnsTrue()
        {
            var context = MovieDbContext.InMemoryContext();
            var controller = new MoviesController(context);
            Assert.IsTrue(controller.BaseIsAuthorized(TableOperation.None, null));
        }

        [TestMethod]
        public void PrepareItemForStore_ReturnsSelf()
        {
            var context = MovieDbContext.InMemoryContext();
            var controller = new MoviesController(context);
            var testItem = TestData.RandomMovie();
            Assert.AreEqual(testItem, controller.BasePrepareItemForStore(testItem));
        }
        #endregion

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

        #region ReadItemAsync
        [TestMethod]
        public async Task ReadItem_ExistingId_Returns200()
        {
            var context = MovieDbContext.InMemoryContext();
            var controller = new MoviesController(context);
            var testItem = TestData.RandomMovie();
            controller.SetRequest(HttpMethod.Get, $"https://foo.com/tables/movies/{testItem.Id}");

            var response = await controller.ReadItemAsync(testItem.Id);
            Assert.IsInstanceOfType(response, typeof(ObjectResult));

            var actual = response as ObjectResult;
            Assert.AreEqual(200, actual.StatusCode);

            var returnedItem = actual.Value as Movie;
            Assert.AreEqual(testItem, returnedItem);

            var responseHeaders = controller.Response.Headers;
            Assert.AreEqual(ETag.FromByteArray(testItem.Version), responseHeaders["ETag"][0]);
            Assert.AreEqual(testItem.UpdatedAt.ToString("r"), responseHeaders["Last-Modified"][0]);

            // Calls IsAuthorized
            Assert.AreEqual(1, controller.IsAuthorizedCallCount);
        }

        [TestMethod]
        public async Task ReadItem_IsNotAuthorized_Returns404()
        {
            var context = MovieDbContext.InMemoryContext();
            var controller = new MoviesController(context) { IsAuthorizedResult = false };
            var testItem = TestData.RandomMovie();
            controller.SetRequest(HttpMethod.Get, $"https://foo.com/tables/movies/{testItem.Id}");

            var response = await controller.ReadItemAsync(testItem.Id);
            Assert.IsInstanceOfType(response, typeof(NotFoundResult));

            var actual = response as NotFoundResult;
            Assert.AreEqual(404, actual.StatusCode);
            Assert.AreEqual(1, controller.IsAuthorizedCallCount);
        }

        [TestMethod]
        public async Task ReadItem_MissingItem_Returns404()
        {
            var context = MovieDbContext.InMemoryContext();
            var controller = new MoviesController(context);
            var testId = Guid.NewGuid().ToString();
            controller.SetRequest(HttpMethod.Get, $"https://foo.com/tables/movies/{testId}");

            var response = await controller.ReadItemAsync(testId);
            Assert.IsInstanceOfType(response, typeof(NotFoundResult));

            var actual = response as NotFoundResult;
            Assert.AreEqual(404, actual.StatusCode);
        }

        [TestMethod]
        public async Task ReadItem_SoftDeletedItem_Returns404()
        {
            var context = MovieDbContext.InMemoryContext();
            var options = new TableControllerOptions<Movie> { SoftDeleteEnabled = true };
            var controller = new MoviesController(context) { TableControllerOptions = options };
            var testItem = TestData.RandomMovie();
            controller.SetRequest(HttpMethod.Get, $"https://foo.com/tables/movies/{testItem.Id}");

            await controller.DeleteItemAsync(testItem.Id);
            var response = await controller.ReadItemAsync(testItem.Id);
            Assert.IsInstanceOfType(response, typeof(NotFoundResult));

            var actual = response as NotFoundResult;
            Assert.AreEqual(404, actual.StatusCode);
        }

        [TestMethod]
        public async Task ReadItem_PreconditionFailed_Returns304()
        {
            var context = MovieDbContext.InMemoryContext();
            var controller = new MoviesController(context);
            var testItem = TestData.RandomMovie();
            controller.SetRequest(HttpMethod.Get, $"https://foo.com/tables/movies/{testItem.Id}", new Dictionary<string, string>()
            {
                { "If-None-Match", ETag.FromByteArray(testItem.Version) }
            });

            var response = await controller.ReadItemAsync(testItem.Id);
            Assert.IsInstanceOfType(response, typeof(ObjectResult));

            var actual = response as ObjectResult;
            Assert.AreEqual(304, actual.StatusCode);

            var responseHeaders = controller.Response.Headers;
            Assert.AreEqual(ETag.FromByteArray(testItem.Version), responseHeaders["ETag"][0]);
            Assert.AreEqual(testItem.UpdatedAt.ToString("r"), responseHeaders["Last-Modified"][0]);
        }

        [TestMethod]
        public async Task ReadItem_PreconditionSuccess_Returns200()
        {
            var context = MovieDbContext.InMemoryContext();
            var controller = new MoviesController(context);
            var testItem = TestData.RandomMovie();
            controller.SetRequest(HttpMethod.Get, $"https://foo.com/tables/movies/{testItem.Id}", new Dictionary<string, string>()
            {
                { "If-Match", ETag.FromByteArray(testItem.Version) }
            });

            var response = await controller.ReadItemAsync(testItem.Id);
            Assert.IsInstanceOfType(response, typeof(ObjectResult));

            var actual = response as ObjectResult;
            Assert.AreEqual(200, actual.StatusCode);
            
            var returnedItem = actual.Value as Movie;
            Assert.AreEqual(testItem, returnedItem);

            var responseHeaders = controller.Response.Headers;
            Assert.AreEqual(ETag.FromByteArray(testItem.Version), responseHeaders["ETag"][0]);
            Assert.AreEqual(testItem.UpdatedAt.ToString("r"), responseHeaders["Last-Modified"][0]);
        }
        #endregion

        #region CreateItemAsync
        [TestMethod]
        public async Task CreatItem_MissingItem_Returns201()
        {
            var context = MovieDbContext.InMemoryContext();
            var controller = new MoviesController(context);
            var newItem = new Movie() { Title = "My Home Movie", MpaaRating = "G", BestPictureWinner = false, Duration = 42, ReleaseDate = DateTime.Parse("7/4/2020"), Year = 2020 };
            var originalItem = newItem.Clone();
            controller.SetRequest(HttpMethod.Post, "https://foo.com/tables/movies");

            var response = await controller.CreateItemAsync(newItem);
            Assert.IsInstanceOfType(response, typeof(CreatedAtActionResult));

            var actual = response as CreatedAtActionResult;
            Assert.AreEqual(201, actual.StatusCode);

            var returnedItem = actual.Value as Movie;
            Assert.AreEqual(originalItem.Title, returnedItem.Title);
            CollectionAssert.AreNotEqual(originalItem.Version, returnedItem.Version);
            Assert.IsTrue(DateTimeOffset.UtcNow.Subtract(returnedItem.UpdatedAt).TotalMilliseconds < 500);

            var responseHeaders = controller.Response.Headers;
            Assert.AreEqual(ETag.FromByteArray(returnedItem.Version), responseHeaders["ETag"][0]);
            Assert.AreEqual(returnedItem.UpdatedAt.ToString("r"), responseHeaders["Last-Modified"][0]);

            // Calls IsAuthorized
            Assert.AreEqual(1, controller.IsAuthorizedCallCount);
            // Calls PrepareItemForStore
            Assert.AreEqual(1, controller.PrepareItemForStoreCallCount);
        }

        [TestMethod]
        public async Task CreatItem_IsNotAuthorized_Returns401()
        {
            var context = MovieDbContext.InMemoryContext();
            var controller = new MoviesController(context) { IsAuthorizedResult = false };
            var newItem = new Movie() { Title = "My Home Movie", MpaaRating = "G", BestPictureWinner = false, Duration = 42, ReleaseDate = DateTime.Parse("7/4/2020"), Year = 2020 };
            controller.SetRequest(HttpMethod.Post, "https://foo.com/tables/movies");

            var response = await controller.CreateItemAsync(newItem);
            Assert.IsInstanceOfType(response, typeof(UnauthorizedResult));

            var actual = response as UnauthorizedResult;
            Assert.AreEqual(401, actual.StatusCode);

            // Calls IsAuthorized
            Assert.AreEqual(1, controller.IsAuthorizedCallCount);
        }

        [TestMethod]
        public async Task CreatItem_IdenticalItem_Returns409()
        {
            var context = MovieDbContext.InMemoryContext();
            var controller = new MoviesController(context);
            var newItem = TestData.RandomMovie();
            var originalItem = newItem.Clone();
            controller.SetRequest(HttpMethod.Post, "https://foo.com/tables/movies");

            var response = await controller.CreateItemAsync(newItem);
            Assert.IsInstanceOfType(response, typeof(ConflictObjectResult));

            var actual = response as ConflictObjectResult;
            Assert.AreEqual(409, actual.StatusCode);

            var returnedItem = actual.Value as Movie;
            Assert.AreEqual(originalItem, returnedItem);

            var responseHeaders = controller.Response.Headers;
            Assert.AreEqual(ETag.FromByteArray(returnedItem.Version), responseHeaders["ETag"][0]);
            Assert.AreEqual(returnedItem.UpdatedAt.ToString("r"), responseHeaders["Last-Modified"][0]);
        }

        [TestMethod]
        public async Task CreatItem_MissingId_Returns201_AndCreatesId()
        {
            var context = MovieDbContext.InMemoryContext();
            var controller = new MoviesController(context);
            var newItem = new Movie() { Id = null, Title = "My Home Movie", MpaaRating = "G", BestPictureWinner = false, Duration = 42, ReleaseDate = DateTime.Parse("7/4/2020"), Year = 2020 };
            var originalItem = newItem.Clone();
            controller.SetRequest(HttpMethod.Post, "https://foo.com/tables/movies");

            var response = await controller.CreateItemAsync(newItem);
            Assert.IsInstanceOfType(response, typeof(CreatedAtActionResult));

            var actual = response as CreatedAtActionResult;
            Assert.AreEqual(201, actual.StatusCode);

            var returnedItem = actual.Value as Movie;
            Assert.IsNotNull(returnedItem.Id);
            Assert.AreEqual(32, returnedItem.Id.Length);    // It's a GUID!
            Assert.AreEqual(originalItem.Title, returnedItem.Title);
            CollectionAssert.AreNotEqual(originalItem.Version, returnedItem.Version);
            Assert.IsTrue(DateTimeOffset.UtcNow.Subtract(returnedItem.UpdatedAt).TotalMilliseconds < 500);

            var responseHeaders = controller.Response.Headers;
            Assert.AreEqual(ETag.FromByteArray(returnedItem.Version), responseHeaders["ETag"][0]);
            Assert.AreEqual(returnedItem.UpdatedAt.ToString("r"), responseHeaders["Last-Modified"][0]);

            // Calls IsAuthorized
            Assert.AreEqual(1, controller.IsAuthorizedCallCount);
            // Calls PrepareItemForStore
            Assert.AreEqual(1, controller.PrepareItemForStoreCallCount);
        }

        [TestMethod]
        public async Task CreatItem_PreconditionsSuccess_Returns201()
        {
            var context = MovieDbContext.InMemoryContext();
            var controller = new MoviesController(context);
            var newItem = new Movie() { Title = "My Home Movie", MpaaRating = "G", BestPictureWinner = false, Duration = 42, ReleaseDate = DateTime.Parse("7/4/2020"), Year = 2020 };
            var originalItem = newItem.Clone();
            controller.SetRequest(HttpMethod.Post, "https://foo.com/tables/movies", new Dictionary<string, string>()
            {
                { "If-None-Match", "*" }
            });

            var response = await controller.CreateItemAsync(newItem);
            Assert.IsInstanceOfType(response, typeof(CreatedAtActionResult));

            var actual = response as CreatedAtActionResult;
            Assert.AreEqual(201, actual.StatusCode);

            var returnedItem = actual.Value as Movie;
            Assert.AreEqual(originalItem.Title, returnedItem.Title);
            CollectionAssert.AreNotEqual(originalItem.Version, returnedItem.Version);
            Assert.IsTrue(DateTimeOffset.UtcNow.Subtract(returnedItem.UpdatedAt).TotalMilliseconds < 500);

            var responseHeaders = controller.Response.Headers;
            Assert.AreEqual(ETag.FromByteArray(returnedItem.Version), responseHeaders["ETag"][0]);
            Assert.AreEqual(returnedItem.UpdatedAt.ToString("r"), responseHeaders["Last-Modified"][0]);

            // Calls IsAuthorized
            Assert.AreEqual(1, controller.IsAuthorizedCallCount);
            // Calls PrepareItemForStore
            Assert.AreEqual(1, controller.PrepareItemForStoreCallCount);
        }

        [TestMethod]
        public async Task CreatItem_PreconditionsFail_Returns412()
        {
            var context = MovieDbContext.InMemoryContext();
            var controller = new MoviesController(context);
            var newItem = TestData.RandomMovie();
            controller.SetRequest(HttpMethod.Post, "https://foo.com/tables/movies", new Dictionary<string, string>()
            {
                { "If-None-Match", "*" }
            });

            var response = await controller.CreateItemAsync(newItem);
            Assert.IsInstanceOfType(response, typeof(ObjectResult));

            var actual = response as ObjectResult;
            Assert.AreEqual(412, actual.StatusCode);
        }
        #endregion

        #region DeleteItemAsync
        [TestMethod]
        public async Task DeleteItem_Existing_SoftDeleteDisabled_Returns204()
        {
            var context = MovieDbContext.InMemoryContext();
            var controller = new MoviesController(context);
            var testItem = TestData.RandomMovie();
            var originalItem = testItem.Clone();
            controller.SetRequest(HttpMethod.Delete, $"https://foo.com/tables/movies/{testItem.Id}");

            var response = await controller.DeleteItemAsync(testItem.Id);
            Assert.IsInstanceOfType(response, typeof(NoContentResult));

            var actual = response as NoContentResult;
            Assert.AreEqual(204, actual.StatusCode);

            // Check the context for the existance of the Id
            Assert.IsFalse(context.Movies.Any(m => m.Id == originalItem.Id));

            // Calls IsAuthorized
            Assert.AreEqual(1, controller.IsAuthorizedCallCount);
            // Doesn't call PrepareItemForStore
            Assert.AreEqual(0, controller.PrepareItemForStoreCallCount);
        }

        [TestMethod]
        public async Task DeleteItem_Existing_SoftDeleteEnabled_Returns204()
        {
            var context = MovieDbContext.InMemoryContext();
            var options = new TableControllerOptions<Movie>() { SoftDeleteEnabled = true };
            var controller = new MoviesController(context) { TableControllerOptions = options };
            var testItem = TestData.RandomMovie();
            var originalItem = testItem.Clone();
            controller.SetRequest(HttpMethod.Delete, $"https://foo.com/tables/movies/{testItem.Id}");

            var response = await controller.DeleteItemAsync(testItem.Id);
            Assert.IsInstanceOfType(response, typeof(NoContentResult));

            var actual = response as NoContentResult;
            Assert.AreEqual(204, actual.StatusCode);

            // Check the context for the existance of the Id with a Deleted = true flag
            Assert.IsTrue(context.Movies.Any(m => m.Id == originalItem.Id && m.Deleted == true));

            // TODO: Check that UpdatedAt is updated since originalItem
            // TODO: Check that Version is updated since originalItem

            // Calls IsAuthorized
            Assert.AreEqual(1, controller.IsAuthorizedCallCount);
            // Does call PrepareItemForStore
            Assert.AreEqual(1, controller.PrepareItemForStoreCallCount);
        }

        [TestMethod]
        public async Task DeleteItem_Missing_SoftDeleteDisabled_Returns404()
        {
            var context = MovieDbContext.InMemoryContext();
            var controller = new MoviesController(context);
            var testItem = new Movie();
            controller.SetRequest(HttpMethod.Delete, $"https://foo.com/tables/movies/{testItem.Id}");

            var response = await controller.DeleteItemAsync(testItem.Id);
            Assert.IsInstanceOfType(response, typeof(NotFoundResult));

            var actual = response as NotFoundResult;
            Assert.AreEqual(404, actual.StatusCode);
        }

        [TestMethod]
        public async Task DeleteItem_Missing_SoftDeleteEnabled_Returns404()
        {
            var context = MovieDbContext.InMemoryContext();
            var options = new TableControllerOptions<Movie>() { SoftDeleteEnabled = true };
            var controller = new MoviesController(context) { TableControllerOptions = options };
            var testItem = new Movie();
            controller.SetRequest(HttpMethod.Delete, $"https://foo.com/tables/movies/{testItem.Id}");

            var response = await controller.DeleteItemAsync(testItem.Id);
            Assert.IsInstanceOfType(response, typeof(NotFoundResult));

            var actual = response as NotFoundResult;
            Assert.AreEqual(404, actual.StatusCode);
        }

        [TestMethod]
        public async Task DeleteItem_ExistingTwice_SoftDeleteEnabled_Returns404()
        {
            var context = MovieDbContext.InMemoryContext();
            var options = new TableControllerOptions<Movie>() { SoftDeleteEnabled = true };
            var controller = new MoviesController(context) { TableControllerOptions = options };
            var testItem = TestData.RandomMovie();
            var originalItem = testItem.Clone();
            controller.SetRequest(HttpMethod.Delete, $"https://foo.com/tables/movies/{testItem.Id}");

            await controller.DeleteItemAsync(testItem.Id);
            var response = await controller.DeleteItemAsync(originalItem.Id);
            Assert.IsInstanceOfType(response, typeof(NotFoundResult));

            var actual = response as NotFoundResult;
            Assert.AreEqual(404, actual.StatusCode);

            // Check the context for the existance of the Id with a Deleted = true flag
            Assert.IsTrue(context.Movies.Any(m => m.Id == originalItem.Id && m.Deleted == true));
        }

        [TestMethod]
        public async Task DeleteItem_Unauthorized_Returns404()
        {
            var context = MovieDbContext.InMemoryContext();
            var controller = new MoviesController(context) { IsAuthorizedResult = false };
            var testItem = TestData.RandomMovie();
            var originalItem = testItem.Clone();
            controller.SetRequest(HttpMethod.Delete, $"https://foo.com/tables/movies/{testItem.Id}");

            var response = await controller.DeleteItemAsync(testItem.Id);
            Assert.IsInstanceOfType(response, typeof(NotFoundResult));

            var actual = response as NotFoundResult;
            Assert.AreEqual(404, actual.StatusCode);

            // Check the context for the existance of the Id
            Assert.IsTrue(context.Movies.Any(m => m.Id == originalItem.Id));

            // Calls IsAuthorized
            Assert.AreEqual(1, controller.IsAuthorizedCallCount);
            // Doesn't call PrepareItemForStore
            Assert.AreEqual(0, controller.PrepareItemForStoreCallCount);
        }

        [TestMethod]
        public async Task DeleteItem_PreconditionsFail_Returns412()
        {
            var context = MovieDbContext.InMemoryContext();
            var controller = new MoviesController(context);
            var testItem = TestData.RandomMovie();
            var originalItem = testItem.Clone();
            controller.SetRequest(HttpMethod.Delete, $"https://foo.com/tables/movies/{testItem.Id}", new Dictionary<string, string>()
            {
                { "If-None-Match", ETag.FromByteArray(testItem.Version) }
            });

            var response = await controller.DeleteItemAsync(testItem.Id);
            Assert.IsInstanceOfType(response, typeof(ObjectResult));

            var actual = response as ObjectResult;
            Assert.AreEqual(412, actual.StatusCode);

            // Check the context for the existance of the Id - should still exist
            Assert.IsTrue(context.Movies.Any(m => m.Id == originalItem.Id));
        }

        [TestMethod]
        public async Task DeleteItem_Existing_PreconditionsSucceed_Returns204()
        {
            var context = MovieDbContext.InMemoryContext();
            var controller = new MoviesController(context);
            var testItem = TestData.RandomMovie();
            var originalItem = testItem.Clone();
            controller.SetRequest(HttpMethod.Delete, $"https://foo.com/tables/movies/{testItem.Id}", new Dictionary<string, string>()
            {
                { "If-Match", ETag.FromByteArray(testItem.Version) }
            });

            var response = await controller.DeleteItemAsync(testItem.Id);
            Assert.IsInstanceOfType(response, typeof(NoContentResult));

            var actual = response as NoContentResult;
            Assert.AreEqual(204, actual.StatusCode);

            // Check the context for the existance of the Id
            Assert.IsFalse(context.Movies.Any(m => m.Id == originalItem.Id));
        }
        #endregion

        #region ReplaceItemAsync
        [TestMethod]
        public async Task ReplaceItem_ExistingItem_Returns200()
        {
            var context = MovieDbContext.InMemoryContext();
            var controller = new MoviesController(context);
            var testItem = TestData.RandomMovie().Clone();
            var originalItem = testItem.Clone();
            testItem.Title = "Replaced";
            controller.SetRequest(HttpMethod.Put, $"https://foo.com/tables/movies/{testItem.Id}");

            var response = await controller.ReplaceItemAsync(testItem.Id, testItem);
            Assert.IsInstanceOfType(response, typeof(ObjectResult));

            var actual = response as ObjectResult;
            Assert.AreEqual(200, actual.StatusCode);

            var returnedItem = actual.Value as Movie;
            Assert.AreEqual(originalItem.Id, returnedItem.Id);
            Assert.AreEqual("Replaced", returnedItem.Title);
            CollectionAssert.AreNotEqual(originalItem.Version, returnedItem.Version);
            Assert.IsTrue(DateTimeOffset.UtcNow.Subtract(returnedItem.UpdatedAt).TotalMilliseconds < 500);

            // Check the context for the existance of the Id with right Title.
            Assert.IsTrue(context.Movies.Any(m => m.Id == originalItem.Id && m.Title == "Replaced"));

            var responseHeaders = controller.Response.Headers;
            Assert.AreEqual(ETag.FromByteArray(returnedItem.Version), responseHeaders["ETag"][0]);
            Assert.AreEqual(returnedItem.UpdatedAt.ToString("r"), responseHeaders["Last-Modified"][0]);

            // Calls IsAuthorized
            Assert.AreEqual(1, controller.IsAuthorizedCallCount);
            // Does call PrepareItemForStore
            Assert.AreEqual(1, controller.PrepareItemForStoreCallCount);
        }

        [TestMethod]
        public async Task ReplaceItem_MissingItem_Returns404()
        {
            var context = MovieDbContext.InMemoryContext();
            var controller = new MoviesController(context);
            var testItem = new Movie();
            controller.SetRequest(HttpMethod.Put, $"https://foo.com/tables/movies/{testItem.Id}");

            var response = await controller.ReplaceItemAsync(testItem.Id, testItem);
            Assert.IsInstanceOfType(response, typeof(NotFoundResult));

            var actual = response as NotFoundResult;
            Assert.AreEqual(404, actual.StatusCode);
        }

        [TestMethod]
        public async Task ReplaceItem_MismatchedId_Returns400()
        {
            var context = MovieDbContext.InMemoryContext();
            var controller = new MoviesController(context);
            var testItem = TestData.RandomMovie().Clone();
            var originalItem = testItem.Clone();
            testItem.Title = "Replaced";
            controller.SetRequest(HttpMethod.Put, $"https://foo.com/tables/movies/{Guid.NewGuid()}");

            var response = await controller.ReplaceItemAsync(Guid.NewGuid().ToString(), testItem);
            Assert.IsInstanceOfType(response, typeof(BadRequestResult));

            var actual = response as BadRequestResult;
            Assert.AreEqual(400, actual.StatusCode);
        }
        [TestMethod]
        public async Task ReplaceItem_DeletedItem_SoftDeleteEnabled_Returns404()
        {
            var context = MovieDbContext.InMemoryContext();
            var options = new TableControllerOptions<Movie>() { SoftDeleteEnabled = true };
            var controller = new MoviesController(context) { TableControllerOptions = options };
            var testItem = TestData.RandomMovie().Clone();
            controller.SetRequest(HttpMethod.Put, $"https://foo.com/tables/movies/{testItem.Id}");

            await controller.DeleteItemAsync(testItem.Id);
            var response = await controller.ReplaceItemAsync(testItem.Id, testItem);
            Assert.IsInstanceOfType(response, typeof(NotFoundResult));

            var actual = response as NotFoundResult;
            Assert.AreEqual(404, actual.StatusCode);
        }

        [TestMethod]
        public async Task ReplaceItem_Unauthorized_Returns404()
        {
            var context = MovieDbContext.InMemoryContext();
            var controller = new MoviesController(context) { IsAuthorizedResult = false };
            var testItem = TestData.RandomMovie();
            controller.SetRequest(HttpMethod.Put, $"https://foo.com/tables/movies/{testItem.Id}");

            var response = await controller.ReplaceItemAsync(testItem.Id, testItem);
            Assert.IsInstanceOfType(response, typeof(NotFoundResult));

            var actual = response as NotFoundResult;
            Assert.AreEqual(404, actual.StatusCode);
        }

        [TestMethod]
        public async Task ReplaceItem_PreconditionsFail_Returns412()
        {
            var context = MovieDbContext.InMemoryContext();
            var controller = new MoviesController(context);
            var testItem = TestData.RandomMovie().Clone();
            var originalItem = testItem.Clone();
            testItem.Title = "Replaced";
            controller.SetRequest(HttpMethod.Put, $"https://foo.com/tables/movies/{testItem.Id}", new Dictionary<string, string>()
            {
                { "If-None-Match", ETag.FromByteArray(testItem.Version) }
            });

            var response = await controller.ReplaceItemAsync(testItem.Id, testItem);
            Assert.IsInstanceOfType(response, typeof(ObjectResult));

            var actual = response as ObjectResult;
            Assert.AreEqual(412, actual.StatusCode);

            var returnedItem = actual.Value as Movie;
            Assert.AreEqual(originalItem, returnedItem);

            var responseHeaders = controller.Response.Headers;
            Assert.AreEqual(ETag.FromByteArray(returnedItem.Version), responseHeaders["ETag"][0]);
            Assert.AreEqual(returnedItem.UpdatedAt.ToString("r"), responseHeaders["Last-Modified"][0]);
        }

        [TestMethod]
        public async Task ReplaceItem_PreconditionSucceed_Returns200()
        {
            var context = MovieDbContext.InMemoryContext();
            var controller = new MoviesController(context);
            var testItem = TestData.RandomMovie().Clone();
            var originalItem = testItem.Clone();
            testItem.Title = "Replaced";
            controller.SetRequest(HttpMethod.Put, $"https://foo.com/tables/movies/{testItem.Id}", new Dictionary<string, string>()
            {
                { "If-Match", ETag.FromByteArray(testItem.Version) }
            });

            var response = await controller.ReplaceItemAsync(testItem.Id, testItem);
            Assert.IsInstanceOfType(response, typeof(ObjectResult));

            var actual = response as ObjectResult;
            Assert.AreEqual(200, actual.StatusCode);

            var returnedItem = actual.Value as Movie;
            Assert.AreEqual(originalItem.Id, returnedItem.Id);
            Assert.AreEqual("Replaced", returnedItem.Title);
            CollectionAssert.AreNotEqual(originalItem.Version, returnedItem.Version);
            Assert.IsTrue(DateTimeOffset.UtcNow.Subtract(returnedItem.UpdatedAt).TotalMilliseconds < 500);
        }
        #endregion

        #region PatchItemAsync
        [TestMethod]
        public async Task PatchItem_ExistingItem_Returns200()
        {
            var context = MovieDbContext.InMemoryContext();
            var controller = new MoviesController(context);
            var testItem = TestData.RandomMovie().Clone();
            JsonPatchDocument<Movie> patchDoc = new JsonPatchDocument<Movie>();
            patchDoc.Replace(e => e.Title, "Replaced");
            controller.SetRequest(HttpMethod.Patch, $"https://foo.com/tables/movies/{testItem.Id}");

            var response = await controller.PatchItemAsync(testItem.Id, patchDoc);
            Assert.IsInstanceOfType(response, typeof(ObjectResult));

            var actual = response as ObjectResult;
            Assert.AreEqual(200, actual.StatusCode);

            var returnedItem = actual.Value as Movie;
            Assert.AreEqual(testItem.Id, returnedItem.Id);
            Assert.AreEqual("Replaced", returnedItem.Title);
            CollectionAssert.AreNotEqual(testItem.Version, returnedItem.Version);
            Assert.IsTrue(DateTimeOffset.UtcNow.Subtract(returnedItem.UpdatedAt).TotalMilliseconds < 500);

            // Check the context for the existance of the Id with right Title.
            Assert.IsTrue(context.Movies.Any(m => m.Id == testItem.Id && m.Title == "Replaced"));

            var responseHeaders = controller.Response.Headers;
            Assert.AreEqual(ETag.FromByteArray(returnedItem.Version), responseHeaders["ETag"][0]);
            Assert.AreEqual(returnedItem.UpdatedAt.ToString("r"), responseHeaders["Last-Modified"][0]);

            // Calls IsAuthorized
            Assert.AreEqual(1, controller.IsAuthorizedCallCount);
            // Does call PrepareItemForStore
            Assert.AreEqual(1, controller.PrepareItemForStoreCallCount);
        }

        [TestMethod]
        public async Task PatchItem_MissingItem_Returns404()
        {
            var context = MovieDbContext.InMemoryContext();
            var controller = new MoviesController(context);
            var testItem = new Movie();
            JsonPatchDocument<Movie> patchDoc = new JsonPatchDocument<Movie>();
            patchDoc.Replace(e => e.Title, "Replaced");
            controller.SetRequest(HttpMethod.Patch, $"https://foo.com/tables/movies/{testItem.Id}");

            var response = await controller.PatchItemAsync(testItem.Id, patchDoc);
            Assert.IsInstanceOfType(response, typeof(NotFoundResult));

            var actual = response as NotFoundResult;
            Assert.AreEqual(404, actual.StatusCode);
        }

        [TestMethod]
        public async Task PatchItem_MismatchedId_Returns400()
        {
            var context = MovieDbContext.InMemoryContext();
            var controller = new MoviesController(context);
            var testItem = TestData.RandomMovie().Clone();
            JsonPatchDocument<Movie> patchDoc = new JsonPatchDocument<Movie>();
            patchDoc.Replace(e => e.Title, "Replaced");
            patchDoc.Replace(e => e.Id, "Replaced");
            controller.SetRequest(HttpMethod.Patch, $"https://foo.com/tables/movies/{testItem.Id}");

            var response = await controller.PatchItemAsync(testItem.Id, patchDoc);
            Assert.IsInstanceOfType(response, typeof(BadRequestResult));

            var actual = response as BadRequestResult;
            Assert.AreEqual(400, actual.StatusCode);
        }

        [TestMethod]
        public async Task PatchItem_DeletedItem_SoftDeleteEnabled_Returns404()
        {
            var context = MovieDbContext.InMemoryContext();
            var options = new TableControllerOptions<Movie>() { SoftDeleteEnabled = true };
            var controller = new MoviesController(context) { TableControllerOptions = options };
            var testItem = TestData.RandomMovie().Clone();
            JsonPatchDocument<Movie> patchDoc = new JsonPatchDocument<Movie>();
            patchDoc.Replace(e => e.Title, "Replaced");
            controller.SetRequest(HttpMethod.Patch, $"https://foo.com/tables/movies/{testItem.Id}");

            await controller.DeleteItemAsync(testItem.Id);
            var response = await controller.PatchItemAsync(testItem.Id, patchDoc);
            Assert.IsInstanceOfType(response, typeof(NotFoundResult));

            var actual = response as NotFoundResult;
            Assert.AreEqual(404, actual.StatusCode);
        }

        [TestMethod]
        public async Task PatchItem_DeletedItem_SoftDeleteEnabled_CanUndeleteItem()
        {
            var context = MovieDbContext.InMemoryContext();
            var options = new TableControllerOptions<Movie>() { SoftDeleteEnabled = true };
            var controller = new MoviesController(context) { TableControllerOptions = options };
            var testItem = TestData.RandomMovie().Clone();
            JsonPatchDocument<Movie> patchDoc = new JsonPatchDocument<Movie>();
            patchDoc.Replace(e => e.Title, "Replaced");
            patchDoc.Replace(e => e.Deleted, false);
            controller.SetRequest(HttpMethod.Patch, $"https://foo.com/tables/movies/{testItem.Id}");

            await controller.DeleteItemAsync(testItem.Id);
            Assert.IsTrue(context.Movies.Any(m => m.Id == testItem.Id));
            var response = await controller.PatchItemAsync(testItem.Id, patchDoc);
            Assert.IsInstanceOfType(response, typeof(ObjectResult));

            var actual = response as ObjectResult;
            Assert.AreEqual(200, actual.StatusCode);

            var returnedItem = actual.Value as Movie;
            Assert.AreEqual(testItem.Id, returnedItem.Id);
            Assert.AreEqual("Replaced", returnedItem.Title);
            CollectionAssert.AreNotEqual(testItem.Version, returnedItem.Version);
            Assert.IsTrue(DateTimeOffset.UtcNow.Subtract(returnedItem.UpdatedAt).TotalMilliseconds < 500);
        }

        [TestMethod]
        public async Task PatchItem_Unauthorized_Returns404()
        {
            var context = MovieDbContext.InMemoryContext();
            var controller = new MoviesController(context) { IsAuthorizedResult = false };
            var testItem = TestData.RandomMovie();
            JsonPatchDocument<Movie> patchDoc = new JsonPatchDocument<Movie>();
            patchDoc.Replace(e => e.Title, "Replaced");
            controller.SetRequest(HttpMethod.Put, $"https://foo.com/tables/movies/{testItem.Id}");

            var response = await controller.PatchItemAsync(testItem.Id, patchDoc);
            Assert.IsInstanceOfType(response, typeof(NotFoundResult));

            var actual = response as NotFoundResult;
            Assert.AreEqual(404, actual.StatusCode);
        }

        [TestMethod]
        public async Task PatchItem_PreconditionsFail_Returns412()
        {
            var context = MovieDbContext.InMemoryContext();
            var controller = new MoviesController(context);
            var testItem = TestData.RandomMovie().Clone();
            var originalItem = testItem.Clone();
            JsonPatchDocument<Movie> patchDoc = new JsonPatchDocument<Movie>();
            patchDoc.Replace(e => e.Title, "Replaced");
            controller.SetRequest(HttpMethod.Put, $"https://foo.com/tables/movies/{testItem.Id}", new Dictionary<string, string>()
            {
                { "If-None-Match", ETag.FromByteArray(testItem.Version) }
            });

            var response = await controller.PatchItemAsync(testItem.Id, patchDoc);
            Assert.IsInstanceOfType(response, typeof(ObjectResult));

            var actual = response as ObjectResult;
            Assert.AreEqual(412, actual.StatusCode);

            var returnedItem = actual.Value as Movie;
            Assert.AreEqual(originalItem, returnedItem);

            var responseHeaders = controller.Response.Headers;
            Assert.AreEqual(ETag.FromByteArray(returnedItem.Version), responseHeaders["ETag"][0]);
            Assert.AreEqual(returnedItem.UpdatedAt.ToString("r"), responseHeaders["Last-Modified"][0]);
        }

        [TestMethod]
        public async Task PatchItem_PreconditionSucceed_Returns200()
        {
            var context = MovieDbContext.InMemoryContext();
            var controller = new MoviesController(context);
            var testItem = TestData.RandomMovie().Clone();
            JsonPatchDocument<Movie> patchDoc = new JsonPatchDocument<Movie>();
            patchDoc.Replace(e => e.Title, "Replaced");
            controller.SetRequest(HttpMethod.Put, $"https://foo.com/tables/movies/{testItem.Id}", new Dictionary<string, string>()
            {
                { "If-Match", ETag.FromByteArray(testItem.Version) }
            });

            var response = await controller.PatchItemAsync(testItem.Id, patchDoc);
            Assert.IsInstanceOfType(response, typeof(ObjectResult));

            var actual = response as ObjectResult;
            Assert.AreEqual(200, actual.StatusCode);

            var returnedItem = actual.Value as Movie;
            Assert.AreEqual(testItem.Id, returnedItem.Id);
            Assert.AreEqual("Replaced", returnedItem.Title);
            CollectionAssert.AreNotEqual(testItem.Version, returnedItem.Version);
            Assert.IsTrue(DateTimeOffset.UtcNow.Subtract(returnedItem.UpdatedAt).TotalMilliseconds < 500);
        }
        #endregion
    }
}
