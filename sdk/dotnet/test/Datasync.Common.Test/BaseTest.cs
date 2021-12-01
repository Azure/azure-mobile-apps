// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Datasync.Common.Test.Models;
using Datasync.Common.Test.Service;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Datasync.Client;
using Microsoft.Datasync.Client.Authentication;
using Microsoft.Datasync.Client.Test.Helpers;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;

namespace Datasync.Common.Test
{
    /// <summary>
    /// A basic template for a test suite
    /// </summary>
    [ExcludeFromCodeCoverage(Justification = "Test suite")]
    public class BaseTest
    {
        private readonly Lazy<TestServer> _server = new(() => MovieApiServer.CreateTestServer());

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
        /// Get a datasync client that is completely mocked and doesn't require a service
        /// </summary>
        /// <param name="authProvider">... with the provided authentication provider</param>
        /// <returns>A datasync client</returns>
        protected DatasyncClient GetMockClient(AuthenticationProvider authProvider = null)
        {
            var options = new DatasyncClientOptions
            {
                HttpPipeline = new HttpMessageHandler[] { MockHandler }
            };
            return authProvider == null ? new DatasyncClient(Endpoint, options) : new DatasyncClient(Endpoint, authProvider, options);
        }

        /// <summary>
        /// Get a datasync client that is connected to the integration movie service.
        /// </summary>
        /// <param name="authProvider">... with the provided authentication provider</param>
        /// <returns>A datasync client</returns>
        protected DatasyncClient GetMovieClient(AuthenticationProvider authProvider = null)
        {
            var options = new DatasyncClientOptions
            {
                HttpPipeline = new HttpMessageHandler[] { MovieServer.CreateHandler() }
            };
            return authProvider == null ? new DatasyncClient(Endpoint, options) : new DatasyncClient(Endpoint, authProvider, options);
        }

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
        /// The mock handler that allows us to set responses and see requests.
        /// </summary>
        protected TestDelegatingHandler MockHandler { get; } = new();

        /// <summary>
        /// Creates a reference to the movie server, when needed.
        /// </summary>
        protected TestServer MovieServer { get => _server.Value; }

        /// <summary>
        /// Creates a <see cref="HttpClient"/> to access the movie server
        /// </summary>
        protected HttpClient MovieHttpClient { get => MovieServer.CreateClient(); }

        /// <summary>
        /// The start time for the test.
        /// </summary>
        protected DateTimeOffset StartTime { get; } = DateTimeOffset.UtcNow;

        /// <summary>
        /// Gets the random ID for a movie.
        /// </summary>
        /// <returns></returns>
        protected string GetRandomId() => TestData.Movies.GetRandomId();


    }
}
