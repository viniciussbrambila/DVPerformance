using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DVPerformance.Api.Migrations
{
    public partial class AdicionarSegundaDicaAoTreino : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "segunda_dica_objetivo_id",
                table: "treinos",
                type: "integer",
                nullable: true);

            // Preserva dicas existentes; fichas antigas sem dica recebem a primeira do objetivo.
            migrationBuilder.Sql("""
                UPDATE treinos AS treino
                SET dica_objetivo_id = (
                    SELECT dica.id
                    FROM dicas_objetivo AS dica
                    JOIN alunos AS aluno ON aluno.id = treino.aluno_id
                    WHERE dica.objetivo = CASE
                        WHEN aluno.objetivo IN ('Hipertrofia', 'Emagrecimento', 'Força', 'Resistência')
                            THEN aluno.objetivo
                        ELSE 'Hipertrofia'
                    END
                    ORDER BY dica.id
                    LIMIT 1
                )
                WHERE treino.dica_objetivo_id IS NULL;

                UPDATE treinos AS treino
                SET segunda_dica_objetivo_id = (
                    SELECT dica.id
                    FROM dicas_objetivo AS dica
                    JOIN dicas_objetivo AS primeira ON primeira.id = treino.dica_objetivo_id
                    WHERE dica.objetivo = primeira.objetivo
                        AND dica.id <> primeira.id
                        AND btrim(dica.texto) <> btrim(primeira.texto)
                    ORDER BY CASE WHEN dica.id > primeira.id THEN 0 ELSE 1 END, dica.id
                    LIMIT 1
                )
                WHERE treino.segunda_dica_objetivo_id IS NULL;
                """);

            migrationBuilder.CreateIndex(
                name: "IX_treinos_segunda_dica_objetivo_id",
                table: "treinos",
                column: "segunda_dica_objetivo_id");

            migrationBuilder.AddForeignKey(
                name: "FK_treinos_dicas_objetivo_segunda_dica_objetivo_id",
                table: "treinos",
                column: "segunda_dica_objetivo_id",
                principalTable: "dicas_objetivo",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddCheckConstraint(
                name: "ck_treinos_dicas_distintas",
                table: "treinos",
                sql: "\"dica_objetivo_id\" <> \"segunda_dica_objetivo_id\"");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "ck_treinos_dicas_distintas",
                table: "treinos");

            migrationBuilder.DropForeignKey(
                name: "FK_treinos_dicas_objetivo_segunda_dica_objetivo_id",
                table: "treinos");

            migrationBuilder.DropIndex(
                name: "IX_treinos_segunda_dica_objetivo_id",
                table: "treinos");

            migrationBuilder.DropColumn(
                name: "segunda_dica_objetivo_id",
                table: "treinos");
        }
    }
}
