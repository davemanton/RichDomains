using System.ComponentModel;
using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System.Data.Common;

namespace DataAccess;

public class OrderDemoContext : DbContext
{
    private readonly DbConnection _connection;

    public OrderDemoContext(DbContextOptions<OrderDemoContext> options)
        : base(options)
    {
        _connection = RelationalOptionsExtension.Extract(options).Connection!;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Order>(entity =>
                                   {
                                       entity.ToTable("orders");

                                       entity.HasKey(e => e.OrderId);
                                       entity.Property(e => e.OrderId)
                                             .ValueGeneratedOnAdd();

                                       entity.Property(e => e.Address)
                                             .HasMaxLength(255)
                                             .IsUnicode(false);

                                       entity.Property(e => e.FirstName)
                                             .HasMaxLength(255)
                                             .IsUnicode(false);

                                       entity.Property(e => e.LastName)
                                             .HasMaxLength(255)
                                             .IsUnicode(false);

                                       entity.Property(e => e.DiscountId)
                                             .HasColumnName("DiscountId")
                                             .HasColumnType("int")
                                           ;
                                   });

        modelBuilder.Entity<LineItem>(entity =>
                                      {
                                          entity.ToTable("lineItems");
                                          entity.HasKey(e => new
                                          {
                                              e.OrderId,
                                              e.ProductId
                                          });

                                          entity.Property(e => e.Sku)
                                                .HasMaxLength(255)
                                                .IsUnicode(false);

                                          entity.Property(e => e.UnitCost)
                                                .HasColumnType("money");
                                          entity.Property(e => e.TotalCost)
                                                .HasColumnType("money");

                                          entity.HasOne(d => d.Order)
                                                .WithMany(p => p.LineItems)
                                                .HasForeignKey(d => d.OrderId)
                                                .OnDelete(DeleteBehavior.ClientSetNull)
                                                .HasConstraintName("FK_OrderProducts_Orders");

                                          entity.HasOne(d => d.Product)
                                                .WithMany(p => p.LineItems)
                                                .HasForeignKey(d => d.ProductId)
                                                .OnDelete(DeleteBehavior.ClientSetNull)
                                                .HasConstraintName("FK_OrderProducts_Products");
                                      });

        modelBuilder.Entity<Product>(entity =>
                                     {
                                         entity.ToTable("products");

                                         entity.HasKey(e => e.ProductId);
                                         entity.Property(e => e.ProductId)
                                               .ValueGeneratedOnAdd();

                                         entity.Property(e => e.Name)
                                               .HasMaxLength(255)
                                               .IsUnicode(false);

                                         entity.Property(e => e.UnitCost)
                                               .HasColumnType("money");

                                         entity.Property(e => e.Sku)
                                               .HasMaxLength(50)
                                               .IsUnicode(false);
                                     });

        modelBuilder.Entity<Discount>(entity =>
                                      {
                                          entity.ToTable("discountTypes");

                                          entity.HasKey(x => x.DiscountId);
                                          entity.Property(e => e.DiscountId)
                                                .ValueGeneratedOnAdd();

                                          entity.Property(e => e.Code)
                                                .HasMaxLength(50)
                                                .IsUnicode(false);

                                          entity.Property(e => e.Percentage)
                                                .HasColumnType("decimal");

                                          entity.Property(e => e.DiscountType)
                                                .HasConversion<int>();
                                      });
    }

    public override void Dispose()
    {
        _connection.Dispose();

        base.Dispose();
    }
}
