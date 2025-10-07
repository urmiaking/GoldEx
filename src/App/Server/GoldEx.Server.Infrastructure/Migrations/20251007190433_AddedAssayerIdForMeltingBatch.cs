using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoldEx.Server.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddedAssayerIdForMeltingBatch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "AssayerId",
                table: "MeltingBatches",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MeltingBatches_AssayerId",
                table: "MeltingBatches",
                column: "AssayerId");

            migrationBuilder.AddForeignKey(
                name: "FK_MeltingBatches_Customers_AssayerId",
                table: "MeltingBatches",
                column: "AssayerId",
                principalTable: "Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MeltingBatches_Customers_AssayerId",
                table: "MeltingBatches");

            migrationBuilder.DropIndex(
                name: "IX_MeltingBatches_AssayerId",
                table: "MeltingBatches");

            migrationBuilder.DropColumn(
                name: "AssayerId",
                table: "MeltingBatches");
        }
    }
}
