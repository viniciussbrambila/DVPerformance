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

            entity.HasOne<Usuario>()
                  .WithMany()
                  .HasForeignKey(a => a.UsuarioId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
