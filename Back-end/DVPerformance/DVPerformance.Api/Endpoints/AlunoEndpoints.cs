using System.Text.RegularExpressions;
using DVPerformance.Api.Models;

namespace DVPerformance.Api.Endpoints;

public static class AlunoEndpoints
{
    // Lista em memória (começa vazia; os alunos serão cadastrados pelo Personal)
    private static readonly List<Aluno> Alunos = new();

    // Opções exatas dos campos de seleção (Select) do Front-end
    private static readonly HashSet<string> SexosPermitidos = new()
    {
        "M", "F"
    };

    private static readonly HashSet<string> ExperienciasPermitidas = new()
    {
        "Iniciante", "Intermediário", "Avançado"
    };

    private static readonly HashSet<string> ObjetivosPermitidos = new()
    {
        "Hipertrofia", "Emagrecimento", "Força", "Condicionamento", "Resistência"
    };

    // Expressão regular que aceita apenas letras (incluindo acentuadas) e espaços
    private static readonly Regex ApenasLetrasRegex = new(@"^[\p{L}\s]+$", RegexOptions.Compiled);

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
            if (id <= 0)
            {
                return Results.BadRequest(new { mensagem = "O ID do aluno deve ser um número inteiro positivo." });
            }

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
            var erros = ValidarAluno(novoAluno);
            if (erros.Count > 0)
            {
                return Results.BadRequest(new
                {
                    mensagem = "Dados inválidos no cadastro do aluno.",
                    erros
                });
            }

            novoAluno.Nome = novoAluno.Nome.Trim();
            novoAluno.Id = Alunos.Count > 0 ? Alunos.Max(a => a.Id) + 1 : 1;
            Alunos.Add(novoAluno);

            return Results.Created($"/alunos/{novoAluno.Id}", novoAluno);
        });

        // PUT /alunos/{id} -> Atualiza os dados de um aluno existente
        grupo.MapPut("/{id:int}", (int id, Aluno dadosAtualizados) =>
        {
            if (id <= 0)
            {
                return Results.BadRequest(new { mensagem = "O ID do aluno deve ser um número inteiro positivo." });
            }

            var alunoExistente = Alunos.FirstOrDefault(a => a.Id == id);
            if (alunoExistente is null)
            {
                return Results.NotFound(new { mensagem = "Aluno não encontrado." });
            }

            var erros = ValidarAluno(dadosAtualizados);
            if (erros.Count > 0)
            {
                return Results.BadRequest(new
                {
                    mensagem = "Dados inválidos na atualização do aluno.",
                    erros
                });
            }

            alunoExistente.Nome = dadosAtualizados.Nome.Trim();
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
            if (id <= 0)
            {
                return Results.BadRequest(new { mensagem = "O ID do aluno deve ser um número inteiro positivo." });
            }

            var aluno = Alunos.FirstOrDefault(a => a.Id == id);
            if (aluno is null)
            {
                return Results.NotFound(new { mensagem = "Aluno não encontrado." });
            }

            Alunos.Remove(aluno);
            return Results.NoContent();
        });
    }

    // Valida todos os campos do Aluno segundo o Dicionário de Dados e as Regras de Negócio (RNs)
    private static List<string> ValidarAluno(Aluno? aluno)
    {
        var erros = new List<string>();

        if (aluno is null)
        {
            erros.Add("Os dados do aluno não foram enviados.");
            return erros;
        }

        // 1. Validação do Nome (único campo digitado: varchar 100, obrigatório, apenas letras e espaços)
        var nome = aluno.Nome?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(nome))
        {
            erros.Add("O nome do aluno é obrigatório.");
        }
        else
        {
            if (nome.Length < 3 || nome.Length > 100)
            {
                erros.Add("O nome do aluno deve ter entre 3 e 100 caracteres.");
            }

            if (!ApenasLetrasRegex.IsMatch(nome))
            {
                erros.Add("O nome do aluno deve conter apenas letras e espaços (números ou símbolos não são permitidos).");
            }
        }

        // 2. Validação da Idade (inteiro 3 dígitos, faixa segura de treino)
        if (aluno.Idade <= 0)
        {
            erros.Add("A idade é obrigatória e deve ser um número inteiro maior que zero.");
        }
        else if (aluno.Idade < 10 || aluno.Idade > 120)
        {
            erros.Add("A idade do aluno deve estar entre 10 e 120 anos.");
        }

        // 3. Validação do Sexo (Select: "M" ou "F")
        if (string.IsNullOrWhiteSpace(aluno.Sexo) || !SexosPermitidos.Contains(aluno.Sexo))
        {
            erros.Add("Selecione uma opção válida para o sexo ('M' ou 'F').");
        }

        // 4. Validação da Experiência (Select: "Iniciante", "Intermediário", "Avançado" + RN01)
        if (string.IsNullOrWhiteSpace(aluno.Experiencia) || !ExperienciasPermitidas.Contains(aluno.Experiencia))
        {
            erros.Add("Selecione um nível de experiência válido ('Iniciante', 'Intermediário' ou 'Avançado').");
        }
        else if (aluno.Idade > 65 && aluno.Experiencia == "Avançado")
        {
            // RN01: O sistema deve utilizar a idade do cliente como critério para limitar os níveis de treino disponíveis
            erros.Add("Para alunos com mais de 65 anos, o nível 'Avançado' não é permitido por segurança fisiológica.");
        }

        // 5. Validação da Frequência Semanal (Select: 2 a 6 dias + RN02 e RN07)
        if (aluno.FrequenciaSemanal < 2 || aluno.FrequenciaSemanal > 6)
        {
            erros.Add("A frequência semanal de treino deve ser entre 2 e 6 dias.");
        }
        else if (aluno.Idade > 65 && aluno.FrequenciaSemanal > 5)
        {
            erros.Add("Alunos com mais de 65 anos devem ter no mínimo 2 dias de descanso semanal (frequência máxima de 5 dias).");
        }

        // 6. Validação do Objetivo (Select: "Hipertrofia", "Emagrecimento", "Força", "Resistência")
        if (string.IsNullOrWhiteSpace(aluno.Objetivo) || !ObjetivosPermitidos.Contains(aluno.Objetivo))
        {
            erros.Add("Selecione um objetivo válido ('Hipertrofia', 'Emagrecimento', 'Força', ou 'Resistência').");
        }

        return erros;
    }
}
