using E2EServer.DataObjects;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace E2EServer.Database
{
    public class E2EDbContext : DbContext
    {
        public E2EDbContext(DbContextOptions<E2EDbContext> options): base(options)
        {
        }

        public DbSet<BlogPost> BlogPosts { get; set; }
        public DbSet<BlogComment> BlogComments { get; set; }
    }
}
