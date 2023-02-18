using System.Diagnostics;
using System.Net.Http.Headers;

namespace TodoMauiApp.Services;

public class LoggingHandler : DelegatingHandler
{
    public LoggingHandler() : base()
    {
    }

    public LoggingHandler(HttpMessageHandler innerHandler) : base(innerHandler)
    {
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken = default)
    {
        Debug.WriteLine($"[HTTP] >>> {request.Method} {request.RequestUri}");
        PrintHeaders(">>>", request.Headers);
        await PrintContentAsync(">>>", request.Content);

        HttpResponseMessage response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);

        Debug.WriteLine($"[HTTP] <<< {response.StatusCode} {response.ReasonPhrase}");
        PrintHeaders("<<<", response.Headers);
        await PrintContentAsync("<<<", response.Content);

        return response;
    }

    private static void PrintHeaders(string prefix, HttpHeaders headers)
    {
        foreach (var header in headers)
        {
            foreach (var hdrVal in header.Value)
            {
                Debug.WriteLine($"[HTTP] {prefix} {header.Key}: {hdrVal}");
            }
        }
    }

    private static async Task PrintContentAsync(string prefix, HttpContent content)
    {
        if (content != null)
        {
            PrintHeaders(prefix, content.Headers);
            Debug.WriteLine($"[HTTP] {prefix} {await content.ReadAsStringAsync().ConfigureAwait(false)}");
        }
    }
}
