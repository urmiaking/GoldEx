using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoldEx.Server.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RenameToInventoryEntries : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InventoryStocks_OpeningInventories_OpeningInventoryId",
                table: "InventoryStocks");

            migrationBuilder.DropTable(
                name: "OpeningInventories");

            migrationBuilder.RenameColumn(
                name: "OpeningInventoryId",
                table: "InventoryStocks",
                newName: "InventoryEntryId");

            migrationBuilder.RenameIndex(
                name: "IX_InventoryStocks_OpeningInventoryId",
                table: "InventoryStocks",
                newName: "IX_InventoryStocks_InventoryEntryId");

            migrationBuilder.CreateTable(
                name: "InventoryEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryEntries", x => x.Id);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryStocks_InventoryEntries_InventoryEntryId",
                table: "InventoryStocks",
                column: "InventoryEntryId",
                principalTable: "InventoryEntries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InventoryStocks_InventoryEntries_InventoryEntryId",
                table: "InventoryStocks");

            migrationBuilder.DropTable(
                name: "InventoryEntries");

            migrationBuilder.RenameColumn(
                name: "InventoryEntryId",
                table: "InventoryStocks",
                newName: "OpeningInventoryId");

            migrationBuilder.RenameIndex(
                name: "IX_InventoryStocks_InventoryEntryId",
                table: "InventoryStocks",
                newName: "IX_InventoryStocks_OpeningInventoryId");

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

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryStocks_OpeningInventories_OpeningInventoryId",
                table: "InventoryStocks",
                column: "OpeningInventoryId",
                principalTable: "OpeningInventories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
