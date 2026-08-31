using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoldEx.Server.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddVitrineThemeToSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "VitrineThemePreset",
                table: "Settings",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "royal-emerald");

            migrationBuilder.AddColumn<string>(
                name: "VitrinePrimaryColor",
                table: "Settings",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VitrineAccentColor",
                table: "Settings",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VitrineBackgroundColor",
                table: "Settings",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VitrineSurfaceColor",
                table: "Settings",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VitrineCardStyle",
                table: "Settings",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "minimal");

            migrationBuilder.AddColumn<string>(
                name: "VitrineRadiusStyle",
                table: "Settings",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "rounded");

            migrationBuilder.AddColumn<string>(
                name: "VitrineFontStyle",
                table: "Settings",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "iransans");

            migrationBuilder.AddColumn<string>(
                name: "VitrineHeaderStyle",
                table: "Settings",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "glass-sticky");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VitrineThemePreset",
                table: "Settings");

            migrationBuilder.DropColumn(
                name: "VitrinePrimaryColor",
                table: "Settings");

            migrationBuilder.DropColumn(
                name: "VitrineAccentColor",
                table: "Settings");

            migrationBuilder.DropColumn(
                name: "VitrineBackgroundColor",
                table: "Settings");

            migrationBuilder.DropColumn(
                name: "VitrineSurfaceColor",
                table: "Settings");

            migrationBuilder.DropColumn(
                name: "VitrineCardStyle",
                table: "Settings");

            migrationBuilder.DropColumn(
                name: "VitrineRadiusStyle",
                table: "Settings");

            migrationBuilder.DropColumn(
                name: "VitrineFontStyle",
                table: "Settings");

            migrationBuilder.DropColumn(
                name: "VitrineHeaderStyle",
                table: "Settings");
        }
    }
}
