// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Zumo.Server.Entity;
using Microsoft.Zumo.Server.Test.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Zumo.Server.Test
{
    /// <summary>
    /// Most of the TableController tests are in the TableController folder,
    /// and separated out by operation.  However, certain tests require more
    /// control over the CTOR, so they are done separately.
    /// </summary>
    [TestClass]
    public class TableController_Tests
    {
        class TestTableController : TableController<Movie>
        {
            public TestTableController() : base(null)
            {
            }
        }

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
            _ = new TestTableController();
            Assert.Fail("ArgumentNullException expected");
        }

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
            var controller = new MoviesController
            {
                TableRepository = new EntityTableRepository<Movie>(context)
            };
            Assert.IsNotNull(controller.TableRepository);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TableRepository_ThrowsIfSetTwice()
        {
            var context = MovieDbContext.InMemoryContext();
            _ = new MoviesController(context)
            {
                TableRepository = new EntityTableRepository<Movie>(context)
            };
            Assert.Fail("InvalidOperationException expected");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TableRepository_ThrowsIfSetNull()
        {
            var context = MovieDbContext.InMemoryContext();
            _ = new MoviesController()
            {
                TableRepository = null
            };
            Assert.Fail("ArgumentNullException expected");
        }

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
            var testItem = new Movie() { Title = "foo" };
            Assert.AreEqual(testItem, controller.BasePrepareItemForStore(testItem));
        }

        [TestMethod]
        public void ValidateOperations_Returns200()
        {
            var context = MovieDbContext.InMemoryContext();
            var controller = new MoviesController(context);
            var actual = controller.ValidateOperation(TableOperation.None, null);
            Assert.AreEqual(200, actual);
        }

        [TestMethod]
        public async Task ValidateOperationsAsync_Returns200()
        {
            var context = MovieDbContext.InMemoryContext();
            var controller = new MoviesController(context);
            var actual = await controller.ValidateOperationAsync(TableOperation.None, null);
            Assert.AreEqual(200, actual);
        }
        
        [TestMethod]
        public async Task CreateItemAsync_ReturnsValidateOperationsResult()
        {
            var context = MovieDbContext.InMemoryContext();
            var controller = new ValidationController(context);
            var movie = new Movie() { Title = "foo" };
            var actual = await controller.CreateItemAsync(movie);
            Assert.IsInstanceOfType(actual, typeof(StatusCodeResult));
            var result = actual as StatusCodeResult;
            Assert.AreEqual(418, result.StatusCode);
            Assert.AreEqual(TableOperation.Create, controller.LastValidationOperation);
            Assert.AreEqual("foo", controller.LastValidationItem.Title);
        }

        [TestMethod]
        public async Task DeleteItemAsync_ReturnsValidateOperationsResult()
        {
            var context = MovieDbContext.InMemoryContext();
            var controller = new ValidationController(context);
            var movie = context.Movies.First().Clone();
            var actual = await controller.DeleteItemAsync(movie.Id);
            Assert.IsInstanceOfType(actual, typeof(StatusCodeResult));
            var result = actual as StatusCodeResult;
            Assert.AreEqual(418, result.StatusCode);
            Assert.AreEqual(TableOperation.Delete, controller.LastValidationOperation);
            Assert.AreEqual(movie.Title, controller.LastValidationItem.Title);
        }

        [TestMethod]
        public async Task GetItemAsync_ReturnsValidateOperationsResult()
        {
            var context = MovieDbContext.InMemoryContext();
            var controller = new ValidationController(context);
            var movie = context.Movies.First().Clone();
            var actual = await controller.ReadItemAsync(movie.Id);
            Assert.IsInstanceOfType(actual, typeof(StatusCodeResult));
            var result = actual as StatusCodeResult;
            Assert.AreEqual(418, result.StatusCode);
            Assert.AreEqual(TableOperation.Read, controller.LastValidationOperation);
            Assert.AreEqual(movie.Title, controller.LastValidationItem.Title);
        }

        [TestMethod]
        public async Task ListItemAsync_ReturnsValidateOperationsResult()
        {
            var context = MovieDbContext.InMemoryContext();
            var controller = new ValidationController(context);
            var actual = await controller.GetItems();
            Assert.IsInstanceOfType(actual, typeof(StatusCodeResult));
            var result = actual as StatusCodeResult;
            Assert.AreEqual(418, result.StatusCode);
            Assert.AreEqual(TableOperation.List, controller.LastValidationOperation);
        }

        [TestMethod]
        public async Task ReplaceItemAsync_ReturnsValidateOperationsResult()
        {
            var context = MovieDbContext.InMemoryContext();
            var controller = new ValidationController(context);
            var movie = context.Movies.First().Clone();
            var replacement = movie.Clone(); replacement.Title = "foo";
            var actual = await controller.ReplaceItemAsync(replacement.Id, replacement);
            Assert.IsInstanceOfType(actual, typeof(StatusCodeResult));
            var result = actual as StatusCodeResult;
            Assert.AreEqual(418, result.StatusCode);
            Assert.AreEqual(TableOperation.Replace, controller.LastValidationOperation);
            Assert.AreEqual("foo", controller.LastValidationItem.Title);
        }
    }
}
