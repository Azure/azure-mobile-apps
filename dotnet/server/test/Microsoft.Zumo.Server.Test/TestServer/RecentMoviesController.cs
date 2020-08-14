// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Mvc;
using Microsoft.Zumo.Server.Test.Helpers;
using System;

namespace Microsoft.Zumo.Server.Test.TestServer
{
    /// <summary>
    /// This controller is designed to test the table controller options for dataview.
    /// </summary>
    [Route("tables/recentmovies")]
    [ApiController]
    public class RecentMoviesController : TableController<Movie>
    {
        public RecentMoviesController()
        {
            // Create the repository
            var repository = new MockTableRepository<Movie>();
            for (var i = 0; i < TestData.Movies.Length; i++)
            {
                var clone = TestData.Movies[i].Clone();
                clone.Id = $"movie-{i}";
                clone.Version = Guid.NewGuid().ToByteArray();
                clone.UpdatedAt = DateTimeOffset.UtcNow.AddDays(-(180 + (new Random()).Next(180)));
                clone.Deleted = clone.MpaaRating == "R";
                repository.Data.Add(clone.Id, clone);
            }

            // Set up the table controller
            TableRepository = repository;
            TableControllerOptions = new TableControllerOptions<Movie> { 
                DataView = m => m.Year > 2000,
                MaxTop = 10,
                PageSize = 10
            };
        }

        public override bool IsAuthorized(TableOperation operation, Movie item)
        {
            return operation == TableOperation.Read || operation == TableOperation.List;
        }
    }
}
