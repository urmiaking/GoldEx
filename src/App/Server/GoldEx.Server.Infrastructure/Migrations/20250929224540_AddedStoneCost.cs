using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoldEx.Server.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddedStoneCost : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "StonePriceUnitId",
                table: "Products",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "StonePriceUnitExchangeRate",
                table: "InvoiceProductItems",
                type: "decimal(36,10)",
                precision: 36,
                scale: 10,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Cost",
                table: "GemStones",
                type: "decimal(36,10)",
                precision: 36,
                scale: 10,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateIndex(
                name: "IX_Products_StonePriceUnitId",
                table: "Products",
                column: "StonePriceUnitId");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_PriceUnits_StonePriceUnitId",
                table: "Products",
                column: "StonePriceUnitId",
                principalTable: "PriceUnits",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_PriceUnits_StonePriceUnitId",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_StonePriceUnitId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "StonePriceUnitId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "StonePriceUnitExchangeRate",
                table: "InvoiceProductItems");

            migrationBuilder.DropColumn(
                name: "Cost",
                table: "GemStones");
        }
    }
}
