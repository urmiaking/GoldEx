using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoldEx.Server.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class LinkedInventoryStocksToTransactions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "InventoryStockId",
                table: "Transactions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_InventoryStockId",
                table: "Transactions",
                column: "InventoryStockId");

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_InventoryStocks_InventoryStockId",
                table: "Transactions",
                column: "InventoryStockId",
                principalTable: "InventoryStocks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_InventoryStocks_InventoryStockId",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_InventoryStockId",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "InventoryStockId",
                table: "Transactions");
        }
    }
}
