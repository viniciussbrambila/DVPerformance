using DVPerformance.Api.Endpoints;

namespace DVPerformance.Api;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // 1. Configuração de Serviços
        builder.Services.AddAuthorization();
        builder.Services.AddOpenApi();

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

        // 2. Configuração do Pipeline HTTP (Middlewares)
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.UseCors("PermitirFrontEnd");
        app.UseAuthorization();

        // 3. Mapeamento dos Endpoints (Minimal APIs organizadas por arquivo)
        app.MapAlunoEndpoints();

        app.Run();
    }
}
