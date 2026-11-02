using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace DVPerformance.Api.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarDicasObjetivo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "dica_objetivo_id",
                table: "treinos",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "dicas_objetivo",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    objetivo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    texto = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: false),
                    ativo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dicas_objetivo", x => x.id);
                });

            migrationBuilder.InsertData(
                table: "dicas_objetivo",
                columns: new[] { "id", "ativo", "objetivo", "texto" },
                values: new object[,]
                {
                    { 1, true, "Hipertrofia", "Priorize a execução controlada na fase excêntrica (descida do peso) em cada repetição, mantendo a tensão mecânica constante no músculo-alvo para maximizar o estímulo de hipertrofia." },
                    { 2, true, "Hipertrofia", "Busque a progressão gradual de carga ou de repetições semana após semana, sempre preservando a amplitude completa do movimento antes de aumentar o peso." },
                    { 3, true, "Hipertrofia", "Respeite os intervalos de descanso prescritos entre as séries para recuperar sua capacidade de contração e manter um alto volume de carga útil durante toda a sessão." },
                    { 4, true, "Hipertrofia", "O crescimento muscular acontece durante a recuperação: priorize de 7 a 9 horas de sono reparador por noite para otimizar a síntese proteica e a regeneração das fibras." },
                    { 5, true, "Hipertrofia", "Mantenha uma ingestão adequada de proteínas fracionada ao longo do dia e hidratação constante para fornecer os nutrientes necessários à construção de massa magra." },
                    { 6, true, "Hipertrofia", "Leve as séries de trabalho próximas à falha concêntrica com segurança técnica, evitando compensações posturais ou impulsos que roubem o esforço do músculo principal." },
                    { 7, true, "Hipertrofia", "Concentre-se na conexão mente-músculo durante cada série, sentindo o alongamento completo e o pico de contração antes de iniciar a próxima repetição." },
                    { 8, true, "Hipertrofia", "Realize o cardio pós-treino na intensidade moderada indicada na ficha para melhorar a sensibilidade à insulina e o condicionamento sem comprometer a recuperação muscular." },
                    { 9, true, "Hipertrofia", "Não pule as séries iniciais de aquecimento articular e específico: elas preparam tendões e sistema nervoso para suportar cargas mais produtivas nas séries principais." },
                    { 10, true, "Hipertrofia", "A constância semanal supera treinos isolados extremos: cumprir todas as sessões planejadas garante a frequência ideal de estímulo para cada grupamento muscular." },
                    { 11, true, "Hipertrofia", "Anote as cargas utilizadas nos exercícios base do seu protocolo para acompanhar sua evolução real e identificar o momento exato de progredir nos treinos seguintes." },
                    { 12, true, "Hipertrofia", "Evite treinar o mesmo grupo muscular em dias consecutivos fora da divisão prescrita, garantindo a supercompensação necessária para o ganho sólido de volume muscular." },
                    { 13, true, "Emagrecimento", "Mantenha os intervalos de descanso curtos e rigorosos conforme prescrito na ficha para sustentar a frequência cardíaca elevada e ampliar o gasto calórico da sessão." },
                    { 14, true, "Emagrecimento", "A musculação é essencial no emagrecimento: preservar e estimular sua massa magra mantém o metabolismo basal ativo e evita a flacidez durante a redução de gordura." },
                    { 15, true, "Emagrecimento", "Combine o treino de força com o cardio pós-treino indicado no protocolo para potencializar a oxidação de gorduras aproveitando a depleção prévia de glicogênio." },
                    { 16, true, "Emagrecimento", "Aumente seu nível de movimento fora da academia: caminhar mais no dia a dia, usar escadas e evitar longos períodos sentado acelera significativamente o gasto energético semanal." },
                    { 17, true, "Emagrecimento", "Mantenha excelente hidratação antes, durante e após o treino; o consumo adequado de água otimiza o metabolismo energético, o desempenho físico e o controle da saciedade." },
                    { 18, true, "Emagrecimento", "Não reduza drasticamente as cargas por estar em foco de perda de gordura: manter treinos intensos sinaliza ao organismo que a musculatura deve ser preservada." },
                    { 19, true, "Emagrecimento", "Alinhe a regularidade dos treinos a uma alimentação equilibrada em déficit calórico sustentável, priorizando alimentos naturais, fibras e bom aporte de proteínas." },
                    { 20, true, "Emagrecimento", "Cuide da qualidade do sono diariamente: noites mal dormidas desregulam hormônios ligados à fome e ao estresse, dificultando a queima de gordura e a disposição para treinar." },
                    { 21, true, "Emagrecimento", "Evite compensar o gasto do treino com excessos alimentares: a consistência diária entre exercício e nutrição é o que constrói resultados duradouros na composição corporal." },
                    { 22, true, "Emagrecimento", "Acompanhe sua evolução por medidas corporais, roupas e desempenho nos treinos, pois a balança isoladamente não diferencia perda de gordura de ganho de massa muscular." },
                    { 23, true, "Emagrecimento", "Execute os exercícios multiarticulares com empenho e boa amplitude, pois movimentos que recrutam grandes grupos musculares demandam muito mais energia do organismo." },
                    { 24, true, "Emagrecimento", "Mesmo nos dias de menor disposição, cumpra a sessão planejada: manter o hábito ativo é mais eficiente para o emagrecimento do que buscar perfeição apenas em alguns dias." },
                    { 25, true, "Força", "Respeite integralmente os tempos mais longos de descanso entre as séries: eles são indispensáveis para restaurar os estoques de ATP-CP e o sistema nervoso central." },
                    { 26, true, "Força", "Ative firmemente o core e controle a respiração (estabilização intra-abdominal) antes de cada repetição pesada para proteger a coluna e transferir mais força ao movimento." },
                    { 27, true, "Força", "Realize séries progressivas de aquecimento específico no primeiro exercício do dia antes de entrar na carga máxima de trabalho, preparando articulações e recrutamento neural." },
                    { 28, true, "Força", "Imprima intenção de velocidade na fase concêntrica (subida da carga) mantendo controle total na descida, maximizando o recrutamento das fibras musculares de maior potência." },
                    { 29, true, "Força", "No treino de força, a qualidade técnica vem sempre antes do ego: interrompa a série caso perceba perda do alinhamento articular ou compensações perigosas na postura." },
                    { 30, true, "Força", "Garanta pés bem plantados no solo, escápulas estabilizadas e pegada firme na barra ou equipamento antes de iniciar cada repetição para evitar dissipação de força." },
                    { 31, true, "Força", "Pequenos incrementos de carga ao longo das semanas geram grandes ganhos de força a médio prazo sem sobrecarregar tendões, ligamentos e articulações." },
                    { 32, true, "Força", "Priorize os exercícios multiarticulares livres ou guiados do início da ficha enquanto o sistema nervoso está descansado, pois são eles que constroem sua base de força." },
                    { 33, true, "Força", "Mantenha o cardio leve conforme prescrito na ficha apenas como suporte cardiovascular e aquecimento, preservando suas reservas energéticas para as séries pesadas." },
                    { 34, true, "Força", "Cuide da mobilidade de tornozelos, quadris e ombros regularmente: articulações móveis e estáveis permitem alavancas mecânicas muito mais seguras e eficientes sob carga." },
                    { 35, true, "Força", "Valorize os dias de descanso da sua divisão semanal, pois adaptações neurais e estruturais de força exigem recuperação completa entre sessões intensas." },
                    { 36, true, "Força", "Mantenha um bom aporte energético e hidratação adequada antes de treinar; estoques plenos de glicogênio muscular são fundamentais para sustentar cargas elevadas." },
                    { 37, true, "Resistência", "Mantenha um ritmo cadenciado e respiração fluida do início ao fim de cada série longa, evitando prender o ar para retardar a instalação precoce da fadiga periférica." },
                    { 38, true, "Resistência", "Controle a escolha da carga para completar toda a faixa de repetições altas prescrita mantendo a postura impecável até a última repetição da série." },
                    { 39, true, "Resistência", "Respeite os intervalos reduzidos de descanso entre as séries para estimular a capilarização muscular, a eficiência mitocondrial e a tolerância à acidose metabólica." },
                    { 40, true, "Resistência", "Capriche na hidratação ao longo do dia e durante a sessão: a perda hídrica reduz rapidamente o rendimento em treinos de resistência muscular e aeróbica." },
                    { 41, true, "Resistência", "Execute o cardio contínuo pós-treino na zona aeróbica orientada para fortalecer o sistema cardiorrespiratório e acelerar a remoção de metabólitos entre os esforços." },
                    { 42, true, "Resistência", "Não encurte a amplitude das repetições quando a queimação muscular surgir: manter o movimento completo sob fadiga controlada é o que desenvolve resistência real." },
                    { 43, true, "Resistência", "Garanta uma ingestão equilibrada de carboidratos de qualidade na rotina alimentar para manter os estoques de energia necessários às sessões de maior volume de repetições." },
                    { 44, true, "Resistência", "Mantenha ombros, pescoço e musculaturas acessórias livres de tensão desnecessária durante o exercício para economizar energia e focar o esforço nos músculos-alvo." },
                    { 45, true, "Resistência", "A evolução na resistência aparece quando o mesmo esforço passa a parecer mais leve e sua recuperação entre as séries se torna cada vez mais rápida." },
                    { 46, true, "Resistência", "Alongamentos leves e exercícios de mobilidade após a sessão ajudam a aliviar a rigidez muscular e preservar a fluidez articular ao longo da semana de treinos." },
                    { 47, true, "Resistência", "Mantenha regularidade absoluta na frequência semanal: adaptações aeróbicas e de resistência muscular respondem de forma excelente à continuidade dos estímulos." },
                    { 48, true, "Resistência", "Preste atenção redobrada à estabilidade do core no final das séries longas, garantindo que a fadiga não comprometa o alinhamento da coluna lombar." }
                });

            migrationBuilder.CreateIndex(
                name: "IX_treinos_dica_objetivo_id",
                table: "treinos",
                column: "dica_objetivo_id");

            migrationBuilder.CreateIndex(
                name: "IX_dicas_objetivo_objetivo_ativo",
                table: "dicas_objetivo",
                columns: new[] { "objetivo", "ativo" });

            migrationBuilder.AddForeignKey(
                name: "FK_treinos_dicas_objetivo_dica_objetivo_id",
                table: "treinos",
                column: "dica_objetivo_id",
                principalTable: "dicas_objetivo",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            // Ajusta a sequence de IDs e sorteia automaticamente 1 dica compatível para fichas antigas já existentes no banco
            migrationBuilder.Sql(@"
                SELECT setval(
                    pg_get_serial_sequence('dicas_objetivo', 'id'),
                    COALESCE((SELECT MAX(id) FROM dicas_objetivo), 1),
                    true
                );

                UPDATE treinos t
                SET dica_objetivo_id = (
                    SELECT d.id
                    FROM dicas_objetivo d
                    WHERE d.ativo = TRUE
                      AND d.objetivo = CASE
                          WHEN a.objetivo IN ('Hipertrofia', 'Emagrecimento', 'Força', 'Resistência') THEN a.objetivo
                          ELSE 'Hipertrofia'
                      END
                    ORDER BY random() + (t.id * 0)
                    LIMIT 1
                )
                FROM alunos a
                WHERE t.aluno_id = a.id
                  AND t.dica_objetivo_id IS NULL;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_treinos_dicas_objetivo_dica_objetivo_id",
                table: "treinos");

            migrationBuilder.DropTable(
                name: "dicas_objetivo");

            migrationBuilder.DropIndex(
                name: "IX_treinos_dica_objetivo_id",
                table: "treinos");

            migrationBuilder.DropColumn(
                name: "dica_objetivo_id",
                table: "treinos");
        }
    }
}
