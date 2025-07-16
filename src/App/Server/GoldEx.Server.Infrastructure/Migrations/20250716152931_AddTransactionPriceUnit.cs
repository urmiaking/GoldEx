using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoldEx.Server.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTransactionPriceUnit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "PriceUnitId",
                table: "Transactions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_PriceUnitId",
                table: "Transactions",
                column: "PriceUnitId");

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_PriceUnits_PriceUnitId",
                table: "Transactions",
                column: "PriceUnitId",
                principalTable: "PriceUnits",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_PriceUnits_PriceUnitId",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_PriceUnitId",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "PriceUnitId",
                table: "Transactions");
        }
    }
}
