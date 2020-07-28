using Azure.Mobile.Server.Entity;
using Azure.Mobile.Server.Test.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Azure.Mobile.Server.Test
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
            var controller = new TestTableController();
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
    }
}
