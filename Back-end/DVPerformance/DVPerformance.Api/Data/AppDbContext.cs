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

            entity.Property(u => u.ChaveRecuperacaoHash)
                  .HasColumnName("chave_recuperacao_hash")
                  .HasMaxLength(100);

            entity.Property(u => u.ChaveRecuperacaoCifrada)
                  .HasColumnName("chave_recuperacao_cifrada")
                  .HasMaxLength(200);

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

            entity.Property(a => a.DataDeNascimento)
                  .HasColumnName("data_de_nascimento")
                  .HasColumnType("date");

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
            // 1 = Grandes | 2 = Bíceps, tríceps, ombros e adutores/abdutores | 3 = Trapézio e panturrilha | 4 = Finalizadores
            entity.HasData(
                new GrupoMuscular { Id = 1, Nome = "Peito", Regiao = "Membros Superiores", OrdemPrioridade = 1 },
                new GrupoMuscular { Id = 2, Nome = "Costas", Regiao = "Membros Superiores", OrdemPrioridade = 1 },
                new GrupoMuscular { Id = 3, Nome = "Quadríceps", Regiao = "Membros Inferiores", OrdemPrioridade = 1 },
                new GrupoMuscular { Id = 4, Nome = "Posterior de Coxa", Regiao = "Membros Inferiores", OrdemPrioridade = 1 },
                new GrupoMuscular { Id = 5, Nome = "Glúteos", Regiao = "Membros Inferiores", OrdemPrioridade = 1 },
                new GrupoMuscular { Id = 6, Nome = "Ombros", Regiao = "Membros Superiores", OrdemPrioridade = 2 },
                new GrupoMuscular { Id = 7, Nome = "Trapézio", Regiao = "Membros Superiores", OrdemPrioridade = 3 },
                new GrupoMuscular { Id = 8, Nome = "Adutores / Abdutores", Regiao = "Membros Inferiores", OrdemPrioridade = 2 },
                new GrupoMuscular { Id = 9, Nome = "Bíceps", Regiao = "Membros Superiores", OrdemPrioridade = 2 },
                new GrupoMuscular { Id = 10, Nome = "Tríceps", Regiao = "Membros Superiores", OrdemPrioridade = 2 },
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

            entity.Property(e => e.FocoMuscular)
                  .HasColumnName("foco_muscular")
                  .HasMaxLength(60)
                  .IsRequired();

            entity.Property(e => e.NivelMinimo)
                  .HasColumnName("nivel_minimo")
                  .HasMaxLength(20)
                  .HasDefaultValue("Iniciante")
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

            entity.Property(m => m.FocoMuscular)
                  .HasColumnName("foco_muscular")
                  .HasMaxLength(60)
                  .IsRequired();

            entity.Property(m => m.NivelMinimo)
                  .HasColumnName("nivel_minimo")
                  .HasMaxLength(20)
                  .HasDefaultValue("Iniciante")
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
                new DicaObjetivo { Id = 1, Objetivo = "Hipertrofia", Texto = "Segure a descida do peso com controle em cada repetição. Não deixe o peso despencar: controlar a volta força o músculo a trabalhar o dobro e gera muito mais resultado." },
                new DicaObjetivo { Id = 2, Objetivo = "Hipertrofia", Texto = "Tente aumentar um pouco o peso ou fazer 1 ou 2 repetições a mais com o passar das semanas, mas só suba a carga quando conseguir fazer o movimento completo e bem feito." },
                new DicaObjetivo { Id = 3, Objetivo = "Hipertrofia", Texto = "Respeite o tempo de descanso marcado na ficha. Descansar o tempo certo recupera sua energia para você conseguir pegar firme na próxima série." },
                new DicaObjetivo { Id = 4, Objetivo = "Hipertrofia", Texto = "O músculo cresce quando você descansa, não durante o treino. Procure dormir de 7 a 9 horas por noite para seu corpo se recuperar e construir massa muscular." },
                new DicaObjetivo { Id = 5, Objetivo = "Hipertrofia", Texto = "Inclua boas fontes de proteína nas suas refeições ao longo do dia e beba bastante água: é disso que seu corpo precisa para construir músculos." },
                new DicaObjetivo { Id = 6, Objetivo = "Hipertrofia", Texto = "Treine até sentir que o músculo realmente cansou nas últimas repetições, mas sem roubar no movimento ou dar impulsos com o corpo para levantar o peso." },
                new DicaObjetivo { Id = 7, Objetivo = "Hipertrofia", Texto = "Foque no músculo que está treinando em vez de apenas mover o peso no automático. Sinta ele alongar e contrair bem a cada repetição." },
                new DicaObjetivo { Id = 8, Objetivo = "Hipertrofia", Texto = "Faça o cardio depois da musculação no ritmo moderado indicado na ficha. Isso melhora seu fôlego e ajuda o corpo a absorver melhor os nutrientes sem atrapalhar o ganho de massa." },
                new DicaObjetivo { Id = 9, Objetivo = "Hipertrofia", Texto = "Nunca pule o aquecimento antes de pegar pesado. Fazer 1 ou 2 séries leves prepara suas articulações, evita lesões e melhora sua força no treino." },
                new DicaObjetivo { Id = 10, Objetivo = "Hipertrofia", Texto = "Mais vale treinar bem toda semana do que fazer um treino exagerado e faltar nos outros dias. A regularidade é o que traz resultados visíveis." },
                new DicaObjetivo { Id = 11, Objetivo = "Hipertrofia", Texto = "Anote os pesos que você usa nos exercícios principais. Assim você acompanha sua evolução de verdade e sabe exatamente quando está na hora de aumentar a carga." },
                new DicaObjetivo { Id = 12, Objetivo = "Hipertrofia", Texto = "Não treine o mesmo músculo em dias seguidos. Siga a divisão da sua ficha para dar o descanso necessário para o músculo crescer mais forte." },

                // ==================== EMAGRECIMENTO (12 dicas) ====================
                new DicaObjetivo { Id = 13, Objetivo = "Emagrecimento", Texto = "Fique de olho no relógio e respeite os descansos curtos da ficha. Isso mantém seus batimentos acelerados e faz você gastar muito mais calorias no treino." },
                new DicaObjetivo { Id = 14, Objetivo = "Emagrecimento", Texto = "Musculação também emagrece! Manter e ganhar músculos deixa seu metabolismo mais acelerado até em repouso e ajuda a evitar a flacidez enquanto você perde peso." },
                new DicaObjetivo { Id = 15, Objetivo = "Emagrecimento", Texto = "Fazer o cardio logo após a musculação turbina a queima de gordura, já que seu corpo já gastou a energia rápida durante os exercícios com peso." },
                new DicaObjetivo { Id = 16, Objetivo = "Emagrecimento", Texto = "Mexa-se mais fora da academia: troque o elevador pela escada, caminhe sempre que puder e evite ficar sentado por horas. Esses pequenos hábitos fazem muita diferença na balança." },
                new DicaObjetivo { Id = 17, Objetivo = "Emagrecimento", Texto = "Beba bastante água ao longo do dia e durante o treino. Além de melhorar sua disposição, a água ajuda o metabolismo a funcionar bem e controla a sensação de fome." },
                new DicaObjetivo { Id = 18, Objetivo = "Emagrecimento", Texto = "Não pegue leve demais nos pesos só porque quer emagrecer. Treinar com intensidade mostra ao seu corpo que ele precisa queimar gordura, e não os seus músculos." },
                new DicaObjetivo { Id = 19, Objetivo = "Emagrecimento", Texto = "O treino anda junto com a alimentação: prefira comidas naturais, frutas, verduras e boas proteínas no dia a dia, sem precisar passar fome com dietas radicais." },
                new DicaObjetivo { Id = 20, Objetivo = "Emagrecimento", Texto = "Dormir bem ajuda a emagrecer: noites mal dormidas aumentam o estresse e a vontade de comer doces e besteiras no dia seguinte, além de tirarem sua energia para treinar." },
                new DicaObjetivo { Id = 21, Objetivo = "Emagrecimento", Texto = "Cuidado para não exagerar na comida achando que 'já queimou tudo na academia'. É o equilíbrio diário entre treino e alimentação que transforma seu corpo." },
                new DicaObjetivo { Id = 22, Objetivo = "Emagrecimento", Texto = "Não se prenda só ao número da balança! Observe como suas roupas estão vestindo, suas medidas e sua disposição, pois você pode estar trocando gordura por músculo." },
                new DicaObjetivo { Id = 23, Objetivo = "Emagrecimento", Texto = "Capriche nos exercícios que trabalham vários músculos ao mesmo tempo (como agachamento, leg press e puxadas). Eles exigem mais do corpo e queimam muito mais calorias." },
                new DicaObjetivo { Id = 24, Objetivo = "Emagrecimento", Texto = "Nos dias de preguiça, vá treinar mesmo assim, nem que seja em um ritmo mais leve. Manter o hábito ativo vale muito mais do que esperar pelo dia perfeito." },

                // ==================== FORÇA (12 dicas) ====================
                new DicaObjetivo { Id = 25, Objetivo = "Força", Texto = "No treino de força, o descanso maior entre as séries é obrigatório. Espere o tempo completo marcado na ficha para recuperar totalmente a energia antes de levantar pesado de novo." },
                new DicaObjetivo { Id = 26, Objetivo = "Força", Texto = "Trave bem o abdômen e respire fundo antes de iniciar cada repetição pesada. Um tronco firme protege sua coluna e ajuda você a empurrar ou puxar mais peso com segurança." },
                new DicaObjetivo { Id = 27, Objetivo = "Força", Texto = "Antes de colocar sua carga máxima no primeiro exercício, faça algumas séries de aquecimento aumentando o peso aos poucos. Isso prepara seu corpo e suas articulações para o esforço." },
                new DicaObjetivo { Id = 28, Objetivo = "Força", Texto = "Desça o peso com total controle e faça força para subir com firmeza e explosão. Essa intenção de velocidade ativa mais fibras musculares e aumenta sua força." },
                new DicaObjetivo { Id = 29, Objetivo = "Força", Texto = "Técnica sempre vem antes do peso: se perceber que a postura entortou ou que você está forçando a coluna para subir a carga, pare a série e ajuste o peso." },
                new DicaObjetivo { Id = 30, Objetivo = "Força", Texto = "Antes de começar o movimento, firme bem os pés no chão, encaixe os ombros e segure a barra com firmeza. Uma base estável evita desperdício de energia." },
                new DicaObjetivo { Id = 31, Objetivo = "Força", Texto = "Não tenha pressa de dobrar os pesos. Aumentar de 1 kg em 1 kg ou 2 kg em 2 kg toda semana gera um ganho enorme de força sem machucar suas articulações." },
                new DicaObjetivo { Id = 32, Objetivo = "Força", Texto = "Dê o seu máximo nos primeiros exercícios da ficha (como supino, agachamento e remadas), pois você ainda está descansado e são eles que constroem sua base de força." },
                new DicaObjetivo { Id = 33, Objetivo = "Força", Texto = "Mantenha o cardio em ritmo leve, apenas como indicado na ficha. O objetivo aqui é cuidar do coração sem gastar a energia que você precisa para levantar cargas altas." },
                new DicaObjetivo { Id = 34, Objetivo = "Força", Texto = "Faça exercícios de mobilidade para ombros, quadris e tornozelos. Articulações soltas e flexíveis permitem movimentos mais profundos, seguros e com muito mais força." },
                new DicaObjetivo { Id = 35, Objetivo = "Força", Texto = "Respeite os dias de descanso da semana. Levantar cargas altas exige muito do corpo, e é no repouso que você realmente fica mais forte para o próximo treino." },
                new DicaObjetivo { Id = 36, Objetivo = "Força", Texto = "Não vá treinar força em jejum ou desidratado. Alimente-se bem e beba água antes do treino para ter energia suficiente na hora de encarar os pesos maiores." },

                // ==================== RESISTÊNCIA (12 dicas) ====================
                new DicaObjetivo { Id = 37, Objetivo = "Resistência", Texto = "Mantenha um ritmo constante e respire sem prender o ar durante as séries longas. Segurar a respiração faz você cansar muito mais rápido." },
                new DicaObjetivo { Id = 38, Objetivo = "Resistência", Texto = "Escolha um peso que permita completar todas as repetições altas da ficha mantendo a postura correta do começo ao fim, sem precisar parar no meio da série." },
                new DicaObjetivo { Id = 39, Objetivo = "Resistência", Texto = "Respeite os intervalos curtos de descanso entre as séries. É exatamente treinar com o músculo ainda um pouco cansado que aumenta sua resistência física." },
                new DicaObjetivo { Id = 40, Objetivo = "Resistência", Texto = "Beba água antes, durante e depois do treino. Em treinos longos e com muitas repetições, a falta de água derruba seu fôlego e sua energia rapidamente." },
                new DicaObjetivo { Id = 41, Objetivo = "Resistência", Texto = "Faça o cardio contínuo após o treino no ritmo indicado na ficha. Ele fortalece seu coração, aumenta o fôlego e ajuda seu corpo a se recuperar mais rápido do cansaço." },
                new DicaObjetivo { Id = 42, Objetivo = "Resistência", Texto = "Quando o músculo começar a queimar nas repetições finais, não encurte o movimento. Manter a amplitude completa mesmo cansado é o que desenvolve resistência de verdade." },
                new DicaObjetivo { Id = 43, Objetivo = "Resistência", Texto = "Inclua carboidratos saudáveis na sua alimentação (como arroz, aveia, frutas e raízes). Eles são o combustível principal para aguentar treinos longos sem perder o ritmo." },
                new DicaObjetivo { Id = 44, Objetivo = "Resistência", Texto = "Relaxe o pescoço e não tensione os ombros à toa durante os exercícios. Evitar tensão desnecessária economiza energia para você ir mais longe na série." },
                new DicaObjetivo { Id = 45, Objetivo = "Resistência", Texto = "Você sabe que está evoluindo quando o mesmo treino começa a parecer mais fácil e sua respiração volta ao normal cada vez mais rápido entre uma série e outra." },
                new DicaObjetivo { Id = 46, Objetivo = "Resistência", Texto = "Tire 5 minutinhos para fazer alongamentos leves ao final do treino. Isso alivia a rigidez muscular e deixa seu corpo mais solto para as próximas sessões da semana." },
                new DicaObjetivo { Id = 47, Objetivo = "Resistência", Texto = "A regularidade é o segredo do fôlego: quanto mais constante você for na semana, mais rápido seu corpo se adapta e demora mais para cansar." },
                new DicaObjetivo { Id = 48, Objetivo = "Resistência", Texto = "Nas últimas repetições, quando o cansaço bater, redobre a atenção na postura e mantenha o abdômen firme para não sobrecarregar a lombar." }
            );
        });

        // 7. Mapeamento da tabela 'treinos' (Cabeçalho da Ficha de Treino gerada para o Aluno)
        modelBuilder.Entity<Treino>(entity =>
        {
            entity.ToTable("treinos", tabela => tabela.HasCheckConstraint(
                "ck_treinos_dicas_distintas",
                "\"dica_objetivo_id\" <> \"segunda_dica_objetivo_id\""));

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

            entity.Property(t => t.SegundaDicaObjetivoId)
                  .HasColumnName("segunda_dica_objetivo_id");

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

            entity.HasOne(t => t.SegundaDicaObjetivo)
                  .WithMany()
                  .HasForeignKey(t => t.SegundaDicaObjetivoId)
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
