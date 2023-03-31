// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Test.Mocks;
using Datasync.Common.Test.Models;
using Datasync.Common.Test.Service;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Datasync.Client;
using Microsoft.Datasync.Client.Authentication;
using Microsoft.Datasync.Client.Offline;
using Microsoft.Datasync.Client.Serialization;
using Microsoft.Datasync.Client.Table;
using Microsoft.Datasync.Client.Utils;
using Newtonsoft.Json.Linq;
using System.Net;
using Xunit.Abstractions;

namespace Datasync.Common.Test;

/// <summary>
/// A basic template for a test suite
/// </summary>
[ExcludeFromCodeCoverage]
public class BaseTest
{
    private readonly Lazy<TestServer> _server = new(() => MovieApiServer.CreateTestServer());
    private readonly ITestOutputHelper logger;

    protected BaseTest(ITestOutputHelper helper)
    {
        logger = helper;
    }

    protected BaseTest()
    {
    }

    /// <summary>
    /// An authentication token that is expired.
    /// </summary>
    protected static readonly AuthenticationToken ExpiredAuthenticationToken = new()
    {
        DisplayName = "John Smith",
        ExpiresOn = DateTimeOffset.Now.AddMinutes(-5),
        Token = "YmFzaWMgdG9rZW4gZm9yIHRlc3Rpbmc=",
        UserId = "the_doctor"
    };

    /// <summary>
    /// A completely valid authentication token.
    /// </summary>
    protected readonly AuthenticationToken ValidAuthenticationToken = new()
    {
        DisplayName = "John Smith",
        ExpiresOn = DateTimeOffset.Now.AddMinutes(5),
        Token = "YmFzaWMgdG9rZW4gZm9yIHRlc3Rpbmc=",
        UserId = "the_doctor"
    };

    /// <summary>
    /// Default endpoint.
    /// </summary>
    protected Uri Endpoint { get; } = new Uri("https://localhost");

    /// <summary>
    /// The mock handler that allows us to set responses and see requests.
    /// </summary>
    protected MockDelegatingHandler MockHandler { get; } = new();

    /// <summary>
    /// The count of the movies in the data set.
    /// </summary>
    protected static int MovieCount { get; } = TestData.Movies.Count;

    /// <summary>
    /// Creates a <see cref="HttpClient"/> to access the movie server
    /// </summary>
    protected HttpClient MovieHttpClient { get => MovieServer.CreateClient(); }

    /// <summary>
    /// Creates a reference to the movie server, when needed.
    /// </summary>
    protected TestServer MovieServer { get => _server.Value; }

    /// <summary>
    /// The start time for the test.
    /// </summary>
    protected DateTimeOffset StartTime { get; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Creates a paging response for a list of movies, with an updatedAt set.
    /// </summary>
    /// <param name="count">Count of items</param>
    /// <param name="lastUpdatedAt">The last updated at to use.</param>
    /// <param name="nDeleted">Number of deleted items</param>
    /// <param name="noUpdatedAt">Don't include the updatedAt field.</param>
    /// <returns></returns>
    protected List<JObject> CreatePageOfMovies(int count, DateTimeOffset lastUpdatedAt, int nDeleted = 0)
    {
        List<EFMovie> movies = TestData.Movies.OfType<EFMovie>().Take(count).ToList();
        for (int i = 0; i < count; i++)
        {
            int offset = count - i - 1; // Number of days offset.
            movies[i].UpdatedAt = lastUpdatedAt.AddDays(-offset);
        }
        while (nDeleted > 0)
        {
            movies[nDeleted--].Deleted = true;
        }

        var page = new Page<EFMovie> { Items = movies };
        MockHandler.AddResponse(HttpStatusCode.OK, page);

        return movies.ConvertAll(m => (JObject)GetMockClient().Serializer.Serialize(m));
    }

    /// <summary>
    /// Builds the handler set for the clients.
    /// </summary>
    /// <param name="authProvider"></param>
    /// <param name="clientHandler"></param>
    /// <returns></returns>
    private static List<HttpMessageHandler> BuildHandlers(AuthenticationProvider authProvider, HttpMessageHandler clientHandler)
    {
        List<HttpMessageHandler> handlers = new(new[] { clientHandler });
        if (authProvider != null)
        {
            handlers.Insert(0, authProvider);
        }
        return handlers;
    }

    /// <summary>
    /// Get a datasync client that is completely mocked and doesn't require a service
    /// </summary>
    /// <param name="authProvider">... with the provided authentication provider</param>
    /// <returns>A datasync client</returns>
    protected DatasyncClient GetMockClient(AuthenticationProvider authProvider = null, IOfflineStore store = null)
    {
        var options = new DatasyncClientOptions
        {
            HttpPipeline = BuildHandlers(authProvider, MockHandler),
            OfflineStore = store
        };
        return authProvider == null ? new DatasyncClient(Endpoint, options) : new DatasyncClient(Endpoint, options);
    }

    /// <summary>
    /// Get a datasync client that is connected to the integration movie service.
    /// </summary>
    /// <param name="authProvider">... with the provided authentication provider</param>
    /// <returns>A datasync client</returns>
    protected DatasyncClient GetMovieClient(AuthenticationProvider authProvider = null, IOfflineStore store = null)
    {
        var options = new DatasyncClientOptions
        {
            HttpPipeline = BuildHandlers(authProvider, MovieServer.CreateHandler()),
            OfflineStore = store
        };
        return authProvider == null ? new DatasyncClient(Endpoint, options) : new DatasyncClient(Endpoint, options);
    }

    /// <summary>
    /// Get a datasync client that is connected to the integration movie service.
    /// </summary>
    /// <param name="authProvider">... with the provided authentication provider</param>
    /// <returns>A datasync client</returns>
    protected DatasyncClient GetMovieClientWithIdGenerator(AuthenticationProvider authProvider = null, IOfflineStore store = null)
    {
        var options = new DatasyncClientOptions
        {
            HttpPipeline = BuildHandlers(authProvider, MovieServer.CreateHandler()),
            OfflineStore = store,
            IdGenerator = MyIdGenerator
        };
        return authProvider == null ? new DatasyncClient(Endpoint, options) : new DatasyncClient(Endpoint, options);
    }

    /// <summary>
    /// Custom Id Generator
    /// </summary>
    /// <param name="tableName"></param>
    /// <returns></returns>
    protected string MyIdGenerator(string tableName)
    {
        Arguments.IsNotNullOrWhitespace(tableName, nameof(tableName));
        // user code for custom id generation
        return tableName + "_id";
    }

    /// <summary>
    /// Gets the random ID for a movie.
    /// </summary>
    /// <returns></returns>
    protected static string GetRandomId() => TestData.Movies.GetRandomId();

    /// <summary>
    /// We use "Black Panther" as a sample movie throughout the tests.
    /// </summary>
    /// <typeparam name="T">The concrete type of create.</typeparam>
    /// <returns>The sample movie in the type provided</returns>
    protected static T GetSampleMovie<T>() where T : IMovie, new()
    {
        return new T()
        {
            BestPictureWinner = true,
            Duration = 134,
            Rating = "PG-13",
            ReleaseDate = DateTimeOffset.Parse("16-Feb-2018"),
            Title = "Black Panther",
            Year = 2018
        };
    }

    /// <summary>
    /// Converts an object into a <see cref="JObject"/>.
    /// </summary>
    /// <typeparam name="T">The type of the object.</typeparam>
    /// <param name="obj">The object.</param>
    /// <returns>The equivalent <see cref="JObject"/></returns>
    protected static JObject CreateJsonDocument<T>(T obj)
    {
        var settings = new DatasyncSerializerSettings();
        return JObject.FromObject(obj, settings.GetSerializerFromSettings());
    }

    /// <summary>
    /// Split, sort, and re-join a query string so that it can be compared easily
    /// </summary>
    protected static string NormalizeQueryString(string queryString)
    {
        var splitArgs = queryString.Split('&').ToList();
        splitArgs.Sort();
        return string.Join('&', splitArgs).TrimStart('&');
    }

    /// <summary>
    /// Wait until a condition is met - useful for testing async processes.
    /// </summary>
    /// <param name="func"></param>
    /// <param name="ms"></param>
    /// <param name="maxloops"></param>
    /// <returns></returns>
    protected static async Task<bool> WaitUntil(Func<bool> func, int ms = 10, int maxloops = 500)
    {
        int waitCtr = 0;
        do
        {
            waitCtr++;
            await Task.Delay(ms).ConfigureAwait(false);
        } while (!func.Invoke() && waitCtr < maxloops);
        return waitCtr < maxloops;
    }

    /// <summary>
    /// Log the provided response.
    /// </summary>
    /// <param name="logger">The logger</param>
    /// <param name="response">The response</param>
    /// <returns></returns>
    protected async Task AssertResponseWithLoggingAsync(HttpStatusCode expectedStatusCode, HttpResponseMessage response)
    {
        if (response.RequestMessage != null)
        {
            logger?.WriteLine($"Request: {response.RequestMessage.RequestUri}");
        }
        if (response.StatusCode != expectedStatusCode)
        {
            logger?.WriteLine($"Response (expected: {expectedStatusCode}): {response.StatusCode} {response.ReasonPhrase}");
            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(true);
            logger?.WriteLine($"Content: {content}");
        }
        Assert.Equal(expectedStatusCode, response.StatusCode);
    }
}
