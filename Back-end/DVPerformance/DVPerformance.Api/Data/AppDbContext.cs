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

    public DbSet<Exercicio> Exercicios { get; set; }

    public DbSet<Maquina> Maquinas { get; set; }

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

        // 3. Mapeamento da tabela 'exercicios' (Catálogo de Exercícios separado por Região e Grupo Muscular)
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

            entity.Property(e => e.Regiao)
                  .HasColumnName("regiao")
                  .HasMaxLength(40)
                  .IsRequired();

            entity.Property(e => e.Grupo)
                  .HasColumnName("grupo")
                  .HasMaxLength(50)
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

            entity.HasOne<Usuario>()
                  .WithMany()
                  .HasForeignKey(e => e.UsuarioId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // 4. Mapeamento da tabela 'maquinas' (Dicionário de Dados pág. 21: id, nome, grupo, descricao + padronização)
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

            entity.Property(m => m.Regiao)
                  .HasColumnName("regiao")
                  .HasMaxLength(40)
                  .IsRequired();

            entity.Property(m => m.Grupo)
                  .HasColumnName("grupo")
                  .HasMaxLength(50)
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

            entity.HasOne<Usuario>()
                  .WithMany()
                  .HasForeignKey(m => m.UsuarioId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
