using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ShopDomain.Model;

namespace ShopInfrastructure;

public partial class ShopDbContext : IdentityDbContext<User>
{
    public ShopDbContext()
    {
    }

    public ShopDbContext(DbContextOptions<ShopDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Cart> Carts { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Country> Countries { get; set; }

    public virtual DbSet<Manufacturer> Manufacturers { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<ProductCart> ProductCarts { get; set; }

    public virtual DbSet<ProductCategory> ProductCategories { get; set; }

    public virtual DbSet<ProductOrder> ProductOrders { get; set; }

    public virtual DbSet<Receipt> Receipts { get; set; }

    public virtual DbSet<Shiping> Shipings { get; set; }

    public virtual DbSet<ShippingCompany> ShippingCompanies { get; set; }

    //public virtual DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Cart>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.ToTable("carts");

            entity.HasIndex(e => e.UserId, "IX_carts_user_id_unique")
                .IsUnique()
                .HasFilter("[user_id] IS NOT NULL");

            entity.HasIndex(e => e.SessionId, "IX_carts_session_id_unique")
                .IsUnique()
                .HasFilter("[session_id] IS NOT NULL");

            entity.Property(e => e.Id).HasColumnName("ct_id");
            entity.Property(e => e.CtPrice).HasColumnName("ct_price");
            entity.Property(e => e.CtQuantity).HasColumnName("ct_quantity");
            entity.Property(e => e.UserId).HasColumnName("user_id")
                .HasMaxLength(450);
            entity.Property(e => e.SessionId).HasColumnName("session_id")
                .HasMaxLength(450);

            entity.HasOne(d => d.User).WithOne(p => p.Cart)
                .HasForeignKey<Cart>(d => d.UserId)
                .HasConstraintName("FK_carts_users");
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.ToTable("categories");

            entity.Property(e => e.Id).HasColumnName("cg_id");
            entity.Property(e => e.CgName)
                .HasMaxLength(100)
                .HasColumnName("cg_name");
            entity.Property(e => e.CgParentCategory)
                .HasMaxLength(100)
                .HasColumnName("cg_parent_category");
            entity.Property(e => e.CgDescription)
                .HasMaxLength(1000)
                .HasColumnName("cg_description");
            entity.Property(e => e.CgImage)
                .HasMaxLength(1000)
                .HasColumnName("cg_image");

            entity.Property(e => e.ParentCategoryId).HasColumnName("parent_category_id");

            entity.HasOne(e => e.ParentCategory)
                .WithMany(e => e.SubCategories)
                .HasForeignKey(e => e.ParentCategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });


        modelBuilder.Entity<Country>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.ToTable("countries");

            entity.Property(e => e.Id).HasColumnName("co_id");
            entity.Property(e => e.CoName)
                .HasMaxLength(50)
                .HasColumnName("co_name");
        });

        modelBuilder.Entity<Manufacturer>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.ToTable("manufacturers");

            entity.Property(e => e.Id).HasColumnName("mn_id");
            entity.Property(e => e.CountryId).HasColumnName("country_id");
            entity.Property(e => e.MnContactInfo)
                .HasMaxLength(1000)
                .HasColumnName("mn_contact_info");
            entity.Property(e => e.MnName)
                .HasMaxLength(50)
                .HasColumnName("mn_name");

            entity.HasOne(d => d.Country).WithMany(p => p.Manufacturers)
                .HasForeignKey(d => d.CountryId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_manufacturers_countries");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.ToTable("orders");

            entity.Property(e => e.Id).HasColumnName("od_id");
            entity.Property(e => e.OdDiscount).HasColumnName("od_discount")
                .HasMaxLength(4);
            entity.Property(e => e.OdNotes)
                .HasMaxLength(100)
                .HasColumnName("od_notes");
            entity.Property(e => e.OdPayment)
                .HasMaxLength(50)
                .HasColumnName("od_payment");
            entity.Property(e => e.OdTotal)
                .HasColumnType("decimal(18, 0)")
                .HasColumnName("od_total");
            entity.Property(e => e.OdUser).HasColumnName("od_user");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.ReceiptId).HasColumnName("receipt_id");
            entity.Property(e => e.ShippingId).HasColumnName("shipping_id");

            entity.HasOne(d => d.OdUserNavigation).WithMany(p => p.Orders)
                .HasForeignKey(d => d.OdUser)
                .HasConstraintName("FK_orders_users");

            entity.HasOne(d => d.Product).WithMany(p => p.Orders)
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("FK_orders_products");

            entity.HasOne(d => d.Receipt).WithMany(p => p.Orders)
                .HasForeignKey(d => d.ReceiptId)
                .HasConstraintName("FK_orders_receipts");

            entity.HasOne(d => d.Shipping).WithMany(p => p.Orders)
                .HasForeignKey(d => d.ShippingId)
                .HasConstraintName("FK_orders_shipings");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_product");

            entity.ToTable("products");

            entity.Property(e => e.Id).HasColumnName("pd_id");
            entity.Property(e => e.ManufacturerId).HasColumnName("manufacturer_id");
            entity.Property(e => e.PdAbout)
                .HasMaxLength(1000)
                .HasColumnName("pd_about");
            entity.Property(e => e.PdDiscount).HasColumnName("pd_discount");
            entity.Property(e => e.PdMeasurements)
                .HasMaxLength(10)
                .HasColumnName("pd_measurements");
            entity.Property(e => e.PdName)
                .HasMaxLength(100)
                .HasColumnName("pd_name");
            entity.Property(e => e.PdPrice)
                .HasColumnType("money")
                .HasColumnName("pd_price");
            entity.Property(e => e.PdQuantity).HasColumnName("pd_quantity");
            entity.Property(e => e.PdImagePath).HasColumnName("pd_image_path")
                .HasMaxLength(450);

            entity.HasOne(d => d.Manufacturer).WithMany(p => p.Products)
                .HasForeignKey(d => d.ManufacturerId)
                .HasConstraintName("FK_products_manufacturers");
        });

        modelBuilder.Entity<ProductCart>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.ToTable("product_carts");

            entity.Property(e => e.Id).HasColumnName("pc_id");
            entity.Property(e => e.CartId).HasColumnName("cart_id");
            entity.Property(e => e.PcPrice)
                .HasColumnType("decimal(18, 0)")
                .HasColumnName("pc_price");
            entity.Property(e => e.PcQuantity).HasColumnName("pc_quantity");
            entity.Property(e => e.ProductId).HasColumnName("product_id");

            entity.HasOne(d => d.Cart).WithMany(p => p.ProductCarts)
                .HasForeignKey(d => d.CartId)
                .HasConstraintName("FK_product_carts_carts");

            entity.HasOne(d => d.Product).WithMany(p => p.ProductCarts)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_product_carts_products");
        });

        modelBuilder.Entity<ProductCategory>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.ToTable("product_categories");

            entity.Property(e => e.Id).HasColumnName("pct_id");
            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.ProductId).HasColumnName("product_id");

            entity.HasOne(d => d.Category).WithMany(p => p.ProductCategories)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_product_categories_categories_1");

            entity.HasOne(d => d.Product).WithMany(p => p.ProductCategories)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_product_categories_products");
        });

        modelBuilder.Entity<ProductOrder>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.ToTable("product_orders");

            entity.Property(e => e.Id).HasColumnName("po_id");
            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.PoPrice)
                .HasColumnType("decimal(18, 0)")
                .HasColumnName("po_price");
            entity.Property(e => e.PoQuantity).HasColumnName("po_quantity");
            entity.Property(e => e.ProductId).HasColumnName("product_id");

            entity.HasOne(d => d.Order).WithMany(p => p.ProductOrders)
                .HasForeignKey(d => d.OrderId)
                .HasConstraintName("FK_product_orders_orders_1");

            entity.HasOne(d => d.Product).WithMany(p => p.ProductOrders)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_product_orders_products");
        });

        modelBuilder.Entity<Receipt>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.ToTable("receipts");

            entity.Property(e => e.Id).HasColumnName("rp_id");
            entity.Property(e => e.RpAbout)
                .HasMaxLength(100)
                .HasColumnName("rp_about");
            entity.Property(e => e.RpDateCreated)
                .HasColumnType("datetime")
                .HasColumnName("rp_date_created");
            entity.Property(e => e.RpDiscount)
                .HasMaxLength(10)
                .HasColumnName("rp_discount");
            entity.Property(e => e.RpPayment)
                .HasMaxLength(100)
                .HasColumnName("rp_payment");
            entity.Property(e => e.RpQuantity).HasColumnName("rp_quantity");
            entity.Property(e => e.RpTotal)
                .HasColumnType("decimal(18, 0)")
                .HasColumnName("rp_total");
            entity.Property(e => e.ShippingId).HasColumnName("shipping_id");

            entity.HasOne(d => d.Shipping).WithMany(p => p.Receipts)
                .HasForeignKey(d => d.ShippingId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_receipts_shipings");
        });

        modelBuilder.Entity<Shiping>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.ToTable("shipings");

            entity.Property(e => e.Id).HasColumnName("sh_id");
            entity.Property(e => e.CountryId).HasColumnName("country_id");
            entity.Property(e => e.ShAdress)
                .HasMaxLength(100)
                .HasColumnName("sh_adress");
            entity.Property(e => e.ShippingCompanyId).HasColumnName("shipping_company_id");
            entity.Property(e => e.ShTrackingNumber)
                .HasMaxLength(50)
                .HasColumnName("sh_tracking_number");
            entity.Property(e => e.ShStatus)
                .HasMaxLength(50)
                .HasColumnName("sh_status");

            entity.HasOne(d => d.Country).WithMany(p => p.Shipings)
                .HasForeignKey(d => d.CountryId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_shipings_countries");

            entity.HasOne(d => d.ShippingCompany).WithMany(p => p.Shipings)
                .HasForeignKey(d => d.ShippingCompanyId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_shipings_shipping_companies");
        });

        modelBuilder.Entity<ShippingCompany>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.ToTable("shipping_companies");

            entity.Property(e => e.Id).HasColumnName("sc_id");
            entity.Property(e => e.ScAvgTimeNeed)
                .HasMaxLength(100)
                .HasColumnName("sc_avg_time_need");
            entity.Property(e => e.ScName)
                .HasMaxLength(100)
                .HasColumnName("sc_name");
            entity.Property(e => e.ScPricing)
                .HasColumnType("decimal(18, 0)")
                .HasColumnName("sc_pricing");
            entity.Property(e => e.ScContactInfo)
                .HasMaxLength(100)
                .HasColumnName("sc_contact_info");
        });

        modelBuilder.Entity<User>(entity =>
        {
            // entity.HasKey(e => e.Id).HasName("PK_users");
            //
            // entity.ToTable("// users");
            //
            // entity.Property(e => e.Id).HasColumnName("ur_id");
            // entity.Property(e => e.UrBirthdate)
            //     .HasColumnType("datetime")
            //     .HasColumnName("ur_birthdate");
            // entity.Property(e => e.UrCountryId).HasColumnName("ur_country_id");
            // entity.Property(e => e.UrEmail)
            //     .HasMaxLength(10)
            //     .IsUnicode(false)
            //     .IsFixedLength()
            //     .HasColumnName("ur_email");
            // entity.Property(e => e.UrNickname)
            //     .HasMaxLength(10)
            //     .IsUnicode(false)
            //     .IsFixedLength()
            //     .HasColumnName("ur_nickname");
            // entity.Property(e => e.UrRole)
            //     .HasMaxLength(10)
            //     .IsUnicode(false)
            //     .IsFixedLength()
            //     .HasColumnName("ur_role");
            //
            // entity.HasOne(d => d.UrCountry).WithMany(p => p.Users)
            //     .HasForeignKey(d => d.UrCountryId)
            //     .OnDelete(DeleteBehavior.Cascade)
            //     .HasConstraintName("FK_users_countries");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
