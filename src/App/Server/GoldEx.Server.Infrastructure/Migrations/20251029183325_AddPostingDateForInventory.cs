using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoldEx.Server.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPostingDateForInventory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "PostingDate",
                table: "InventoryStocks",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "ReverseInventoryStockId",
                table: "InventoryStocks",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_InventoryStocks_PostingDate",
                table: "InventoryStocks",
                column: "PostingDate");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryStocks_ReverseInventoryStockId",
                table: "InventoryStocks",
                column: "ReverseInventoryStockId",
                unique: true,
                filter: "[ReverseInventoryStockId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryStocks_InventoryStocks_ReverseInventoryStockId",
                table: "InventoryStocks",
                column: "ReverseInventoryStockId",
                principalTable: "InventoryStocks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InventoryStocks_InventoryStocks_ReverseInventoryStockId",
                table: "InventoryStocks");

            migrationBuilder.DropIndex(
                name: "IX_InventoryStocks_PostingDate",
                table: "InventoryStocks");

            migrationBuilder.DropIndex(
                name: "IX_InventoryStocks_ReverseInventoryStockId",
                table: "InventoryStocks");

            migrationBuilder.DropColumn(
                name: "PostingDate",
                table: "InventoryStocks");

            migrationBuilder.DropColumn(
                name: "ReverseInventoryStockId",
                table: "InventoryStocks");
        }
    }
}
