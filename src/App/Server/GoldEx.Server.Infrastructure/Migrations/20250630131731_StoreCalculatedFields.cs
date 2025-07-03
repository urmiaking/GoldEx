using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoldEx.Server.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class StoreCalculatedFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "ItemFinalAmount",
                table: "InvoiceItems",
                type: "decimal(36,10)",
                precision: 36,
                scale: 10,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ItemProfitAmount",
                table: "InvoiceItems",
                type: "decimal(36,10)",
                precision: 36,
                scale: 10,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ItemRawAmount",
                table: "InvoiceItems",
                type: "decimal(36,10)",
                precision: 36,
                scale: 10,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ItemTaxAmount",
                table: "InvoiceItems",
                type: "decimal(36,10)",
                precision: 36,
                scale: 10,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ItemWageAmount",
                table: "InvoiceItems",
                type: "decimal(36,10)",
                precision: 36,
                scale: 10,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalAmount",
                table: "InvoiceItems",
                type: "decimal(36,10)",
                precision: 36,
                scale: 10,
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ItemFinalAmount",
                table: "InvoiceItems");

            migrationBuilder.DropColumn(
                name: "ItemProfitAmount",
                table: "InvoiceItems");

            migrationBuilder.DropColumn(
                name: "ItemRawAmount",
                table: "InvoiceItems");

            migrationBuilder.DropColumn(
                name: "ItemTaxAmount",
                table: "InvoiceItems");

            migrationBuilder.DropColumn(
                name: "ItemWageAmount",
                table: "InvoiceItems");

            migrationBuilder.DropColumn(
                name: "TotalAmount",
                table: "InvoiceItems");
        }
    }
}
