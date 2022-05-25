// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using Microsoft.Azure.Mobile.Server.Tables;
using ZumoE2EServerApp.DataObjects;

namespace ZumoE2EServerApp.Models
{
    public class SDKClientTestContext : DbContext
    {
        // If you want Entity Framework to drop and regenerate your database
        // automatically whenever you change your model schema, please use data migrations.
        // For more information refer to the documentation:
        // http://msdn.microsoft.com/en-us/data/jj591621.aspx

        public SDKClientTestContext()
            : base("Name=MS_TableConnectionString")
        {
        }

        public DbSet<Movie> Movies { get; set; }
        public DbSet<IntIdMovie> IntIdMovies { get; set; }

        public DbSet<RoundTripTableItem> RoundTripTableItems { get; set; }
        public DbSet<IntIdRoundTripTableItem> IntIdRoundTripTableItems { get; set; }

        public DbSet<Dates> Dates { get; set; }

        public DbSet<BlogPost> BlogPosts { get; set; }

        public DbSet<BlogComments> BlogComments { get; set; }

        public DbSet<OfflineReady> OfflineReadyItems { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Add(
                new AttributeToColumnAnnotationConvention<TableColumnAttribute, string>(
                    "ServiceTableColumn", (property, attributes) => attributes.Single().ColumnType.ToString()));
        }
    }
}