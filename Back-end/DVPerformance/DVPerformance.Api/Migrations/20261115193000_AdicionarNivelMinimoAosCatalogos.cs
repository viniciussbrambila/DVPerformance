using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DVPerformance.Api.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarNivelMinimoAosCatalogos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "nivel_minimo",
                table: "maquinas",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Iniciante");

            migrationBuilder.AddColumn<string>(
                name: "nivel_minimo",
                table: "exercicios",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Iniciante");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "nivel_minimo",
                table: "maquinas");

            migrationBuilder.DropColumn(
                name: "nivel_minimo",
                table: "exercicios");
        }
    }
}
