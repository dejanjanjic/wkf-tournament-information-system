using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WKFTournamentIS.Migrations
{
    /// <inheritdoc />
    public partial class AddCategoryInTournamentTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CategoriesInTournaments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    TournamentId = table.Column<int>(type: "int", nullable: false),
                    CategoryId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategoriesInTournaments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CategoriesInTournaments_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CategoriesInTournaments_Tournaments_TournamentId",
                        column: x => x.TournamentId,
                        principalTable: "Tournaments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$.91dofskC9CroDRMgH.5ju4hreJNymFk5WQNJs2LgL7snK/ZxkiaW");

            migrationBuilder.CreateIndex(
                name: "IX_CategoriesInTournaments_CategoryId",
                table: "CategoriesInTournaments",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_CategoriesInTournaments_TournamentId_CategoryId",
                table: "CategoriesInTournaments",
                columns: new[] { "TournamentId", "CategoryId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CategoriesInTournaments");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$iJqJfWA36RaLTyTC9npANOjfmY//qNU6OFtm.f6MfZrWKthsPk38S");
        }
    }
}
