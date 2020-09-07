﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.AspNet.OData;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Zumo.Server.Test.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Microsoft.Zumo.Server.Test
{
    [TestClass]
    public class TableController_Tests : BaseTest
    {
        #region Test Data
        const int ExpectedPageSize = 50;

        class TestTableController : TableController<Movie>
        {
            public TestTableController() : base(null) { }
        }

        /// <summary>
        /// Populates the test data into the repository.
        /// </summary>
        /// <param name="repository">The repository</param>
        /// <param name="markDeleted">If true, mark all the R-rated movies as deleted</param>
        private void PopulateRepository(MockTableRepository<Movie> repository, bool markDeleted = false)
        {
            for (var i = 0; i < TestData.Movies.Length; i++)
            {
                var clone = TestData.Movies[i].Clone();
                clone.Id = $"movie-{i}";
                clone.Version = Guid.NewGuid().ToByteArray();
                clone.UpdatedAt = DateTimeOffset.UtcNow.AddDays(-(180 + (new Random()).Next(180)));
                clone.Deleted = markDeleted && clone.MpaaRating == "R";
                repository.Data.Add(clone.Id, clone);
            }
        }

        /// <summary>
        /// Adds a header to the Request being sent to the controller
        /// </summary>
        /// <param name="context"></param>
        /// <param name="headerName"></param>
        /// <param name="headerValue"></param>
        private void AddHeaderToRequest(HttpContext context, string headerName, string headerValue)
        {
            context.Request.Headers[headerName] = new StringValues(headerValue);
        }
        #endregion

        #region Constructors
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Ctor_NullRepository_Throws()
        {
            _ = new TestTableController();
            Assert.Fail("ArgumentNullExpception expected");
        }

        [TestMethod]
        public void Ctor_WithRepository_SetsTableRepositry()
        {
            var controller = new MoviesController(new MockTableRepository<Movie>());
            Assert.IsNotNull(controller);
            Assert.IsNotNull(controller.TableRepository);
        }
        #endregion

        #region TableRepository
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TableRepository_GetThrows_IfNull()
        {
            var controller = new MoviesController();
            _ = controller.TableRepository;
            Assert.Fail("InvalidOperationException expected");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TableRepository_SetThrows_IfNull()
        {
            _ = new MoviesController() { TableRepository = null };
            Assert.Fail("ArgumentNullException expected");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TableRepository_SetThrows_IfSetTwice()
        {
            var repository = new MockTableRepository<Movie>();
            _ = new MoviesController(repository) { TableRepository = repository };
            Assert.Fail("InvalidOperationException expected");
        }

        [TestMethod]
        public void TableRepository_RoundTrips()
        {
            var repository = new MockTableRepository<Movie>();
            var controller = new MoviesController(repository);
            Assert.AreEqual(repository, controller.TableRepository);
        }
        #endregion

        #region IsAuthorized
        [DataTestMethod]
        [DataRow(TableOperation.Create)]
        [DataRow(TableOperation.Delete)]
        [DataRow(TableOperation.None)]
        [DataRow(TableOperation.Patch)]
        [DataRow(TableOperation.Read)]
        [DataRow(TableOperation.Replace)]
        public void IsAuthorized_ReturnsTrue(TableOperation operation)
        {
            var controller = new MoviesController(new MockTableRepository<Movie>());
            var item = new Movie { Title = "foo" };
            Assert.IsTrue(controller.BaseIsAuthorized(operation, null));
            Assert.IsTrue(controller.BaseIsAuthorized(operation, item));
        }
        #endregion

        #region ValidateOperation (sync and async)
        [DataTestMethod]
        [DataRow(TableOperation.Create)]
        [DataRow(TableOperation.Delete)]
        [DataRow(TableOperation.None)]
        [DataRow(TableOperation.Patch)]
        [DataRow(TableOperation.Read)]
        [DataRow(TableOperation.Replace)]
        public void ValidateOperation_Returns200(TableOperation operation)
        {
            var controller = new MoviesController(new MockTableRepository<Movie>());
            var item = new Movie { Title = "foo" };
            Assert.AreEqual(200, controller.BaseValidateOperation(operation, null));
            Assert.AreEqual(200, controller.BaseValidateOperation(operation, item));
        }

        [DataTestMethod]
        [DataRow(TableOperation.Create)]
        [DataRow(TableOperation.Delete)]
        [DataRow(TableOperation.None)]
        [DataRow(TableOperation.Patch)]
        [DataRow(TableOperation.Read)]
        [DataRow(TableOperation.Replace)]
        public async Task ValidateOperationAsync_Returns200(TableOperation operation)
        {
            var controller = new MoviesController(new MockTableRepository<Movie>());
            var item = new Movie { Title = "foo" };
            Assert.AreEqual(200, await controller.BaseValidateOperationAsync(operation, null));
            Assert.AreEqual(200, await controller.BaseValidateOperationAsync(operation, item));
        }
        #endregion

        #region PrepareItemForStore (sync and async)
        [TestMethod]
        public void PrepareItemForStore_ReturnsSelf()
        {
            var controller = new MoviesController(new MockTableRepository<Movie>());
            var item = new Movie { Title = "foo" };
            var expected = item.Clone();
            Assert.AreEqual(expected, controller.BasePrepareItemForStore(item));
        }

        [TestMethod]
        public void PrepareItemForStore_ReturnsNull_OnNull()
        {
            var controller = new MoviesController(new MockTableRepository<Movie>());
            Assert.IsNull(controller.BasePrepareItemForStore(null));
        }

        [TestMethod]
        public async Task PrepareItemForStoreAsync_ReturnsSelf()
        {
            var controller = new MoviesController(new MockTableRepository<Movie>());
            var item = new Movie { Title = "foo" };
            var expected = item.Clone();
            var actual = await controller.BasePrepareItemForStoreAsync(item);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public async Task PrepareItemForStoreAsync_ReturnsNull_OnNull()
        {
            var controller = new MoviesController(new MockTableRepository<Movie>());
            var actual = await controller.BasePrepareItemForStoreAsync(null);
            Assert.IsNull(actual);
        }
        #endregion

        #region CreateItemAsync
        [TestMethod]
        public async Task CreateItemAsync_Returns403_WhenIsAuthorized_False()
        {
            var repository = new MockTableRepository<Movie>();
            var controller = new MoviesController(repository)
            {
                IsAuthorizedResult = false
            };
            var newItem = new Movie() { Title = "Foo" };
            
            var actual = await controller.CreateItemAsync(newItem);

            Assert.IsInstanceOfType(actual, typeof(StatusCodeResult));
            var objectResult = actual as StatusCodeResult;
            Assert.AreEqual(403, objectResult.StatusCode);

            Assert.AreEqual(1, controller.IsAuthorizedCount);
            Assert.AreEqual(newItem.Id, controller.LastAuthorizedMovie.Id);

            // Check the repository has not been modified
            Assert.AreEqual(0, repository.Modifications);
        }

        [TestMethod]
        public async Task CreateItemAsync_ReturnsValidateResult_WhenValidateOperations_Used()
        {
            var repository = new MockTableRepository<Movie>();
            var controller = new MoviesController(repository)
            {
                ValidateOperationResult = 418
            };
            var newItem = new Movie() { Title = "Foo" };

            var actual = await controller.CreateItemAsync(newItem);

            Assert.IsInstanceOfType(actual, typeof(StatusCodeResult));
            var objectResult = actual as StatusCodeResult;
            Assert.AreEqual(418, objectResult.StatusCode);

            Assert.AreEqual(1, controller.ValidateOperationCount);
            Assert.IsFalse(controller.WasLastValidateOperationAsync);
            Assert.AreEqual(newItem.Id, controller.LastValidateOperationMovie.Id);

            // Check the repository has not been modified
            Assert.AreEqual(0, repository.Modifications);

        }

        [TestMethod]
        public async Task CreateItemAsync_ReturnsValidateResult_WhenValidateOperationsAsync_Used()
        {
            var repository = new MockTableRepository<Movie>();
            var controller = new MoviesController(repository)
            {
                ValidateOperationAsyncResult = 418
            };
            var newItem = new Movie() { Title = "Foo" };

            var actual = await controller.CreateItemAsync(newItem);

            Assert.IsInstanceOfType(actual, typeof(StatusCodeResult));
            var objectResult = actual as StatusCodeResult;
            Assert.AreEqual(418, objectResult.StatusCode);

            Assert.AreEqual(1, controller.ValidateOperationCount);
            Assert.IsTrue(controller.WasLastValidateOperationAsync);
            Assert.AreEqual(newItem.Id, controller.LastValidateOperationMovie.Id);

            // Check the repository has not been modified
            Assert.AreEqual(0, repository.Modifications);
        }

        [TestMethod]
        public async Task CreateItemAsync_Returns201_WhenInsertingValidItem()
        {
            var repository = new MockTableRepository<Movie>();
            PopulateRepository(repository);
            var controller = new MoviesController(repository);
            var expectedItem = new Movie()
            {
                BestPictureWinner = false,
                Duration = 60,
                MpaaRating = "G",
                ReleaseDate = new DateTime(2020, 12, 24),
                Title = "Home Movie",
                Year = 2020
            };
            var newItem = expectedItem.Clone();
            var httpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext = httpContext;
            var nItems = repository.Data.Count;

            var response = await controller.CreateItemAsync(newItem);

            Assert.IsInstanceOfType(response, typeof(ObjectResult));
            var actual = response as ObjectResult;

            // Status Code is correct
            Assert.AreEqual(201, actual.StatusCode);

            // Response value is correct
            Assert.IsNotNull(actual.Value);
            var actualItem = actual.Value as Movie;
            Assert.IsInstanceOfType(actualItem, typeof(Movie));
            Assert.AreEqual(newItem, actualItem);

            // Version and UpdatedAt are correct
            Assert.IsNotNull(actualItem.Version);
            Assert.IsNotNull(actualItem.UpdatedAt);
            CollectionAssert.AreNotEqual(expectedItem.Version, actualItem.Version);
            TimestampAssert.AreClose(DateTimeOffset.UtcNow, actualItem.UpdatedAt);

            // ETag and Last-Modified headers are set
            HttpAssert.HasResponseHeader(httpContext, "ETag", $"\"{Convert.ToBase64String(actualItem.Version)}\"");
            HttpAssert.HasResponseHeader(httpContext, "Last-Modified", actualItem.UpdatedAt.ToString("r"));

            // The repository was called and modified.
            Assert.AreEqual(1, repository.Modifications);
            Assert.AreEqual("CreateAsync", repository.CallData);

            // The mock repository contains a record with the ID
            Assert.IsTrue(repository.Data.ContainsKey(newItem.Id));
            Assert.AreEqual(nItems + 1, repository.Data.Count);
        }

        [TestMethod]
        public async Task CreateItemAsync_Returns201_WhenInsertingValidItemWithNullId()
        {
            var repository = new MockTableRepository<Movie>();
            PopulateRepository(repository);
            var controller = new MoviesController(repository);
            var expectedItem = new Movie()
            {
                Id = null,
                BestPictureWinner = false,
                Duration = 60,
                MpaaRating = "G",
                ReleaseDate = new DateTime(2020, 12, 24),
                Title = "Home Movie",
                Year = 2020
            };
            var newItem = expectedItem.Clone();
            var httpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext = httpContext;
            var nItems = repository.Data.Count;

            var response = await controller.CreateItemAsync(newItem);

            Assert.IsInstanceOfType(response, typeof(ObjectResult));
            var actual = response as ObjectResult;

            // Status Code is correct
            Assert.AreEqual(201, actual.StatusCode);

            // Response value is correct
            Assert.IsNotNull(actual.Value);
            var actualItem = actual.Value as Movie;
            Assert.IsInstanceOfType(actualItem, typeof(Movie));
            // Copy Id from actualItem into newItem before comparing
            Assert.IsNotNull(actualItem.Id);
            newItem.Id = actualItem.Id;
            Assert.AreEqual(newItem, actualItem);

            // Version and UpdatedAt are correct
            Assert.IsNotNull(actualItem.Version);
            Assert.IsNotNull(actualItem.UpdatedAt);
            CollectionAssert.AreNotEqual(expectedItem.Version, actualItem.Version);
            TimestampAssert.AreClose(DateTimeOffset.UtcNow, actualItem.UpdatedAt);

            // ETag and Last-Modified headers are set
            HttpAssert.HasResponseHeader(httpContext, "ETag", $"\"{Convert.ToBase64String(actualItem.Version)}\"");
            HttpAssert.HasResponseHeader(httpContext, "Last-Modified", actualItem.UpdatedAt.ToString("r"));

            // The repository was called and modified.
            Assert.AreEqual(1, repository.Modifications);
            Assert.AreEqual("CreateAsync", repository.CallData);

            // The mock repository contains a record with the ID
            Assert.IsTrue(repository.Data.ContainsKey(newItem.Id));
            Assert.AreEqual(nItems + 1, repository.Data.Count);
        }

        [TestMethod]
        public async Task CreateItemAsync_Returns409_WhenExistingIdAdded()
        {
            var repository = new MockTableRepository<Movie>();
            PopulateRepository(repository);
            var controller = new MoviesController(repository);
            var expectedItem = repository.Data["movie-5"];
            var newItem = expectedItem.Clone(); newItem.Title = "Replacement";
            var httpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext = httpContext;
            var nItems = repository.Data.Count;

            var response = await controller.CreateItemAsync(newItem);

            Assert.IsInstanceOfType(response, typeof(ObjectResult));
            var actual = response as ObjectResult;

            // Status Code is correct
            Assert.AreEqual(409, actual.StatusCode);

            // Response value is correct
            Assert.IsNotNull(actual.Value);
            var actualItem = actual.Value as Movie;
            Assert.IsInstanceOfType(actualItem, typeof(Movie));
            Assert.AreEqual(expectedItem, actualItem);

            // Version and UpdatedAt are correct
            Assert.IsNotNull(actualItem.Version);
            Assert.IsNotNull(actualItem.UpdatedAt);
            CollectionAssert.AreEqual(expectedItem.Version, actualItem.Version);
            Assert.AreEqual(expectedItem.UpdatedAt, actualItem.UpdatedAt);

            // ETag and Last-Modified headers are set
            HttpAssert.HasResponseHeader(httpContext, "ETag", $"\"{Convert.ToBase64String(expectedItem.Version)}\"");
            HttpAssert.HasResponseHeader(httpContext, "Last-Modified", expectedItem.UpdatedAt.ToString("r"));

            // The repository was NOT called
            Assert.AreEqual(0, repository.Modifications);
            Assert.AreEqual(nItems, repository.Data.Count);
        }

        [TestMethod]
        public async Task CreateItemAsync_Returns412_WhenPreconditionsFail()
        {
            var repository = new MockTableRepository<Movie>();
            PopulateRepository(repository);
            var controller = new MoviesController(repository);
            var expectedItem = repository.Data["movie-5"];
            var newItem = expectedItem.Clone(); newItem.Title = "Replacement";
            var httpContext = new DefaultHttpContext();
            AddHeaderToRequest(httpContext, "If-None-Match", "*");
            controller.ControllerContext.HttpContext = httpContext;
            var nItems = repository.Data.Count;

            var response = await controller.CreateItemAsync(newItem);

            Assert.IsInstanceOfType(response, typeof(ObjectResult));
            var actual = response as ObjectResult;

            // Status Code is correct
            Assert.AreEqual(412, actual.StatusCode);

            // Response value is correct
            Assert.IsNotNull(actual.Value);
            var actualItem = actual.Value as Movie;
            Assert.IsInstanceOfType(actualItem, typeof(Movie));
            Assert.AreEqual(expectedItem, actualItem);

            // Version and UpdatedAt are correct
            Assert.IsNotNull(actualItem.Version);
            Assert.IsNotNull(actualItem.UpdatedAt);
            CollectionAssert.AreEqual(expectedItem.Version, actualItem.Version);
            Assert.AreEqual(expectedItem.UpdatedAt, actualItem.UpdatedAt);

            // ETag and Last-Modified headers are set
            HttpAssert.HasResponseHeader(httpContext, "ETag", $"\"{Convert.ToBase64String(expectedItem.Version)}\"");
            HttpAssert.HasResponseHeader(httpContext, "Last-Modified", expectedItem.UpdatedAt.ToString("r"));

            // The repository was NOT called
            Assert.AreEqual(0, repository.Modifications);
            Assert.AreEqual(nItems, repository.Data.Count);
        }

        [TestMethod]
        public async Task CreateItemAsync_Returns201_WhenPreconditionsMatch()
        {
            var repository = new MockTableRepository<Movie>();
            PopulateRepository(repository);
            var controller = new MoviesController(repository);
            var expectedItem = new Movie()
            {
                BestPictureWinner = false,
                Duration = 60,
                MpaaRating = "G",
                ReleaseDate = new DateTime(2020, 12, 24),
                Title = "Home Movie",
                Year = 2020
            };
            var newItem = expectedItem.Clone();
            var httpContext = new DefaultHttpContext();
            AddHeaderToRequest(httpContext, "If-None-Match", "*");
            controller.ControllerContext.HttpContext = httpContext;
            var nItems = repository.Data.Count;

            var response = await controller.CreateItemAsync(newItem);

            Assert.IsInstanceOfType(response, typeof(ObjectResult));
            var actual = response as ObjectResult;

            // Status Code is correct
            Assert.AreEqual(201, actual.StatusCode);

            // Response value is correct
            Assert.IsNotNull(actual.Value);
            var actualItem = actual.Value as Movie;
            Assert.IsInstanceOfType(actualItem, typeof(Movie));
            Assert.AreEqual(newItem, actualItem);

            // Version and UpdatedAt are correct
            Assert.IsNotNull(actualItem.Version);
            Assert.IsNotNull(actualItem.UpdatedAt);
            CollectionAssert.AreNotEqual(expectedItem.Version, actualItem.Version);
            TimestampAssert.AreClose(DateTimeOffset.UtcNow, actualItem.UpdatedAt);

            // ETag and Last-Modified headers are set
            HttpAssert.HasResponseHeader(httpContext, "ETag", $"\"{Convert.ToBase64String(actualItem.Version)}\"");
            HttpAssert.HasResponseHeader(httpContext, "Last-Modified", actualItem.UpdatedAt.ToString("r"));

            // The repository was called
            Assert.AreEqual("CreateAsync", repository.CallData);

            // The mock repository contains a record with the ID
            Assert.AreEqual(1, repository.Modifications);
            Assert.IsTrue(repository.Data.ContainsKey(newItem.Id));
            Assert.AreEqual(nItems + 1, repository.Data.Count);
        }
        #endregion

        #region DeleteItemAsync
        [TestMethod]
        public async Task DeleteItemAsync_Returns403_WhenIsAuthorized_False()
        {
            var repository = new MockTableRepository<Movie>();
            PopulateRepository(repository);
            var controller = new MoviesController(repository)
            {
                IsAuthorizedResult = false
            };
            var itemToDelete = repository.Data["movie-5"].Id;

            var actual = await controller.DeleteItemAsync(itemToDelete);

            // The correct status code is returned
            Assert.IsInstanceOfType(actual, typeof(StatusCodeResult));
            var objectResult = actual as StatusCodeResult;
            Assert.AreEqual(403, objectResult.StatusCode);

            // IsAuthorized is called
            Assert.AreEqual(1, controller.IsAuthorizedCount);
            Assert.AreEqual(itemToDelete, controller.LastAuthorizedMovie.Id);

            // The repository is not modified
            Assert.AreEqual(0, repository.Modifications);
        }

        [TestMethod]
        public async Task DeleteItemAsync_ReturnsValidateResult_WhenValidateOperations_Used()
        {
            var repository = new MockTableRepository<Movie>();
            PopulateRepository(repository);
            var controller = new MoviesController(repository)
            {
                ValidateOperationResult = 418
            };
            var itemToDelete = repository.Data["movie-5"].Id;

            var actual = await controller.DeleteItemAsync(itemToDelete);

            // The correct status code is returned
            Assert.IsInstanceOfType(actual, typeof(StatusCodeResult));
            var objectResult = actual as StatusCodeResult;
            Assert.AreEqual(418, objectResult.StatusCode);

            // ValidateOperation is called
            Assert.AreEqual(1, controller.ValidateOperationCount);
            Assert.AreEqual(itemToDelete, controller.LastValidateOperationMovie.Id);

            // The repository is not modified
            Assert.AreEqual(0, repository.Modifications);
        }

        [TestMethod]
        public async Task DeleteItemAsync_ReturnsValidateResult_WhenValidateOperationsAsync_Used()
        {
            var repository = new MockTableRepository<Movie>();
            PopulateRepository(repository);
            var controller = new MoviesController(repository)
            {
                ValidateOperationAsyncResult = 418
            };
            var itemToDelete = repository.Data["movie-5"].Id;

            var actual = await controller.DeleteItemAsync(itemToDelete);

            // The correct status code is returned
            Assert.IsInstanceOfType(actual, typeof(StatusCodeResult));
            var objectResult = actual as StatusCodeResult;
            Assert.AreEqual(418, objectResult.StatusCode);

            // ValidateOperationAsync is called
            Assert.AreEqual(1, controller.ValidateOperationCount);
            Assert.AreEqual(itemToDelete, controller.LastValidateOperationMovie.Id);
            Assert.IsTrue(controller.WasLastValidateOperationAsync);

            // The repository is not modified
            Assert.AreEqual(0, repository.Modifications);
        }

        [TestMethod]
        public async Task DeleteItemAsync_WithSoftDelete_Returns204_WhenDeletingExistingItem()
        {
            var repository = new MockTableRepository<Movie>();
            PopulateRepository(repository);
            var controller = new MoviesController(repository, new TableControllerOptions { SoftDeleteEnabled = true });
            var httpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext = httpContext;
            var nItems = repository.Data.Count;
            var expectedItem = repository.Data["movie-8"];

            var response = await controller.DeleteItemAsync(expectedItem.Id);

            // Response is the correct form
            Assert.IsInstanceOfType(response, typeof(StatusCodeResult));
            var actual = response as StatusCodeResult;

            // Status Code Matches
            Assert.AreEqual(204, actual.StatusCode);

            // The DeleteAsync method in the repository was not called (it's a hard delete)
            Assert.AreEqual(1, repository.Modifications);
            Assert.AreNotEqual("DeleteAsync", repository.CallData);

            // The entity is not actually deleted - just marked deleted
            Assert.AreEqual(nItems, repository.Data.Count);
            Assert.IsTrue(repository.Data["movie-8"].Deleted);
        }

        [TestMethod]
        public async Task DeleteItemAsync_WithHardDelete_Returns204_WhenDeletingExistingItem()
        {
            var repository = new MockTableRepository<Movie>();
            PopulateRepository(repository);
            var controller = new MoviesController(repository);
            var httpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext = httpContext;
            var nItems = repository.Data.Count;
            var expectedItem = repository.Data["movie-8"];

            var response = await controller.DeleteItemAsync(expectedItem.Id);

            // Response is the correct form
            Assert.IsInstanceOfType(response, typeof(StatusCodeResult));
            var actual = response as StatusCodeResult;

            // Status Code Matches
            Assert.AreEqual(204, actual.StatusCode);

            // The DeleteItemAsync method in the repository was the last thing called
            Assert.AreEqual(1, repository.Modifications);
            Assert.AreEqual("DeleteAsync", repository.CallData);

            // The entity is actually deleted
            Assert.AreEqual(nItems - 1, repository.Data.Count);
            Assert.IsFalse(repository.Data.ContainsKey("movie-8"));
        }

        [TestMethod]
        public async Task DeleteItemAsync_WithSoftDelete_Returns404_WhenDeletingMissingItem()
        {
            var repository = new MockTableRepository<Movie>();
            PopulateRepository(repository);
            var controller = new MoviesController(repository, new TableControllerOptions { SoftDeleteEnabled = true });
            var httpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext = httpContext;
            var nItems = repository.Data.Count;
            var expectedItem = "not-present";

            var response = await controller.DeleteItemAsync(expectedItem);

            // Response is the correct form
            Assert.IsInstanceOfType(response, typeof(StatusCodeResult));
            var actual = response as StatusCodeResult;

            // Status Code Matches
            Assert.AreEqual(404, actual.StatusCode);

            // The DeleteItemAsync method was not called
            Assert.AreEqual(0, repository.Modifications);
            Assert.AreNotEqual("DeleteAsync", repository.CallData);

            // The repository is untouched
            Assert.AreEqual(nItems, repository.Data.Count);
        }

        [TestMethod]
        public async Task DeleteItemAsync_WithHardDelete_Returns404_WhenDeletingMissingItem()
        {
            var repository = new MockTableRepository<Movie>();
            PopulateRepository(repository);
            var controller = new MoviesController(repository);
            var httpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext = httpContext;
            var nItems = repository.Data.Count;
            var expectedItem = "not-present";

            var response = await controller.DeleteItemAsync(expectedItem);

            // Response is the correct form
            Assert.IsInstanceOfType(response, typeof(StatusCodeResult));
            var actual = response as StatusCodeResult;

            // Status Code Matches
            Assert.AreEqual(404, actual.StatusCode);

            // The DeleteItemAsync method was not called
            Assert.AreEqual(0, repository.Modifications);
            Assert.AreNotEqual("DeleteAsync", repository.CallData);

            // The repository is untouched
            Assert.AreEqual(nItems, repository.Data.Count);
        }

        [TestMethod]
        public async Task DeleteItemAsync_WithSoftDelete_Returns404_WhenDeletingDeletedItem()
        {
            var repository = new MockTableRepository<Movie>();
            PopulateRepository(repository, true);
            var controller = new MoviesController(repository, new TableControllerOptions { SoftDeleteEnabled = true });
            var httpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext = httpContext;
            var nItems = repository.Data.Count;
            var expectedItem = repository.Data.Values.Where(m => m.Deleted).First().Id;

            var response = await controller.DeleteItemAsync(expectedItem);

            // Response is the correct form
            Assert.IsInstanceOfType(response, typeof(StatusCodeResult));
            var actual = response as StatusCodeResult;

            // Status Code Matches
            Assert.AreEqual(404, actual.StatusCode);

            // The DeleteItemAsync method was not called
            Assert.AreEqual(0, repository.Modifications);
            Assert.AreNotEqual("DeleteAsync", repository.CallData);

            // The repository is untouched
            Assert.AreEqual(nItems, repository.Data.Count);
            Assert.IsTrue(repository.Data[expectedItem].Deleted);
        }

        [TestMethod]
        public async Task DeleteItemAsync_WithSoftDelete_Returns412_WhenPreconditionsFail()
        {
            var repository = new MockTableRepository<Movie>();
            PopulateRepository(repository);
            var controller = new MoviesController(repository, new TableControllerOptions { SoftDeleteEnabled = true });
            var httpContext = new DefaultHttpContext(); 
            AddHeaderToRequest(httpContext, "If-None-Match", "*");
            controller.ControllerContext.HttpContext = httpContext;
            var nItems = repository.Data.Count;
            var expectedItem = repository.Data["movie-8"].Clone();

            var response = await controller.DeleteItemAsync(expectedItem.Id);

            Assert.IsInstanceOfType(response, typeof(ObjectResult));
            var actual = response as ObjectResult;

            // Status Code is correct
            Assert.AreEqual(412, actual.StatusCode);

            // Response value is correct
            Assert.IsNotNull(actual.Value);
            var actualItem = actual.Value as Movie;
            Assert.IsInstanceOfType(actualItem, typeof(Movie));
            Assert.AreEqual(expectedItem, actualItem);

            // Version and UpdatedAt are correct
            Assert.IsNotNull(actualItem.Version);
            Assert.IsNotNull(actualItem.UpdatedAt);
            CollectionAssert.AreEqual(expectedItem.Version, actualItem.Version);
            Assert.AreEqual(expectedItem.UpdatedAt, actualItem.UpdatedAt);

            // ETag and Last-Modified headers are set
            HttpAssert.HasResponseHeader(httpContext, "ETag", $"\"{Convert.ToBase64String(actualItem.Version)}\"");
            HttpAssert.HasResponseHeader(httpContext, "Last-Modified", actualItem.UpdatedAt.ToString("r"));

            // The repository was NOT called
            Assert.AreEqual(0, repository.Modifications);

            // The mock repository is untouched
            Assert.AreEqual(nItems, repository.Data.Count);
        }

        [TestMethod]
        public async Task DeleteItemAsync_WithHardDelete_Returns412_WhenPreconditionsFail()
        {
            var repository = new MockTableRepository<Movie>();
            PopulateRepository(repository);
            var controller = new MoviesController(repository);
            var httpContext = new DefaultHttpContext();
            AddHeaderToRequest(httpContext, "If-None-Match", "*");
            controller.ControllerContext.HttpContext = httpContext;
            var nItems = repository.Data.Count;
            var expectedItem = repository.Data["movie-8"].Clone();

            var response = await controller.DeleteItemAsync(expectedItem.Id);

            Assert.IsInstanceOfType(response, typeof(ObjectResult));
            var actual = response as ObjectResult;

            // Status Code is correct
            Assert.AreEqual(412, actual.StatusCode);

            // Response value is correct
            Assert.IsNotNull(actual.Value);
            var actualItem = actual.Value as Movie;
            Assert.IsInstanceOfType(actualItem, typeof(Movie));
            Assert.AreEqual(expectedItem, actualItem);

            // Version and UpdatedAt are correct
            Assert.IsNotNull(actualItem.Version);
            Assert.IsNotNull(actualItem.UpdatedAt);
            CollectionAssert.AreEqual(expectedItem.Version, actualItem.Version);
            Assert.AreEqual(expectedItem.UpdatedAt, actualItem.UpdatedAt);

            // ETag and Last-Modified headers are set
            HttpAssert.HasResponseHeader(httpContext, "ETag", $"\"{Convert.ToBase64String(actualItem.Version)}\"");
            HttpAssert.HasResponseHeader(httpContext, "Last-Modified", actualItem.UpdatedAt.ToString("r"));

            // The repository was NOT called
            Assert.AreEqual(0, repository.Modifications);

            // The mock repository is untouched
            Assert.AreEqual(nItems, repository.Data.Count);
        }

        [TestMethod]
        public async Task DeleteItemAsync_WithSoftDelete_Returns204_WhenPreconditionsMatch()
        {
            var repository = new MockTableRepository<Movie>();
            PopulateRepository(repository);
            var expectedItem = repository.Data["movie-8"];
            var controller = new MoviesController(repository, new TableControllerOptions { SoftDeleteEnabled = true });
            var httpContext = new DefaultHttpContext();
            AddHeaderToRequest(httpContext, "If-Match", $"\"{Convert.ToBase64String(expectedItem.Version)}\"");
            controller.ControllerContext.HttpContext = httpContext;
            var nItems = repository.Data.Count;

            var response = await controller.DeleteItemAsync(expectedItem.Id);

            // Response is the correct form
            Assert.IsInstanceOfType(response, typeof(StatusCodeResult));
            var actual = response as StatusCodeResult;

            // Status Code Matches
            Assert.AreEqual(204, actual.StatusCode);

            // The ReplaceAsync method in the repository was the last thing called
            Assert.AreEqual(1, repository.Modifications);
            Assert.AreEqual("ReplaceAsync", repository.CallData);

            // The entity is not actually deleted - just marked deleted
            Assert.AreEqual(nItems, repository.Data.Count);
            Assert.IsTrue(repository.Data["movie-8"].Deleted);
        }

        [TestMethod]
        public async Task DeleteItemAsync_WithHardDelete_Returns204_WhenPreconditionsMatch()
        {
            var repository = new MockTableRepository<Movie>();
            PopulateRepository(repository);
            var expectedItem = repository.Data["movie-8"];
            var controller = new MoviesController(repository);
            var httpContext = new DefaultHttpContext();
            AddHeaderToRequest(httpContext, "If-Match", $"\"{Convert.ToBase64String(expectedItem.Version)}\"");
            controller.ControllerContext.HttpContext = httpContext;
            var nItems = repository.Data.Count;

            var response = await controller.DeleteItemAsync(expectedItem.Id);

            // Response is the correct form
            Assert.IsInstanceOfType(response, typeof(StatusCodeResult));
            var actual = response as StatusCodeResult;

            // Status Code Matches
            Assert.AreEqual(204, actual.StatusCode);

            // The DeleteItemAsync method in the repository was the last thing called
            Assert.AreEqual(1, repository.Modifications);
            Assert.AreEqual("DeleteAsync", repository.CallData);

            // The entity is actually deleted
            Assert.AreEqual(nItems - 1, repository.Data.Count);
            Assert.IsFalse(repository.Data.ContainsKey("movie-8"));
        }
        #endregion

        #region GetItemsAsync
        [TestMethod]
        public async Task GetItemsAsync_Returns400_WithBadRequest()
        {
            // This unit test requires a complete ASP.NET Core service set up
            var response = await SendRequestToServer<Movie>(HttpMethod.Get, "/tables/movies?$foo=true");
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [TestMethod]
        public async Task GetItemsAsync_Returns200_WithArray_WhenRequested()
        {
            // This unit test requires a complete ASP.NET Core service set up
            var response = await SendRequestToServer<Movie>(HttpMethod.Get, "/tables/movies");
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var actual = await GetValueFromResponse<List<Movie>>(response);
            Assert.IsNotNull(actual);

            // Ensure that the actual response is a unique list
            Assert.AreEqual(ExpectedPageSize, actual.Count);
            CollectionAssert.AllItemsAreNotNull(actual);
            CollectionAssert.AllItemsAreUnique(actual);
        }

        [TestMethod]
        public async Task GetItemsAsync_CanPageThroughResults()
        {
            var responseCount = 0;
            var totalItemCount = 0;
            int lastItemCount;

            // There are 154 items in the page.
            // Calculate the correct number of pages - it will be n + 1 empty page
            int expectedPages = 5; // (154 / 50 == 4 total pages) + 1 for the blank page.
            do
            {
                var response = await SendRequestToServer<Movie>(HttpMethod.Get, $"/tables/movies?$skip={totalItemCount}");
                responseCount++;

                // The correct status code is returned
                Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

                // The result is a list of items
                var actual = await GetValueFromResponse<List<Movie>>(response);
                Assert.IsNotNull(actual);
                Assert.IsTrue(actual.Count <= ExpectedPageSize);
                CollectionAssert.AllItemsAreUnique(actual);

                lastItemCount = actual.Count;
                totalItemCount += lastItemCount;
            } while (lastItemCount != 0 && responseCount <= expectedPages);

            // Check the results are correct
            Assert.AreEqual(expectedPages, responseCount);
            Assert.AreEqual(0, lastItemCount);
            // The controller is Soft-Delete, so we don't expect the Soft Deleted items to be provided
            Assert.AreEqual(TestData.Movies.Where(m => m.MpaaRating != "R").Count(), totalItemCount);
        }

        [TestMethod]
        public async Task GetItemsAsync_Returns200_WithPagedResult_WhenInlineCountRequested()
        {
            // This unit test requires a complete ASP.NET Core service set up
            var response = await SendRequestToServer<Movie>(HttpMethod.Get, "/tables/movies?$inlinecount=allpages");
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var actual = await GetValueFromResponse<PagedListResult<Movie>>(response);
            Assert.IsNotNull(actual);

            // Ensure that the actual response is a unique list
            Assert.AreEqual(ExpectedPageSize, actual.Results.Count);
            CollectionAssert.AllItemsAreNotNull(actual.Results.ToList());
            CollectionAssert.AllItemsAreUnique(actual.Results.ToList());
            Assert.AreEqual(TestData.Movies.Where(m => m.MpaaRating != "R").Count(), actual.Count);
        }

        [TestMethod]
        public async Task GetItemsAsync_Returns200_WithPagedResult_WhenCountRequested()
        {
            // This unit test requires a complete ASP.NET Core service set up
            var response = await SendRequestToServer<Movie>(HttpMethod.Get, "/tables/movies?$inlinecount=allpages");
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var actual = await GetValueFromResponse<PagedListResult<Movie>>(response);
            Assert.IsNotNull(actual);

            // Ensure that the actual response is a unique list
            Assert.AreEqual(ExpectedPageSize, actual.Results.Count);
            CollectionAssert.AllItemsAreNotNull(actual.Results.ToList());
            CollectionAssert.AllItemsAreUnique(actual.Results.ToList());
            // The controller is Soft-Delete, so we don't expect the Soft Deleted items to be provided
            Assert.AreEqual(TestData.Movies.Where(m => m.MpaaRating != "R").Count(), actual.Count);
        }

        [TestMethod]
        public async Task GetItemsAsync_Returns200_WhenFilterRequested()
        {
            // This unit test requires a complete ASP.NET Core service set up
            var response = await SendRequestToServer<Movie>(HttpMethod.Get, "/tables/movies?$filter=mpaaRating eq 'R'");
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var actual = await GetValueFromResponse<List<Movie>>(response);
            Assert.IsNotNull(actual);

            // Ensure that the actual response is an empty list - all R rated movies are deleted.
            Assert.AreEqual(0, actual.Count);
        }

        [TestMethod]
        public async Task GetItemsAsync_Returns200_WithIdFilter()
        {
            var response = await SendRequestToServer<Movie>(HttpMethod.Get, "/tables/movies?$filter=id eq 'movie-4'");
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var actual = await GetValueFromResponse<List<Movie>>(response);
            Assert.IsNotNull(actual);
            Assert.AreEqual(1, actual.Count);
            Assert.AreEqual("The Good, the Bad and the Ugly", actual[0].Title);
        }

        [TestMethod]
        public async Task GetItemsAsync_Returns200_WhenFilterRequested_WithCount()
        {
            // This unit test requires a complete ASP.NET Core service set up
            var response = await SendRequestToServer<Movie>(HttpMethod.Get, "/tables/movies?$filter=mpaaRating ne 'R'&$inlinecount=allpages");
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var actual = await GetValueFromResponse<PagedListResult<Movie>>(response);
            Assert.IsNotNull(actual);

            // Ensure that the actual response is a unique list - there are 194 non 'R' rated movies
            Assert.AreEqual(ExpectedPageSize, actual.Results.Count);
            CollectionAssert.AllItemsAreNotNull(actual.Results.ToList());
            CollectionAssert.AllItemsAreUnique(actual.Results.ToList());
            Assert.AreEqual(TestData.Movies.Where(m => m.MpaaRating != "R").Count(), actual.Count);
        }

        [TestMethod]
        public async Task GetItemsAsync_Returns200_WhenDeletedItemsRequested_WithCount()
        {
            // This unit test requires a complete ASP.NET Core service set up
            var response = await SendRequestToServer<Movie>(HttpMethod.Get, "/tables/movies?__includedeleted=true&$inlinecount=allpages");
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var actual = await GetValueFromResponse<PagedListResult<Movie>>(response);
            Assert.IsNotNull(actual);

            // Ensure that the actual response is a unique list - there are 194 non 'R' rated movies
            Assert.AreEqual(ExpectedPageSize, actual.Results.Count);
            CollectionAssert.AllItemsAreNotNull(actual.Results.ToList());
            CollectionAssert.AllItemsAreUnique(actual.Results.ToList());
            Assert.AreEqual(TestData.Movies.Length, actual.Count);
        }

        [TestMethod]
        public async Task GetItemsAsync_Returns400_WhenInvalidODataQueryProvided()
        {
            // This unit test requires a complete ASP.NET Core service set up
            var response = await SendRequestToServer<Movie>(HttpMethod.Get, "/tables/movies?$filter=invalid");
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [TestMethod]
        public async Task GetItemsAsync_Returns200_WithDataView()
        {
            // This unit test requires a complete ASP.NET Core service set up
            // This unit test requires a complete ASP.NET Core service set up
            var response = await SendRequestToServer<Movie>(HttpMethod.Get, "/tables/recentmovies");
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var actual = await GetValueFromResponse<List<Movie>>(response);
            Assert.IsNotNull(actual);
            CollectionAssert.AllItemsAreUnique(actual);
            Assert.AreEqual(10, actual.Count);      // This is the page size given in recentmovies controller.
        }

        [TestMethod]
        public async Task GetItemsAsync_Returns200_WithDataView_AndTop()
        {
            // This unit test requires a complete ASP.NET Core service set up
            // This unit test requires a complete ASP.NET Core service set up
            var response = await SendRequestToServer<Movie>(HttpMethod.Get, "/tables/recentmovies?$top=5");
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var actual = await GetValueFromResponse<List<Movie>>(response);
            Assert.IsNotNull(actual);
            CollectionAssert.AllItemsAreUnique(actual);
            Assert.AreEqual(5, actual.Count);      // This is an explicit top value that is valid.
        }


        [TestMethod]
        public async Task GetItemsAsync_Returns400_WithMaxTopExceeded()
        {
            // This unit test requires a complete ASP.NET Core service set up
            // This unit test requires a complete ASP.NET Core service set up
            var response = await SendRequestToServer<Movie>(HttpMethod.Get, "/tables/recentmovies?$top=50");
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }
        #endregion

        #region PatchItemAsync
        [TestMethod]
        public async Task PatchItemAsync_Returns403_WhenIsAuthorized_False()
        {
            var repository = new MockTableRepository<Movie>();
            PopulateRepository(repository);
            var expectedItem = repository.Data["movie-7"].Clone();
            var delta = new Delta<Movie>(typeof(Movie));
            delta.TrySetPropertyValue("Title", "Replaced");
            var controller = new MoviesController(repository)
            {
                IsAuthorizedResult = false
            };

            var actual = await controller.PatchItemAsync(expectedItem.Id, delta);

            // The correct status code is returned
            Assert.IsInstanceOfType(actual, typeof(StatusCodeResult));
            var objectResult = actual as StatusCodeResult;
            Assert.AreEqual(403, objectResult.StatusCode);

            // IsAuthorized is called
            Assert.AreEqual(1, controller.IsAuthorizedCount);
            Assert.AreEqual(expectedItem.Id, controller.LastAuthorizedMovie.Id);

            // The repository is not modified
            Assert.AreEqual(0, repository.Modifications);
        }

        [TestMethod]
        public async Task PatchItemAsync_ReturnsValidateResult_WhenValidateOperations_Used()
        {
            var repository = new MockTableRepository<Movie>();
            PopulateRepository(repository);
            var expectedItem = repository.Data["movie-7"].Clone();
            var delta = new Delta<Movie>(typeof(Movie));
            delta.TrySetPropertyValue("Title", "Replaced");
            var controller = new MoviesController(repository)
            {
                ValidateOperationResult = 418
            };

            var actual = await controller.PatchItemAsync(expectedItem.Id, delta);

            // The correct status code is returned
            Assert.IsInstanceOfType(actual, typeof(StatusCodeResult));
            var objectResult = actual as StatusCodeResult;
            Assert.AreEqual(418, objectResult.StatusCode);

            // ValidateOperation is called
            Assert.AreEqual(1, controller.ValidateOperationCount);
            Assert.AreEqual(expectedItem.Id, controller.LastValidateOperationMovie.Id);

            // The repository is not modified
            Assert.AreEqual(0, repository.Modifications);
        }

        [TestMethod]
        public async Task PatchItemAsync_ReturnsValidateResult_WhenValidateOperationsAsync_Used()
        {
            var repository = new MockTableRepository<Movie>();
            PopulateRepository(repository);
            var expectedItem = repository.Data["movie-7"].Clone();
            var delta = new Delta<Movie>(typeof(Movie));
            delta.TrySetPropertyValue("Title", "Replaced");
            var controller = new MoviesController(repository)
            {
                ValidateOperationAsyncResult = 418
            };

            var actual = await controller.PatchItemAsync(expectedItem.Id, delta);

            // The correct status code is returned
            Assert.IsInstanceOfType(actual, typeof(StatusCodeResult));
            var objectResult = actual as StatusCodeResult;
            Assert.AreEqual(418, objectResult.StatusCode);

            // ValidateOperationAsync is called
            Assert.AreEqual(1, controller.ValidateOperationCount);
            Assert.AreEqual(expectedItem.Id, controller.LastValidateOperationMovie.Id);
            Assert.IsTrue(controller.WasLastValidateOperationAsync);

            // The repository is not modified
            Assert.AreEqual(0, repository.Modifications);
        }

        [TestMethod]
        public async Task PatchItemAsync_Returns200_WithValidReplacement()
        {
            var repository = new MockTableRepository<Movie>();
            PopulateRepository(repository);
            var expectedItem = repository.Data["movie-7"].Clone();
            var delta = new Delta<Movie>(typeof(Movie));
            delta.TrySetPropertyValue("Title", "Replaced");
            var controller = new MoviesController(repository);
            var httpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext = httpContext;

            var response = await controller.PatchItemAsync(expectedItem.Id, delta);

            // The correct status code is returned
            Assert.IsInstanceOfType(response, typeof(ObjectResult));
            var actual = response as ObjectResult;
            Assert.AreEqual(200, actual.StatusCode);

            // Response value is correct
            Assert.IsNotNull(actual.Value);
            var actualItem = actual.Value as Movie;
            Assert.IsInstanceOfType(actualItem, typeof(Movie));
            Assert.AreEqual("Replaced", actualItem.Title);
            Assert.AreEqual(expectedItem.MpaaRating, actualItem.MpaaRating);

            // Version and UpdatedAt are correct
            Assert.IsNotNull(actualItem.Version);
            Assert.IsNotNull(actualItem.UpdatedAt);
            CollectionAssert.AreNotEqual(expectedItem.Version, actualItem.Version);
            TimestampAssert.AreClose(DateTimeOffset.UtcNow, actualItem.UpdatedAt);

            // ETag and Last-Modified headers are set
            HttpAssert.HasResponseHeader(httpContext, "ETag", $"\"{Convert.ToBase64String(actualItem.Version)}\"");
            HttpAssert.HasResponseHeader(httpContext, "Last-Modified", actualItem.UpdatedAt.ToString("r"));

            // The repository is called
            Assert.IsTrue(repository.CallCount > 0);
            Assert.AreEqual("ReplaceAsync", repository.CallData);

            // The repository is modified
            Assert.AreEqual(1, repository.Modifications);
        }

        [TestMethod]
        public async Task PatchItemAsync_Returns404_ReplacingMissingItem()
        {
            var repository = new MockTableRepository<Movie>();
            PopulateRepository(repository);
            var delta = new Delta<Movie>(typeof(Movie));
            delta.TrySetPropertyValue("Title", "Replaced");

            var controller = new MoviesController(repository);
            var httpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext = httpContext;

            var response = await controller.PatchItemAsync("not-present", delta);

            // The correct status code is returned
            Assert.IsInstanceOfType(response, typeof(StatusCodeResult));
            var actual = response as StatusCodeResult;
            Assert.AreEqual(404, actual.StatusCode);

            // The repository is called (so this is an actual lookup in the repository)
            Assert.IsTrue(repository.CallCount > 0);

            // The repository is not modified
            Assert.AreEqual(0, repository.Modifications);
        }

        [TestMethod]
        public async Task PatchItemAsync_WithSoftDelete_Returns200_WhenUndeletingItem()
        {
            var repository = new MockTableRepository<Movie>();
            PopulateRepository(repository, true);
            var expectedItem = repository.Data.Values.Where(m => m.Deleted).First().Clone();
            var delta = new Delta<Movie>(typeof(Movie));
            delta.TrySetPropertyValue("Deleted", false);
            var controller = new MoviesController(repository, new TableControllerOptions { SoftDeleteEnabled = true });
            var httpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext = httpContext;

            var response = await controller.PatchItemAsync(expectedItem.Id, delta);

            // The correct status code is returned
            Assert.IsInstanceOfType(response, typeof(ObjectResult));
            var actual = response as ObjectResult;
            Assert.AreEqual(200, actual.StatusCode);

            // Response value is correct
            Assert.IsNotNull(actual.Value);
            var actualItem = actual.Value as Movie;
            Assert.IsInstanceOfType(actualItem, typeof(Movie));
            Assert.IsFalse(actualItem.Deleted);
            Assert.AreEqual(expectedItem.MpaaRating, actualItem.MpaaRating);

            // Version and UpdatedAt are correct
            Assert.IsNotNull(actualItem.Version);
            Assert.IsNotNull(actualItem.UpdatedAt);
            CollectionAssert.AreNotEqual(expectedItem.Version, actualItem.Version);
            TimestampAssert.AreClose(DateTimeOffset.UtcNow, actualItem.UpdatedAt);

            // ETag and Last-Modified headers are set
            HttpAssert.HasResponseHeader(httpContext, "ETag", $"\"{Convert.ToBase64String(actualItem.Version)}\"");
            HttpAssert.HasResponseHeader(httpContext, "Last-Modified", actualItem.UpdatedAt.ToString("r"));

            // The repository is called
            Assert.IsTrue(repository.CallCount > 0);
            Assert.AreEqual("ReplaceAsync", repository.CallData);

            // The repository is modified
            Assert.AreEqual(1, repository.Modifications);
        }

        [TestMethod]
        public async Task PatchItemAsync_Returns412_WhenETagDoesNotMatch()
        {
            var repository = new MockTableRepository<Movie>();
            PopulateRepository(repository);
            var expectedItem = repository.Data["movie-9"].Clone();
            var delta = new Delta<Movie>(typeof(Movie));
            delta.TrySetPropertyValue("Title", "Replaced");
            var controller = new MoviesController(repository);
            var httpContext = new DefaultHttpContext();
            AddHeaderToRequest(httpContext, "If-Match", $"\"{Guid.NewGuid()}\"");
            controller.ControllerContext.HttpContext = httpContext;

            var response = await controller.PatchItemAsync(expectedItem.Id, delta);

            Assert.IsInstanceOfType(response, typeof(ObjectResult));
            var actual = response as ObjectResult;

            // Status Code is correct
            Assert.AreEqual(412, actual.StatusCode);

            // Response value is correct
            Assert.IsNotNull(actual.Value);
            var actualItem = actual.Value as Movie;
            Assert.IsInstanceOfType(actualItem, typeof(Movie));
            Assert.AreEqual(expectedItem, actualItem);

            // Version and UpdatedAt are correct
            Assert.IsNotNull(actualItem.Version);
            Assert.IsNotNull(actualItem.UpdatedAt);
            CollectionAssert.AreEqual(expectedItem.Version, actualItem.Version);
            Assert.AreEqual(expectedItem.UpdatedAt, actualItem.UpdatedAt);

            // ETag and Last-Modified headers are set
            HttpAssert.HasResponseHeader(httpContext, "ETag", $"\"{Convert.ToBase64String(actualItem.Version)}\"");
            HttpAssert.HasResponseHeader(httpContext, "Last-Modified", actualItem.UpdatedAt.ToString("r"));

            // The repository was NOT called
            Assert.AreEqual(0, repository.Modifications);
        }

        [TestMethod]
        public async Task PatchItemAsync_Returns200_WhenETagMatches()
        {
            var repository = new MockTableRepository<Movie>();
            PopulateRepository(repository);
            var expectedItem = repository.Data["movie-7"].Clone();
            var delta = new Delta<Movie>(typeof(Movie));
            delta.TrySetPropertyValue("Title", "Replaced");
            var controller = new MoviesController(repository);
            var httpContext = new DefaultHttpContext();
            AddHeaderToRequest(httpContext, "If-Match", $"\"{Convert.ToBase64String(expectedItem.Version)}\"");
            controller.ControllerContext.HttpContext = httpContext;

            var response = await controller.PatchItemAsync(expectedItem.Id, delta);

            // The correct status code is returned
            Assert.IsInstanceOfType(response, typeof(ObjectResult));
            var actual = response as ObjectResult;
            Assert.AreEqual(200, actual.StatusCode);

            // Response value is correct
            Assert.IsNotNull(actual.Value);
            var actualItem = actual.Value as Movie;
            Assert.IsInstanceOfType(actualItem, typeof(Movie));
            Assert.AreEqual("Replaced", actualItem.Title);

            // Version and UpdatedAt are correct
            Assert.IsNotNull(actualItem.Version);
            Assert.IsNotNull(actualItem.UpdatedAt);
            CollectionAssert.AreNotEqual(expectedItem.Version, actualItem.Version);
            TimestampAssert.AreClose(DateTimeOffset.UtcNow, actualItem.UpdatedAt);

            // ETag and Last-Modified headers are set
            HttpAssert.HasResponseHeader(httpContext, "ETag", $"\"{Convert.ToBase64String(actualItem.Version)}\"");
            HttpAssert.HasResponseHeader(httpContext, "Last-Modified", actualItem.UpdatedAt.ToString("r"));

            // The repository is called
            Assert.IsTrue(repository.CallCount > 0);
            Assert.AreEqual("ReplaceAsync", repository.CallData);

            // The repository is modified
            Assert.AreEqual(1, repository.Modifications);
        }
        #endregion

        #region ReadItemAsync
        [TestMethod]
        public async Task ReadItemAsync_Returns403_WhenIsAuthorized_False()
        {
            var repository = new MockTableRepository<Movie>();
            PopulateRepository(repository);
            var controller = new MoviesController(repository)
            {
                IsAuthorizedResult = false
            };
            var itemToGet = repository.Data["movie-5"].Id;

            var actual = await controller.DeleteItemAsync(itemToGet);

            // The correct status code is returned
            Assert.IsInstanceOfType(actual, typeof(StatusCodeResult));
            var objectResult = actual as StatusCodeResult;
            Assert.AreEqual(403, objectResult.StatusCode);

            // IsAuthorized is called
            Assert.AreEqual(1, controller.IsAuthorizedCount);
            Assert.AreEqual(itemToGet, controller.LastAuthorizedMovie.Id);

            // The repository is not modified
            Assert.AreEqual(0, repository.Modifications);
        }

        [TestMethod]
        public async Task ReadItemAsync_ReturnsValidateResult_WhenValidateOperations_Used()
        {
            var repository = new MockTableRepository<Movie>();
            PopulateRepository(repository);
            var controller = new MoviesController(repository)
            {
                ValidateOperationResult = 418
            };
            var itemToGet = repository.Data["movie-5"].Id;

            var actual = await controller.ReadItemAsync(itemToGet);

            // The correct status code is returned
            Assert.IsInstanceOfType(actual, typeof(StatusCodeResult));
            var objectResult = actual as StatusCodeResult;
            Assert.AreEqual(418, objectResult.StatusCode);

            // ValidateOperation is called
            Assert.AreEqual(1, controller.ValidateOperationCount);
            Assert.AreEqual(itemToGet, controller.LastValidateOperationMovie.Id);

            // The repository is not modified
            Assert.AreEqual(0, repository.Modifications);
        }

        [TestMethod]
        public async Task ReadItemAsync_ReturnsValidateResult_WhenValidateOperationsAsync_Used()
        {
            var repository = new MockTableRepository<Movie>();
            PopulateRepository(repository);
            var controller = new MoviesController(repository)
            {
                ValidateOperationAsyncResult = 418
            };
            var itemToGet = repository.Data["movie-5"].Id;

            var actual = await controller.ReadItemAsync(itemToGet);

            // The correct status code is returned
            Assert.IsInstanceOfType(actual, typeof(StatusCodeResult));
            var objectResult = actual as StatusCodeResult;
            Assert.AreEqual(418, objectResult.StatusCode);

            // ValidateOperationAsync is called
            Assert.AreEqual(1, controller.ValidateOperationCount);
            Assert.AreEqual(itemToGet, controller.LastValidateOperationMovie.Id);
            Assert.IsTrue(controller.WasLastValidateOperationAsync);

            // The repository is not modified
            Assert.AreEqual(0, repository.Modifications);
        }

        [TestMethod]
        public async Task ReadItemAsync_WithHardDelete_Returns200_WithValidId()
        {
            var repository = new MockTableRepository<Movie>();
            PopulateRepository(repository);
            var controller = new MoviesController(repository, new TableControllerOptions { SoftDeleteEnabled = false });
            var httpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext = httpContext;
            var expectedItem = repository.Data["movie-5"].Clone();

            var response = await controller.ReadItemAsync(expectedItem.Id);

            // The correct status code is returned
            Assert.IsInstanceOfType(response, typeof(ObjectResult));
            var actual = response as ObjectResult;
            Assert.AreEqual(200, actual.StatusCode);

            // Response value is correct
            Assert.IsNotNull(actual.Value);
            var actualItem = actual.Value as Movie;
            Assert.IsInstanceOfType(actualItem, typeof(Movie));
            Assert.AreEqual(expectedItem, actualItem);

            // Version and UpdatedAt are correct
            Assert.IsNotNull(actualItem.Version);
            Assert.IsNotNull(actualItem.UpdatedAt);
            CollectionAssert.AreEqual(expectedItem.Version, actualItem.Version);
            Assert.AreEqual(expectedItem.UpdatedAt, actualItem.UpdatedAt);

            // ETag and Last-Modified headers are set
            HttpAssert.HasResponseHeader(httpContext, "ETag", $"\"{Convert.ToBase64String(actualItem.Version)}\"");
            HttpAssert.HasResponseHeader(httpContext, "Last-Modified", actualItem.UpdatedAt.ToString("r"));

            // The repository is called
            Assert.IsTrue(repository.CallCount > 0);
            Assert.AreEqual("LookupAsync", repository.CallData);

            // The repository is not modified
            Assert.AreEqual(0, repository.Modifications);
        }

        [TestMethod]
        public async Task ReadItemAsync_WithSoftDelete_Returns200_WithValidId()
        {
            var repository = new MockTableRepository<Movie>();
            PopulateRepository(repository);
            var controller = new MoviesController(repository, new TableControllerOptions { SoftDeleteEnabled = true });
            var httpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext = httpContext;
            var expectedItem = repository.Data["movie-5"].Clone();

            var response = await controller.ReadItemAsync(expectedItem.Id);

            // The correct status code is returned
            Assert.IsInstanceOfType(response, typeof(ObjectResult));
            var actual = response as ObjectResult;
            Assert.AreEqual(200, actual.StatusCode);

            // Response value is correct
            Assert.IsNotNull(actual.Value);
            var actualItem = actual.Value as Movie;
            Assert.IsInstanceOfType(actualItem, typeof(Movie));
            Assert.AreEqual(expectedItem, actualItem);

            // Version and UpdatedAt are correct
            Assert.IsNotNull(actualItem.Version);
            Assert.IsNotNull(actualItem.UpdatedAt);
            CollectionAssert.AreEqual(expectedItem.Version, actualItem.Version);
            Assert.AreEqual(expectedItem.UpdatedAt, actualItem.UpdatedAt);

            // ETag and Last-Modified headers are set
            HttpAssert.HasResponseHeader(httpContext, "ETag", $"\"{Convert.ToBase64String(actualItem.Version)}\"");
            HttpAssert.HasResponseHeader(httpContext, "Last-Modified", actualItem.UpdatedAt.ToString("r"));

            // The repository is called
            Assert.IsTrue(repository.CallCount > 0);
            Assert.AreEqual("LookupAsync", repository.CallData);

            // The repository is not modified
            Assert.AreEqual(0, repository.Modifications);
        }

        [TestMethod]
        public async Task ReadItemAsync_WithHardDelete_Returns404_WithInvalidId()
        {
            var repository = new MockTableRepository<Movie>();
            PopulateRepository(repository);
            var controller = new MoviesController(repository, new TableControllerOptions { SoftDeleteEnabled = false });
            var httpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext = httpContext;
            var expectedItem = "not-present";

            var response = await controller.ReadItemAsync(expectedItem);

            // The correct status code is returned
            Assert.IsInstanceOfType(response, typeof(StatusCodeResult));
            var actual = response as StatusCodeResult;
            Assert.AreEqual(404, actual.StatusCode);

            // The repository is called
            Assert.IsTrue(repository.CallCount > 0);
            Assert.AreEqual("LookupAsync", repository.CallData);

            // The repository is not modified
            Assert.AreEqual(0, repository.Modifications);
        }

        [TestMethod]
        public async Task ReadItemAsync_WithSoftDelete_Returns404_WithInvalidId()
        {
            var repository = new MockTableRepository<Movie>();
            PopulateRepository(repository);
            var controller = new MoviesController(repository, new TableControllerOptions { SoftDeleteEnabled = true });
            var httpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext = httpContext;
            var expectedItem = "not-present";

            var response = await controller.ReadItemAsync(expectedItem);

            // The correct status code is returned
            Assert.IsInstanceOfType(response, typeof(StatusCodeResult));
            var actual = response as StatusCodeResult;
            Assert.AreEqual(404, actual.StatusCode);

            // The repository is called
            Assert.IsTrue(repository.CallCount > 0);
            Assert.AreEqual("LookupAsync", repository.CallData);

            // The repository is not modified
            Assert.AreEqual(0, repository.Modifications);
        }

        [TestMethod]
        public async Task ReadItemAsync_WithSoftDelete_Returns404_WithSoftDeletedId()
        {
            var repository = new MockTableRepository<Movie>();
            PopulateRepository(repository, true);
            var controller = new MoviesController(repository, new TableControllerOptions { SoftDeleteEnabled = true });
            var httpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext = httpContext;
            var expectedItem = repository.Data.Values.Where(m => m.Deleted).First().Id;

            var response = await controller.ReadItemAsync(expectedItem);

            // The correct status code is returned
            Assert.IsInstanceOfType(response, typeof(StatusCodeResult));
            var actual = response as StatusCodeResult;
            Assert.AreEqual(404, actual.StatusCode);

            // The repository is called
            Assert.IsTrue(repository.CallCount > 0);

            // The repository is not modified
            Assert.AreEqual(0, repository.Modifications);
        }

        [TestMethod]
        public async Task ReadItemAsync_Returns200_WhenPreconditionsSucceed()
        {
            var repository = new MockTableRepository<Movie>();
            PopulateRepository(repository);
            var expectedItem = repository.Data["movie-5"].Clone();
            var controller = new MoviesController(repository, new TableControllerOptions { SoftDeleteEnabled = true });
            var httpContext = new DefaultHttpContext();
            AddHeaderToRequest(httpContext, "If-Match", $"\"{Convert.ToBase64String(expectedItem.Version)}\"");
            controller.ControllerContext.HttpContext = httpContext;

            var response = await controller.ReadItemAsync(expectedItem.Id);

            // The correct status code is returned
            Assert.IsInstanceOfType(response, typeof(ObjectResult));
            var actual = response as ObjectResult;
            Assert.AreEqual(200, actual.StatusCode);

            // Response value is correct
            Assert.IsNotNull(actual.Value);
            var actualItem = actual.Value as Movie;
            Assert.IsInstanceOfType(actualItem, typeof(Movie));
            Assert.AreEqual(expectedItem, actualItem);

            // Version and UpdatedAt are correct
            Assert.IsNotNull(actualItem.Version);
            Assert.IsNotNull(actualItem.UpdatedAt);
            CollectionAssert.AreEqual(expectedItem.Version, actualItem.Version);
            Assert.AreEqual(expectedItem.UpdatedAt, actualItem.UpdatedAt);

            // ETag and Last-Modified headers are set
            HttpAssert.HasResponseHeader(httpContext, "ETag", $"\"{Convert.ToBase64String(actualItem.Version)}\"");
            HttpAssert.HasResponseHeader(httpContext, "Last-Modified", actualItem.UpdatedAt.ToString("r"));

            // The repository is called
            Assert.IsTrue(repository.CallCount > 0);
            Assert.AreEqual("LookupAsync", repository.CallData);

            // The repository is not modified
            Assert.AreEqual(0, repository.Modifications);
        }

        [TestMethod]
        public async Task ReadItemAsync_Returns304_WhenETagMatches()
        {
            var repository = new MockTableRepository<Movie>();
            PopulateRepository(repository);
            var expectedItem = repository.Data["movie-5"].Clone();
            var controller = new MoviesController(repository, new TableControllerOptions { SoftDeleteEnabled = true });
            var httpContext = new DefaultHttpContext();
            AddHeaderToRequest(httpContext, "If-None-Match", $"\"{Convert.ToBase64String(expectedItem.Version)}\"");
            controller.ControllerContext.HttpContext = httpContext;

            var response = await controller.ReadItemAsync(expectedItem.Id);

            // The correct status code is returned
            Assert.IsInstanceOfType(response, typeof(ObjectResult));
            var actual = response as ObjectResult;
            Assert.AreEqual(304, actual.StatusCode);

            // The repository is called
            Assert.IsTrue(repository.CallCount > 0);
            Assert.AreEqual("LookupAsync", repository.CallData);

            // The repository is not modified
            Assert.AreEqual(0, repository.Modifications);
        }

        [TestMethod]
        public async Task ReadItemAsync_Returns304_WhenLastModifiedInPast()
        {
            var repository = new MockTableRepository<Movie>();
            PopulateRepository(repository);
            var expectedItem = repository.Data["movie-5"].Clone();
            var controller = new MoviesController(repository, new TableControllerOptions { SoftDeleteEnabled = true });
            var httpContext = new DefaultHttpContext();
            AddHeaderToRequest(httpContext, "If-Modified-Since", expectedItem.UpdatedAt.AddDays(1).ToString("r"));
            controller.ControllerContext.HttpContext = httpContext;

            var response = await controller.ReadItemAsync(expectedItem.Id);

            // The correct status code is returned
            Assert.IsInstanceOfType(response, typeof(ObjectResult));
            var actual = response as ObjectResult;
            Assert.AreEqual(304, actual.StatusCode);

            // The repository is called
            Assert.IsTrue(repository.CallCount > 0);
            Assert.AreEqual("LookupAsync", repository.CallData);

            // The repository is not modified
            Assert.AreEqual(0, repository.Modifications);
        }

        [TestMethod]
        public async Task ReadItemAsync_Returns200_WhenLastModifiedInFuture()
        {
            var repository = new MockTableRepository<Movie>();
            PopulateRepository(repository);
            var expectedItem = repository.Data["movie-5"].Clone();
            var controller = new MoviesController(repository, new TableControllerOptions { SoftDeleteEnabled = true });
            var httpContext = new DefaultHttpContext();
            AddHeaderToRequest(httpContext, "If-Modified-Since", expectedItem.UpdatedAt.AddDays(-1).ToString("r"));
            controller.ControllerContext.HttpContext = httpContext;

            var response = await controller.ReadItemAsync(expectedItem.Id);

            // The correct status code is returned
            Assert.IsInstanceOfType(response, typeof(ObjectResult));
            var actual = response as ObjectResult;
            Assert.AreEqual(200, actual.StatusCode);

            // Response value is correct
            Assert.IsNotNull(actual.Value);
            var actualItem = actual.Value as Movie;
            Assert.IsInstanceOfType(actualItem, typeof(Movie));
            Assert.AreEqual(expectedItem, actualItem);

            // Version and UpdatedAt are correct
            Assert.IsNotNull(actualItem.Version);
            Assert.IsNotNull(actualItem.UpdatedAt);
            CollectionAssert.AreEqual(expectedItem.Version, actualItem.Version);
            Assert.AreEqual(expectedItem.UpdatedAt, actualItem.UpdatedAt);

            // ETag and Last-Modified headers are set
            HttpAssert.HasResponseHeader(httpContext, "ETag", $"\"{Convert.ToBase64String(actualItem.Version)}\"");
            HttpAssert.HasResponseHeader(httpContext, "Last-Modified", actualItem.UpdatedAt.ToString("r"));

            // The repository is called
            Assert.IsTrue(repository.CallCount > 0);
            Assert.AreEqual("LookupAsync", repository.CallData);

            // The repository is not modified
            Assert.AreEqual(0, repository.Modifications);
        }
        #endregion

        #region ReplaceItemAsync
        [TestMethod]
        public async Task ReplaceItemAsync_Returns403_WhenIsAuthorized_False()
        {
            var repository = new MockTableRepository<Movie>();
            PopulateRepository(repository);
            var expectedItem = repository.Data["movie-7"].Clone();
            var replacedItem = expectedItem.Clone(); replacedItem.Title = "Replaced";
            var controller = new MoviesController(repository)
            {
                IsAuthorizedResult = false
            };

            var actual = await controller.ReplaceItemAsync(replacedItem.Id, replacedItem);

            // The correct status code is returned
            Assert.IsInstanceOfType(actual, typeof(StatusCodeResult));
            var objectResult = actual as StatusCodeResult;
            Assert.AreEqual(403, objectResult.StatusCode);

            // IsAuthorized is called
            Assert.AreEqual(1, controller.IsAuthorizedCount);
            Assert.AreEqual(expectedItem.Id, controller.LastAuthorizedMovie.Id);

            // The repository is not modified
            Assert.AreEqual(0, repository.Modifications);
        }

        [TestMethod]
        public async Task ReplaceItemAsync_ReturnsValidateResult_WhenValidateOperations_Used()
        {
            var repository = new MockTableRepository<Movie>();
            PopulateRepository(repository);
            var expectedItem = repository.Data["movie-7"].Clone();
            var replacedItem = expectedItem.Clone(); replacedItem.Title = "Replaced";
            var controller = new MoviesController(repository)
            {
                ValidateOperationResult = 418
            };

            var actual = await controller.ReplaceItemAsync(replacedItem.Id, replacedItem);

            // The correct status code is returned
            Assert.IsInstanceOfType(actual, typeof(StatusCodeResult));
            var objectResult = actual as StatusCodeResult;
            Assert.AreEqual(418, objectResult.StatusCode);

            // ValidateOperation is called
            Assert.AreEqual(1, controller.ValidateOperationCount);
            Assert.AreEqual(expectedItem.Id, controller.LastValidateOperationMovie.Id);

            // The repository is not modified
            Assert.AreEqual(0, repository.Modifications);
        }

        [TestMethod]
        public async Task ReplaceItemAsync_ReturnsValidateResult_WhenValidateOperationsAsync_Used()
        {
            var repository = new MockTableRepository<Movie>();
            PopulateRepository(repository);
            var expectedItem = repository.Data["movie-7"].Clone();
            var replacedItem = expectedItem.Clone(); replacedItem.Title = "Replaced";
            var controller = new MoviesController(repository)
            {
                ValidateOperationAsyncResult = 418
            };

            var actual = await controller.ReplaceItemAsync(replacedItem.Id, replacedItem);

            // The correct status code is returned
            Assert.IsInstanceOfType(actual, typeof(StatusCodeResult));
            var objectResult = actual as StatusCodeResult;
            Assert.AreEqual(418, objectResult.StatusCode);

            // ValidateOperationAsync is called
            Assert.AreEqual(1, controller.ValidateOperationCount);
            Assert.AreEqual(expectedItem.Id, controller.LastValidateOperationMovie.Id);
            Assert.IsTrue(controller.WasLastValidateOperationAsync);

            // The repository is not modified
            Assert.AreEqual(0, repository.Modifications);
        }

        [TestMethod]
        public async Task ReplaceItemAsync_Returns200_WithValidReplacement()
        {
            var repository = new MockTableRepository<Movie>();
            PopulateRepository(repository);
            var expectedItem = repository.Data["movie-7"].Clone();
            var replacedItem = expectedItem.Clone(); replacedItem.Title = "Replaced";
            var controller = new MoviesController(repository);
            var httpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext = httpContext;

            var response = await controller.ReplaceItemAsync(replacedItem.Id, replacedItem);

            // The correct status code is returned
            Assert.IsInstanceOfType(response, typeof(ObjectResult));
            var actual = response as ObjectResult;
            Assert.AreEqual(200, actual.StatusCode);

            // Response value is correct
            Assert.IsNotNull(actual.Value);
            var actualItem = actual.Value as Movie;
            Assert.IsInstanceOfType(actualItem, typeof(Movie));
            Assert.AreEqual(replacedItem, actualItem);

            // Version and UpdatedAt are correct
            Assert.IsNotNull(actualItem.Version);
            Assert.IsNotNull(actualItem.UpdatedAt);
            CollectionAssert.AreNotEqual(expectedItem.Version, actualItem.Version);
            TimestampAssert.AreClose(DateTimeOffset.UtcNow, actualItem.UpdatedAt);

            // ETag and Last-Modified headers are set
            HttpAssert.HasResponseHeader(httpContext, "ETag", $"\"{Convert.ToBase64String(actualItem.Version)}\"");
            HttpAssert.HasResponseHeader(httpContext, "Last-Modified", actualItem.UpdatedAt.ToString("r"));

            // The repository is called
            Assert.IsTrue(repository.CallCount > 0);
            Assert.AreEqual("ReplaceAsync", repository.CallData);

            // The repository is modified
            Assert.AreEqual(1, repository.Modifications);
        }

        [TestMethod]
        public async Task ReplaceItemAsync_Returns404_ReplacingMissingItem()
        {
            var repository = new MockTableRepository<Movie>();
            PopulateRepository(repository);
            var expectedItem = repository.Data["movie-7"].Clone();
            var replacedItem = expectedItem.Clone();
            replacedItem.Id = "not-present";
            replacedItem.Title = "Replaced";
            var controller = new MoviesController(repository);
            var httpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext = httpContext;

            var response = await controller.ReplaceItemAsync(replacedItem.Id, replacedItem);

            // The correct status code is returned
            Assert.IsInstanceOfType(response, typeof(StatusCodeResult));
            var actual = response as StatusCodeResult;
            Assert.AreEqual(404, actual.StatusCode);

            // The repository is called (so this is an actual lookup in the repository)
            Assert.IsTrue(repository.CallCount > 0);

            // The repository is not modified
            Assert.AreEqual(0, repository.Modifications);
        }

        [TestMethod]
        public async Task ReplaceItemAsync_Returns400_WhenMismatchedIds()
        {
            var repository = new MockTableRepository<Movie>();
            PopulateRepository(repository);
            var expectedItem = repository.Data["movie-7"].Clone();
            var replacedItem = expectedItem.Clone();
            replacedItem.Title = "Replaced";
            var controller = new MoviesController(repository);
            var httpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext = httpContext;

            var response = await controller.ReplaceItemAsync("movie-5", replacedItem);

            // The correct status code is returned
            Assert.IsInstanceOfType(response, typeof(StatusCodeResult));
            var actual = response as StatusCodeResult;
            Assert.AreEqual(400, actual.StatusCode);

            // The repository is not modified
            Assert.AreEqual(0, repository.Modifications);
        }

        [TestMethod]
        public async Task ReplaceItemAsync_WithSoftDelete_Returns404_WhenReplacingDeletedItem()
        {
            var repository = new MockTableRepository<Movie>();
            PopulateRepository(repository, true);
            var expectedItem = repository.Data.Values.Where(m => m.Deleted).First().Clone();
            var replacedItem = expectedItem.Clone(); replacedItem.Title = "Replaced";
            var controller = new MoviesController(repository, new TableControllerOptions { SoftDeleteEnabled = true });
            var httpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext = httpContext;

            var response = await controller.ReplaceItemAsync(replacedItem.Id, replacedItem);

            // The correct status code is returned
            Assert.IsInstanceOfType(response, typeof(StatusCodeResult));
            var actual = response as StatusCodeResult;
            Assert.AreEqual(404, actual.StatusCode);

            // The repository is called (so this is an actual lookup in the repository)
            Assert.IsTrue(repository.CallCount > 0);

            // The repository is not modified
            Assert.AreEqual(0, repository.Modifications);
        }

        [TestMethod]
        public async Task ReplaceItemAsync_Returns412_WhenETagDoesNotMatch()
        {
            var repository = new MockTableRepository<Movie>();
            PopulateRepository(repository);
            var expectedItem = repository.Data["movie-9"].Clone();
            var replacedItem = expectedItem.Clone(); replacedItem.Title = "Replaced";
            var controller = new MoviesController(repository);
            var httpContext = new DefaultHttpContext();
            AddHeaderToRequest(httpContext, "If-Match", $"\"{Guid.NewGuid()}\"");
            controller.ControllerContext.HttpContext = httpContext;

            var response = await controller.ReplaceItemAsync(replacedItem.Id, replacedItem);

            Assert.IsInstanceOfType(response, typeof(ObjectResult));
            var actual = response as ObjectResult;

            // Status Code is correct
            Assert.AreEqual(412, actual.StatusCode);

            // Response value is correct
            Assert.IsNotNull(actual.Value);
            var actualItem = actual.Value as Movie;
            Assert.IsInstanceOfType(actualItem, typeof(Movie));
            Assert.AreEqual(expectedItem, actualItem);

            // Version and UpdatedAt are correct
            Assert.IsNotNull(actualItem.Version);
            Assert.IsNotNull(actualItem.UpdatedAt);
            CollectionAssert.AreEqual(expectedItem.Version, actualItem.Version);
            Assert.AreEqual(expectedItem.UpdatedAt, actualItem.UpdatedAt);

            // ETag and Last-Modified headers are set
            HttpAssert.HasResponseHeader(httpContext, "ETag", $"\"{Convert.ToBase64String(actualItem.Version)}\"");
            HttpAssert.HasResponseHeader(httpContext, "Last-Modified", actualItem.UpdatedAt.ToString("r"));

            // The repository was NOT called
            Assert.AreEqual(0, repository.Modifications);
        }

        [TestMethod]
        public async Task ReplaceItemAsync_Returns200_WhenETagMatches()
        {
            var repository = new MockTableRepository<Movie>();
            PopulateRepository(repository);
            var expectedItem = repository.Data["movie-7"].Clone();
            var replacedItem = expectedItem.Clone(); replacedItem.Title = "Replaced";
            var controller = new MoviesController(repository);
            var httpContext = new DefaultHttpContext();
            AddHeaderToRequest(httpContext, "If-Match", $"\"{Convert.ToBase64String(replacedItem.Version)}\"");
            controller.ControllerContext.HttpContext = httpContext;

            var response = await controller.ReplaceItemAsync(replacedItem.Id, replacedItem);

            // The correct status code is returned
            Assert.IsInstanceOfType(response, typeof(ObjectResult));
            var actual = response as ObjectResult;
            Assert.AreEqual(200, actual.StatusCode);

            // Response value is correct
            Assert.IsNotNull(actual.Value);
            var actualItem = actual.Value as Movie;
            Assert.IsInstanceOfType(actualItem, typeof(Movie));
            Assert.AreEqual(replacedItem, actualItem);

            // Version and UpdatedAt are correct
            Assert.IsNotNull(actualItem.Version);
            Assert.IsNotNull(actualItem.UpdatedAt);
            CollectionAssert.AreNotEqual(expectedItem.Version, actualItem.Version);
            TimestampAssert.AreClose(DateTimeOffset.UtcNow, actualItem.UpdatedAt);

            // ETag and Last-Modified headers are set
            HttpAssert.HasResponseHeader(httpContext, "ETag", $"\"{Convert.ToBase64String(actualItem.Version)}\"");
            HttpAssert.HasResponseHeader(httpContext, "Last-Modified", actualItem.UpdatedAt.ToString("r"));

            // The repository is called
            Assert.IsTrue(repository.CallCount > 0);
            Assert.AreEqual("ReplaceAsync", repository.CallData);

            // The repository is modified
            Assert.AreEqual(1, repository.Modifications);
        }
        #endregion

        [TestMethod]
        public void AddHeadersToResponse_DoesNotAddETag_WhenVersionMissing()
        {
            var controller = new MoviesController(new MockTableRepository<Movie>());
            controller.ControllerContext.HttpContext = new DefaultHttpContext();

            var entity = new Movie()
            {
                BestPictureWinner = false,
                Duration = 60,
                MpaaRating = "G",
                ReleaseDate = new DateTime(2020, 12, 24),
                Title = "Home Movie",
                Year = 2020,
                Version = null
            };

            controller.AddHeadersToResponse(entity);

            Assert.IsFalse(controller.Response.Headers.ContainsKey("ETag"));
            Assert.IsTrue(controller.Response.Headers.ContainsKey("Last-Modified"));
        }

        [TestMethod]
        public void AddHeadersToResponse_DoesNotAddETag_WhenVersionEmpty()
        {
            var controller = new MoviesController(new MockTableRepository<Movie>());
            controller.ControllerContext.HttpContext = new DefaultHttpContext();

            var entity = new Movie()
            {
                BestPictureWinner = false,
                Duration = 60,
                MpaaRating = "G",
                ReleaseDate = new DateTime(2020, 12, 24),
                Title = "Home Movie",
                Year = 2020,
                Version = new byte[] {}
            };

            controller.AddHeadersToResponse(entity);

            Assert.IsFalse(controller.Response.Headers.ContainsKey("ETag"));
            Assert.IsTrue(controller.Response.Headers.ContainsKey("Last-Modified"));
        }
    }
}
