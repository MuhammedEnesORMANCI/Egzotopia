using Microsoft.EntityFrameworkCore;
using Egzotopia.Models;

namespace Egzotopia.Data
{
    public class EgZotopiaDbContext : DbContext
    {
        public EgZotopiaDbContext(DbContextOptions<EgZotopiaDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }

    }
}