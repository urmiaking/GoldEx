using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoldEx.Server.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddedLedgerAccounts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "LedgerAccountId",
                table: "Coins",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Coins_LedgerAccountId",
                table: "Coins",
                column: "LedgerAccountId");

            migrationBuilder.AddForeignKey(
                name: "FK_Coins_LedgerAccounts_LedgerAccountId",
                table: "Coins",
                column: "LedgerAccountId",
                principalTable: "LedgerAccounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Coins_LedgerAccounts_LedgerAccountId",
                table: "Coins");

            migrationBuilder.DropIndex(
                name: "IX_Coins_LedgerAccountId",
                table: "Coins");

            migrationBuilder.DropColumn(
                name: "LedgerAccountId",
                table: "Coins");
        }
    }
}
