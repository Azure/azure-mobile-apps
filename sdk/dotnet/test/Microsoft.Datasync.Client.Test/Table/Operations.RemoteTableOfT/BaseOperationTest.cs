// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Test;
using Datasync.Common.Test.Mocks;
using Microsoft.Datasync.Client.Test.Helpers;
using System.Text;
using System.Text.Json;

namespace Microsoft.Datasync.Client.Test.Table.Operations.RemoteTableOfT;

[ExcludeFromCodeCoverage]
public class BaseOperationTest : ClientBaseTest
{
    protected readonly IdEntity payload = new() { Id = "db0ec08d-46a9-465d-9f5e-0066a3ee5b5f", StringValue = "test" };
    protected const string sJsonPayload = "{\"id\":\"db0ec08d-46a9-465d-9f5e-0066a3ee5b5f\",\"stringValue\":\"test\"}";
    protected const string sBadJson = "{this-is-bad-json";

    protected IRemoteTable<IdEntity> table, authTable;
    protected readonly string sId, expectedEndpoint, tableEndpoint;

    public BaseOperationTest() : base()
    {
        sId = Guid.NewGuid().ToString("N");
        expectedEndpoint = new Uri(Endpoint, $"/tables/movies/{sId}").ToString();
        tableEndpoint = new Uri(Endpoint, "/tables/movies").ToString();

        table = GetMockClient().GetRemoteTable<IdEntity>("movies");
        authTable = GetMockClient(new MockAuthenticationProvider(ValidAuthenticationToken)).GetRemoteTable<IdEntity>("movies");
    }

    /// <summary>
    /// Check that the request is the correct method and path.
    /// </summary>
    /// <param name="method"></param>
    /// <param name="endpoint"></param>
    protected HttpRequestMessage AssertSingleRequest(HttpMethod method, string endpoint)
    {
        Assert.Single(MockHandler.Requests);
        return AssertRequest(MockHandler.Requests[0], method, endpoint);
    }

    /// <summary>
    /// Checks that the specified request is the right method and path
    /// </summary>
    /// <param name="request"></param>
    /// <param name="method"></param>
    /// <param name="endpoint"></param>
    /// <returns></returns>
    protected static HttpRequestMessage AssertRequest(HttpRequestMessage request, HttpMethod method, string endpoint)
    {
        Assert.Equal(method, request.Method);
        Assert.Equal(endpoint, request.RequestUri.ToString());
        AssertEx.HasHeader(request.Headers, "ZUMO-API-VERSION", "3.0.0");
        return request;
    }

    /// <summary>
    /// Compares the request content to the required entity.
    /// </summary>
    /// <param name="request">The request holding the content</param>
    /// <param name="expected">The expected entity.</param>
    /// <returns></returns>
    protected static async Task AssertRequestContentMatchesAsync(HttpRequestMessage request, IdEntity expected)
    {
        Assert.Equal("application/json", request.Content.Headers.ContentType.MediaType);
        var content = await request.Content.ReadAsStringAsync();
        var idEntity = JsonSerializer.Deserialize<IdEntity>(content, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        Assert.Equal(expected, idEntity);
    }


    /// <summary>
    /// Returns bad JSON response.
    /// </summary>
    /// <param name="statusCode"></param>
    protected void ReturnBadJson(HttpStatusCode statusCode)
    {
        var response = new HttpResponseMessage(statusCode)
        {
            Content = new StringContent(sBadJson, Encoding.UTF8, "application/json")
        };
        MockHandler.Responses.Add(response);
    }

    /// <summary>
    /// A set of invalid IDs for testing
    /// </summary>
    public static IEnumerable<object[]> GetInvalidIds() => new List<object[]>
    {
        new object[] { "" },
        new object[] { " " },
        new object[] { "\t" },
        new object[] { "abcdef gh" },
        new object[] { "!!!" },
        new object[] { "?" },
        new object[] { ";" },
        new object[] { "{EA235ADF-9F38-44EA-8DA4-EF3D24755767}" },
        new object[] { "###" }
    };
}
