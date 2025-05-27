using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;
using GrossistenApp.Data;
using GrossistenApp.Models;

namespace GrossistenApp.Data
{
   
    public class GrossistenAppDatabaseContext : DbContext
    {
        public GrossistenAppDatabaseContext(DbContextOptions<GrossistenAppDatabaseContext> options)
        : base(options)
        {
        }

        public DbSet<Product> ProductsTable { get; set; }
        public DbSet<Receipt> ReceiptsTable { get; set; }

        protected override async void OnModelCreating(ModelBuilder builder)
        {
        }
    }
}
