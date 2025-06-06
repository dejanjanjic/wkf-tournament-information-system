using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WKFTournamentIS.Migrations
{
    /// <inheritdoc />
    public partial class AddCompetitorInCategoryInTournamentTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CompetitorRegistrations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CategoryInTournamentId = table.Column<int>(type: "int", nullable: false),
                    CompetitorId = table.Column<int>(type: "int", nullable: false),
                    Placement = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompetitorRegistrations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompetitorRegistrations_CategoriesInTournaments_CategoryInTo~",
                        column: x => x.CategoryInTournamentId,
                        principalTable: "CategoriesInTournaments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CompetitorRegistrations_Competitors_CompetitorId",
                        column: x => x.CompetitorId,
                        principalTable: "Competitors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$Ky/IbRVvgW3.yCNyOyES5Oh.9LIdCFMfxOfchTURerBrXRJThON6u");

            migrationBuilder.CreateIndex(
                name: "IX_CompetitorRegistrations_CategoryInTournamentId_CompetitorId",
                table: "CompetitorRegistrations",
                columns: new[] { "CategoryInTournamentId", "CompetitorId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CompetitorRegistrations_CompetitorId",
                table: "CompetitorRegistrations",
                column: "CompetitorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CompetitorRegistrations");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$.91dofskC9CroDRMgH.5ju4hreJNymFk5WQNJs2LgL7snK/ZxkiaW");
        }
    }
}
