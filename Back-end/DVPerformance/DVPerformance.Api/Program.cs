using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using DVPerformance.Api.Data;
using DVPerformance.Api.Endpoints;
using DVPerformance.Api.Security;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

namespace DVPerformance.Api;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // 1. Configuração de Serviços e Segurança
        SegurancaService.ConfigurarSegredo(builder.Configuration["Security:JwtSecret"]);

        builder.Services.AddAuthorization();
        builder.Services.AddOpenApi();

        // Limite de requisições por minuto nas rotas de autenticação (/auth/*) contra ataques de força bruta
        builder.Services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.OnRejected = async (context, cancellationToken) =>
            {
                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                await context.HttpContext.Response.WriteAsJsonAsync(new
                {
                    mensagem = "Muitas tentativas consecutivas. Aguarde um minuto antes de tentar novamente.",
                    erros = new[] { "Limite de segurança atingido para proteção da sua conta." }
                }, cancellationToken);
            };

            options.AddPolicy("AuthRateLimit", httpContext =>
            {
                var ipCliente = httpContext.Connection.RemoteIpAddress?.ToString() ?? "local";
                return RateLimitPartition.GetFixedWindowLimiter(ipCliente, _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 20,
                    Window = TimeSpan.FromMinutes(1),
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    QueueLimit = 0
                });
            });
        });

        // Configuração do Banco de Dados PostgreSQL
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
        builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString));

        // Bloqueia envio de texto em campos numéricos (ex: "frequenciaSemanal": "3")
        builder.Services.Configure<JsonOptions>(options =>
        {
            options.SerializerOptions.NumberHandling = JsonNumberHandling.Strict;
        });

        // Permite apenas origens explícitas do Front-end (React/Vite) configuradas ou localhost
        var origensPermitidas = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();
        if (origensPermitidas is null || origensPermitidas.Length == 0)
        {
            origensPermitidas =
            [
                "http://localhost:5173",
                "http://127.0.0.1:5173",
                "http://localhost:4173",
                "http://127.0.0.1:4173",
                "http://localhost:3000"
            ];
        }

        builder.Services.AddCors(options =>
        {
            options.AddPolicy("PermitirFrontEnd", policy =>
            {
                policy.WithOrigins(origensPermitidas)
                      .AllowAnyHeader()
                      .AllowAnyMethod();
            });
        });

        var app = builder.Build();

        // Aplica as Migrations automaticamente ao iniciar a API e provisiona o catálogo pré-cadastrado
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await db.Database.MigrateAsync();
            await CatalogoPadraoService.InicializarContasPendentesAsync(db);
            await CatalogoPadraoService.ClassificarFocosPendentesAsync(db);
        }

        // 2. Middleware de cabeçalhos de segurança HTTP e captura padronizada de erros de JSON (sem vazar detalhes internos)
        app.Use(async (context, next) =>
        {
            context.Response.Headers["X-Content-Type-Options"] = "nosniff";
            context.Response.Headers["X-Frame-Options"] = "DENY";
            context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";

            try
            {
                await next(context);
            }
            catch (BadHttpRequestException)
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsJsonAsync(new
                {
                    mensagem = "Formato de dados inválido. Envie a data de nascimento no formato AAAA-MM-DD, a frequência semanal como número inteiro e os campos de texto entre aspas."
                });
            }
        });

        // 3. Configuração do Pipeline HTTP (Middlewares)
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.UseCors("PermitirFrontEnd");
        app.UseRateLimiter();

        // Middleware de validação de Token Bearer (HMAC-SHA256)
        app.Use(async (context, next) =>
        {
            var authHeader = context.Request.Headers.Authorization.ToString();
            if (!string.IsNullOrWhiteSpace(authHeader) &&
                authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                var token = authHeader["Bearer ".Length..].Trim();
                if (!SegurancaService.TentarValidarTokenSessao(token, out var usuarioAutenticado) ||
                    usuarioAutenticado is null)
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsJsonAsync(new
                    {
                        mensagem = "Sessão inválida ou expirada. Faça login novamente.",
                        erros = new[] { "O token de autenticação enviado é inválido ou expirou." }
                    });
                    return;
                }

                AutorizacaoEndpoints.AplicarClaimsNoContexto(context, usuarioAutenticado);
            }

            await next(context);
        });

        app.UseAuthorization();

        // 4. Mapeamento dos Endpoints (Minimal APIs organizadas por arquivo)
        app.MapAuthEndpoints();
        app.MapDashboardEndpoints();
        app.MapAlunoEndpoints();
        app.MapExercicioEndpoints();
        app.MapMaquinaEndpoints();
        app.MapTreinoEndpoints();

        await app.RunAsync();
    }

}
