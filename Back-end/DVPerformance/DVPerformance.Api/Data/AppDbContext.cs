using DVPerformance.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace DVPerformance.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Aluno> Alunos { get; set; }

    public DbSet<Usuario> Usuarios { get; set; }

    public DbSet<GrupoMuscular> GruposMusculares { get; set; }

    public DbSet<Exercicio> Exercicios { get; set; }

    public DbSet<Maquina> Maquinas { get; set; }

    public DbSet<DicaObjetivo> DicasObjetivo { get; set; }

    public DbSet<Treino> Treinos { get; set; }

    public DbSet<ItemTreino> ItensTreino { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // 1. Mapeamento da tabela 'usuarios' (Contas de Personal / Admin)
        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.ToTable("usuarios");

            entity.HasKey(u => u.Id);

            entity.Property(u => u.Id)
                  .HasColumnName("id");

            entity.Property(u => u.NomeUsuario)
                  .HasColumnName("nome_usuario")
                  .HasMaxLength(50)
                  .IsRequired();

            entity.HasIndex(u => u.NomeUsuario)
                  .IsUnique();

            entity.Property(u => u.Senha)
                  .HasColumnName("senha")
                  .HasMaxLength(100)
                  .IsRequired();

            entity.Property(u => u.Perfil)
                  .HasColumnName("perfil")
                  .HasMaxLength(20)
                  .IsRequired();

            entity.Property(u => u.CatalogoInicializado)
                  .HasColumnName("catalogo_inicializado")
                  .HasDefaultValue(false)
                  .IsRequired();

            entity.Property(u => u.Ativo)
                  .HasColumnName("ativo")
                  .HasDefaultValue(true)
                  .IsRequired();
        });

        // 2. Mapeamento da tabela 'alunos' (vinculada ao Personal via usuario_id)
        modelBuilder.Entity<Aluno>(entity =>
        {
            entity.ToTable("alunos");

            entity.HasKey(a => a.Id);

            entity.Property(a => a.Id)
                  .HasColumnName("id");

            entity.Property(a => a.Nome)
                  .HasColumnName("nome")
                  .HasMaxLength(100)
                  .IsRequired();

            entity.Property(a => a.Sexo)
                  .HasColumnName("sexo")
                  .HasMaxLength(1)
                  .IsRequired();

            entity.Property(a => a.Idade)
                  .HasColumnName("idade")
                  .IsRequired();

            entity.Property(a => a.Experiencia)
                  .HasColumnName("experiencia")
                  .HasMaxLength(20)
                  .IsRequired();

            entity.Property(a => a.FrequenciaSemanal)
                  .HasColumnName("frequencia_semanal")
                  .IsRequired();

            entity.Property(a => a.Objetivo)
                  .HasColumnName("objetivo")
                  .HasMaxLength(30)
                  .IsRequired();

            entity.Property(a => a.Peso)
                  .HasColumnName("peso")
                  .HasPrecision(5, 2);

            entity.Property(a => a.Altura)
                  .HasColumnName("altura")
                  .HasPrecision(4, 2);

            entity.Property(a => a.DataCadastro)
                  .HasColumnName("data_cadastro")
                  .HasDefaultValueSql("CURRENT_TIMESTAMP")
                  .IsRequired();

            entity.Property(a => a.UsuarioId)
                  .HasColumnName("usuario_id");

            entity.Property(a => a.Ativo)
                  .HasColumnName("ativo")
                  .HasDefaultValue(true)
                  .IsRequired();

            entity.HasOne<Usuario>()
                  .WithMany()
                  .HasForeignKey(a => a.UsuarioId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // 3. Mapeamento da tabela mestre 'grupos_musculares' (unifica área muscular de Exercícios e Máquinas + Ordem de Prioridade no treino)
        modelBuilder.Entity<GrupoMuscular>(entity =>
        {
            entity.ToTable("grupos_musculares");

            entity.HasKey(g => g.Id);

            entity.Property(g => g.Id)
                  .HasColumnName("id");

            entity.Property(g => g.Nome)
                  .HasColumnName("nome")
                  .HasMaxLength(50)
                  .IsRequired();

            entity.HasIndex(g => g.Nome)
                  .IsUnique();

            entity.Property(g => g.Regiao)
                  .HasColumnName("regiao")
                  .HasMaxLength(40)
                  .IsRequired();

            entity.Property(g => g.OrdemPrioridade)
                  .HasColumnName("ordem_prioridade")
                  .IsRequired();

            // Carga padrão dos 14 grupos musculares com prioridade anatômica:
            // 1 = Músculo Grande (Base) | 2 = Médio (Sinergista) | 3 = Pequeno (Auxiliar) | 4 = Core / Finalizador
            entity.HasData(
                new GrupoMuscular { Id = 1, Nome = "Peito", Regiao = "Membros Superiores", OrdemPrioridade = 1 },
                new GrupoMuscular { Id = 2, Nome = "Costas", Regiao = "Membros Superiores", OrdemPrioridade = 1 },
                new GrupoMuscular { Id = 3, Nome = "Quadríceps", Regiao = "Membros Inferiores", OrdemPrioridade = 1 },
                new GrupoMuscular { Id = 4, Nome = "Posterior de Coxa", Regiao = "Membros Inferiores", OrdemPrioridade = 1 },
                new GrupoMuscular { Id = 5, Nome = "Glúteos", Regiao = "Membros Inferiores", OrdemPrioridade = 1 },
                new GrupoMuscular { Id = 6, Nome = "Ombros", Regiao = "Membros Superiores", OrdemPrioridade = 2 },
                new GrupoMuscular { Id = 7, Nome = "Trapézio", Regiao = "Membros Superiores", OrdemPrioridade = 2 },
                new GrupoMuscular { Id = 8, Nome = "Adutores / Abdutores", Regiao = "Membros Inferiores", OrdemPrioridade = 2 },
                new GrupoMuscular { Id = 9, Nome = "Bíceps", Regiao = "Membros Superiores", OrdemPrioridade = 3 },
                new GrupoMuscular { Id = 10, Nome = "Tríceps", Regiao = "Membros Superiores", OrdemPrioridade = 3 },
                new GrupoMuscular { Id = 11, Nome = "Panturrilha", Regiao = "Membros Inferiores", OrdemPrioridade = 3 },
                new GrupoMuscular { Id = 12, Nome = "Antebraço", Regiao = "Membros Superiores", OrdemPrioridade = 4 },
                new GrupoMuscular { Id = 13, Nome = "Abdômen", Regiao = "Core / Tronco", OrdemPrioridade = 4 },
                new GrupoMuscular { Id = 14, Nome = "Lombar", Regiao = "Core / Tronco", OrdemPrioridade = 4 }
            );
        });

        // 4. Mapeamento da tabela 'exercicios' (vinculada a grupos_musculares sem redundância de colunas)
        modelBuilder.Entity<Exercicio>(entity =>
        {
            entity.ToTable("exercicios");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                  .HasColumnName("id");

            entity.Property(e => e.Nome)
                  .HasColumnName("nome")
                  .HasMaxLength(100)
                  .IsRequired();

            entity.Property(e => e.GrupoMuscularId)
                  .HasColumnName("grupo_muscular_id")
                  .IsRequired();

            entity.Property(e => e.Descricao)
                  .HasColumnName("descricao")
                  .HasMaxLength(255);

            entity.Property(e => e.UsuarioId)
                  .HasColumnName("usuario_id");

            entity.Property(e => e.Ativo)
                  .HasColumnName("ativo")
                  .HasDefaultValue(true)
                  .IsRequired();

            entity.HasIndex(e => new { e.UsuarioId, e.Ativo });

            entity.HasOne(e => e.GrupoMuscular)
                  .WithMany()
                  .HasForeignKey(e => e.GrupoMuscularId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<Usuario>()
                  .WithMany()
                  .HasForeignKey(e => e.UsuarioId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // 5. Mapeamento da tabela 'maquinas' (vinculada a grupos_musculares sem redundância de colunas)
        modelBuilder.Entity<Maquina>(entity =>
        {
            entity.ToTable("maquinas");

            entity.HasKey(m => m.Id);

            entity.Property(m => m.Id)
                  .HasColumnName("id");

            entity.Property(m => m.Nome)
                  .HasColumnName("nome")
                  .HasMaxLength(100)
                  .IsRequired();

            entity.Property(m => m.GrupoMuscularId)
                  .HasColumnName("grupo_muscular_id")
                  .IsRequired();

            entity.Property(m => m.Descricao)
                  .HasColumnName("descricao")
                  .HasMaxLength(255);

            entity.Property(m => m.UsuarioId)
                  .HasColumnName("usuario_id");

            entity.Property(m => m.Ativo)
                  .HasColumnName("ativo")
                  .HasDefaultValue(true)
                  .IsRequired();

            entity.HasIndex(m => new { m.UsuarioId, m.Ativo });

            entity.HasOne(m => m.GrupoMuscular)
                  .WithMany()
                  .HasForeignKey(m => m.GrupoMuscularId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<Usuario>()
                  .WithMany()
                  .HasForeignKey(m => m.UsuarioId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // 6. Mapeamento da tabela mestre 'dicas_objetivo' (12 dicas contextualizadas para cada um dos 4 objetivos para exibição aleatória no PDF)
        modelBuilder.Entity<DicaObjetivo>(entity =>
        {
            entity.ToTable("dicas_objetivo");

            entity.HasKey(d => d.Id);

            entity.Property(d => d.Id)
                  .HasColumnName("id");

            entity.Property(d => d.Objetivo)
                  .HasColumnName("objetivo")
                  .HasMaxLength(30)
                  .IsRequired();

            entity.Property(d => d.Texto)
                  .HasColumnName("texto")
                  .HasMaxLength(400)
                  .IsRequired();

            entity.HasIndex(d => d.Objetivo);

            entity.HasData(
                // ==================== HIPERTROFIA (12 dicas) ====================
                new DicaObjetivo { Id = 1, Objetivo = "Hipertrofia", Texto = "Priorize a execução controlada na fase excêntrica (descida do peso) em cada repetição, mantendo a tensão mecânica constante no músculo-alvo para maximizar o estímulo de hipertrofia." },
                new DicaObjetivo { Id = 2, Objetivo = "Hipertrofia", Texto = "Busque a progressão gradual de carga ou de repetições semana após semana, sempre preservando a amplitude completa do movimento antes de aumentar o peso." },
                new DicaObjetivo { Id = 3, Objetivo = "Hipertrofia", Texto = "Respeite os intervalos de descanso prescritos entre as séries para recuperar sua capacidade de contração e manter um alto volume de carga útil durante toda a sessão." },
                new DicaObjetivo { Id = 4, Objetivo = "Hipertrofia", Texto = "O crescimento muscular acontece durante a recuperação: priorize de 7 a 9 horas de sono reparador por noite para otimizar a síntese proteica e a regeneração das fibras." },
                new DicaObjetivo { Id = 5, Objetivo = "Hipertrofia", Texto = "Mantenha uma ingestão adequada de proteínas fracionada ao longo do dia e hidratação constante para fornecer os nutrientes necessários à construção de massa magra." },
                new DicaObjetivo { Id = 6, Objetivo = "Hipertrofia", Texto = "Leve as séries de trabalho próximas à falha concêntrica com segurança técnica, evitando compensações posturais ou impulsos que roubem o esforço do músculo principal." },
                new DicaObjetivo { Id = 7, Objetivo = "Hipertrofia", Texto = "Concentre-se na conexão mente-músculo durante cada série, sentindo o alongamento completo e o pico de contração antes de iniciar a próxima repetição." },
                new DicaObjetivo { Id = 8, Objetivo = "Hipertrofia", Texto = "Realize o cardio pós-treino na intensidade moderada indicada na ficha para melhorar a sensibilidade à insulina e o condicionamento sem comprometer a recuperação muscular." },
                new DicaObjetivo { Id = 9, Objetivo = "Hipertrofia", Texto = "Não pule as séries iniciais de aquecimento articular e específico: elas preparam tendões e sistema nervoso para suportar cargas mais produtivas nas séries principais." },
                new DicaObjetivo { Id = 10, Objetivo = "Hipertrofia", Texto = "A constância semanal supera treinos isolados extremos: cumprir todas as sessões planejadas garante a frequência ideal de estímulo para cada grupamento muscular." },
                new DicaObjetivo { Id = 11, Objetivo = "Hipertrofia", Texto = "Anote as cargas utilizadas nos exercícios base do seu protocolo para acompanhar sua evolução real e identificar o momento exato de progredir nos treinos seguintes." },
                new DicaObjetivo { Id = 12, Objetivo = "Hipertrofia", Texto = "Evite treinar o mesmo grupo muscular em dias consecutivos fora da divisão prescrita, garantindo a supercompensação necessária para o ganho sólido de volume muscular." },

                // ==================== EMAGRECIMENTO (12 dicas) ====================
                new DicaObjetivo { Id = 13, Objetivo = "Emagrecimento", Texto = "Mantenha os intervalos de descanso curtos e rigorosos conforme prescrito na ficha para sustentar a frequência cardíaca elevada e ampliar o gasto calórico da sessão." },
                new DicaObjetivo { Id = 14, Objetivo = "Emagrecimento", Texto = "A musculação é essencial no emagrecimento: preservar e estimular sua massa magra mantém o metabolismo basal ativo e evita a flacidez durante a redução de gordura." },
                new DicaObjetivo { Id = 15, Objetivo = "Emagrecimento", Texto = "Combine o treino de força com o cardio pós-treino indicado no protocolo para potencializar a oxidação de gorduras aproveitando a depleção prévia de glicogênio." },
                new DicaObjetivo { Id = 16, Objetivo = "Emagrecimento", Texto = "Aumente seu nível de movimento fora da academia: caminhar mais no dia a dia, usar escadas e evitar longos períodos sentado acelera significativamente o gasto energético semanal." },
                new DicaObjetivo { Id = 17, Objetivo = "Emagrecimento", Texto = "Mantenha excelente hidratação antes, durante e após o treino; o consumo adequado de água otimiza o metabolismo energético, o desempenho físico e o controle da saciedade." },
                new DicaObjetivo { Id = 18, Objetivo = "Emagrecimento", Texto = "Não reduza drasticamente as cargas por estar em foco de perda de gordura: manter treinos intensos sinaliza ao organismo que a musculatura deve ser preservada." },
                new DicaObjetivo { Id = 19, Objetivo = "Emagrecimento", Texto = "Alinhe a regularidade dos treinos a uma alimentação equilibrada em déficit calórico sustentável, priorizando alimentos naturais, fibras e bom aporte de proteínas." },
                new DicaObjetivo { Id = 20, Objetivo = "Emagrecimento", Texto = "Cuide da qualidade do sono diariamente: noites mal dormidas desregulam hormônios ligados à fome e ao estresse, dificultando a queima de gordura e a disposição para treinar." },
                new DicaObjetivo { Id = 21, Objetivo = "Emagrecimento", Texto = "Evite compensar o gasto do treino com excessos alimentares: a consistência diária entre exercício e nutrição é o que constrói resultados duradouros na composição corporal." },
                new DicaObjetivo { Id = 22, Objetivo = "Emagrecimento", Texto = "Acompanhe sua evolução por medidas corporais, roupas e desempenho nos treinos, pois a balança isoladamente não diferencia perda de gordura de ganho de massa muscular." },
                new DicaObjetivo { Id = 23, Objetivo = "Emagrecimento", Texto = "Execute os exercícios multiarticulares com empenho e boa amplitude, pois movimentos que recrutam grandes grupos musculares demandam muito mais energia do organismo." },
                new DicaObjetivo { Id = 24, Objetivo = "Emagrecimento", Texto = "Mesmo nos dias de menor disposição, cumpra a sessão planejada: manter o hábito ativo é mais eficiente para o emagrecimento do que buscar perfeição apenas em alguns dias." },

                // ==================== FORÇA (12 dicas) ====================
                new DicaObjetivo { Id = 25, Objetivo = "Força", Texto = "Respeite integralmente os tempos mais longos de descanso entre as séries: eles são indispensáveis para restaurar os estoques de ATP-CP e o sistema nervoso central." },
                new DicaObjetivo { Id = 26, Objetivo = "Força", Texto = "Ative firmemente o core e controle a respiração (estabilização intra-abdominal) antes de cada repetição pesada para proteger a coluna e transferir mais força ao movimento." },
                new DicaObjetivo { Id = 27, Objetivo = "Força", Texto = "Realize séries progressivas de aquecimento específico no primeiro exercício do dia antes de entrar na carga máxima de trabalho, preparando articulações e recrutamento neural." },
                new DicaObjetivo { Id = 28, Objetivo = "Força", Texto = "Imprima intenção de velocidade na fase concêntrica (subida da carga) mantendo controle total na descida, maximizando o recrutamento das fibras musculares de maior potência." },
                new DicaObjetivo { Id = 29, Objetivo = "Força", Texto = "No treino de força, a qualidade técnica vem sempre antes do ego: interrompa a série caso perceba perda do alinhamento articular ou compensações perigosas na postura." },
                new DicaObjetivo { Id = 30, Objetivo = "Força", Texto = "Garanta pés bem plantados no solo, escápulas estabilizadas e pegada firme na barra ou equipamento antes de iniciar cada repetição para evitar dissipação de força." },
                new DicaObjetivo { Id = 31, Objetivo = "Força", Texto = "Pequenos incrementos de carga ao longo das semanas geram grandes ganhos de força a médio prazo sem sobrecarregar tendões, ligamentos e articulações." },
                new DicaObjetivo { Id = 32, Objetivo = "Força", Texto = "Priorize os exercícios multiarticulares livres ou guiados do início da ficha enquanto o sistema nervoso está descansado, pois são eles que constroem sua base de força." },
                new DicaObjetivo { Id = 33, Objetivo = "Força", Texto = "Mantenha o cardio leve conforme prescrito na ficha apenas como suporte cardiovascular e aquecimento, preservando suas reservas energéticas para as séries pesadas." },
                new DicaObjetivo { Id = 34, Objetivo = "Força", Texto = "Cuide da mobilidade de tornozelos, quadris e ombros regularmente: articulações móveis e estáveis permitem alavancas mecânicas muito mais seguras e eficientes sob carga." },
                new DicaObjetivo { Id = 35, Objetivo = "Força", Texto = "Valorize os dias de descanso da sua divisão semanal, pois adaptações neurais e estruturais de força exigem recuperação completa entre sessões intensas." },
                new DicaObjetivo { Id = 36, Objetivo = "Força", Texto = "Mantenha um bom aporte energético e hidratação adequada antes de treinar; estoques plenos de glicogênio muscular são fundamentais para sustentar cargas elevadas." },

                // ==================== RESISTÊNCIA (12 dicas) ====================
                new DicaObjetivo { Id = 37, Objetivo = "Resistência", Texto = "Mantenha um ritmo cadenciado e respiração fluida do início ao fim de cada série longa, evitando prender o ar para retardar a instalação precoce da fadiga periférica." },
                new DicaObjetivo { Id = 38, Objetivo = "Resistência", Texto = "Controle a escolha da carga para completar toda a faixa de repetições altas prescrita mantendo a postura impecável até a última repetição da série." },
                new DicaObjetivo { Id = 39, Objetivo = "Resistência", Texto = "Respeite os intervalos reduzidos de descanso entre as séries para estimular a capilarização muscular, a eficiência mitocondrial e a tolerância à acidose metabólica." },
                new DicaObjetivo { Id = 40, Objetivo = "Resistência", Texto = "Capriche na hidratação ao longo do dia e durante a sessão: a perda hídrica reduz rapidamente o rendimento em treinos de resistência muscular e aeróbica." },
                new DicaObjetivo { Id = 41, Objetivo = "Resistência", Texto = "Execute o cardio contínuo pós-treino na zona aeróbica orientada para fortalecer o sistema cardiorrespiratório e acelerar a remoção de metabólitos entre os esforços." },
                new DicaObjetivo { Id = 42, Objetivo = "Resistência", Texto = "Não encurte a amplitude das repetições quando a queimação muscular surgir: manter o movimento completo sob fadiga controlada é o que desenvolve resistência real." },
                new DicaObjetivo { Id = 43, Objetivo = "Resistência", Texto = "Garanta uma ingestão equilibrada de carboidratos de qualidade na rotina alimentar para manter os estoques de energia necessários às sessões de maior volume de repetições." },
                new DicaObjetivo { Id = 44, Objetivo = "Resistência", Texto = "Mantenha ombros, pescoço e musculaturas acessórias livres de tensão desnecessária durante o exercício para economizar energia e focar o esforço nos músculos-alvo." },
                new DicaObjetivo { Id = 45, Objetivo = "Resistência", Texto = "A evolução na resistência aparece quando o mesmo esforço passa a parecer mais leve e sua recuperação entre as séries se torna cada vez mais rápida." },
                new DicaObjetivo { Id = 46, Objetivo = "Resistência", Texto = "Alongamentos leves e exercícios de mobilidade após a sessão ajudam a aliviar a rigidez muscular e preservar a fluidez articular ao longo da semana de treinos." },
                new DicaObjetivo { Id = 47, Objetivo = "Resistência", Texto = "Mantenha regularidade absoluta na frequência semanal: adaptações aeróbicas e de resistência muscular respondem de forma excelente à continuidade dos estímulos." },
                new DicaObjetivo { Id = 48, Objetivo = "Resistência", Texto = "Preste atenção redobrada à estabilidade do core no final das séries longas, garantindo que a fadiga não comprometa o alinhamento da coluna lombar." }
            );
        });

        // 7. Mapeamento da tabela 'treinos' (Cabeçalho da Ficha de Treino gerada para o Aluno)
        modelBuilder.Entity<Treino>(entity =>
        {
            entity.ToTable("treinos");

            entity.HasKey(t => t.Id);

            entity.Property(t => t.Id)
                  .HasColumnName("id");

            entity.Property(t => t.AlunoId)
                  .HasColumnName("aluno_id")
                  .IsRequired();

            entity.Property(t => t.UsuarioId)
                  .HasColumnName("usuario_id");

            entity.Property(t => t.DicaObjetivoId)
                  .HasColumnName("dica_objetivo_id");

            entity.Property(t => t.NomePlano)
                  .HasColumnName("nome_plano")
                  .HasMaxLength(150)
                  .IsRequired();

            entity.Property(t => t.Divisao)
                  .HasColumnName("divisao")
                  .HasMaxLength(80)
                  .IsRequired();

            entity.Property(t => t.TempoAerobico)
                  .HasColumnName("tempo_aerobico")
                  .HasMaxLength(120)
                  .IsRequired();

            entity.Property(t => t.Status)
                  .HasColumnName("status")
                  .HasMaxLength(30)
                  .HasDefaultValue("Rascunho")
                  .IsRequired();

            entity.Property(t => t.DataCriacao)
                  .HasColumnName("data_criacao")
                  .IsRequired();

            entity.Property(t => t.Ativo)
                  .HasColumnName("ativo")
                  .HasDefaultValue(true)
                  .IsRequired();

            entity.HasOne(t => t.Aluno)
                  .WithMany()
                  .HasForeignKey(t => t.AlunoId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne<Usuario>()
                  .WithMany()
                  .HasForeignKey(t => t.UsuarioId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(t => t.DicaObjetivo)
                  .WithMany()
                  .HasForeignKey(t => t.DicaObjetivoId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasMany(t => t.Itens)
                  .WithOne(i => i.Treino)
                  .HasForeignKey(i => i.TreinoId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // 8. Mapeamento da tabela 'itens_treino' (ItemTreino: id_item, divisao_treino, ordem_exercicio, series, repeticao, tempo_descanso, tempo_aerobico + FKs para exercicios e maquinas)
        modelBuilder.Entity<ItemTreino>(entity =>
        {
            entity.ToTable("itens_treino");

            entity.HasKey(i => i.Id);

            entity.Property(i => i.Id)
                  .HasColumnName("id_item");

            entity.Property(i => i.TreinoId)
                  .HasColumnName("treino_id")
                  .IsRequired();

            entity.Property(i => i.DivisaoTreino)
                  .HasColumnName("divisao_treino")
                  .HasMaxLength(100)
                  .IsRequired();

            entity.Property(i => i.OrdemExercicio)
                  .HasColumnName("ordem_exercicio")
                  .IsRequired();

            entity.Property(i => i.ExercicioId)
                  .HasColumnName("exercicio_id");

            entity.Property(i => i.MaquinaId)
                  .HasColumnName("maquina_id");

            entity.Property(i => i.Series)
                  .HasColumnName("series")
                  .IsRequired();

            entity.Property(i => i.Repeticao)
                  .HasColumnName("repeticao")
                  .HasMaxLength(30)
                  .IsRequired();

            entity.Property(i => i.TempoDescanso)
                  .HasColumnName("tempo_descanso")
                  .HasMaxLength(30)
                  .IsRequired();

            entity.Property(i => i.TempoAerobico)
                  .HasColumnName("tempo_aerobico")
                  .HasDefaultValue(0)
                  .IsRequired();

            entity.HasOne(i => i.Exercicio)
                  .WithMany()
                  .HasForeignKey(i => i.ExercicioId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(i => i.Maquina)
                  .WithMany()
                  .HasForeignKey(i => i.MaquinaId)
                  .OnDelete(DeleteBehavior.SetNull);
        });
    }
}
