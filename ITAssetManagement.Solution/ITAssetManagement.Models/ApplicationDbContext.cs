using ITAssetManagement.Models.Entities; // Hoặc .Entitis tùy tên thư mục của bạn
using ITAssetManagement.Models.Entitis;  // Giữ dòng này nếu namespace của bạn đang để là Entitis
using Microsoft.EntityFrameworkCore;

namespace ITAssetManagement.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // --- Danh sách các bảng trong Database ---
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
        public DbSet<RepairTicketDetail> RepairTicketDetails { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 1. Đặt tên bảng là số ít
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

            // 2. Ràng buộc duy nhất (Unique)
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // 3. CẤU HÌNH CHO REPAIR TICKET
            modelBuilder.Entity<RepairTicket>(entity =>
            {
                // A. Liên kết với Máy hỏng: Xóa Máy -> Xóa phiếu (Cascade)
                entity.HasOne(rt => rt.Asset)
                      .WithMany(a => a.RepairTickets)
                      .HasForeignKey(rt => rt.AssetID)
                      .OnDelete(DeleteBehavior.Cascade);

                // B. Liên kết với Linh kiện thay thế: Xóa Linh kiện -> KHÔNG xóa phiếu (Restrict)
                entity.HasOne(rt => rt.ReplacedAsset)
                      .WithMany()
                      .HasForeignKey(rt => rt.ReplacedAssetID)
                      .OnDelete(DeleteBehavior.Restrict);

                // C. Liên kết với Khoa phòng: Xóa Khoa -> Set NULL phiếu
                entity.HasOne(rt => rt.Department)
                      .WithMany()
                      .HasForeignKey(rt => rt.DepartmentID)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // 4. Định dạng tiền tệ
            modelBuilder.Entity<Asset>()
                .Property(a => a.Price)
                .HasPrecision(18, 2);

            modelBuilder.Entity<RepairTicket>()
                .Property(rt => rt.Cost)
                .HasPrecision(18, 2);

            // 5. Index tìm kiếm nhanh
            modelBuilder.Entity<Asset>()
                .HasIndex(a => a.Status);

            // 👇👇👇 6. CẤU HÌNH QUAN TRỌNG: USER - DEPARTMENT (FIX LỖI MIGRATION) 👇👇👇
            // Đoạn này giúp EF Core hiểu: User thuộc về Department, 
            // nhưng nếu xóa Department thì KHÔNG ĐƯỢC xóa User (Restrict) -> Tránh lỗi vòng lặp.
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasOne(u => u.Department)       // User có 1 Khoa
                      .WithMany()                      // Khoa có nhiều User
                      .HasForeignKey(u => u.DepartmentID) // Khóa ngoại là DepartmentID
                      .OnDelete(DeleteBehavior.Restrict); // QUAN TRỌNG: Chặn xóa cascade
            });
          
        }
    }
}