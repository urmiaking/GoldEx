using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoldEx.Server.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddedOpeningInventory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "OpeningInventoryId",
                table: "InventoryStocks",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "OpeningInventories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OpeningInventories", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InventoryStocks_OpeningInventoryId",
                table: "InventoryStocks",
                column: "OpeningInventoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryStocks_OpeningInventories_OpeningInventoryId",
                table: "InventoryStocks",
                column: "OpeningInventoryId",
                principalTable: "OpeningInventories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InventoryStocks_OpeningInventories_OpeningInventoryId",
                table: "InventoryStocks");

            migrationBuilder.DropTable(
                name: "OpeningInventories");

            migrationBuilder.DropIndex(
                name: "IX_InventoryStocks_OpeningInventoryId",
                table: "InventoryStocks");

            migrationBuilder.DropColumn(
                name: "OpeningInventoryId",
                table: "InventoryStocks");
        }
    }
}
