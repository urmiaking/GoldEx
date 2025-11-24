using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoldEx.Server.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ChangeInventoryEntryRestrictToCascade : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_InventoryEntries_InventoryEntryId",
                table: "Transactions");

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_InventoryEntries_InventoryEntryId",
                table: "Transactions",
                column: "InventoryEntryId",
                principalTable: "InventoryEntries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_InventoryEntries_InventoryEntryId",
                table: "Transactions");

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_InventoryEntries_InventoryEntryId",
                table: "Transactions",
                column: "InventoryEntryId",
                principalTable: "InventoryEntries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
