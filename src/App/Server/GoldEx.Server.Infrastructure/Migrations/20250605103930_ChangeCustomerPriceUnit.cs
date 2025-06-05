using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoldEx.Server.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ChangeCustomerPriceUnit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreditLimitUnit",
                table: "Customers");

            migrationBuilder.AddColumn<Guid>(
                name: "CreditLimitPriceUnitId",
                table: "Customers",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Customers_CreditLimitPriceUnitId",
                table: "Customers",
                column: "CreditLimitPriceUnitId");

            migrationBuilder.AddForeignKey(
                name: "FK_Customers_PriceUnits_CreditLimitPriceUnitId",
                table: "Customers",
                column: "CreditLimitPriceUnitId",
                principalTable: "PriceUnits",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Customers_PriceUnits_CreditLimitPriceUnitId",
                table: "Customers");

            migrationBuilder.DropIndex(
                name: "IX_Customers_CreditLimitPriceUnitId",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "CreditLimitPriceUnitId",
                table: "Customers");

            migrationBuilder.AddColumn<int>(
                name: "CreditLimitUnit",
                table: "Customers",
                type: "int",
                nullable: true);
        }
    }
}
