// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Models;
using Datasync.Common.TestData;
using System.Net;
using System.Text.Json;

// We want constant arrays in tests since it makes the tests non-dependent and readable.
#pragma warning disable CA1861 // Avoid constant arrays as arguments

namespace Microsoft.AspNetCore.Datasync.Tests.Service;

[ExcludeFromCodeCoverage]
public class Query_Tests : ServiceTest, IClassFixture<ServiceApplicationFactory>
{
    public Query_Tests(ServiceApplicationFactory factory) : base(factory)
    {
    }

    [Theory]
    [InlineData("$filter=(year - 1900) ge 100", HttpStatusCode.BadRequest)]
    [InlineData("$filter=missing eq 20", HttpStatusCode.BadRequest)]
    [InlineData("$orderby=duration fizz", HttpStatusCode.BadRequest)]
    [InlineData("$orderby=missing asc", HttpStatusCode.BadRequest)]
    [InlineData("$select=foo", HttpStatusCode.BadRequest)]
    [InlineData("$select=year rating", HttpStatusCode.BadRequest)]
    [InlineData("$skip=-1", HttpStatusCode.BadRequest)]
    [InlineData("$skip=NaN", HttpStatusCode.BadRequest)]
    [InlineData("$top=-1", HttpStatusCode.BadRequest)]
    [InlineData("$top=1000000", HttpStatusCode.BadRequest)]
    [InlineData("$top=NaN", HttpStatusCode.BadRequest)]
    public async Task FailedQueryTest(string query, HttpStatusCode expectedStatusCode)
    {
        HttpResponseMessage response = await client.GetAsync($"{factory.MovieEndpoint}?{query}");
        response.Should().HaveStatusCode(expectedStatusCode);
    }

    [Fact]
    public async Task Query_Test_001()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}",
            100,
            "$skip=100",
            null,
            new[] { "id-000", "id-001", "id-002", "id-003", "id-004" }
        );
    }

    [Fact]
    public async Task Query_Test_002()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$count=true",
            100,
            "$count=true&$skip=100",
            factory.Count<InMemoryMovie>(),
            new[] { "id-000", "id-001", "id-002", "id-003", "id-004" }
        );
    }

    [Fact]
    public async Task Query_Test_003()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$count=true&$filter=((year div 1000.5) eq 2) and (rating eq 'R')",
            2,
            null,
            2,
            new[] { "id-061", "id-173" }
        );
    }

    [Fact]
    public async Task Query_Test_004()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$count=true&$filter=((year sub 1900) ge 80) and ((year add 10) le 2000) and (duration le 120)",
            13,
            null,
            13,
            new[] { "id-026", "id-047", "id-081", "id-103", "id-121" }
        );
    }

    [Fact]
    public async Task Query_Test_005()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$count=true&$filter=(year div 1000.5) eq 2",
            6,
            null,
            6,
            new[] { "id-012", "id-042", "id-061", "id-173", "id-194" }
            );
    }

    [Fact]
    public async Task Query_Test_006()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$count=true&$filter=(year ge 1930 and year le 1940) or (year ge 1950 and year le 1960)",
            46,
            null,
            46,
            new[] { "id-005", "id-016", "id-027", "id-028", "id-031" }
            );
    }

    [Fact]
    public async Task Query_Test_007()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$count=true&$filter=(year sub 1900) ge 80",
            100,
            "$count=true&$filter=(year sub 1900) ge 80&$skip=100",
            138,
            new[] { "id-000", "id-003", "id-006", "id-007", "id-008" }
            );
    }

    [Fact]
    public async Task Query_Test_008()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$count=true&$filter=bestPictureWinner eq false",
            100,
            "$count=true&$filter=bestPictureWinner eq false&$skip=100",
            210,
            new[] { "id-000", "id-003", "id-004", "id-005", "id-006" }
        );
    }

    [Fact]
    public async Task Query_Test_009()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$count=true&$filter=bestPictureWinner eq true and ceiling(duration div 60.0) eq 2",
            11,
            null,
            11,
            new[] { "id-023", "id-024", "id-112", "id-135", "id-142" }
        );
    }

    [Fact]
    public async Task Query_Test_010()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$count=true&$filter=bestPictureWinner eq true and floor(duration div 60.0) eq 2",
            21,
            null,
            21,
            new[] { "id-001", "id-011", "id-018", "id-048", "id-051" }
        );
    }

    [Fact]
    public async Task Query_Test_011()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$count=true&$filter=bestPictureWinner eq true and round(duration div 60.0) eq 2",
            24,
            null,
            24,
            new[] { "id-011", "id-018", "id-023", "id-024", "id-048" }
        );
    }

    [Fact]
    public async Task Query_Test_012()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$count=true&$filter=bestPictureWinner eq true",
            38,
            null,
            38,
            new[] { "id-001", "id-002", "id-007", "id-008", "id-011" }
        );
    }

    [Fact]
    public async Task Query_Test_013()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$count=true&$filter=bestPictureWinner ne false",
            38,
            null,
            38,
            new[] { "id-001", "id-002", "id-007", "id-008", "id-011" }
        );
    }

    [Fact]
    public async Task Query_Test_014()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$count=true&$filter=bestPictureWinner ne true",
            100,
            "$count=true&$filter=bestPictureWinner ne true&$skip=100",
            210,
            new[] { "id-000", "id-003", "id-004", "id-005", "id-006" }
        );
    }

    [Fact]
    public async Task Query_Test_015()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$count=true&$filter=ceiling(duration div 60.0) eq 2",
            100,
            "$count=true&$filter=ceiling(duration div 60.0) eq 2&$skip=100",
            124,
            new[] { "id-005", "id-023", "id-024", "id-025", "id-026" }
        );
    }

    [Fact]
    public async Task Query_Test_016()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$count=true&$filter=day(releaseDate) eq 1",
            7,
            null,
            7,
            new[] { "id-019", "id-048", "id-129", "id-131", "id-132" }
        );
    }

    [Fact]
    public async Task Query_Test_017()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$count=true&$filter=duration ge 60",
            100,
            "$count=true&$filter=duration ge 60&$skip=100",
            factory.Count<InMemoryMovie>(),
            new[] { "id-000", "id-001", "id-002", "id-003", "id-004" }
        );
    }

    [Fact]
    public async Task Query_Test_018()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$count=true&$filter=endswith(title, 'er')",
            12,
            null,
            12,
            new[] { "id-001", "id-052", "id-121", "id-130", "id-164" }
        );
    }

    [Fact]
    public async Task Query_Test_019()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$count=true&$filter=endswith(tolower(title), 'er')",
            12,
            null,
            12,
            new[] { "id-001", "id-052", "id-121", "id-130", "id-164" }
        );
    }

    [Fact]
    public async Task Query_Test_020()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$count=true&$filter=endswith(toupper(title), 'ER')",
            12,
            null,
            12,
            new[] { "id-001", "id-052", "id-121", "id-130", "id-164" }
        );
    }

    [Fact]
    public async Task Query_Test_021()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$count=true&$filter=floor(duration div 60.0) eq 2",
            100,
            "$count=true&$filter=floor(duration div 60.0) eq 2&$skip=100",
            120,
            new[] { "id-000", "id-001", "id-003", "id-004", "id-006" }
        );
    }

    [Fact]
    public async Task Query_Test_022()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$count=true&$filter=month(releaseDate) eq 11",
            14,
            null,
            14,
            new[] { "id-011", "id-016", "id-030", "id-064", "id-085" }
        );
    }

    [Fact]
    public async Task Query_Test_023()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$count=true&$filter=not(bestPictureWinner eq false)",
            38,
            null,
            38,
            new[] { "id-001", "id-002", "id-007", "id-008", "id-011" }
        );
    }

    [Fact]
    public async Task Query_Test_024()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$count=true&$filter=not(bestPictureWinner eq true)",
            100,
            "$count=true&$filter=not(bestPictureWinner eq true)&$skip=100",
            210,
            new[] { "id-000", "id-003", "id-004", "id-005", "id-006" }
        );
    }

    [Fact]
    public async Task Query_Test_025()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$count=true&$filter=not(bestPictureWinner ne false)",
            100,
            "$count=true&$filter=not(bestPictureWinner ne false)&$skip=100",
            210,
            new[] { "id-000", "id-003", "id-004", "id-005", "id-006" }
        );
    }

    [Fact]
    public async Task Query_Test_026()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$count=true&$filter=not(bestPictureWinner ne true)",
            38,
            null,
            38,
            new[] { "id-001", "id-002", "id-007", "id-008", "id-011" }
        );
    }

    [Fact]
    public async Task Query_Test_027()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$count=true&$filter=rating eq 'R'",
            95,
            null,
            95,
            new[] { "id-000", "id-001", "id-002", "id-003", "id-007" }
        );
    }

    [Fact]
    public async Task Query_Test_028()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$count=true&$filter=rating ne 'PG13'",
            100,
            "$count=true&$filter=rating ne 'PG13'&$skip=100",
            220,
            new[] { "id-000", "id-001", "id-002", "id-003", "id-004" }
        );
    }

    [Fact]
    public async Task Query_Test_029()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$count=true&$filter=rating eq 'Unrated'",
            74,
            null,
            74,
            new[] { "id-004", "id-005", "id-011", "id-016", "id-031" }
        );
    }

    [Fact]
    public async Task Query_Test_030()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$count=true&$filter=releaseDate eq cast(1994-10-14,Edm.Date)",
            2,
            null,
            2,
            new[] { "id-000", "id-003" }
        );
    }

    [Fact]
    public async Task Query_Test_031()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$count=true&$filter=releaseDate ge cast(1999-12-31,Edm.Date)",
            69,
            null,
            69,
            new[] { "id-006", "id-008", "id-012", "id-013", "id-019" }
        );
    }

    [Fact]
    public async Task Query_Test_032()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$count=true&$filter=releaseDate gt cast(1999-12-31,Edm.Date)",
            69,
            null,
            69,
            new[] { "id-006", "id-008", "id-012", "id-013", "id-019" }
        );
    }

    [Fact]
    public async Task Query_Test_033()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$count=true&$filter=releaseDate le cast(2000-01-01,Edm.Date)",
            100,
            "$count=true&$filter=releaseDate le cast(2000-01-01,Edm.Date)&$skip=100",
            179,
            new[] { "id-000", "id-001", "id-002", "id-003", "id-004" }
        );
    }

    [Fact]
    public async Task Query_Test_034()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$count=true&$filter=releaseDate lt cast(2000-01-01,Edm.Date)",
            100,
            "$count=true&$filter=releaseDate lt cast(2000-01-01,Edm.Date)&$skip=100",
            179,
            new[] { "id-000", "id-001", "id-002", "id-003", "id-004" }
        );
    }

    [Fact]
    public async Task Query_Test_035()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$count=true&$filter=round(duration div 60.0) eq 2",
            100,
            "$count=true&$filter=round(duration div 60.0) eq 2&$skip=100",
            Movies.MovieList.Count(x => Math.Round(x.Duration / 60.0) == 2.0),
            new[] { "id-000", "id-005", "id-009", "id-010", "id-011" }
        );
    }

    [Fact]
    public async Task Query_Test_037()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$count=true&$filter=startswith(tolower(title), 'the')",
            63,
            null,
            63,
            new[] { "id-000", "id-001", "id-002", "id-004", "id-006" }
        );
    }

    [Fact]
    public async Task Query_Test_038()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$count=true&$filter=startswith(toupper(title), 'THE')",
            63,
            null,
            63,
            new[] { "id-000", "id-001", "id-002", "id-004", "id-006" }
        );
    }

    [Fact]
    public async Task Query_Test_039()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$count=true&$filter=year eq 1994",
            5,
            null,
            5,
            new[] { "id-000", "id-003", "id-018", "id-030", "id-079" }
        );
    }

    [Fact]
    public async Task Query_Test_040()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$count=true&$filter=year ge 2000 and year le 2009",
            55,
            null,
            55,
            new[] { "id-006", "id-008", "id-012", "id-019", "id-020" }
        );
    }

    [Fact]
    public async Task Query_Test_041()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$count=true&$filter=year ge 2000",
            69,
            null,
            69,
            new[] { "id-006", "id-008", "id-012", "id-013", "id-019" }
        );
    }

    [Fact]
    public async Task Query_Test_042()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$count=true&$filter=year gt 1999 and year lt 2010",
            55,
            null,
            55,
            new[] { "id-006", "id-008", "id-012", "id-019", "id-020" }
        );
    }

    [Fact]
    public async Task Query_Test_043()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$count=true&$filter=year gt 1999",
            69,
            null,
            69,
            new[] { "id-006", "id-008", "id-012", "id-013", "id-019" }
        );
    }

    [Fact]
    public async Task Query_Test_044()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$count=true&$filter=year le 2000",
            100,
            "$count=true&$filter=year le 2000&$skip=100",
            185,
            new[] { "id-000", "id-001", "id-002", "id-003", "id-004" }
        );
    }

    [Fact]
    public async Task Query_Test_045()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$count=true&$filter=year lt 2001",
            100,
            "$count=true&$filter=year lt 2001&$skip=100",
            185,
            new[] { "id-000", "id-001", "id-002", "id-003", "id-004" }
        );
    }

    [Fact]
    public async Task Query_Test_046()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$count=true&$filter=year(releaseDate) eq 1994",
            6,
            null,
            6,
            new[] { "id-000", "id-003", "id-018", "id-030", "id-079" }
        );
    }

    [Fact]
    public async Task Query_Test_047()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$count=true&$orderby=bestPictureWinner asc",
            100,
            "$count=true&$orderby=bestPictureWinner asc&$skip=100",
            factory.Count<InMemoryMovie>(),
            new[] { "id-000", "id-003", "id-004", "id-005", "id-006" }
        );
    }

    [Fact]
    public async Task Query_Test_048()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$count=true&$orderby=bestPictureWinner desc",
            100,
            "$count=true&$orderby=bestPictureWinner desc&$skip=100",
            factory.Count<InMemoryMovie>(),
            new[] { "id-001", "id-002", "id-007", "id-008", "id-011" }
        );
    }

    [Fact]
    public async Task Query_Test_049()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$count=true&$orderby=duration asc",
            100,
            "$count=true&$orderby=duration asc&$skip=100",
            factory.Count<InMemoryMovie>(),
            new[] { "id-227", "id-125", "id-133", "id-107", "id-118" }
        );
    }

    [Fact]
    public async Task Query_Test_050()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$count=true&$orderby=duration desc",
            100,
            "$count=true&$orderby=duration desc&$skip=100",
            factory.Count<InMemoryMovie>(),
            new[] { "id-153", "id-065", "id-165", "id-008", "id-002" }
        );
    }

    [Fact]
    public async Task Query_Test_051()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$count=true&$orderby=rating asc",
            100,
            "$count=true&$orderby=rating asc&$skip=100",
            factory.Count<InMemoryMovie>(),
            new[] { "id-004", "id-005", "id-011", "id-016", "id-031" }
        );
    }

    [Fact]
    public async Task Query_Test_052()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$count=true&$orderby=rating desc",
            100,
            "$count=true&$orderby=rating desc&$skip=100",
            factory.Count<InMemoryMovie>(),
            new[] { "id-000", "id-001", "id-002", "id-003", "id-007" }
        );
    }

    [Fact]
    public async Task Query_Test_053()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$count=true&$orderby=releaseDate asc",
            100,
            "$count=true&$orderby=releaseDate asc&$skip=100",
            factory.Count<InMemoryMovie>(),
            new[] { "id-125", "id-133", "id-227", "id-118", "id-088" }
        );
    }

    [Fact]
    public async Task Query_Test_054()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$count=true&$orderby=releaseDate desc",
            100,
            "$count=true&$orderby=releaseDate desc&$skip=100",
            factory.Count<InMemoryMovie>(),
            new[] { "id-188", "id-033", "id-122", "id-186", "id-064" }
        );
    }

    [Fact]
    public async Task Query_Test_055()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$count=true&$orderby=title asc",
            100,
            "$count=true&$orderby=title asc&$skip=100",
            factory.Count<InMemoryMovie>(),
            new[] { "id-005", "id-091", "id-243", "id-194", "id-060" }
        );
    }

    [Fact]
    public async Task Query_Test_056()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$count=true&$orderby=title desc",
            100,
            "$count=true&$orderby=title desc&$skip=100",
            factory.Count<InMemoryMovie>(),
            new[] { "id-107", "id-100", "id-123", "id-190", "id-149" }
        );
    }

    [Fact]
    public async Task Query_Test_057()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$count=true&$orderby=year asc",
            100,
            "$count=true&$orderby=year asc&$skip=100",
            factory.Count<InMemoryMovie>(),
            new[] { "id-125", "id-229", "id-133", "id-227", "id-118" }
        );
    }

    [Fact]
    public async Task Query_Test_058()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$count=true&$orderby=year asc,title asc",
            100,
            "$count=true&$orderby=year asc,title asc&$skip=100",
            factory.Count<InMemoryMovie>(),
            new[] { "id-125", "id-229", "id-227", "id-133", "id-118" }
        );
    }

    [Fact]
    public async Task Query_Test_059()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$count=true&$orderby=year asc,title desc",
            100,
            "$count=true&$orderby=year asc,title desc&$skip=100",
            factory.Count<InMemoryMovie>(),
            new[] { "id-125", "id-229", "id-133", "id-227", "id-118" }
        );
    }

    [Fact]
    public async Task Query_Test_060()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$count=true&$orderby=year desc",
            100,
            "$count=true&$orderby=year desc&$skip=100",
            factory.Count<InMemoryMovie>(),
            new[] { "id-033", "id-122", "id-188", "id-064", "id-102" }
        );
    }

    [Fact]
    public async Task Query_Test_061()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$count=true&$orderby=year desc,title asc",
            100,
            "$count=true&$orderby=year desc,title asc&$skip=100",
            factory.Count<InMemoryMovie>(),
            new[] { "id-188", "id-122", "id-033", "id-102", "id-213" }
        );
    }

    [Fact]
    public async Task Query_Test_062()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$count=true&$orderby=year desc,title desc",
            100,
            "$count=true&$orderby=year desc,title desc&$skip=100",
            factory.Count<InMemoryMovie>(),
            new[] { "id-033", "id-122", "id-188", "id-149", "id-064" }
        );
    }

    [Fact]
    public async Task Query_Test_063()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$count=true&$top=125&$filter=((year div 1000.5) eq 2) and (rating eq 'R')",
            2,
            null,
            2,
            new[] { "id-061", "id-173" }
        );
    }

    [Fact]
    public async Task Query_Test_064()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$count=true&$top=125&$filter=((year sub 1900) ge 80) and ((year add 10) le 2000) and (duration le 120)",
            13,
            null,
            13,
            new[] { "id-026", "id-047", "id-081", "id-103", "id-121" }
        );
    }

    [Fact]
    public async Task Query_Test_065()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$count=true&$top=125&$filter=(year div 1000.5) eq 2",
            6,
            null,
            6,
            new[] { "id-012", "id-042", "id-061", "id-173", "id-194" }
        );
    }

    [Fact]
    public async Task Query_Test_066()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$count=true&$top=125&$filter=(year ge 1930 and year le 1940) or (year ge 1950 and year le 1960)",
            46,
            null,
            46,
            new[] { "id-005", "id-016", "id-027", "id-028", "id-031" }
        );
    }

    [Fact]
    public async Task Query_Test_067()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$count=true&$top=125&$filter=(year sub 1900) ge 80",
            100,
            "$count=true&$filter=(year sub 1900) ge 80&$skip=100&$top=25",
            138,
            new[] { "id-000", "id-003", "id-006", "id-007", "id-008" }
        );
    }

    [Fact]
    public async Task Query_Test_068()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$count=true&$top=125&$filter=bestPictureWinner eq false",
            100,
            "$count=true&$filter=bestPictureWinner eq false&$skip=100&$top=25",
            210,
            new[] { "id-000", "id-003", "id-004", "id-005", "id-006" }
        );
    }

    [Fact]
    public async Task Query_Test_069()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$count=true&$top=125&$filter=bestPictureWinner eq true and ceiling(duration div 60.0) eq 2",
            11,
            null,
            11,
            new[] { "id-023", "id-024", "id-112", "id-135", "id-142" }
        );
    }

    [Fact]
    public async Task Query_Test_070()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$count=true&$top=125&$filter=bestPictureWinner eq true and floor(duration div 60.0) eq 2",
            21,
            null,
            21,
            new[] { "id-001", "id-011", "id-018", "id-048", "id-051" }
        );
    }

    [Fact]
    public async Task Query_Test_071()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$count=true&$top=125&$filter=bestPictureWinner eq true and round(duration div 60.0) eq 2",
            24,
            null,
            24,
            new[] { "id-011", "id-018", "id-023", "id-024", "id-048" }
        );
    }

    [Fact]
    public async Task Query_Test_072()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$count=true&$top=125&$filter=bestPictureWinner eq true",
            38,
            null,
            38,
            new[] { "id-001", "id-002", "id-007", "id-008", "id-011" }
        );
    }

    [Fact]
    public async Task Query_Test_073()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$count=true&$top=125&$filter=bestPictureWinner ne false",
            38,
            null,
            38,
            new[] { "id-001", "id-002", "id-007", "id-008", "id-011" }
        );
    }

    [Fact]
    public async Task Query_Test_074()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$count=true&$top=125&$filter=bestPictureWinner ne true",
            100,
            "$count=true&$filter=bestPictureWinner ne true&$skip=100&$top=25",
            210,
            new[] { "id-000", "id-003", "id-004", "id-005", "id-006" }
        );
    }

    [Fact]
    public async Task Query_Test_075()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$count=true&$top=125&$filter=ceiling(duration div 60.0) eq 2",
            100,
            "$count=true&$filter=ceiling(duration div 60.0) eq 2&$skip=100&$top=25",
            124,
            new[] { "id-005", "id-023", "id-024", "id-025", "id-026" }
        );
    }

    [Fact]
    public async Task Query_Test_076()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$count=true&$top=125&$filter=day(releaseDate) eq 1",
            7,
            null,
            7,
            new[] { "id-019", "id-048", "id-129", "id-131", "id-132" }
        );
    }

    [Fact]
    public async Task Query_Test_077()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$count=true&$top=125&$filter=duration ge 60",
            100,
            "$count=true&$filter=duration ge 60&$skip=100&$top=25",
            factory.Count<InMemoryMovie>(),
            new[] { "id-000", "id-001", "id-002", "id-003", "id-004" }
        );
    }

    [Fact]
    public async Task Query_Test_078()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$count=true&$top=125&$filter=endswith(title, 'er')",
            12,
            null,
            12,
            new[] { "id-001", "id-052", "id-121", "id-130", "id-164" }
        );
    }

    [Fact]
    public async Task Query_Test_079()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$count=true&$top=125&$filter=endswith(tolower(title), 'er')",
            12,
            null,
            12,
            new[] { "id-001", "id-052", "id-121", "id-130", "id-164" }
        );
    }

    [Fact]
    public async Task Query_Test_080()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$count=true&$top=125&$filter=endswith(toupper(title), 'ER')",
            12,
            null,
            12,
            new[] { "id-001", "id-052", "id-121", "id-130", "id-164" }
        );
    }

    [Fact]
    public async Task Query_Test_081()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$count=true&$top=125&$filter=floor(duration div 60.0) eq 2",
            100,
            "$count=true&$filter=floor(duration div 60.0) eq 2&$skip=100&$top=25",
            120,
            new[] { "id-000", "id-001", "id-003", "id-004", "id-006" }
        );
    }

    [Fact]
    public async Task Query_Test_082()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$count=true&$top=125&$filter=month(releaseDate) eq 11",
            14,
            null,
            14,
            new[] { "id-011", "id-016", "id-030", "id-064", "id-085" }
        );
    }

    [Fact]
    public async Task Query_Test_083()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$count=true&$top=125&$filter=not(bestPictureWinner eq false)",
            38,
            null,
            38,
            new[] { "id-001", "id-002", "id-007", "id-008", "id-011" }
        );
    }

    [Fact]
    public async Task Query_Test_084()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$count=true&$top=125&$filter=not(bestPictureWinner eq true)",
            100,
            "$count=true&$filter=not(bestPictureWinner eq true)&$skip=100&$top=25",
            210,
            new[] { "id-000", "id-003", "id-004", "id-005", "id-006" }
        );
    }

    [Fact]
    public async Task Query_Test_085()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$count=true&$top=125&$filter=not(bestPictureWinner ne false)",
            100,
            "$count=true&$filter=not(bestPictureWinner ne false)&$skip=100&$top=25",
            210,
            new[] { "id-000", "id-003", "id-004", "id-005", "id-006" }
        );
    }

    [Fact]
    public async Task Query_Test_086()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$count=true&$top=125&$filter=not(bestPictureWinner ne true)",
            38,
            null,
            38,
            new[] { "id-001", "id-002", "id-007", "id-008", "id-011" }
        );
    }

    [Fact]
    public async Task Query_Test_087()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$count=true&$top=125&$filter=rating eq 'R'",
            95,
            null,
            95,
            new[] { "id-000", "id-001", "id-002", "id-003", "id-007" }
        );
    }

    [Fact]
    public async Task Query_Test_088()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$count=true&$top=125&$filter=rating ne 'PG13'",
            100,
            "$count=true&$filter=rating ne 'PG13'&$skip=100&$top=25",
            220,
            new[] { "id-000", "id-001", "id-002", "id-003", "id-004" }
        );
    }

    [Fact]
    public async Task Query_Test_089()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$count=true&$top=125&$filter=rating eq 'Unrated'",
            74,
            null,
            74,
            new[] { "id-004", "id-005", "id-011", "id-016", "id-031" }
        );
    }

    [Fact]
    public async Task Query_Test_090()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$count=true&$top=125&$filter=releaseDate eq cast(1994-10-14,Edm.Date)",
            2,
            null,
            2,
            new[] { "id-000", "id-003" }
        );
    }

    [Fact]
    public async Task Query_Test_091()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$count=true&$top=125&$filter=releaseDate ge cast(1999-12-31,Edm.Date)",
            69,
            null,
            69,
            new[] { "id-006", "id-008", "id-012", "id-013", "id-019" }
        );
    }

    [Fact]
    public async Task Query_Test_092()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$count=true&$top=125&$filter=releaseDate gt cast(1999-12-31,Edm.Date)",
            69,
            null,
            69,
            new[] { "id-006", "id-008", "id-012", "id-013", "id-019" }
        );
    }

    [Fact]
    public async Task Query_Test_093()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$count=true&$top=125&$filter=releaseDate le cast(2000-01-01,Edm.Date)",
            100,
            "$count=true&$filter=releaseDate le cast(2000-01-01,Edm.Date)&$skip=100&$top=25",
            179,
            new[] { "id-000", "id-001", "id-002", "id-003", "id-004" }
        );
    }

    [Fact]
    public async Task Query_Test_094()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$count=true&$top=125&$filter=releaseDate lt cast(2000-01-01,Edm.Date)",
            100,
            "$count=true&$filter=releaseDate lt cast(2000-01-01,Edm.Date)&$skip=100&$top=25",
            179,
            new[] { "id-000", "id-001", "id-002", "id-003", "id-004" }
        );
    }

    [Fact]
    public async Task Query_Test_095()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$count=true&$top=125&$filter=round(duration div 60.0) eq 2",
            100,
            "$count=true&$filter=round(duration div 60.0) eq 2&$skip=100&$top=25",
            186,
            new[] { "id-000", "id-005", "id-009", "id-010", "id-011" }
        );
    }

    [Fact]
    public async Task Query_Test_097()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$count=true&$top=125&$filter=startswith(tolower(title), 'the')",
            63,
            null,
            63,
            new[] { "id-000", "id-001", "id-002", "id-004", "id-006" }
        );
    }

    [Fact]
    public async Task Query_Test_098()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$count=true&$top=125&$filter=startswith(toupper(title), 'THE')",
            63,
            null,
            63,
            new[] { "id-000", "id-001", "id-002", "id-004", "id-006" }
        );
    }

    [Fact]
    public async Task Query_Test_099()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$count=true&$top=125&$filter=year eq 1994",
            5,
            null,
            5,
            new[] { "id-000", "id-003", "id-018", "id-030", "id-079" }
        );
    }

    [Fact]
    public async Task Query_Test_100()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$count=true&$top=125&$filter=year ge 2000 and year le 2009",
            55,
            null,
            55,
            new[] { "id-006", "id-008", "id-012", "id-019", "id-020" }
        );
    }

    [Fact]
    public async Task Query_Test_101()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$count=true&$top=125&$filter=year ge 2000",
            69,
            null,
            69,
            new[] { "id-006", "id-008", "id-012", "id-013", "id-019" }
        );
    }

    [Fact]
    public async Task Query_Test_102()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$count=true&$top=125&$filter=year gt 1999 and year lt 2010",
            55,
            null,
            55,
            new[] { "id-006", "id-008", "id-012", "id-019", "id-020" }
        );
    }

    [Fact]
    public async Task Query_Test_103()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$count=true&$top=125&$filter=year gt 1999",
            69,
            null,
            69,
            new[] { "id-006", "id-008", "id-012", "id-013", "id-019" }
        );
    }

    [Fact]
    public async Task Query_Test_104()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$count=true&$top=125&$filter=year le 2000",
            100,
            "$count=true&$filter=year le 2000&$skip=100&$top=25",
            185,
            new[] { "id-000", "id-001", "id-002", "id-003", "id-004" }
        );
    }

    [Fact]
    public async Task Query_Test_105()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$count=true&$top=125&$filter=year lt 2001",
            100,
            "$count=true&$filter=year lt 2001&$skip=100&$top=25",
            185,
            new[] { "id-000", "id-001", "id-002", "id-003", "id-004" }
        );
    }

    [Fact]
    public async Task Query_Test_106()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$count=true&$top=125&$filter=year(releaseDate) eq 1994",
            6,
            null,
            6,
            new[] { "id-000", "id-003", "id-018", "id-030", "id-079" }
        );
    }

    [Fact]
    public async Task Query_Test_107()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$count=true&$top=125&$orderby=bestPictureWinner asc",
            100,
            "$count=true&$orderby=bestPictureWinner asc&$skip=100&$top=25",
            factory.Count<InMemoryMovie>(),
            new[] { "id-000", "id-003", "id-004", "id-005", "id-006" }
        );
    }

    [Fact]
    public async Task Query_Test_108()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$count=true&$top=125&$orderby=bestPictureWinner desc",
            100,
            "$count=true&$orderby=bestPictureWinner desc&$skip=100&$top=25",
            factory.Count<InMemoryMovie>(),
            new[] { "id-001", "id-002", "id-007", "id-008", "id-011" }
        );
    }

    [Fact]
    public async Task Query_Test_109()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$count=true&$top=125&$orderby=duration asc",
            100,
            "$count=true&$orderby=duration asc&$skip=100&$top=25",
            factory.Count<InMemoryMovie>(),
            new[] { "id-227", "id-125", "id-133", "id-107", "id-118" }
        );
    }

    [Fact]
    public async Task Query_Test_110()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$count=true&$top=125&$orderby=duration desc",
            100,
            "$count=true&$orderby=duration desc&$skip=100&$top=25",
            factory.Count<InMemoryMovie>(),
            new[] { "id-153", "id-065", "id-165", "id-008", "id-002" }
        );
    }

    [Fact]
    public async Task Query_Test_111()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$count=true&$top=125&$orderby=rating asc",
            100,
            "$count=true&$orderby=rating asc&$skip=100&$top=25",
            factory.Count<InMemoryMovie>(),
            new[] { "id-004", "id-005", "id-011", "id-016", "id-031" }
        );
    }

    [Fact]
    public async Task Query_Test_112()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$count=true&$top=125&$orderby=rating desc",
            100,
            "$count=true&$orderby=rating desc&$skip=100&$top=25",
            factory.Count<InMemoryMovie>(),
            new[] { "id-000", "id-001", "id-002", "id-003", "id-007" }
        );
    }

    [Fact]
    public async Task Query_Test_113()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$count=true&$top=125&$orderby=releaseDate asc",
            100,
            "$count=true&$orderby=releaseDate asc&$skip=100&$top=25",
            factory.Count<InMemoryMovie>(),
            new[] { "id-125", "id-133", "id-227", "id-118", "id-088" }
        );
    }

    [Fact]
    public async Task Query_Test_114()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$count=true&$top=125&$orderby=releaseDate desc",
            100,
            "$count=true&$orderby=releaseDate desc&$skip=100&$top=25",
            factory.Count<InMemoryMovie>(),
            new[] { "id-188", "id-033", "id-122", "id-186", "id-064" }
        );
    }

    [Fact]
    public async Task Query_Test_115()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$count=true&$top=125&$orderby=title asc",
            100,
            "$count=true&$orderby=title asc&$skip=100&$top=25",
            factory.Count<InMemoryMovie>(),
            new[] { "id-005", "id-091", "id-243", "id-194", "id-060" }
        );
    }

    [Fact]
    public async Task Query_Test_116()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$count=true&$top=125&$orderby=title desc",
            100,
            "$count=true&$orderby=title desc&$skip=100&$top=25",
            factory.Count<InMemoryMovie>(),
            new[] { "id-107", "id-100", "id-123", "id-190", "id-149" }
        );
    }

    [Fact]
    public async Task Query_Test_117()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$count=true&$top=125&$orderby=year asc",
            100,
            "$count=true&$orderby=year asc&$skip=100&$top=25",
            factory.Count<InMemoryMovie>(),
            new[] { "id-125", "id-229", "id-133", "id-227", "id-118" }
        );
    }

    [Fact]
    public async Task Query_Test_118()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$count=true&$top=125&$orderby=year asc,title asc",
            100,
            "$count=true&$orderby=year asc,title asc&$skip=100&$top=25",
            factory.Count<InMemoryMovie>(),
            new[] { "id-125", "id-229", "id-227", "id-133", "id-118" }
        );
    }

    [Fact]
    public async Task Query_Test_119()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$count=true&$top=125&$orderby=year asc,title desc",
            100,
            "$count=true&$orderby=year asc,title desc&$skip=100&$top=25",
            factory.Count<InMemoryMovie>(),
            new[] { "id-125", "id-229", "id-133", "id-227", "id-118" }
        );
    }

    [Fact]
    public async Task Query_Test_120()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$count=true&$top=125&$orderby=year desc",
            100,
            "$count=true&$orderby=year desc&$skip=100&$top=25",
            factory.Count<InMemoryMovie>(),
            new[] { "id-033", "id-122", "id-188", "id-064", "id-102" }
        );
    }

    [Fact]
    public async Task Query_Test_121()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$count=true&$top=125&$orderby=year desc,title asc",
            100,
            "$count=true&$orderby=year desc,title asc&$skip=100&$top=25",
            factory.Count<InMemoryMovie>(),
            new[] { "id-188", "id-122", "id-033", "id-102", "id-213" }
        );
    }

    [Fact]
    public async Task Query_Test_122()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$count=true&$top=125&$orderby=year desc,title desc",
            100,
            "$count=true&$orderby=year desc,title desc&$skip=100&$top=25",
            factory.Count<InMemoryMovie>(),
            new[] { "id-033", "id-122", "id-188", "id-149", "id-064" }
        );
    }

    [Fact]
    public async Task Query_Test_123()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$filter=((year div 1000.5) eq 2) and (rating eq 'R')",
            2,
            null,
            null,
            new[] { "id-061", "id-173" }
        );
    }

    [Fact]
    public async Task Query_Test_124()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$filter=((year sub 1900) ge 80) and ((year add 10) le 2000) and (duration le 120)",
            13,
            null,
            null,
            new[] { "id-026", "id-047", "id-081", "id-103", "id-121" }
        );
    }

    [Fact]
    public async Task Query_Test_125()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$filter=(year div 1000.5) eq 2",
            6,
            null,
            null,
            new[] { "id-012", "id-042", "id-061", "id-173", "id-194" }
        );
    }

    [Fact]
    public async Task Query_Test_126()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$filter=(year ge 1930 and year le 1940) or (year ge 1950 and year le 1960)",
            46,
            null,
            null,
            new[] { "id-005", "id-016", "id-027", "id-028", "id-031" }
        );
    }

    [Fact]
    public async Task Query_Test_127()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$filter=(year sub 1900) ge 80",
            100,
            "$filter=(year sub 1900) ge 80&$skip=100",
            null,
            new[] { "id-000", "id-003", "id-006", "id-007", "id-008" }
        );
    }

    [Fact]
    public async Task Query_Test_128()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$filter=bestPictureWinner eq false",
            100,
            "$filter=bestPictureWinner eq false&$skip=100",
            null,
            new[] { "id-000", "id-003", "id-004", "id-005", "id-006" }
        );
    }

    [Fact]
    public async Task Query_Test_129()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$filter=bestPictureWinner eq true and ceiling(duration div 60.0) eq 2",
            11,
            null,
            null,
            new[] { "id-023", "id-024", "id-112", "id-135", "id-142" }
        );
    }

    [Fact]
    public async Task Query_Test_130()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$filter=bestPictureWinner eq true and floor(duration div 60.0) eq 2",
            21,
            null,
            null,
            new[] { "id-001", "id-011", "id-018", "id-048", "id-051" }
        );
    }

    [Fact]
    public async Task Query_Test_131()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$filter=bestPictureWinner eq true and round(duration div 60.0) eq 2",
            24,
            null,
            null,
            new[] { "id-011", "id-018", "id-023", "id-024", "id-048" }
        );
    }

    [Fact]
    public async Task Query_Test_132()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$filter=bestPictureWinner eq true",
            38,
            null,
            null,
            new[] { "id-001", "id-002", "id-007", "id-008", "id-011" }
        );
    }

    [Fact]
    public async Task Query_Test_133()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$filter=bestPictureWinner ne false",
            38,
            null,
            null,
            new[] { "id-001", "id-002", "id-007", "id-008", "id-011" }
        );
    }

    [Fact]
    public async Task Query_Test_134()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$filter=bestPictureWinner ne true",
            100,
            "$filter=bestPictureWinner ne true&$skip=100",
            null,
            new[] { "id-000", "id-003", "id-004", "id-005", "id-006" }
        );
    }

    [Fact]
    public async Task Query_Test_135()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$filter=ceiling(duration div 60.0) eq 2",
            100,
            "$filter=ceiling(duration div 60.0) eq 2&$skip=100",
            null,
            new[] { "id-005", "id-023", "id-024", "id-025", "id-026" }
        );
    }

    [Fact]
    public async Task Query_Test_136()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$filter=day(releaseDate) eq 1",
            7,
            null,
            null,
            new[] { "id-019", "id-048", "id-129", "id-131", "id-132" }
        );
    }

    [Fact]
    public async Task Query_Test_137()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$filter=duration ge 60",
            100,
            "$filter=duration ge 60&$skip=100",
            null,
            new[] { "id-000", "id-001", "id-002", "id-003", "id-004" }
        );
    }

    [Fact]
    public async Task Query_Test_138()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$filter=endswith(title, 'er')",
            12,
            null,
            null,
            new[] { "id-001", "id-052", "id-121", "id-130", "id-164" }
        );
    }

    [Fact]
    public async Task Query_Test_139()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$filter=endswith(tolower(title), 'er')",
            12,
            null,
            null,
            new[] { "id-001", "id-052", "id-121", "id-130", "id-164" }
        );
    }

    [Fact]
    public async Task Query_Test_140()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$filter=endswith(toupper(title), 'ER')",
            12,
            null,
            null,
            new[] { "id-001", "id-052", "id-121", "id-130", "id-164" }
        );
    }

    [Fact]
    public async Task Query_Test_141()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$filter=floor(duration div 60.0) eq 2",
            100,
            "$filter=floor(duration div 60.0) eq 2&$skip=100",
            null,
            new[] { "id-000", "id-001", "id-003", "id-004", "id-006" }
        );
    }

    [Fact]
    public async Task Query_Test_142()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$filter=month(releaseDate) eq 11",
            14,
            null,
            null,
            new[] { "id-011", "id-016", "id-030", "id-064", "id-085" }
        );
    }

    [Fact]
    public async Task Query_Test_143()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$filter=not(bestPictureWinner eq false)",
            38,
            null,
            null,
            new[] { "id-001", "id-002", "id-007", "id-008", "id-011" }
        );
    }

    [Fact]
    public async Task Query_Test_144()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$filter=not(bestPictureWinner eq true)",
            100,
            "$filter=not(bestPictureWinner eq true)&$skip=100",
            null,
            new[] { "id-000", "id-003", "id-004", "id-005", "id-006" }
        );
    }

    [Fact]
    public async Task Query_Test_145()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$filter=not(bestPictureWinner ne false)",
            100,
            "$filter=not(bestPictureWinner ne false)&$skip=100",
            null,
            new[] { "id-000", "id-003", "id-004", "id-005", "id-006" }
        );
    }

    [Fact]
    public async Task Query_Test_146()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$filter=not(bestPictureWinner ne true)",
            38,
            null,
            null,
            new[] { "id-001", "id-002", "id-007", "id-008", "id-011" }
        );
    }

    [Fact]
    public async Task Query_Test_147()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$filter=rating eq 'R'",
            95,
            null,
            null,
            new[] { "id-000", "id-001", "id-002", "id-003", "id-007" }
        );
    }

    [Fact]
    public async Task Query_Test_148()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$filter=rating ne 'PG13'",
            100,
            "$filter=rating ne 'PG13'&$skip=100",
            null,
            new[] { "id-000", "id-001", "id-002", "id-003", "id-004" }
        );
    }

    [Fact]
    public async Task Query_Test_149()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$filter=rating eq 'Unrated'",
            74,
            null,
            null,
            new[] { "id-004", "id-005", "id-011", "id-016", "id-031" }
        );
    }

    [Fact]
    public async Task Query_Test_150()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$filter=releaseDate eq cast(1994-10-14,Edm.Date)",
            2,
            null,
            null,
            new[] { "id-000", "id-003" }
        );
    }

    [Fact]
    public async Task Query_Test_151()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$filter=releaseDate ge cast(1999-12-31,Edm.Date)",
            69,
            null,
            null,
            new[] { "id-006", "id-008", "id-012", "id-013", "id-019" }
        );
    }

    [Fact]
    public async Task Query_Test_152()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$filter=releaseDate gt cast(1999-12-31,Edm.Date)",
            69,
            null,
            null,
            new[] { "id-006", "id-008", "id-012", "id-013", "id-019" }
        );
    }

    [Fact]
    public async Task Query_Test_153()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$filter=releaseDate le cast(2000-01-01,Edm.Date)",
            100,
            "$filter=releaseDate le cast(2000-01-01,Edm.Date)&$skip=100",
            null,
            new[] { "id-000", "id-001", "id-002", "id-003", "id-004" }
        );
    }

    [Fact]
    public async Task Query_Test_154()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$filter=releaseDate lt cast(2000-01-01,Edm.Date)",
            100,
            "$filter=releaseDate lt cast(2000-01-01,Edm.Date)&$skip=100",
            null,
            new[] { "id-000", "id-001", "id-002", "id-003", "id-004" }
        );
    }

    [Fact]
    public async Task Query_Test_155()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$filter=round(duration div 60.0) eq 2",
            100,
            "$filter=round(duration div 60.0) eq 2&$skip=100",
            null,
            new[] { "id-000", "id-005", "id-009", "id-010", "id-011" }
        );
    }

    [Fact]
    public async Task Query_Test_157()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$filter=startswith(tolower(title), 'the')",
            63,
            null,
            null,
            new[] { "id-000", "id-001", "id-002", "id-004", "id-006" }
        );
    }

    [Fact]
    public async Task Query_Test_158()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$filter=startswith(toupper(title), 'THE')",
            63,
            null,
            null,
            new[] { "id-000", "id-001", "id-002", "id-004", "id-006" }
        );
    }

    [Fact]
    public async Task Query_Test_159()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$filter=year eq 1994",
            5,
            null,
            null,
            new[] { "id-000", "id-003", "id-018", "id-030", "id-079" }
        );
    }

    [Fact]
    public async Task Query_Test_160()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$filter=year ge 2000 and year le 2009",
            55,
            null,
            null,
            new[] { "id-006", "id-008", "id-012", "id-019", "id-020" }
        );
    }

    [Fact]
    public async Task Query_Test_161()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$filter=year ge 2000",
            69,
            null,
            null,
            new[] { "id-006", "id-008", "id-012", "id-013", "id-019" }
        );
    }

    [Fact]
    public async Task Query_Test_162()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$filter=year gt 1999 and year lt 2010",
            55,
            null,
            null,
            new[] { "id-006", "id-008", "id-012", "id-019", "id-020" }
        );
    }

    [Fact]
    public async Task Query_Test_163()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$filter=year gt 1999",
            69,
            null,
            null,
            new[] { "id-006", "id-008", "id-012", "id-013", "id-019" }
        );
    }

    [Fact]
    public async Task Query_Test_164()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$filter=year le 2000",
            100,
            "$filter=year le 2000&$skip=100",
            null,
            new[] { "id-000", "id-001", "id-002", "id-003", "id-004" }
        );
    }

    [Fact]
    public async Task Query_Test_165()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$filter=year lt 2001",
            100,
            "$filter=year lt 2001&$skip=100",
            null,
            new[] { "id-000", "id-001", "id-002", "id-003", "id-004" }
        );
    }

    [Fact]
    public async Task Query_Test_166()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$filter=year(releaseDate) eq 1994",
            6,
            null,
            null,
            new[] { "id-000", "id-003", "id-018", "id-030", "id-079" }
        );
    }

    [Fact]
    public async Task Query_Test_167()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$orderby=bestPictureWinner asc",
            100,
            "$orderby=bestPictureWinner asc&$skip=100",
            null,
            new[] { "id-000", "id-003", "id-004", "id-005", "id-006" }
        );
    }

    [Fact]
    public async Task Query_Test_168()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$orderby=bestPictureWinner desc",
            100,
            "$orderby=bestPictureWinner desc&$skip=100",
            null,
            new[] { "id-001", "id-002", "id-007", "id-008", "id-011" }
        );
    }

    [Fact]
    public async Task Query_Test_169()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$orderby=duration asc",
            100,
            "$orderby=duration asc&$skip=100",
            null,
            new[] { "id-227", "id-125", "id-133", "id-107", "id-118" }
        );
    }

    [Fact]
    public async Task Query_Test_170()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$orderby=duration desc",
            100,
            "$orderby=duration desc&$skip=100",
            null,
            new[] { "id-153", "id-065", "id-165", "id-008", "id-002" }
        );
    }

    [Fact]
    public async Task Query_Test_171()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$orderby=rating asc",
            100,
            "$orderby=rating asc&$skip=100",
            null,
            new[] { "id-004", "id-005", "id-011", "id-016", "id-031" }
        );
    }

    [Fact]
    public async Task Query_Test_172()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$orderby=rating desc",
            100,
            "$orderby=rating desc&$skip=100",
            null,
            new[] { "id-000", "id-001", "id-002", "id-003", "id-007" }
        );
    }

    [Fact]
    public async Task Query_Test_173()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$orderby=releaseDate asc",
            100,
            "$orderby=releaseDate asc&$skip=100",
            null,
            new[] { "id-125", "id-133", "id-227", "id-118", "id-088" }
        );
    }

    [Fact]
    public async Task Query_Test_174()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$orderby=releaseDate desc",
            100,
            "$orderby=releaseDate desc&$skip=100",
            null,
            new[] { "id-188", "id-033", "id-122", "id-186", "id-064" }
        );
    }

    [Fact]
    public async Task Query_Test_175()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$orderby=title asc",
            100,
            "$orderby=title asc&$skip=100",
            null,
            new[] { "id-005", "id-091", "id-243", "id-194", "id-060" }
        );
    }

    [Fact]
    public async Task Query_Test_176()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$orderby=title desc",
            100,
            "$orderby=title desc&$skip=100",
            null,
            new[] { "id-107", "id-100", "id-123", "id-190", "id-149" }
        );
    }

    [Fact]
    public async Task Query_Test_177()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$orderby=year asc",
            100,
            "$orderby=year asc&$skip=100",
            null,
            new[] { "id-125", "id-229", "id-133", "id-227", "id-118" }
        );
    }

    [Fact]
    public async Task Query_Test_178()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$orderby=year asc,title asc",
            100,
            "$orderby=year asc,title asc&$skip=100",
            null,
            new[] { "id-125", "id-229", "id-227", "id-133", "id-118" }
        );
    }

    [Fact]
    public async Task Query_Test_179()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$orderby=year asc,title desc",
            100,
            "$orderby=year asc,title desc&$skip=100",
            null,
            new[] { "id-125", "id-229", "id-133", "id-227", "id-118" }
        );
    }

    [Fact]
    public async Task Query_Test_180()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$orderby=year desc",
            100,
            "$orderby=year desc&$skip=100",
            null,
            new[] { "id-033", "id-122", "id-188", "id-064", "id-102" }
        );
    }

    [Fact]
    public async Task Query_Test_181()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$orderby=year desc,title asc",
            100,
            "$orderby=year desc,title asc&$skip=100",
            null,
            new[] { "id-188", "id-122", "id-033", "id-102", "id-213" }
        );
    }

    [Fact]
    public async Task Query_Test_182()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$orderby=year desc,title desc",
            100,
            "$orderby=year desc,title desc&$skip=100",
            null,
            new[] { "id-033", "id-122", "id-188", "id-149", "id-064" }
        );
    }

    [Fact]
    public async Task Query_Test_183()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$filter=((year div 1000.5) eq 2) and (rating eq 'R')&$skip=5",
            0,
            null,
            null,
            Array.Empty<string>());
    }

    [Fact]
    public async Task Query_Test_184()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$filter=((year sub 1900) ge 80) and ((year add 10) le 2000) and (duration le 120)&$skip=5",
            8,
            null,
            null,
            new[] { "id-142", "id-143", "id-162", "id-166", "id-172" }
        );
    }

    [Fact]
    public async Task Query_Test_185()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$filter=(year div 1000.5) eq 2&$skip=5",
            1,
            null,
            null,
            new[] { "id-216" }
        );
    }

    [Fact]
    public async Task Query_Test_186()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$filter=(year ge 1930 and year le 1940) or (year ge 1950 and year le 1960)&$skip=5",
            41,
            null,
            null,
            new[] { "id-040", "id-041", "id-044", "id-046", "id-049" }
        );
    }

    [Fact]
    public async Task Query_Test_187()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$filter=(year sub 1900) ge 80&$skip=5",
            100,
            "$filter=(year sub 1900) ge 80&$skip=105",
            null,
            new[] { "id-009", "id-010", "id-012", "id-013", "id-014" }
        );
    }

    [Fact]
    public async Task Query_Test_188()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$filter=bestPictureWinner eq false&$skip=5",
            100,
            "$filter=bestPictureWinner eq false&$skip=105",
            null,
            new[] { "id-009", "id-010", "id-012", "id-013", "id-014" }
        );
    }

    [Fact]
    public async Task Query_Test_189()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$filter=bestPictureWinner eq true and ceiling(duration div 60.0) eq 2&$skip=5",
            6,
            null,
            null,
            new[] { "id-150", "id-155", "id-186", "id-189", "id-196" }
        );
    }

    [Fact]
    public async Task Query_Test_190()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$filter=bestPictureWinner eq true and floor(duration div 60.0) eq 2&$skip=5",
            16,
            null,
            null,
            new[] { "id-062", "id-083", "id-087", "id-092", "id-093" }
        );
    }

    [Fact]
    public async Task Query_Test_191()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$filter=bestPictureWinner eq true and round(duration div 60.0) eq 2&$skip=5",
            19,
            null,
            null,
            new[] { "id-092", "id-093", "id-094", "id-096", "id-112" }
        );
    }

    [Fact]
    public async Task Query_Test_192()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$filter=bestPictureWinner eq true&$skip=5",
            33,
            null,
            null,
            new[] { "id-018", "id-023", "id-024", "id-048", "id-051" }
        );
    }

    [Fact]
    public async Task Query_Test_193()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$filter=bestPictureWinner ne false&$skip=5",
            33,
            null,
            null,
            new[] { "id-018", "id-023", "id-024", "id-048", "id-051" }
        );
    }

    [Fact]
    public async Task Query_Test_194()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$filter=bestPictureWinner ne true&$skip=5",
            100,
            "$filter=bestPictureWinner ne true&$skip=105",
            null,
            new[] { "id-009", "id-010", "id-012", "id-013", "id-014" }
        );
    }

    [Fact]
    public async Task Query_Test_195()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$filter=ceiling(duration div 60.0) eq 2&$skip=5",
            100,
            "$filter=ceiling(duration div 60.0) eq 2&$skip=105",
            null,
            new[] { "id-027", "id-028", "id-030", "id-031", "id-032" }
        );
    }

    [Fact]
    public async Task Query_Test_196()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$filter=day(releaseDate) eq 1&$skip=5",
            2,
            null,
            null,
            new[] { "id-197", "id-215" }
        );
    }

    [Fact]
    public async Task Query_Test_197()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$filter=duration ge 60&$skip=5",
            100,
            "$filter=duration ge 60&$skip=105",
            null,
            new[] { "id-005", "id-006", "id-007", "id-008", "id-009" }
        );
    }

    [Fact]
    public async Task Query_Test_198()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$filter=endswith(title, 'er')&$skip=5",
            7,
            null,
            null,
            new[] { "id-170", "id-193", "id-197", "id-205", "id-217" }
        );
    }

    [Fact]
    public async Task Query_Test_199()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$filter=endswith(tolower(title), 'er')&$skip=5",
            7,
            null,
            null,
            new[] { "id-170", "id-193", "id-197", "id-205", "id-217" }
        );
    }

    [Fact]
    public async Task Query_Test_200()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$filter=endswith(toupper(title), 'ER')&$skip=5",
            7,
            null,
            null,
            new[] { "id-170", "id-193", "id-197", "id-205", "id-217" }
        );
    }

    [Fact]
    public async Task Query_Test_201()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$filter=floor(duration div 60.0) eq 2&$skip=5",
            100,
            "$filter=floor(duration div 60.0) eq 2&$skip=105",
            null,
            new[] { "id-009", "id-010", "id-011", "id-012", "id-013" }
        );
    }

    [Fact]
    public async Task Query_Test_202()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$filter=month(releaseDate) eq 11&$skip=5",
            9,
            null,
            null,
            new[] { "id-115", "id-131", "id-136", "id-146", "id-167" }
        );
    }

    [Fact]
    public async Task Query_Test_203()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$filter=not(bestPictureWinner eq false)&$skip=5",
            33,
            null,
            null,
            new[] { "id-018", "id-023", "id-024", "id-048", "id-051" }
        );
    }

    [Fact]
    public async Task Query_Test_204()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$filter=not(bestPictureWinner eq true)&$skip=5",
            100,
            "$filter=not(bestPictureWinner eq true)&$skip=105",
            null,
            new[] { "id-009", "id-010", "id-012", "id-013", "id-014" }
        );
    }

    [Fact]
    public async Task Query_Test_205()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$filter=not(bestPictureWinner ne false)&$skip=5",
            100,
            "$filter=not(bestPictureWinner ne false)&$skip=105",
            null,
            new[] { "id-009", "id-010", "id-012", "id-013", "id-014" }
        );
    }

    [Fact]
    public async Task Query_Test_206()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$filter=not(bestPictureWinner ne true)&$skip=5",
            33,
            null,
            null,
            new[] { "id-018", "id-023", "id-024", "id-048", "id-051" }
        );
    }

    [Fact]
    public async Task Query_Test_207()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$filter=rating eq 'Unrated'&$skip=5",
            69,
            null,
            null,
            new[] { "id-040", "id-041", "id-044", "id-046", "id-049" }
        );
    }

    [Fact]
    public async Task Query_Test_208()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$filter=rating eq 'R'&$skip=5",
            90,
            null,
            null,
            new[] { "id-009", "id-014", "id-017", "id-019", "id-022" }
        );
    }

    [Fact]
    public async Task Query_Test_209()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$filter=rating ne 'PG13'&$skip=5",
            100,
            "$filter=rating ne 'PG13'&$skip=105",
            null,
            new[] { "id-005", "id-007", "id-009", "id-010", "id-011" }
        );
    }

    [Fact]
    public async Task Query_Test_210()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$filter=releaseDate eq cast(1994-10-14,Edm.Date)&$skip=5",
            0,
            null,
            null,
            Array.Empty<string>());
    }

    [Fact]
    public async Task Query_Test_211()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$filter=releaseDate ge cast(1999-12-31,Edm.Date)&$skip=5",
            64,
            null,
            null,
            new[] { "id-020", "id-032", "id-033", "id-042", "id-050" }
        );
    }

    [Fact]
    public async Task Query_Test_212()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$filter=releaseDate gt cast(1999-12-31,Edm.Date)&$skip=5",
            64,
            null,
            null,
            new[] { "id-020", "id-032", "id-033", "id-042", "id-050" }
        );
    }

    [Fact]
    public async Task Query_Test_213()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$filter=releaseDate le cast(2000-01-01,Edm.Date)&$skip=5",
            100,
            "$filter=releaseDate le cast(2000-01-01,Edm.Date)&$skip=105",
            null,
            new[] { "id-005", "id-007", "id-009", "id-010", "id-011" }
        );
    }

    [Fact]
    public async Task Query_Test_214()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$filter=releaseDate lt cast(2000-01-01,Edm.Date)&$skip=5",
            100,
            "$filter=releaseDate lt cast(2000-01-01,Edm.Date)&$skip=105",
            null,
            new[] { "id-005", "id-007", "id-009", "id-010", "id-011" }
        );
    }

    [Fact]
    public async Task Query_Test_215()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$filter=round(duration div 60.0) eq 2&$skip=5",
            100,
            "$filter=round(duration div 60.0) eq 2&$skip=105",
            null,
            new[] { "id-013", "id-014", "id-015", "id-016", "id-017" }
        );
    }

    [Fact]
    public async Task Query_Test_217()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$filter=startswith(tolower(title), 'the')&$skip=5",
            58,
            null,
            null,
            new[] { "id-008", "id-012", "id-017", "id-020", "id-023" }
        );
    }

    [Fact]
    public async Task Query_Test_218()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$filter=startswith(toupper(title), 'THE')&$skip=5",
            58,
            null,
            null,
            new[] { "id-008", "id-012", "id-017", "id-020", "id-023" }
        );
    }

    [Fact]
    public async Task Query_Test_219()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$filter=year eq 1994&$skip=5",
            0,
            null,
            null,
            Array.Empty<string>());
    }

    [Fact]
    public async Task Query_Test_220()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$filter=year ge 2000 and year le 2009&$skip=5",
            50,
            null,
            null,
            new[] { "id-032", "id-042", "id-050", "id-051", "id-058" }
        );
    }

    [Fact]
    public async Task Query_Test_221()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$filter=year ge 2000&$skip=5",
            64,
            null,
            null,
            new[] { "id-020", "id-032", "id-033", "id-042", "id-050" }
        );
    }

    [Fact]
    public async Task Query_Test_222()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$filter=year gt 1999 and year lt 2010&$skip=5",
            50,
            null,
            null,
            new[] { "id-032", "id-042", "id-050", "id-051", "id-058" }
        );
    }

    [Fact]
    public async Task Query_Test_223()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$filter=year gt 1999&$skip=5",
            64,
            null,
            null,
            new[] { "id-020", "id-032", "id-033", "id-042", "id-050" }
        );
    }

    [Fact]
    public async Task Query_Test_224()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$filter=year le 2000&$skip=5",
            100,
            "$filter=year le 2000&$skip=105",
            null,
            new[] { "id-005", "id-007", "id-009", "id-010", "id-011" }
        );
    }

    [Fact]
    public async Task Query_Test_225()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$filter=year lt 2001&$skip=5",
            100,
            "$filter=year lt 2001&$skip=105",
            null,
            new[] { "id-005", "id-007", "id-009", "id-010", "id-011" }
        );
    }

    [Fact]
    public async Task Query_Test_226()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$filter=year(releaseDate) eq 1994&$skip=5",
            1,
            null,
            null,
            new[] { "id-217" }
        );
    }

    [Fact]
    public async Task Query_Test_227()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$orderby=bestPictureWinner asc&$skip=5",
            100,
            "$orderby=bestPictureWinner asc&$skip=105",
            null,
            new[] { "id-009", "id-010", "id-012", "id-013", "id-014" }
        );
    }

    [Fact]
    public async Task Query_Test_228()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$orderby=bestPictureWinner desc&$skip=5",
            100,
            "$orderby=bestPictureWinner desc&$skip=105",
            null,
            new[] { "id-018", "id-023", "id-024", "id-048", "id-051" }
        );
    }

    [Fact]
    public async Task Query_Test_229()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$orderby=duration asc&$skip=5",
            100,
            "$orderby=duration asc&$skip=105",
            null,
            new[] { "id-238", "id-201", "id-115", "id-229", "id-181" }
        );
    }

    [Fact]
    public async Task Query_Test_230()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$orderby=duration desc&$skip=5",
            100,
            "$orderby=duration desc&$skip=105",
            null,
            new[] { "id-007", "id-183", "id-063", "id-202", "id-130" }
        );
    }

    [Fact]
    public async Task Query_Test_231()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$orderby=rating asc&$skip=5",
            100,
            "$orderby=rating asc&$skip=105",
            null,
            new[] { "id-040", "id-041", "id-044", "id-046", "id-049" }
        );
    }

    [Fact]
    public async Task Query_Test_232()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$orderby=rating desc&$skip=5",
            100,
            "$orderby=rating desc&$skip=105",
            null,
            new[] { "id-009", "id-014", "id-017", "id-019", "id-022" }
        );
    }

    [Fact]
    public async Task Query_Test_233()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$orderby=releaseDate asc&$skip=5",
            100,
            "$orderby=releaseDate asc&$skip=105",
            null,
            new[] { "id-229", "id-224", "id-041", "id-049", "id-135" }
        );
    }

    [Fact]
    public async Task Query_Test_234()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$orderby=releaseDate desc&$skip=5",
            100,
            "$orderby=releaseDate desc&$skip=105",
            null,
            new[] { "id-149", "id-213", "id-102", "id-155", "id-169" }
        );
    }

    [Fact]
    public async Task Query_Test_235()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$orderby=title asc&$skip=5",
            100,
            "$orderby=title asc&$skip=105",
            null,
            new[] { "id-214", "id-102", "id-215", "id-039", "id-057" }
        );
    }

    [Fact]
    public async Task Query_Test_236()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$orderby=title desc&$skip=5",
            100,
            "$orderby=title desc&$skip=105",
            null,
            new[] { "id-058", "id-046", "id-160", "id-092", "id-176" }
        );
    }

    [Fact]
    public async Task Query_Test_237()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$orderby=year asc&$skip=5",
            100,
            "$orderby=year asc&$skip=105",
            null,
            new[] { "id-088", "id-224", "id-041", "id-049", "id-135" }
        );
    }

    [Fact]
    public async Task Query_Test_238()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$orderby=year asc,title asc&$skip=5",
            100,
            "$orderby=year asc,title asc&$skip=105",
            null,
            new[] { "id-088", "id-224", "id-041", "id-049", "id-135" }
        );
    }

    [Fact]
    public async Task Query_Test_239()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$orderby=year asc,title desc&$skip=5",
            100,
            "$orderby=year asc,title desc&$skip=105",
            null,
            new[] { "id-088", "id-224", "id-049", "id-041", "id-135" }
        );
    }

    [Fact]
    public async Task Query_Test_240()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$orderby=year desc&$skip=5",
            100,
            "$orderby=year desc&$skip=105",
            null,
            new[] { "id-149", "id-186", "id-213", "id-013", "id-053" }
        );
    }

    [Fact]
    public async Task Query_Test_241()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$orderby=year desc,title asc&$skip=5",
            100,
            "$orderby=year desc,title asc&$skip=105",
            null,
            new[] { "id-186", "id-064", "id-149", "id-169", "id-161" }
        );
    }

    [Fact]
    public async Task Query_Test_242()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$orderby=year desc,title desc&$skip=5",
            100,
            "$orderby=year desc,title desc&$skip=105",
            null,
            new[] { "id-186", "id-213", "id-102", "id-053", "id-155" }
        );
    }

    [Fact]
    public async Task Query_Test_243()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$skip=0",
            100,
            "$skip=100",
            null,
            new[] { "id-000", "id-001", "id-002", "id-003", "id-004" }
        );
    }

    [Fact]
    public async Task Query_Test_244()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$skip=100",
            100,
            "$skip=200",
            null,
            new[] { "id-100", "id-101", "id-102", "id-103", "id-104" }
        );
    }

    [Fact]
    public async Task Query_Test_245()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$skip=200",
            48,
            null,
            null,
            new[] { "id-200", "id-201", "id-202", "id-203", "id-204" }
        );
    }

    [Fact]
    public async Task Query_Test_246()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$skip=300",
            0,
            null,
            null,
            Array.Empty<string>());
    }

    [Fact]
    public async Task Query_Test_247()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$top=5&$filter=((year div 1000.5) eq 2) and (rating eq 'R')",
            2,
            null,
            null,
            new[] { "id-061", "id-173" }
        );
    }

    [Fact]
    public async Task Query_Test_248()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$top=5&$filter=((year sub 1900) ge 80) and ((year add 10) le 2000) and (duration le 120)",
            5,
            null,
            null,
            new[] { "id-026", "id-047", "id-081", "id-103", "id-121" }
        );
    }

    [Fact]
    public async Task Query_Test_249()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$top=5&$filter=(year div 1000.5) eq 2",
            5,
            null,
            null,
            new[] { "id-012", "id-042", "id-061", "id-173", "id-194" }
        );
    }

    [Fact]
    public async Task Query_Test_250()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$top=5&$filter=(year ge 1930 and year le 1940) or (year ge 1950 and year le 1960)",
            5,
            null,
            null,
            new[] { "id-005", "id-016", "id-027", "id-028", "id-031" }
        );
    }

    [Fact]
    public async Task Query_Test_251()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$top=5&$filter=(year sub 1900) ge 80",
            5,
            null,
            null,
            new[] { "id-000", "id-003", "id-006", "id-007", "id-008" }
        );
    }

    [Fact]
    public async Task Query_Test_252()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$top=5&$filter=bestPictureWinner eq false",
            5,
            null,
            null,
            new[] { "id-000", "id-003", "id-004", "id-005", "id-006" }
        );
    }

    [Fact]
    public async Task Query_Test_253()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$top=5&$filter=bestPictureWinner eq true and ceiling(duration div 60.0) eq 2",
            5,
            null,
            null,
            new[] { "id-023", "id-024", "id-112", "id-135", "id-142" }
        );
    }

    [Fact]
    public async Task Query_Test_254()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$top=5&$filter=bestPictureWinner eq true and floor(duration div 60.0) eq 2",
            5,
            null,
            null,
            new[] { "id-001", "id-011", "id-018", "id-048", "id-051" }
        );
    }

    [Fact]
    public async Task Query_Test_255()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$top=5&$filter=bestPictureWinner eq true and round(duration div 60.0) eq 2",
            5,
            null,
            null,
            new[] { "id-011", "id-018", "id-023", "id-024", "id-048" }
        );
    }

    [Fact]
    public async Task Query_Test_256()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$top=5&$filter=bestPictureWinner eq true",
            5,
            null,
            null,
            new[] { "id-001", "id-002", "id-007", "id-008", "id-011" }
        );
    }

    [Fact]
    public async Task Query_Test_257()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$top=5&$filter=bestPictureWinner ne false",
            5,
            null,
            null,
            new[] { "id-001", "id-002", "id-007", "id-008", "id-011" }
        );
    }

    [Fact]
    public async Task Query_Test_258()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$top=5&$filter=bestPictureWinner ne true",
            5,
            null,
            null,
            new[] { "id-000", "id-003", "id-004", "id-005", "id-006" }
        );
    }

    [Fact]
    public async Task Query_Test_259()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$top=5&$filter=ceiling(duration div 60.0) eq 2",
            5,
            null,
            null,
            new[] { "id-005", "id-023", "id-024", "id-025", "id-026" }
        );
    }

    [Fact]
    public async Task Query_Test_260()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$top=5&$filter=day(releaseDate) eq 1",
            5,
            null,
            null,
            new[] { "id-019", "id-048", "id-129", "id-131", "id-132" }
        );
    }

    [Fact]
    public async Task Query_Test_261()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$top=5&$filter=duration ge 60",
            5,
            null,
            null,
            new[] { "id-000", "id-001", "id-002", "id-003", "id-004" }
        );
    }

    [Fact]
    public async Task Query_Test_262()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$top=5&$filter=endswith(title, 'er')",
            5,
            null,
            null,
            new[] { "id-001", "id-052", "id-121", "id-130", "id-164" }
        );
    }

    [Fact]
    public async Task Query_Test_263()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$top=5&$filter=endswith(tolower(title), 'er')",
            5,
            null,
            null,
            new[] { "id-001", "id-052", "id-121", "id-130", "id-164" }
        );
    }

    [Fact]
    public async Task Query_Test_264()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$top=5&$filter=endswith(toupper(title), 'ER')",
            5,
            null,
            null,
            new[] { "id-001", "id-052", "id-121", "id-130", "id-164" }
        );
    }

    [Fact]
    public async Task Query_Test_265()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$top=5&$filter=floor(duration div 60.0) eq 2",
            5,
            null,
            null,
            new[] { "id-000", "id-001", "id-003", "id-004", "id-006" }
        );
    }

    [Fact]
    public async Task Query_Test_266()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$top=5&$filter=month(releaseDate) eq 11",
            5,
            null,
            null,
            new[] { "id-011", "id-016", "id-030", "id-064", "id-085" }
        );
    }

    [Fact]
    public async Task Query_Test_267()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$top=5&$filter=not(bestPictureWinner eq false)",
            5,
            null,
            null,
            new[] { "id-001", "id-002", "id-007", "id-008", "id-011" }
        );
    }

    [Fact]
    public async Task Query_Test_268()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$top=5&$filter=not(bestPictureWinner eq true)",
            5,
            null,
            null,
            new[] { "id-000", "id-003", "id-004", "id-005", "id-006" }
        );
    }

    [Fact]
    public async Task Query_Test_269()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$top=5&$filter=not(bestPictureWinner ne false)",
            5,
            null,
            null,
            new[] { "id-000", "id-003", "id-004", "id-005", "id-006" }
        );
    }

    [Fact]
    public async Task Query_Test_270()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$top=5&$filter=not(bestPictureWinner ne true)",
            5,
            null,
            null,
            new[] { "id-001", "id-002", "id-007", "id-008", "id-011" }
        );
    }

    [Fact]
    public async Task Query_Test_271()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$top=5&$filter=rating eq 'R'",
            5,
            null,
            null,
            new[] { "id-000", "id-001", "id-002", "id-003", "id-007" }
        );
    }

    [Fact]
    public async Task Query_Test_272()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$top=5&$filter=rating ne 'PG13'",
            5,
            null,
            null,
            new[] { "id-000", "id-001", "id-002", "id-003", "id-004" }
        );
    }

    [Fact]
    public async Task Query_Test_273()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$top=5&$filter=rating eq 'Unrated'",
            5,
            null,
            null,
            new[] { "id-004", "id-005", "id-011", "id-016", "id-031" }
        );
    }

    [Fact]
    public async Task Query_Test_274()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$top=5&$filter=releaseDate eq cast(1994-10-14,Edm.Date)",
            2,
            null,
            null,
            new[] { "id-000", "id-003" }
        );
    }

    [Fact]
    public async Task Query_Test_275()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$top=5&$filter=releaseDate ge cast(1999-12-31,Edm.Date)",
            5,
            null,
            null,
            new[] { "id-006", "id-008", "id-012", "id-013", "id-019" }
        );
    }

    [Fact]
    public async Task Query_Test_276()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$top=5&$filter=releaseDate gt cast(1999-12-31,Edm.Date)",
            5,
            null,
            null,
            new[] { "id-006", "id-008", "id-012", "id-013", "id-019" }
        );
    }

    [Fact]
    public async Task Query_Test_277()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$top=5&$filter=releaseDate le cast(2000-01-01,Edm.Date)",
            5,
            null,
            null,
            new[] { "id-000", "id-001", "id-002", "id-003", "id-004" }
        );
    }

    [Fact]
    public async Task Query_Test_278()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$top=5&$filter=releaseDate lt cast(2000-01-01,Edm.Date)",
            5,
            null,
            null,
            new[] { "id-000", "id-001", "id-002", "id-003", "id-004" }
        );
    }

    [Fact]
    public async Task Query_Test_279()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$top=5&$filter=round(duration div 60.0) eq 2",
            5,
            null,
            null,
            new[] { "id-000", "id-005", "id-009", "id-010", "id-011" }
        );
    }

    [Fact]
    public async Task Query_Test_281()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$top=5&$filter=startswith(tolower(title), 'the')",
            5,
            null,
            null,
            new[] { "id-000", "id-001", "id-002", "id-004", "id-006" }
        );
    }

    [Fact]
    public async Task Query_Test_282()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$top=5&$filter=startswith(toupper(title), 'THE')",
            5,
            null,
            null,
            new[] { "id-000", "id-001", "id-002", "id-004", "id-006" }
        );
    }

    [Fact]
    public async Task Query_Test_283()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$top=5&$filter=year eq 1994",
            5,
            null,
            null,
            new[] { "id-000", "id-003", "id-018", "id-030", "id-079" }
        );
    }

    [Fact]
    public async Task Query_Test_284()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$top=5&$filter=year ge 2000 and year le 2009",
            5,
            null,
            null,
            new[] { "id-006", "id-008", "id-012", "id-019", "id-020" }
        );
    }

    [Fact]
    public async Task Query_Test_285()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$top=5&$filter=year ge 2000",
            5,
            null,
            null,
            new[] { "id-006", "id-008", "id-012", "id-013", "id-019" }
        );
    }

    [Fact]
    public async Task Query_Test_286()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$top=5&$filter=year gt 1999 and year lt 2010",
            5,
            null,
            null,
            new[] { "id-006", "id-008", "id-012", "id-019", "id-020" }
        );
    }

    [Fact]
    public async Task Query_Test_287()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$top=5&$filter=year gt 1999",
            5,
            null,
            null,
            new[] { "id-006", "id-008", "id-012", "id-013", "id-019" }
        );
    }

    [Fact]
    public async Task Query_Test_288()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$top=5&$filter=year le 2000",
            5,
            null,
            null,
            new[] { "id-000", "id-001", "id-002", "id-003", "id-004" }
        );
    }

    [Fact]
    public async Task Query_Test_289()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$top=5&$filter=year lt 2001",
            5,
            null,
            null,
            new[] { "id-000", "id-001", "id-002", "id-003", "id-004" }
        );
    }

    [Fact]
    public async Task Query_Test_290()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$top=5&$filter=year(releaseDate) eq 1994",
            5,
            null,
            null,
            new[] { "id-000", "id-003", "id-018", "id-030", "id-079" }
        );
    }

    [Fact]
    public async Task Query_Test_291()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$top=5&$orderby=bestPictureWinner asc",
            5,
            null,
            null,
            new[] { "id-000", "id-003", "id-004", "id-005", "id-006" }
        );
    }

    [Fact]
    public async Task Query_Test_292()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$top=5&$orderby=bestPictureWinner desc",
            5,
            null,
            null,
            new[] { "id-001", "id-002", "id-007", "id-008", "id-011" }
        );
    }

    [Fact]
    public async Task Query_Test_293()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$top=5&$orderby=duration asc",
            5,
            null,
            null,
            new[] { "id-227", "id-125", "id-133", "id-107", "id-118" }
        );
    }

    [Fact]
    public async Task Query_Test_294()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$top=5&$orderby=duration desc",
            5,
            null,
            null,
            new[] { "id-153", "id-065", "id-165", "id-008", "id-002" }
        );
    }

    [Fact]
    public async Task Query_Test_295()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$top=5&$orderby=rating asc",
            5,
            null,
            null,
            new[] { "id-004", "id-005", "id-011", "id-016", "id-031" }
        );
    }

    [Fact]
    public async Task Query_Test_296()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$top=5&$orderby=rating desc",
            5,
            null,
            null,
            new[] { "id-000", "id-001", "id-002", "id-003", "id-007" }
        );
    }

    [Fact]
    public async Task Query_Test_297()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$top=5&$orderby=releaseDate asc",
            5,
            null,
            null,
            new[] { "id-125", "id-133", "id-227", "id-118", "id-088" }
        );
    }

    [Fact]
    public async Task Query_Test_298()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$top=5&$orderby=releaseDate desc",
            5,
            null,
            null,
            new[] { "id-188", "id-033", "id-122", "id-186", "id-064" }
        );
    }

    [Fact]
    public async Task Query_Test_299()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$top=5&$orderby=title asc",
            5,
            null,
            null,
            new[] { "id-005", "id-091", "id-243", "id-194", "id-060" }
        );
    }

    [Fact]
    public async Task Query_Test_300()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$top=5&$orderby=title desc",
            5,
            null,
            null,
            new[] { "id-107", "id-100", "id-123", "id-190", "id-149" }
        );
    }

    [Fact]
    public async Task Query_Test_301()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$top=5&$orderby=year asc",
            5,
            null,
            null,
            new[] { "id-125", "id-229", "id-133", "id-227", "id-118" }
        );
    }

    [Fact]
    public async Task Query_Test_302()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$top=5&$orderby=year asc,title asc",
            5,
            null,
            null,
            new[] { "id-125", "id-229", "id-227", "id-133", "id-118" }
        );
    }

    [Fact]
    public async Task Query_Test_303()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$top=5&$orderby=year asc,title desc",
            5,
            null,
            null,
            new[] { "id-125", "id-229", "id-133", "id-227", "id-118" }
        );
    }

    [Fact]
    public async Task Query_Test_304()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$top=5&$orderby=year desc",
            5,
            null,
            null,
            new[] { "id-033", "id-122", "id-188", "id-064", "id-102" }
        );
    }

    [Fact]
    public async Task Query_Test_305()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$top=5&$orderby=year desc,title asc",
            5,
            null,
            null,
            new[] { "id-188", "id-122", "id-033", "id-102", "id-213" }
        );
    }

    [Fact]
    public async Task Query_Test_306()
    {
        await MovieQueryTest(
            $"{factory.MovieEndpoint}?$top=5&$orderby=year desc,title desc",
            5,
            null,
            null,
            new[] { "id-033", "id-122", "id-188", "id-149", "id-064" }
        );
    }

    [Fact]
    public async Task SoftDeleteQueryTest_002()
    {
        factory.SoftDelete<InMemoryMovie>(x => x.Rating == MovieRating.R);
        await MovieQueryTest(
            $"{factory.SoftDeletedMovieEndpoint}?$count=true",
            100,
            "$count=true&$skip=100",
            153,
            new[] { "id-004", "id-005", "id-006", "id-008", "id-010" }
        );
    }

    [Fact]
    public async Task SoftDeleteQueryTest_003()
    {
        factory.SoftDelete<InMemoryMovie>(x => x.Rating == MovieRating.R);
        await MovieQueryTest(
            $"{factory.SoftDeletedMovieEndpoint}?$filter=deleted eq false&__includedeleted=true",
            100,
            "$filter=deleted eq false&__includedeleted=true&$skip=100",
            null,
            new[] { "id-004", "id-005", "id-006", "id-008", "id-010" }
        );
    }

    [Fact]
    public async Task SoftDeleteQueryTest_004()
    {
        factory.SoftDelete<InMemoryMovie>(x => x.Rating == MovieRating.R);
        await MovieQueryTest(
            $"{factory.SoftDeletedMovieEndpoint}?$filter=deleted eq true&__includedeleted=true",
            95,
            null,
            null,
            new[] { "id-000", "id-001", "id-002", "id-003", "id-007" }
        );
    }

    [Fact]
    public async Task SoftDeleteQueryTest_005()
    {
        factory.SoftDelete<InMemoryMovie>(x => x.Rating == MovieRating.R);
        await MovieQueryTest(
            $"{factory.SoftDeletedMovieEndpoint}?__includedeleted=true",
            100,
            "__includedeleted=true&$skip=100",
            null,
            new[] { "id-000", "id-001", "id-002", "id-003", "id-004", "id-005" }
        );
    }

    [Fact]
    public async Task KitchSinkQueryTest_010()
    {
        SeedKitchenSink();
        await KitchenSinkQueryTest(
            $"{factory.KitchenSinkEndpoint}?$orderby=id&$filter=(second(timeOnlyValue) eq 0)",
            100,
            "$orderby=id&$filter=(second(timeOnlyValue) eq 0)&$skip=100",
            null,
            new[] { "id-000", "id-001", "id-002", "id-003", "id-004", "id-005" }
        );
    }

    [Fact]
    public async Task KitchSinkQueryTest_011()
    {
        SeedKitchenSink();
        await KitchenSinkQueryTest(
            $"{factory.KitchenSinkEndpoint}?$orderby=id&$filter=(hour(timeOnlyValue) eq 3)",
            31,
            null,
            null,
            new[] { "id-059", "id-060", "id-061", "id-062", "id-063", "id-064" }
        );
    }

    [Fact]
    public async Task KitchSinkQueryTest_012()
    {
        SeedKitchenSink();
        await KitchenSinkQueryTest(
            $"{factory.KitchenSinkEndpoint}?$orderby=id&$filter=(minute(timeOnlyValue) eq 21)",
            12,
            null,
            null,
            new[] { "id-020", "id-051", "id-079", "id-110", "id-140", "id-171" }
        );
    }

    [Fact]
    public async Task KitchSinkQueryTest_013()
    {
        SeedKitchenSink();
        await KitchenSinkQueryTest(
            $"{factory.KitchenSinkEndpoint}?$count=true&$orderby=id&$filter=(hour(timeOnlyValue) le 12)",
            100,
            "$count=true&$orderby=id&$filter=(hour(timeOnlyValue) le 12)&$skip=100",
            365,
            new[] { "id-000", "id-001", "id-002", "id-003", "id-004", "id-005" }
        );
    }

    [Fact]
    public async Task KitchSinkQueryTest_014()
    {
        SeedKitchenSink();
        await KitchenSinkQueryTest(
            $"{factory.KitchenSinkEndpoint}?$count=true&$orderby=id&$filter=timeOnlyValue eq cast(02:14:00,Edm.TimeOfDay)",
            1,
            null,
            1,
            new[] { "id-044" }

        );
    }

    [Fact]
    public async Task KitchSinkQueryTest_015()
    {
        SeedKitchenSink();
        await KitchenSinkQueryTest(
            $"{factory.KitchenSinkEndpoint}?$count=true&$orderby=id&$filter=timeOnlyValue ge cast(12:15:00,Edm.TimeOfDay)",
            17,
            null,
            17,
            new[] { "id-348", "id-349", "id-350", "id-351" }

        );
    }

    [Fact]
    public async Task KitchSinkQueryTest_016()
    {
        SeedKitchenSink();
        await KitchenSinkQueryTest(
            $"{factory.KitchenSinkEndpoint}?$count=true&$orderby=id&$filter=timeOnlyValue gt cast(12:15:00,Edm.TimeOfDay)",
            16,
            null,
            16,
            new[] { "id-349", "id-350", "id-351", "id-352" }

        );
    }

    [Fact]
    public async Task KitchSinkQueryTest_017()
    {
        SeedKitchenSink();
        await KitchenSinkQueryTest(
            $"{factory.KitchenSinkEndpoint}?$count=true&$orderby=id&$filter=timeOnlyValue le cast(07:14:00,Edm.TimeOfDay)",
            100,
            "$count=true&$orderby=id&$filter=timeOnlyValue le cast(07:14:00,Edm.TimeOfDay)&$skip=100",
            195,
            new[] { "id-000", "id-001", "id-002", "id-003" }

        );
    }

    [Fact]
    public async Task KitchSinkQueryTest_018()
    {
        SeedKitchenSink();
        await KitchenSinkQueryTest(
            $"{factory.KitchenSinkEndpoint}?$count=true&$orderby=id&$filter=timeOnlyValue lt cast(07:14:00,Edm.TimeOfDay)",
            100,
            "$count=true&$orderby=id&$filter=timeOnlyValue lt cast(07:14:00,Edm.TimeOfDay)&$skip=100",
            194,
            new[] { "id-000", "id-001", "id-002", "id-003" }
        );
    }

    [Fact]
    public async Task KitchenSinkQueryTest_019()
    {
        SeedKitchenSinkWithCountryData();
        await KitchenSinkQueryTest(
            $"{factory.KitchenSinkEndpoint}?$filter=pointValue eq geography'POINT(-95 38)'",
            1,
            null,
            null,
            new[] { "US" }
        );
    }

    [Fact]
    public async Task Paging_Test_1()
    {
        await PagingTest(factory.MovieEndpoint, 248, 3);
    }

    [Fact]
    public async Task Paging_Test_2()
    {
        await PagingTest($"{factory.PagedMovieEndpoint}?$top=100", 100, 4);
    }

    [Fact]
    public async Task Paging_Test_3()
    {
        await PagingTest($"{factory.MovieEndpoint}?$count=true", 248, 3);
    }

    [Fact]
    public async Task Paging_Test_4()
    {
        await PagingTest($"{factory.MovieEndpoint}?$top=50&$count=true", 50, 1);
    }

    [Fact]
    public async Task Paging_Test_5()
    {
        await PagingTest($"{factory.MovieEndpoint}?$filter=releaseDate ge cast(1960-01-01,Edm.Date)&orderby=releaseDate asc", 186, 2);
    }

    [Fact]
    public async Task Paging_Test_6()
    {
        await PagingTest($"{factory.MovieEndpoint}?$filter=releaseDate ge cast(1960-01-01,Edm.Date)&orderby=releaseDate asc&$top=20", 20, 1);
    }

    /// <summary>
    /// We do a bunch of tests for select by dealing with the overflow properties capabilities within
    /// System.Text.Json - we are not interested in the search (we're doing the same thing over and
    /// over).  Instead, we are ensuring the right selections are made.
    /// </summary>
    [Theory, PairwiseData]
    public async Task SelectQueryTest(bool sId, bool sUpdatedAt, bool sVersion, bool sDeleted, bool sBPW, bool sduration, bool srating, bool sreleaseDate, bool stitle, bool syear)
    {
        List<string> selection = new();
        if (sId) selection.Add("id");
        if (sUpdatedAt) selection.Add("updatedAt");
        if (sVersion) selection.Add("version");
        if (sDeleted) selection.Add("deleted");
        if (sBPW) selection.Add("bestPictureWinner");
        if (sduration) selection.Add("duration");
        if (srating) selection.Add("rating");
        if (sreleaseDate) selection.Add("releaseDate");
        if (stitle) selection.Add("title");
        if (syear) selection.Add("year");
        if (selection.Count == 0) return;
        string query = $"{factory.MovieEndpoint}?$top=5&$skip=5&$select={string.Join(',', selection)}";

        HttpResponseMessage response = await client.GetAsync(query);
        response.Should().HaveStatusCode(HttpStatusCode.OK);
        string content = await response.Content.ReadAsStringAsync();
        PageOfItems<ClientObject> result = JsonSerializer.Deserialize<PageOfItems<ClientObject>>(content, serializerOptions);
        result.Should().NotBeNull();
        result.Items.Should().NotBeNullOrEmpty();
        foreach (ClientObject item in result.Items)
        {
            List<string> keys = item.Data.Keys.ToList();
            keys.Should().BeEquivalentTo(selection);
        }
    }

    #region Base Tests
    /// <summary>
    /// This is the base test for the individual query tests.
    /// </summary>
    /// <param name="pathAndQuery">The request URI (path and query only)</param>
    /// <param name="itemCount">The number of items expected to be returned</param>
    /// <param name="nextLinkQuery">The value of the nextLink expected</param>
    /// <param name="totalCount">If provided, the value of the count expected</param>
    /// <param name="firstItems">The start of the list of IDs that should be returned.</param>
    /// <returns>A task that completes when the test is complete.</returns>
    private async Task MovieQueryTest(string pathAndQuery, int itemCount, string nextLinkQuery, int? totalCount, string[] firstItems)
    {
        HttpResponseMessage response = await client.GetAsync(pathAndQuery);
        string content = await response.Content.ReadAsStringAsync();
        response.Should().HaveStatusCode(HttpStatusCode.OK);

        PageOfItems<ClientMovie> result = JsonSerializer.Deserialize<PageOfItems<ClientMovie>>(content, serializerOptions);

        // Payload has the right content
        Assert.Equal(itemCount, result!.Items!.Length);
        Assert.Equal(nextLinkQuery, result.NextLink == null ? null : Uri.UnescapeDataString(result.NextLink));
        Assert.Equal(totalCount, result.Count);

        // The first n items must match what is expected
        Assert.True(result.Items.Length >= firstItems.Length);
        Assert.Equal(firstItems, result.Items.Take(firstItems.Length).Select(m => m.Id).ToArray());
        for (int idx = 0; idx < firstItems.Length; idx++)
        {
            InMemoryMovie expected = factory.GetServerEntityById<InMemoryMovie>(firstItems[idx])!;
            result.Items[idx].Should().HaveEquivalentMetadataTo(expected).And.BeEquivalentTo<IMovie>(expected);
        }
    }

    /// <summary>
    /// This is the base test for the individual query tests.
    /// </summary>
    /// <param name="pathAndQuery">The request URI (path and query only)</param>
    /// <param name="itemCount">The number of items expected to be returned</param>
    /// <param name="nextLinkQuery">The value of the nextLink expected</param>
    /// <param name="totalCount">If provided, the value of the count expected</param>
    /// <param name="firstItems">The start of the list of IDs that should be returned.</param>
    /// <returns>A task that completes when the test is complete.</returns>
    private async Task KitchenSinkQueryTest(string pathAndQuery, int itemCount, string nextLinkQuery, int? totalCount, string[] firstItems)
    {
        HttpResponseMessage response = await client.GetAsync(pathAndQuery);
        string content = await response.Content.ReadAsStringAsync();
        response.Should().HaveStatusCode(HttpStatusCode.OK);

        PageOfItems<ClientKitchenSink> result = JsonSerializer.Deserialize<PageOfItems<ClientKitchenSink>>(content, serializerOptions);

        // Payload has the right content
        Assert.Equal(itemCount, result!.Items!.Length);
        Assert.Equal(nextLinkQuery, result.NextLink == null ? null : Uri.UnescapeDataString(result.NextLink));
        Assert.Equal(totalCount, result.Count);

        // The first n items must match what is expected
        Assert.True(result.Items.Length >= firstItems.Length);
        Assert.Equal(firstItems, result.Items.Take(firstItems.Length).Select(m => m.Id).ToArray());
        for (int idx = 0; idx < firstItems.Length; idx++)
        {
            InMemoryKitchenSink expected = factory.GetServerEntityById<InMemoryKitchenSink>(firstItems[idx])!;
            result.Items[idx].Should().HaveEquivalentMetadataTo(expected).And.BeEquivalentTo<IKitchenSink>(expected);
        }
    }

    /// <summary>
    /// Tests the paging capability of the query table endpoint.
    /// </summary>
    /// <param name="startQuery">The starting query.</param>
    /// <param name="expectedCount">The total expected number of records, after paging is complete.</param>
    /// <param name="expectedLoops">The number of loops expected.</param>
    /// <returns>A task that completes when the test is complete.</returns>
    private async Task PagingTest(string startQuery, int expectedCount, int expectedLoops)
    {
        string path = startQuery.Contains('?') ? startQuery[..(startQuery.IndexOf('?'))] : startQuery;
        string query = startQuery;

        int loops = 0;
        Dictionary<string, ClientMovie> items = new();

        do
        {
            loops++;
            HttpResponseMessage response = await client.GetAsync(query);
            string content = await response.Content.ReadAsStringAsync();
            response.Should().HaveStatusCode(HttpStatusCode.OK);
            PageOfItems<ClientMovie> result = JsonSerializer.Deserialize<PageOfItems<ClientMovie>>(content, serializerOptions);
            result.Items.Should().NotBeNull();
            result.Items.ToList().ForEach(x => items.Add(x.Id, x));
            if (result.NextLink != null)
            {
                query = $"{path}?{result.NextLink}";
            }
            else
            {
                break;
            }
        } while (loops < expectedLoops + 2);

        items.Should().HaveCount(expectedCount);
        loops.Should().Be(expectedLoops);
    }
    #endregion
}