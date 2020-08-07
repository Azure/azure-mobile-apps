// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Todo.AspNetCore.Server.Database
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
        }
    }
}
