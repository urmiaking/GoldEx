using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoldEx.Server.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddedPriceProviderMapping : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PriceCatalog",
                table: "Prices",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "PriceProviderMappings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProviderType = table.Column<int>(type: "int", nullable: false),
                    ProviderSymbol = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    PriceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PriceProviderMappings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PriceProviderMappings_Prices_PriceId",
                        column: x => x.PriceId,
                        principalTable: "Prices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PriceProviderMappings_PriceId",
                table: "PriceProviderMappings",
                column: "PriceId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PriceProviderMappings");

            migrationBuilder.DropColumn(
                name: "PriceCatalog",
                table: "Prices");
        }
    }
}
