// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.VisualBasic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Zumo.Server.Entity.Test.Helpers;
using Microsoft.Zumo.Server.Exceptions;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Zumo.Server.Entity.Test
{
    /// <summary>
    /// Test Suite for the EntityTableRepository
    /// </summary>
    [TestClass]
    public class EntityTableRepository_Tests : BaseTest
    {
        #region Constructor
        [TestMethod]
        public void EntityTableRepository_CanCreate_WithContext()
        {
            var repository = GetTableRepository();
            Assert.IsNotNull(repository);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void EntityTableRepository_Throws_WithNullContext()
        {
            _ = new EntityTableRepository<Movie>(null);
            Assert.Fail("ArgumentNullException expected");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void EntityTableRepository_Throws_WithMissingSet()
        {
            var context = GetMovieContext();
            _ = new EntityTableRepository<ErrorEntity>(context);
            Assert.Fail("ArgumentException expected");
        }
        #endregion

        #region AsQueryable()
        [TestMethod]
        public void AsQueryable_Returns_IQueryable()
        {
            var repository = GetTableRepository();
            var actual = repository.AsQueryable();

            Assert.IsInstanceOfType(actual, typeof(IQueryable<Movie>));
        }

        [TestMethod]
        public void AsQueryable_CanCount()
        {
            var repository = GetTableRepository();
            var actual = repository.AsQueryable().Count();

            Assert.AreEqual(MOVIE_COUNT, actual);
        }

        [TestMethod]
        public void AsQueryable_CanFilter()
        {
            var repository = GetTableRepository();
            var actual = repository.AsQueryable().Where(m => m.MpaaRating == "R").Count();

            Assert.AreEqual(R_MOVIE_COUNT, actual);
        }
        #endregion

        #region CreateAsync
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task CreateAsync_Throws_NullEntity()
        {
            var repository = GetTableRepository();
            _ = await repository.CreateAsync(null);
            Assert.Fail("ArgumentNullException expected");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task CreateAsync_Throws_EmptyIdInEntity()
        {
            var repository = GetTableRepository();
            var item = new Movie { Id = "" };
            _ = await repository.CreateAsync(item);
            Assert.Fail("ArgumentNullException expected");
        }

        [TestMethod]
        [ExpectedException(typeof(EntityExistsException))]
        public async Task CreateAsync_Throws_EntityExists()
        {
            var repository = GetTableRepository();
            var item = new Movie { Id = "movie-5" };
            _ = await repository.CreateAsync(item);
            Assert.Fail("EntityExistsException expected");
        }

        [TestMethod]
        public async Task CreateAsync_CreatesItem_WithNullId()
        {
            var repository = GetTableRepository();
            var item = new Movie
            {
                Id = null,
                BestPictureWinner = false,
                Duration = 50,
                MpaaRating = "G",
                Title = "Home Movie Magic",
                Year = 2020,
                ReleaseDate = new DateTime(2020, 12, 24)
            };
            var updatedItem = await repository.CreateAsync(item);
            Assert.IsNotNull(updatedItem);
            Assert.IsNotNull(updatedItem.Id);
            Assert.IsFalse(updatedItem.Deleted);
            Assert.IsNotNull(updatedItem.CreatedAt);

            var isFound = repository.AsQueryable().Any(m => m.Id == updatedItem.Id);
            Assert.IsTrue(isFound);
        }

        [TestMethod]
        public async Task CreateAsync_CreatesItem_WhenValid()
        {
            var repository = GetTableRepository();
            var item = new Movie
            {
                Id = Guid.NewGuid().ToString("N"),
                BestPictureWinner = false,
                Duration = 50,
                MpaaRating = "G",
                Title = "Home Movie Magic",
                Year = 2020,
                ReleaseDate = new DateTime(2020, 12, 24)
            };
            var updatedItem = await repository.CreateAsync(item);
            Assert.IsNotNull(updatedItem);
            Assert.IsFalse(updatedItem.Deleted);
            Assert.IsNotNull(updatedItem.CreatedAt);

            var isFound = repository.AsQueryable().Any(m => m.Id == item.Id);
            Assert.IsTrue(isFound);
        }

        [TestMethod]
        public async Task CreateAsync_CreatesItem_WithCreatedAtDate()
        {
            var repository = GetTableRepository();
            var item = new Movie
            {
                Id = Guid.NewGuid().ToString("N"),
                BestPictureWinner = false,
                Duration = 50,
                MpaaRating = "G",
                Title = "Home Movie Magic",
                Year = 2020,
                ReleaseDate = new DateTime(2020, 12, 24),
                CreatedAt = new DateTimeOffset(2020, 12, 31, 08, 00, 00, TimeSpan.Zero)
            };
            var updatedItem = await repository.CreateAsync(item);
            Assert.IsNotNull(updatedItem);
            Assert.IsFalse(updatedItem.Deleted);
            Assert.AreEqual(item.CreatedAt, updatedItem.CreatedAt);

            var isFound = repository.AsQueryable().Any(m => m.Id == item.Id);
            Assert.IsTrue(isFound);
        }

        [TestMethod]
        public async Task CreateAsync_UpdatesVersion_WhenValid()
        {
            var repository = GetTableRepository();
            var version = Guid.NewGuid().ToByteArray();
            var item = new Movie
            {
                Id = Guid.NewGuid().ToString("N"),
                UpdatedAt = DateTimeOffset.Now.AddDays(-20),
                Version = version,
                BestPictureWinner = false,
                Duration = 50,
                MpaaRating = "G",
                Title = "Home Movie Magic",
                Year = 2020,
                ReleaseDate = new DateTime(2020, 12, 24)
            };
            var updatedItem = await repository.CreateAsync(item);
            Assert.IsNotNull(updatedItem);
            CollectionAssert.AreNotEqual(version, updatedItem.Version);
        }

        [TestMethod]
        public async Task CreateAsync_UpdatesTimestamp_WhenValid()
        {
            var repository = GetTableRepository();
            var version = Guid.NewGuid().ToByteArray();
            var item = new Movie
            {
                Id = Guid.NewGuid().ToString("N"),
                UpdatedAt = DateTimeOffset.Now.AddDays(-20),
                Version = version,
                BestPictureWinner = false,
                Duration = 50,
                MpaaRating = "G",
                Title = "Home Movie Magic",
                Year = 2020,
                ReleaseDate = new DateTime(2020, 12, 24)
            };
            var updatedItem = await repository.CreateAsync(item);
            Assert.IsNotNull(updatedItem);
            TimestampAssert.AreClose(DateTimeOffset.UtcNow, updatedItem.UpdatedAt);
        }

        [TestMethod]
        public async Task CreateAsync_CreatesVersion_WhenNotPresent()
        {
            var repository = GetTableRepository();
            var item = new Movie
            {
                Id = Guid.NewGuid().ToString("N"),
                BestPictureWinner = false,
                Duration = 50,
                MpaaRating = "G",
                Title = "Home Movie Magic",
                Year = 2020,
                ReleaseDate = new DateTime(2020, 12, 24)
            };
            var updatedItem = await repository.CreateAsync(item);
            Assert.IsNotNull(updatedItem);
            Assert.IsNotNull(updatedItem.Version);
            Assert.IsTrue(updatedItem.Version.Length > 0);
        }

        [TestMethod]
        public async Task CreateAsync_CreatesTimestamp_WhenNotPresent()
        {
            var repository = GetTableRepository();
            var item = new Movie
            {
                Id = Guid.NewGuid().ToString("N"),
                BestPictureWinner = false,
                Duration = 50,
                MpaaRating = "G",
                Title = "Home Movie Magic",
                Year = 2020,
                ReleaseDate = new DateTime(2020, 12, 24)
            };
            var updatedItem = await repository.CreateAsync(item);
            Assert.IsNotNull(updatedItem);
            TimestampAssert.AreClose(DateTimeOffset.UtcNow, updatedItem.UpdatedAt);
        }
        #endregion

        #region DeleteAsync
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task DeleteAsync_Throws_NullId()
        {
            var repository = GetTableRepository();
            await repository.DeleteAsync(null);
            Assert.Fail("ArgumentNullException expected");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task DeleteAsync_Throws_EmptyId()
        {
            var repository = GetTableRepository();
            await repository.DeleteAsync("");
            Assert.Fail("ArgumentNullException expected");
        }

        [TestMethod]
        [ExpectedException(typeof(EntityDoesNotExistException))]
        public async Task DeleteAsync_Throws_EntityDoesNotExist()
        {
            var repository = GetTableRepository();
            await repository.DeleteAsync("does-not-exist");
            Assert.Fail("EntityDoesNotExist expected");
        }

        [TestMethod]
        public async Task DeleteAsync_RemovesEntity_WhenValid()
        {
            var repository = GetTableRepository();
            var id = "movie-10";
            await repository.DeleteAsync(id);
            var isPresent = repository.AsQueryable().Any(m => m.Id == id);
            Assert.IsFalse(isPresent);
        }
        #endregion

        #region LookupAsync
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task LookupAsync_Throws_NullId()
        {
            var repository = GetTableRepository();
            _ = await repository.LookupAsync(null);
            Assert.Fail("ArgumentNullException expected");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task LookupAsync_Throws_EmptyId()
        {
            var repository = GetTableRepository();
            _ = await repository.LookupAsync("");
            Assert.Fail("ArgumentNullException expected");
        }

        [TestMethod]
        public async Task LookupAsync_ReturnsNull_MissingId()
        {
            var repository = GetTableRepository();
            var actual = await repository.LookupAsync(Guid.NewGuid().ToString("N"));
            Assert.IsNull(actual);
        }

        [TestMethod]
        public async Task LookupAsync_ReturnsEntity_PresentId()
        {
            var repository = GetTableRepository();
            
            for (var i = 0; i < MOVIE_COUNT; i++)
            {
                var id = $"movie-{i}";
                var actual = await repository.LookupAsync(id);
                var expected = new Movie()
                {
                    Id = $"movie-{i}",
                    Title = TestData.Movies[i].Title,
                    Duration = TestData.Movies[i].Duration,
                    MpaaRating = TestData.Movies[i].MpaaRating,
                    ReleaseDate = TestData.Movies[i].ReleaseDate,
                    BestPictureWinner = TestData.Movies[i].BestPictureWinner,
                    Year = TestData.Movies[i].Year
                };

                Assert.AreEqual(expected, actual);
            }
        }
        #endregion

        #region ReplaceAsync
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ReplaceAsync_Throws_NullEntity()
        {
            var repository = GetTableRepository();
            _ = await repository.ReplaceAsync(null);
            Assert.Fail("ArgumentNullException expected");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ReplaceAsync_Throws_NullIdInEntity()
        {
            var repository = GetTableRepository();
            var item = new Movie { Id = null };
            _ = await repository.ReplaceAsync(item);
            Assert.Fail("ArgumentNullException expected");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ReplaceAsync_Throws_EmptyIdInEntit()
        {
            var repository = GetTableRepository();
            var item = new Movie { Id = "" };
            _ = await repository.ReplaceAsync(item);
            Assert.Fail("ArgumentNullException expected");
        }

        [TestMethod]
        [ExpectedException(typeof(EntityDoesNotExistException))]
        public async Task ReplaceAsync_Throws_EntityMissing()
        {
            var repository = GetTableRepository();
            var item = new Movie { Id = "does-not-exist" };
            _ = await repository.ReplaceAsync(item);
            Assert.Fail("EntityDoesNotExist expected");
        }

        [TestMethod]
        public async Task ReplaceAsync_UpdatesItem_WhenValid()
        {
            var repository = GetTableRepository();
            var item = repository.AsQueryable().Where(t => t.Id == "movie-5").First();
            item.Title = "Updated";
            var updatedItem = await repository.ReplaceAsync(item);
            Assert.IsNotNull(updatedItem);
            Assert.AreEqual("Updated", item.Title);
        }

        [TestMethod]
        public async Task ReplaceAsync_UpdatesVersion_WhenValid()
        {
            var repository = GetTableRepository();
            var item = repository.AsQueryable().Where(t => t.Id == "movie-5").First();
            var version = (byte[])item.Version.Clone();
            item.Title = "Updated";
            var updatedItem = await repository.ReplaceAsync(item);
            Assert.IsNotNull(updatedItem);
            CollectionAssert.AreNotEqual(version, updatedItem.Version);
        }

        [TestMethod]
        public async Task ReplaceAsync_UpdatesTimestamp_WhenValid()
        {
            var repository = GetTableRepository();
            var item = repository.AsQueryable().Where(t => t.Id == "movie-5").First();
            item.Title = "Updated";
            var updatedItem = await repository.ReplaceAsync(item);
            Assert.IsNotNull(updatedItem);
            TimestampAssert.AreClose(DateTimeOffset.UtcNow, updatedItem.UpdatedAt);
        }
        #endregion
    }
}
