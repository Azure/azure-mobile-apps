// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Datasync.Common.Test;
using Datasync.Common.Test.Models;
using Datasync.Common.Test.TestData;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Xunit;

namespace Microsoft.AspNetCore.Datasync.EFCore.Test
{
    #region MovieDbContext
    [ExcludeFromCodeCoverage(Justification = "Test suite")]
    public class MovieDbContext : DbContext
    {
        public MovieDbContext(DbContextOptions<MovieDbContext> options) : base(options)
        {
        }

        // This is for storing the explicit connection used for SQLite.
        // It's used for testing error handling, by prematurely closing the connection
        public SqliteConnection Connection { get; set; }

        public DbSet<EntityMovie> Movies { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            if (Database.IsSqlite())
            {
                // Sqlite does not support [Timestamp] attribute, which is required by the EntityTableRepository.
                // So we must fake it out and set up our own handling for version updates.
                // If you use Sqlite in your own code as an actual store, you will need to do this too
                var props = builder.Model.GetEntityTypes().SelectMany(t => t.GetProperties()).Where(p => IsTimestampField(p));
                foreach (var prop in props)
                {
                    prop.SetValueConverter(new SqliteTimestampConverter());
                    // We use the STRFTIME ISO8601 timestamp so that we can get ms precision.
                    // Using CURRENT_TIMESTAMP does not have the required resolution for rapid tests.
                    prop.SetDefaultValueSql("STRFTIME('%Y%m%dT%H%M%f', 'NOW')");
                }
            }
        }

        /// <summary>
        /// Determines if a particular property is a [Timestamp] property.
        /// </summary>
        /// <param name="t">The property</param>
        /// <returns>true if the property is decorated as [Timestamp]</returns>
        private static bool IsTimestampField(IProperty p)
            => p.ClrType == typeof(byte[]) && p.ValueGenerated == ValueGenerated.OnAddOrUpdate && p.IsConcurrencyToken;

        /// <summary>
        /// Generates the SQL comamnd necessary to install the Timestamp trigger
        /// </summary>
        /// <param name="tableName">The name of the table</param>
        /// <param name="fieldName">The name of the field</param>
        /// <returns>The SQL command</returns>
        private static string GetTriggerSqlCommand(string tableName, string fieldName)
            => $@"
            CREATE TRIGGER s_{tableName}_{fieldName}_update
                AFTER UPDATE ON {tableName}
                BEGIN 
                    UPDATE {tableName}
                    SET {fieldName} = randomblob(8)
                    WHERE rowid = NEW.rowid;
                END
            ";

        /// <summary>
        /// Installs the triggers necessary to run the database, specifically around the [Timestamp]
        /// requirements.
        /// </summary>
        internal void InstallTriggers()
        {
            if (Database.IsSqlite())
            {
                var tables = Model.GetEntityTypes();
                foreach (var table in tables)
                {
                    var props = table.GetProperties().Where(p => IsTimestampField(p));
                    var tableName = table.GetTableName();

                    foreach (var field in props)
                    {
                        _ = Database.ExecuteSqlRaw(GetTriggerSqlCommand(tableName, field.Name));
                    }
                }
            }
        }
    }

    /// <summary>
    /// ValueConverter for Sqlite to support the [Timestamp] type.
    /// </summary>
    [ExcludeFromCodeCoverage(Justification = "Test suite")]
    internal class SqliteTimestampConverter : ValueConverter<byte[], string>
    {
        public SqliteTimestampConverter() : base(v => v == null ? null : ToDb(v), v => v == null ? null : FromDb(v))
        {
        }

        private static byte[] FromDb(string v) => Encoding.ASCII.GetBytes(v);

        private static string ToDb(byte[] v) => Encoding.ASCII.GetString(v);
    }
    #endregion MovieDbcontext

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
            (repository.Context as MovieDbContext)?.Connection.Close();

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
