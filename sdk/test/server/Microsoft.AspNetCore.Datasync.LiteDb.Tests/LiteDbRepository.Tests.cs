// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Tests;
using LiteDB;

namespace Microsoft.AspNetCore.Datasync.LiteDb.Tests;

[ExcludeFromCodeCoverage]
public class LiteDbRepository_Tests : RepositoryTests<LiteDbMovie>, IDisposable
{
    #region Setup
    private string dbFilename;
    private LiteDatabase database;
    private ILiteCollection<LiteDbMovie> collection;
    private LiteDbRepository<LiteDbMovie> repository;
    private List<LiteDbMovie> movies = new();

    protected override Task<LiteDbMovie> GetEntityAsync(string id)
        => Task.FromResult(collection.FindById(id));

    protected override Task<int> GetEntityCountAsync()
        => Task.FromResult(collection.Count());

    protected override Task<IRepository<LiteDbMovie>> GetPopulatedRepositoryAsync()
    {
        dbFilename = Path.GetTempFileName();
        database = new LiteDatabase($"Filename={dbFilename};Connection=direct;InitialSize=0");

        collection = database.GetCollection<LiteDbMovie>("litedbmovies");
        foreach (LiteDbMovie movie in Movies.OfType<LiteDbMovie>())
        {
            movie.UpdatedAt = DateTimeOffset.Now;
            movie.Version = Guid.NewGuid().ToByteArray();
            collection.Insert(movie);
            movies.Add(movie);
        }
        repository = new LiteDbRepository<LiteDbMovie>(database);
        return Task.FromResult<IRepository<LiteDbMovie>>(repository);
    }

    protected override Task<string> GetRandomEntityIdAsync(bool exists)
    {
        Random random = new();
        return Task.FromResult(exists ? movies[random.Next(movies.Count)].Id : Guid.NewGuid().ToString());
    }

    public void Dispose()
    {
        database.Dispose();
        File.Delete(dbFilename);
        GC.SuppressFinalize(this);
    }
    #endregion
}
