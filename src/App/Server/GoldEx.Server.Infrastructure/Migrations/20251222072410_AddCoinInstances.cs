using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoldEx.Server.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCoinInstances : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InventoryStocks_Coins_CoinId",
                table: "InventoryStocks");

            migrationBuilder.DropForeignKey(
                name: "FK_InvoiceCoinItems_Coins_CoinId",
                table: "InvoiceCoinItems");

            migrationBuilder.DropIndex(
                name: "IX_InvoiceCoinItems_InvoiceId",
                table: "InvoiceCoinItems");

            migrationBuilder.RenameColumn(
                name: "CoinId",
                table: "InvoiceCoinItems",
                newName: "CoinInstanceId");

            migrationBuilder.RenameIndex(
                name: "IX_InvoiceCoinItems_CoinId",
                table: "InvoiceCoinItems",
                newName: "IX_InvoiceCoinItems_CoinInstanceId");

            migrationBuilder.RenameColumn(
                name: "CoinId",
                table: "InventoryStocks",
                newName: "CoinInstanceId");

            migrationBuilder.RenameIndex(
                name: "IX_InventoryStocks_CoinId",
                table: "InventoryStocks",
                newName: "IX_InventoryStocks_CoinInstanceId");

            migrationBuilder.AlterColumn<string>(
                name: "Prefix",
                table: "BarcodeReservations",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(3)",
                oldMaxLength: 3);

            migrationBuilder.AddColumn<int>(
                name: "BarcodeType",
                table: "BarcodeReservations",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "CoinInstances",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Barcode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    MintYear = table.Column<int>(type: "int", nullable: true),
                    Weight = table.Column<decimal>(type: "decimal(36,10)", precision: 36, scale: 10, nullable: false),
                    Fineness = table.Column<decimal>(type: "decimal(9,6)", precision: 9, scale: 6, nullable: false),
                    MintType = table.Column<int>(type: "int", nullable: false),
                    PackageType = table.Column<int>(type: "int", nullable: false),
                    CoinId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoinInstances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CoinInstances_Coins_CoinId",
                        column: x => x.CoinId,
                        principalTable: "Coins",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CoinInstancePackages",
                columns: table => new
                {
                    CoinInstanceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VacuumedWeight = table.Column<decimal>(type: "decimal(36,10)", precision: 36, scale: 10, nullable: false),
                    CardColor = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    StandardCode = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    IssuerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoinInstancePackages", x => x.CoinInstanceId);
                    table.ForeignKey(
                        name: "FK_CoinInstancePackages_CoinInstances_CoinInstanceId",
                        column: x => x.CoinInstanceId,
                        principalTable: "CoinInstances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CoinInstancePackages_Customers_IssuerId",
                        column: x => x.IssuerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceCoinItems_InvoiceId_CoinInstanceId",
                table: "InvoiceCoinItems",
                columns: new[] { "InvoiceId", "CoinInstanceId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CoinInstancePackages_IssuerId",
                table: "CoinInstancePackages",
                column: "IssuerId");

            migrationBuilder.CreateIndex(
                name: "IX_CoinInstances_Barcode",
                table: "CoinInstances",
                column: "Barcode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CoinInstances_CoinId",
                table: "CoinInstances",
                column: "CoinId");

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryStocks_CoinInstances_CoinInstanceId",
                table: "InventoryStocks",
                column: "CoinInstanceId",
                principalTable: "CoinInstances",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_InvoiceCoinItems_CoinInstances_CoinInstanceId",
                table: "InvoiceCoinItems",
                column: "CoinInstanceId",
                principalTable: "CoinInstances",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InventoryStocks_CoinInstances_CoinInstanceId",
                table: "InventoryStocks");

            migrationBuilder.DropForeignKey(
                name: "FK_InvoiceCoinItems_CoinInstances_CoinInstanceId",
                table: "InvoiceCoinItems");

            migrationBuilder.DropTable(
                name: "CoinInstancePackages");

            migrationBuilder.DropTable(
                name: "CoinInstances");

            migrationBuilder.DropIndex(
                name: "IX_InvoiceCoinItems_InvoiceId_CoinInstanceId",
                table: "InvoiceCoinItems");

            migrationBuilder.DropColumn(
                name: "BarcodeType",
                table: "BarcodeReservations");

            migrationBuilder.RenameColumn(
                name: "CoinInstanceId",
                table: "InvoiceCoinItems",
                newName: "CoinId");

            migrationBuilder.RenameIndex(
                name: "IX_InvoiceCoinItems_CoinInstanceId",
                table: "InvoiceCoinItems",
                newName: "IX_InvoiceCoinItems_CoinId");

            migrationBuilder.RenameColumn(
                name: "CoinInstanceId",
                table: "InventoryStocks",
                newName: "CoinId");

            migrationBuilder.RenameIndex(
                name: "IX_InventoryStocks_CoinInstanceId",
                table: "InventoryStocks",
                newName: "IX_InventoryStocks_CoinId");

            migrationBuilder.AlterColumn<string>(
                name: "Prefix",
                table: "BarcodeReservations",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(3)",
                oldMaxLength: 3,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceCoinItems_InvoiceId",
                table: "InvoiceCoinItems",
                column: "InvoiceId");

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryStocks_Coins_CoinId",
                table: "InventoryStocks",
                column: "CoinId",
                principalTable: "Coins",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_InvoiceCoinItems_Coins_CoinId",
                table: "InvoiceCoinItems",
                column: "CoinId",
                principalTable: "Coins",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
