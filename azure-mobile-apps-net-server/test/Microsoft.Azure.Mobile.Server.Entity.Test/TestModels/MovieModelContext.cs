// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------------------- 

using System.Data.Entity;

namespace Microsoft.Azure.Mobile.Server.TestModels
{
    public class MovieModelContext : DbContext
    {
        public MovieModelContext()
            : base("Name=MS_TableConnectionString")
        {
        }

        public DbSet<MovieModel> MovieModels { get; set; }
    }
}
