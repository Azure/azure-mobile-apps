// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Zumo.E2EServer.DataObjects;
using System.Linq;

namespace Microsoft.Zumo.E2EServer.Models
{
    public static class DbInitializer
    {
        /// <summary>
        /// Initializes the database within the context provided.
        /// </summary>
        /// <param name="context">The application database context</param>
        public static void Initialize(AppDbContext context)
        {
            context.Database.EnsureCreated();

            if (context.Movies.Any())
            {
                // We've already initialized the database
                return;
            }

            var movies = TestMovies.GetTestMovies();
            context.Movies.AddRange(movies);
            context.SaveChanges();
        }
    }
}
