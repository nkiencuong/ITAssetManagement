using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ITAssetManagement.Models.Migrations
{
    /// <inheritdoc />
    public partial class UpdateUserIDNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // --- Code này chỉ ra lệnh SỬA (Alter), không ra lệnh XÓA hay TẠO LẠI ---

            // 1. Tìm bảng WarehouseTransaction, sửa cột UserID
            migrationBuilder.AlterColumn<int>(
                name: "UserID",
                table: "WarehouseTransaction",
                type: "int",
                nullable: true, // Cho phép Null
                oldClrType: typeof(int),
                oldType: "int");

            // 2. Tìm bảng AssetAllocation, sửa cột UserID
            migrationBuilder.AlterColumn<int>(
                name: "UserID",
                table: "AssetAllocation",
                type: "int",
                nullable: true, // Cho phép Null
                oldClrType: typeof(int),
                oldType: "int");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Code này để phòng hờ, nếu muốn quay lại như cũ thì chạy
            migrationBuilder.AlterColumn<int>(
                name: "UserID",
                table: "WarehouseTransaction",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "UserID",
                table: "AssetAllocation",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }
    }
}