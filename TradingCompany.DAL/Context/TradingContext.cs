using Microsoft.EntityFrameworkCore;
using TradingCompany.Core.Entities;

namespace TradingCompany.DAL.Context;

public class TradingDbContext : DbContext
{
    public TradingDbContext(DbContextOptions<TradingDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<Waybill> Waybills => Set<Waybill>();
    public DbSet<ShipmentStatus> ShipmentStatuses => Set<ShipmentStatus>();
    public DbSet<Shipment> Shipments => Set<Shipment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(e =>
        {
            e.Property(u => u.Login).HasMaxLength(50);
            e.Property(u => u.PasswordHash).HasMaxLength(64);
            e.Property(u => u.FullName).HasMaxLength(100);
            e.Property(u => u.PhoneNumber).HasMaxLength(20);
            e.Property(u => u.Email).HasMaxLength(100);
            e.HasIndex(u => u.Login).IsUnique();
            e.HasIndex(u => u.Email).IsUnique();
        });

        modelBuilder.Entity<Order>(e =>
        {
            e.Property(o => o.OrderNumber).HasMaxLength(50);
            e.Property(o => o.TotalAmount).HasPrecision(18, 2);
            e.HasIndex(o => o.OrderNumber).IsUnique();
        });

        modelBuilder.Entity<Waybill>(e =>
        {
            e.Property(w => w.WaybillNumber).HasMaxLength(50);
            e.Property(w => w.PaymentType).HasMaxLength(50);
            e.Property(w => w.DeliveryMethod).HasMaxLength(100);
            e.Property(w => w.City).HasMaxLength(100);
            e.Property(w => w.DeliveryAddress).HasMaxLength(200);
            e.HasIndex(w => w.WaybillNumber).IsUnique();

            e.HasOne(w => w.Order)
                .WithOne(o => o.Waybill)
                .HasForeignKey<Waybill>(w => w.OrderId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(w => w.Customer)
                .WithMany(u => u.Waybills)
                .HasForeignKey(w => w.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ShipmentStatus>(e =>
        {
            e.Property(s => s.Name).HasMaxLength(50);
            e.HasIndex(s => s.Name).IsUnique();
            e.HasData(
                new ShipmentStatus { Id = ShipmentStatus.PendingId, Name = "Очікує відправлення" },
                new ShipmentStatus { Id = ShipmentStatus.ShippedId, Name = "Відправлено" });
        });

        modelBuilder.Entity<Shipment>(e =>
        {
            e.Property(s => s.Notes).HasMaxLength(500);

            e.HasOne(s => s.Waybill)
                .WithMany(w => w.Shipments)
                .HasForeignKey(s => s.WaybillId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(s => s.Manager)
                .WithMany(u => u.ManagedShipments)
                .HasForeignKey(s => s.ManagerId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(s => s.Status)
                .WithMany(st => st.Shipments)
                .HasForeignKey(s => s.StatusId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
