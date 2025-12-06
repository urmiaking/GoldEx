using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoldEx.Server.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddInventoryExit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "InventoryExitId",
                table: "Transactions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "InventoryExitId",
                table: "InventoryStocks",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "InventoryExits",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ExitReason = table.Column<int>(type: "int", nullable: false),
                    ExitDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryExits", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_InventoryExitId",
                table: "Transactions",
                column: "InventoryExitId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryStocks_InventoryExitId",
                table: "InventoryStocks",
                column: "InventoryExitId");

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryStocks_InventoryExits_InventoryExitId",
                table: "InventoryStocks",
                column: "InventoryExitId",
                principalTable: "InventoryExits",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_InventoryExits_InventoryExitId",
                table: "Transactions",
                column: "InventoryExitId",
                principalTable: "InventoryExits",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InventoryStocks_InventoryExits_InventoryExitId",
                table: "InventoryStocks");

            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_InventoryExits_InventoryExitId",
                table: "Transactions");

            migrationBuilder.DropTable(
                name: "InventoryExits");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_InventoryExitId",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_InventoryStocks_InventoryExitId",
                table: "InventoryStocks");

            migrationBuilder.DropColumn(
                name: "InventoryExitId",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "InventoryExitId",
                table: "InventoryStocks");
        }
    }
}
