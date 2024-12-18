using Microsoft.EntityFrameworkCore;
using StockServiceAPI.Models;

namespace StockServiceAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        public DbSet<Users> Users { get; set; }
        public DbSet<Stocks> Stocks { get; set; }
        public DbSet<StockPrices> StockPrices { get; set; }
        public DbSet<Portfolios> Portfolios { get; set; }
        public DbSet<Transactions> Transactions { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Users
            modelBuilder.Entity<Users>()
                .HasMany(u => u.Portfolio)
                .WithOne(p => p.User)
                .HasForeignKey(p => p.UserId);

            modelBuilder.Entity<Users>()
                .HasMany(u => u.Transaction)
                .WithOne(t => t.User)
                .HasForeignKey(t => t.UserId);

            // Stocks
            modelBuilder.Entity<Stocks>()
                .HasMany(s => s.StockPrice)
                .WithOne(sp => sp.Stock)
                .HasForeignKey(sp => sp.StockId);

            modelBuilder.Entity<Stocks>()
                .HasMany(s => s.Portfolio)
                .WithOne(p => p.Stock)
                .HasForeignKey(p => p.StockId);

            modelBuilder.Entity<Stocks>()
                .HasMany(s => s.Transaction)
                .WithOne(t => t.Stock)
                .HasForeignKey(t => t.StockId);

            // Portfolios
            modelBuilder.Entity<Portfolios>()
                .HasKey(p => p.Id);

            // Transactions
            modelBuilder.Entity<Transactions>()
                .HasKey(t => t.Id);
        }

    }
}