using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ITAssetManagement.Models.Migrations
{
    /// <inheritdoc />
    public partial class AddSolutionColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Solution",
                table: "RepairTicket",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Solution",
                table: "RepairTicket");
        }
    }
}
