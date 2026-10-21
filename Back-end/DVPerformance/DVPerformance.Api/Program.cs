using DVPerformance.Api.Models;
namespace DVPerformance.Api;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddAuthorization();

        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi();

        var app = builder.Build();
        app.MapGet("/Alunos", () =>
        {
            var aluno = new Aluno
            {
                Id = 1,
                Nome = "João Silva",
                Sexo = "Masculino",
                Idade = 25,
                Experiencia = "Iniciante",
                FrequenciaSemanal = 4,
                Objetivo = "Hipertrofia"
            };

            return aluno;
        });

       
        app.Run();
    }
}