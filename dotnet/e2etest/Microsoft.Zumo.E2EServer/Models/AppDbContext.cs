// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Zumo.E2EServer.DataObjects;
using System.Linq;
using System.Text;

namespace Microsoft.Zumo.E2EServer.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }

        public DbSet<Movie> Movies { get; set; }
        public DbSet<IntIdMovie> IntIdMovies { get; set; }
        public DbSet<Dates> Dates { get; set; }
        public DbSet<BlogPost> BlogPosts { get; set; }
        public DbSet<BlogComment> BlogComments { get; set; }
        public DbSet<RoundTripTableItem> RoundTripTableItems { get; set; }
        public DbSet<IntIdRoundTripTableItem> IntIdRoundTripTableItems { get; set; }
        public DbSet<OfflineReady> OfflineReadyItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<IntIdMovie>().Property(t => t.Id).ValueGeneratedOnAdd();

            if (Database.IsSqlite())
            {
                var properties = modelBuilder.Model.GetEntityTypes()
                    .SelectMany(t => t.GetProperties())
                    .Where(p => p.ClrType == typeof(byte[]) && p.ValueGenerated == ValueGenerated.OnAddOrUpdate && p.IsConcurrencyToken);
                foreach (var prop in properties)
                {
                    prop.SetValueConverter(new SqliteTimestampConverter());
                    // This allows ms timestamp
                    prop.SetDefaultValueSql("strftime('%Y-%m-%d %H:%M:%f', 'now', 'localtime')");

                }
            }
        }

        class SqliteTimestampConverter : ValueConverter<byte[], string>
        {
            public SqliteTimestampConverter() : base(v => v == null ? null : ToDb(v), v => v == null ? null : FromDb(v))
            { 
            }

            static byte[] FromDb(string v) => Encoding.ASCII.GetBytes(v);
            static string ToDb(byte[] v) => Encoding.ASCII.GetString(v);
        }
    }
}
