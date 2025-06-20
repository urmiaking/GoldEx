using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoldEx.Server.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddInvoicePriceUnit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "PriceUnitId",
                table: "Invoices",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_PriceUnitId",
                table: "Invoices",
                column: "PriceUnitId");

            migrationBuilder.AddForeignKey(
                name: "FK_Invoices_PriceUnits_PriceUnitId",
                table: "Invoices",
                column: "PriceUnitId",
                principalTable: "PriceUnits",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Invoices_PriceUnits_PriceUnitId",
                table: "Invoices");

            migrationBuilder.DropIndex(
                name: "IX_Invoices_PriceUnitId",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "PriceUnitId",
                table: "Invoices");
        }
    }
}
