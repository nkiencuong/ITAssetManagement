using Microsoft.EntityFrameworkCore;
using ITAssetManagement.Models;
using ITAssetManagement.Models.Entitis;

namespace ITAssetManagement.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<AssetType> AssetTypes { get; set; } = null!;
        public DbSet<Supplier> Suppliers { get; set; } = null!;
        public DbSet<Department> Departments { get; set; } = null!;
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Asset> Assets { get; set; } = null!;
        public DbSet<WarehouseTransaction> WarehouseTransactions { get; set; } = null!;
        public DbSet<AssetAllocation> AssetAllocations { get; set; } = null!;
        public DbSet<AuditLog> AuditLogs { get; set; } = null!;
        public DbSet<InventoryCheck> InventoryChecks { get; set; } = null!;
        public DbSet<RepairTicket> RepairTickets { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // === Buộc tên bảng là số ít (không có "s") ===
            modelBuilder.Entity<AssetType>().ToTable("AssetType");
            modelBuilder.Entity<Supplier>().ToTable("Supplier");
            modelBuilder.Entity<Department>().ToTable("Department");
            modelBuilder.Entity<User>().ToTable("User");
            modelBuilder.Entity<Asset>().ToTable("Asset");
            modelBuilder.Entity<WarehouseTransaction>().ToTable("WarehouseTransaction");
            modelBuilder.Entity<AssetAllocation>().ToTable("AssetAllocation");
            modelBuilder.Entity<AuditLog>().ToTable("AuditLog");
            modelBuilder.Entity<InventoryCheck>().ToTable("InventoryCheck");
            modelBuilder.Entity<RepairTicket>().ToTable("RepairTicket");

            // === Unique constraints ===
            modelBuilder.Entity<Asset>()
                .HasIndex(a => a.Serial)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // === Fix cascade path cho RepairTicket (rất quan trọng) ===
            modelBuilder.Entity<RepairTicket>(entity =>
            {
                entity.HasOne(rt => rt.Asset)
                      .WithMany(a => a.RepairTickets)
                      .HasForeignKey(rt => rt.AssetID)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(rt => rt.ReplacedAsset)
                      .WithMany()
                      .HasForeignKey(rt => rt.ReplacedAssetID)
                      .OnDelete(DeleteBehavior.Restrict); // Đổi thành Restrict để tránh lỗi cascade
            });

            // === Precision cho decimal ===
            modelBuilder.Entity<Asset>()
                .Property(a => a.Price)
                .HasPrecision(18, 2);

            modelBuilder.Entity<RepairTicket>()
                .Property(rt => rt.Cost)
                .HasPrecision(18, 2);

            // === Index nhanh cho Status ===
            modelBuilder.Entity<Asset>()
                .HasIndex(a => a.Status);
        }
    }
}