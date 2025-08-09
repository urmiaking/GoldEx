using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoldEx.Server.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RenameInvoiceItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InvoiceItems");

            migrationBuilder.CreateTable(
                name: "InvoiceProductItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProfitPercent = table.Column<decimal>(type: "decimal(9,6)", precision: 9, scale: 6, nullable: false),
                    TaxPercent = table.Column<decimal>(type: "decimal(9,6)", precision: 9, scale: 6, nullable: false),
                    GramPrice = table.Column<decimal>(type: "decimal(36,10)", precision: 36, scale: 10, nullable: false),
                    ExchangeRate = table.Column<decimal>(type: "decimal(36,10)", precision: 36, scale: 10, nullable: true),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    PriceUnitId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SellProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PurchaseProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    InvoiceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ItemRawAmount = table.Column<decimal>(type: "decimal(36,10)", precision: 36, scale: 10, nullable: false),
                    ItemWageAmount = table.Column<decimal>(type: "decimal(36,10)", precision: 36, scale: 10, nullable: false),
                    ItemProfitAmount = table.Column<decimal>(type: "decimal(36,10)", precision: 36, scale: 10, nullable: false),
                    ItemTaxAmount = table.Column<decimal>(type: "decimal(36,10)", precision: 36, scale: 10, nullable: false),
                    ItemFinalAmount = table.Column<decimal>(type: "decimal(36,10)", precision: 36, scale: 10, nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(36,10)", precision: 36, scale: 10, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvoiceProductItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InvoiceProductItems_Invoices_InvoiceId",
                        column: x => x.InvoiceId,
                        principalTable: "Invoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InvoiceProductItems_PriceUnits_PriceUnitId",
                        column: x => x.PriceUnitId,
                        principalTable: "PriceUnits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InvoiceProductItems_Products_PurchaseProductId",
                        column: x => x.PurchaseProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InvoiceProductItems_Products_SellProductId",
                        column: x => x.SellProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceProductItems_InvoiceId",
                table: "InvoiceProductItems",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceProductItems_PriceUnitId",
                table: "InvoiceProductItems",
                column: "PriceUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceProductItems_PurchaseProductId",
                table: "InvoiceProductItems",
                column: "PurchaseProductId",
                unique: true,
                filter: "[PurchaseProductId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceProductItems_SellProductId",
                table: "InvoiceProductItems",
                column: "SellProductId",
                unique: true,
                filter: "[SellProductId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InvoiceProductItems");

            migrationBuilder.CreateTable(
                name: "InvoiceItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InvoiceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PriceUnitId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PurchaseProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SellProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExchangeRate = table.Column<decimal>(type: "decimal(36,10)", precision: 36, scale: 10, nullable: true),
                    GramPrice = table.Column<decimal>(type: "decimal(36,10)", precision: 36, scale: 10, nullable: false),
                    ItemFinalAmount = table.Column<decimal>(type: "decimal(36,10)", precision: 36, scale: 10, nullable: false),
                    ItemProfitAmount = table.Column<decimal>(type: "decimal(36,10)", precision: 36, scale: 10, nullable: false),
                    ItemRawAmount = table.Column<decimal>(type: "decimal(36,10)", precision: 36, scale: 10, nullable: false),
                    ItemTaxAmount = table.Column<decimal>(type: "decimal(36,10)", precision: 36, scale: 10, nullable: false),
                    ItemWageAmount = table.Column<decimal>(type: "decimal(36,10)", precision: 36, scale: 10, nullable: false),
                    ProfitPercent = table.Column<decimal>(type: "decimal(9,6)", precision: 9, scale: 6, nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    TaxPercent = table.Column<decimal>(type: "decimal(9,6)", precision: 9, scale: 6, nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(36,10)", precision: 36, scale: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvoiceItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InvoiceItems_Invoices_InvoiceId",
                        column: x => x.InvoiceId,
                        principalTable: "Invoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InvoiceItems_PriceUnits_PriceUnitId",
                        column: x => x.PriceUnitId,
                        principalTable: "PriceUnits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InvoiceItems_Products_PurchaseProductId",
                        column: x => x.PurchaseProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InvoiceItems_Products_SellProductId",
                        column: x => x.SellProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceItems_InvoiceId",
                table: "InvoiceItems",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceItems_PriceUnitId",
                table: "InvoiceItems",
                column: "PriceUnitId");

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
        }
    }
}
