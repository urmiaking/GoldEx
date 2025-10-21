using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoldEx.Server.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLedgerAccountForPayments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "LedgerAccountId",
                table: "InvoicePayments",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PaymentType",
                table: "InvoicePayments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_InvoicePayments_LedgerAccountId",
                table: "InvoicePayments",
                column: "LedgerAccountId");

            migrationBuilder.AddForeignKey(
                name: "FK_InvoicePayments_LedgerAccounts_LedgerAccountId",
                table: "InvoicePayments",
                column: "LedgerAccountId",
                principalTable: "LedgerAccounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InvoicePayments_LedgerAccounts_LedgerAccountId",
                table: "InvoicePayments");

            migrationBuilder.DropIndex(
                name: "IX_InvoicePayments_LedgerAccountId",
                table: "InvoicePayments");

            migrationBuilder.DropColumn(
                name: "LedgerAccountId",
                table: "InvoicePayments");

            migrationBuilder.DropColumn(
                name: "PaymentType",
                table: "InvoicePayments");
        }
    }
}
