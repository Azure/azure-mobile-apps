// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
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
        public static void Initialize(IWebHostEnvironment env, AppDbContext context)
        {
            if (env.IsDevelopment())
            {
                context.Database.EnsureDeleted();
            }
            context.Database.EnsureCreated();

            if (!context.Movies.Any())
            {
                var movies = TestMovies.GetTestMovies();
                context.Movies.AddRange(movies);
                context.SaveChanges();
            }
        }
    }
}
