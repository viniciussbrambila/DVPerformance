using DVPerformance.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace DVPerformance.Api.Endpoints;

public static class DashboardEndpoints
{
    public static void MapDashboardEndpoints(this WebApplication app)
    {
        var grupo = app.MapGroup("/dashboard");

        // GET /dashboard/resumo?usuarioId=1 -> Retorna as contagens do painel em uma única consulta leve no PostgreSQL
        grupo.MapGet("/resumo", async (int? usuarioId, AppDbContext db) =>
        {
            if (usuarioId.HasValue && usuarioId.Value > 0)
            {
                var resumo = await db.Usuarios
                    .AsNoTracking()
                    .Where(u => u.Id == usuarioId.Value)
                    .Select(u => new
                    {
                        totalAlunos = db.Alunos.Count(a => a.Ativo && a.UsuarioId == u.Id),
                        totalPlanos = db.Treinos.Count(t =>
                            t.Ativo &&
                            t.UsuarioId == u.Id &&
                            t.Aluno != null &&
                            t.Aluno.Ativo)
                    })
                    .FirstOrDefaultAsync();

                return Results.Ok(resumo ?? new { totalAlunos = 0, totalPlanos = 0 });
            }

            var totalAlunosAtivos = await db.Alunos.AsNoTracking().CountAsync(a => a.Ativo);
            var totalPlanosAtivos = await db.Treinos.AsNoTracking()
                .CountAsync(t => t.Ativo && t.Aluno != null && t.Aluno.Ativo);

            return Results.Ok(new
            {
                totalAlunos = totalAlunosAtivos,
                totalPlanos = totalPlanosAtivos
            });
        });
    }
}
