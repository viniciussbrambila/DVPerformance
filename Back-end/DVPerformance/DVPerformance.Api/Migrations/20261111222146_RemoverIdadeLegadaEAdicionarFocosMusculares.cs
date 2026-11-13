using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DVPerformance.Api.Migrations
{
    /// <inheritdoc />
    public partial class RemoverIdadeLegadaEAdicionarFocosMusculares : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "idade_legada",
                table: "alunos");

            migrationBuilder.AddColumn<string>(
                name: "foco_muscular",
                table: "maquinas",
                type: "character varying(60)",
                maxLength: 60,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "foco_muscular",
                table: "exercicios",
                type: "character varying(60)",
                maxLength: 60,
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "grupos_musculares",
                keyColumn: "id",
                keyValue: 7,
                column: "ordem_prioridade",
                value: 3);

            migrationBuilder.UpdateData(
                table: "grupos_musculares",
                keyColumn: "id",
                keyValue: 9,
                column: "ordem_prioridade",
                value: 2);

            migrationBuilder.UpdateData(
                table: "grupos_musculares",
                keyColumn: "id",
                keyValue: 10,
                column: "ordem_prioridade",
                value: 2);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "foco_muscular",
                table: "maquinas");

            migrationBuilder.DropColumn(
                name: "foco_muscular",
                table: "exercicios");

            migrationBuilder.AddColumn<int>(
                name: "idade_legada",
                table: "alunos",
                type: "integer",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "grupos_musculares",
                keyColumn: "id",
                keyValue: 7,
                column: "ordem_prioridade",
                value: 2);

            migrationBuilder.UpdateData(
                table: "grupos_musculares",
                keyColumn: "id",
                keyValue: 9,
                column: "ordem_prioridade",
                value: 3);

            migrationBuilder.UpdateData(
                table: "grupos_musculares",
                keyColumn: "id",
                keyValue: 10,
                column: "ordem_prioridade",
                value: 3);
        }
    }
}
