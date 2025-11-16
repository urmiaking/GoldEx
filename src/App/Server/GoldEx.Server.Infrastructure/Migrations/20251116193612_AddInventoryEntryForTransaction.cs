using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoldEx.Server.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddInventoryEntryForTransaction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "InventoryEntryId",
                table: "Transactions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_InventoryEntryId",
                table: "Transactions",
                column: "InventoryEntryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_InventoryEntries_InventoryEntryId",
                table: "Transactions",
                column: "InventoryEntryId",
                principalTable: "InventoryEntries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_InventoryEntries_InventoryEntryId",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_InventoryEntryId",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "InventoryEntryId",
                table: "Transactions");
        }
    }
}
