using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoldEx.Server.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class IncreaseExchangeRatePrecision : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "ExchangeRate",
                table: "Transactions",
                type: "decimal(38,18)",
                precision: 38,
                scale: 18,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(36,10)",
                oldPrecision: 36,
                oldScale: 10,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "ExchangeRate",
                table: "PaymentVouchers",
                type: "decimal(38,18)",
                precision: 38,
                scale: 18,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(36,10)",
                oldPrecision: 36,
                oldScale: 10,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "UnpaidAmountExchangeRate",
                table: "Invoices",
                type: "decimal(38,18)",
                precision: 38,
                scale: 18,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(36,10)",
                oldPrecision: 36,
                oldScale: 10,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "ExchangeRate",
                table: "Invoices",
                type: "decimal(38,18)",
                precision: 38,
                scale: 18,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(36,10)",
                oldPrecision: 36,
                oldScale: 10,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "StonePriceUnitExchangeRate",
                table: "InvoiceProductItems",
                type: "decimal(38,18)",
                precision: 38,
                scale: 18,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(36,10)",
                oldPrecision: 36,
                oldScale: 10,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "SaleWagePriceUnitExchangeRate",
                table: "InvoiceProductItems",
                type: "decimal(38,18)",
                precision: 38,
                scale: 18,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(36,10)",
                oldPrecision: 36,
                oldScale: 10,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "PurchaseWagePriceUnitExchangeRate",
                table: "InvoiceProductItems",
                type: "decimal(38,18)",
                precision: 38,
                scale: 18,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(36,10)",
                oldPrecision: 36,
                oldScale: 10,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "CostPriceExchangeRate",
                table: "InvoiceProductItems",
                type: "decimal(38,18)",
                precision: 38,
                scale: 18,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(36,10)",
                oldPrecision: 36,
                oldScale: 10,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "ExchangeRate",
                table: "InvoicePayments",
                type: "decimal(38,18)",
                precision: 38,
                scale: 18,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(36,10)",
                oldPrecision: 36,
                oldScale: 10,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "ExchangeRate",
                table: "InvoiceExtraCosts",
                type: "decimal(38,18)",
                precision: 38,
                scale: 18,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(36,10)",
                oldPrecision: 36,
                oldScale: 10,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "ExchangeRate",
                table: "InvoiceDiscounts",
                type: "decimal(38,18)",
                precision: 38,
                scale: 18,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(36,10)",
                oldPrecision: 36,
                oldScale: 10,
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "ExchangeRate",
                table: "Transactions",
                type: "decimal(36,10)",
                precision: 36,
                scale: 10,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(38,18)",
                oldPrecision: 38,
                oldScale: 18,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "ExchangeRate",
                table: "PaymentVouchers",
                type: "decimal(36,10)",
                precision: 36,
                scale: 10,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(38,18)",
                oldPrecision: 38,
                oldScale: 18,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "UnpaidAmountExchangeRate",
                table: "Invoices",
                type: "decimal(36,10)",
                precision: 36,
                scale: 10,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(38,18)",
                oldPrecision: 38,
                oldScale: 18,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "ExchangeRate",
                table: "Invoices",
                type: "decimal(36,10)",
                precision: 36,
                scale: 10,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(38,18)",
                oldPrecision: 38,
                oldScale: 18,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "StonePriceUnitExchangeRate",
                table: "InvoiceProductItems",
                type: "decimal(36,10)",
                precision: 36,
                scale: 10,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(38,18)",
                oldPrecision: 38,
                oldScale: 18,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "SaleWagePriceUnitExchangeRate",
                table: "InvoiceProductItems",
                type: "decimal(36,10)",
                precision: 36,
                scale: 10,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(38,18)",
                oldPrecision: 38,
                oldScale: 18,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "PurchaseWagePriceUnitExchangeRate",
                table: "InvoiceProductItems",
                type: "decimal(36,10)",
                precision: 36,
                scale: 10,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(38,18)",
                oldPrecision: 38,
                oldScale: 18,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "CostPriceExchangeRate",
                table: "InvoiceProductItems",
                type: "decimal(36,10)",
                precision: 36,
                scale: 10,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(38,18)",
                oldPrecision: 38,
                oldScale: 18,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "ExchangeRate",
                table: "InvoicePayments",
                type: "decimal(36,10)",
                precision: 36,
                scale: 10,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(38,18)",
                oldPrecision: 38,
                oldScale: 18,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "ExchangeRate",
                table: "InvoiceExtraCosts",
                type: "decimal(36,10)",
                precision: 36,
                scale: 10,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(38,18)",
                oldPrecision: 38,
                oldScale: 18,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "ExchangeRate",
                table: "InvoiceDiscounts",
                type: "decimal(36,10)",
                precision: 36,
                scale: 10,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(38,18)",
                oldPrecision: 38,
                oldScale: 18,
                oldNullable: true);
        }
    }
}
