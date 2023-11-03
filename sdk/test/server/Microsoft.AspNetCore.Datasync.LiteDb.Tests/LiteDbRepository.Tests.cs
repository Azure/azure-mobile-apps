// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Tests;
using LiteDB;

namespace Microsoft.AspNetCore.Datasync.LiteDb.Tests;

[ExcludeFromCodeCoverage]
public class LiteDbRepository_Tests : RepositoryTests<LiteDbMovie>, IDisposable
{
    #region Setup
    private readonly string dbFilename;
    private readonly LiteDatabase database;
    private readonly ILiteCollection<LiteDbMovie> collection;
    private readonly LiteDbRepository<LiteDbMovie> repository;


    public LiteDbRepository_Tests()
    {
        dbFilename = Path.GetTempFileName();
        database = new LiteDatabase($"Filename={dbFilename};Connection=direct;InitialSize=0");

        collection = database.GetCollection<LiteDbMovie>("litedbmovies");
        foreach (LiteDbMovie movie in Movies.OfType<LiteDbMovie>())
        {
            movie.UpdatedAt = DateTimeOffset.Now;
            movie.Version = Guid.NewGuid().ToByteArray();
            collection.Insert(movie);
        }
        repository = new LiteDbRepository<LiteDbMovie>(database);
        Repository = repository;
    }

    public void Dispose()
    {
        database.Dispose();
        File.Delete(dbFilename);
        GC.SuppressFinalize(this);
    }

    protected override LiteDbMovie GetEntity(string id)
        => collection.FindById(id);

    protected override int GetEntityCount()
        => collection.Count();
    #endregion
}
