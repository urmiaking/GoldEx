using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoldEx.Server.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddInvoiceExchangeRates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "ExchangeRate",
                table: "InvoicePayments",
                type: "decimal(36,10)",
                precision: 36,
                scale: 10,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ExchangeRate",
                table: "InvoiceExtraCosts",
                type: "decimal(36,10)",
                precision: 36,
                scale: 10,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ExchangeRate",
                table: "InvoiceDiscounts",
                type: "decimal(36,10)",
                precision: 36,
                scale: 10,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExchangeRate",
                table: "InvoicePayments");

            migrationBuilder.DropColumn(
                name: "ExchangeRate",
                table: "InvoiceExtraCosts");

            migrationBuilder.DropColumn(
                name: "ExchangeRate",
                table: "InvoiceDiscounts");
        }
    }
}
