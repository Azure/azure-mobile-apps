// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System.Collections.Concurrent;
using System.Net;

namespace Microsoft.Datasync.Client.Http;

internal class DefaultHttpClientProvider : IHttpClientProvider
{
    /// <summary>
    /// A cache of the currently active HTTP clients.
    /// </summary>
    private readonly ConcurrentDictionary<string, HttpClient> cache = new();

    /// <summary>
    /// The header name for transmitting the installation ID.
    /// </summary>
    private const string InstallationIdHeaderName = "X-ZUMO-Installation-Id";

    /// <summary>
    /// The header name for transmitting the user agent; used for telemetry when the User-Agent cannot be set.
    /// </summary>
    private const string InternalUserAgentHeaderName = "X-ZUMO-Version";

    /// <summary>
    /// The header name for transmitting the user agent.
    /// </summary>
    private const string UserAgentHeaderName = "User-Agent";

    /// <summary>
    /// Creates a new instance of the <see cref="DefaultHttpClientProvider"/> class
    /// using the provided options.
    /// </summary>
    /// <param name="options">The client options to use for communicating with the service.</param>
    public DefaultHttpClientProvider(ServiceClientOptions options)
    {
        ClientOptions = options;
    }

    /// <summary>
    /// The client options to use for communicating with the service.
    /// </summary>
    public ServiceClientOptions ClientOptions { get; }

    /// <summary>
    /// A factory method for creating the default <see cref="HttpClientHandler"/>.
    /// </summary>
    protected Func<HttpMessageHandler> DefaultHandlerFactory = GetDefaultHttpClientHandler;

    /// <inheritdoc />
    public HttpClient GetHttpClient(string providerName = "")
    {
        if (!cache.TryGetValue(providerName, out HttpClient? client))
        {
            client = BuildHttpClient();
            cache.TryAdd(providerName, client);
        }
        return client;
    }

    internal HttpClient BuildHttpClient()
    {
        HttpMessageHandler rootHandler = CreatePipeline(ClientOptions.HttpPipeline);
        HttpClient client = new(rootHandler) { Timeout = ClientOptions.HttpTimeout };
        if (!string.IsNullOrWhiteSpace(ClientOptions.UserAgent))
        {
            client.DefaultRequestHeaders.TryAddWithoutValidation(UserAgentHeaderName, ClientOptions.UserAgent);
            client.DefaultRequestHeaders.Add(InternalUserAgentHeaderName, ClientOptions.UserAgent);
        }
        if (!string.IsNullOrWhiteSpace(ClientOptions.InstallationId))
        {
            client.DefaultRequestHeaders.Add(InstallationIdHeaderName, ClientOptions.InstallationId);
        }
        return client;
    }

    /// <summary>
    /// Transform a list of <see cref="HttpMessageHandler"/> objects into a chain suitable for using
    /// as the pipeline of a <see cref="HttpClient"/>.
    /// </summary>
    /// <param name="handlers">The list of <see cref="HttpMessageHandler"/> objects to transform</param>
    /// <returns>The chained <see cref="HttpMessageHandler"/></returns>
    protected HttpMessageHandler CreatePipeline(IEnumerable<HttpMessageHandler> handlers)
    {
        HttpMessageHandler pipeline = handlers.LastOrDefault() ?? DefaultHandlerFactory();
        if (pipeline is DelegatingHandler lastPolicy && lastPolicy.InnerHandler == null)
        {
            lastPolicy.InnerHandler = DefaultHandlerFactory();
            pipeline = lastPolicy;
        }

        // Wire handlers up in reverse order
        foreach (HttpMessageHandler handler in handlers.Reverse().Skip(1))
        {
            if (handler is DelegatingHandler policy)
            {
                policy.InnerHandler = pipeline;
                pipeline = policy;
            }
            else
            {
                throw new ArgumentException("All message handlers except the last one must be 'DelegatingHandler'", nameof(handlers));
            }
        }
        return pipeline;
    }

    /// <summary>
    /// Returns a <see cref="HttpClientHandler"/> that supports automatic decompression.
    /// </summary>
    protected static HttpMessageHandler GetDefaultHttpClientHandler()
        => new HttpClientHandler() { AutomaticDecompression = DecompressionMethods.All };
}
