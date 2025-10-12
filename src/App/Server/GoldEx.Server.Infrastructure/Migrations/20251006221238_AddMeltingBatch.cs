using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoldEx.Server.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMeltingBatch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "MeltingBatchId",
                table: "InventoryStocks",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "MeltingBatches",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TotalWeight = table.Column<decimal>(type: "decimal(36,10)", precision: 36, scale: 10, nullable: false),
                    WeightUnitType = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MeltingBatches", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MeltingBatchChangeLogs",
                columns: table => new
                {
                    MeltingBatchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MeltingBatchChangeLogs", x => new { x.MeltingBatchId, x.Id });
                    table.ForeignKey(
                        name: "FK_MeltingBatchChangeLogs_MeltingBatches_MeltingBatchId",
                        column: x => x.MeltingBatchId,
                        principalTable: "MeltingBatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InventoryStocks_MeltingBatchId",
                table: "InventoryStocks",
                column: "MeltingBatchId");

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryStocks_MeltingBatches_MeltingBatchId",
                table: "InventoryStocks",
                column: "MeltingBatchId",
                principalTable: "MeltingBatches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InventoryStocks_MeltingBatches_MeltingBatchId",
                table: "InventoryStocks");

            migrationBuilder.DropTable(
                name: "MeltingBatchChangeLogs");

            migrationBuilder.DropTable(
                name: "MeltingBatches");

            migrationBuilder.DropIndex(
                name: "IX_InventoryStocks_MeltingBatchId",
                table: "InventoryStocks");

            migrationBuilder.DropColumn(
                name: "MeltingBatchId",
                table: "InventoryStocks");
        }
    }
}
