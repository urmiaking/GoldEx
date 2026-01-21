using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoldEx.Server.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTargetInvoicePayment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "SourcePaymentId",
                table: "InvoicePayments",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TargetInvoiceId",
                table: "InvoicePayments",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_InvoicePayments_SourcePaymentId",
                table: "InvoicePayments",
                column: "SourcePaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_InvoicePayments_TargetInvoiceId",
                table: "InvoicePayments",
                column: "TargetInvoiceId");

            migrationBuilder.AddForeignKey(
                name: "FK_InvoicePayments_InvoicePayments_SourcePaymentId",
                table: "InvoicePayments",
                column: "SourcePaymentId",
                principalTable: "InvoicePayments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_InvoicePayments_Invoices_TargetInvoiceId",
                table: "InvoicePayments",
                column: "TargetInvoiceId",
                principalTable: "Invoices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InvoicePayments_InvoicePayments_SourcePaymentId",
                table: "InvoicePayments");

            migrationBuilder.DropForeignKey(
                name: "FK_InvoicePayments_Invoices_TargetInvoiceId",
                table: "InvoicePayments");

            migrationBuilder.DropIndex(
                name: "IX_InvoicePayments_SourcePaymentId",
                table: "InvoicePayments");

            migrationBuilder.DropIndex(
                name: "IX_InvoicePayments_TargetInvoiceId",
                table: "InvoicePayments");

            migrationBuilder.DropColumn(
                name: "SourcePaymentId",
                table: "InvoicePayments");

            migrationBuilder.DropColumn(
                name: "TargetInvoiceId",
                table: "InvoicePayments");
        }
    }
}
