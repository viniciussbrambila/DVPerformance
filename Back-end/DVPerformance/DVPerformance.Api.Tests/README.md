# Verificações do backend

Execute a partir da raiz do repositório:

~~~powershell
dotnet run --project Back-end/DVPerformance/DVPerformance.Api.Tests --configuration Release
~~~

O executável retorna erro se alguma verificação falhar. Valida os sorteios de duas dicas, ciclos sem repetição, estabilidade da resposta, ordem dos exercícios, vínculos do modelo e SQL da migração. Também valida aniversário e ano bissexto, fuso de São Paulo, limites etários, contrato JSON de nascimento e remoção da antiga coluna de idade, preservação das datas de nascimento e diversidade de focos musculares. Não conecta ao banco de dados nem inicia a API.

A API aplica as migrações automaticamente ao iniciar. Alunos anteriores à migração permanecem com `dataDeNascimento: null`; informe a data real ao editar o cadastro antes de gerar outra ficha. A coluna antiga de idade foi removida; a idade usada nas regras é calculada exclusivamente pela data de nascimento.

## Geração com divisão escolhida

Consulte as opções com GET /treinos/divisoes?dias=4. Ao gerar uma ficha, envie
o identificador da divisão na query: POST /treinos/gerar/ID?usuarioId=PERSONAL&divisao=upper-lower.
A API aceita apenas as divisões correspondentes à frequência do aluno.

O nome da divisão é preservado no campo existente treinos.divisao, sem nova migração.
A geração exige cobertura dos grupos definidos para cada sessão, evita repetição
do mesmo item no dia e prioriza variações entre sessões. As fichas anteriores
só são desativadas após a nova ficha ser montada e validada.


## Focos musculares e ordem

Cada exercício e máquina possui `focoMuscular`, editável no catálogo. Consulte as opções
com GET /exercicios/focos-musculares. POST e PUT aceitam uma chave válida do grupo;
valor vazio classifica pelo nome, e foco incompatível retorna erro. A inicialização
preenche apenas focos ainda vazios, inclusive em contas antigas, sem sobrescrever ajustes.

A geração combina exercícios livres e máquinas na contagem por grupo, foco e dia.
Primeiro cobre cada grupo da sessão; depois distribui as vagas equilibrando a quantidade
e dando preferência às prioridades maiores (número menor). As prioridades 3 e 4 têm
no máximo dois exercícios por grupo/dia, com focos diferentes. As prioridades 1 e 2
admitem até dois movimentos distintos por foco/dia, buscando focos ainda ausentes
antes da segunda ocorrência. Essas regras prevalecem sobre equipamento e ineditismo
na semana. O mesmo cadastro nunca se repete dentro do dia.

Ao atingir o limite de um grupo, suas vagas podem ir para outro grupo da mesma sessão.
Por exemplo, com catálogo completo, Peito/Abdômen gera 4+2 no iniciante, 5+2 no
intermediário e 6+2 no avançado. Se faltarem opções válidas, a ficha fica abaixo da meta.
O foco geral/desconhecido permanece conservador: no máximo um, sem combinar com
outros focos do grupo. As contagens reiniciam em cada sessão.

A meta por sessão é de 6 exercícios para iniciantes, 7 para intermediários e 8
para avançados, sem exceder esse total. Quando os limites de foco ou o catálogo não
permitirem completar a meta, a sessão fica menor. Os ajustes de séries, descanso,
aeróbico e preferência por máquinas conforme a idade continuam sendo aplicados.

Adutores / Abdutores permanece um único grupo no catálogo, mas as sessões exigem
exatamente um foco de adução junto de quadríceps e um de abdução junto de glúteos.
Pernas completas inclui um de cada dentro da meta, sem usar esse grupo para preencher
vagas extras. A validação exige o foco ativo correto, e avisa se ele faltar no catálogo.

Full Body alterna grupos para caber nas metas: A inclui peito, costas, quadríceps,
posteriores, panturrilha e adutor; B inclui peito, costas, ombros, glúteos, panturrilha
e abdutor. Assim, todos os grupos continuam contemplados durante a semana.
O campo gruposExibicao das sessões apresenta adutores e abdutores separadamente
nas opções de divisão, conforme o movimento exigido naquele dia.

As categorias expressam ênfase/padrão, não isolamento anatômico: para tríceps, por
exemplo, distinguem posição do braço e empurrões compostos. Referência primária:
[Maeo et al. (2023)](https://pubmed.ncbi.nlm.nih.gov/35819335/).

Ordem: 1) peito, costas, quadríceps, posteriores e glúteos; 2) bíceps, tríceps,
ombros e adutores/abdutores; 3) trapézio e panturrilha; 4) antebraço, abdômen e lombar.
As séries mantêm as regras anteriores, independentemente da mudança na ordem.

A migração nova remove a antiga coluna de idade e adiciona os campos de foco.
As migrações anteriores permanecem como histórico para bancos já instalados;
reverter a remoção recria a coluna vazia, sem recuperar os valores excluídos.
