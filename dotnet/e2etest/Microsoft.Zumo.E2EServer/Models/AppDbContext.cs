﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.EntityFrameworkCore;
using Microsoft.Zumo.E2EServer.DataObjects;

namespace Microsoft.Zumo.E2EServer.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }

        public DbSet<Movie> Movies { get; set; }
    }
}
