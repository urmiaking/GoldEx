using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoldEx.Server.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCashAccounts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InternationalAccount_AccountHolderName",
                table: "FinancialAccounts");

            migrationBuilder.DropColumn(
                name: "InternationalAccount_BankName",
                table: "FinancialAccounts");

            migrationBuilder.RenameColumn(
                name: "LocalAccount_BankName",
                table: "FinancialAccounts",
                newName: "HolderName");

            migrationBuilder.RenameColumn(
                name: "LocalAccount_AccountHolderName",
                table: "FinancialAccounts",
                newName: "BrokerName");

            migrationBuilder.AddColumn<int>(
                name: "CashAccount_AccountType",
                table: "FinancialAccounts",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CashAccount_CreatedAt",
                table: "FinancialAccounts",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "InternationalAccount_CreatedAt",
                table: "FinancialAccounts",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LocalAccount_CreatedAt",
                table: "FinancialAccounts",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CashAccount_AccountType",
                table: "FinancialAccounts");

            migrationBuilder.DropColumn(
                name: "CashAccount_CreatedAt",
                table: "FinancialAccounts");

            migrationBuilder.DropColumn(
                name: "InternationalAccount_CreatedAt",
                table: "FinancialAccounts");

            migrationBuilder.DropColumn(
                name: "LocalAccount_CreatedAt",
                table: "FinancialAccounts");

            migrationBuilder.RenameColumn(
                name: "HolderName",
                table: "FinancialAccounts",
                newName: "LocalAccount_BankName");

            migrationBuilder.RenameColumn(
                name: "BrokerName",
                table: "FinancialAccounts",
                newName: "LocalAccount_AccountHolderName");

            migrationBuilder.AddColumn<string>(
                name: "InternationalAccount_AccountHolderName",
                table: "FinancialAccounts",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InternationalAccount_BankName",
                table: "FinancialAccounts",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);
        }
    }
}
