using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ITAssetManagement.Models.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AssetType",
                columns: table => new
                {
                    AssetTypeID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TypeName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssetType", x => x.AssetTypeID);
                });

            migrationBuilder.CreateTable(
                name: "Supplier",
                columns: table => new
                {
                    SupplierID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SupplierName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Contact = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Supplier", x => x.SupplierID);
                });

            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    UserID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Username = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Role = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.UserID);
                });

            migrationBuilder.CreateTable(
                name: "Asset",
                columns: table => new
                {
                    AssetID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AssetName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Serial = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Model = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    AssetTypeID = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PurchaseDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SupplierID = table.Column<int>(type: "int", nullable: true),
                    Location = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Config = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    QRCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Asset", x => x.AssetID);
                    table.ForeignKey(
                        name: "FK_Asset_AssetType_AssetTypeID",
                        column: x => x.AssetTypeID,
                        principalTable: "AssetType",
                        principalColumn: "AssetTypeID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Asset_Supplier_SupplierID",
                        column: x => x.SupplierID,
                        principalTable: "Supplier",
                        principalColumn: "SupplierID");
                });

            migrationBuilder.CreateTable(
                name: "AuditLog",
                columns: table => new
                {
                    LogID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TableName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    RecordID = table.Column<int>(type: "int", nullable: true),
                    OldValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NewValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLog", x => x.LogID);
                    table.ForeignKey(
                        name: "FK_AuditLog_User_UserID",
                        column: x => x.UserID,
                        principalTable: "User",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Department",
                columns: table => new
                {
                    DepartmentID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DeptName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ManagerID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Department", x => x.DepartmentID);
                    table.ForeignKey(
                        name: "FK_Department_User_ManagerID",
                        column: x => x.ManagerID,
                        principalTable: "User",
                        principalColumn: "UserID");
                });

            migrationBuilder.CreateTable(
                name: "InventoryCheck",
                columns: table => new
                {
                    CheckID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CheckDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    AssetID = table.Column<int>(type: "int", nullable: false),
                    ActualStatus = table.Column<int>(type: "int", nullable: true),
                    Discrepancy = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Note = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryCheck", x => x.CheckID);
                    table.ForeignKey(
                        name: "FK_InventoryCheck_Asset_AssetID",
                        column: x => x.AssetID,
                        principalTable: "Asset",
                        principalColumn: "AssetID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InventoryCheck_User_UserID",
                        column: x => x.UserID,
                        principalTable: "User",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RepairTicket",
                columns: table => new
                {
                    TicketID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AssetID = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RepairDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReplacedAssetID = table.Column<int>(type: "int", nullable: true),
                    Cost = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RepairTicket", x => x.TicketID);
                    table.ForeignKey(
                        name: "FK_RepairTicket_Asset_AssetID",
                        column: x => x.AssetID,
                        principalTable: "Asset",
                        principalColumn: "AssetID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RepairTicket_Asset_ReplacedAssetID",
                        column: x => x.ReplacedAssetID,
                        principalTable: "Asset",
                        principalColumn: "AssetID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RepairTicket_User_UserID",
                        column: x => x.UserID,
                        principalTable: "User",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AssetAllocation",
                columns: table => new
                {
                    AllocationID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AssetID = table.Column<int>(type: "int", nullable: false),
                    DepartmentID = table.Column<int>(type: "int", nullable: false),
                    UserID = table.Column<int>(type: "int", nullable: true),
                    AllocatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReturnedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssetAllocation", x => x.AllocationID);
                    table.ForeignKey(
                        name: "FK_AssetAllocation_Asset_AssetID",
                        column: x => x.AssetID,
                        principalTable: "Asset",
                        principalColumn: "AssetID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AssetAllocation_Department_DepartmentID",
                        column: x => x.DepartmentID,
                        principalTable: "Department",
                        principalColumn: "DepartmentID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AssetAllocation_User_UserID",
                        column: x => x.UserID,
                        principalTable: "User",
                        principalColumn: "UserID");
                });

            migrationBuilder.CreateTable(
                name: "WarehouseTransaction",
                columns: table => new
                {
                    TransactionID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Type = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    AssetID = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DepartmentID = table.Column<int>(type: "int", nullable: true),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ReferenceNo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WarehouseTransaction", x => x.TransactionID);
                    table.ForeignKey(
                        name: "FK_WarehouseTransaction_Asset_AssetID",
                        column: x => x.AssetID,
                        principalTable: "Asset",
                        principalColumn: "AssetID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WarehouseTransaction_Department_DepartmentID",
                        column: x => x.DepartmentID,
                        principalTable: "Department",
                        principalColumn: "DepartmentID");
                    table.ForeignKey(
                        name: "FK_WarehouseTransaction_User_UserID",
                        column: x => x.UserID,
                        principalTable: "User",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Asset_AssetTypeID",
                table: "Asset",
                column: "AssetTypeID");

            migrationBuilder.CreateIndex(
                name: "IX_Asset_Serial",
                table: "Asset",
                column: "Serial",
                unique: true,
                filter: "[Serial] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Asset_Status",
                table: "Asset",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Asset_SupplierID",
                table: "Asset",
                column: "SupplierID");

            migrationBuilder.CreateIndex(
                name: "IX_AssetAllocation_AssetID",
                table: "AssetAllocation",
                column: "AssetID");

            migrationBuilder.CreateIndex(
                name: "IX_AssetAllocation_DepartmentID",
                table: "AssetAllocation",
                column: "DepartmentID");

            migrationBuilder.CreateIndex(
                name: "IX_AssetAllocation_UserID",
                table: "AssetAllocation",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLog_UserID",
                table: "AuditLog",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_Department_ManagerID",
                table: "Department",
                column: "ManagerID",
                unique: true,
                filter: "[ManagerID] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryCheck_AssetID",
                table: "InventoryCheck",
                column: "AssetID");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryCheck_UserID",
                table: "InventoryCheck",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_RepairTicket_AssetID",
                table: "RepairTicket",
                column: "AssetID");

            migrationBuilder.CreateIndex(
                name: "IX_RepairTicket_ReplacedAssetID",
                table: "RepairTicket",
                column: "ReplacedAssetID");

            migrationBuilder.CreateIndex(
                name: "IX_RepairTicket_UserID",
                table: "RepairTicket",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_User_Email",
                table: "User",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_User_Username",
                table: "User",
                column: "Username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WarehouseTransaction_AssetID",
                table: "WarehouseTransaction",
                column: "AssetID");

            migrationBuilder.CreateIndex(
                name: "IX_WarehouseTransaction_DepartmentID",
                table: "WarehouseTransaction",
                column: "DepartmentID");

            migrationBuilder.CreateIndex(
                name: "IX_WarehouseTransaction_UserID",
                table: "WarehouseTransaction",
                column: "UserID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AssetAllocation");

            migrationBuilder.DropTable(
                name: "AuditLog");

            migrationBuilder.DropTable(
                name: "InventoryCheck");

            migrationBuilder.DropTable(
                name: "RepairTicket");

            migrationBuilder.DropTable(
                name: "WarehouseTransaction");

            migrationBuilder.DropTable(
                name: "Asset");

            migrationBuilder.DropTable(
                name: "Department");

            migrationBuilder.DropTable(
                name: "AssetType");

            migrationBuilder.DropTable(
                name: "Supplier");

            migrationBuilder.DropTable(
                name: "User");
        }
    }
}
