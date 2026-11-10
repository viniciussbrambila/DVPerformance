using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DVPerformance.Api.Migrations
{
    public partial class SubstituirIdadePorDataDeNascimento : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Preserva o valor histórico; a idade não permite recuperar o aniversário exato.
            migrationBuilder.RenameColumn(
                name: "idade", table: "alunos", newName: "idade_legada");
            migrationBuilder.AlterColumn<int>(
                name: "idade_legada", table: "alunos", type: "integer", nullable: true,
                oldClrType: typeof(int), oldType: "integer");
            migrationBuilder.AddColumn<DateOnly>(
                name: "data_de_nascimento", table: "alunos", type: "date", nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Na reversão, converte somente as datas conhecidas; mantém os valores legados.
            migrationBuilder.Sql("""
                UPDATE alunos
                SET idade_legada = EXTRACT(YEAR FROM AGE(
                    (CURRENT_TIMESTAMP AT TIME ZONE 'America/Sao_Paulo')::date,
                    data_de_nascimento))::integer
                WHERE data_de_nascimento IS NOT NULL;

                DO $$
                BEGIN
                    IF EXISTS (SELECT 1 FROM alunos WHERE idade_legada IS NULL) THEN
                        RAISE EXCEPTION 'Não é possível reverter: há alunos sem data de nascimento nem idade legada.';
                    END IF;
                END $$;
                """);
            migrationBuilder.DropColumn(name: "data_de_nascimento", table: "alunos");
            migrationBuilder.AlterColumn<int>(
                name: "idade_legada", table: "alunos", type: "integer", nullable: false,
                oldClrType: typeof(int), oldType: "integer", oldNullable: true);
            migrationBuilder.RenameColumn(
                name: "idade_legada", table: "alunos", newName: "idade");
        }
    }
}
