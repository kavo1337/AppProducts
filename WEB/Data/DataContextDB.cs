using Microsoft.EntityFrameworkCore;
using WEB.Models;

namespace WEB.Data
{
    public class DataContextDB(DbContextOptions<DataContextDB> options) : DbContext(options)
    {
        public DbSet<Product> Products => Set<Product>();
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Admin> Admins => Set<Admin>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
