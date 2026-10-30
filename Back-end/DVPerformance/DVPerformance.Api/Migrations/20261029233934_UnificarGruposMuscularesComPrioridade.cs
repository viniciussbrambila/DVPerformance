using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace DVPerformance.Api.Migrations
{
    /// <inheritdoc />
    public partial class UnificarGruposMuscularesComPrioridade : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Cria a tabela mestre grupos_musculares e popula os 14 grupos com suas prioridades (1 a 4)
            migrationBuilder.CreateTable(
                name: "grupos_musculares",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nome = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    regiao = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    ordem_prioridade = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_grupos_musculares", x => x.id);
                });

            migrationBuilder.InsertData(
                table: "grupos_musculares",
                columns: new[] { "id", "nome", "ordem_prioridade", "regiao" },
                values: new object[,]
                {
                    { 1, "Peito", 1, "Membros Superiores" },
                    { 2, "Costas", 1, "Membros Superiores" },
                    { 3, "Quadríceps", 1, "Membros Inferiores" },
                    { 4, "Posterior de Coxa", 1, "Membros Inferiores" },
                    { 5, "Glúteos", 1, "Membros Inferiores" },
                    { 6, "Ombros", 2, "Membros Superiores" },
                    { 7, "Trapézio", 2, "Membros Superiores" },
                    { 8, "Adutores / Abdutores", 2, "Membros Inferiores" },
                    { 9, "Bíceps", 3, "Membros Superiores" },
                    { 10, "Tríceps", 3, "Membros Superiores" },
                    { 11, "Panturrilha", 3, "Membros Inferiores" },
                    { 12, "Antebraço", 4, "Membros Superiores" },
                    { 13, "Abdômen", 4, "Core / Tronco" },
                    { 14, "Lombar", 4, "Core / Tronco" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_grupos_musculares_nome",
                table: "grupos_musculares",
                column: "nome",
                unique: true);

            // 2. Adiciona a chave estrangeira grupo_muscular_id nas tabelas exercicios e maquinas
            migrationBuilder.AddColumn<int>(
                name: "grupo_muscular_id",
                table: "maquinas",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "grupo_muscular_id",
                table: "exercicios",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            // 3. Migra automaticamente qualquer exercício ou máquina já existente preservando seu grupo muscular!
            migrationBuilder.Sql(@"
                UPDATE exercicios e
                SET grupo_muscular_id = g.id
                FROM grupos_musculares g
                WHERE e.grupo = g.nome;
            ");

            migrationBuilder.Sql(@"
                UPDATE maquinas m
                SET grupo_muscular_id = g.id
                FROM grupos_musculares g
                WHERE m.grupo = g.nome;
            ");

            // 4. Remove as colunas redundantes (grupo e regiao) de exercicios e maquinas
            migrationBuilder.DropColumn(
                name: "grupo",
                table: "maquinas");

            migrationBuilder.DropColumn(
                name: "regiao",
                table: "maquinas");

            migrationBuilder.DropColumn(
                name: "grupo",
                table: "exercicios");

            migrationBuilder.DropColumn(
                name: "regiao",
                table: "exercicios");

            // 5. Cria os índices e chaves estrangeiras para grupos_musculares
            migrationBuilder.CreateIndex(
                name: "IX_maquinas_grupo_muscular_id",
                table: "maquinas",
                column: "grupo_muscular_id");

            migrationBuilder.CreateIndex(
                name: "IX_exercicios_grupo_muscular_id",
                table: "exercicios",
                column: "grupo_muscular_id");

            migrationBuilder.AddForeignKey(
                name: "FK_exercicios_grupos_musculares_grupo_muscular_id",
                table: "exercicios",
                column: "grupo_muscular_id",
                principalTable: "grupos_musculares",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_maquinas_grupos_musculares_grupo_muscular_id",
                table: "maquinas",
                column: "grupo_muscular_id",
                principalTable: "grupos_musculares",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "grupo",
                table: "maquinas",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "regiao",
                table: "maquinas",
                type: "character varying(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "grupo",
                table: "exercicios",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "regiao",
                table: "exercicios",
                type: "character varying(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql(@"
                UPDATE exercicios e
                SET grupo = g.nome, regiao = g.regiao
                FROM grupos_musculares g
                WHERE e.grupo_muscular_id = g.id;
            ");

            migrationBuilder.Sql(@"
                UPDATE maquinas m
                SET grupo = g.nome, regiao = g.regiao
                FROM grupos_musculares g
                WHERE m.grupo_muscular_id = g.id;
            ");

            migrationBuilder.DropForeignKey(
                name: "FK_exercicios_grupos_musculares_grupo_muscular_id",
                table: "exercicios");

            migrationBuilder.DropForeignKey(
                name: "FK_maquinas_grupos_musculares_grupo_muscular_id",
                table: "maquinas");

            migrationBuilder.DropIndex(
                name: "IX_maquinas_grupo_muscular_id",
                table: "maquinas");

            migrationBuilder.DropIndex(
                name: "IX_exercicios_grupo_muscular_id",
                table: "exercicios");

            migrationBuilder.DropColumn(
                name: "grupo_muscular_id",
                table: "maquinas");

            migrationBuilder.DropColumn(
                name: "grupo_muscular_id",
                table: "exercicios");

            migrationBuilder.DropTable(
                name: "grupos_musculares");
        }
    }
}
