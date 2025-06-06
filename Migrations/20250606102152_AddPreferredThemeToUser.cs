using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WKFTournamentIS.Migrations
{
    /// <inheritdoc />
    public partial class AddPreferredThemeToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PreferredTheme",
                table: "Users",
                type: "varchar(50)",
                maxLength: 50,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "PasswordHash", "PreferredTheme" },
                values: new object[] { "$2a$11$HQBfCjjMuMi8EyZOGD1V9.TrsKUwmixzStlKD/5lYTgS2bewXP0q6", null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PreferredTheme",
                table: "Users");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$bT/IPxSUjbAoDbrl.i125u6Gs5vpTl0La4sDwQmztr2XfHaHfXwfi");
        }
    }
}
