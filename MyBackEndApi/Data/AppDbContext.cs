using Microsoft.EntityFrameworkCore;
using MyBackEndApi.Models;

namespace MyBackEndApi.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // -------------------- DbSets --------------------
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }
        public DbSet<ProductColor> ProductColors { get; set; }
        public DbSet<ProductSize> ProductSizes { get; set; }
        public DbSet<ProductVariant> ProductVariants { get; set; }

        public DbSet<Admin> Admins { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<User> Users { get; set; }

        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Sale> Sales { get; set; }

        public DbSet<LowStockAlert> LowStockAlerts { get; set; }
        public DbSet<ActivityLog> ActivityLogs { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<Favorite> Favorites { get; set; }
        public DbSet<Banner> Banners { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<PasswordResetToken> PasswordResetTokens { get; set; }
        //public DbSet<passwordResets> passwordResets { get; set; }

        // -------------------- Model Configuration --------------------
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ----- Table names (if not following conventions) -----
            modelBuilder.Entity<Category>().ToTable("categories");
            modelBuilder.Entity<Product>().ToTable("products");
            modelBuilder.Entity<ProductImage>().ToTable("product_images");
            modelBuilder.Entity<ProductColor>().ToTable("product_colors");
            modelBuilder.Entity<ProductSize>().ToTable("product_sizes");
            modelBuilder.Entity<ProductVariant>().ToTable("product_variants");

            modelBuilder.Entity<Order>().ToTable("orders");
            modelBuilder.Entity<OrderItem>().ToTable("order_items");
            modelBuilder.Entity<Sale>().ToTable("sales");

            modelBuilder.Entity<Admin>().ToTable("admins");
            modelBuilder.Entity<User>().ToTable("users");
            modelBuilder.Entity<RefreshToken>().ToTable("refresh_tokens");

            modelBuilder.Entity<LowStockAlert>().ToTable("low_stock_alerts");
            modelBuilder.Entity<ActivityLog>().ToTable("activity_logs");
            modelBuilder.Entity<Cart>().ToTable("carts");
            modelBuilder.Entity<Favorite>().ToTable("favorites");

            // ----- Product Variant (unique constraint & FK) -----
            modelBuilder.Entity<ProductVariant>()
                .HasIndex(v => new { v.ProductId, v.Size, v.ColorName })
                .IsUnique()
                .HasDatabaseName("UQ_product_variants_product_size_color");

            modelBuilder.Entity<ProductVariant>()
                .HasOne(v => v.Product)
                .WithMany(p => p.Variants)
                .HasForeignKey(v => v.ProductId)
                .OnDelete(DeleteBehavior.Cascade);  

            // ----- Product relationships -----
            modelBuilder.Entity<Product>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.category_id)    // uses C# property name (mapped via [Column] in model)
                .OnDelete(DeleteBehavior.SetNull);   // or Restrict – choose based on business rules

            modelBuilder.Entity<ProductImage>()
                .HasOne(pi => pi.Product)
                .WithMany(p => p.ProductImages)
                .HasForeignKey(pi => pi.product_id)
                .OnDelete(DeleteBehavior.Cascade);

            // If you still have ProductColor and ProductSize (they may be replaced by variants later)
            modelBuilder.Entity<ProductColor>()
                .HasOne(pc => pc.Product)
                .WithMany(p => p.ProductColors)
                .HasForeignKey(pc => pc.product_id)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ProductSize>()
                .HasOne(ps => ps.Product)
                .WithMany(p => p.ProductSizes)
                .HasForeignKey(ps => ps.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // ----- Order relationships -----
            modelBuilder.Entity<Order>()
                .HasOne(o => o.User)
                .WithMany()
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Order>()
                .HasOne(o => o.Cashier)
                .WithMany()
                .HasForeignKey(o => o.CashierId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Order)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Product)
                .WithMany()
                .HasForeignKey(oi => oi.ProductId)
                .OnDelete(DeleteBehavior.Restrict);   // prevent deletion of product if ordered

            // ----- Sale relationships -----
            modelBuilder.Entity<Sale>()
                .HasOne(s => s.Order)
                .WithMany()
                .HasForeignKey(s => s.OrderId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Sale>()
                .HasOne(s => s.Cashier)
                .WithMany()
                .HasForeignKey(s => s.CashierId)
                .OnDelete(DeleteBehavior.SetNull);

            // ----- Cart & Favorite -----
            modelBuilder.Entity<Cart>()
                .HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Cart>()
                .HasOne(c => c.Product)
                .WithMany()
                .HasForeignKey(c => c.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Favorite>()
                .HasOne(f => f.User)
                .WithMany()
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Favorite>()
                .HasOne(f => f.Product)
                .WithMany()
                .HasForeignKey(f => f.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // ----- LowStockAlert -----
            modelBuilder.Entity<LowStockAlert>()
                .HasOne(ls => ls.Product)
                .WithMany()
                .HasForeignKey(ls => ls.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // ----- RefreshToken -----
            modelBuilder.Entity<RefreshToken>()
                .HasOne(rt => rt.Admin)
                .WithMany()
                .HasForeignKey(rt => rt.AdminId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}