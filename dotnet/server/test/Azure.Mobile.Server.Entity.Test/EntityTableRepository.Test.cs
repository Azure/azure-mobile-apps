using Azure.Mobile.Common.Test;
using Azure.Mobile.Server.Exceptions;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Azure.Mobile.Server.Entity.Test
{
    [TestClass]
    public class EntityTableRepository_Tests
    {
        [TestMethod]
        public void AsQueryable_CanCountItems()
        {
            var dbcontext = MovieDbContext.InMemoryContext();
            var repository = new EntityTableRepository<Movie>(dbcontext);

            var actual = repository.AsQueryable().Count();

            Assert.AreEqual(TestData.TestMovies.Length, actual);
        }

        [TestMethod]
        public async Task LookupAsync_ReturnsValidData()
        {
            var dbcontext = MovieDbContext.InMemoryContext();
            var repository = new EntityTableRepository<Movie>(dbcontext);
            var testItem = TestData.RandomMovie();

            var actual = await repository.LookupAsync(testItem.Id);
            Assert.IsNotNull(actual);
            Assert.AreEqual(testItem.Title, actual.Title);
        }

        [TestMethod]
        public async Task LookupAsync_ReturnsNullOnMissingData()
        {
            var dbcontext = MovieDbContext.InMemoryContext();
            var repository = new EntityTableRepository<Movie>(dbcontext);
            var testId = "random-invalid-id";

            var actual = await repository.LookupAsync(testId);
            Assert.IsNull(actual);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task LookupAsync_ThrowsOnNullId()
        {
            var dbcontext = MovieDbContext.InMemoryContext();
            var repository = new EntityTableRepository<Movie>(dbcontext);

            var actual = await repository.LookupAsync(null);
            Assert.Fail("ArgumentNullException expected");
        }

        [TestMethod]
        public async Task DeleteAsync_DeletesValidData()
        {
            var dbcontext = MovieDbContext.InMemoryContext();
            var repository = new EntityTableRepository<Movie>(dbcontext);
            var testItem = TestData.RandomMovie();

            await repository.DeleteAsync(testItem.Id);
            var actual = repository.AsQueryable().Count();
            Assert.AreEqual(TestData.TestMovies.Length - 1, actual);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task DeleteAsync_ThrowsOnNullId()
        {
            var dbcontext = MovieDbContext.InMemoryContext();
            var repository = new EntityTableRepository<Movie>(dbcontext);

            await repository.DeleteAsync(null);
            Assert.Fail("ArgumentNullException expected");
        }

        [TestMethod]
        [ExpectedException(typeof(EntityDoesNotExistException))]
        public async Task DeleteAsync_ThrowsOnMissingData()
        {
            var dbcontext = MovieDbContext.InMemoryContext();
            var repository = new EntityTableRepository<Movie>(dbcontext);
            var testId = "random-invalid-id";

            await repository.DeleteAsync(testId);
            Assert.Fail("EntityDoesNotExistException expected");
        }

        [TestMethod]
        public async Task CreateAsync_CreatesNewItem()
        {
            var dbcontext = MovieDbContext.InMemoryContext();
            var dataset = dbcontext.Set<Movie>();
            var repository = new EntityTableRepository<Movie>(dbcontext);
            var newItem = new Movie()
            {
                Title = "Test Item"
            };

            var actual = await repository.CreateAsync(newItem);
            Assert.IsNotNull(actual);
            Assert.AreEqual(32, actual.Id.Length);
            Assert.AreEqual(1, dataset.Count(m => m.Id == actual.Id));
            Assert.IsTrue(DateTimeOffset.UtcNow.Subtract(actual.UpdatedAt).TotalMilliseconds < 500);
            Assert.IsNotNull(actual.Version);
            Assert.AreEqual("Test Item", actual.Title);
        }

        [TestMethod]
        [ExpectedException(typeof(EntityExistsException))]
        public async Task CreateAsync_Duplicate_Throws()
        {
            var dbcontext = MovieDbContext.InMemoryContext();
            var repository = new EntityTableRepository<Movie>(dbcontext);
            var testItem = TestData.RandomMovie();

            var actual = await repository.CreateAsync(testItem);
            Assert.Fail("EntityExistsException expected");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task CreateAsync_ThrowsOnNull()
        {
            var dbcontext = MovieDbContext.InMemoryContext();
            var repository = new EntityTableRepository<Movie>(dbcontext);

            await repository.CreateAsync(null);
            Assert.Fail("ArgumentNullException expected");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task CreateAsync_ThrowsOnNullId()
        {
            var dbcontext = MovieDbContext.InMemoryContext();
            var repository = new EntityTableRepository<Movie>(dbcontext);
            var testItem = new Movie() { Id = null };

            await repository.CreateAsync(testItem);
            Assert.Fail("ArgumentNullException expected");
        }

        [TestMethod]
        public async Task ReplaceAsync_ReplacesExistingItem()
        {
            var dbcontext = MovieDbContext.InMemoryContext();
            var repository = new EntityTableRepository<Movie>(dbcontext);
            var original = TestData.RandomMovie();
            original.Title = "Test Data";

            var actual = await repository.ReplaceAsync(original);
            Assert.IsNotNull(actual);
            Assert.AreEqual(original.Id, actual.Id);
            Assert.IsTrue(DateTimeOffset.UtcNow.Subtract(actual.UpdatedAt).TotalMilliseconds < 500);
            CollectionAssert.AreEqual(original.Version, actual.Version);
            Assert.AreEqual("Test Data", actual.Title);
            Assert.AreEqual(original.ReleaseDate, actual.ReleaseDate);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ReplaceAsync_ThrowsOnNull()
        {
            var dbcontext = MovieDbContext.InMemoryContext();
            var repository = new EntityTableRepository<Movie>(dbcontext);

            await repository.ReplaceAsync(null);
            Assert.Fail("ArgumentNullException expected");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ReplaceAsync_ThrowsOnNullId()
        {
            var dbcontext = MovieDbContext.InMemoryContext();
            var repository = new EntityTableRepository<Movie>(dbcontext);
            var item = new Movie() { Id = null };

            await repository.ReplaceAsync(item);
            Assert.Fail("ArgumentNullException expected");
        }

        [TestMethod]
        [ExpectedException(typeof(EntityDoesNotExistException))]
        public async Task ReplaceAsync_ThrowsOnMissingEntity()
        {
            var dbcontext = MovieDbContext.InMemoryContext();
            var repository = new EntityTableRepository<Movie>(dbcontext);
            var item = new Movie();

            await repository.ReplaceAsync(item);
            Assert.Fail("EntityDoesNotExistException expected");
        }
    }
}
