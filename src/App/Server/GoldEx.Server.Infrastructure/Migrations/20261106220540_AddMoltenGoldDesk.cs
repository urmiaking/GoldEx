using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoldEx.Server.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMoltenGoldDesk : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MoltenGoldTrades",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StoreId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Side = table.Column<int>(type: "int", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PriceUnitId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExchangeRate = table.Column<decimal>(type: "decimal(38,18)", precision: 38, scale: 18, nullable: false),
                    Weight = table.Column<decimal>(type: "decimal(36,10)", precision: 36, scale: 10, nullable: false),
                    RemainingWeight = table.Column<decimal>(type: "decimal(36,10)", precision: 36, scale: 10, nullable: false),
                    Fineness = table.Column<decimal>(type: "decimal(9,6)", precision: 9, scale: 6, nullable: false),
                    RateBasis = table.Column<int>(type: "int", nullable: false),
                    Rate = table.Column<decimal>(type: "decimal(36,10)", precision: 36, scale: 10, nullable: false),
                    GramRate = table.Column<decimal>(type: "decimal(38,18)", precision: 38, scale: 18, nullable: false),
                    CommissionPercent = table.Column<decimal>(type: "decimal(9,6)", precision: 9, scale: 6, nullable: false),
                    AssayNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsCancelled = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MoltenGoldTrades", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MoltenGoldTrades_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MoltenGoldTrades_PriceUnits_PriceUnitId",
                        column: x => x.PriceUnitId,
                        principalTable: "PriceUnits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MoltenGoldTrades_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MoltenGoldTradeEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StoreId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TradeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Kind = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(36,10)", precision: 36, scale: 10, nullable: false),
                    Weight = table.Column<decimal>(type: "decimal(36,10)", precision: 36, scale: 10, nullable: false),
                    InvoiceId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    VoucherId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Reference = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    JournalGroupId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MoltenGoldTradeEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MoltenGoldTradeEvents_CustomerTransferVouchers_VoucherId",
                        column: x => x.VoucherId,
                        principalTable: "CustomerTransferVouchers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MoltenGoldTradeEvents_Invoices_InvoiceId",
                        column: x => x.InvoiceId,
                        principalTable: "Invoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MoltenGoldTradeEvents_MoltenGoldTrades_TradeId",
                        column: x => x.TradeId,
                        principalTable: "MoltenGoldTrades",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MoltenGoldTradeEvents_InvoiceId",
                table: "MoltenGoldTradeEvents",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_MoltenGoldTradeEvents_StoreId_TradeId_CreatedAt",
                table: "MoltenGoldTradeEvents",
                columns: new[] { "StoreId", "TradeId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_MoltenGoldTradeEvents_TradeId",
                table: "MoltenGoldTradeEvents",
                column: "TradeId");

            migrationBuilder.CreateIndex(
                name: "IX_MoltenGoldTradeEvents_VoucherId",
                table: "MoltenGoldTradeEvents",
                column: "VoucherId");

            migrationBuilder.CreateIndex(
                name: "IX_MoltenGoldTrades_CustomerId",
                table: "MoltenGoldTrades",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_MoltenGoldTrades_PriceUnitId",
                table: "MoltenGoldTrades",
                column: "PriceUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_MoltenGoldTrades_ProductId",
                table: "MoltenGoldTrades",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_MoltenGoldTrades_StoreId_CreatedAt",
                table: "MoltenGoldTrades",
                columns: new[] { "StoreId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_MoltenGoldTrades_StoreId_ProductId_IsCancelled",
                table: "MoltenGoldTrades",
                columns: new[] { "StoreId", "ProductId", "IsCancelled" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MoltenGoldTradeEvents");

            migrationBuilder.DropTable(
                name: "MoltenGoldTrades");
        }
    }
}
