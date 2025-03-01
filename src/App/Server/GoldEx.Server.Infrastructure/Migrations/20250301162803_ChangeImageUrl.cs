using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoldEx.Server.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ChangeImageUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IconUrl",
                table: "Prices");

            migrationBuilder.AddColumn<string>(
                name: "IconFile",
                table: "Prices",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IconFile",
                table: "Prices");

            migrationBuilder.AddColumn<string>(
                name: "IconUrl",
                table: "Prices",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }
    }
}
