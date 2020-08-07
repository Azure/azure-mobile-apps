// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Zumo.Server.Test.E2EServer.DataObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Linq;

namespace Microsoft.Zumo.Server.Test.E2EServer.Database
{
    public class E2EDbContext : DbContext
    {
        public E2EDbContext(DbContextOptions<E2EDbContext> options) : base(options)
        {
        }

        public DbSet<BlogPost> BlogPosts { get; set; }
        public DbSet<BlogComment> BlogComments { get; set; }

        // Datasets for List and Get unit tests
        public DbSet<Movie> Movies { get; set; }
        public DbSet<RMovie> RMovies { get; set; }

        // Datasets for unit tests
        public DbSet<Unit> Units { get; set; }

        // Datasets for Create, Patch, Replace & Delete unit tests
        public DbSet<SUnit> SUnits { get; set; }
        public DbSet<HUnit> HUnits { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            if (Database.IsSqlite())
            {
                // SQLite does not support [Timestamp] attribute, which is required
                // by the EntityTableRepository
                var timestampProperties = modelBuilder.Model.GetEntityTypes()
                    .SelectMany(t => t.GetProperties())
                    .Where(p => p.ClrType == typeof(byte[]) && p.ValueGenerated == ValueGenerated.OnAddOrUpdate && p.IsConcurrencyToken);

                foreach (var property in timestampProperties)
                {
                    property.SetValueConverter(new SqliteTimestampConverter());
                    property.SetDefaultValueSql("CURRENT_TIMESTAMP");
                }
            }
        }
    }

    internal class SqliteTimestampConverter : ValueConverter<byte[], string>
    {
        public SqliteTimestampConverter() : base(
            v => v == null ? null : ToDb(v),
            v => v == null ? null : FromDb(v))
        { }

        static byte[] FromDb(string v) => v.Select(c => (byte)c).ToArray();

        static string ToDb(byte[] v) => new string(v.Select(b => (char)b).ToArray());
    }

}
