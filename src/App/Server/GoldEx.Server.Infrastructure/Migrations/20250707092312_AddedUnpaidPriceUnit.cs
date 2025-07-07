using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoldEx.Server.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddedUnpaidPriceUnit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "UnpaidAmountExchangeRate",
                table: "Invoices",
                type: "decimal(36,10)",
                precision: 36,
                scale: 10,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UnpaidPriceUnitId",
                table: "Invoices",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_UnpaidPriceUnitId",
                table: "Invoices",
                column: "UnpaidPriceUnitId");

            migrationBuilder.AddForeignKey(
                name: "FK_Invoices_PriceUnits_UnpaidPriceUnitId",
                table: "Invoices",
                column: "UnpaidPriceUnitId",
                principalTable: "PriceUnits",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Invoices_PriceUnits_UnpaidPriceUnitId",
                table: "Invoices");

            migrationBuilder.DropIndex(
                name: "IX_Invoices_UnpaidPriceUnitId",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "UnpaidAmountExchangeRate",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "UnpaidPriceUnitId",
                table: "Invoices");
        }
    }
}
