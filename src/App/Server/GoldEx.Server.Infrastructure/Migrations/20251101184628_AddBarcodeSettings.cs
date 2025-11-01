using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoldEx.Server.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBarcodeSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "BarcodeDisplayValue",
                table: "BarcodePositionItems",
                type: "bit",
                nullable: true,
                defaultValue: true);

            migrationBuilder.AddColumn<int>(
                name: "BarcodeFontSize",
                table: "BarcodePositionItems",
                type: "int",
                nullable: true,
                defaultValue: 14);

            migrationBuilder.AddColumn<int>(
                name: "BarcodeHeight",
                table: "BarcodePositionItems",
                type: "int",
                nullable: true,
                defaultValue: 50);

            migrationBuilder.AddColumn<int>(
                name: "BarcodeMargin",
                table: "BarcodePositionItems",
                type: "int",
                nullable: true,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "BarcodeSettings_CreatedAt",
                table: "BarcodePositionItems",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BarcodeWidth",
                table: "BarcodePositionItems",
                type: "int",
                nullable: true,
                defaultValue: 2);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BarcodeDisplayValue",
                table: "BarcodePositionItems");

            migrationBuilder.DropColumn(
                name: "BarcodeFontSize",
                table: "BarcodePositionItems");

            migrationBuilder.DropColumn(
                name: "BarcodeHeight",
                table: "BarcodePositionItems");

            migrationBuilder.DropColumn(
                name: "BarcodeMargin",
                table: "BarcodePositionItems");

            migrationBuilder.DropColumn(
                name: "BarcodeSettings_CreatedAt",
                table: "BarcodePositionItems");

            migrationBuilder.DropColumn(
                name: "BarcodeWidth",
                table: "BarcodePositionItems");
        }
    }
}
