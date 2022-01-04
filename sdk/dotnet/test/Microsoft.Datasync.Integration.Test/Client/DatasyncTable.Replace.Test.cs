// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Test;
using Datasync.Common.Test.Models;
using Microsoft.AspNetCore.Datasync;
using Microsoft.Datasync.Client;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Datasync.Integration.Test.Client
{
    [ExcludeFromCodeCoverage(Justification = "Test suite")]
    [Collection("Integration")]
    public class DatasyncTable_Replace_Tests : BaseTest
    {
        [Fact]
        [Trait("Method", "ReplaceItemAsync")]
        public async Task ReplaceItemAsync_Basic()
        {
            // Arrange
            var client = GetMovieClient();
            var id = GetRandomId();
            var original = MovieServer.GetMovieById(id)!;
            var expected = ClientMovie.From(original);
            expected.Title = "Replacement Title";
            expected.Rating = "PG-13";

            // Set up event handler
            var table = client.GetTable<ClientMovie>("movies");
            var modifications = new List<DatasyncTableEventArgs>();
            table.TableModified += (_, args) => modifications.Add(args);

            // Act
            var response = await table.ReplaceItemAsync(expected).ConfigureAwait(false);
            var stored = MovieServer.GetMovieById(id);

            // Assert
            Assert.Equal(200, response.StatusCode);
            Assert.Equal<IMovie>(expected, stored);
            AssertEx.SystemPropertiesChanged(original, stored);
            AssertEx.SystemPropertiesMatch(stored, response.Value);
            AssertEx.Contains("ETag", $"\"{response.Value.Version}\"", response.Headers);

            Assert.Single(modifications);
            var modArg = modifications[0];
            Assert.Equal(table.Endpoint, modArg.TableEndpoint);
            Assert.Equal(Microsoft.Datasync.Client.TableOperation.Update, modArg.Operation);
            Assert.Equal(response.Value.Id, modArg.Id);
            Assert.Equal<IMovie>(response.Value, (IMovie)modArg.Entity);
        }

        [Theory]
        [InlineData("duration", 50)]
        [InlineData("duration", 370)]
        [InlineData("rating", "M")]
        [InlineData("rating", "PG-13 but not always")]
        [InlineData("title", "a")]
        [InlineData("title", "Lorem ipsum dolor sit amet, consectetur adipiscing elit accumsan.")]
        [InlineData("year", 1900)]
        [InlineData("year", 2035)]
        [Trait("Method", "ReplaceItemAsync")]
        public async Task ReplaceItemAsync_Validation(string propName, object propValue)
        {
            // Arrange
            var client = GetMovieClient();
            var id = GetRandomId();
            var original = MovieServer.GetMovieById(id)!;
            var expected = ClientMovie.From(original);
            var entity = expected.Clone();

            switch (propName)
            {
                case "duration": entity.Duration = (int)propValue; break;
                case "rating": entity.Rating = (string)propValue; break;
                case "title": entity.Title = (string)propValue; break;
                case "year": entity.Year = (int)propValue; break;
            }

            // Set up event handler
            var table = client.GetTable<ClientMovie>("movies");
            var modifications = new List<DatasyncTableEventArgs>();
            table.TableModified += (_, args) => modifications.Add(args);

            // Act
            var exception = await Assert.ThrowsAsync<DatasyncOperationException>(() => table.ReplaceItemAsync(entity)).ConfigureAwait(false);
            var stored = MovieServer.GetMovieById(id);

            // Assert
            Assert.Equal(400, exception.StatusCode);
            Assert.Equal<IMovie>(expected, stored);
            Assert.Equal<ITableData>(original, stored);
            Assert.Empty(modifications);
        }

        [Fact]
        [Trait("Method", "ReplaceItemAsync")]
        public async Task ReplaceItemAsync_ThrowsWhenNotFound()
        {
            // Arrange
            var client = GetMovieClient();
            var obj = GetSampleMovie<ClientMovie>();
            obj.Id = "not-found";

            // Set up event handler
            var table = client.GetTable<ClientMovie>("movies");
            var modifications = new List<DatasyncTableEventArgs>();
            table.TableModified += (_, args) => modifications.Add(args);

            // Act
            var exception = await Assert.ThrowsAsync<DatasyncOperationException>(() => table.ReplaceItemAsync(obj)).ConfigureAwait(false);

            // Assert
            Assert.Equal(404, exception.StatusCode);
            Assert.Empty(modifications);
        }

        [Fact]
        [Trait("Method", "ReplaceItemAsync")]
        public async Task ReplaceItemAsync_ConditionalSuccess()
        {
            // Arrange
            var client = GetMovieClient();
            var id = GetRandomId();
            var original = MovieServer.GetMovieById(id);
            var expected = ClientMovie.From(original);
            expected.Title = "Replacement Title";
            expected.Rating = "PG-13";

            // Set up event handler
            var table = client.GetTable<ClientMovie>("movies");
            var modifications = new List<DatasyncTableEventArgs>();
            table.TableModified += (_, args) => modifications.Add(args);

            // Act
            var response = await table.ReplaceItemAsync(expected, IfMatch.Version(expected.Version)).ConfigureAwait(false);
            var stored = MovieServer.GetMovieById(id);

            // Assert
            Assert.Equal(200, response.StatusCode);
            Assert.Equal<IMovie>(expected, stored);
            AssertEx.SystemPropertiesChanged(original, stored);
            AssertEx.SystemPropertiesMatch(stored, response.Value);
            AssertEx.Contains("ETag", $"\"{response.Value.Version}\"", response.Headers);

            Assert.Single(modifications);
            var modArg = modifications[0];
            Assert.Equal(table.Endpoint, modArg.TableEndpoint);
            Assert.Equal(Microsoft.Datasync.Client.TableOperation.Update, modArg.Operation);
            Assert.Equal(response.Value.Id, modArg.Id);
            Assert.Equal<IMovie>(response.Value, (IMovie)modArg.Entity);
        }

        [Fact]
        [Trait("Method", "ReplaceItemAsync")]
        public async Task ReplaceItemAsync_ConditionalFailure()
        {
            // Arrange
            var client = GetMovieClient();
            var id = GetRandomId();
            var original = MovieServer.GetMovieById(id)!;
            var expected = ClientMovie.From(original);
            expected.Title = "Replacement Title";
            expected.Rating = "PG-13";
            const string etag = "dGVzdA==";

            // Set up event handler
            var table = client.GetTable<ClientMovie>("movies");
            var modifications = new List<DatasyncTableEventArgs>();
            table.TableModified += (_, args) => modifications.Add(args);

            // Act
            var exception = await Assert.ThrowsAsync<DatasyncConflictException<ClientMovie>>(() => table.ReplaceItemAsync(expected, IfMatch.Version(etag))).ConfigureAwait(false);

            // Assert
            Assert.NotNull(exception.Request);
            Assert.NotNull(exception.Response);
            Assert.Equal(412, exception.StatusCode);
            Assert.Equal(MovieCount, MovieServer.GetMovieCount());
            var entity = MovieServer.GetMovieById(id);
            Assert.NotNull(entity);
            AssertEx.SystemPropertiesSet(entity);
            AssertEx.SystemPropertiesMatch(entity, exception.ServerItem);
            Assert.Equal<IMovie>(original, exception.ServerItem);
            Assert.Equal<IMovie>(original, entity);
            Assert.Equal<ITableData>(original, entity);
            AssertEx.ResponseHasConditionalHeaders(entity, exception.Response);
            Assert.Empty(modifications);
        }

        [Fact]
        [Trait("Method", "ReplaceItemAsync")]
        public async Task ReplaceItemAsync_SoftNotDeleted()
        {
            // Arrange
            var client = GetMovieClient();
            var id = GetRandomId();
            var original = MovieServer.GetMovieById(id)!;
            var expected = ClientMovie.From(original);
            expected.Title = "Replacement Title";
            expected.Rating = "PG-13";

            // Set up event handler
            var table = client.GetTable<ClientMovie>("soft");
            var modifications = new List<DatasyncTableEventArgs>();
            table.TableModified += (_, args) => modifications.Add(args);

            // Act
            var response = await table.ReplaceItemAsync(expected).ConfigureAwait(false);
            var stored = MovieServer.GetMovieById(id).Clone();

            // Assert
            Assert.Equal(200, response.StatusCode);
            Assert.Equal<IMovie>(expected, stored);
            AssertEx.SystemPropertiesChanged(original, stored);
            AssertEx.SystemPropertiesMatch(stored, response.Value);
            AssertEx.Contains("ETag", $"\"{response.Value.Version}\"", response.Headers);

            Assert.Single(modifications);
            var modArg = modifications[0];
            Assert.Equal(table.Endpoint, modArg.TableEndpoint);
            Assert.Equal(Microsoft.Datasync.Client.TableOperation.Update, modArg.Operation);
            Assert.Equal(response.Value.Id, modArg.Id);
            Assert.Equal<IMovie>(response.Value, (IMovie)modArg.Entity);
        }

        [Fact]
        [Trait("Method", "ReplaceItemAsync")]
        public async Task ReplaceItemAsync_SoftDeleted_ReturnsGone()
        {
            // Arrange
            var client = GetMovieClient();
            var id = GetRandomId();
            await MovieServer.SoftDeleteMoviesAsync(x => x.Id == id).ConfigureAwait(false);
            var original = MovieServer.GetMovieById(id)!;
            var expected = ClientMovie.From(original);
            expected.Title = "Replacement Title";
            expected.Rating = "PG-13";

            // Set up event handler
            var table = client.GetTable<ClientMovie>("soft");
            var modifications = new List<DatasyncTableEventArgs>();
            table.TableModified += (_, args) => modifications.Add(args);

            // Act
            var exception = await Assert.ThrowsAsync<DatasyncOperationException>(() => table.ReplaceItemAsync(expected)).ConfigureAwait(false);
            var stored = MovieServer.GetMovieById(id).Clone();

            // Assert
            Assert.Equal(410, exception.StatusCode);
            Assert.Equal<IMovie>(original, stored);
            Assert.Equal<ITableData>(original, stored);
            Assert.Empty(modifications);
        }
    }
}
