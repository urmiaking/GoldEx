using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoldEx.Server.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPricePin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsPinned",
                table: "Prices",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsPinned",
                table: "Prices");
        }
    }
}
