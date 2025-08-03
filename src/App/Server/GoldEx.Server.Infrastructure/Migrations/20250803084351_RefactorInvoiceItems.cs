using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoldEx.Server.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RefactorInvoiceItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InvoiceItems_Products_ProductId",
                table: "InvoiceItems");

            migrationBuilder.DropIndex(
                name: "IX_InvoiceItems_ProductId",
                table: "InvoiceItems");

            migrationBuilder.DropColumn(
                name: "ProductId",
                table: "InvoiceItems");

            migrationBuilder.AddColumn<Guid>(
                name: "PurchaseProductId",
                table: "InvoiceItems",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SellProductId",
                table: "InvoiceItems",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceItems_PurchaseProductId",
                table: "InvoiceItems",
                column: "PurchaseProductId",
                unique: true,
                filter: "[PurchaseProductId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceItems_SellProductId",
                table: "InvoiceItems",
                column: "SellProductId",
                unique: true,
                filter: "[SellProductId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_InvoiceItems_Products_PurchaseProductId",
                table: "InvoiceItems",
                column: "PurchaseProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_InvoiceItems_Products_SellProductId",
                table: "InvoiceItems",
                column: "SellProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InvoiceItems_Products_PurchaseProductId",
                table: "InvoiceItems");

            migrationBuilder.DropForeignKey(
                name: "FK_InvoiceItems_Products_SellProductId",
                table: "InvoiceItems");

            migrationBuilder.DropIndex(
                name: "IX_InvoiceItems_PurchaseProductId",
                table: "InvoiceItems");

            migrationBuilder.DropIndex(
                name: "IX_InvoiceItems_SellProductId",
                table: "InvoiceItems");

            migrationBuilder.DropColumn(
                name: "PurchaseProductId",
                table: "InvoiceItems");

            migrationBuilder.DropColumn(
                name: "SellProductId",
                table: "InvoiceItems");

            migrationBuilder.AddColumn<Guid>(
                name: "ProductId",
                table: "InvoiceItems",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceItems_ProductId",
                table: "InvoiceItems",
                column: "ProductId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_InvoiceItems_Products_ProductId",
                table: "InvoiceItems",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
