using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoldEx.Server.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCostPrice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "CostPrice",
                table: "InvoiceProductItems",
                type: "decimal(36,10)",
                precision: 36,
                scale: 10,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CostPriceExchangeRate",
                table: "InvoiceProductItems",
                type: "decimal(36,10)",
                precision: 36,
                scale: 10,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CostPriceUnitId",
                table: "InvoiceProductItems",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceProductItems_CostPriceUnitId",
                table: "InvoiceProductItems",
                column: "CostPriceUnitId");

            migrationBuilder.AddForeignKey(
                name: "FK_InvoiceProductItems_PriceUnits_CostPriceUnitId",
                table: "InvoiceProductItems",
                column: "CostPriceUnitId",
                principalTable: "PriceUnits",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InvoiceProductItems_PriceUnits_CostPriceUnitId",
                table: "InvoiceProductItems");

            migrationBuilder.DropIndex(
                name: "IX_InvoiceProductItems_CostPriceUnitId",
                table: "InvoiceProductItems");

            migrationBuilder.DropColumn(
                name: "CostPrice",
                table: "InvoiceProductItems");

            migrationBuilder.DropColumn(
                name: "CostPriceExchangeRate",
                table: "InvoiceProductItems");

            migrationBuilder.DropColumn(
                name: "CostPriceUnitId",
                table: "InvoiceProductItems");
        }
    }
}
