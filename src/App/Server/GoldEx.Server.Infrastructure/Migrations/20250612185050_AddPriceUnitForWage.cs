using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoldEx.Server.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPriceUnitForWage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "WagePriceUnitId",
                table: "Products",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Products_WagePriceUnitId",
                table: "Products",
                column: "WagePriceUnitId");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_PriceUnits_WagePriceUnitId",
                table: "Products",
                column: "WagePriceUnitId",
                principalTable: "PriceUnits",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_PriceUnits_WagePriceUnitId",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_WagePriceUnitId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "WagePriceUnitId",
                table: "Products");
        }
    }
}
