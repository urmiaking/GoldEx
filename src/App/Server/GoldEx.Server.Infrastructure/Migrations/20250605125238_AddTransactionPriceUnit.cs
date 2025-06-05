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
            migrationBuilder.DropColumn(
                name: "CreditUnit",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "DebitUnit",
                table: "Transactions");

            migrationBuilder.AddColumn<Guid>(
                name: "CreditUnitId",
                table: "Transactions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DebitUnitId",
                table: "Transactions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_CreditUnitId",
                table: "Transactions",
                column: "CreditUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_DebitUnitId",
                table: "Transactions",
                column: "DebitUnitId");

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_PriceUnits_CreditUnitId",
                table: "Transactions",
                column: "CreditUnitId",
                principalTable: "PriceUnits",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_PriceUnits_DebitUnitId",
                table: "Transactions",
                column: "DebitUnitId",
                principalTable: "PriceUnits",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_PriceUnits_CreditUnitId",
                table: "Transactions");

            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_PriceUnits_DebitUnitId",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_CreditUnitId",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_DebitUnitId",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "CreditUnitId",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "DebitUnitId",
                table: "Transactions");

            migrationBuilder.AddColumn<int>(
                name: "CreditUnit",
                table: "Transactions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DebitUnit",
                table: "Transactions",
                type: "int",
                nullable: true);
        }
    }
}
