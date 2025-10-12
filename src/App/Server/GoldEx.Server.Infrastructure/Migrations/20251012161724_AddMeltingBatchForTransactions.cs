using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoldEx.Server.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMeltingBatchForTransactions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "MeltingBatchId",
                table: "Transactions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_MeltingBatchId",
                table: "Transactions",
                column: "MeltingBatchId");

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_MeltingBatches_MeltingBatchId",
                table: "Transactions",
                column: "MeltingBatchId",
                principalTable: "MeltingBatches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_MeltingBatches_MeltingBatchId",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_MeltingBatchId",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "MeltingBatchId",
                table: "Transactions");
        }
    }
}
