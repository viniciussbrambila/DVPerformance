using System.Text.Json.Serialization;
using DVPerformance.Api.Data;
using DVPerformance.Api.Endpoints;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.EntityFrameworkCore;

namespace DVPerformance.Api;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // 1. Configuração de Serviços
        builder.Services.AddAuthorization();
        builder.Services.AddOpenApi();

        // Configuração do Banco de Dados PostgreSQL
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
        builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString));

        // Bloqueia envio de texto em campos numéricos (ex: "idade": "25" ou "idade": "abc")
        builder.Services.Configure<JsonOptions>(options =>
        {
            options.SerializerOptions.NumberHandling = JsonNumberHandling.Strict;
        });

        // Permite que o Front-end (React/Vite) faça requisições para esta API
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("PermitirFrontEnd", policy =>
            {
                policy.AllowAnyOrigin()
                      .AllowAnyHeader()
                      .AllowAnyMethod();
            });
        });

        var app = builder.Build();

        // Aplica as Migrations automaticamente ao iniciar a API e provisiona o catálogo pré-cadastrado
        // para todas as contas antigas já existentes no banco de dados
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await db.Database.MigrateAsync();
            await CatalogoPadraoService.InicializarContasPendentesAsync(db);
        }

        // 2. Middleware para capturar erros de tipo no JSON (ex: enviar texto em campo numérico)
        app.Use(async (context, next) =>
        {
            try
            {
                await next(context);
            }
            catch (BadHttpRequestException ex)
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsJsonAsync(new
                {
                    mensagem = "Formato de dados inválido. Verifique se campos numéricos (como idade e frequência semanal) contêm apenas números inteiros e se os campos de texto estão entre aspas.",
                    detalhe = ex.Message
                });
            }
        });

        // 3. Configuração do Pipeline HTTP (Middlewares)
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.UseCors("PermitirFrontEnd");
        app.UseAuthorization();

        // 4. Mapeamento dos Endpoints (Minimal APIs organizadas por arquivo)
        app.MapAuthEndpoints();
        app.MapAlunoEndpoints();
        app.MapExercicioEndpoints();
        app.MapMaquinaEndpoints();
        app.MapTreinoEndpoints();

        await app.RunAsync();
    }
}
