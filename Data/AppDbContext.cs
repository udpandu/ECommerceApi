using ECommerceApi.Models;
using Microsoft.EntityFrameworkCore;

namespace ECommerceApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Inventory> Inventories => Set<Inventory>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Indices
        modelBuilder.Entity<Customer>().HasIndex(c => c.Email).IsUnique();
        modelBuilder.Entity<Product>().HasIndex(p => p.SKU).IsUnique();

        // Composite Key for OrderItem
        modelBuilder.Entity<OrderItem>()
            .HasKey(oi => new { oi.OrderId, oi.ProductId });

        // Relationships
        modelBuilder.Entity<Inventory>()
            .HasOne(i => i.Product)
            .WithOne(p => p.Inventory)
            .HasForeignKey<Inventory>(i => i.ProductId);
    }
}
