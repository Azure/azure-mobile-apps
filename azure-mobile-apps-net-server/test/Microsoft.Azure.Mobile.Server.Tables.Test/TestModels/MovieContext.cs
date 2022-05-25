// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------------------- 

using System.Data.Entity;

namespace Microsoft.Azure.Mobile.Server.TestModels
{
    public class MovieContext : EntityContext
    {
        public DbSet<Movie> Movies { get; set; }
    }
}
