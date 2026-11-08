# Verificações do backend

Execute a partir da raiz do repositório:

~~~powershell
dotnet run --project Back-end/DVPerformance/DVPerformance.Api.Tests --configuration Release
~~~

O executável retorna erro se alguma verificação falhar. Valida os sorteios de duas dicas, ciclos sem repetição, estabilidade da resposta, ordem dos exercícios, vínculos do modelo e SQL da migração. Não conecta ao banco de dados nem inicia a API.
