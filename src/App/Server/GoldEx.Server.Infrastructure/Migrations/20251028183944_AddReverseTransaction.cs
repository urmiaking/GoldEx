using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoldEx.Server.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddReverseTransaction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ReverseTransactionId",
                table: "Transactions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_ReverseTransactionId",
                table: "Transactions",
                column: "ReverseTransactionId",
                unique: true,
                filter: "[ReverseTransactionId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_Transactions_ReverseTransactionId",
                table: "Transactions",
                column: "ReverseTransactionId",
                principalTable: "Transactions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_Transactions_ReverseTransactionId",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_ReverseTransactionId",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "ReverseTransactionId",
                table: "Transactions");
        }
    }
}
