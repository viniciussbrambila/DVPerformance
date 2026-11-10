# Verificações do backend

Execute a partir da raiz do repositório:

~~~powershell
dotnet run --project Back-end/DVPerformance/DVPerformance.Api.Tests --configuration Release
~~~

O executável retorna erro se alguma verificação falhar. Valida os sorteios de duas dicas, ciclos sem repetição, estabilidade da resposta, ordem dos exercícios, vínculos do modelo e SQL da migração. Também valida aniversário e ano bissexto, fuso de São Paulo, limites etários, contrato JSON de nascimento e preservação dos dados antigos na migração. Não conecta ao banco de dados nem inicia a API.

A API aplica as migrações automaticamente ao iniciar. Alunos anteriores à migração permanecem com `dataDeNascimento: null`; informe a data real ao editar o cadastro antes de gerar outra ficha. A idade antiga fica preservada em `alunos.idade_legada` para consulta histórica, sem exposição na API nem participação nas regras de treino.

## Geração com divisão escolhida

Consulte as opções com GET /treinos/divisoes?dias=4. Ao gerar uma ficha, envie
o identificador da divisão na query: POST /treinos/gerar/ID?usuarioId=PERSONAL&divisao=upper-lower.
A API aceita apenas as divisões correspondentes à frequência do aluno.

O nome da divisão é preservado no campo existente treinos.divisao, sem nova migração.
A geração exige cobertura dos grupos definidos para cada sessão, evita repetição
do mesmo item no dia e prioriza variações entre sessões. As fichas anteriores
só são desativadas após a nova ficha ser montada e validada.
