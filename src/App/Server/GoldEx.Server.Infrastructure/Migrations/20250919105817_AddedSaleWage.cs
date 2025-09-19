using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoldEx.Server.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddedSaleWage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "SaleWage",
                table: "InvoiceProductItems",
                type: "decimal(36,10)",
                precision: 36,
                scale: 10,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "SaleWagePriceUnitExchangeRate",
                table: "InvoiceProductItems",
                type: "decimal(36,10)",
                precision: 36,
                scale: 10,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SaleWagePriceUnitId",
                table: "InvoiceProductItems",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SaleWageType",
                table: "InvoiceProductItems",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceProductItems_SaleWagePriceUnitId",
                table: "InvoiceProductItems",
                column: "SaleWagePriceUnitId");

            migrationBuilder.AddForeignKey(
                name: "FK_InvoiceProductItems_PriceUnits_SaleWagePriceUnitId",
                table: "InvoiceProductItems",
                column: "SaleWagePriceUnitId",
                principalTable: "PriceUnits",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InvoiceProductItems_PriceUnits_SaleWagePriceUnitId",
                table: "InvoiceProductItems");

            migrationBuilder.DropIndex(
                name: "IX_InvoiceProductItems_SaleWagePriceUnitId",
                table: "InvoiceProductItems");

            migrationBuilder.DropColumn(
                name: "SaleWage",
                table: "InvoiceProductItems");

            migrationBuilder.DropColumn(
                name: "SaleWagePriceUnitExchangeRate",
                table: "InvoiceProductItems");

            migrationBuilder.DropColumn(
                name: "SaleWagePriceUnitId",
                table: "InvoiceProductItems");

            migrationBuilder.DropColumn(
                name: "SaleWageType",
                table: "InvoiceProductItems");
        }
    }
}
