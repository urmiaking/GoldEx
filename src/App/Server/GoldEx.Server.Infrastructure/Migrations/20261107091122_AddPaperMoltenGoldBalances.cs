using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoldEx.Server.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPaperMoltenGoldBalances : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "MoltenGoldTradeId",
                table: "Transactions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "GoldPriceUnitId",
                table: "MoltenGoldTrades",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsPaper",
                table: "MoltenGoldTrades",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "RemainingGoldBookAmount",
                table: "MoltenGoldTrades",
                type: "decimal(36,10)",
                precision: 36,
                scale: 10,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "RemainingGoldWeight",
                table: "MoltenGoldTrades",
                type: "decimal(36,10)",
                precision: 36,
                scale: 10,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_MoltenGoldTradeId",
                table: "Transactions",
                column: "MoltenGoldTradeId");

            migrationBuilder.CreateIndex(
                name: "IX_MoltenGoldTrades_GoldPriceUnitId",
                table: "MoltenGoldTrades",
                column: "GoldPriceUnitId");

            migrationBuilder.AddForeignKey(
                name: "FK_MoltenGoldTrades_PriceUnits_GoldPriceUnitId",
                table: "MoltenGoldTrades",
                column: "GoldPriceUnitId",
                principalTable: "PriceUnits",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_MoltenGoldTrades_MoltenGoldTradeId",
                table: "Transactions",
                column: "MoltenGoldTradeId",
                principalTable: "MoltenGoldTrades",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MoltenGoldTrades_PriceUnits_GoldPriceUnitId",
                table: "MoltenGoldTrades");

            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_MoltenGoldTrades_MoltenGoldTradeId",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_MoltenGoldTradeId",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_MoltenGoldTrades_GoldPriceUnitId",
                table: "MoltenGoldTrades");

            migrationBuilder.DropColumn(
                name: "MoltenGoldTradeId",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "GoldPriceUnitId",
                table: "MoltenGoldTrades");

            migrationBuilder.DropColumn(
                name: "IsPaper",
                table: "MoltenGoldTrades");

            migrationBuilder.DropColumn(
                name: "RemainingGoldBookAmount",
                table: "MoltenGoldTrades");

            migrationBuilder.DropColumn(
                name: "RemainingGoldWeight",
                table: "MoltenGoldTrades");
        }
    }
}
