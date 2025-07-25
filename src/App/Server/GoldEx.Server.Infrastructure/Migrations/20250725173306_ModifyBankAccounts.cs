using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoldEx.Server.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ModifyBankAccounts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PaymentVouchers_BankAccounts_BankAccountId",
                table: "PaymentVouchers");

            migrationBuilder.DropTable(
                name: "BankAccounts");

            migrationBuilder.RenameColumn(
                name: "BankAccountId",
                table: "PaymentVouchers",
                newName: "FinancialAccountId");

            migrationBuilder.RenameIndex(
                name: "IX_PaymentVouchers_BankAccountId",
                table: "PaymentVouchers",
                newName: "IX_PaymentVouchers_FinancialAccountId");

            migrationBuilder.CreateTable(
                name: "FinancialAccounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AccountType = table.Column<int>(type: "int", nullable: false),
                    IsSystemAccount = table.Column<bool>(type: "bit", nullable: false),
                    PriceUnitId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LocalAccount_AccountHolderName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    LocalAccount_BankName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    LocalAccount_CardNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    LocalAccount_ShabaNumber = table.Column<string>(type: "nvarchar(26)", maxLength: 26, nullable: true),
                    LocalAccount_AccountNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    InternationalAccount_AccountHolderName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    InternationalAccount_BankName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    InternationalAccount_SwiftBicCode = table.Column<string>(type: "nvarchar(11)", maxLength: 11, nullable: true),
                    InternationalAccount_IbanNumber = table.Column<string>(type: "nvarchar(34)", maxLength: 34, nullable: true),
                    InternationalAccount_AccountNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinancialAccounts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FinancialAccounts_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FinancialAccounts_PriceUnits_PriceUnitId",
                        column: x => x.PriceUnitId,
                        principalTable: "PriceUnits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FinancialAccounts_CustomerId",
                table: "FinancialAccounts",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialAccounts_PriceUnitId",
                table: "FinancialAccounts",
                column: "PriceUnitId");

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentVouchers_FinancialAccounts_FinancialAccountId",
                table: "PaymentVouchers",
                column: "FinancialAccountId",
                principalTable: "FinancialAccounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PaymentVouchers_FinancialAccounts_FinancialAccountId",
                table: "PaymentVouchers");

            migrationBuilder.DropTable(
                name: "FinancialAccounts");

            migrationBuilder.RenameColumn(
                name: "FinancialAccountId",
                table: "PaymentVouchers",
                newName: "BankAccountId");

            migrationBuilder.RenameIndex(
                name: "IX_PaymentVouchers_FinancialAccountId",
                table: "PaymentVouchers",
                newName: "IX_PaymentVouchers_BankAccountId");

            migrationBuilder.CreateTable(
                name: "BankAccounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PriceUnitId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AccountHolderName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    AccountType = table.Column<int>(type: "int", nullable: false),
                    BankName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsSystemAccount = table.Column<bool>(type: "bit", nullable: false),
                    InternationalAccount_AccountNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    InternationalAccount_IbanNumber = table.Column<string>(type: "nvarchar(34)", maxLength: 34, nullable: true),
                    InternationalAccount_SwiftBicCode = table.Column<string>(type: "nvarchar(11)", maxLength: 11, nullable: true),
                    LocalAccount_AccountNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    LocalAccount_CardNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    LocalAccount_ShabaNumber = table.Column<string>(type: "nvarchar(26)", maxLength: 26, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BankAccounts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BankAccounts_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BankAccounts_PriceUnits_PriceUnitId",
                        column: x => x.PriceUnitId,
                        principalTable: "PriceUnits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BankAccounts_CustomerId",
                table: "BankAccounts",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_BankAccounts_PriceUnitId",
                table: "BankAccounts",
                column: "PriceUnitId");

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentVouchers_BankAccounts_BankAccountId",
                table: "PaymentVouchers",
                column: "BankAccountId",
                principalTable: "BankAccounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
