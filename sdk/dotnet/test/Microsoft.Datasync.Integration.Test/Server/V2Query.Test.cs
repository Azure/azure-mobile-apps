// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

namespace Microsoft.Datasync.Integration.Test.Server;

/// <summary>
/// A set of integration tests that send requests with X-ZUMO-Version: 2.0
/// The only tests here concentrate on the differences between OData v3 and OData v4
/// </summary>
[ExcludeFromCodeCoverage]
[Collection("Integration")]
public class V2Query_Tests : BaseTest
{
    public V2Query_Tests(ITestOutputHelper logger) : base(logger) { }

    [Theory]
    [InlineData("tables/movies", 100, new[] { "id-000", "id-001", "id-002", "id-003", "id-004" })]
    [InlineData("tables/movies?$filter=releaseDate eq datetimeoffset'1994-10-14T00:00:00.000Z'", 2, new[] { "id-000", "id-003" })]
    [InlineData("tables/movies?$filter=releaseDate ge datetimeoffset'1999-12-31T00:00:00.000Z'", 69, new[] { "id-006", "id-008", "id-012", "id-013", "id-019" })]
    [InlineData("tables/movies?$filter=releaseDate gt datetimeoffset'1999-12-31T00:00:00.000Z'", 69, new[] { "id-006", "id-008", "id-012", "id-013", "id-019" })]
    [InlineData("tables/movies?$filter=releaseDate le datetimeoffset'2000-01-01T00:00:00.000Z'", 100, new[] { "id-000", "id-001", "id-002", "id-003", "id-004" })]
    [InlineData("tables/movies?$filter=releaseDate lt datetimeoffset'2000-01-01T00:00:00.000Z'", 100, new[] { "id-000", "id-001", "id-002", "id-003", "id-004" })]
    [InlineData("tables/movies?$filter=releaseDate eq datetimeoffset'1994-10-14T00:00:00.000Z'&$skip=5", 0, new string[] { })]
    [InlineData("tables/movies?$filter=releaseDate ge datetimeoffset'1999-12-31T00:00:00.000Z'&$skip=5", 64, new[] { "id-020", "id-032", "id-033", "id-042", "id-050" })]
    [InlineData("tables/movies?$filter=releaseDate gt datetimeoffset'1999-12-31T00:00:00.000Z'&$skip=5", 64, new[] { "id-020", "id-032", "id-033", "id-042", "id-050" })]
    [InlineData("tables/movies?$filter=releaseDate le datetimeoffset'2000-01-01T00:00:00.000Z'&$skip=5", 100, new[] { "id-005", "id-007", "id-009", "id-010", "id-011" })]
    [InlineData("tables/movies?$filter=releaseDate lt datetimeoffset'2000-01-01T00:00:00.000Z'&$skip=5", 100, new[] { "id-005", "id-007", "id-009", "id-010", "id-011" })]
    [InlineData("tables/movies?$filter=releaseDate eq datetime'1994-10-14T00:00:00.000Z'", 2, new[] { "id-000", "id-003" })]
    [InlineData("tables/movies?$filter=releaseDate ge datetime'1999-12-31T00:00:00.000Z'", 69, new[] { "id-006", "id-008", "id-012", "id-013", "id-019" })]
    [InlineData("tables/movies?$filter=releaseDate gt datetime'1999-12-31T00:00:00.000Z'", 69, new[] { "id-006", "id-008", "id-012", "id-013", "id-019" })]
    [InlineData("tables/movies?$filter=releaseDate le datetime'2000-01-01T00:00:00.000Z'", 100, new[] { "id-000", "id-001", "id-002", "id-003", "id-004" })]
    [InlineData("tables/movies?$filter=releaseDate lt datetime'2000-01-01T00:00:00.000Z'", 100, new[] { "id-000", "id-001", "id-002", "id-003", "id-004" })]
    [InlineData("tables/movies?$filter=releaseDate eq datetime'1994-10-14T00:00:00.000Z'&$skip=5", 0, new string[] { })]
    [InlineData("tables/movies?$filter=releaseDate ge datetime'1999-12-31T00:00:00.000Z'&$skip=5", 64, new[] { "id-020", "id-032", "id-033", "id-042", "id-050" })]
    [InlineData("tables/movies?$filter=releaseDate gt datetime'1999-12-31T00:00:00.000Z'&$skip=5", 64, new[] { "id-020", "id-032", "id-033", "id-042", "id-050" })]
    [InlineData("tables/movies?$filter=releaseDate le datetime'2000-01-01T00:00:00.000Z'&$skip=5", 100, new[] { "id-005", "id-007", "id-009", "id-010", "id-011" })]
    [InlineData("tables/movies?$filter=releaseDate lt datetime'2000-01-01T00:00:00.000Z'&$skip=5", 100, new[] { "id-005", "id-007", "id-009", "id-010", "id-011" })]
    [InlineData("tables/movies?$inlinecount=none&$filter=releaseDate eq datetimeoffset'1994-10-14T00:00:00.000Z'", 2, new[] { "id-000", "id-003" })]
    [InlineData("tables/movies?$inlinecount=none&$filter=releaseDate ge datetimeoffset'1999-12-31T00:00:00.000Z'", 69, new[] { "id-006", "id-008", "id-012", "id-013", "id-019" })]
    [InlineData("tables/movies?$inlinecount=none&$filter=releaseDate gt datetimeoffset'1999-12-31T00:00:00.000Z'", 69, new[] { "id-006", "id-008", "id-012", "id-013", "id-019" })]
    [InlineData("tables/movies?$inlinecount=none&$filter=releaseDate le datetimeoffset'2000-01-01T00:00:00.000Z'", 100, new[] { "id-000", "id-001", "id-002", "id-003", "id-004" })]
    [InlineData("tables/movies?$inlinecount=none&$filter=releaseDate lt datetimeoffset'2000-01-01T00:00:00.000Z'", 100, new[] { "id-000", "id-001", "id-002", "id-003", "id-004" })]
    [InlineData("tables/movies?$inlinecount=none&$filter=releaseDate eq datetimeoffset'1994-10-14T00:00:00.000Z'&$skip=5", 0, new string[] { })]
    [InlineData("tables/movies?$inlinecount=none&$filter=releaseDate ge datetimeoffset'1999-12-31T00:00:00.000Z'&$skip=5", 64, new[] { "id-020", "id-032", "id-033", "id-042", "id-050" })]
    [InlineData("tables/movies?$inlinecount=none&$filter=releaseDate gt datetimeoffset'1999-12-31T00:00:00.000Z'&$skip=5", 64, new[] { "id-020", "id-032", "id-033", "id-042", "id-050" })]
    [InlineData("tables/movies?$inlinecount=none&$filter=releaseDate le datetimeoffset'2000-01-01T00:00:00.000Z'&$skip=5", 100, new[] { "id-005", "id-007", "id-009", "id-010", "id-011" })]
    [InlineData("tables/movies?$inlinecount=none&$filter=releaseDate lt datetimeoffset'2000-01-01T00:00:00.000Z'&$skip=5", 100, new[] { "id-005", "id-007", "id-009", "id-010", "id-011" })]
    [InlineData("tables/movies?$inlinecount=none&$filter=releaseDate eq datetime'1994-10-14T00:00:00.000Z'", 2, new[] { "id-000", "id-003" })]
    [InlineData("tables/movies?$inlinecount=none&$filter=releaseDate ge datetime'1999-12-31T00:00:00.000Z'", 69, new[] { "id-006", "id-008", "id-012", "id-013", "id-019" })]
    [InlineData("tables/movies?$inlinecount=none&$filter=releaseDate gt datetime'1999-12-31T00:00:00.000Z'", 69, new[] { "id-006", "id-008", "id-012", "id-013", "id-019" })]
    [InlineData("tables/movies?$inlinecount=none&$filter=releaseDate le datetime'2000-01-01T00:00:00.000Z'", 100, new[] { "id-000", "id-001", "id-002", "id-003", "id-004" })]
    [InlineData("tables/movies?$inlinecount=none&$filter=releaseDate lt datetime'2000-01-01T00:00:00.000Z'", 100, new[] { "id-000", "id-001", "id-002", "id-003", "id-004" })]
    [InlineData("tables/movies?$inlinecount=none&$filter=releaseDate eq datetime'1994-10-14T00:00:00.000Z'&$skip=5", 0, new string[] { })]
    [InlineData("tables/movies?$inlinecount=none&$filter=releaseDate ge datetime'1999-12-31T00:00:00.000Z'&$skip=5", 64, new[] { "id-020", "id-032", "id-033", "id-042", "id-050" })]
    [InlineData("tables/movies?$inlinecount=none&$filter=releaseDate gt datetime'1999-12-31T00:00:00.000Z'&$skip=5", 64, new[] { "id-020", "id-032", "id-033", "id-042", "id-050" })]
    [InlineData("tables/movies?$inlinecount=none&$filter=releaseDate le datetime'2000-01-01T00:00:00.000Z'&$skip=5", 100, new[] { "id-005", "id-007", "id-009", "id-010", "id-011" })]
    [InlineData("tables/movies?$inlinecount=none&$filter=releaseDate lt datetime'2000-01-01T00:00:00.000Z'&$skip=5", 100, new[] { "id-005", "id-007", "id-009", "id-010", "id-011" })]
    [InlineData("tables/movies?$top=5&$filter=releaseDate eq datetimeoffset'1994-10-14T00:00:00.000Z'", 2, new[] { "id-000", "id-003" })]
    [InlineData("tables/movies?$top=5&$filter=releaseDate ge datetimeoffset'1999-12-31T00:00:00.000Z'", 5, new[] { "id-006", "id-008", "id-012", "id-013", "id-019" })]
    [InlineData("tables/movies?$top=5&$filter=releaseDate gt datetimeoffset'1999-12-31T00:00:00.000Z'", 5, new[] { "id-006", "id-008", "id-012", "id-013", "id-019" })]
    [InlineData("tables/movies?$top=5&$filter=releaseDate le datetimeoffset'2000-01-01T00:00:00.000Z'", 5, new[] { "id-000", "id-001", "id-002", "id-003", "id-004" })]
    [InlineData("tables/movies?$top=5&$filter=releaseDate lt datetimeoffset'2000-01-01T00:00:00.000Z'", 5, new[] { "id-000", "id-001", "id-002", "id-003", "id-004" })]
    [InlineData("tables/movies?$top=5&$filter=releaseDate eq datetime'1994-10-14T00:00:00.000Z'", 2, new[] { "id-000", "id-003" })]
    [InlineData("tables/movies?$top=5&$filter=releaseDate ge datetime'1999-12-31T00:00:00.000Z'", 5, new[] { "id-006", "id-008", "id-012", "id-013", "id-019" })]
    [InlineData("tables/movies?$top=5&$filter=releaseDate gt datetime'1999-12-31T00:00:00.000Z'", 5, new[] { "id-006", "id-008", "id-012", "id-013", "id-019" })]
    [InlineData("tables/movies?$top=5&$filter=releaseDate le datetime'2000-01-01T00:00:00.000Z'", 5, new[] { "id-000", "id-001", "id-002", "id-003", "id-004" })]
    [InlineData("tables/movies?$top=5&$filter=releaseDate lt datetime'2000-01-01T00:00:00.000Z'", 5, new[] { "id-000", "id-001", "id-002", "id-003", "id-004" })]
    public async Task BasicV2QueryTest(string query, int expectedItemCount, string[] firstExpectedItems)
    {
        Dictionary<string, string> headers = new() { { "ZUMO-API-VERSION", "2.0.0" } };

        var response = await MovieServer.SendRequest(HttpMethod.Get, query, headers).ConfigureAwait(false);
        await AssertResponseWithLoggingAsync(HttpStatusCode.OK, response);

        // Response payload can be decoded
        var result = response.DeserializeContent<List<ClientMovie>>();
        Assert.NotNull(result);

        // Payload has the right content
        Assert.Equal(expectedItemCount, result!.Count);

        // The first n items must match what is expected
        Assert.True(result.Count >= firstExpectedItems.Length);
        Assert.Equal(firstExpectedItems, result.Take(firstExpectedItems.Length).Select(m => m.Id).ToArray());
        for (int idx = 0; idx < firstExpectedItems.Length; idx++)
        {
            var expected = MovieServer.GetMovieById(firstExpectedItems[idx]);
            var actual = result[idx];

            Assert.Equal<IMovie>(expected!, actual);
            AssertEx.SystemPropertiesMatch(expected, actual);
        }
    }

    [Theory]
    [InlineData("tables/movies?$inlinecount=allpages&$filter=releaseDate eq datetimeoffset'1994-10-14T00:00:00.000Z'", 2, 2, new[] { "id-000", "id-003" })]
    [InlineData("tables/movies?$inlinecount=allpages&$filter=releaseDate ge datetimeoffset'1999-12-31T00:00:00.000Z'", 69, 69, new[] { "id-006", "id-008", "id-012", "id-013", "id-019" })]
    [InlineData("tables/movies?$inlinecount=allpages&$filter=releaseDate gt datetimeoffset'1999-12-31T00:00:00.000Z'", 69, 69, new[] { "id-006", "id-008", "id-012", "id-013", "id-019" })]
    [InlineData("tables/movies?$inlinecount=allpages&$filter=releaseDate le datetimeoffset'2000-01-01T00:00:00.000Z'", 100, 179, new[] { "id-000", "id-001", "id-002", "id-003", "id-004" })]
    [InlineData("tables/movies?$inlinecount=allpages&$filter=releaseDate lt datetimeoffset'2000-01-01T00:00:00.000Z'", 100, 179, new[] { "id-000", "id-001", "id-002", "id-003", "id-004" })]
    [InlineData("tables/movies?$inlinecount=allpages&$top=5&$filter=releaseDate eq datetimeoffset'1994-10-14T00:00:00.000Z'", 2, 2, new[] { "id-000", "id-003" })]
    [InlineData("tables/movies?$inlinecount=allpages&$top=5&$filter=releaseDate ge datetimeoffset'1999-12-31T00:00:00.000Z'", 5, 69, new[] { "id-006", "id-008", "id-012", "id-013", "id-019" })]
    [InlineData("tables/movies?$inlinecount=allpages&$top=5&$filter=releaseDate gt datetimeoffset'1999-12-31T00:00:00.000Z'", 5, 69, new[] { "id-006", "id-008", "id-012", "id-013", "id-019" })]
    [InlineData("tables/movies?$inlinecount=allpages&$top=5&$filter=releaseDate le datetimeoffset'2000-01-01T00:00:00.000Z'", 5, 179, new[] { "id-000", "id-001", "id-002", "id-003", "id-004" })]
    [InlineData("tables/movies?$inlinecount=allpages&$top=5&$filter=releaseDate lt datetimeoffset'2000-01-01T00:00:00.000Z'", 5, 179, new[] { "id-000", "id-001", "id-002", "id-003", "id-004" })]
    [InlineData("tables/movies?$inlinecount=allpages&$filter=releaseDate eq datetime'1994-10-14T00:00:00.000Z'", 2, 2, new[] { "id-000", "id-003" })]
    [InlineData("tables/movies?$inlinecount=allpages&$filter=releaseDate ge datetime'1999-12-31T00:00:00.000Z'", 69, 69, new[] { "id-006", "id-008", "id-012", "id-013", "id-019" })]
    [InlineData("tables/movies?$inlinecount=allpages&$filter=releaseDate gt datetime'1999-12-31T00:00:00.000Z'", 69, 69, new[] { "id-006", "id-008", "id-012", "id-013", "id-019" })]
    [InlineData("tables/movies?$inlinecount=allpages&$filter=releaseDate le datetime'2000-01-01T00:00:00.000Z'", 100, 179, new[] { "id-000", "id-001", "id-002", "id-003", "id-004" })]
    [InlineData("tables/movies?$inlinecount=allpages&$filter=releaseDate lt datetime'2000-01-01T00:00:00.000Z'", 100, 179, new[] { "id-000", "id-001", "id-002", "id-003", "id-004" })]
    [InlineData("tables/movies?$inlinecount=allpages&$top=5&$filter=releaseDate eq datetime'1994-10-14T00:00:00.000Z'", 2, 2, new[] { "id-000", "id-003" })]
    [InlineData("tables/movies?$inlinecount=allpages&$top=5&$filter=releaseDate ge datetime'1999-12-31T00:00:00.000Z'", 5, 69, new[] { "id-006", "id-008", "id-012", "id-013", "id-019" })]
    [InlineData("tables/movies?$inlinecount=allpages&$top=5&$filter=releaseDate gt datetime'1999-12-31T00:00:00.000Z'", 5, 69, new[] { "id-006", "id-008", "id-012", "id-013", "id-019" })]
    [InlineData("tables/movies?$inlinecount=allpages&$top=5&$filter=releaseDate le datetime'2000-01-01T00:00:00.000Z'", 5, 179, new[] { "id-000", "id-001", "id-002", "id-003", "id-004" })]
    [InlineData("tables/movies?$inlinecount=allpages&$top=5&$filter=releaseDate lt datetime'2000-01-01T00:00:00.000Z'", 5, 179, new[] { "id-000", "id-001", "id-002", "id-003", "id-004" })]
    public async Task CountedV2QueryTest(string query, int expectedItemCount, long expectedTotalCount, string[] firstExpectedItems)
    {
        Dictionary<string, string> headers = new() { { "ZUMO-API-VERSION", "2.0.0" } };

        var response = await MovieServer.SendRequest(HttpMethod.Get, query, headers).ConfigureAwait(false);
        await AssertResponseWithLoggingAsync(HttpStatusCode.OK, response);

        // Response payload can be decoded
        var result = response.DeserializeContent<V2PageOfItems<ClientMovie>>();
        Assert.NotNull(result);

        // Payload has the right content
        Assert.Equal(expectedItemCount, result!.Results!.Length);
        Assert.Equal(expectedTotalCount, result.Count);

        // The first n items must match what is expected
        Assert.True(result.Results.Length >= firstExpectedItems.Length);
        Assert.Equal(firstExpectedItems, result.Results.Take(firstExpectedItems.Length).Select(m => m.Id).ToArray());
        for (int idx = 0; idx < firstExpectedItems.Length; idx++)
        {
            var expected = MovieServer.GetMovieById(firstExpectedItems[idx])!;
            var actual = result.Results[idx];

            Assert.Equal<IMovie>(expected, actual);
            AssertEx.SystemPropertiesMatch(expected, actual);
        }
    }
}
