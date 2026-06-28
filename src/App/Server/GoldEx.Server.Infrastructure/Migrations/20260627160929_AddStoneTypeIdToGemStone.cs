using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoldEx.Server.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddStoneTypeIdToGemStone : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "StoneTypeId",
                table: "GemStones",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_GemStones_StoneTypeId",
                table: "GemStones",
                column: "StoneTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_GemStones_StoneTypes_StoneTypeId",
                table: "GemStones",
                column: "StoneTypeId",
                principalTable: "StoneTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GemStones_StoneTypes_StoneTypeId",
                table: "GemStones");

            migrationBuilder.DropIndex(
                name: "IX_GemStones_StoneTypeId",
                table: "GemStones");

            migrationBuilder.DropColumn(
                name: "StoneTypeId",
                table: "GemStones");
        }
    }
}
