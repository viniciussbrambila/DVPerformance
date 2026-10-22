using System.Text.Json.Serialization;
using DVPerformance.Api.Endpoints;
using Microsoft.AspNetCore.Http.Json;

namespace DVPerformance.Api;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // 1. Configuração de Serviços
        builder.Services.AddAuthorization();
        builder.Services.AddOpenApi();

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
        app.MapAlunoEndpoints();

        app.Run();
    }
}
