using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoldEx.Server.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RefactorPaymentVouchers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PaymentVouchers_Customers_CustomerId",
                table: "PaymentVouchers");

            migrationBuilder.DropForeignKey(
                name: "FK_PaymentVouchers_FinancialAccounts_FinancialAccountId",
                table: "PaymentVouchers");

            migrationBuilder.DropForeignKey(
                name: "FK_PaymentVouchers_PriceUnits_AmountPriceUnitId",
                table: "PaymentVouchers");

            migrationBuilder.RenameColumn(
                name: "FinancialAccountId",
                table: "PaymentVouchers",
                newName: "SourceFinancialAccountId");

            migrationBuilder.RenameColumn(
                name: "AmountPriceUnitId",
                table: "PaymentVouchers",
                newName: "DestinationFinancialAccountId");

            migrationBuilder.RenameIndex(
                name: "IX_PaymentVouchers_FinancialAccountId",
                table: "PaymentVouchers",
                newName: "IX_PaymentVouchers_SourceFinancialAccountId");

            migrationBuilder.RenameIndex(
                name: "IX_PaymentVouchers_AmountPriceUnitId",
                table: "PaymentVouchers",
                newName: "IX_PaymentVouchers_DestinationFinancialAccountId");

            migrationBuilder.AlterColumn<Guid>(
                name: "CustomerId",
                table: "PaymentVouchers",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentVouchers_Customers_CustomerId",
                table: "PaymentVouchers",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentVouchers_FinancialAccounts_DestinationFinancialAccountId",
                table: "PaymentVouchers",
                column: "DestinationFinancialAccountId",
                principalTable: "FinancialAccounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentVouchers_FinancialAccounts_SourceFinancialAccountId",
                table: "PaymentVouchers",
                column: "SourceFinancialAccountId",
                principalTable: "FinancialAccounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PaymentVouchers_Customers_CustomerId",
                table: "PaymentVouchers");

            migrationBuilder.DropForeignKey(
                name: "FK_PaymentVouchers_FinancialAccounts_DestinationFinancialAccountId",
                table: "PaymentVouchers");

            migrationBuilder.DropForeignKey(
                name: "FK_PaymentVouchers_FinancialAccounts_SourceFinancialAccountId",
                table: "PaymentVouchers");

            migrationBuilder.RenameColumn(
                name: "SourceFinancialAccountId",
                table: "PaymentVouchers",
                newName: "FinancialAccountId");

            migrationBuilder.RenameColumn(
                name: "DestinationFinancialAccountId",
                table: "PaymentVouchers",
                newName: "AmountPriceUnitId");

            migrationBuilder.RenameIndex(
                name: "IX_PaymentVouchers_SourceFinancialAccountId",
                table: "PaymentVouchers",
                newName: "IX_PaymentVouchers_FinancialAccountId");

            migrationBuilder.RenameIndex(
                name: "IX_PaymentVouchers_DestinationFinancialAccountId",
                table: "PaymentVouchers",
                newName: "IX_PaymentVouchers_AmountPriceUnitId");

            migrationBuilder.AlterColumn<Guid>(
                name: "CustomerId",
                table: "PaymentVouchers",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentVouchers_Customers_CustomerId",
                table: "PaymentVouchers",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentVouchers_FinancialAccounts_FinancialAccountId",
                table: "PaymentVouchers",
                column: "FinancialAccountId",
                principalTable: "FinancialAccounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentVouchers_PriceUnits_AmountPriceUnitId",
                table: "PaymentVouchers",
                column: "AmountPriceUnitId",
                principalTable: "PriceUnits",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
