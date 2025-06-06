using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace WKFTournamentIS.Migrations
{
    /// <inheritdoc />
    public partial class AddTournamentTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Tournaments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    BeginningDateTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    EndingDateTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Location = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tournaments", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.InsertData(
                table: "Tournaments",
                columns: new[] { "Id", "BeginningDateTime", "EndingDateTime", "Location", "Name" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 6, 28, 14, 13, 51, 48, DateTimeKind.Local).AddTicks(7251), new DateTime(2025, 6, 30, 14, 13, 51, 48, DateTimeKind.Local).AddTicks(7342), "Sportska hala Partizan, Beograd", "Nacionalno prvenstvo u karateu" },
                    { 2, new DateTime(2025, 5, 14, 14, 13, 51, 48, DateTimeKind.Local).AddTicks(7354), new DateTime(2025, 5, 16, 14, 13, 51, 48, DateTimeKind.Local).AddTicks(7357), "Sportski centar Borik, Banja Luka", "Međunarodni turnir Banja Luka Open" }
                });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$zWHS9TKs02cHcunGsfMEA.IMMtgy.qKQN8DPSxTuyu9Ylu/0Vz1Sy");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Tournaments");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$nkxCQgEBsP03.8J32jEkWebSpwly5te7tcWSeUx5KXYj5H..GFs8a");
        }
    }
}
