using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoldEx.Server.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddInventoryStock : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InventoryStocks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CoinId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CurrencyId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ChangeAmount = table.Column<decimal>(type: "decimal(36,10)", precision: 36, scale: 10, nullable: false),
                    ActionType = table.Column<int>(type: "int", nullable: false),
                    InvoiceId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryStocks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InventoryStocks_Coins_CoinId",
                        column: x => x.CoinId,
                        principalTable: "Coins",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InventoryStocks_Invoices_InvoiceId",
                        column: x => x.InvoiceId,
                        principalTable: "Invoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InventoryStocks_PriceUnits_CurrencyId",
                        column: x => x.CurrencyId,
                        principalTable: "PriceUnits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InventoryStocks_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InventoryStocks_ActionType",
                table: "InventoryStocks",
                column: "ActionType");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryStocks_ChangeAmount",
                table: "InventoryStocks",
                column: "ChangeAmount");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryStocks_CoinId",
                table: "InventoryStocks",
                column: "CoinId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryStocks_CurrencyId",
                table: "InventoryStocks",
                column: "CurrencyId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryStocks_InvoiceId",
                table: "InventoryStocks",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryStocks_ProductId",
                table: "InventoryStocks",
                column: "ProductId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InventoryStocks");
        }
    }
}
