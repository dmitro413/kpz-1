using CourseWork.Models;
using Microsoft.EntityFrameworkCore;

namespace CourseWork.Data;

public partial class MyDbContext : DbContext
{
    public MyDbContext()
    {
    }

    public MyDbContext(DbContextOptions<MyDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Brand> Brands { get; set; }
    public virtual DbSet<Order> Orders { get; set; }
    public virtual DbSet<OrderDetail> OrderDetails { get; set; }
    public virtual DbSet<OrderStatus> OrderStatuses { get; set; }
    public virtual DbSet<Product> Products { get; set; }
    public virtual DbSet<ProductBatch> ProductBatches { get; set; }
    public virtual DbSet<ProductVariant> ProductVariants { get; set; }
    public virtual DbSet<Review> Reviews { get; set; }
    public virtual DbSet<SchemaVersion> SchemaVersions { get; set; }
    public virtual DbSet<TypeOfProduct> TypeOfProducts { get; set; }
    public virtual DbSet<User> Users { get; set; }
    public virtual DbSet<Weight> Weights { get; set; }
    public virtual DbSet<LowStockProductDto> LowStockProducts { get; set; }


    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)=> optionsBuilder.UseSqlServer("Server=DESKTOP-2788V47;Database=SweetShop;Trusted_Connection=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Brand>(entity =>
        {
            entity.HasKey(e => e.BrandId).HasName("PK__Brand__DAD4F3BE0CECD94C");
            entity.ToTable("Brand");
            entity.HasIndex(e => e.BrandName, "UQ__Brand__2206CE9B83DF2487").IsUnique();
            entity.Property(e => e.BrandId).HasColumnName("BrandID");
            entity.Property(e => e.BrandName).HasMaxLength(50);
            entity.Property(e => e.Country).HasMaxLength(50);
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.ProductId).HasName("PK__Products__B40CC6CDCADD8157");

            entity.Property(e => e.BrandId).HasColumnName("BrandID");
            entity.Property(e => e.CaloriesPer100g).HasColumnType("decimal(8, 2)");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())").HasColumnType("datetime");
            entity.Property(e => e.ImageUrl).HasMaxLength(4000).HasColumnName("ImageURL");
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.TypeOfProductId).HasColumnName("TypeOfProductID");  
            
            entity.HasOne(d => d.Brand).WithMany(p => p.Products)
                .HasForeignKey(d => d.BrandId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Products__BrandI__24285DB4");

            entity.HasOne(d => d.TypeOfProduct).WithMany(p => p.Products)
                .HasForeignKey(d => d.TypeOfProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Products__TypeOf__2334397B");


            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(getdate())").HasColumnType("datetime")
                  .ValueGeneratedOnAddOrUpdate();
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            entity.HasQueryFilter(p => !p.IsDeleted);
            entity.ToTable(tb => tb.HasTrigger("TR_Products_UpdateDate"));
        });

        modelBuilder.Entity<ProductVariant>(entity =>
        {
            entity.HasKey(e => e.VariantId).HasName("PK__ProductV__0EA233E41C3EBE31");
            entity.HasIndex(e => new { e.ProductId, e.WeightId }, "UQ__ProductV__1426C9F364BF4ABB").IsUnique();
            entity.Property(e => e.VariantId).HasColumnName("VariantID");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())").HasColumnType("datetime");
            entity.Property(e => e.Price).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(getdate())").HasColumnType("datetime");
            entity.Property(e => e.WeightId).HasColumnName("WeightID");

            entity.HasOne(d => d.Product).WithMany(p => p.ProductVariants)
                .HasForeignKey(d => d.ProductId).OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("FK__ProductVa__Produ__2DB1C7EE");

            entity.HasOne(d => d.Weight).WithMany(p => p.ProductVariants)
                .HasForeignKey(d => d.WeightId).OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("FK__ProductVa__Weigh__2EA5EC27");


            entity.ToTable(tb => tb.HasTrigger("TR_ProductVariants_UpdateDate"));
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.OrderId).HasName("PK__Orders__C3905BAFF537B087");
            entity.Property(e => e.OrderId).HasColumnName("OrderID");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())").HasColumnType("datetime");
            entity.Property(e => e.CustomerEmail).HasMaxLength(400);
            entity.Property(e => e.CustomerName).HasMaxLength(250);
            entity.Property(e => e.CustomerPhone).HasMaxLength(20);
            entity.Property(e => e.DeliveryAddress).HasMaxLength(550);
            entity.Property(e => e.DeliveryDate).HasColumnType("datetime");
            entity.Property(e => e.OrderDate).HasDefaultValueSql("(getdate())").HasColumnType("datetime");
            entity.Property(e => e.StatusId).HasDefaultValue(1).HasColumnName("StatusID");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(getdate())").HasColumnType("datetime");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.Status).WithMany(p => p.Orders)
                .HasForeignKey(d => d.StatusId).OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("FK__Orders__StatusID__4589517F");

            entity.HasOne(d => d.User).WithMany(p => p.Orders)
                .HasForeignKey(d => d.UserId).HasConstraintName("FK__Orders__UserID__44952D46");

            entity.ToTable(tb => tb.HasTrigger("TR_Orders_UpdateDate"));
        });

        modelBuilder.Entity<ProductBatch>(entity =>
        {
            entity.HasKey(e => e.BatchId).HasName("PK__ProductB__5D55CE383C91B4E7");
            entity.Property(e => e.BatchId).HasColumnName("BatchID");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())").HasColumnType("datetime");
            entity.Property(e => e.PurchasePrice).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.SupplierName).HasMaxLength(300);
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(getdate())").HasColumnType("datetime");
            entity.Property(e => e.VariantId).HasColumnName("VariantID");


            entity.HasOne(d => d.Variant).WithMany(p => p.ProductBatches)
                .HasForeignKey(d => d.VariantId).OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("FK__ProductBa__Varia__345EC57D");

            entity.ToTable(tb => tb.HasTrigger("TR_ProductBatches_CheckStock"));
        });

        modelBuilder.Entity<Review>(entity =>
        {
            entity.HasKey(e => e.ReviewId).HasName("PK__Reviews__74BC79AEE570329D");
            entity.HasIndex(e => new { e.UserId, e.ProductId }, "UQ__Reviews__DCC800C3663E4FA9").IsUnique();
            entity.Property(e => e.ReviewId).HasColumnName("ReviewID");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())").HasColumnType("datetime");
            entity.Property(e => e.ProductId).HasColumnName("ProductID");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.Product).WithMany(p => p.Reviews)
                .HasForeignKey(d => d.ProductId).OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("FK__Reviews__Product__5006DFF2");

            entity.HasOne(d => d.User).WithMany(p => p.Reviews)
                .HasForeignKey(d => d.UserId).OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("FK__Reviews__UserID__4F12BBB9");

            entity.ToTable(tb => tb.HasTrigger("TR_UpdateProductRating"));
        });

      

        modelBuilder.Entity<OrderDetail>(entity => { 
            entity.HasKey(e => e.OrderDetailId).HasName("PK__OrderDet__D3B9D30C4FE21A12"); 
            entity.Property(e => e.OrderDetailId).HasColumnName("OrderDetailID"); 
            entity.Property(e => e.OrderId).HasColumnName("OrderID"); 
            entity.Property(e => e.UnitPrice).HasColumnType("decimal(10, 2)"); 
            entity.Property(e => e.VariantId).HasColumnName("VariantID"); 
            entity.HasOne(d => d.Order).WithMany(p => p.OrderDetails).HasForeignKey(d => d.OrderId).OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("FK__OrderDeta__Order__4865BE2A"); 
            entity.HasOne(d => d.Variant).WithMany(p => p.OrderDetails).HasForeignKey(d => d.VariantId).OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("FK__OrderDeta__Varia__4959E263"); 
        });
        modelBuilder.Entity<OrderStatus>(entity => { 
            entity.HasKey(e => e.StatusId).HasName("PK__OrderSta__C8EE20432EEA569B"); 
            entity.ToTable("OrderStatus"); entity.HasIndex(e => e.StatusName, "UQ__OrderSta__05E7698AED4E51AE").IsUnique(); 
            entity.Property(e => e.StatusId).HasColumnName("StatusID"); entity.Property(e => e.Description).HasMaxLength(250); 
            entity.Property(e => e.StatusName).HasMaxLength(50); 
        });
        modelBuilder.Entity<SchemaVersion>(entity => { 
            entity.HasKey(e => e.Id).HasName("PK__SchemaVe__3214EC07CFE87422"); 
            entity.Property(e => e.ScriptHash).HasMaxLength(64); 
            entity.Property(e => e.ScriptName).HasMaxLength(255); 
        });
        modelBuilder.Entity<TypeOfProduct>(entity => { 
            entity.HasKey(e => e.TypeOfProductId).HasName("PK__TypeOfPr__4FE05BB597A2FF5D"); 
            entity.ToTable("TypeOfProduct"); entity.HasIndex(e => e.TypeOfProductName, "UQ__TypeOfPr__2B8BAA28D2EBD8E3").IsUnique(); 
            entity.Property(e => e.TypeOfProductId).HasColumnName("TypeOfProductID"); entity.Property(e => e.TypeOfProductName).HasMaxLength(50); 
        });
        modelBuilder.Entity<User>(entity => { 
            entity.HasKey(e => e.UserId).HasName("PK__Users__1788CCAC02F2CEDB"); 
            entity.HasIndex(e => e.Email, "UQ__Users__A9D1053499056662").IsUnique(); 
            entity.Property(e => e.UserId).HasColumnName("UserID"); 
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())").HasColumnType("datetime"); 
            entity.Property(e => e.Email).HasMaxLength(400); entity.Property(e => e.FullName).HasMaxLength(250); 
            entity.Property(e => e.PasswordHash).HasMaxLength(2000); entity.Property(e => e.Phone).HasMaxLength(20); 
            entity.Property(e => e.Role).HasMaxLength(50).HasDefaultValue("Customer"); 
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(getdate())").HasColumnType("datetime"); 
        });

        modelBuilder.Entity<Weight>(entity => { 
            entity.HasKey(e => e.WeightId).HasName("PK__Weight__02A0F3FBC27949F2"); 
            entity.ToTable("Weight"); entity.HasIndex(e => new { e.WeightValue, e.Unit }, "UQ__Weight__41532DDA45565220").IsUnique(); 
            entity.Property(e => e.WeightId).HasColumnName("WeightID"); 
            entity.Property(e => e.Unit).HasMaxLength(10).HasDefaultValue("г"); 
            entity.Property(e => e.WeightValue).HasColumnType("decimal(8, 2)"); 
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}