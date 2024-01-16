namespace Microsoft.Datasync.Client.Http;

/// <summary>
/// An implementation of the <see cref="IHttpClientFactory"/> interface that is used
/// to provide Datasync service compatible <see cref="HttpClient"/> objects with a
/// similar interface to the v6 Datasync client.
/// </summary>
public class DefaultHttpClientFactory : IHttpClientFactory, IDisposable
{
    /// <summary>
    /// The cache for storing HttpClient instances.
    /// </summary>
    internal readonly ClientCache<string, HttpClient> _cache = new();

    /// <summary>
    /// A factory method that returns the default <see cref="HttpClientHandler"/> to use.
    /// </summary>
    protected Func<HttpMessageHandler> DefaultHandlerFactory = GetDefaultHttpClientHandler;

    /// <summary>
    /// Creates a new instance of the <see cref="DefaultHttpClientFactory"/> class using
    /// the default options.
    /// </summary>
    public DefaultHttpClientFactory()
    {
        Options = new HttpClientOptions();
    }

    /// <summary>
    /// Creates a new instance of the <see cref="DefaultHttpClientFactory"/> class using
    /// a specific set of options.
    /// </summary>
    /// <param name="options">The options to use in creating new <see cref="HttpClient"/> instances.</param>
    public DefaultHttpClientFactory(IHttpClientOptions options)
    {
        Options = options;
    }

    /// <summary>
    /// The current options in use by the factory.
    /// </summary>
    public IHttpClientOptions Options { get; }

    /// <summary>
    /// Creates a named <see cref="HttpClient"/>.  Providing the same name also returns the same client
    /// if it has previously been created.
    /// </summary>
    /// <param name="name">The name of the client.</param>
    /// <returns>A configured <see cref="HttpClient"/> instance.</returns>
    public HttpClient CreateClient(string name)
        => _cache.GetOrAdd(name, () => CreateNewClient());

    /// <summary>
    /// Creates a new <see cref="HttpClient"/> instance using the current options.
    /// </summary>
    /// <returns>A configured <see cref="HttpClient"/> instance.</returns>
    protected HttpClient CreateNewClient()
    {
        Ensure.That(Options.Endpoint).IsValidEndpoint();

        HttpMessageHandler rootHandler = CreatePipeline(Options.HttpPipeline);
        HttpClient client = new(rootHandler) { BaseAddress = Options.Endpoint, Timeout = Options.HttpTimeout };
        if (!string.IsNullOrWhiteSpace(Options.ProtocolVersion))
        {
            client.DefaultRequestHeaders.Add(DatasyncHttpHeaders.ProtocolVersion, Options.ProtocolVersion);
        }
        if (!string.IsNullOrWhiteSpace(Options.UserAgent))
        {
            client.DefaultRequestHeaders.TryAddWithoutValidation(DatasyncHttpHeaders.UserAgent, Options.UserAgent);
            client.DefaultRequestHeaders.Add(DatasyncHttpHeaders.InternalUserAgent, Options.UserAgent);
        }
        if (!string.IsNullOrWhiteSpace(Options.InstallationId))
        {
            client.DefaultRequestHeaders.Add(DatasyncHttpHeaders.InstallationId, Options.InstallationId);
        }
        return client;
    }

    /// <summary>
    /// Transform a list of <see cref="HttpMessageHandler"/> objects into a chain suitable for using
    /// as the pipeline of a <see cref="HttpClient"/>.
    /// </summary>
    /// <param name="handlers">The list of <see cref="HttpMessageHandler"/> objects to transform</param>
    /// <returns>The chained <see cref="HttpMessageHandler"/></returns>
    internal HttpMessageHandler CreatePipeline(IEnumerable<HttpMessageHandler> handlers)
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
    internal static HttpMessageHandler GetDefaultHttpClientHandler()
    {
        HttpClientHandler handler = new();
        if (handler.SupportsAutomaticDecompression)
        {
            handler.AutomaticDecompression = System.Net.DecompressionMethods.GZip;
        }
        return handler;
    }

    /// <summary>
    /// Implementation of the <see cref="IDisposable"/> pattern for derived classes to use.
    /// </summary>
    /// <param name="disposing"><c>true</c> if calling from <see cref="Dispose()"/> or the finalizer.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _cache.Dispose();
        }
    }

    /// <summary>
    /// Implementation of the <see cref="IDisposable"/> pattern.
    /// </summary>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
