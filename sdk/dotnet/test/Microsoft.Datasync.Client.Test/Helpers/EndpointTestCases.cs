// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

namespace Microsoft.Datasync.Client.Test.Helpers;

[ExcludeFromCodeCoverage]
public class EndpointTestCase
{
    internal EndpointTestCase(string baseEndpoint, string normalizedEndpoint)
    {
        BaseEndpoint = baseEndpoint;
        NormalizedEndpoint = normalizedEndpoint;
    }

    public string BaseEndpoint { get; }
    public string NormalizedEndpoint { get; }
}

/// <summary>
/// A set of test cases for the valid endpoints that the client supports
/// </summary>
[ExcludeFromCodeCoverage]
public class EndpointTestCases : TheoryData<EndpointTestCase>
{
    public EndpointTestCases()
    {
        string[] protocols = { "http", "https" };
        string[] hostnames = { "localhost", "127.0.0.1", "[::1]" };
        string[] ports = { "", ":3000" };
        string[] paths = { "", "/myapi" };

        foreach (var protocol in protocols)
        {
            foreach (var hostname in hostnames)
            {
                foreach (var port in ports)
                {
                    foreach (var path in paths)
                    {
                        var uri = $"{protocol}://{hostname}{port}{path}";
                        Add(new EndpointTestCase(uri, uri + "/"));
                        Add(new EndpointTestCase(uri + "?queryparam", uri + "/"));
                        Add(new EndpointTestCase(uri + "#fragment", uri + "/"));
                        Add(new EndpointTestCase(uri + "/?queryparam#fragment", uri + "/"));
                    }
                }
            }
        }

        string[] securehosts = { "myapi.azurewebsites.net", "myapi.azure-api.net" };
        foreach (var hostname in securehosts)
        {
            foreach (var port in ports)
            {
                foreach (var path in paths)
                {
                    var uri = $"https://{hostname}{port}{path}";
                    Add(new EndpointTestCase(uri, uri + "/"));
                    Add(new EndpointTestCase(uri + "?queryparam", uri + "/"));
                    Add(new EndpointTestCase(uri + "#fragment", uri + "/"));
                    Add(new EndpointTestCase(uri + "/?queryparam#fragment", uri + "/"));
                }
            }
        }
    }
}
