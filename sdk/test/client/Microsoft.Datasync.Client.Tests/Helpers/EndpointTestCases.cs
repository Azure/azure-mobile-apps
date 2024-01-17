// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

namespace Microsoft.Datasync.Client.Tests.Helpers;

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

public class EndpointTestCases : TheoryData<EndpointTestCase>
{
    public EndpointTestCases()
    {
        string[] protocols = { "http", "https" };
        string[] hostnames = { "localhost", "127.0.0.1", "[::1]" };
        string[] securehosts = { "myapi.azurewebsites.net", "myapi.azure-api.net" };
        string[] ports = { "", ":3000" };
        string[] paths = { "", "/myapi" };

        foreach (string protocol in protocols)
        {
            foreach (string hostname in hostnames)
            {
                foreach (string port in ports)
                {
                    foreach (string path in paths)
                    {
                        string uri = $"{protocol}://{hostname}{port}{path}";
                        Add(new EndpointTestCase(uri, uri + "/"));
                        Add(new EndpointTestCase(uri + "?queryparam", uri + "/"));
                        Add(new EndpointTestCase(uri + "#fragment", uri + "/"));
                        Add(new EndpointTestCase(uri + "/?queryparam#fragment", uri + "/"));
                    }
                }
            }
        }

        foreach (string hostname in securehosts)
        {
            foreach (string port in ports)
            {
                foreach (string path in paths)
                {
                    string uri = $"https://{hostname}{port}{path}";
                    Add(new EndpointTestCase(uri, uri + "/"));
                    Add(new EndpointTestCase(uri + "?queryparam", uri + "/"));
                    Add(new EndpointTestCase(uri + "#fragment", uri + "/"));
                    Add(new EndpointTestCase(uri + "/?queryparam#fragment", uri + "/"));
                }
            }
        }
    }
}
