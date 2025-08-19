using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoldEx.Server.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPriceUnitLedgerAccounts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "LedgerAccountId",
                table: "PriceUnits",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_PriceUnits_LedgerAccountId",
                table: "PriceUnits",
                column: "LedgerAccountId");

            migrationBuilder.AddForeignKey(
                name: "FK_PriceUnits_LedgerAccounts_LedgerAccountId",
                table: "PriceUnits",
                column: "LedgerAccountId",
                principalTable: "LedgerAccounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PriceUnits_LedgerAccounts_LedgerAccountId",
                table: "PriceUnits");

            migrationBuilder.DropIndex(
                name: "IX_PriceUnits_LedgerAccountId",
                table: "PriceUnits");

            migrationBuilder.DropColumn(
                name: "LedgerAccountId",
                table: "PriceUnits");
        }
    }
}
