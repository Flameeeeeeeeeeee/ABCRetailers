using ABCRetailers.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic; // This was not in the image but is needed

namespace ABCRetailers.Data
{
    public class AuthDbContext : DbContext
    {
        public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options) { }

        public DbSet<User> Users => Set<User>();

        // Add these two:
        public DbSet<Cart> Cart => Set<Cart>();
        public DbSet<Order> Orders => Set<Order>();
    }
}