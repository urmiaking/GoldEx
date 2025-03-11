using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoldEx.Client.Offline.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSettingsForJewelry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Profit",
                table: "Settings",
                newName: "JewelryProfit");

            migrationBuilder.AddColumn<double>(
                name: "GoldProfit",
                table: "Settings",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GoldProfit",
                table: "Settings");

            migrationBuilder.RenameColumn(
                name: "JewelryProfit",
                table: "Settings",
                newName: "Profit");
        }
    }
}
