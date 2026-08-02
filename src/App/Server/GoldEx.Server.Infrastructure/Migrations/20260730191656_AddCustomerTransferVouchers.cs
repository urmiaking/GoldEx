using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoldEx.Server.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomerTransferVouchers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CustomerTransferVoucherId",
                table: "Transactions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CustomerTransferVoucherId",
                table: "InvoicePayments",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CustomerTransferVouchers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StoreId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VoucherNumber = table.Column<long>(type: "bigint", nullable: false),
                    TransferDate = table.Column<DateOnly>(type: "date", nullable: false),
                    SourceCustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DestinationCustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PriceUnitId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(36,10)", precision: 36, scale: 10, nullable: false),
                    ExchangeRate = table.Column<decimal>(type: "decimal(38,18)", precision: 38, scale: 18, nullable: true),
                    SourceInvoiceId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DestinationInvoiceId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerTransferVouchers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomerTransferVouchers_Customers_DestinationCustomerId",
                        column: x => x.DestinationCustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CustomerTransferVouchers_Customers_SourceCustomerId",
                        column: x => x.SourceCustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CustomerTransferVouchers_Invoices_DestinationInvoiceId",
                        column: x => x.DestinationInvoiceId,
                        principalTable: "Invoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CustomerTransferVouchers_Invoices_SourceInvoiceId",
                        column: x => x.SourceInvoiceId,
                        principalTable: "Invoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CustomerTransferVouchers_PriceUnits_PriceUnitId",
                        column: x => x.PriceUnitId,
                        principalTable: "PriceUnits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_CustomerTransferVoucherId",
                table: "Transactions",
                column: "CustomerTransferVoucherId");

            migrationBuilder.CreateIndex(
                name: "IX_InvoicePayments_CustomerTransferVoucherId",
                table: "InvoicePayments",
                column: "CustomerTransferVoucherId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerTransferVouchers_DestinationCustomerId",
                table: "CustomerTransferVouchers",
                column: "DestinationCustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerTransferVouchers_DestinationInvoiceId",
                table: "CustomerTransferVouchers",
                column: "DestinationInvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerTransferVouchers_PriceUnitId",
                table: "CustomerTransferVouchers",
                column: "PriceUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerTransferVouchers_SourceCustomerId",
                table: "CustomerTransferVouchers",
                column: "SourceCustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerTransferVouchers_SourceInvoiceId",
                table: "CustomerTransferVouchers",
                column: "SourceInvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerTransferVouchers_StoreId_VoucherNumber",
                table: "CustomerTransferVouchers",
                columns: new[] { "StoreId", "VoucherNumber" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_InvoicePayments_CustomerTransferVouchers_CustomerTransferVoucherId",
                table: "InvoicePayments",
                column: "CustomerTransferVoucherId",
                principalTable: "CustomerTransferVouchers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_CustomerTransferVouchers_CustomerTransferVoucherId",
                table: "Transactions",
                column: "CustomerTransferVoucherId",
                principalTable: "CustomerTransferVouchers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InvoicePayments_CustomerTransferVouchers_CustomerTransferVoucherId",
                table: "InvoicePayments");

            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_CustomerTransferVouchers_CustomerTransferVoucherId",
                table: "Transactions");

            migrationBuilder.DropTable(
                name: "CustomerTransferVouchers");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_CustomerTransferVoucherId",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_InvoicePayments_CustomerTransferVoucherId",
                table: "InvoicePayments");

            migrationBuilder.DropColumn(
                name: "CustomerTransferVoucherId",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "CustomerTransferVoucherId",
                table: "InvoicePayments");
        }
    }
}
