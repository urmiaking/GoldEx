using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoldEx.Server.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFinancialAccountForCurrencyItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "FinancialAccountId",
                table: "InvoiceCurrencyItems",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceCurrencyItems_FinancialAccountId",
                table: "InvoiceCurrencyItems",
                column: "FinancialAccountId");

            migrationBuilder.AddForeignKey(
                name: "FK_InvoiceCurrencyItems_FinancialAccounts_FinancialAccountId",
                table: "InvoiceCurrencyItems",
                column: "FinancialAccountId",
                principalTable: "FinancialAccounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InvoiceCurrencyItems_FinancialAccounts_FinancialAccountId",
                table: "InvoiceCurrencyItems");

            migrationBuilder.DropIndex(
                name: "IX_InvoiceCurrencyItems_FinancialAccountId",
                table: "InvoiceCurrencyItems");

            migrationBuilder.DropColumn(
                name: "FinancialAccountId",
                table: "InvoiceCurrencyItems");
        }
    }
}
