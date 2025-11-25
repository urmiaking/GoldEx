using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoldEx.Server.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ChangeTransactionPaymentRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_InvoicePayments_InvoicePaymentId",
                table: "Transactions");

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_InvoicePayments_InvoicePaymentId",
                table: "Transactions",
                column: "InvoicePaymentId",
                principalTable: "InvoicePayments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_InvoicePayments_InvoicePaymentId",
                table: "Transactions");

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_InvoicePayments_InvoicePaymentId",
                table: "Transactions",
                column: "InvoicePaymentId",
                principalTable: "InvoicePayments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
