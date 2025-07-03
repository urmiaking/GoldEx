using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoldEx.Server.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RenameColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InvoiceDiscounts_PriceUnits_DiscountUnitId",
                table: "InvoiceDiscounts");

            migrationBuilder.DropForeignKey(
                name: "FK_InvoicePayments_PriceUnits_AmountUnitId",
                table: "InvoicePayments");

            migrationBuilder.RenameColumn(
                name: "AmountUnitId",
                table: "InvoicePayments",
                newName: "PriceUnitId");

            migrationBuilder.RenameIndex(
                name: "IX_InvoicePayments_AmountUnitId",
                table: "InvoicePayments",
                newName: "IX_InvoicePayments_PriceUnitId");

            migrationBuilder.RenameColumn(
                name: "DiscountUnitId",
                table: "InvoiceDiscounts",
                newName: "PriceUnitId");

            migrationBuilder.RenameIndex(
                name: "IX_InvoiceDiscounts_DiscountUnitId",
                table: "InvoiceDiscounts",
                newName: "IX_InvoiceDiscounts_PriceUnitId");

            migrationBuilder.AddForeignKey(
                name: "FK_InvoiceDiscounts_PriceUnits_PriceUnitId",
                table: "InvoiceDiscounts",
                column: "PriceUnitId",
                principalTable: "PriceUnits",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_InvoicePayments_PriceUnits_PriceUnitId",
                table: "InvoicePayments",
                column: "PriceUnitId",
                principalTable: "PriceUnits",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InvoiceDiscounts_PriceUnits_PriceUnitId",
                table: "InvoiceDiscounts");

            migrationBuilder.DropForeignKey(
                name: "FK_InvoicePayments_PriceUnits_PriceUnitId",
                table: "InvoicePayments");

            migrationBuilder.RenameColumn(
                name: "PriceUnitId",
                table: "InvoicePayments",
                newName: "AmountUnitId");

            migrationBuilder.RenameIndex(
                name: "IX_InvoicePayments_PriceUnitId",
                table: "InvoicePayments",
                newName: "IX_InvoicePayments_AmountUnitId");

            migrationBuilder.RenameColumn(
                name: "PriceUnitId",
                table: "InvoiceDiscounts",
                newName: "DiscountUnitId");

            migrationBuilder.RenameIndex(
                name: "IX_InvoiceDiscounts_PriceUnitId",
                table: "InvoiceDiscounts",
                newName: "IX_InvoiceDiscounts_DiscountUnitId");

            migrationBuilder.AddForeignKey(
                name: "FK_InvoiceDiscounts_PriceUnits_DiscountUnitId",
                table: "InvoiceDiscounts",
                column: "DiscountUnitId",
                principalTable: "PriceUnits",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_InvoicePayments_PriceUnits_AmountUnitId",
                table: "InvoicePayments",
                column: "AmountUnitId",
                principalTable: "PriceUnits",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
