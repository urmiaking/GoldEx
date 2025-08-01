using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoldEx.Server.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddVoucherForInvoicePayment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "PaymentMethodId",
                table: "InvoicePayments",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<Guid>(
                name: "PaymentVoucherId",
                table: "InvoicePayments",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_InvoicePayments_PaymentVoucherId",
                table: "InvoicePayments",
                column: "PaymentVoucherId");

            migrationBuilder.AddForeignKey(
                name: "FK_InvoicePayments_PaymentVouchers_PaymentVoucherId",
                table: "InvoicePayments",
                column: "PaymentVoucherId",
                principalTable: "PaymentVouchers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InvoicePayments_PaymentVouchers_PaymentVoucherId",
                table: "InvoicePayments");

            migrationBuilder.DropIndex(
                name: "IX_InvoicePayments_PaymentVoucherId",
                table: "InvoicePayments");

            migrationBuilder.DropColumn(
                name: "PaymentVoucherId",
                table: "InvoicePayments");

            migrationBuilder.AlterColumn<Guid>(
                name: "PaymentMethodId",
                table: "InvoicePayments",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);
        }
    }
}
