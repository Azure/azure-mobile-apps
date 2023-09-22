// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Test.Service;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace Datasync.Common.Test;

/// <summary>
/// We do some pretty major things to make MovieDbContext work "in memory",
/// so we want to make sure those work and that it's not the rest of the test
/// suite playing up.
/// </summary>
[ExcludeFromCodeCoverage]
public class MovieDbContext_Tests : BaseTest
{
    private readonly TestServer server = MovieApiServer.CreateTestServer();

    [Fact]
    public void Context_IsPopulated()
    {
        Assert.Equal(TestData.Movies.Count, server.GetMovieCount());
    }

    [Theory, CombinatorialData]
    public void EveryRecord_HasValidDateTimeOffsets([CombinatorialRange(0, TestData.Movies.Count)] int idx)
    {
        var id = Utils.GetMovieId(idx);
        var entity = server.GetMovieById(id)!;

        Assert.True(entity.ReleaseDate.Year > 1900);
        AssertEx.CloseTo(entity.UpdatedAt, DateTimeOffset.UtcNow);
    }

    [Theory, CombinatorialData]
    public void EveryRecord_HasNonEmptyVersion([CombinatorialRange(0, TestData.Movies.Count)] int idx)
    {
        var id = Utils.GetMovieId(idx);
        var entity = server.GetMovieById(id)!;

        Assert.NotNull(entity.Version);
        Assert.NotEmpty(entity.Version);
    }

    [Fact]
    public void ChangingRecord_ChangesVersion()
    {
        using var scope = server.Host.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MovieDbContext>();
        var id = TestData.Movies.GetRandomId();

        var entity = context.Movies.Single(x => x.Id == id);
        var version = Convert.ToBase64String(entity.Version);
        entity.Title = "Updated Title";
        context.SaveChanges();

        // This discards all the trackings so that the next thing happens against the DB
        context.ChangeTracker.Clear();

        var entity2 = context.Movies.Single(x => x.Id == id);
        var version2 = Convert.ToBase64String(entity2.Version);
        Assert.Equal("Updated Title", entity2.Title);
        Assert.NotEqual(version2, version);
    }

    [Fact]
    public void CanUpdate_DateTimeOffset()
    {
        using var scope = server.Host.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MovieDbContext>();
        var id = TestData.Movies.GetRandomId();
        var exampleDate = DateTimeOffset.Parse("2018-03-29T12:30:45.000Z");

        var entity = context.Movies.Single(x => x.Id == id);
        entity.ReleaseDate = exampleDate;
        context.SaveChanges();

        // This discards all the trackings so that the next thing happens against the DB
        context.ChangeTracker.Clear();

        var entity2 = context.Movies.Single(x => x.Id == id);
        Assert.Equal(exampleDate, entity.ReleaseDate);
    }
}
