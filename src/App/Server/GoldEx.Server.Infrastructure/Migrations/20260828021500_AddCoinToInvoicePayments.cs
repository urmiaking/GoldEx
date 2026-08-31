using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoldEx.Server.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCoinToInvoicePayments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CoinInstanceId",
                table: "InvoicePayments",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CoinQuantity",
                table: "InvoicePayments",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CoinUnitPrice",
                table: "InvoicePayments",
                type: "decimal(36,10)",
                precision: 36,
                scale: 10,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_InvoicePayments_CoinInstanceId",
                table: "InvoicePayments",
                column: "CoinInstanceId");

            migrationBuilder.AddForeignKey(
                name: "FK_InvoicePayments_CoinInstances_CoinInstanceId",
                table: "InvoicePayments",
                column: "CoinInstanceId",
                principalTable: "CoinInstances",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InvoicePayments_CoinInstances_CoinInstanceId",
                table: "InvoicePayments");

            migrationBuilder.DropIndex(
                name: "IX_InvoicePayments_CoinInstanceId",
                table: "InvoicePayments");

            migrationBuilder.DropColumn(
                name: "CoinInstanceId",
                table: "InvoicePayments");

            migrationBuilder.DropColumn(
                name: "CoinQuantity",
                table: "InvoicePayments");

            migrationBuilder.DropColumn(
                name: "CoinUnitPrice",
                table: "InvoicePayments");
        }
    }
}
