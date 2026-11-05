using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DVPerformance.Api.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarTreinosEItensTreino : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "treinos",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    aluno_id = table.Column<int>(type: "integer", nullable: false),
                    usuario_id = table.Column<int>(type: "integer", nullable: true),
                    nome_plano = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    divisao = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    tempo_aerobico = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    data_criacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ativo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_treinos", x => x.id);
                    table.ForeignKey(
                        name: "FK_treinos_alunos_aluno_id",
                        column: x => x.aluno_id,
                        principalTable: "alunos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_treinos_usuarios_usuario_id",
                        column: x => x.usuario_id,
                        principalTable: "usuarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "itens_treino",
                columns: table => new
                {
                    id_item = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    treino_id = table.Column<int>(type: "integer", nullable: false),
                    divisao_treino = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ordem_exercicio = table.Column<int>(type: "integer", nullable: false),
                    exercicio_id = table.Column<int>(type: "integer", nullable: true),
                    maquina_id = table.Column<int>(type: "integer", nullable: true),
                    series = table.Column<int>(type: "integer", nullable: false),
                    repeticao = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    tempo_descanso = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    tempo_aerobico = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_itens_treino", x => x.id_item);
                    table.ForeignKey(
                        name: "FK_itens_treino_exercicios_exercicio_id",
                        column: x => x.exercicio_id,
                        principalTable: "exercicios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_itens_treino_maquinas_maquina_id",
                        column: x => x.maquina_id,
                        principalTable: "maquinas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_itens_treino_treinos_treino_id",
                        column: x => x.treino_id,
                        principalTable: "treinos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_itens_treino_exercicio_id",
                table: "itens_treino",
                column: "exercicio_id");

            migrationBuilder.CreateIndex(
                name: "IX_itens_treino_maquina_id",
                table: "itens_treino",
                column: "maquina_id");

            migrationBuilder.CreateIndex(
                name: "IX_itens_treino_treino_id",
                table: "itens_treino",
                column: "treino_id");

            migrationBuilder.CreateIndex(
                name: "IX_treinos_aluno_id",
                table: "treinos",
                column: "aluno_id");

            migrationBuilder.CreateIndex(
                name: "IX_treinos_usuario_id",
                table: "treinos",
                column: "usuario_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "itens_treino");

            migrationBuilder.DropTable(
                name: "treinos");
        }
    }
}
