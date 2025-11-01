using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoldEx.Server.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddedBarcodeEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BarcodePrintSettings",
                columns: table => new
                {
                    SettingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LabelWidth = table.Column<int>(type: "int", nullable: false, defaultValue: 300),
                    LabelHeight = table.Column<int>(type: "int", nullable: false, defaultValue: 150),
                    BarcodeMarginTop = table.Column<int>(type: "int", nullable: false, defaultValue: 5),
                    BarcodeMarginRight = table.Column<int>(type: "int", nullable: false, defaultValue: 5),
                    BarcodeMarginBottom = table.Column<int>(type: "int", nullable: false, defaultValue: 5),
                    BarcodeMarginLeft = table.Column<int>(type: "int", nullable: false, defaultValue: 5),
                    Margin_CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    BarcodePaddingTop = table.Column<int>(type: "int", nullable: false, defaultValue: 10),
                    BarcodePaddingRight = table.Column<int>(type: "int", nullable: false, defaultValue: 10),
                    BarcodePaddingBottom = table.Column<int>(type: "int", nullable: false, defaultValue: 10),
                    BarcodePaddingLeft = table.Column<int>(type: "int", nullable: false, defaultValue: 10),
                    Padding_CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BarcodePrintSettings", x => x.SettingId);
                    table.ForeignKey(
                        name: "FK_BarcodePrintSettings_Settings_SettingId",
                        column: x => x.SettingId,
                        principalTable: "Settings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BarcodePositionItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Position = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ItemType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    IsVisible = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    FontSize = table.Column<int>(type: "int", nullable: false, defaultValue: 12),
                    ItemSpacing = table.Column<int>(type: "int", nullable: false, defaultValue: 5),
                    BarcodePrintSettingsId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BarcodePositionItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BarcodePositionItems_BarcodePrintSettings_BarcodePrintSettingsId",
                        column: x => x.BarcodePrintSettingsId,
                        principalTable: "BarcodePrintSettings",
                        principalColumn: "SettingId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BarcodePositionItems_BarcodePrintSettingsId",
                table: "BarcodePositionItems",
                column: "BarcodePrintSettingsId");

            migrationBuilder.CreateIndex(
                name: "IX_BarcodePositionItems_Position_ItemType",
                table: "BarcodePositionItems",
                columns: new[] { "Position", "ItemType" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BarcodePositionItems");

            migrationBuilder.DropTable(
                name: "BarcodePrintSettings");
        }
    }
}
