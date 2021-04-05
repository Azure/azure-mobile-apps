// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AzureMobile.Common.Test;
using Microsoft.AzureMobile.Common.Test.Models;
using Microsoft.AzureMobile.Common.Test.TestData;
using Microsoft.AzureMobile.Server.Exceptions;
using Xunit;

namespace Microsoft.AzureMobile.Server.InMemory.Test
{
    [SuppressMessage("Design", "RCS1090:Add call to 'ConfigureAwait' (or vice versa).", Justification = "Test suite")]
    [ExcludeFromCodeCoverage(Justification = "Test suite")]
    public class InMemoryRepository_Tests
    {
        #region Test Artifacts
        /// <summary>
        /// A basic movie without any adornment that does not exist in the movie data. Tests must clone this
        /// object and then adjust.
        /// </summary>
        private readonly InMemoryMovie blackPantherMovie = new()
        {
            BestPictureWinner = true,
            Duration = 134,
            Rating = "PG-13",
            ReleaseDate = DateTimeOffset.Parse("16-Feb-2018"),
            Title = "Black Panther",
            Year = 2018
        };

        /// <summary>
        /// Gets a seeded test repository
        /// </summary>
        /// <returns></returns>
        private static InMemoryRepository<InMemoryMovie> GetTestRepository() => new(Movies.OfType<InMemoryMovie>());

        /// <summary>
        /// Converts an index to an id.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        private static string GetId(int index) => string.Format("id-{0:000}", index);
        #endregion

        #region Ctor
        [Fact]
        public void Ctor_Empty()
        {
            // Arrange

            // Act
            var repository = new InMemoryRepository<InMemoryMovie>();

            // Assert
            Assert.NotNull(repository);
            Assert.NotNull(repository.Entities);
            Assert.Empty(repository.Entities);
        }

        [Fact]
        public void Ctor_Seeded()
        {
            // Arrange

            // Act
            var repository = GetTestRepository();

            // Assert
            Assert.NotNull(repository);
            Assert.NotNull(repository.Entities);
            Assert.NotEmpty(repository.Entities);
        }
        #endregion

        #region AsQueryable
        [Fact]
        public void AsQueryable_ReturnsQueryable()
        {
            // Arrange
            var repository = GetTestRepository();

            // Act
            var queryable = repository.AsQueryable();

            // Assert
            Assert.IsAssignableFrom<IQueryable<InMemoryMovie>>(queryable);
        }

        [Theory, CombinatorialData]
        public void AsQueryable_CanRetrieveSingleItems([CombinatorialRange(0, Movies.Count)] int index)
        {
            // Arrange
            var repository = GetTestRepository();
            var id = GetId(index);

            // Act
            var actual = repository.AsQueryable().Single(m => m.Id == id);

            // Assert
            var expected = repository.GetEntity(id);
            Assert.Equal<IMovie>(expected, actual);
            Assert.Equal<ITableData>(expected, actual);
        }

        [Fact]
        public void AsQueryable_CanRetrieveFilteredLists()
        {
            // Arrange
            var repository = GetTestRepository();

            // Act
            var ratedMovies = repository.AsQueryable().Where(m => m.Rating == "R").ToList();

            // Assert
            Assert.Equal(94, ratedMovies.Count);
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
            var entity = repository.GetEntity(id);
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
        public async Task CreateAsync_StoresDisconnectedEntity()
        {
            //Arrange
            var repository = GetTestRepository();
            var item = blackPantherMovie.Clone();
            item.Id = "movie-blackpanther";

            //Act
            await repository.CreateAsync(item);

            //Assert
            var entity = repository.GetEntity(item.Id);
            Assert.NotSame(entity, item);
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
            Assert.Equal(Movies.Count, repository.Entities.Count);
        }

        [Fact]
        public async Task DeleteAsync_Throws_OnEmptyId()
        {
            // Arrange
            var repository = GetTestRepository();

            // Act
            await Assert.ThrowsAsync<BadRequestException>(() => repository.DeleteAsync(""));

            // Assert
            Assert.Equal(Movies.Count, repository.Entities.Count);
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
            Assert.Equal(Movies.Count, repository.Entities.Count);
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
            var entity = repository.GetEntity(id);
            Assert.Equal(Movies.Count, repository.Entities.Count);
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
            var entity = repository.GetEntity(id);
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
            var entity = repository.GetEntity(id);
            Assert.Null(entity);
        }

        [Fact]
        public async Task DeleteAsync_Throws_WhenEntityVersionNull()
        {
            // Arrange
            var repository = GetTestRepository();
            var id = Utils.GetMovieId(100);
            var entity = repository.GetEntity(id);
            var version = Guid.NewGuid().ToByteArray();
            entity.Version = null;

            // Act
            var ex = await Assert.ThrowsAsync<PreconditionFailedException>(() => repository.DeleteAsync(id, version));

            // Assert
            entity = repository.GetEntity(id);
            Assert.NotNull(entity);
            Assert.NotNull(ex.Payload);
            Assert.NotSame(entity, ex.Payload);
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
            var expected = repository.GetEntity(id);
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
            var expected = repository.GetEntity(entity.Id);
            Assert.NotSame(expected, ex.Payload);
            Assert.Equal<IMovie>(expected, ex.Payload as IMovie);
            Assert.Equal<ITableData>(expected, ex.Payload as ITableData);
        }

        [Theory, CombinatorialData]
        public async Task ReplaceAsync_Replaces_OnVersionMatch([CombinatorialRange(0, Movies.Count)] int index)
        {
            // Arrange
            var repository = GetTestRepository();
            var original = repository.GetEntity(GetId(index)).Clone();
            var entity = blackPantherMovie.Clone();
            entity.Id = original.Id;
            var version = original.Version.ToArray();

            // Act
            await repository.ReplaceAsync(entity, version);

            // Assert
            var expected = repository.GetEntity(entity.Id);
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
            var original = repository.GetEntity(entity.Id).Clone();

            // Act
            await repository.ReplaceAsync(entity);

            // Assert
            var expected = repository.GetEntity(entity.Id);
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
            var original = repository.GetEntity(replacement.Id);
            original.Version = null;
            var version = Guid.NewGuid().ToByteArray();

            // Act
            var ex = await Assert.ThrowsAsync<PreconditionFailedException>(() => repository.ReplaceAsync(replacement, version));

            // Assert
            Assert.NotSame(original, ex.Payload);
            Assert.Equal<IMovie>(original, ex.Payload as IMovie);
        }
        #endregion

        #region Test Throws
        [Theory, CombinatorialData]
        public async Task Throws_TestException([CombinatorialRange(0, 5)] int op)
        {
            // Arrange
            var repository = new InMemoryRepository<InMemoryMovie>
            {
                ThrowException = new AzureMobileException("test exception")
            };
            var item = new InMemoryMovie
            {
                Id = Guid.NewGuid().ToString("N"),
                Duration = 90,
                ReleaseDate = DateTimeOffset.Parse("12/31/2021"),
                Rating = "G",
                Title = "Test Movie",
                Year = 2021
            };

            // Act
            AzureMobileException ex = null;
            switch (op)
            {
                case 0:
                    ex = await Assert.ThrowsAsync<AzureMobileException>(() => repository.CreateAsync(item));
                    break;
                case 1:
                    ex = await Assert.ThrowsAsync<AzureMobileException>(() => repository.DeleteAsync(item.Id));
                    break;
                case 2:
                    ex = await Assert.ThrowsAsync<AzureMobileException>(() => repository.ReadAsync(item.Id));
                    break;
                case 3:
                    ex = await Assert.ThrowsAsync<AzureMobileException>(() => repository.ReplaceAsync(item));
                    break;
                case 4:
                    ex = Assert.Throws<AzureMobileException>(() => repository.AsQueryable());
                    break;
                default:
                    Assert.True(false, "Invalid case statement for op");
                    break;
            }

            // Assert
            Assert.NotNull(ex);
            Assert.Equal("test exception", ex.Message);
        }
        #endregion
    }
}