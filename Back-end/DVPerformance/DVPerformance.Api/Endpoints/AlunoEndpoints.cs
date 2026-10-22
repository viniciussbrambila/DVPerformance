using DVPerformance.Api.Models;

namespace DVPerformance.Api.Endpoints;

public static class AlunoEndpoints
{
    // Lista em memória (começa vazia; os alunos serão cadastrados pelo Personal)
    private static readonly List<Aluno> Alunos = new();

    public static void MapAlunoEndpoints(this WebApplication app)
    {
        // Cria um grupo de rotas com o prefixo "/alunos"
        var grupo = app.MapGroup("/alunos");

        // GET /alunos -> Lista todos os alunos cadastrados
        grupo.MapGet("/", () =>
        {
            return Results.Ok(Alunos);
        });

        // GET /alunos/{id} -> Busca um aluno específico pelo Id
        grupo.MapGet("/{id:int}", (int id) =>
        {
            var aluno = Alunos.FirstOrDefault(a => a.Id == id);
            if (aluno is null)
            {
                return Results.NotFound(new { mensagem = "Aluno não encontrado." });
            }

            return Results.Ok(aluno);
        });

        // POST /alunos -> Cadastra um novo aluno enviado pelo Personal
        grupo.MapPost("/", (Aluno novoAluno) =>
        {
            if (string.IsNullOrWhiteSpace(novoAluno.Nome) || novoAluno.Idade <= 0)
            {
                return Results.BadRequest(new { mensagem = "Nome e idade válida são obrigatórios." });
            }

            novoAluno.Id = Alunos.Count > 0 ? Alunos.Max(a => a.Id) + 1 : 1;
            Alunos.Add(novoAluno);

            return Results.Created($"/alunos/{novoAluno.Id}", novoAluno);
        });

        // PUT /alunos/{id} -> Atualiza os dados de um aluno existente
        grupo.MapPut("/{id:int}", (int id, Aluno dadosAtualizados) =>
        {
            var alunoExistente = Alunos.FirstOrDefault(a => a.Id == id);
            if (alunoExistente is null)
            {
                return Results.NotFound(new { mensagem = "Aluno não encontrado." });
            }

            alunoExistente.Nome = dadosAtualizados.Nome;
            alunoExistente.Idade = dadosAtualizados.Idade;
            alunoExistente.Sexo = dadosAtualizados.Sexo;
            alunoExistente.Experiencia = dadosAtualizados.Experiencia;
            alunoExistente.FrequenciaSemanal = dadosAtualizados.FrequenciaSemanal;
            alunoExistente.Objetivo = dadosAtualizados.Objetivo;

            return Results.Ok(alunoExistente);
        });

        // DELETE /alunos/{id} -> Remove um aluno pelo Id
        grupo.MapDelete("/{id:int}", (int id) =>
        {
            var aluno = Alunos.FirstOrDefault(a => a.Id == id);
            if (aluno is null)
            {
                return Results.NotFound(new { mensagem = "Aluno não encontrado." });
            }

            Alunos.Remove(aluno);
            return Results.NoContent();
        });
    }
}
