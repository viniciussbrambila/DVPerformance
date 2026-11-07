using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DVPerformance.Api.Migrations
{
    /// <inheritdoc />
    public partial class ArmazenarTempoAerobicoEmMinutos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Aproveita a duração dos textos antigos; vazio ou sem minutos indica ausência de aeróbico.
            migrationBuilder.Sql(@"
                ALTER TABLE itens_treino
                ALTER COLUMN tempo_aerobico TYPE integer
                USING CASE
                    WHEN substring(tempo_aerobico FROM '[0-9]+')::numeric BETWEEN 0 AND 2147483647
                    THEN substring(tempo_aerobico FROM '[0-9]+')::integer
                    ELSE 0
                END;

                ALTER TABLE itens_treino
                ALTER COLUMN tempo_aerobico SET DEFAULT 0;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                ALTER TABLE itens_treino
                ALTER COLUMN tempo_aerobico DROP DEFAULT;

                ALTER TABLE itens_treino
                ALTER COLUMN tempo_aerobico TYPE character varying(120)
                USING CASE
                    WHEN tempo_aerobico > 0 THEN tempo_aerobico::text || ' min de aeróbico'
                    ELSE ''
                END;
            ");
        }
    }
}
