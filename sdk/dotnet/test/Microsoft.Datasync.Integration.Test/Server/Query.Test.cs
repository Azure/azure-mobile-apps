// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Test.TestData;
using System.Text.Json.Serialization;

namespace Microsoft.Datasync.Integration.Test.Server;

[ExcludeFromCodeCoverage]
[Collection("Integration")]
public class Query_Tests : BaseTest
{
	public Query_Tests(ITestOutputHelper logger) : base(logger) { }

	[Fact]
	public async Task Query_Test_001()
	{
		await BaseQueryTest(
			"tables/movies",
			100,
			"tables/movies?$skip=100",
			null,
			new[] { "id-000", "id-001", "id-002", "id-003", "id-004" }
		);
	}

	[Fact]
	public async Task Query_Test_002()
	{
		await BaseQueryTest(
			"tables/movies?$count=true",
			100,
			"tables/movies?$count=true&$skip=100",
			Movies.Count,
			new[] { "id-000", "id-001", "id-002", "id-003", "id-004" }
		);
	}

	[Fact]
	public async Task Query_Test_003()
	{
		await BaseQueryTest(
			"tables/movies?$count=true&$filter=((year div 1000.5) eq 2) and (rating eq 'R')",
			2,
			null,
			2,
			new[] { "id-061", "id-173" }
		);
	}

	[Fact]
	public async Task Query_Test_004()
	{
		await BaseQueryTest(
			"tables/movies?$count=true&$filter=((year sub 1900) ge 80) and ((year add 10) le 2000) and (duration le 120)",
			13,
			null,
			13,
			new[] { "id-026", "id-047", "id-081", "id-103", "id-121" }
		);
	}

	[Fact]
	public async Task Query_Test_005()
	{
		await BaseQueryTest(
			"tables/movies?$count=true&$filter=(year div 1000.5) eq 2",
			6,
			null,
			6,
			new[] { "id-012", "id-042", "id-061", "id-173", "id-194" }
			);
	}

	[Fact]
	public async Task Query_Test_006()
	{
		await BaseQueryTest(
			"tables/movies?$count=true&$filter=(year ge 1930 and year le 1940) or (year ge 1950 and year le 1960)",
			46,
			null,
			46,
			new[] { "id-005", "id-016", "id-027", "id-028", "id-031" }
			);
	}

	[Fact]
	public async Task Query_Test_007()
	{
		await BaseQueryTest(
			"tables/movies?$count=true&$filter=(year sub 1900) ge 80",
			100,
			"tables/movies?$count=true&$filter=(year sub 1900) ge 80&$skip=100",
			138,
			new[] { "id-000", "id-003", "id-006", "id-007", "id-008" }
			);
	}

	[Fact]
	public async Task Query_Test_008()
	{
		await BaseQueryTest(
			"tables/movies?$count=true&$filter=bestPictureWinner eq false",
			100, 
			"tables/movies?$count=true&$filter=bestPictureWinner eq false&$skip=100",
			210, 
			new[] { "id-000", "id-003", "id-004", "id-005", "id-006" }
		);
	}

	[Fact]
	public async Task Query_Test_009()
	{
		await BaseQueryTest(
			"tables/movies?$count=true&$filter=bestPictureWinner eq true and ceiling(duration div 60.0) eq 2",
			11,
			null,
			11,
			new[] { "id-023", "id-024", "id-112", "id-135", "id-142" }
		);
	}

	[Fact]
	public async Task Query_Test_010()
	{
		await BaseQueryTest(
			"tables/movies?$count=true&$filter=bestPictureWinner eq true and floor(duration div 60.0) eq 2",
			21,
			null,
			21,
			new[] { "id-001", "id-011", "id-018", "id-048", "id-051" }
		);
	}

	[Fact]
	public async Task Query_Test_011()
	{
		await BaseQueryTest(
			"tables/movies?$count=true&$filter=bestPictureWinner eq true and round(duration div 60.0) eq 2",
			24,
			null,
			24,
			new[] { "id-011", "id-018", "id-023", "id-024", "id-048" }
		);
	}

	[Fact]
	public async Task Query_Test_012()
	{
		await BaseQueryTest(
			"tables/movies?$count=true&$filter=bestPictureWinner eq true",
			38,
			null,
			38,
			new[] { "id-001", "id-002", "id-007", "id-008", "id-011" }
		);
	}

	[Fact]
	public async Task Query_Test_013()
	{
		await BaseQueryTest(
			"tables/movies?$count=true&$filter=bestPictureWinner ne false",
			38,
			null,
			38,
			new[] { "id-001", "id-002", "id-007", "id-008", "id-011" }
		);
	}

	[Fact]
	public async Task Query_Test_014()
	{
		await BaseQueryTest(
			"tables/movies?$count=true&$filter=bestPictureWinner ne true",
			100, 
			"tables/movies?$count=true&$filter=bestPictureWinner ne true&$skip=100",
			210,
			new[] { "id-000", "id-003", "id-004", "id-005", "id-006" }
		);
	}

	[Fact]
	public async Task Query_Test_015()
	{
		await BaseQueryTest(
			"tables/movies?$count=true&$filter=ceiling(duration div 60.0) eq 2",
			100, 
			"tables/movies?$count=true&$filter=ceiling(duration div 60.0) eq 2&$skip=100",
			124,
			new[] { "id-005", "id-023", "id-024", "id-025", "id-026" }
		);
	}

	[Fact]
	public async Task Query_Test_016()
	{
		await BaseQueryTest(
			"tables/movies?$count=true&$filter=day(releaseDate) eq 1",
			7,
			null,
			7,
			new[] { "id-019", "id-048", "id-129", "id-131", "id-132" }
		);
	}

	[Fact]
	public async Task Query_Test_017()
	{
		await BaseQueryTest(
			"tables/movies?$count=true&$filter=duration ge 60",
			100, 
			"tables/movies?$count=true&$filter=duration ge 60&$skip=100",
			Movies.Count,
			new[] { "id-000", "id-001", "id-002", "id-003", "id-004" }
		);
	}

	[Fact]
	public async Task Query_Test_018()
	{
		await BaseQueryTest(
			"tables/movies?$count=true&$filter=endswith(title, 'er')",
			12,
			null,
			12,
			new[] { "id-001", "id-052", "id-121", "id-130", "id-164" }
		);
	}

	[Fact]
	public async Task Query_Test_019()
	{
		await BaseQueryTest(
			"tables/movies?$count=true&$filter=endswith(tolower(title), 'er')",
			12,
			null,
			12,
			new[] { "id-001", "id-052", "id-121", "id-130", "id-164" }
		);
	}

	[Fact]
	public async Task Query_Test_020()
	{
		await BaseQueryTest(
			"tables/movies?$count=true&$filter=endswith(toupper(title), 'ER')",
			12,
			null,
			12,
			new[] { "id-001", "id-052", "id-121", "id-130", "id-164" }
		);
	}

	[Fact]
	public async Task Query_Test_021()
	{
		await BaseQueryTest(
			"tables/movies?$count=true&$filter=floor(duration div 60.0) eq 2",
			100,
			"tables/movies?$count=true&$filter=floor(duration div 60.0) eq 2&$skip=100",
			120,
			new[] { "id-000", "id-001", "id-003", "id-004", "id-006" }
		);
	}

	[Fact]
	public async Task Query_Test_022()
	{
		await BaseQueryTest(
			"tables/movies?$count=true&$filter=month(releaseDate) eq 11",
			14,
			null,
			14,
			new[] { "id-011", "id-016", "id-030", "id-064", "id-085" }
		);
	}

	[Fact]
	public async Task Query_Test_023()
	{
		await BaseQueryTest(
			"tables/movies?$count=true&$filter=not(bestPictureWinner eq false)",
			38,
			null,
			38,
			new[] { "id-001", "id-002", "id-007", "id-008", "id-011" }
		);
	}

	[Fact]
	public async Task Query_Test_024()
	{
		await BaseQueryTest(
			"tables/movies?$count=true&$filter=not(bestPictureWinner eq true)",
			100,
			"tables/movies?$count=true&$filter=not(bestPictureWinner eq true)&$skip=100",
			210,
			new[] { "id-000", "id-003", "id-004", "id-005", "id-006" }
		);
	}

	[Fact]
	public async Task Query_Test_025()
	{
		await BaseQueryTest(
			"tables/movies?$count=true&$filter=not(bestPictureWinner ne false)",
			100,
			"tables/movies?$count=true&$filter=not(bestPictureWinner ne false)&$skip=100",
			210,
			new[] { "id-000", "id-003", "id-004", "id-005", "id-006" }
		);
	}

	[Fact]
	public async Task Query_Test_026()
	{
		await BaseQueryTest(
			"tables/movies?$count=true&$filter=not(bestPictureWinner ne true)",
			38,
			null,
			38,
			new[] { "id-001", "id-002", "id-007", "id-008", "id-011" }
		);
	}

	[Fact]
	public async Task Query_Test_027()
	{
		await BaseQueryTest(
			"tables/movies?$count=true&$filter=rating eq 'R'",
			95,
			null,
			95,
			new[] { "id-000", "id-001", "id-002", "id-003", "id-007" }
		);
	}

	[Fact]
	public async Task Query_Test_028()
	{
		await BaseQueryTest(
			"tables/movies?$count=true&$filter=rating ne 'PG-13'",
			100,
			"tables/movies?$count=true&$filter=rating ne 'PG-13'&$skip=100",
			220,
			new[] { "id-000", "id-001", "id-002", "id-003", "id-004" }
		);
	}

	[Fact]
	public async Task Query_Test_029()
	{
		await BaseQueryTest(
			"tables/movies?$count=true&$filter=rating eq null",
			74,
			null,
			74,
			new[] { "id-004", "id-005", "id-011", "id-016", "id-031" }
		);
	}

	[Fact]
	public async Task Query_Test_030()
	{
		await BaseQueryTest(
			"tables/movies?$count=true&$filter=releaseDate eq cast(1994-10-14T00:00:00.000Z,Edm.DateTimeOffset)",
			2,
			null,
			2,
			new[] { "id-000", "id-003" }
		);
	}

	[Fact]
	public async Task Query_Test_031()
	{
		await BaseQueryTest(
			"tables/movies?$count=true&$filter=releaseDate ge cast(1999-12-31T00:00:00.000Z,Edm.DateTimeOffset)",
			69,
			null,
			69,
			new[] { "id-006", "id-008", "id-012", "id-013", "id-019" }
		);
	}

	[Fact]
	public async Task Query_Test_032()
	{
		await BaseQueryTest(
			"tables/movies?$count=true&$filter=releaseDate gt cast(1999-12-31T00:00:00.000Z,Edm.DateTimeOffset)",
			69,
			null,
			69,
			new[] { "id-006", "id-008", "id-012", "id-013", "id-019" }
		);
	}

	[Fact]
	public async Task Query_Test_033()
	{
		await BaseQueryTest(
			"tables/movies?$count=true&$filter=releaseDate le cast(2000-01-01T00:00:00.000Z,Edm.DateTimeOffset)",
			100,
			"tables/movies?$count=true&$filter=releaseDate le cast(2000-01-01T00:00:00.000Z,Edm.DateTimeOffset)&$skip=100",
			179,
			new[] { "id-000", "id-001", "id-002", "id-003", "id-004" }
		);
	}

	[Fact]
	public async Task Query_Test_034()
	{
		await BaseQueryTest(
			"tables/movies?$count=true&$filter=releaseDate lt cast(2000-01-01T00:00:00.000Z,Edm.DateTimeOffset)",
			100,
			"tables/movies?$count=true&$filter=releaseDate lt cast(2000-01-01T00:00:00.000Z,Edm.DateTimeOffset)&$skip=100",
			179,
			new[] { "id-000", "id-001", "id-002", "id-003", "id-004" }
		);
	}

	[Fact]
	public async Task Query_Test_035()
	{
		await BaseQueryTest(
			"tables/movies?$count=true&$filter=round(duration div 60.0) eq 2",
			100,
			"tables/movies?$count=true&$filter=round(duration div 60.0) eq 2&$skip=100", 
			Movies.MovieList.Count(x => Math.Round(x.Duration / 60.0) == 2.0),
			new[] { "id-000", "id-005", "id-009", "id-010", "id-011" }
		);
	}

	[Fact]
	public async Task Query_Test_036()
	{
		await BaseQueryTest(
			"tables/movies?$count=true&$filter=startswith(rating, 'PG')",
			68,
			null,
			68,
			new[] { "id-006", "id-008", "id-010", "id-012", "id-013" }
		);
	}

	[Fact]
	public async Task Query_Test_037()
	{
		await BaseQueryTest(
			"tables/movies?$count=true&$filter=startswith(tolower(title), 'the')",
			63,
			null,
			63,
			new[] { "id-000", "id-001", "id-002", "id-004", "id-006" }
		);
	}

	[Fact]
	public async Task Query_Test_038()
	{
		await BaseQueryTest(
			"tables/movies?$count=true&$filter=startswith(toupper(title), 'THE')",
			63,
			null,
			63,
			new[] { "id-000", "id-001", "id-002", "id-004", "id-006" }
		);
	}

	[Fact]
	public async Task Query_Test_039()
	{
		await BaseQueryTest(
			"tables/movies?$count=true&$filter=year eq 1994",
			5,
			null,
			5,
			new[] { "id-000", "id-003", "id-018", "id-030", "id-079" }
		);
	}

	[Fact]
	public async Task Query_Test_040()
	{
		await BaseQueryTest(
			"tables/movies?$count=true&$filter=year ge 2000 and year le 2009",
			55,
			null,
			55,
			new[] { "id-006", "id-008", "id-012", "id-019", "id-020" }
		);
	}

	[Fact]
	public async Task Query_Test_041()
	{
		await BaseQueryTest(
			"tables/movies?$count=true&$filter=year ge 2000",
			69,
			null,
			69,
			new[] { "id-006", "id-008", "id-012", "id-013", "id-019" }
		);
	}

	[Fact]
	public async Task Query_Test_042()
	{
		await BaseQueryTest(
			"tables/movies?$count=true&$filter=year gt 1999 and year lt 2010",
			55,
			null,
			55,
			new[] { "id-006", "id-008", "id-012", "id-019", "id-020" }
		);
	}

	[Fact]
	public async Task Query_Test_043()
	{
		await BaseQueryTest(
			"tables/movies?$count=true&$filter=year gt 1999",
			69,
			null,
			69,
			new[] { "id-006", "id-008", "id-012", "id-013", "id-019" }
		);
	}

	[Fact]
	public async Task Query_Test_044()
	{
		await BaseQueryTest(
			"tables/movies?$count=true&$filter=year le 2000",
			100,
			"tables/movies?$count=true&$filter=year le 2000&$skip=100",
			185,
			new[] { "id-000", "id-001", "id-002", "id-003", "id-004" }
		);
	}

	[Fact]
	public async Task Query_Test_045()
	{
		await BaseQueryTest(
			"tables/movies?$count=true&$filter=year lt 2001",
			100,
			"tables/movies?$count=true&$filter=year lt 2001&$skip=100",
			185,
			new[] { "id-000", "id-001", "id-002", "id-003", "id-004" }
		);
	}

	[Fact]
	public async Task Query_Test_046()
	{
		await BaseQueryTest(
			"tables/movies?$count=true&$filter=year(releaseDate) eq 1994",
			6,
			null,
			6,
			new[] { "id-000", "id-003", "id-018", "id-030", "id-079" }
		);
	}

	[Fact]
	public async Task Query_Test_047()
	{
		await BaseQueryTest(
			"tables/movies?$count=true&$orderby=bestPictureWinner asc",
			100,
			"tables/movies?$count=true&$orderby=bestPictureWinner asc&$skip=100",
			Movies.Count,
			new[] { "id-000", "id-003", "id-004", "id-005", "id-006" }
		);
	}

	[Fact]
	public async Task Query_Test_048()
	{
		await BaseQueryTest(
			"tables/movies?$count=true&$orderby=bestPictureWinner desc",
			100,
			"tables/movies?$count=true&$orderby=bestPictureWinner desc&$skip=100",
			Movies.Count,
			new[] { "id-001", "id-002", "id-007", "id-008", "id-011" }
		);
	}

	[Fact]
	public async Task Query_Test_049()
	{
		await BaseQueryTest(
			"tables/movies?$count=true&$orderby=duration asc",
			100,
			"tables/movies?$count=true&$orderby=duration asc&$skip=100",
			Movies.Count,
			new[] { "id-227", "id-125", "id-133", "id-107", "id-118" }
		);
	}

	[Fact]
	public async Task Query_Test_050()
	{
		await BaseQueryTest(
			"tables/movies?$count=true&$orderby=duration desc",
			100,
			"tables/movies?$count=true&$orderby=duration desc&$skip=100",
			Movies.Count,
			new[] { "id-153", "id-065", "id-165", "id-008", "id-002" }
		);
	}

	[Fact]
	public async Task Query_Test_051()
	{
		await BaseQueryTest(
			"tables/movies?$count=true&$orderby=rating asc",
			100,
			"tables/movies?$count=true&$orderby=rating asc&$skip=100",
			Movies.Count,
			new[] { "id-004", "id-005", "id-011", "id-016", "id-031" }
		);
	}

	[Fact]
	public async Task Query_Test_052()
	{
		await BaseQueryTest(
			"tables/movies?$count=true&$orderby=rating desc",
			100,
			"tables/movies?$count=true&$orderby=rating desc&$skip=100",
			Movies.Count,
			new[] { "id-000", "id-001", "id-002", "id-003", "id-007" }
		);
	}

	[Fact]
	public async Task Query_Test_053()
	{
		await BaseQueryTest(
			"tables/movies?$count=true&$orderby=releaseDate asc",
			100,
			"tables/movies?$count=true&$orderby=releaseDate asc&$skip=100",
			Movies.Count,
			new[] { "id-125", "id-133", "id-227", "id-118", "id-088" }
		);
	}

	[Fact]
	public async Task Query_Test_054()
	{
		await BaseQueryTest(
			"tables/movies?$count=true&$orderby=releaseDate desc",
			100,
			"tables/movies?$count=true&$orderby=releaseDate desc&$skip=100",
			Movies.Count,
			new[] { "id-188", "id-033", "id-122", "id-186", "id-064" }
		);
	}

	[Fact]
	public async Task Query_Test_055()
	{
		await BaseQueryTest(
			"tables/movies?$count=true&$orderby=title asc",
			100,
			"tables/movies?$count=true&$orderby=title asc&$skip=100",
			Movies.Count,
			new[] { "id-005", "id-091", "id-243", "id-194", "id-060" }
		);
	}

	[Fact]
	public async Task Query_Test_056()
	{
		await BaseQueryTest(
			"tables/movies?$count=true&$orderby=title desc",
			100,
			"tables/movies?$count=true&$orderby=title desc&$skip=100",
			Movies.Count,
			new[] { "id-107", "id-100", "id-123", "id-190", "id-149" }
		);
	}

	[Fact]
	public async Task Query_Test_057()
	{
		await BaseQueryTest(
			"tables/movies?$count=true&$orderby=year asc",
			100,
			"tables/movies?$count=true&$orderby=year asc&$skip=100",
			Movies.Count,
			new[] { "id-125", "id-229", "id-133", "id-227", "id-118" }
		);
	}

	[Fact]
	public async Task Query_Test_058()
	{
		await BaseQueryTest(
			"tables/movies?$count=true&$orderby=year asc,title asc",
			100,
			"tables/movies?$count=true&$orderby=year asc,title asc&$skip=100",
			Movies.Count,
			new[] { "id-125", "id-229", "id-227", "id-133", "id-118" }
		);
	}

	[Fact]
	public async Task Query_Test_059()
	{
		await BaseQueryTest(
			"tables/movies?$count=true&$orderby=year asc,title desc",
			100,
			"tables/movies?$count=true&$orderby=year asc,title desc&$skip=100",
			Movies.Count,
			new[] { "id-125", "id-229", "id-133", "id-227", "id-118" }
		);
	}

	[Fact]
	public async Task Query_Test_060()
	{
		await BaseQueryTest(
			"tables/movies?$count=true&$orderby=year desc",
			100,
			"tables/movies?$count=true&$orderby=year desc&$skip=100",
			Movies.Count,
			new[] { "id-033", "id-122", "id-188", "id-064", "id-102" }
		);
	}

	[Fact]
	public async Task Query_Test_061()
	{
		await BaseQueryTest(
			"tables/movies?$count=true&$orderby=year desc,title asc",
			100,
			"tables/movies?$count=true&$orderby=year desc,title asc&$skip=100",
			Movies.Count,
			new[] { "id-188", "id-122", "id-033", "id-102", "id-213" }
		);
	}

	[Fact]
	public async Task Query_Test_062()
	{
		await BaseQueryTest(
			"tables/movies?$count=true&$orderby=year desc,title desc",
			100,
			"tables/movies?$count=true&$orderby=year desc,title desc&$skip=100",
			Movies.Count,
			new[] { "id-033", "id-122", "id-188", "id-149", "id-064" }
		);
	}

	[Fact]
	public async Task Query_Test_063()
	{
		await BaseQueryTest(
			"tables/movies?$count=true&$top=125&$filter=((year div 1000.5) eq 2) and (rating eq 'R')",
			2,
			null,
			2,
			new[] { "id-061", "id-173" }
		);
	}

	[Fact]
	public async Task Query_Test_064()
	{
		await BaseQueryTest(
			"tables/movies?$count=true&$top=125&$filter=((year sub 1900) ge 80) and ((year add 10) le 2000) and (duration le 120)",
			13,
			null,
			13,
			new[] { "id-026", "id-047", "id-081", "id-103", "id-121" }
		);
	}

	[Fact]
	public async Task Query_Test_065()
	{
		await BaseQueryTest(
			"tables/movies?$count=true&$top=125&$filter=(year div 1000.5) eq 2",
			6,
			null,
			6,
			new[] { "id-012", "id-042", "id-061", "id-173", "id-194" }
		);
	}

	[Fact]
	public async Task Query_Test_066()
	{
		await BaseQueryTest(
			"tables/movies?$count=true&$top=125&$filter=(year ge 1930 and year le 1940) or (year ge 1950 and year le 1960)",
			46,
			null,
			46,
			new[] { "id-005", "id-016", "id-027", "id-028", "id-031" }
		);
	}

	[Fact]
	public async Task Query_Test_067()
	{
		await BaseQueryTest(
			"tables/movies?$count=true&$top=125&$filter=(year sub 1900) ge 80",
			100,
			"tables/movies?$count=true&$filter=(year sub 1900) ge 80&$skip=100&$top=25",
			138,
			new[] { "id-000", "id-003", "id-006", "id-007", "id-008" }
		);
	}

	[Fact]
	public async Task Query_Test_068()
	{
		await BaseQueryTest(
			"tables/movies?$count=true&$top=125&$filter=bestPictureWinner eq false",
			100,
			"tables/movies?$count=true&$filter=bestPictureWinner eq false&$skip=100&$top=25",
			210,
			new[] { "id-000", "id-003", "id-004", "id-005", "id-006" }
		);
	}

	[Fact]
	public async Task Query_Test_069()
	{
		await BaseQueryTest(
			"tables/movies?$count=true&$top=125&$filter=bestPictureWinner eq true and ceiling(duration div 60.0) eq 2",
			11,
			null,
			11,
			new[] { "id-023", "id-024", "id-112", "id-135", "id-142" }
		);
	}

	[Fact]
	public async Task Query_Test_070()
	{
		await BaseQueryTest(
			"tables/movies?$count=true&$top=125&$filter=bestPictureWinner eq true and floor(duration div 60.0) eq 2",
			21,
			null,
			21,
			new[] { "id-001", "id-011", "id-018", "id-048", "id-051" }
		);
	}

	[Fact]
	public async Task Query_Test_071()
	{
		await BaseQueryTest(
			"tables/movies?$count=true&$top=125&$filter=bestPictureWinner eq true and round(duration div 60.0) eq 2",
			24,
			null,
			24,
			new[] { "id-011", "id-018", "id-023", "id-024", "id-048" }
		);
	}

	[Fact]
	public async Task Query_Test_072()
	{
		await BaseQueryTest(
			"tables/movies?$count=true&$top=125&$filter=bestPictureWinner eq true",
			38,
			null,
			38,
			new[] { "id-001", "id-002", "id-007", "id-008", "id-011" }
		);
	}

	[Fact]
	public async Task Query_Test_073()
	{
		await BaseQueryTest(
			"tables/movies?$count=true&$top=125&$filter=bestPictureWinner ne false",
			38,
			null,
			38,
			new[] { "id-001", "id-002", "id-007", "id-008", "id-011" }
		);
	}

	[Fact]
	public async Task Query_Test_074()
	{
		await BaseQueryTest(
			"tables/movies?$count=true&$top=125&$filter=bestPictureWinner ne true",
			100,
			"tables/movies?$count=true&$filter=bestPictureWinner ne true&$skip=100&$top=25",
			210,
			new[] { "id-000", "id-003", "id-004", "id-005", "id-006" }
		);
	}

	[Fact]
	public async Task Query_Test_075()
	{
		await BaseQueryTest(
			"tables/movies?$count=true&$top=125&$filter=ceiling(duration div 60.0) eq 2",
			100,
			"tables/movies?$count=true&$filter=ceiling(duration div 60.0) eq 2&$skip=100&$top=25",
			124,
			new[] { "id-005", "id-023", "id-024", "id-025", "id-026" }
		);
	}

	[Fact]
	public async Task Query_Test_076()
	{
		await BaseQueryTest(
			"tables/movies?$count=true&$top=125&$filter=day(releaseDate) eq 1",
			7,
			null,
			7,
			new[] { "id-019", "id-048", "id-129", "id-131", "id-132" }
		);
	}

	[Fact]
	public async Task Query_Test_077()
	{
		await BaseQueryTest(
			"tables/movies?$count=true&$top=125&$filter=duration ge 60",
			100,
			"tables/movies?$count=true&$filter=duration ge 60&$skip=100&$top=25",
			Movies.Count,
			new[] { "id-000", "id-001", "id-002", "id-003", "id-004" }
		);
	}

	[Fact]
	public async Task Query_Test_078()
	{
		await BaseQueryTest(
			"tables/movies?$count=true&$top=125&$filter=endswith(title, 'er')",
			12,
			null,
			12,
			new[] { "id-001", "id-052", "id-121", "id-130", "id-164" }
		);
	}

	[Fact]
	public async Task Query_Test_079()
	{
		await BaseQueryTest(
			"tables/movies?$count=true&$top=125&$filter=endswith(tolower(title), 'er')",
			12,
			null,
			12,
			new[] { "id-001", "id-052", "id-121", "id-130", "id-164" }
		);
	}

	[Fact]
	public async Task Query_Test_080()
	{
		await BaseQueryTest(
			"tables/movies?$count=true&$top=125&$filter=endswith(toupper(title), 'ER')",
			12,
			null,
			12,
			new[] { "id-001", "id-052", "id-121", "id-130", "id-164" }
		);
	}

	[Fact]
	public async Task Query_Test_081()
	{
		await BaseQueryTest(
			"tables/movies?$count=true&$top=125&$filter=floor(duration div 60.0) eq 2",
			100,
			"tables/movies?$count=true&$filter=floor(duration div 60.0) eq 2&$skip=100&$top=25",
			120,
			new[] { "id-000", "id-001", "id-003", "id-004", "id-006" }
		);
	}

	[Fact]
	public async Task Query_Test_082()
	{
		await BaseQueryTest(
			"tables/movies?$count=true&$top=125&$filter=month(releaseDate) eq 11",
			14,
			null,
			14,
			new[] { "id-011", "id-016", "id-030", "id-064", "id-085" }
		);
	}

	[Fact]
	public async Task Query_Test_083()
	{
		await BaseQueryTest(
			"tables/movies?$count=true&$top=125&$filter=not(bestPictureWinner eq false)",
			38,
			null,
			38,
			new[] { "id-001", "id-002", "id-007", "id-008", "id-011" }
		);
	}

	[Fact]
	public async Task Query_Test_084()
	{
		await BaseQueryTest(
			"tables/movies?$count=true&$top=125&$filter=not(bestPictureWinner eq true)",
			100,
			"tables/movies?$count=true&$filter=not(bestPictureWinner eq true)&$skip=100&$top=25",
			210,
			new[] { "id-000", "id-003", "id-004", "id-005", "id-006" }
		);
	}

	[Fact]
	public async Task Query_Test_085()
	{
		await BaseQueryTest(
			"tables/movies?$count=true&$top=125&$filter=not(bestPictureWinner ne false)",
			100,
			"tables/movies?$count=true&$filter=not(bestPictureWinner ne false)&$skip=100&$top=25",
			210,
			new[] { "id-000", "id-003", "id-004", "id-005", "id-006" }
		);
	}

	[Fact]
	public async Task Query_Test_086()
	{
		await BaseQueryTest(
			"tables/movies?$count=true&$top=125&$filter=not(bestPictureWinner ne true)",
			38,
			null,
			38,
			new[] { "id-001", "id-002", "id-007", "id-008", "id-011" }
		);
	}

	[Fact]
	public async Task Query_Test_087()
	{
		await BaseQueryTest(
			"tables/movies?$count=true&$top=125&$filter=rating eq 'R'",
			95,
			null,
			95,
			new[] { "id-000", "id-001", "id-002", "id-003", "id-007" }
		);
	}

	[Fact]
	public async Task Query_Test_088()
	{
		await BaseQueryTest(
			"tables/movies?$count=true&$top=125&$filter=rating ne 'PG-13'",
			100,
			"tables/movies?$count=true&$filter=rating ne 'PG-13'&$skip=100&$top=25",
			220,
			new[] { "id-000", "id-001", "id-002", "id-003", "id-004" }
		);
	}

	[Fact]
	public async Task Query_Test_089()
	{
		await BaseQueryTest(
			"tables/movies?$count=true&$top=125&$filter=rating eq null",
			74,
			null,
			74,
			new[] { "id-004", "id-005", "id-011", "id-016", "id-031" }
		);
	}

	[Fact]
	public async Task Query_Test_090()
	{
		await BaseQueryTest(
			"tables/movies?$count=true&$top=125&$filter=releaseDate eq cast(1994-10-14T00:00:00.000Z,Edm.DateTimeOffset)",
			2,
			null,
			2,
			new[] { "id-000", "id-003" }
		);
	}

	[Fact]
	public async Task Query_Test_091()
	{
		await BaseQueryTest(
			"tables/movies?$count=true&$top=125&$filter=releaseDate ge cast(1999-12-31T00:00:00.000Z,Edm.DateTimeOffset)",
			69,
			null,
			69,
			new[] { "id-006", "id-008", "id-012", "id-013", "id-019" }
		);
	}

	[Fact]
	public async Task Query_Test_092()
	{
		await BaseQueryTest(
			"tables/movies?$count=true&$top=125&$filter=releaseDate gt cast(1999-12-31T00:00:00.000Z,Edm.DateTimeOffset)",
			69,
			null,
			69,
			new[] { "id-006", "id-008", "id-012", "id-013", "id-019" }
		);
	}

	[Fact]
	public async Task Query_Test_093()
	{
		await BaseQueryTest(
			"tables/movies?$count=true&$top=125&$filter=releaseDate le cast(2000-01-01T00:00:00.000Z,Edm.DateTimeOffset)",
			100,
			"tables/movies?$count=true&$filter=releaseDate le cast(2000-01-01T00:00:00.000Z,Edm.DateTimeOffset)&$skip=100&$top=25",
			179,
			new[] { "id-000", "id-001", "id-002", "id-003", "id-004" }
		);
	}

	[Fact]
	public async Task Query_Test_094()
	{
		await BaseQueryTest(
			"tables/movies?$count=true&$top=125&$filter=releaseDate lt cast(2000-01-01T00:00:00.000Z,Edm.DateTimeOffset)",
			100,
			"tables/movies?$count=true&$filter=releaseDate lt cast(2000-01-01T00:00:00.000Z,Edm.DateTimeOffset)&$skip=100&$top=25",
			179,
			new[] { "id-000", "id-001", "id-002", "id-003", "id-004" }
		);
	}

	[Fact]
	public async Task Query_Test_095()
	{
		await BaseQueryTest(
			"tables/movies?$count=true&$top=125&$filter=round(duration div 60.0) eq 2",
			100,
			"tables/movies?$count=true&$filter=round(duration div 60.0) eq 2&$skip=100&$top=25",
			186,
			new[] { "id-000", "id-005", "id-009", "id-010", "id-011" }
		);
	}

	[Fact]
	public async Task Query_Test_096()
	{
		await BaseQueryTest(
			"tables/movies?$count=true&$top=125&$filter=startswith(rating, 'PG')",
			68,
			null,
			68,
			new[] { "id-006", "id-008", "id-010", "id-012", "id-013" }
		);
	}

	[Fact]
	public async Task Query_Test_097()
	{
		await BaseQueryTest(
			"tables/movies?$count=true&$top=125&$filter=startswith(tolower(title), 'the')",
			63,
			null,
			63,
			new[] { "id-000", "id-001", "id-002", "id-004", "id-006" }
		);
	}

	[Fact]
	public async Task Query_Test_098()
	{
		await BaseQueryTest(
			"tables/movies?$count=true&$top=125&$filter=startswith(toupper(title), 'THE')",
			63,
			null,
			63,
			new[] { "id-000", "id-001", "id-002", "id-004", "id-006" }
		);
	}

	[Fact]
	public async Task Query_Test_099()
	{
		await BaseQueryTest(
			"tables/movies?$count=true&$top=125&$filter=year eq 1994",
			5,
			null,
			5,
			new[] { "id-000", "id-003", "id-018", "id-030", "id-079" }
		);
	}

	[Fact]
	public async Task Query_Test_100()
	{
		await BaseQueryTest(
			"tables/movies?$count=true&$top=125&$filter=year ge 2000 and year le 2009",
			55,
			null,
			55,
			new[] { "id-006", "id-008", "id-012", "id-019", "id-020" }
		);
	}

	[Fact]
	public async Task Query_Test_101()
	{
		await BaseQueryTest(
			"tables/movies?$count=true&$top=125&$filter=year ge 2000",
			69,
			null,
			69,
			new[] { "id-006", "id-008", "id-012", "id-013", "id-019" }
		);
	}

	[Fact]
	public async Task Query_Test_102()
	{
		await BaseQueryTest(
			"tables/movies?$count=true&$top=125&$filter=year gt 1999 and year lt 2010",
			55,
			null,
			55,
			new[] { "id-006", "id-008", "id-012", "id-019", "id-020" }
		);
	}

	[Fact]
	public async Task Query_Test_103()
	{
		await BaseQueryTest(
			"tables/movies?$count=true&$top=125&$filter=year gt 1999",
			69,
			null,
			69,
			new[] { "id-006", "id-008", "id-012", "id-013", "id-019" }
		);
	}

	[Fact]
	public async Task Query_Test_104()
	{
		await BaseQueryTest(
			"tables/movies?$count=true&$top=125&$filter=year le 2000",
			100,
			"tables/movies?$count=true&$filter=year le 2000&$skip=100&$top=25",
			185,
			new[] { "id-000", "id-001", "id-002", "id-003", "id-004" }
		);
	}

	[Fact]
	public async Task Query_Test_105()
	{
		await BaseQueryTest(
			"tables/movies?$count=true&$top=125&$filter=year lt 2001",
			100,
			"tables/movies?$count=true&$filter=year lt 2001&$skip=100&$top=25",
			185,
			new[] { "id-000", "id-001", "id-002", "id-003", "id-004" }
		);
	}

	[Fact]
	public async Task Query_Test_106()
	{
		await BaseQueryTest(
			"tables/movies?$count=true&$top=125&$filter=year(releaseDate) eq 1994",
			6,
			null,
			6,
			new[] { "id-000", "id-003", "id-018", "id-030", "id-079" }
		);
	}

	[Fact]
	public async Task Query_Test_107()
	{
		await BaseQueryTest(
			"tables/movies?$count=true&$top=125&$orderby=bestPictureWinner asc",
			100,
			"tables/movies?$count=true&$orderby=bestPictureWinner asc&$skip=100&$top=25",
			Movies.Count,
			new[] { "id-000", "id-003", "id-004", "id-005", "id-006" }
		);
	}

	[Fact]
	public async Task Query_Test_108()
	{
		await BaseQueryTest(
			"tables/movies?$count=true&$top=125&$orderby=bestPictureWinner desc",
			100,
			"tables/movies?$count=true&$orderby=bestPictureWinner desc&$skip=100&$top=25",
			Movies.Count,
			new[] { "id-001", "id-002", "id-007", "id-008", "id-011" }
		);
	}

	[Fact]
	public async Task Query_Test_109()
	{
		await BaseQueryTest(
			"tables/movies?$count=true&$top=125&$orderby=duration asc",
			100,
			"tables/movies?$count=true&$orderby=duration asc&$skip=100&$top=25",
			Movies.Count,
			new[] { "id-227", "id-125", "id-133", "id-107", "id-118" }
		);
	}

	[Fact]
	public async Task Query_Test_110()
	{
		await BaseQueryTest(
			"tables/movies?$count=true&$top=125&$orderby=duration desc",
			100,
			"tables/movies?$count=true&$orderby=duration desc&$skip=100&$top=25",
			Movies.Count,
			new[] { "id-153", "id-065", "id-165", "id-008", "id-002" }
		);
	}

	[Fact]
	public async Task Query_Test_111()
	{
		await BaseQueryTest(
			"tables/movies?$count=true&$top=125&$orderby=rating asc",
			100,
			"tables/movies?$count=true&$orderby=rating asc&$skip=100&$top=25",
			Movies.Count,
			new[] { "id-004", "id-005", "id-011", "id-016", "id-031" }
		);
	}

	[Fact]
	public async Task Query_Test_112()
	{
		await BaseQueryTest(
			"tables/movies?$count=true&$top=125&$orderby=rating desc",
			100,
			"tables/movies?$count=true&$orderby=rating desc&$skip=100&$top=25",
			Movies.Count,
			new[] { "id-000", "id-001", "id-002", "id-003", "id-007" }
		);
	}

	[Fact]
	public async Task Query_Test_113()
	{
		await BaseQueryTest(
			"tables/movies?$count=true&$top=125&$orderby=releaseDate asc",
			100,
			"tables/movies?$count=true&$orderby=releaseDate asc&$skip=100&$top=25",
			Movies.Count,
			new[] { "id-125", "id-133", "id-227", "id-118", "id-088" }
		);
	}

	[Fact]
	public async Task Query_Test_114()
	{
		await BaseQueryTest(
			"tables/movies?$count=true&$top=125&$orderby=releaseDate desc",
			100,
			"tables/movies?$count=true&$orderby=releaseDate desc&$skip=100&$top=25",
			Movies.Count,
			new[] { "id-188", "id-033", "id-122", "id-186", "id-064" }
		);
	}

	[Fact]
	public async Task Query_Test_115()
	{
		await BaseQueryTest(
			"tables/movies?$count=true&$top=125&$orderby=title asc",
			100,
			"tables/movies?$count=true&$orderby=title asc&$skip=100&$top=25",
			Movies.Count,
			new[] { "id-005", "id-091", "id-243", "id-194", "id-060" }
		);
	}

	[Fact]
	public async Task Query_Test_116()
	{
		await BaseQueryTest(
			"tables/movies?$count=true&$top=125&$orderby=title desc",
			100,
			"tables/movies?$count=true&$orderby=title desc&$skip=100&$top=25",
			Movies.Count,
			new[] { "id-107", "id-100", "id-123", "id-190", "id-149" }
		);
	}

	[Fact]
	public async Task Query_Test_117()
	{
		await BaseQueryTest(
			"tables/movies?$count=true&$top=125&$orderby=year asc",
			100,
			"tables/movies?$count=true&$orderby=year asc&$skip=100&$top=25",
			Movies.Count,
			new[] { "id-125", "id-229", "id-133", "id-227", "id-118" }
		);
	}

	[Fact]
	public async Task Query_Test_118()
	{
		await BaseQueryTest(
			"tables/movies?$count=true&$top=125&$orderby=year asc,title asc",
			100,
			"tables/movies?$count=true&$orderby=year asc,title asc&$skip=100&$top=25",
			Movies.Count,
			new[] { "id-125", "id-229", "id-227", "id-133", "id-118" }
		);
	}

	[Fact]
	public async Task Query_Test_119()
	{
		await BaseQueryTest(
			"tables/movies?$count=true&$top=125&$orderby=year asc,title desc",
			100,
			"tables/movies?$count=true&$orderby=year asc,title desc&$skip=100&$top=25",
			Movies.Count,
			new[] { "id-125", "id-229", "id-133", "id-227", "id-118" }
		);
	}

	[Fact]
	public async Task Query_Test_120()
	{
		await BaseQueryTest(
			"tables/movies?$count=true&$top=125&$orderby=year desc",
			100,
			"tables/movies?$count=true&$orderby=year desc&$skip=100&$top=25",
			Movies.Count,
			new[] { "id-033", "id-122", "id-188", "id-064", "id-102" }
		);
	}

	[Fact]
	public async Task Query_Test_121()
	{
		await BaseQueryTest(
			"tables/movies?$count=true&$top=125&$orderby=year desc,title asc",
			100,
			"tables/movies?$count=true&$orderby=year desc,title asc&$skip=100&$top=25",
			Movies.Count,
			new[] { "id-188", "id-122", "id-033", "id-102", "id-213" }
		);
	}

	[Fact]
	public async Task Query_Test_122()
	{
		await BaseQueryTest(
			"tables/movies?$count=true&$top=125&$orderby=year desc,title desc",
			100,
			"tables/movies?$count=true&$orderby=year desc,title desc&$skip=100&$top=25",
			Movies.Count,
			new[] { "id-033", "id-122", "id-188", "id-149", "id-064" }
		);
	}

	[Fact]
	public async Task Query_Test_123()
	{
		await BaseQueryTest(
			"tables/movies?$filter=((year div 1000.5) eq 2) and (rating eq 'R')",
			2,
			null,
			null,
			new[] { "id-061", "id-173" }
		);
	}

	[Fact]
	public async Task Query_Test_124()
	{
		await BaseQueryTest(
			"tables/movies?$filter=((year sub 1900) ge 80) and ((year add 10) le 2000) and (duration le 120)",
			13,
			null,
			null,
			new[] { "id-026", "id-047", "id-081", "id-103", "id-121" }
		);
	}

	[Fact]
	public async Task Query_Test_125()
	{
		await BaseQueryTest(
			"tables/movies?$filter=(year div 1000.5) eq 2",
			6,
			null,
			null,
			new[] { "id-012", "id-042", "id-061", "id-173", "id-194" }
		);
	}

	[Fact]
	public async Task Query_Test_126()
	{
		await BaseQueryTest(
			"tables/movies?$filter=(year ge 1930 and year le 1940) or (year ge 1950 and year le 1960)",
			46,
			null,
			null,
			new[] { "id-005", "id-016", "id-027", "id-028", "id-031" }
		);
	}

	[Fact]
	public async Task Query_Test_127()
	{
		await BaseQueryTest(
			"tables/movies?$filter=(year sub 1900) ge 80",
			100,
			"tables/movies?$filter=(year sub 1900) ge 80&$skip=100",
			null,
			new[] { "id-000", "id-003", "id-006", "id-007", "id-008" }
		);
	}

	[Fact]
	public async Task Query_Test_128()
	{
		await BaseQueryTest(
			"tables/movies?$filter=bestPictureWinner eq false",
			100,
			"tables/movies?$filter=bestPictureWinner eq false&$skip=100",
			null,
			new[] { "id-000", "id-003", "id-004", "id-005", "id-006" }
		);
	}

	[Fact]
	public async Task Query_Test_129()
	{
		await BaseQueryTest(
			"tables/movies?$filter=bestPictureWinner eq true and ceiling(duration div 60.0) eq 2",
			11,
			null,
			null,
			new[] { "id-023", "id-024", "id-112", "id-135", "id-142" }
		);
	}

	[Fact]
	public async Task Query_Test_130()
	{
		await BaseQueryTest(
			"tables/movies?$filter=bestPictureWinner eq true and floor(duration div 60.0) eq 2",
			21,
			null,
			null,
			new[] { "id-001", "id-011", "id-018", "id-048", "id-051" }
		);
	}

	[Fact]
	public async Task Query_Test_131()
	{
		await BaseQueryTest(
			"tables/movies?$filter=bestPictureWinner eq true and round(duration div 60.0) eq 2",
			24,
			null,
			null,
			new[] { "id-011", "id-018", "id-023", "id-024", "id-048" }
		);
	}

	[Fact]
	public async Task Query_Test_132()
	{
		await BaseQueryTest(
			"tables/movies?$filter=bestPictureWinner eq true",
			38,
			null,
			null,
			new[] { "id-001", "id-002", "id-007", "id-008", "id-011" }
		);
	}

	[Fact]
	public async Task Query_Test_133()
	{
		await BaseQueryTest(
			"tables/movies?$filter=bestPictureWinner ne false",
			38,
			null,
			null,
			new[] { "id-001", "id-002", "id-007", "id-008", "id-011" }
		);
	}

	[Fact]
	public async Task Query_Test_134()
	{
		await BaseQueryTest(
			"tables/movies?$filter=bestPictureWinner ne true",
			100,
			"tables/movies?$filter=bestPictureWinner ne true&$skip=100",
			null,
			new[] { "id-000", "id-003", "id-004", "id-005", "id-006" }
		);
	}

	[Fact]
	public async Task Query_Test_135()
	{
		await BaseQueryTest(
			"tables/movies?$filter=ceiling(duration div 60.0) eq 2",
			100,
			"tables/movies?$filter=ceiling(duration div 60.0) eq 2&$skip=100",
			null,
			new[] { "id-005", "id-023", "id-024", "id-025", "id-026" }
		);
	}

	[Fact]
	public async Task Query_Test_136()
	{
		await BaseQueryTest(
			"tables/movies?$filter=day(releaseDate) eq 1",
			7,
			null,
			null,
			new[] { "id-019", "id-048", "id-129", "id-131", "id-132" }
		);
	}

	[Fact]
	public async Task Query_Test_137()
	{
		await BaseQueryTest(
			"tables/movies?$filter=duration ge 60",
			100,
			"tables/movies?$filter=duration ge 60&$skip=100",
			null,
			new[] { "id-000", "id-001", "id-002", "id-003", "id-004" }
		);
	}

	[Fact]
	public async Task Query_Test_138()
	{
		await BaseQueryTest(
			"tables/movies?$filter=endswith(title, 'er')",
			12,
			null,
			null,
			new[] { "id-001", "id-052", "id-121", "id-130", "id-164" }
		);
	}

	[Fact]
	public async Task Query_Test_139()
	{
		await BaseQueryTest(
			"tables/movies?$filter=endswith(tolower(title), 'er')",
			12,
			null,
			null,
			new[] { "id-001", "id-052", "id-121", "id-130", "id-164" }
		);
	}

	[Fact]
	public async Task Query_Test_140()
	{
		await BaseQueryTest(
			"tables/movies?$filter=endswith(toupper(title), 'ER')",
			12,
			null,
			null,
			new[] { "id-001", "id-052", "id-121", "id-130", "id-164" }
		);
	}

	[Fact]
	public async Task Query_Test_141()
	{
		await BaseQueryTest(
			"tables/movies?$filter=floor(duration div 60.0) eq 2",
			100,
			"tables/movies?$filter=floor(duration div 60.0) eq 2&$skip=100",
			null,
			new[] { "id-000", "id-001", "id-003", "id-004", "id-006" }
		);
	}

	[Fact]
	public async Task Query_Test_142()
	{
		await BaseQueryTest(
			"tables/movies?$filter=month(releaseDate) eq 11",
			14,
			null,
			null,
			new[] { "id-011", "id-016", "id-030", "id-064", "id-085" }
		);
	}

	[Fact]
	public async Task Query_Test_143()
	{
		await BaseQueryTest(
			"tables/movies?$filter=not(bestPictureWinner eq false)",
			38,
			null,
			null,
			new[] { "id-001", "id-002", "id-007", "id-008", "id-011" }
		);
	}

	[Fact]
	public async Task Query_Test_144()
	{
		await BaseQueryTest(
			"tables/movies?$filter=not(bestPictureWinner eq true)",
			100,
			"tables/movies?$filter=not(bestPictureWinner eq true)&$skip=100",
			null,
			new[] { "id-000", "id-003", "id-004", "id-005", "id-006" }
		);
	}

	[Fact]
	public async Task Query_Test_145()
	{
		await BaseQueryTest(
			"tables/movies?$filter=not(bestPictureWinner ne false)",
			100,
			"tables/movies?$filter=not(bestPictureWinner ne false)&$skip=100",
			null,
			new[] { "id-000", "id-003", "id-004", "id-005", "id-006" }
		);
	}

	[Fact]
	public async Task Query_Test_146()
	{
		await BaseQueryTest(
			"tables/movies?$filter=not(bestPictureWinner ne true)",
			38,
			null,
			null,
			new[] { "id-001", "id-002", "id-007", "id-008", "id-011" }
		);
	}

	[Fact]
	public async Task Query_Test_147()
	{
		await BaseQueryTest(
			"tables/movies?$filter=rating eq 'R'",
			95,
			null,
			null,
			new[] { "id-000", "id-001", "id-002", "id-003", "id-007" }
		);
	}

	[Fact]
	public async Task Query_Test_148()
	{
		await BaseQueryTest(
			"tables/movies?$filter=rating ne 'PG-13'",
			100,
			"tables/movies?$filter=rating ne 'PG-13'&$skip=100",
			null,
			new[] { "id-000", "id-001", "id-002", "id-003", "id-004" }
		);
	}

	[Fact]
	public async Task Query_Test_149()
	{
		await BaseQueryTest(
			"tables/movies?$filter=rating eq null",
			74,
			null,
			null,
			new[] { "id-004", "id-005", "id-011", "id-016", "id-031" }
		);
	}

	[Fact]
	public async Task Query_Test_150()
	{
		await BaseQueryTest(
			"tables/movies?$filter=releaseDate eq cast(1994-10-14T00:00:00.000Z,Edm.DateTimeOffset)",
			2,
			null,
			null,
			new[] { "id-000", "id-003" }
		);
	}

	[Fact]
	public async Task Query_Test_151()
	{
		await BaseQueryTest(
			"tables/movies?$filter=releaseDate ge cast(1999-12-31T00:00:00.000Z,Edm.DateTimeOffset)",
			69,
			null,
			null,
			new[] { "id-006", "id-008", "id-012", "id-013", "id-019" }
		);
	}

	[Fact]
	public async Task Query_Test_152()
	{
		await BaseQueryTest(
			"tables/movies?$filter=releaseDate gt cast(1999-12-31T00:00:00.000Z,Edm.DateTimeOffset)",
			69,
			null,
			null,
			new[] { "id-006", "id-008", "id-012", "id-013", "id-019" }
		);
	}

	[Fact]
	public async Task Query_Test_153()
	{
		await BaseQueryTest(
			"tables/movies?$filter=releaseDate le cast(2000-01-01T00:00:00.000Z,Edm.DateTimeOffset)",
			100,
			"tables/movies?$filter=releaseDate le cast(2000-01-01T00:00:00.000Z,Edm.DateTimeOffset)&$skip=100",
			null,
			new[] { "id-000", "id-001", "id-002", "id-003", "id-004" }
		);
	}

	[Fact]
	public async Task Query_Test_154()
	{
		await BaseQueryTest(
			"tables/movies?$filter=releaseDate lt cast(2000-01-01T00:00:00.000Z,Edm.DateTimeOffset)",
			100,
			"tables/movies?$filter=releaseDate lt cast(2000-01-01T00:00:00.000Z,Edm.DateTimeOffset)&$skip=100",
			null,
			new[] { "id-000", "id-001", "id-002", "id-003", "id-004" }
		);
	}

	[Fact]
	public async Task Query_Test_155()
	{
		await BaseQueryTest(
			"tables/movies?$filter=round(duration div 60.0) eq 2",
			100,
			"tables/movies?$filter=round(duration div 60.0) eq 2&$skip=100",
			null,
			new[] { "id-000", "id-005", "id-009", "id-010", "id-011" }
		);
	}

	[Fact]
	public async Task Query_Test_156()
	{
		await BaseQueryTest(
			"tables/movies?$filter=startswith(rating, 'PG')",
			68,
			null,
			null,
			new[] { "id-006", "id-008", "id-010", "id-012", "id-013" }
		);
	}

	[Fact]
	public async Task Query_Test_157()
	{
		await BaseQueryTest(
			"tables/movies?$filter=startswith(tolower(title), 'the')",
			63,
			null,
			null,
			new[] { "id-000", "id-001", "id-002", "id-004", "id-006" }
		);
	}

	[Fact]
	public async Task Query_Test_158()
	{
		await BaseQueryTest(
			"tables/movies?$filter=startswith(toupper(title), 'THE')",
			63,
			null,
			null,
			new[] { "id-000", "id-001", "id-002", "id-004", "id-006" }
		);
	}

	[Fact]
	public async Task Query_Test_159()
	{
		await BaseQueryTest(
			"tables/movies?$filter=year eq 1994",
			5,
			null,
			null,
			new[] { "id-000", "id-003", "id-018", "id-030", "id-079" }
		);
	}

	[Fact]
	public async Task Query_Test_160()
	{
		await BaseQueryTest(
			"tables/movies?$filter=year ge 2000 and year le 2009",
			55,
			null,
			null,
			new[] { "id-006", "id-008", "id-012", "id-019", "id-020" }
		);
	}

	[Fact]
	public async Task Query_Test_161()
	{
		await BaseQueryTest(
			"tables/movies?$filter=year ge 2000",
			69,
			null,
			null,
			new[] { "id-006", "id-008", "id-012", "id-013", "id-019" }
		);
	}

	[Fact]
	public async Task Query_Test_162()
	{
		await BaseQueryTest(
			"tables/movies?$filter=year gt 1999 and year lt 2010",
			55,
			null,
			null,
			new[] { "id-006", "id-008", "id-012", "id-019", "id-020" }
		);
	}

	[Fact]
	public async Task Query_Test_163()
	{
		await BaseQueryTest(
			"tables/movies?$filter=year gt 1999",
			69,
			null,
			null,
			new[] { "id-006", "id-008", "id-012", "id-013", "id-019" }
		);
	}

	[Fact]
	public async Task Query_Test_164()
	{
		await BaseQueryTest(
			"tables/movies?$filter=year le 2000",
			100,
			"tables/movies?$filter=year le 2000&$skip=100",
			null,
			new[] { "id-000", "id-001", "id-002", "id-003", "id-004" }
		);
	}

	[Fact]
	public async Task Query_Test_165()
	{
		await BaseQueryTest(
			"tables/movies?$filter=year lt 2001",
			100,
			"tables/movies?$filter=year lt 2001&$skip=100",
			null,
			new[] { "id-000", "id-001", "id-002", "id-003", "id-004" }
		);
	}

	[Fact]
	public async Task Query_Test_166()
	{
		await BaseQueryTest(
			"tables/movies?$filter=year(releaseDate) eq 1994",
			6,
			null,
			null,
			new[] { "id-000", "id-003", "id-018", "id-030", "id-079" }
		);
	}

	[Fact]
	public async Task Query_Test_167()
	{
		await BaseQueryTest(
			"tables/movies?$orderby=bestPictureWinner asc",
			100,
			"tables/movies?$orderby=bestPictureWinner asc&$skip=100",
			null,
			new[] { "id-000", "id-003", "id-004", "id-005", "id-006" }
		);
	}

	[Fact]
	public async Task Query_Test_168()
	{
		await BaseQueryTest(
			"tables/movies?$orderby=bestPictureWinner desc",
			100,
			"tables/movies?$orderby=bestPictureWinner desc&$skip=100",
			null,
			new[] { "id-001", "id-002", "id-007", "id-008", "id-011" }
		);
	}

	[Fact]
	public async Task Query_Test_169()
	{
		await BaseQueryTest(
			"tables/movies?$orderby=duration asc",
			100,
			"tables/movies?$orderby=duration asc&$skip=100",
			null,
			new[] { "id-227", "id-125", "id-133", "id-107", "id-118" }
		);
	}

	[Fact]
	public async Task Query_Test_170()
	{
		await BaseQueryTest(
			"tables/movies?$orderby=duration desc",
			100,
			"tables/movies?$orderby=duration desc&$skip=100",
			null,
			new[] { "id-153", "id-065", "id-165", "id-008", "id-002" }
		);
	}

	[Fact]
	public async Task Query_Test_171()
	{
		await BaseQueryTest(
			"tables/movies?$orderby=rating asc",
			100,
			"tables/movies?$orderby=rating asc&$skip=100",
			null,
			new[] { "id-004", "id-005", "id-011", "id-016", "id-031" }
		);
	}

	[Fact]
	public async Task Query_Test_172()
	{
		await BaseQueryTest(
			"tables/movies?$orderby=rating desc",
			100,
			"tables/movies?$orderby=rating desc&$skip=100",
			null,
			new[] { "id-000", "id-001", "id-002", "id-003", "id-007" }
		);
	}

	[Fact]
	public async Task Query_Test_173()
	{
		await BaseQueryTest(
			"tables/movies?$orderby=releaseDate asc",
			100,
			"tables/movies?$orderby=releaseDate asc&$skip=100",
			null,
			new[] { "id-125", "id-133", "id-227", "id-118", "id-088" }
		);
	}

	[Fact]
	public async Task Query_Test_174()
	{
		await BaseQueryTest(
			"tables/movies?$orderby=releaseDate desc",
			100,
			"tables/movies?$orderby=releaseDate desc&$skip=100",
			null,
			new[] { "id-188", "id-033", "id-122", "id-186", "id-064" }
		);
	}

	[Fact]
	public async Task Query_Test_175()
	{
		await BaseQueryTest(
			"tables/movies?$orderby=title asc",
			100,
			"tables/movies?$orderby=title asc&$skip=100",
			null,
			new[] { "id-005", "id-091", "id-243", "id-194", "id-060" }
		);
	}

	[Fact]
	public async Task Query_Test_176()
	{
		await BaseQueryTest(
			"tables/movies?$orderby=title desc",
			100,
			"tables/movies?$orderby=title desc&$skip=100",
			null,
			new[] { "id-107", "id-100", "id-123", "id-190", "id-149" }
		);
	}

	[Fact]
	public async Task Query_Test_177()
	{
		await BaseQueryTest(
			"tables/movies?$orderby=year asc",
			100,
			"tables/movies?$orderby=year asc&$skip=100",
			null,
			new[] { "id-125", "id-229", "id-133", "id-227", "id-118" }
		);
	}

	[Fact]
	public async Task Query_Test_178()
	{
		await BaseQueryTest(
			"tables/movies?$orderby=year asc,title asc",
			100,
			"tables/movies?$orderby=year asc,title asc&$skip=100",
			null,
			new[] { "id-125", "id-229", "id-227", "id-133", "id-118" }
		);
	}

	[Fact]
	public async Task Query_Test_179()
	{
		await BaseQueryTest(
			"tables/movies?$orderby=year asc,title desc",
			100,
			"tables/movies?$orderby=year asc,title desc&$skip=100",
			null,
			new[] { "id-125", "id-229", "id-133", "id-227", "id-118" }
		);
	}

	[Fact]
	public async Task Query_Test_180()
	{
		await BaseQueryTest(
			"tables/movies?$orderby=year desc",
			100,
			"tables/movies?$orderby=year desc&$skip=100",
			null,
			new[] { "id-033", "id-122", "id-188", "id-064", "id-102" }
		);
	}

	[Fact]
	public async Task Query_Test_181()
	{
		await BaseQueryTest(
			"tables/movies?$orderby=year desc,title asc",
			100,
			"tables/movies?$orderby=year desc,title asc&$skip=100",
			null,
			new[] { "id-188", "id-122", "id-033", "id-102", "id-213" }
		);
	}

	[Fact]
	public async Task Query_Test_182()
	{
		await BaseQueryTest(
			"tables/movies?$orderby=year desc,title desc",
			100,
			"tables/movies?$orderby=year desc,title desc&$skip=100",
			null,
			new[] { "id-033", "id-122", "id-188", "id-149", "id-064" }
		);
	}

	[Fact]
	public async Task Query_Test_183()
	{
		await BaseQueryTest(
			"tables/movies?$filter=((year div 1000.5) eq 2) and (rating eq 'R')&$skip=5",
			0,
			null,
			null, 
			Array.Empty<string>());
	}

	[Fact]
	public async Task Query_Test_184()
	{
		await BaseQueryTest(
			"tables/movies?$filter=((year sub 1900) ge 80) and ((year add 10) le 2000) and (duration le 120)&$skip=5",
			8,
			null,
			null,
			new[] { "id-142", "id-143", "id-162", "id-166", "id-172" }
		);
	}

	[Fact]
	public async Task Query_Test_185()
	{
		await BaseQueryTest(
			"tables/movies?$filter=(year div 1000.5) eq 2&$skip=5",
			1,
			null,
			null,
			new[] { "id-216" }
		);
	}

	[Fact]
	public async Task Query_Test_186()
	{
		await BaseQueryTest(
			"tables/movies?$filter=(year ge 1930 and year le 1940) or (year ge 1950 and year le 1960)&$skip=5",
			41,
			null,
			null,
			new[] { "id-040", "id-041", "id-044", "id-046", "id-049" }
		);
	}

	[Fact]
	public async Task Query_Test_187()
	{
		await BaseQueryTest(
			"tables/movies?$filter=(year sub 1900) ge 80&$skip=5",
			100,
			"tables/movies?$filter=(year sub 1900) ge 80&$skip=105",
			null,
			new[] { "id-009", "id-010", "id-012", "id-013", "id-014" }
		);
	}

	[Fact]
	public async Task Query_Test_188()
	{
		await BaseQueryTest(
			"tables/movies?$filter=bestPictureWinner eq false&$skip=5",
			100,
			"tables/movies?$filter=bestPictureWinner eq false&$skip=105",
			null,
			new[] { "id-009", "id-010", "id-012", "id-013", "id-014" }
		);
	}

	[Fact]
	public async Task Query_Test_189()
	{
		await BaseQueryTest(
			"tables/movies?$filter=bestPictureWinner eq true and ceiling(duration div 60.0) eq 2&$skip=5",
			6,
			null,
			null,
			new[] { "id-150", "id-155", "id-186", "id-189", "id-196" }
		);
	}

	[Fact]
	public async Task Query_Test_190()
	{
		await BaseQueryTest(
			"tables/movies?$filter=bestPictureWinner eq true and floor(duration div 60.0) eq 2&$skip=5",
			16,
			null,
			null,
			new[] { "id-062", "id-083", "id-087", "id-092", "id-093" }
		);
	}

	[Fact]
	public async Task Query_Test_191()
	{
		await BaseQueryTest(
			"tables/movies?$filter=bestPictureWinner eq true and round(duration div 60.0) eq 2&$skip=5",
			19,
			null,
			null,
			new[] { "id-092", "id-093", "id-094", "id-096", "id-112" }
		);
	}

	[Fact]
	public async Task Query_Test_192()
	{
		await BaseQueryTest(
			"tables/movies?$filter=bestPictureWinner eq true&$skip=5",
			33,
			null,
			null,
			new[] { "id-018", "id-023", "id-024", "id-048", "id-051" }
		);
	}

	[Fact]
	public async Task Query_Test_193()
	{
		await BaseQueryTest(
			"tables/movies?$filter=bestPictureWinner ne false&$skip=5",
			33,
			null,
			null,
			new[] { "id-018", "id-023", "id-024", "id-048", "id-051" }
		);
	}

	[Fact]
	public async Task Query_Test_194()
	{
		await BaseQueryTest(
			"tables/movies?$filter=bestPictureWinner ne true&$skip=5",
			100,
			"tables/movies?$filter=bestPictureWinner ne true&$skip=105",
			null,
			new[] { "id-009", "id-010", "id-012", "id-013", "id-014" }
		);
	}

	[Fact]
	public async Task Query_Test_195()
	{
		await BaseQueryTest(
			"tables/movies?$filter=ceiling(duration div 60.0) eq 2&$skip=5",
			100,
			"tables/movies?$filter=ceiling(duration div 60.0) eq 2&$skip=105",
			null,
			new[] { "id-027", "id-028", "id-030", "id-031", "id-032" }
		);
	}

	[Fact]
	public async Task Query_Test_196()
	{
		await BaseQueryTest(
			"tables/movies?$filter=day(releaseDate) eq 1&$skip=5",
			2,
			null,
			null,
			new[] { "id-197", "id-215" }
		);
	}

	[Fact]
	public async Task Query_Test_197()
	{
		await BaseQueryTest(
			"tables/movies?$filter=duration ge 60&$skip=5",
			100,
			"tables/movies?$filter=duration ge 60&$skip=105",
			null,
			new[] { "id-005", "id-006", "id-007", "id-008", "id-009" }
		);
	}

	[Fact]
	public async Task Query_Test_198()
	{
		await BaseQueryTest(
			"tables/movies?$filter=endswith(title, 'er')&$skip=5",
			7,
			null,
			null,
			new[] { "id-170", "id-193", "id-197", "id-205", "id-217" }
		);
	}

	[Fact]
	public async Task Query_Test_199()
	{
		await BaseQueryTest(
			"tables/movies?$filter=endswith(tolower(title), 'er')&$skip=5",
			7,
			null,
			null,
			new[] { "id-170", "id-193", "id-197", "id-205", "id-217" }
		);
	}

	[Fact]
	public async Task Query_Test_200()
	{
		await BaseQueryTest(
			"tables/movies?$filter=endswith(toupper(title), 'ER')&$skip=5",
			7,
			null,
			null,
			new[] { "id-170", "id-193", "id-197", "id-205", "id-217" }
		);
	}

	[Fact]
	public async Task Query_Test_201()
	{
		await BaseQueryTest(
			"tables/movies?$filter=floor(duration div 60.0) eq 2&$skip=5",
			100,
			"tables/movies?$filter=floor(duration div 60.0) eq 2&$skip=105",
			null,
			new[] { "id-009", "id-010", "id-011", "id-012", "id-013" }
		);
	}

	[Fact]
	public async Task Query_Test_202()
	{
		await BaseQueryTest(
			"tables/movies?$filter=month(releaseDate) eq 11&$skip=5",
			9,
			null,
			null,
			new[] { "id-115", "id-131", "id-136", "id-146", "id-167" }
		);
	}

	[Fact]
	public async Task Query_Test_203()
	{
		await BaseQueryTest(
			"tables/movies?$filter=not(bestPictureWinner eq false)&$skip=5",
			33,
			null,
			null,
			new[] { "id-018", "id-023", "id-024", "id-048", "id-051" }
		);
	}

	[Fact]
	public async Task Query_Test_204()
	{
		await BaseQueryTest(
			"tables/movies?$filter=not(bestPictureWinner eq true)&$skip=5",
			100,
			"tables/movies?$filter=not(bestPictureWinner eq true)&$skip=105",
			null,
			new[] { "id-009", "id-010", "id-012", "id-013", "id-014" }
		);
	}

	[Fact]
	public async Task Query_Test_205()
	{
		await BaseQueryTest(
			"tables/movies?$filter=not(bestPictureWinner ne false)&$skip=5",
			100,
			"tables/movies?$filter=not(bestPictureWinner ne false)&$skip=105",
			null,
			new[] { "id-009", "id-010", "id-012", "id-013", "id-014" }
		);
	}

	[Fact]
	public async Task Query_Test_206()
	{
		await BaseQueryTest(
			"tables/movies?$filter=not(bestPictureWinner ne true)&$skip=5",
			33,
			null,
			null,
			new[] { "id-018", "id-023", "id-024", "id-048", "id-051" }
		);
	}

	[Fact]
	public async Task Query_Test_207()
	{
		await BaseQueryTest(
			"tables/movies?$filter=rating eq null&$skip=5",
			69,
			null,
			null,
			new[] { "id-040", "id-041", "id-044", "id-046", "id-049" }
		);
	}

	[Fact]
	public async Task Query_Test_208()
	{
		await BaseQueryTest(
			"tables/movies?$filter=rating eq 'R'&$skip=5",
			90,
			null,
			null,
			new[] { "id-009", "id-014", "id-017", "id-019", "id-022" }
		);
	}

	[Fact]
	public async Task Query_Test_209()
	{
		await BaseQueryTest(
			"tables/movies?$filter=rating ne 'PG-13'&$skip=5",
			100,
			"tables/movies?$filter=rating ne 'PG-13'&$skip=105",
			null,
			new[] { "id-005", "id-007", "id-009", "id-010", "id-011" }
		);
	}

	[Fact]
	public async Task Query_Test_210()
	{
		await BaseQueryTest(
			"tables/movies?$filter=releaseDate eq cast(1994-10-14T00:00:00.000Z,Edm.DateTimeOffset)&$skip=5",
			0,
			null,
			null, 
			Array.Empty<string>());
	}

	[Fact]
	public async Task Query_Test_211()
	{
		await BaseQueryTest(
			"tables/movies?$filter=releaseDate ge cast(1999-12-31T00:00:00.000Z,Edm.DateTimeOffset)&$skip=5",
			64,
			null,
			null,
			new[] { "id-020", "id-032", "id-033", "id-042", "id-050" }
		);
	}

	[Fact]
	public async Task Query_Test_212()
	{
		await BaseQueryTest(
			"tables/movies?$filter=releaseDate gt cast(1999-12-31T00:00:00.000Z,Edm.DateTimeOffset)&$skip=5",
			64,
			null,
			null,
			new[] { "id-020", "id-032", "id-033", "id-042", "id-050" }
		);
	}

	[Fact]
	public async Task Query_Test_213()
	{
		await BaseQueryTest(
			"tables/movies?$filter=releaseDate le cast(2000-01-01T00:00:00.000Z,Edm.DateTimeOffset)&$skip=5",
			100,
			"tables/movies?$filter=releaseDate le cast(2000-01-01T00:00:00.000Z,Edm.DateTimeOffset)&$skip=105",
			null,
			new[] { "id-005", "id-007", "id-009", "id-010", "id-011" }
		);
	}

	[Fact]
	public async Task Query_Test_214()
	{
		await BaseQueryTest(
			"tables/movies?$filter=releaseDate lt cast(2000-01-01T00:00:00.000Z,Edm.DateTimeOffset)&$skip=5",
			100,
			"tables/movies?$filter=releaseDate lt cast(2000-01-01T00:00:00.000Z,Edm.DateTimeOffset)&$skip=105",
			null,
			new[] { "id-005", "id-007", "id-009", "id-010", "id-011" }
		);
	}

	[Fact]
	public async Task Query_Test_215()
	{
		await BaseQueryTest(
			"tables/movies?$filter=round(duration div 60.0) eq 2&$skip=5",
			100,
			"tables/movies?$filter=round(duration div 60.0) eq 2&$skip=105",
			null,
			new[] { "id-013", "id-014", "id-015", "id-016", "id-017" }
		);
	}

	[Fact]
	public async Task Query_Test_216()
	{
		await BaseQueryTest(
			"tables/movies?$filter=startswith(rating, 'PG')&$skip=5",
			63,
			null,
			null,
			new[] { "id-015", "id-018", "id-020", "id-021", "id-024" }
		);
	}

	[Fact]
	public async Task Query_Test_217()
	{
		await BaseQueryTest(
			"tables/movies?$filter=startswith(tolower(title), 'the')&$skip=5",
			58,
			null,
			null,
			new[] { "id-008", "id-012", "id-017", "id-020", "id-023" }
		);
	}

	[Fact]
	public async Task Query_Test_218()
	{
		await BaseQueryTest(
			"tables/movies?$filter=startswith(toupper(title), 'THE')&$skip=5",
			58,
			null,
			null,
			new[] { "id-008", "id-012", "id-017", "id-020", "id-023" }
		);
	}

	[Fact]
	public async Task Query_Test_219()
	{
		await BaseQueryTest(
			"tables/movies?$filter=year eq 1994&$skip=5",
			0,
			null,
			null, 
			Array.Empty<string>());
	}

	[Fact]
	public async Task Query_Test_220()
	{
		await BaseQueryTest(
			"tables/movies?$filter=year ge 2000 and year le 2009&$skip=5",
			50,
			null,
			null,
			new[] { "id-032", "id-042", "id-050", "id-051", "id-058" }
		);
	}

	[Fact]
	public async Task Query_Test_221()
	{
		await BaseQueryTest(
			"tables/movies?$filter=year ge 2000&$skip=5",
			64,
			null,
			null,
			new[] { "id-020", "id-032", "id-033", "id-042", "id-050" }
		);
	}

	[Fact]
	public async Task Query_Test_222()
	{
		await BaseQueryTest(
			"tables/movies?$filter=year gt 1999 and year lt 2010&$skip=5",
			50,
			null,
			null,
			new[] { "id-032", "id-042", "id-050", "id-051", "id-058" }
		);
	}

	[Fact]
	public async Task Query_Test_223()
	{
		await BaseQueryTest(
			"tables/movies?$filter=year gt 1999&$skip=5",
			64,
			null,
			null,
			new[] { "id-020", "id-032", "id-033", "id-042", "id-050" }
		);
	}

	[Fact]
	public async Task Query_Test_224()
	{
		await BaseQueryTest(
			"tables/movies?$filter=year le 2000&$skip=5",
			100,
			"tables/movies?$filter=year le 2000&$skip=105",
			null,
			new[] { "id-005", "id-007", "id-009", "id-010", "id-011" }
		);
	}

	[Fact]
	public async Task Query_Test_225()
	{
		await BaseQueryTest(
			"tables/movies?$filter=year lt 2001&$skip=5",
			100,
			"tables/movies?$filter=year lt 2001&$skip=105",
			null,
			new[] { "id-005", "id-007", "id-009", "id-010", "id-011" }
		);
	}

	[Fact]
	public async Task Query_Test_226()
	{
		await BaseQueryTest(
			"tables/movies?$filter=year(releaseDate) eq 1994&$skip=5",
			1,
			null,
			null,
			new[] { "id-217" }
		);
	}

	[Fact]
	public async Task Query_Test_227()
	{
		await BaseQueryTest(
			"tables/movies?$orderby=bestPictureWinner asc&$skip=5",
			100,
			"tables/movies?$orderby=bestPictureWinner asc&$skip=105",
			null,
			new[] { "id-009", "id-010", "id-012", "id-013", "id-014" }
		);
	}

	[Fact]
	public async Task Query_Test_228()
	{
		await BaseQueryTest(
			"tables/movies?$orderby=bestPictureWinner desc&$skip=5",
			100,
			"tables/movies?$orderby=bestPictureWinner desc&$skip=105",
			null,
			new[] { "id-018", "id-023", "id-024", "id-048", "id-051" }
		);
	}

	[Fact]
	public async Task Query_Test_229()
	{
		await BaseQueryTest(
			"tables/movies?$orderby=duration asc&$skip=5",
			100,
			"tables/movies?$orderby=duration asc&$skip=105",
			null,
			new[] { "id-238", "id-201", "id-115", "id-229", "id-181" }
		);
	}

	[Fact]
	public async Task Query_Test_230()
	{
		await BaseQueryTest(
			"tables/movies?$orderby=duration desc&$skip=5",
			100,
			"tables/movies?$orderby=duration desc&$skip=105",
			null,
			new[] { "id-007", "id-183", "id-063", "id-202", "id-130" }
		);
	}

	[Fact]
	public async Task Query_Test_231()
	{
		await BaseQueryTest(
			"tables/movies?$orderby=rating asc&$skip=5",
			100,
			"tables/movies?$orderby=rating asc&$skip=105",
			null,
			new[] { "id-040", "id-041", "id-044", "id-046", "id-049" }
		);
	}

	[Fact]
	public async Task Query_Test_232()
	{
		await BaseQueryTest(
			"tables/movies?$orderby=rating desc&$skip=5",
			100,
			"tables/movies?$orderby=rating desc&$skip=105",
			null,
			new[] { "id-009", "id-014", "id-017", "id-019", "id-022" }
		);
	}

	[Fact]
	public async Task Query_Test_233()
	{
		await BaseQueryTest(
			"tables/movies?$orderby=releaseDate asc&$skip=5",
			100,
			"tables/movies?$orderby=releaseDate asc&$skip=105",
			null,
			new[] { "id-229", "id-224", "id-041", "id-049", "id-135" }
		);
	}

	[Fact]
	public async Task Query_Test_234()
	{
		await BaseQueryTest(
			"tables/movies?$orderby=releaseDate desc&$skip=5",
			100,
			"tables/movies?$orderby=releaseDate desc&$skip=105",
			null,
			new[] { "id-149", "id-213", "id-102", "id-155", "id-169" }
		);
	}

	[Fact]
	public async Task Query_Test_235()
	{
		await BaseQueryTest(
			"tables/movies?$orderby=title asc&$skip=5",
			100,
			"tables/movies?$orderby=title asc&$skip=105",
			null,
			new[] { "id-214", "id-102", "id-215", "id-039", "id-057" }
		);
	}

	[Fact]
	public async Task Query_Test_236()
	{
		await BaseQueryTest(
			"tables/movies?$orderby=title desc&$skip=5",
			100,
			"tables/movies?$orderby=title desc&$skip=105",
			null,
			new[] { "id-058", "id-046", "id-160", "id-092", "id-176" }
		);
	}

	[Fact]
	public async Task Query_Test_237()
	{
		await BaseQueryTest(
			"tables/movies?$orderby=year asc&$skip=5",
			100,
			"tables/movies?$orderby=year asc&$skip=105",
			null,
			new[] { "id-088", "id-224", "id-041", "id-049", "id-135" }
		);
	}

	[Fact]
	public async Task Query_Test_238()
	{
		await BaseQueryTest(
			"tables/movies?$orderby=year asc,title asc&$skip=5",
			100,
			"tables/movies?$orderby=year asc,title asc&$skip=105",
			null,
			new[] { "id-088", "id-224", "id-041", "id-049", "id-135" }
		);
	}

	[Fact]
	public async Task Query_Test_239()
	{
		await BaseQueryTest(
			"tables/movies?$orderby=year asc,title desc&$skip=5",
			100,
			"tables/movies?$orderby=year asc,title desc&$skip=105",
			null,
			new[] { "id-088", "id-224", "id-049", "id-041", "id-135" }
		);
	}

	[Fact]
	public async Task Query_Test_240()
	{
		await BaseQueryTest(
			"tables/movies?$orderby=year desc&$skip=5",
			100,
			"tables/movies?$orderby=year desc&$skip=105",
			null,
			new[] { "id-149", "id-186", "id-213", "id-013", "id-053" }
		);
	}

	[Fact]
	public async Task Query_Test_241()
	{
		await BaseQueryTest(
			"tables/movies?$orderby=year desc,title asc&$skip=5",
			100,
			"tables/movies?$orderby=year desc,title asc&$skip=105",
			null,
			new[] { "id-186", "id-064", "id-149", "id-169", "id-161" }
		);
	}

	[Fact]
	public async Task Query_Test_242()
	{
		await BaseQueryTest(
			"tables/movies?$orderby=year desc,title desc&$skip=5",
			100,
			"tables/movies?$orderby=year desc,title desc&$skip=105",
			null,
			new[] { "id-186", "id-213", "id-102", "id-053", "id-155" }
		);
	}

	[Fact]
	public async Task Query_Test_243()
	{
		await BaseQueryTest(
			"tables/movies?$skip=0",
			100,
			"tables/movies?$skip=100",
			null,
			new[] { "id-000", "id-001", "id-002", "id-003", "id-004" }
		);
	}

	[Fact]
	public async Task Query_Test_244()
	{
		await BaseQueryTest(
			"tables/movies?$skip=100",
			100,
			"tables/movies?$skip=200",
			null,
			new[] { "id-100", "id-101", "id-102", "id-103", "id-104" }
		);
	}

	[Fact]
	public async Task Query_Test_245()
	{
		await BaseQueryTest(
			"tables/movies?$skip=200",
			48,
			null,
			null,
			new[] { "id-200", "id-201", "id-202", "id-203", "id-204" }
		);
	}

	[Fact]
	public async Task Query_Test_246()
	{
		await BaseQueryTest(
			"tables/movies?$skip=300",
			0,
			null,
			null, 
			Array.Empty<string>());
	}

	[Fact]
	public async Task Query_Test_247()
	{
		await BaseQueryTest(
			"tables/movies?$top=5&$filter=((year div 1000.5) eq 2) and (rating eq 'R')",
			2,
			null,
			null,
			new[] { "id-061", "id-173" }
		);
	}

	[Fact]
	public async Task Query_Test_248()
	{
		await BaseQueryTest(
			"tables/movies?$top=5&$filter=((year sub 1900) ge 80) and ((year add 10) le 2000) and (duration le 120)",
			5,
			null,
			null,
			new[] { "id-026", "id-047", "id-081", "id-103", "id-121" }
		);
	}

	[Fact]
	public async Task Query_Test_249()
	{
		await BaseQueryTest(
			"tables/movies?$top=5&$filter=(year div 1000.5) eq 2",
			5,
			null,
			null,
			new[] { "id-012", "id-042", "id-061", "id-173", "id-194" }
		);
	}

	[Fact]
	public async Task Query_Test_250()
	{
		await BaseQueryTest(
			"tables/movies?$top=5&$filter=(year ge 1930 and year le 1940) or (year ge 1950 and year le 1960)",
			5,
			null,
			null,
			new[] { "id-005", "id-016", "id-027", "id-028", "id-031" }
		);
	}

	[Fact]
	public async Task Query_Test_251()
	{
		await BaseQueryTest(
			"tables/movies?$top=5&$filter=(year sub 1900) ge 80",
			5,
			null,
			null,
			new[] { "id-000", "id-003", "id-006", "id-007", "id-008" }
		);
	}

	[Fact]
	public async Task Query_Test_252()
	{
		await BaseQueryTest(
			"tables/movies?$top=5&$filter=bestPictureWinner eq false",
			5,
			null,
			null,
			new[] { "id-000", "id-003", "id-004", "id-005", "id-006" }
		);
	}

	[Fact]
	public async Task Query_Test_253()
	{
		await BaseQueryTest(
			"tables/movies?$top=5&$filter=bestPictureWinner eq true and ceiling(duration div 60.0) eq 2",
			5,
			null,
			null,
			new[] { "id-023", "id-024", "id-112", "id-135", "id-142" }
		);
	}

	[Fact]
	public async Task Query_Test_254()
	{
		await BaseQueryTest(
			"tables/movies?$top=5&$filter=bestPictureWinner eq true and floor(duration div 60.0) eq 2",
			5,
			null,
			null,
			new[] { "id-001", "id-011", "id-018", "id-048", "id-051" }
		);
	}

	[Fact]
	public async Task Query_Test_255()
	{
		await BaseQueryTest(
			"tables/movies?$top=5&$filter=bestPictureWinner eq true and round(duration div 60.0) eq 2",
			5,
			null,
			null,
			new[] { "id-011", "id-018", "id-023", "id-024", "id-048" }
		);
	}

	[Fact]
	public async Task Query_Test_256()
	{
		await BaseQueryTest(
			"tables/movies?$top=5&$filter=bestPictureWinner eq true",
			5,
			null,
			null,
			new[] { "id-001", "id-002", "id-007", "id-008", "id-011" }
		);
	}

	[Fact]
	public async Task Query_Test_257()
	{
		await BaseQueryTest(
			"tables/movies?$top=5&$filter=bestPictureWinner ne false",
			5,
			null,
			null,
			new[] { "id-001", "id-002", "id-007", "id-008", "id-011" }
		);
	}

	[Fact]
	public async Task Query_Test_258()
	{
		await BaseQueryTest(
			"tables/movies?$top=5&$filter=bestPictureWinner ne true",
			5,
			null,
			null,
			new[] { "id-000", "id-003", "id-004", "id-005", "id-006" }
		);
	}

	[Fact]
	public async Task Query_Test_259()
	{
		await BaseQueryTest(
			"tables/movies?$top=5&$filter=ceiling(duration div 60.0) eq 2",
			5,
			null,
			null,
			new[] { "id-005", "id-023", "id-024", "id-025", "id-026" }
		);
	}

	[Fact]
	public async Task Query_Test_260()
	{
		await BaseQueryTest(
			"tables/movies?$top=5&$filter=day(releaseDate) eq 1",
			5,
			null,
			null,
			new[] { "id-019", "id-048", "id-129", "id-131", "id-132" }
		);
	}

	[Fact]
	public async Task Query_Test_261()
	{
		await BaseQueryTest(
			"tables/movies?$top=5&$filter=duration ge 60",
			5,
			null,
			null,
			new[] { "id-000", "id-001", "id-002", "id-003", "id-004" }
		);
	}

	[Fact]
	public async Task Query_Test_262()
	{
		await BaseQueryTest(
			"tables/movies?$top=5&$filter=endswith(title, 'er')",
			5,
			null,
			null,
			new[] { "id-001", "id-052", "id-121", "id-130", "id-164" }
		);
	}

	[Fact]
	public async Task Query_Test_263()
	{
		await BaseQueryTest(
			"tables/movies?$top=5&$filter=endswith(tolower(title), 'er')",
			5,
			null,
			null,
			new[] { "id-001", "id-052", "id-121", "id-130", "id-164" }
		);
	}

	[Fact]
	public async Task Query_Test_264()
	{
		await BaseQueryTest(
			"tables/movies?$top=5&$filter=endswith(toupper(title), 'ER')",
			5,
			null,
			null,
			new[] { "id-001", "id-052", "id-121", "id-130", "id-164" }
		);
	}

	[Fact]
	public async Task Query_Test_265()
	{
		await BaseQueryTest(
			"tables/movies?$top=5&$filter=floor(duration div 60.0) eq 2",
			5,
			null,
			null,
			new[] { "id-000", "id-001", "id-003", "id-004", "id-006" }
		);
	}

	[Fact]
	public async Task Query_Test_266()
	{
		await BaseQueryTest(
			"tables/movies?$top=5&$filter=month(releaseDate) eq 11",
			5,
			null,
			null,
			new[] { "id-011", "id-016", "id-030", "id-064", "id-085" }
		);
	}

	[Fact]
	public async Task Query_Test_267()
	{
		await BaseQueryTest(
			"tables/movies?$top=5&$filter=not(bestPictureWinner eq false)",
			5,
			null,
			null,
			new[] { "id-001", "id-002", "id-007", "id-008", "id-011" }
		);
	}

	[Fact]
	public async Task Query_Test_268()
	{
		await BaseQueryTest(
			"tables/movies?$top=5&$filter=not(bestPictureWinner eq true)",
			5,
			null,
			null,
			new[] { "id-000", "id-003", "id-004", "id-005", "id-006" }
		);
	}

	[Fact]
	public async Task Query_Test_269()
	{
		await BaseQueryTest(
			"tables/movies?$top=5&$filter=not(bestPictureWinner ne false)",
			5,
			null,
			null,
			new[] { "id-000", "id-003", "id-004", "id-005", "id-006" }
		);
	}

	[Fact]
	public async Task Query_Test_270()
	{
		await BaseQueryTest(
			"tables/movies?$top=5&$filter=not(bestPictureWinner ne true)",
			5,
			null,
			null,
			new[] { "id-001", "id-002", "id-007", "id-008", "id-011" }
		);
	}

	[Fact]
	public async Task Query_Test_271()
	{
		await BaseQueryTest(
			"tables/movies?$top=5&$filter=rating eq 'R'",
			5,
			null,
			null,
			new[] { "id-000", "id-001", "id-002", "id-003", "id-007" }
		);
	}

	[Fact]
	public async Task Query_Test_272()
	{
		await BaseQueryTest(
			"tables/movies?$top=5&$filter=rating ne 'PG-13'",
			5,
			null,
			null,
			new[] { "id-000", "id-001", "id-002", "id-003", "id-004" }
		);
	}

	[Fact]
	public async Task Query_Test_273()
	{
		await BaseQueryTest(
			"tables/movies?$top=5&$filter=rating eq null",
			5,
			null,
			null,
			new[] { "id-004", "id-005", "id-011", "id-016", "id-031" }
		);
	}

	[Fact]
	public async Task Query_Test_274()
	{
		await BaseQueryTest(
			"tables/movies?$top=5&$filter=releaseDate eq cast(1994-10-14T00:00:00.000Z,Edm.DateTimeOffset)",
			2,
			null,
			null,
			new[] { "id-000", "id-003" }
		);
	}

	[Fact]
	public async Task Query_Test_275()
	{
		await BaseQueryTest(
			"tables/movies?$top=5&$filter=releaseDate ge cast(1999-12-31T00:00:00.000Z,Edm.DateTimeOffset)",
			5,
			null,
			null,
			new[] { "id-006", "id-008", "id-012", "id-013", "id-019" }
		);
	}

	[Fact]
	public async Task Query_Test_276()
	{
		await BaseQueryTest(
			"tables/movies?$top=5&$filter=releaseDate gt cast(1999-12-31T00:00:00.000Z,Edm.DateTimeOffset)",
			5,
			null,
			null,
			new[] { "id-006", "id-008", "id-012", "id-013", "id-019" }
		);
	}

	[Fact]
	public async Task Query_Test_277()
	{
		await BaseQueryTest(
			"tables/movies?$top=5&$filter=releaseDate le cast(2000-01-01T00:00:00.000Z,Edm.DateTimeOffset)",
			5,
			null,
			null,
			new[] { "id-000", "id-001", "id-002", "id-003", "id-004" }
		);
	}

	[Fact]
	public async Task Query_Test_278()
	{
		await BaseQueryTest(
			"tables/movies?$top=5&$filter=releaseDate lt cast(2000-01-01T00:00:00.000Z,Edm.DateTimeOffset)",
			5,
			null,
			null,
			new[] { "id-000", "id-001", "id-002", "id-003", "id-004" }
		);
	}

	[Fact]
	public async Task Query_Test_279()
	{
		await BaseQueryTest(
			"tables/movies?$top=5&$filter=round(duration div 60.0) eq 2",
			5,
			null,
			null,
			new[] { "id-000", "id-005", "id-009", "id-010", "id-011" }
		);
	}

	[Fact]
	public async Task Query_Test_280()
	{
		await BaseQueryTest(
			"tables/movies?$top=5&$filter=startswith(rating, 'PG')",
			5,
			null,
			null,
			new[] { "id-006", "id-008", "id-010", "id-012", "id-013" }
		);
	}

	[Fact]
	public async Task Query_Test_281()
	{
		await BaseQueryTest(
			"tables/movies?$top=5&$filter=startswith(tolower(title), 'the')",
			5,
			null,
			null,
			new[] { "id-000", "id-001", "id-002", "id-004", "id-006" }
		);
	}

	[Fact]
	public async Task Query_Test_282()
	{
		await BaseQueryTest(
			"tables/movies?$top=5&$filter=startswith(toupper(title), 'THE')",
			5,
			null,
			null,
			new[] { "id-000", "id-001", "id-002", "id-004", "id-006" }
		);
	}

	[Fact]
	public async Task Query_Test_283()
	{
		await BaseQueryTest(
			"tables/movies?$top=5&$filter=year eq 1994",
			5,
			null,
			null,
			new[] { "id-000", "id-003", "id-018", "id-030", "id-079" }
		);
	}

	[Fact]
	public async Task Query_Test_284()
	{
		await BaseQueryTest(
			"tables/movies?$top=5&$filter=year ge 2000 and year le 2009",
			5,
			null,
			null,
			new[] { "id-006", "id-008", "id-012", "id-019", "id-020" }
		);
	}

	[Fact]
	public async Task Query_Test_285()
	{
		await BaseQueryTest(
			"tables/movies?$top=5&$filter=year ge 2000",
			5,
			null,
			null,
			new[] { "id-006", "id-008", "id-012", "id-013", "id-019" }
		);
	}

	[Fact]
	public async Task Query_Test_286()
	{
		await BaseQueryTest(
			"tables/movies?$top=5&$filter=year gt 1999 and year lt 2010",
			5,
			null,
			null,
			new[] { "id-006", "id-008", "id-012", "id-019", "id-020" }
		);
	}

	[Fact]
	public async Task Query_Test_287()
	{
		await BaseQueryTest(
			"tables/movies?$top=5&$filter=year gt 1999",
			5,
			null,
			null,
			new[] { "id-006", "id-008", "id-012", "id-013", "id-019" }
		);
	}

	[Fact]
	public async Task Query_Test_288()
	{
		await BaseQueryTest(
			"tables/movies?$top=5&$filter=year le 2000",
			5,
			null,
			null,
			new[] { "id-000", "id-001", "id-002", "id-003", "id-004" }
		);
	}

	[Fact]
	public async Task Query_Test_289()
	{
		await BaseQueryTest(
			"tables/movies?$top=5&$filter=year lt 2001",
			5,
			null,
			null,
			new[] { "id-000", "id-001", "id-002", "id-003", "id-004" }
		);
	}

	[Fact]
	public async Task Query_Test_290()
	{
		await BaseQueryTest(
			"tables/movies?$top=5&$filter=year(releaseDate) eq 1994",
			5,
			null,
			null,
			new[] { "id-000", "id-003", "id-018", "id-030", "id-079" }
		);
	}

	[Fact]
	public async Task Query_Test_291()
	{
		await BaseQueryTest(
			"tables/movies?$top=5&$orderby=bestPictureWinner asc",
			5,
			null,
			null,
			new[] { "id-000", "id-003", "id-004", "id-005", "id-006" }
		);
	}

	[Fact]
	public async Task Query_Test_292()
	{
		await BaseQueryTest(
			"tables/movies?$top=5&$orderby=bestPictureWinner desc",
			5,
			null,
			null,
			new[] { "id-001", "id-002", "id-007", "id-008", "id-011" }
		);
	}

	[Fact]
	public async Task Query_Test_293()
	{
		await BaseQueryTest(
			"tables/movies?$top=5&$orderby=duration asc",
			5,
			null,
			null,
			new[] { "id-227", "id-125", "id-133", "id-107", "id-118" }
		);
	}

	[Fact]
	public async Task Query_Test_294()
	{
		await BaseQueryTest(
			"tables/movies?$top=5&$orderby=duration desc",
			5,
			null,
			null,
			new[] { "id-153", "id-065", "id-165", "id-008", "id-002" }
		);
	}

	[Fact]
	public async Task Query_Test_295()
	{
		await BaseQueryTest(
			"tables/movies?$top=5&$orderby=rating asc",
			5,
			null,
			null,
			new[] { "id-004", "id-005", "id-011", "id-016", "id-031" }
		);
	}

	[Fact]
	public async Task Query_Test_296()
	{
		await BaseQueryTest(
			"tables/movies?$top=5&$orderby=rating desc",
			5,
			null,
			null,
			new[] { "id-000", "id-001", "id-002", "id-003", "id-007" }
		);
	}

	[Fact]
	public async Task Query_Test_297()
	{
		await BaseQueryTest(
			"tables/movies?$top=5&$orderby=releaseDate asc",
			5,
			null,
			null,
			new[] { "id-125", "id-133", "id-227", "id-118", "id-088" }
		);
	}

	[Fact]
	public async Task Query_Test_298()
	{
		await BaseQueryTest(
			"tables/movies?$top=5&$orderby=releaseDate desc",
			5,
			null,
			null,
			new[] { "id-188", "id-033", "id-122", "id-186", "id-064" }
		);
	}

	[Fact]
	public async Task Query_Test_299()
	{
		await BaseQueryTest(
			"tables/movies?$top=5&$orderby=title asc",
			5,
			null,
			null,
			new[] { "id-005", "id-091", "id-243", "id-194", "id-060" }
		);
	}

	[Fact]
	public async Task Query_Test_300()
	{
		await BaseQueryTest(
			"tables/movies?$top=5&$orderby=title desc",
			5,
			null,
			null,
			new[] { "id-107", "id-100", "id-123", "id-190", "id-149" }
		);
	}

	[Fact]
	public async Task Query_Test_301()
	{
		await BaseQueryTest(
			"tables/movies?$top=5&$orderby=year asc",
			5,
			null,
			null,
			new[] { "id-125", "id-229", "id-133", "id-227", "id-118" }
		);
	}

	[Fact]
	public async Task Query_Test_302()
	{
		await BaseQueryTest(
			"tables/movies?$top=5&$orderby=year asc,title asc",
			5,
			null,
			null,
			new[] { "id-125", "id-229", "id-227", "id-133", "id-118" }
		);
	}

	[Fact]
	public async Task Query_Test_303()
	{
		await BaseQueryTest(
			"tables/movies?$top=5&$orderby=year asc,title desc",
			5,
			null,
			null,
			new[] { "id-125", "id-229", "id-133", "id-227", "id-118" }
		);
	}

	[Fact]
	public async Task Query_Test_304()
	{
		await BaseQueryTest(
			"tables/movies?$top=5&$orderby=year desc",
			5,
			null,
			null,
			new[] { "id-033", "id-122", "id-188", "id-064", "id-102" }
		);
	}

	[Fact]
	public async Task Query_Test_305()
	{
		await BaseQueryTest(
			"tables/movies?$top=5&$orderby=year desc,title asc",
			5,
			null,
			null,
			new[] { "id-188", "id-122", "id-033", "id-102", "id-213" }
		);
	}

	[Fact]
	public async Task Query_Test_306()
	{
		await BaseQueryTest(
			"tables/movies?$top=5&$orderby=year desc,title desc",
			5,
			null,
			null,
			new[] { "id-033", "id-122", "id-188", "id-149", "id-064" }
		);
	}

	[Fact]
	public async Task Query_AuthTest_001()
	{
		await BaseQueryTest(
			"tables/movies_rated",
			95,
			null,
			null,
			new[] { "id-000", "id-001", "id-002", "id-003", "id-007" },
			"success"
		);
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
		var query = $"tables/movies?$top=5&$skip=5&$select={string.Join(',', selection)}";

		var response = await MovieServer.SendRequest(HttpMethod.Get, query);

		// Response has the right Status Code
		await AssertResponseWithLoggingAsync(HttpStatusCode.OK, response);

		// Response payload can be decoded
		var result = response.DeserializeContent<PageOfItems<ClientObject>>();
		Assert.NotNull(result);

		// There are items in the response payload
		Assert.NotNull(result?.Items);
		foreach (var item in result!.Items!)
		{
			// Each item in the payload has the requested properties.
			foreach (var property in selection)
			{
				Assert.True(item.Data!.ContainsKey(property));
			}
		}
	}

	[Theory]
	[InlineData("tables/movies", 248, 3)]
	[InlineData("tables/movies_pagesize?$top=100", 100, 4)]
	[InlineData("tables/movies?$count=true", 248, 3)]
	[InlineData("tables/movies?$top=50&$count=true", 50, 1)]
	[InlineData("tables/movies?$filter=releaseDate ge cast(1960-01-01T05:30:00Z,Edm.DateTimeOffset)&orderby=releaseDate asc", 186, 2)]
	[InlineData("tables/movies?$filter=releaseDate ge cast(1960-01-01T05:30:00Z,Edm.DateTimeOffset)&orderby=releaseDate asc&$top=20", 20, 1)]
	public async Task PagingTest(string startQuery, int expectedCount, int expectedLoops)
	{
		var query = startQuery;
		int loops = 0;
		var items = new Dictionary<string, ClientMovie>();

		//Act
		do
		{
			loops++;

			var response = await MovieServer.SendRequest(HttpMethod.Get, query);
			await AssertResponseWithLoggingAsync(HttpStatusCode.OK, response);
			var result = response.DeserializeContent<StringNextLinkPage<ClientMovie>>();
			Assert.NotNull(result?.Items);
			foreach (var item in result!.Items!)
			{
				items.Add(item.Id!, item);
			}

			if (result.NextLink != null)
			{
				Assert.StartsWith("https://localhost/", result.NextLink);
				query = new Uri(result.NextLink).PathAndQuery;
			}
			else
			{
				break;
			}
		} while (loops < expectedLoops + 2);

		Assert.Equal(expectedCount, items.Count);
		Assert.Equal(expectedLoops, loops);
	}

	[Theory]
	[InlineData("tables/notfound", HttpStatusCode.NotFound)]
	[InlineData("tables/movies?$filter=(year - 1900) ge 100", HttpStatusCode.BadRequest)]
	[InlineData("tables/movies?$filter=missing eq 20", HttpStatusCode.BadRequest)]
	[InlineData("tables/movies?$orderby=duration fizz", HttpStatusCode.BadRequest)]
	[InlineData("tables/movies?$orderby=missing asc", HttpStatusCode.BadRequest)]
	[InlineData("tables/movies?$select=foo", HttpStatusCode.BadRequest)]
	[InlineData("tables/movies?$select=year rating", HttpStatusCode.BadRequest)]
	[InlineData("tables/movies?$skip=-1", HttpStatusCode.BadRequest)]
	[InlineData("tables/movies?$skip=NaN", HttpStatusCode.BadRequest)]
	[InlineData("tables/movies?$top=-1", HttpStatusCode.BadRequest)]
	[InlineData("tables/movies?$top=1000000", HttpStatusCode.BadRequest)]
	[InlineData("tables/movies?$top=NaN", HttpStatusCode.BadRequest)]
	[InlineData("tables/movies_pagesize?$filter=(year - 1900) ge 100", HttpStatusCode.BadRequest)]
	[InlineData("tables/movies_pagesize?$filter=missing eq 20", HttpStatusCode.BadRequest)]
	[InlineData("tables/movies_pagesize?$orderby=duration fizz", HttpStatusCode.BadRequest)]
	[InlineData("tables/movies_pagesize?$orderby=missing asc", HttpStatusCode.BadRequest)]
	[InlineData("tables/movies_pagesize?$select=foo", HttpStatusCode.BadRequest)]
	[InlineData("tables/movies_pagesize?$select=year rating", HttpStatusCode.BadRequest)]
	[InlineData("tables/movies_pagesize?$skip=-1", HttpStatusCode.BadRequest)]
	[InlineData("tables/movies_pagesize?$skip=NaN", HttpStatusCode.BadRequest)]
	[InlineData("tables/movies_pagesize?$top=-1", HttpStatusCode.BadRequest)]
	[InlineData("tables/movies_pagesize?$top=1000000", HttpStatusCode.BadRequest)]
	[InlineData("tables/movies_pagesize?$top=NaN", HttpStatusCode.BadRequest)]
	[InlineData("tables/movies_rated", HttpStatusCode.Unauthorized)]
	[InlineData("tables/movies_rated", HttpStatusCode.Unauthorized, "X-Auth", "failed")]
	[InlineData("tables/movies_legal", HttpStatusCode.UnavailableForLegalReasons)]
	[InlineData("tables/movies_legal", HttpStatusCode.UnavailableForLegalReasons, "X-Auth", "failed")]
	public async Task FailedQueryTest(string relativeUri, HttpStatusCode expectedStatusCode, string? headerName = null, string? headerValue = null)
	{
		Dictionary<string, string> headers = new();
		if (headerName != null && headerValue != null) headers.Add(headerName, headerValue);

		// Act
		var response = await MovieServer.SendRequest(HttpMethod.Get, relativeUri, headers);

		// Assert
		await AssertResponseWithLoggingAsync(expectedStatusCode, response);
	}

	[Fact]
	public async Task SoftDeleteQueryTest_001()
	{
		await SoftDeleteQueryTest(
			"tables/soft", 
			100, 
			"tables/soft?$skip=100", 
			null, 
			new[] { "id-000", "id-001", "id-002", "id-003", "id-004" }, 
			"X-ZUMO-Options", "include:deleted"
		);
	}

	[Fact]
	public async Task SoftDeleteQueryTest_002()
	{
		await SoftDeleteQueryTest(
			"tables/soft?$count=true", 
			100, 
			"tables/soft?$count=true&$skip=100", 
			153, 
			new[] { "id-004", "id-005", "id-006", "id-008", "id-010" }
		);
	}

	[Fact]
	public async Task SoftDeleteQueryTest_003()
	{
		await SoftDeleteQueryTest(
			"tables/soft?$filter=deleted eq false&__includedeleted=true", 
			100, 
			"tables/soft?$filter=deleted eq false&__includedeleted=true&$skip=100", 
			null, 
			new[] { "id-004", "id-005", "id-006", "id-008", "id-010" }
		);
	}

	[Fact]
	public async Task SoftDeleteQueryTest_004()
	{
		await SoftDeleteQueryTest(
			"tables/soft?$filter=deleted eq true&__includedeleted=true", 
			95, 
			null, 
			null, 
			new[] { "id-000", "id-001", "id-002", "id-003", "id-007" }
		);
	}

	[Fact]
	public async Task SoftDeleteQueryTest_005()
	{
		await SoftDeleteQueryTest(
			"tables/soft?__includedeleted=true", 
			100, 
			"tables/soft?__includedeleted=true&$skip=100", 
			null, 
			new[] { "id-000", "id-001", "id-002", "id-003", "id-004", "id-005" }
		);
	}

	[Fact]
	public async Task DateTimeQueryTest_001()
	{
		await DateTimeQueryTest(
			"tables/datetime?$orderby=id",
			100,
			"tables/datetime?$orderby=id&$skip=100",
			null,
			new[] { "dtm-000", "dtm-001", "dtm-002", "dtm-003", "dtm-004", "dtm-005" }
		);
	}

	[Fact]
	public async Task DateTimeQueryTest_002()
	{
		await DateTimeQueryTest(
			"tables/datetime?$orderby=id&$filter=(month(dateOnly) eq 3)",
			31,
			null,
			null,
			new[] { "dtm-059", "dtm-060", "dtm-061", "dtm-062", "dtm-063", "dtm-064" }
		);
	}

	[Fact]
	public async Task DateTimeQueryTest_003()
	{
		await DateTimeQueryTest(
			"tables/datetime?$orderby=id&$filter=(day(dateOnly) eq 21)",
			12,
			null,
			null,
			new[] { "dtm-020", "dtm-051", "dtm-079", "dtm-110", "dtm-140", "dtm-171" }
		);
	}

	[Fact]
	public async Task DateTimeQueryTest_004()
	{
		await DateTimeQueryTest(
			"tables/datetime?$count=true&$orderby=id&$filter=(year(dateOnly) eq 2022)",
			100,
			"tables/datetime?$count=true&$orderby=id&$filter=(year(dateOnly) eq 2022)&$skip=100",
			365,
			new[] { "dtm-000", "dtm-001", "dtm-002", "dtm-003", "dtm-004", "dtm-005" }
		);
	}

	[Fact]
	public async Task DateTimeQueryTest_005()
	{
		await DateTimeQueryTest(
			"tables/datetime?$count=true&$orderby=id&$filter=dateOnly eq cast(2022-02-14,Edm.Date)",
			1,
			null,
			1,
			new[] { "dtm-044" }

		);
	}

	[Fact]
	public async Task DateTimeQueryTest_006()
	{
		await DateTimeQueryTest(
			"tables/datetime?$count=true&$orderby=id&$filter=dateOnly ge cast(2022-12-15,Edm.Date)",
			17,
			null,
			17,
			new[] { "dtm-348", "dtm-349", "dtm-350", "dtm-351" }

		);
	}

	[Fact]
	public async Task DateTimeQueryTest_007()
	{
		await DateTimeQueryTest(
			"tables/datetime?$count=true&$orderby=id&$filter=dateOnly gt cast(2022-12-15,Edm.Date)",
			16,
			null,
			16,
			new[] { "dtm-349", "dtm-350", "dtm-351", "dtm-352" }

		);
	}

	[Fact]
	public async Task DateTimeQueryTest_008()
	{
		await DateTimeQueryTest(
			"tables/datetime?$count=true&$orderby=id&$filter=dateOnly le cast(2022-07-14,Edm.Date)",
			100,
			"tables/datetime?$count=true&$orderby=id&$filter=dateOnly le cast(2022-07-14,Edm.Date)&$skip=100",
			195,
			new[] { "dtm-000", "dtm-001", "dtm-002", "dtm-003" }

		);
	}

	[Fact]
	public async Task DateTimeQueryTest_009()
	{
		await DateTimeQueryTest(
			"tables/datetime?$count=true&$orderby=id&$filter=dateOnly lt cast(2022-07-14,Edm.Date)",
			100,
			"tables/datetime?$count=true&$orderby=id&$filter=dateOnly lt cast(2022-07-14,Edm.Date)&$skip=100",
			194,
			new[] { "dtm-000", "dtm-001", "dtm-002", "dtm-003" }
		);
	}

	[Fact]
	public async Task DateTimeQueryTest_010()
	{
		await DateTimeQueryTest(
			"tables/datetime?$orderby=id&$filter=(second(timeOnly) eq 0)",
			100,
			"tables/datetime?$orderby=id&$filter=(second(timeOnly) eq 0)&$skip=100",
			null,
			new[] { "dtm-000", "dtm-001", "dtm-002", "dtm-003", "dtm-004", "dtm-005" }
		);
	}

	[Fact]
	public async Task DateTimeQueryTest_011()
	{
		await DateTimeQueryTest(
			"tables/datetime?$orderby=id&$filter=(hour(timeOnly) eq 3)",
			31,
			null,
			null,
			new[] { "dtm-059", "dtm-060", "dtm-061", "dtm-062", "dtm-063", "dtm-064" }
		);
	}

	[Fact]
	public async Task DateTimeQueryTest_012()
	{
		await DateTimeQueryTest(
			"tables/datetime?$orderby=id&$filter=(minute(timeOnly) eq 21)",
			12,
			null,
			null,
			new[] { "dtm-020", "dtm-051", "dtm-079", "dtm-110", "dtm-140", "dtm-171" }
		);
	}

	[Fact]
	public async Task DateTimeQueryTest_013()
	{
		await DateTimeQueryTest(
			"tables/datetime?$count=true&$orderby=id&$filter=(hour(timeOnly) le 12)",
			100,
			"tables/datetime?$count=true&$orderby=id&$filter=(hour(timeOnly) le 12)&$skip=100",
			365,
			new[] { "dtm-000", "dtm-001", "dtm-002", "dtm-003", "dtm-004", "dtm-005" }
		);
	}

	[Fact]
	public async Task DateTimeQueryTest_014()
	{
		await DateTimeQueryTest(
			"tables/datetime?$count=true&$orderby=id&$filter=timeOnly eq cast(02:14:00,Edm.TimeOfDay)",
			1,
			null,
			1,
			new[] { "dtm-044" }

		);
	}

	[Fact]
	public async Task DateTimeQueryTest_015()
	{
		await DateTimeQueryTest(
			"tables/datetime?$count=true&$orderby=id&$filter=timeOnly ge cast(12:15:00,Edm.TimeOfDay)",
			17,
			null,
			17,
			new[] { "dtm-348", "dtm-349", "dtm-350", "dtm-351" }

		);
	}

	[Fact]
	public async Task DateTimeQueryTest_016()
	{
		await DateTimeQueryTest(
			"tables/datetime?$count=true&$orderby=id&$filter=timeOnly gt cast(12:15:00,Edm.TimeOfDay)",
			16,
			null,
			16,
			new[] { "dtm-349", "dtm-350", "dtm-351", "dtm-352" }

		);
	}

	[Fact]
	public async Task DateTimeQueryTest_017()
	{
		await DateTimeQueryTest(
			"tables/datetime?$count=true&$orderby=id&$filter=timeOnly le cast(07:14:00,Edm.TimeOfDay)",
			100,
			"tables/datetime?$count=true&$orderby=id&$filter=timeOnly le cast(07:14:00,Edm.TimeOfDay)&$skip=100",
			195,
			new[] { "dtm-000", "dtm-001", "dtm-002", "dtm-003" }

		);
	}

	[Fact]
	public async Task DateTimeQueryTest_018()
	{
		await DateTimeQueryTest(
			"tables/datetime?$count=true&$orderby=id&$filter=timeOnly lt cast(07:14:00,Edm.TimeOfDay)",
			100,
			"tables/datetime?$count=true&$orderby=id&$filter=timeOnly lt cast(07:14:00,Edm.TimeOfDay)&$skip=100",
			194,
			new[] { "dtm-000", "dtm-001", "dtm-002", "dtm-003" }
		);
	}

	#region Base Tests
	/// <summary>
	/// This is the base test for the individual query tests.
	/// </summary>
	/// <param name="testcase">The test case being executed.</param>
	/// <returns>A task that completes when the test is complete.</returns>
	private async Task BaseQueryTest(string pathAndQuery, int itemCount, string? nextLinkQuery, long? totalCount, string[] firstItems, string? username = null)
	{
		Dictionary<string, string> headers = new();
		Utils.AddAuthHeaders(headers, username);

		var response = await MovieServer.SendRequest(HttpMethod.Get, pathAndQuery, headers);

		// Response has the right Status Code
		await AssertResponseWithLoggingAsync(HttpStatusCode.OK, response);

		// Response payload can be decoded
		var result = response.DeserializeContent<PageOfItems<ClientMovie>>();
		Assert.NotNull(result);

		// Payload has the right content
		Assert.Equal(itemCount, result!.Items!.Length);
		Assert.Equal(nextLinkQuery, result.NextLink == null ? null : Uri.UnescapeDataString(result.NextLink.PathAndQuery).TrimStart('/'));
		Assert.Equal(totalCount, result.Count);

		// The first n items must match what is expected
		Assert.True(result.Items.Length >= firstItems.Length);
		Assert.Equal(firstItems, result.Items.Take(firstItems.Length).Select(m => m.Id).ToArray());
		for (int idx = 0; idx < firstItems.Length; idx++)
		{
			var expected = MovieServer.GetMovieById(firstItems[idx])!;
			var actual = result.Items[idx];

			Assert.Equal<IMovie>(expected, actual);
			AssertEx.SystemPropertiesMatch(expected, actual);
		}
	}

	/// <summary>
	/// This is the base test for the individual query tests.
	/// </summary>
	/// <param name="testcase">The test case being executed.</param>
	/// <returns>A task that completes when the test is complete.</returns>
	private async Task DateTimeQueryTest(string pathAndQuery, int itemCount, string? nextLinkQuery, long? totalCount, string[] firstItems, string? username = null)
	{
		Dictionary<string, string> headers = new();
		Utils.AddAuthHeaders(headers, username);

		var response = await MovieServer.SendRequest(HttpMethod.Get, pathAndQuery, headers);

		// Response has the right Status Code
		await AssertResponseWithLoggingAsync(HttpStatusCode.OK, response);

		// Response payload can be decoded
		var result = response.DeserializeContent<PageOfItems<DateTimeDto>>();
		Assert.NotNull(result);

		// Payload has the right content
		Assert.Equal(itemCount, result!.Items!.Length);
		Assert.Equal(nextLinkQuery, result.NextLink == null ? null : Uri.UnescapeDataString(result.NextLink.PathAndQuery).TrimStart('/'));
		Assert.Equal(totalCount, result.Count);

		// The first n items must match what is expected
		Assert.True(result.Items.Length >= firstItems.Length);
		Assert.Equal(firstItems, result.Items.Take(firstItems.Length).Select(m => m.Id).ToArray());
	}

	private async Task SoftDeleteQueryTest(string query, int expectedItemCount, string? expectedNextLinkQuery, long? expectedTotalCount, string[] firstExpectedItems, string? headerName = null, string? headerValue = null)
	{
		await MovieServer.SoftDeleteMoviesAsync(m => m.Rating == "R");
		Dictionary<string, string> headers = new();
		if (headerName != null && headerValue != null) headers.Add(headerName, headerValue);

		var response = await MovieServer.SendRequest(HttpMethod.Get, query, headers);

		// Response has the right Status Code
		await AssertResponseWithLoggingAsync(HttpStatusCode.OK, response);

		// Response payload can be decoded
		var result = response.DeserializeContent<PageOfItems<ClientMovie>>();
		Assert.NotNull(result);

		// Payload has the right content
		Assert.Equal(expectedItemCount, result!.Items!.Length);
		Assert.Equal(expectedNextLinkQuery, result.NextLink == null ? null : Uri.UnescapeDataString(result.NextLink.PathAndQuery).TrimStart('/'));
		Assert.Equal(expectedTotalCount, result.Count);

		// The first n items must match what is expected
		Assert.True(result.Items.Length >= firstExpectedItems.Length);
		Assert.Equal(firstExpectedItems, result.Items.Take(firstExpectedItems.Length).Select(m => m.Id).ToArray());
		for (int idx = 0; idx < firstExpectedItems.Length; idx++)
		{
			var expected = MovieServer.GetMovieById(firstExpectedItems[idx])!;
			var actual = result.Items[idx];

			Assert.Equal<IMovie>(expected, actual);
			AssertEx.SystemPropertiesMatch(expected, actual);
		}
	}
	#endregion

	#region Models
	public class ClientObject
	{
		[JsonExtensionData]
		public Dictionary<string, object>? Data { get; set; }
	}
	#endregion
}
