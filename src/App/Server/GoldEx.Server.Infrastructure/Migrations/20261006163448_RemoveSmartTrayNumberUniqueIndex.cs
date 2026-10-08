using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoldEx.Server.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveSmartTrayNumberUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SmartTrays_StoreId_TrayNumber",
                table: "SmartTrays");

            migrationBuilder.CreateIndex(
                name: "IX_SmartTrays_StoreId_TrayNumber",
                table: "SmartTrays",
                columns: new[] { "StoreId", "TrayNumber" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SmartTrays_StoreId_TrayNumber",
                table: "SmartTrays");

            migrationBuilder.CreateIndex(
                name: "IX_SmartTrays_StoreId_TrayNumber",
                table: "SmartTrays",
                columns: new[] { "StoreId", "TrayNumber" },
                unique: true);
        }
    }
}
