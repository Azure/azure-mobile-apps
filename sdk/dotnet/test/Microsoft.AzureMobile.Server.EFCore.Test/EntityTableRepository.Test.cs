// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AzureMobile.Common.Test;
using Microsoft.AzureMobile.Common.Test.Models;
using Microsoft.AzureMobile.Common.Test.TestData;
using Microsoft.AzureMobile.Server.EFCore.Test.Helpers;
using Microsoft.AzureMobile.Server.Exceptions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Microsoft.AzureMobile.Server.EFCore.Test
{
    [SuppressMessage("Design", "RCS1090:Add call to 'ConfigureAwait' (or vice versa).", Justification = "Test suite")]
    [ExcludeFromCodeCoverage(Justification = "Test suite")]
    public class EntityTableRepository_Tests
    {
        #region Test Artifacts
        /// <summary>
        /// A basic movie without any adornment that does not exist in the movie data. Tests must clone this
        /// object and then adjust.
        /// </summary>
        private readonly EntityMovie blackPantherMovie = new()
        {
            BestPictureWinner = true,
            Duration = 134,
            Rating = "PG-13",
            ReleaseDate = DateTimeOffset.Parse("16-Feb-2018"),
            Title = "Black Panther",
            Year = 2018
        };

        /// <summary>
        /// Converts an index to an id.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        private static string GetId(int index) => string.Format("id-{0:000}", index);

        /// <summary>
        /// Get a seeded MovieDbContext to use.
        /// </summary>
        /// <returns></returns>
        private static MovieDbContext GetMovieContext()
        {
            var connection = new SqliteConnection("Data Source=:memory:");
            connection.Open();
            var options = new DbContextOptionsBuilder<MovieDbContext>().UseSqlite(connection).Options;
            var context = new MovieDbContext(options) { Connection = connection };

            // Create the database
            context.Database.EnsureCreated();
            context.InstallTriggers();

            // Populate with test data
            var seedData = Movies.OfType<EntityMovie>();
            seedData.ForEach(movie =>
            {
                var offset = -(180 + new Random().Next(180));
                movie.Version = Guid.NewGuid().ToByteArray();
                movie.UpdatedAt = DateTimeOffset.UtcNow.AddDays(offset);
            });
            context.Movies.AddRange(seedData);
            context.SaveChanges();
            return context;
        }

        /// <summary>
        /// Get a reference to a fully seeded repository
        /// </summary>
        /// <returns></returns>
        private static EntityTableRepository<EntityMovie> GetTestRepository() => new(GetMovieContext());
        #endregion

        #region Constructor
        [Fact]
        public void EntityTableRepository_CanCreate_WithContext()
        {
            // Act
            var repository = GetTestRepository();

            // Assert
            Assert.NotNull(repository);
        }

        [Fact]
        public void EntityTableRepository_Throws_WithNullContext()
        {
            Assert.Throws<ArgumentNullException>(() => new EntityTableRepository<EntityMovie>(null));
        }

        [Fact]
        public void EntityTableRepository_Throws_WithMissingSet()
        {
            // Arrange
            var context = GetMovieContext();

            // Assert
            Assert.Throws<ArgumentException>(() => new EntityTableRepository<ErrorEntity>(context));
        }
        #endregion

        #region AsQueryable()
        [Fact]
        public void AsQueryable_Returns_IQueryable()
        {
            // Arrange
            var repository = GetTestRepository();

            // Act
            var actual = repository.AsQueryable();

            // Assert
            Assert.IsAssignableFrom<IQueryable<EntityMovie>>(actual);
            Assert.Equal(Movies.Count, actual.Count());
        }
        #endregion

        #region CreateAsync
        [Fact]
        public async Task CreateAsync_Throws_OnNullEntity()
        {
            // Arrange
            var repository = GetTestRepository();

            // Act
            var ex = await Assert.ThrowsAsync<ArgumentNullException>(() => repository.CreateAsync(null));

            // Assert
            Assert.Equal("entity", ex.ParamName);
        }

        [Fact]
        public async Task CreateAsync_CreatesNewEntity_WithSpecifiedId()
        {
            //Arrange
            var repository = GetTestRepository();
            var item = blackPantherMovie.Clone();
            item.Id = "movie-blackpanther";

            //Act
            await repository.CreateAsync(item);

            //Assert
            Assert.Equal<IMovie>(blackPantherMovie, item);
            Assert.Equal("movie-blackpanther", item.Id);
            AssertEx.SystemPropertiesSet(item);
        }

        [Fact]
        public async Task CreateAsync_CreatesNewEntity_WithNullId()
        {
            //Arrange
            var repository = GetTestRepository();
            var item = blackPantherMovie.Clone();

            //Act
            await repository.CreateAsync(item);

            //Assert
            Assert.Equal<IMovie>(blackPantherMovie, item);
            Assert.True(Guid.TryParse(item.Id, out _));
            AssertEx.SystemPropertiesSet(item);
        }

        [Theory, CombinatorialData]
        public async Task CreateAsync_ThrowsConflict([CombinatorialRange(0, Movies.Count)] int index)
        {
            //Arrange
            var repository = GetTestRepository();
            var id = GetId(index);
            var item = blackPantherMovie.Clone();
            item.Id = id;

            //Act
            var ex = await Assert.ThrowsAsync<ConflictException>(() => repository.CreateAsync(item));

            //Assert
            var entity = await repository.LookupAsync(id);
            Assert.NotSame(entity, ex.Payload);
            Assert.Equal<IMovie>(entity, ex.Payload as IMovie);
            Assert.Equal<ITableData>(entity, ex.Payload as ITableData);
        }

        [Fact]
        public async Task CreateAsync_UpdatesUpdatedAt()
        {
            //Arrange
            var repository = GetTestRepository();
            var item = blackPantherMovie.Clone();
            item.UpdatedAt = DateTimeOffset.UtcNow.AddMonths(-1);

            //Act
            await repository.CreateAsync(item);

            //Assert
            Assert.Equal<IMovie>(blackPantherMovie, item);
            AssertEx.SystemPropertiesSet(item);
        }

        [Fact]
        public async Task CreateAsync_UpdatesVersion()
        {
            //Arrange
            var repository = GetTestRepository();
            var item = blackPantherMovie.Clone();
            var version = Guid.NewGuid().ToByteArray();
            item.Version = version.ToArray();

            //Act
            await repository.CreateAsync(item);

            //Assert
            Assert.Equal<IMovie>(blackPantherMovie, item);
            AssertEx.SystemPropertiesSet(item);
            Assert.False(item.Version.SequenceEqual(version));
        }

        [Fact]
        public async Task CreateAsync_Throws_OnDbError()
        {
            //Arrange
            var repository = GetTestRepository();
            var item = blackPantherMovie.Clone();
            var version = Guid.NewGuid().ToByteArray();
            item.Version = version.ToArray();

            // Close the database connection - this will force an error
            ((MovieDbContext)repository.Context).Connection.Close();

            //Act
            var ex = await Assert.ThrowsAsync<RepositoryException>(() => repository.CreateAsync(item));

            //Assert
            Assert.NotNull(ex.InnerException);
        }
        #endregion

        #region DeleteAsync
        [Fact]
        public async Task DeleteAsync_Throws_OnNullId()
        {
            // Arrange
            var repository = GetTestRepository();

            // Act
            await Assert.ThrowsAsync<BadRequestException>(() => repository.DeleteAsync(null));

            // Assert
            Assert.Equal(Movies.Count, repository.AsQueryable().Count());
        }

        [Fact]
        public async Task DeleteAsync_Throws_OnEmptyId()
        {
            // Arrange
            var repository = GetTestRepository();

            // Act
            await Assert.ThrowsAsync<BadRequestException>(() => repository.DeleteAsync(""));

            // Assert
            Assert.Equal(Movies.Count, repository.AsQueryable().Count());
        }

        [Theory]
        [InlineData("id")]
        [InlineData("id-0000")]
        [InlineData("id-000 is super long")]
        [InlineData("id-300")]
        public async Task DeleteAsync_Throws_WhenNotFound(string id)
        {
            // Arrange
            var repository = GetTestRepository();

            // Act
            await Assert.ThrowsAsync<NotFoundException>(() => repository.DeleteAsync(id));

            // Assert
            Assert.Equal(Movies.Count, repository.AsQueryable().Count());
        }

        [Theory, CombinatorialData]
        public async Task DeleteAsync_Throws_WhenVersionMismatch([CombinatorialRange(0, 248)] int index)
        {
            // Arrange
            var repository = GetTestRepository();
            var id = GetId(index);
            var version = Guid.NewGuid().ToByteArray();

            // Act
            var ex = await Assert.ThrowsAsync<PreconditionFailedException>(() => repository.DeleteAsync(id, version));

            // Assert
            var entity = await repository.LookupAsync(id);
            Assert.Equal(Movies.Count, repository.AsQueryable().Count());
            Assert.NotSame(entity, ex.Payload);
            Assert.Equal<IMovie>(entity, ex.Payload as IMovie);
            Assert.Equal<ITableData>(entity, ex.Payload as ITableData);
        }

        [Theory, CombinatorialData]
        public async Task DeleteAsync_Deletes_WhenVersionMatch([CombinatorialRange(0, 248)] int index)
        {
            // Arrange
            var repository = GetTestRepository();
            var id = GetId(index);

            // Act
            await repository.DeleteAsync(id);

            // Assert
            var entity = await repository.LookupAsync(id);
            Assert.Null(entity);
        }

        [Theory, CombinatorialData]
        public async Task DeleteAsync_Deletes_WhenNoVersion([CombinatorialRange(0, 248)] int index)
        {
            // Arrange
            var repository = GetTestRepository();
            var id = GetId(index);

            // Act
            await repository.DeleteAsync(id);

            // Assert
            var entity = await repository.LookupAsync(id);
            Assert.Null(entity);
        }

        [Fact]
        public async Task DeleteAsync_Throws_WhenEntityVersionNull()
        {
            // Arrange
            var repository = GetTestRepository();
            var id = Utils.GetMovieId(100);
            var entity = await repository.LookupAsync(id);
            var version = Guid.NewGuid().ToByteArray();
            entity.Version = null;

            // Act
            var ex = await Assert.ThrowsAsync<PreconditionFailedException>(() => repository.DeleteAsync(id, version));

            // Assert
            entity = await repository.LookupAsync(id);
            Assert.NotNull(entity);
            Assert.NotNull(ex.Payload);
            Assert.NotSame(entity, ex.Payload);
        }

        [Fact]
        public async Task DeleteAsync_Throws_WhenUpdateError()
        {
            // Arrange
            var repository = GetTestRepository();
            const string id = "id-101";

            // Close the database connection - this will force an error
            ((MovieDbContext)repository.Context).Connection.Close();

            //Act
            var ex = await Assert.ThrowsAsync<RepositoryException>(() => repository.DeleteAsync(id));

            //Assert
            Assert.NotNull(ex.InnerException);
        }
        #endregion

        #region ReadAsync
        [Theory, CombinatorialData]
        public async Task ReadAsync_ReturnsDisconnectedEntity([CombinatorialRange(0, Movies.Count)] int index)
        {
            // Arrange
            var repository = GetTestRepository();
            var id = GetId(index);

            // Act
            var actual = await repository.ReadAsync(id);

            // Assert
            var expected = await repository.LookupAsync(id);
            Assert.NotSame(expected, actual);
            Assert.Equal<IMovie>(expected, actual);
            Assert.Equal<ITableData>(expected, actual);
        }

        [Fact]
        public async Task ReadAsync_Throws_OnNullId()
        {
            // Arrange
            var repository = GetTestRepository();

            // Act
            var ex = await Assert.ThrowsAsync<BadRequestException>(() => repository.ReadAsync(null));
        }

        [Fact]
        public async Task ReadAsync_Throws_OnEmptyId()
        {
            // Arrange
            var repository = GetTestRepository();

            // Act
            var ex = await Assert.ThrowsAsync<BadRequestException>(() => repository.ReadAsync(""));
        }

        [Theory]
        [InlineData("id")]
        [InlineData("id-0000")]
        [InlineData("id-000 is super long")]
        [InlineData("id-300")]
        public async Task ReadAsync_ReturnsNull_IfMissing(string id)
        {
            // Arrange
            var repository = GetTestRepository();

            // Act
            var actual = await repository.ReadAsync(id);

            // Assert
            Assert.Null(actual);
        }
        #endregion

        #region ReplaceAsync
        [Fact]
        public async Task ReplaceAsync_Throws_OnNullEntity()
        {
            // Arrange
            var repository = GetTestRepository();

            // Act
            await Assert.ThrowsAsync<ArgumentNullException>(() => repository.ReplaceAsync(null));
        }

        [Fact]
        public async Task ReplaceAsync_Throws_OnNullId()
        {
            // Arrange
            var repository = GetTestRepository();
            var entity = blackPantherMovie.Clone();
            entity.Id = null;

            // Act
            await Assert.ThrowsAsync<BadRequestException>(() => repository.ReplaceAsync(entity));
        }

        [Fact]
        public async Task ReplaceAsync_Throws_OnEmptyId()
        {
            // Arrange
            var repository = GetTestRepository();
            var entity = blackPantherMovie.Clone();
            entity.Id = "";

            // Act
            await Assert.ThrowsAsync<BadRequestException>(() => repository.ReplaceAsync(entity));
        }

        [Theory]
        [InlineData("id")]
        [InlineData("id-0000")]
        [InlineData("id-000 is super long")]
        [InlineData("id-300")]
        public async Task ReplaceAsync_Throws_OnMissingEntity(string id)
        {
            // Arrange
            var repository = GetTestRepository();
            var entity = blackPantherMovie.Clone();
            entity.Id = id;

            // Act
            await Assert.ThrowsAsync<NotFoundException>(() => repository.ReplaceAsync(entity));
        }

        [Theory, CombinatorialData]
        public async Task ReplaceAsync_Throws_OnVersionMismatch([CombinatorialRange(0, Movies.Count)] int index)
        {
            // Arrange
            var repository = GetTestRepository();
            var entity = blackPantherMovie.Clone();
            entity.Id = GetId(index);
            var version = Guid.NewGuid().ToByteArray();

            // Act
            var ex = await Assert.ThrowsAsync<PreconditionFailedException>(() => repository.ReplaceAsync(entity, version));

            // Assert
            var expected = await repository.LookupAsync(entity.Id);
            Assert.NotSame(expected, ex.Payload);
            Assert.Equal<IMovie>(expected, ex.Payload as IMovie);
            Assert.Equal<ITableData>(expected, ex.Payload as ITableData);
        }

        [Theory, CombinatorialData]
        public async Task ReplaceAsync_Replaces_OnVersionMatch([CombinatorialRange(0, Movies.Count)] int index)
        {
            // Arrange
            var repository = GetTestRepository();
            var original = (await repository.LookupAsync(GetId(index))).Clone();
            var entity = blackPantherMovie.Clone();
            entity.Id = original.Id;
            var version = original.Version.ToArray();

            // Act
            await repository.ReplaceAsync(entity, version);

            // Assert
            var expected = await repository.LookupAsync(entity.Id);
            Assert.NotSame(expected, entity);
            Assert.Equal<IMovie>(expected, entity);
            AssertEx.SystemPropertiesChanged(original, entity);
        }

        [Theory, CombinatorialData]
        public async Task ReplaceAsync_Replaces_OnNoVersion([CombinatorialRange(0, Movies.Count)] int index)
        {
            // Arrange
            var repository = GetTestRepository();
            var entity = blackPantherMovie.Clone();
            entity.Id = GetId(index);
            var original = (await repository.LookupAsync(entity.Id)).Clone();

            // Act
            await repository.ReplaceAsync(entity);

            // Assert
            var expected = await repository.LookupAsync(entity.Id);
            Assert.NotSame(expected, entity);
            Assert.Equal<IMovie>(expected, entity);
            AssertEx.SystemPropertiesChanged(original, entity);
        }

        [Fact]
        public async Task ReplaceAsync_Throws_WhenEntityVersionNull()
        {
            // Arrange
            var repository = GetTestRepository();
            var replacement = blackPantherMovie.Clone();
            replacement.Id = "id-100";
            var original = await repository.LookupAsync(replacement.Id);
            original.Version = null;
            var version = Guid.NewGuid().ToByteArray();

            // Act
            var ex = await Assert.ThrowsAsync<PreconditionFailedException>(() => repository.ReplaceAsync(replacement, version));

            // Assert
            Assert.NotSame(original, ex.Payload);
            Assert.Equal<IMovie>(original, ex.Payload as IMovie);
        }

        [Fact]
        public async Task ReplaceAsync_Throws_OnDbError()
        {
            // Arrange
            var repository = GetTestRepository();
            var entity = blackPantherMovie.Clone();
            entity.Id = "id-101";

            // Close the database connection - this will force an error
            ((MovieDbContext)repository.Context).Connection.Close();

            //Act
            var ex = await Assert.ThrowsAsync<RepositoryException>(() => repository.ReplaceAsync(entity));

            //Assert
            Assert.NotNull(ex.InnerException);
        }
        #endregion
    }
}
