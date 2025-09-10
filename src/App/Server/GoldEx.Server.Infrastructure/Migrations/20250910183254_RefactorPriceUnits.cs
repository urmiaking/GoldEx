using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoldEx.Server.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RefactorPriceUnits : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PriceUnits_LedgerAccounts_LedgerAccountId",
                table: "PriceUnits");

            migrationBuilder.DropIndex(
                name: "IX_PriceUnits_LedgerAccountId",
                table: "PriceUnits");

            migrationBuilder.DropIndex(
                name: "IX_LedgerAccounts_CustomerId",
                table: "LedgerAccounts");

            migrationBuilder.DropColumn(
                name: "LedgerAccountId",
                table: "PriceUnits");

            migrationBuilder.DropColumn(
                name: "UnitType",
                table: "Prices");

            migrationBuilder.AddColumn<int>(
                name: "UnitType",
                table: "PriceUnits",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PriceUnitId",
                table: "LedgerAccounts",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_LedgerAccounts_CustomerId_ParentAccountId_PriceUnitId",
                table: "LedgerAccounts",
                columns: new[] { "CustomerId", "ParentAccountId", "PriceUnitId" },
                unique: true,
                filter: "[CustomerId] IS NOT NULL AND [ParentAccountId] IS NOT NULL AND [PriceUnitId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_LedgerAccounts_PriceUnitId",
                table: "LedgerAccounts",
                column: "PriceUnitId");

            migrationBuilder.AddForeignKey(
                name: "FK_LedgerAccounts_PriceUnits_PriceUnitId",
                table: "LedgerAccounts",
                column: "PriceUnitId",
                principalTable: "PriceUnits",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LedgerAccounts_PriceUnits_PriceUnitId",
                table: "LedgerAccounts");

            migrationBuilder.DropIndex(
                name: "IX_LedgerAccounts_CustomerId_ParentAccountId_PriceUnitId",
                table: "LedgerAccounts");

            migrationBuilder.DropIndex(
                name: "IX_LedgerAccounts_PriceUnitId",
                table: "LedgerAccounts");

            migrationBuilder.DropColumn(
                name: "UnitType",
                table: "PriceUnits");

            migrationBuilder.DropColumn(
                name: "PriceUnitId",
                table: "LedgerAccounts");

            migrationBuilder.AddColumn<Guid>(
                name: "LedgerAccountId",
                table: "PriceUnits",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<int>(
                name: "UnitType",
                table: "Prices",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PriceUnits_LedgerAccountId",
                table: "PriceUnits",
                column: "LedgerAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_LedgerAccounts_CustomerId",
                table: "LedgerAccounts",
                column: "CustomerId");

            migrationBuilder.AddForeignKey(
                name: "FK_PriceUnits_LedgerAccounts_LedgerAccountId",
                table: "PriceUnits",
                column: "LedgerAccountId",
                principalTable: "LedgerAccounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
