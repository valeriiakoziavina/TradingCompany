using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;
using TradingCompany.Core.Entities;

namespace TradingCompany.DAL.Context;

public class TradingDbContext : DbContext
{
    public TradingDbContext(DbContextOptions<TradingDbContext> options) : base(options) { }

    public DbSet<Role> Roles => Set<Role>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<Waybill> Waybills => Set<Waybill>();
    public DbSet<ShipmentStatus> ShipmentStatuses => Set<ShipmentStatus>();
    public DbSet<Shipment> Shipments => Set<Shipment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Role>().HasIndex(r => r.Name).IsUnique();
        modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();
        modelBuilder.Entity<Product>().HasIndex(p => p.SKU).IsUnique();
        modelBuilder.Entity<Order>().HasIndex(o => o.OrderNumber).IsUnique();
        modelBuilder.Entity<ShipmentStatus>().HasIndex(s => s.Name).IsUnique();
        modelBuilder.Entity<Waybill>().HasIndex(w => w.WaybillNumber).IsUnique();

        modelBuilder.Entity<Waybill>()
            .HasOne(w => w.Order)
            .WithOne(o => o.Waybill)
            .HasForeignKey<Waybill>(w => w.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<OrderItem>()
            .HasOne(oi => oi.Order)
            .WithMany(o => o.OrderItems)
            .HasForeignKey(oi => oi.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Shipment>()
            .HasOne(s => s.Waybill)
            .WithMany(w => w.Shipments)
            .HasForeignKey(s => s.WaybillId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}