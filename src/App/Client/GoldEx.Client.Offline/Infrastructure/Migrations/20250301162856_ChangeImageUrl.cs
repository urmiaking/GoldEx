using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoldEx.Client.Offline.Infrastructure.Migrations
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
                type: "TEXT",
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
                type: "TEXT",
                maxLength: 500,
                nullable: true);
        }
    }
}
