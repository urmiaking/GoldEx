using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoldEx.Server.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSmartTrayAggregate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SmartTrays",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StoreId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TrayNumber = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ClerkId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ClerkName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ClosedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DiscrepancyNotes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    DiscrepancyApprovedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SmartTrays", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SmartTrayItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StoreId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SmartTrayId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ItemType = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CoinInstanceId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Barcode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Weight = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Fineness = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Wage = table.Column<decimal>(type: "decimal(36,10)", precision: 36, scale: 10, nullable: true),
                    WageType = table.Column<int>(type: "int", nullable: true),
                    CategoryTitle = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    ImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    AddedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReturnedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SoldAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    InvoiceId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SmartTrayItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SmartTrayItems_SmartTrays_SmartTrayId",
                        column: x => x.SmartTrayId,
                        principalTable: "SmartTrays",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SmartTrayItems_SmartTrayId_Barcode",
                table: "SmartTrayItems",
                columns: new[] { "SmartTrayId", "Barcode" });

            migrationBuilder.CreateIndex(
                name: "IX_SmartTrayItems_StoreId_Barcode",
                table: "SmartTrayItems",
                columns: new[] { "StoreId", "Barcode" });

            migrationBuilder.CreateIndex(
                name: "IX_SmartTrays_StoreId_Status",
                table: "SmartTrays",
                columns: new[] { "StoreId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_SmartTrays_StoreId_TrayNumber",
                table: "SmartTrays",
                columns: new[] { "StoreId", "TrayNumber" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SmartTrayItems");

            migrationBuilder.DropTable(
                name: "SmartTrays");
        }
    }
}
