using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoldEx.Server.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPurchaseWage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "PurchaseWage",
                table: "InvoiceProductItems",
                type: "decimal(36,10)",
                precision: 36,
                scale: 10,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PurchaseWagePriceUnitExchangeRate",
                table: "InvoiceProductItems",
                type: "decimal(36,10)",
                precision: 36,
                scale: 10,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PurchaseWagePriceUnitId",
                table: "InvoiceProductItems",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PurchaseWageType",
                table: "InvoiceProductItems",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceProductItems_PurchaseWagePriceUnitId",
                table: "InvoiceProductItems",
                column: "PurchaseWagePriceUnitId");

            migrationBuilder.AddForeignKey(
                name: "FK_InvoiceProductItems_PriceUnits_PurchaseWagePriceUnitId",
                table: "InvoiceProductItems",
                column: "PurchaseWagePriceUnitId",
                principalTable: "PriceUnits",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InvoiceProductItems_PriceUnits_PurchaseWagePriceUnitId",
                table: "InvoiceProductItems");

            migrationBuilder.DropIndex(
                name: "IX_InvoiceProductItems_PurchaseWagePriceUnitId",
                table: "InvoiceProductItems");

            migrationBuilder.DropColumn(
                name: "PurchaseWage",
                table: "InvoiceProductItems");

            migrationBuilder.DropColumn(
                name: "PurchaseWagePriceUnitExchangeRate",
                table: "InvoiceProductItems");

            migrationBuilder.DropColumn(
                name: "PurchaseWagePriceUnitId",
                table: "InvoiceProductItems");

            migrationBuilder.DropColumn(
                name: "PurchaseWageType",
                table: "InvoiceProductItems");
        }
    }
}
