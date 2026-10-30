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

        // 6. Mapeamento da tabela 'treinos' (Cabeçalho da Ficha de Treino gerada para o Aluno)
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

            entity.Property(t => t.DataCriacao)
                  .HasColumnName("data_criacao")
                  .IsRequired();

            entity.Property(t => t.Ativo)
                  .HasColumnName("ativo")
                  .HasDefaultValue(true)
                  .IsRequired();

            entity.HasOne<Aluno>()
                  .WithMany()
                  .HasForeignKey(t => t.AlunoId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne<Usuario>()
                  .WithMany()
                  .HasForeignKey(t => t.UsuarioId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasMany(t => t.Itens)
                  .WithOne(i => i.Treino)
                  .HasForeignKey(i => i.TreinoId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // 7. Mapeamento da tabela 'itens_treino' (ItemTreino: id_item, divisao_treino, ordem_exercicio, series, repeticao, tempo_descanso, tempo_aerobico + FKs para exercicios e maquinas)
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
                  .HasMaxLength(120)
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
