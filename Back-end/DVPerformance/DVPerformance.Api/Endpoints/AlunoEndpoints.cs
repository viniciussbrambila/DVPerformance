using System.Text.RegularExpressions;
using DVPerformance.Api.Data;
using DVPerformance.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace DVPerformance.Api.Endpoints;

public static class AlunoEndpoints
{
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
        "Hipertrofia", "Emagrecimento", "Força", "Resistência"
    };

    // Expressão regular que aceita apenas letras (incluindo acentuadas) e espaços
    private static readonly Regex ApenasLetrasRegex = new(@"^[\p{L}\s]+$", RegexOptions.Compiled);

    public static void MapAlunoEndpoints(this WebApplication app)
    {
        // Cria um grupo de rotas com o prefixo "/alunos"
        var grupo = app.MapGroup("/alunos");

        // GET /alunos?usuarioId=1 -> Lista os alunos ativos (ou todos se incluirInativos=true)
        grupo.MapGet("/", async (int? usuarioId, bool? incluirInativos, AppDbContext db) =>
        {
            var query = db.Alunos.AsNoTracking().AsQueryable();

            if (incluirInativos != true)
            {
                query = query.Where(a => a.Ativo);
            }

            if (usuarioId.HasValue && usuarioId.Value > 0)
            {
                query = query.Where(a => a.UsuarioId == usuarioId.Value);
            }

            var alunos = await query.OrderBy(a => a.Id).ToListAsync();
            return Results.Ok(alunos);
        });

        // GET /alunos/sem-treino?usuarioId=1 -> Lista alunos ativos sem plano ativo
        grupo.MapGet("/sem-treino", async (int? usuarioId, AppDbContext db) =>
        {
            var query = db.Alunos
                .AsNoTracking()
                .Where(a => a.Ativo && !db.Treinos.Any(t => t.AlunoId == a.Id && t.Ativo));

            if (usuarioId.HasValue && usuarioId.Value > 0)
            {
                query = query.Where(a => a.UsuarioId == usuarioId.Value);
            }

            var alunos = await query.OrderBy(a => a.Id).ToListAsync();
            return Results.Ok(alunos);
        });

        // GET /alunos/{id} -> Busca um aluno ativo específico pelo Id no banco
        grupo.MapGet("/{id:int}", async (int id, AppDbContext db) =>
        {
            if (id <= 0)
            {
                return Results.BadRequest(new { mensagem = "O ID do aluno deve ser um número inteiro positivo." });
            }

            var aluno = await db.Alunos.AsNoTracking().FirstOrDefaultAsync(a => a.Id == id && a.Ativo);
            if (aluno is null)
            {
                return Results.NotFound(new { mensagem = "Aluno não encontrado ou desativado." });
            }

            return Results.Ok(aluno);
        });

        // POST /alunos -> Cadastra um novo aluno no banco PostgreSQL vinculado ao Personal
        grupo.MapPost("/", async (int? usuarioId, Aluno? novoAluno, AppDbContext db) =>
        {
            if (novoAluno is not null && (!novoAluno.UsuarioId.HasValue || novoAluno.UsuarioId.Value <= 0) && usuarioId.HasValue && usuarioId.Value > 0)
            {
                novoAluno.UsuarioId = usuarioId.Value;
            }

            var erros = ValidarAluno(novoAluno, exigirUsuarioId: true);
            if (erros.Count > 0)
            {
                return Results.BadRequest(new
                {
                    mensagem = "Dados inválidos no cadastro do aluno.",
                    erros
                });
            }

            var donoId = novoAluno!.UsuarioId!.Value;
            var personalExiste = await db.Usuarios.AnyAsync(u => u.Id == donoId && u.Ativo);
            if (!personalExiste)
            {
                return Results.BadRequest(new
                {
                    mensagem = "O Personal informado não foi encontrado.",
                    erros = new[] { "A conta do Personal (usuarioId) não existe ou está desativada no banco de dados." }
                });
            }

            novoAluno.Id = 0; // Garante que o PostgreSQL gere o ID automaticamente (SERIAL)
            novoAluno.Nome = novoAluno.Nome.Trim();
            novoAluno.DataCadastro = DateTime.UtcNow;
            novoAluno.Ativo = true;

            db.Alunos.Add(novoAluno);
            await db.SaveChangesAsync();

            return Results.Created($"/alunos/{novoAluno.Id}", novoAluno);
        });

        // PUT /alunos/{id} -> Atualiza os dados de um aluno ativo existente no banco
        grupo.MapPut("/{id:int}", async (int id, Aluno? dadosAtualizados, AppDbContext db) =>
        {
            if (id <= 0)
            {
                return Results.BadRequest(new { mensagem = "O ID do aluno deve ser um número inteiro positivo." });
            }

            var alunoExistente = await db.Alunos.FirstOrDefaultAsync(a => a.Id == id && a.Ativo);
            if (alunoExistente is null)
            {
                return Results.NotFound(new { mensagem = "Aluno não encontrado ou desativado." });
            }

            var erros = ValidarAluno(dadosAtualizados, exigirUsuarioId: false);
            if (erros.Count > 0)
            {
                return Results.BadRequest(new
                {
                    mensagem = "Dados inválidos na atualização do aluno.",
                    erros
                });
            }

            var objetivoMudou = !string.Equals(alunoExistente.Objetivo, dadosAtualizados!.Objetivo, StringComparison.Ordinal);

            alunoExistente.Nome = dadosAtualizados.Nome.Trim();
            alunoExistente.Idade = dadosAtualizados.Idade;
            alunoExistente.Sexo = dadosAtualizados.Sexo;
            alunoExistente.Experiencia = dadosAtualizados.Experiencia;
            alunoExistente.FrequenciaSemanal = dadosAtualizados.FrequenciaSemanal;
            alunoExistente.Objetivo = dadosAtualizados.Objetivo;
            alunoExistente.Peso = dadosAtualizados.Peso;
            alunoExistente.Altura = dadosAtualizados.Altura;

            if (dadosAtualizados.UsuarioId.HasValue && dadosAtualizados.UsuarioId.Value > 0)
            {
                alunoExistente.UsuarioId = dadosAtualizados.UsuarioId;
            }

            // Se o objetivo do aluno foi alterado, atualiza a dica dos planos ativos ainda não exportados respeitando o ciclo sem repetição
            if (objetivoMudou)
            {
                var objetivoParaDica = TreinoEndpoints.NormalizarObjetivoParaDica(alunoExistente.Objetivo);
                var dicasNovoObjetivo = await db.DicasObjetivo
                    .AsNoTracking()
                    .Where(d => d.Objetivo == objetivoParaDica)
                    .ToListAsync();

                if (dicasNovoObjetivo.Count > 0)
                {
                    var historicoDicasAluno = await db.Treinos
                        .AsNoTracking()
                        .Where(t => t.AlunoId == alunoExistente.Id && t.DicaObjetivoId != null)
                        .OrderBy(t => t.DataCriacao)
                        .ThenBy(t => t.Id)
                        .Select(t => t.DicaObjetivoId!.Value)
                        .ToListAsync();

                    var treinosNaoExportados = await db.Treinos
                        .Where(t => t.AlunoId == alunoExistente.Id && t.Ativo && t.Status != "Exportado")
                        .ToListAsync();

                    foreach (var treino in treinosNaoExportados)
                    {
                        var dicaSorteada = TreinoEndpoints.SortearDicaSemRepetirCiclo(dicasNovoObjetivo, historicoDicasAluno);
                        if (dicaSorteada is not null)
                        {
                            treino.DicaObjetivoId = dicaSorteada.Id;
                            historicoDicasAluno.Add(dicaSorteada.Id);
                        }
                    }
                }
            }

            await db.SaveChangesAsync();

            return Results.Ok(alunoExistente);
        });

        // DELETE /alunos/{id} -> Desativa o aluno (Soft Delete) mantendo o histórico no banco
        grupo.MapDelete("/{id:int}", async (int id, AppDbContext db) =>
        {
            if (id <= 0)
            {
                return Results.BadRequest(new { mensagem = "O ID do aluno deve ser um número inteiro positivo." });
            }

            var aluno = await db.Alunos.FirstOrDefaultAsync(a => a.Id == id && a.Ativo);
            if (aluno is null)
            {
                return Results.NotFound(new { mensagem = "Aluno não encontrado ou já desativado." });
            }

            aluno.Ativo = false;

            // RF01: A exclusão do aluno desativa automaticamente todos os planos de treino vinculados a ele
            var treinosVinculados = await db.Treinos
                .Where(t => t.AlunoId == id && t.Ativo)
                .ToListAsync();

            foreach (var treino in treinosVinculados)
            {
                treino.Ativo = false;
            }

            await db.SaveChangesAsync();

            return Results.NoContent();
        });
    }

    // Valida todos os campos do Aluno segundo o Dicionário de Dados e as Regras de Negócio (RNs)
    private static List<string> ValidarAluno(Aluno? aluno, bool exigirUsuarioId)
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

        // 2. Validação da Idade (inteiro 3 dígitos, faixa segura de treino: 13 a 120 anos)
        if (aluno.Idade <= 0)
        {
            erros.Add("A idade é obrigatória e deve ser um número inteiro maior que zero.");
        }
        else if (aluno.Idade < 13 || aluno.Idade > 120)
        {
            erros.Add("A idade do aluno deve estar entre 13 e 120 anos.");
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

        // 6. Validação do Objetivo (Select: "Hipertrofia", "Emagrecimento", "Força", "Resistência" - usado para as dicas no PDF)
        if (string.IsNullOrWhiteSpace(aluno.Objetivo) || !ObjetivosPermitidos.Contains(aluno.Objetivo))
        {
            erros.Add("Selecione um objetivo válido ('Hipertrofia', 'Emagrecimento', 'Força' ou 'Resistência').");
        }

        // 7. Validação do Peso (em kg: de 30 a 300 kg)
        if (!aluno.Peso.HasValue)
        {
            erros.Add("O peso do aluno é obrigatório.");
        }
        else if (aluno.Peso.Value < 30m || aluno.Peso.Value > 300m)
        {
            erros.Add("O peso do aluno deve estar entre 30 e 300 kg.");
        }

        // 8. Validação da Altura (em metros: de 1,00 a 2,50 m)
        if (!aluno.Altura.HasValue)
        {
            erros.Add("A altura do aluno é obrigatória.");
        }
        else if (aluno.Altura.Value < 1.00m || aluno.Altura.Value > 2.50m)
        {
            erros.Add("A altura do aluno deve estar entre 1,00 e 2,50 m (100 a 250 cm).");
        }

        // 9. Vínculo obrigatório com o Personal responsável no cadastro
        if (exigirUsuarioId && (!aluno.UsuarioId.HasValue || aluno.UsuarioId.Value <= 0))
        {
            erros.Add("Informe o ID do Personal (usuarioId) responsável pelo cadastro do aluno.");
        }

        return erros;
    }
}
