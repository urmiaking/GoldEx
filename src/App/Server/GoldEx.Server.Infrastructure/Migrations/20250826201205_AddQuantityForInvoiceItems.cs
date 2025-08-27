using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoldEx.Server.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddQuantityForInvoiceItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TotalAmount",
                table: "InvoiceCurrencyItems");

            migrationBuilder.DropColumn(
                name: "TotalAmount",
                table: "InvoiceCoinItems");

            migrationBuilder.AddColumn<int>(
                name: "Quantity",
                table: "InvoiceUsedProducts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Quantity",
                table: "InvoiceProductItems",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Quantity",
                table: "InvoiceUsedProducts");

            migrationBuilder.DropColumn(
                name: "Quantity",
                table: "InvoiceProductItems");

            migrationBuilder.AddColumn<decimal>(
                name: "TotalAmount",
                table: "InvoiceCurrencyItems",
                type: "decimal(36,10)",
                precision: 36,
                scale: 10,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalAmount",
                table: "InvoiceCoinItems",
                type: "decimal(36,10)",
                precision: 36,
                scale: 10,
                nullable: false,
                defaultValue: 0m);
        }
    }
}
