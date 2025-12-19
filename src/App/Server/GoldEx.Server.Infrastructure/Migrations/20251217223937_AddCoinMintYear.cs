using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoldEx.Server.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCoinMintYear : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EndMintYear",
                table: "Coins",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StartMintYear",
                table: "Coins",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EndMintYear",
                table: "Coins");

            migrationBuilder.DropColumn(
                name: "StartMintYear",
                table: "Coins");
        }
    }
}
