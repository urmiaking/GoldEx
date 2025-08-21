using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoldEx.Server.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTradeIns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InvoiceTradeIns",
                columns: table => new
                {
                    InvoiceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Description = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Fineness = table.Column<int>(type: "int", nullable: false),
                    Weight = table.Column<decimal>(type: "decimal(36,10)", precision: 36, scale: 10, nullable: false),
                    GramPrice = table.Column<decimal>(type: "decimal(36,10)", precision: 36, scale: 10, nullable: false),
                    IsSellable = table.Column<bool>(type: "bit", nullable: false),
                    ResultingProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ItemFinalAmount = table.Column<decimal>(type: "decimal(36,10)", precision: 36, scale: 10, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InvoiceTradeIns");
        }
    }
}
