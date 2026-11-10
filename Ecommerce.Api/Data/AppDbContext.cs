using Ecommerce.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options): base(options)
    {
        //this.ChangeTracker.LazyLoadingEnabled = false; // disable lazy loading
    }

    public DbSet<AreaAvailable> AreasAvailable => Set<AreaAvailable>();
    public DbSet<Barangay> Barangays => Set<Barangay>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<City> Cities => Set<City>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<CustomerClass> CustomerClasses => Set<CustomerClass>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderDetail> OrderDetails => Set<OrderDetail>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Province> Provinces => Set<Province>();
    public DbSet<UnitOfMeasure> UnitsOfMeasure => Set<UnitOfMeasure>();
    public DbSet<UnitPrice> UnitPrices => Set<UnitPrice>();
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<AreaAvailable>(entity =>
        {
            entity.HasKey(area => area.AreaAvailableId);

            entity.HasOne(area => area.Province)
                .WithMany()
                .HasForeignKey(area => area.ProvinceId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<Barangay>(entity =>
        {
            entity.HasKey(barangay => barangay.BarangayId);

            entity.HasOne(barangay => barangay.Province)
                .WithMany()
                .HasForeignKey(barangay => barangay.ProvinceId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(barangay => barangay.City)
                .WithMany(city => city.Barangays)
                .HasForeignKey(barangay => barangay.CityId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<CartItem>(entity =>
        {
            entity.HasKey(item => item.Id);

            entity.HasOne(item => item.User)
                .WithMany()
                .HasForeignKey(item => item.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(item => item.Product)
                .WithMany()
                .HasForeignKey(item => item.ProductId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(item => item.Uom)
                .WithMany()
                .HasForeignKey(item => item.UomId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<Category>()
            .HasKey(category => category.CategoryId);

        modelBuilder.Entity<City>(entity =>
        {
            entity.HasKey(city => city.CityId);

            entity.HasOne(city => city.Province)
                .WithMany(province => province.Cities)
                .HasForeignKey(city => city.ProvinceId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(customer => customer.CustomerId);

            entity.HasOne(customer => customer.CustomerClass)
                .WithMany()
                .HasForeignKey(customer => customer.CustomerClassId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(customer => customer.Province)
                .WithMany()
                .HasForeignKey(customer => customer.ProvinceId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(customer => customer.City)
                .WithMany()
                .HasForeignKey(customer => customer.CityId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(customer => customer.Barangay)
                .WithMany()
                .HasForeignKey(customer => customer.BarangayId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<CustomerClass>()
            .HasKey(customerClass => customerClass.CustomerClassId);

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(order => order.OrderId);

            entity.HasOne(order => order.Customer)
                .WithMany()
                .HasForeignKey(order => order.CustomerId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasMany(order => order.OrderDetail)
                .WithOne(detail => detail.Order)
                .HasForeignKey(detail => detail.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<OrderDetail>(entity =>
        {
            entity.HasKey(detail => detail.Id);

            entity.HasOne(detail => detail.Product)
                .WithMany()
                .HasForeignKey(detail => detail.ProductId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(detail => detail.UnitOfMeasure)
                .WithMany()
                .HasForeignKey(detail => detail.UomId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(product => product.ProductId);

            entity.HasOne(product => product.Category)
                .WithMany()
                .HasForeignKey(product => product.CategoryId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasMany(product => product.UnitPrices)
                .WithOne(unitPrice => unitPrice.Product)
                .HasForeignKey(unitPrice => unitPrice.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(product => product.AreasAvailable)
                .WithOne()
                .HasForeignKey("ProductId")
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Province>()
            .HasKey(province => province.ProvinceId);

        modelBuilder.Entity<UnitOfMeasure>()
            .HasKey(unit => unit.Id);

        modelBuilder.Entity<UnitPrice>(entity =>
        {
            entity.HasKey(unitPrice => unitPrice.UnitPriceId);

            entity.HasOne(unitPrice => unitPrice.UnitOfMeasure)
                .WithMany()
                .HasForeignKey(unitPrice => unitPrice.UomId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<User>()
            .HasKey(user => user.Id);
    }
}