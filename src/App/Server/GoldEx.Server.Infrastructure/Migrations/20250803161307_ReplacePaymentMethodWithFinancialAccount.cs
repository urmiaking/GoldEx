using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoldEx.Server.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ReplacePaymentMethodWithFinancialAccount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InvoicePayments_PaymentMethods_PaymentMethodId",
                table: "InvoicePayments");

            migrationBuilder.RenameColumn(
                name: "PaymentMethodId",
                table: "InvoicePayments",
                newName: "SourceFinancialAccountId");

            migrationBuilder.RenameIndex(
                name: "IX_InvoicePayments_PaymentMethodId",
                table: "InvoicePayments",
                newName: "IX_InvoicePayments_SourceFinancialAccountId");

            migrationBuilder.AddForeignKey(
                name: "FK_InvoicePayments_FinancialAccounts_SourceFinancialAccountId",
                table: "InvoicePayments",
                column: "SourceFinancialAccountId",
                principalTable: "FinancialAccounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InvoicePayments_FinancialAccounts_SourceFinancialAccountId",
                table: "InvoicePayments");

            migrationBuilder.RenameColumn(
                name: "SourceFinancialAccountId",
                table: "InvoicePayments",
                newName: "PaymentMethodId");

            migrationBuilder.RenameIndex(
                name: "IX_InvoicePayments_SourceFinancialAccountId",
                table: "InvoicePayments",
                newName: "IX_InvoicePayments_PaymentMethodId");

            migrationBuilder.AddForeignKey(
                name: "FK_InvoicePayments_PaymentMethods_PaymentMethodId",
                table: "InvoicePayments",
                column: "PaymentMethodId",
                principalTable: "PaymentMethods",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
