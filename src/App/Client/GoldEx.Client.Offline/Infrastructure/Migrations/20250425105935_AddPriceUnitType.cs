using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoldEx.Client.Offline.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPriceUnitType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UnitType",
                table: "Prices",
                type: "INTEGER",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UnitType",
                table: "Prices");
        }
    }
}
