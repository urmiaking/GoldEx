using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoldEx.Server.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ModifyVoucherRelationShip : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_InvoicePayments_PaymentVoucherId",
                table: "InvoicePayments");

            migrationBuilder.CreateIndex(
                name: "IX_InvoicePayments_PaymentVoucherId",
                table: "InvoicePayments",
                column: "PaymentVoucherId",
                unique: true,
                filter: "[PaymentVoucherId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_InvoicePayments_PaymentVoucherId",
                table: "InvoicePayments");

            migrationBuilder.CreateIndex(
                name: "IX_InvoicePayments_PaymentVoucherId",
                table: "InvoicePayments",
                column: "PaymentVoucherId");
        }
    }
}
