using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoldEx.Server.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUsedProducts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InvoiceTradeIns");

            migrationBuilder.DropColumn(
                name: "CaratType",
                table: "Products");

            migrationBuilder.RenameColumn(
                name: "OldGoldCarat",
                table: "Settings",
                newName: "UsedGoldFineness");

            migrationBuilder.AddColumn<decimal>(
                name: "Fineness",
                table: "Products",
                type: "decimal(9,6)",
                precision: 9,
                scale: 6,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "InvoiceUsedProducts",
                columns: table => new
                {
                    InvoiceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Description = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Fineness = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Weight = table.Column<decimal>(type: "decimal(36,10)", precision: 36, scale: 10, nullable: false),
                    GramPrice = table.Column<decimal>(type: "decimal(36,10)", precision: 36, scale: 10, nullable: false),
                    IsSellable = table.Column<bool>(type: "bit", nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ItemAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ExtraCostsAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ItemFinalAmount = table.Column<decimal>(type: "decimal(36,10)", precision: 36, scale: 10, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvoiceUsedProducts", x => new { x.InvoiceId, x.Id });
                    table.ForeignKey(
                        name: "FK_InvoiceUsedProducts_Invoices_InvoiceId",
                        column: x => x.InvoiceId,
                        principalTable: "Invoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InvoiceUsedProducts_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceUsedProducts_ProductId",
                table: "InvoiceUsedProducts",
                column: "ProductId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InvoiceUsedProducts");

            migrationBuilder.DropColumn(
                name: "Fineness",
                table: "Products");

            migrationBuilder.RenameColumn(
                name: "UsedGoldFineness",
                table: "Settings",
                newName: "OldGoldCarat");

            migrationBuilder.AddColumn<int>(
                name: "CaratType",
                table: "Products",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "InvoiceTradeIns",
                columns: table => new
                {
                    InvoiceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ResultingProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Fineness = table.Column<int>(type: "int", nullable: false),
                    GramPrice = table.Column<decimal>(type: "decimal(36,10)", precision: 36, scale: 10, nullable: false),
                    IsSellable = table.Column<bool>(type: "bit", nullable: false),
                    ItemFinalAmount = table.Column<decimal>(type: "decimal(36,10)", precision: 36, scale: 10, nullable: false),
                    Weight = table.Column<decimal>(type: "decimal(36,10)", precision: 36, scale: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvoiceTradeIns", x => new { x.InvoiceId, x.Id });
                    table.ForeignKey(
                        name: "FK_InvoiceTradeIns_Invoices_InvoiceId",
                        column: x => x.InvoiceId,
                        principalTable: "Invoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InvoiceTradeIns_Products_ResultingProductId",
                        column: x => x.ResultingProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceTradeIns_ResultingProductId",
                table: "InvoiceTradeIns",
                column: "ResultingProductId");
        }
    }
}
