using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ITAssetManagement.Models.Migrations
{
    /// <inheritdoc />
    public partial class RenamePhoneColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "MustChangePassword",
                table: "User",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                table: "User",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MustChangePassword",
                table: "User");

            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                table: "User");
        }
    }
}
