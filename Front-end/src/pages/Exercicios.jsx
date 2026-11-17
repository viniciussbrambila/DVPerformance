import CatalogoItensTreino from "../components/CatalogoItensTreino";

const CONFIG_EXERCICIOS = {
  endpoint: "exercicios",
  prefixoCache: "exercicios",
  tipoCatalogo: "exercicio",
  singularMinusculo: "exercício",
  pluralMinusculo: "exercícios",
  tituloPagina: "Catálogo de Exercícios",
  botaoNovo: "Novo Exercício",
  tituloNovo: "Novo Exercício",
  tituloEditar: "Editar Exercício",
  labelNome: "Nome do Exercício",
  labelGrupo: "Grupo Muscular Relacionado",
  labelDescricao: "Descrição / Equipamento",
  erroSalvar: "Não foi possível salvar o exercício.",
  erroFocoInvalido:
    "Selecione o padrão biomecânico do movimento para que ele encaixe na geração de treinos.",
  helperNivelMinimo:
    "Define a partir de qual nível de aluno este exercício entra na geração automática (compostos multiarticulares sempre vêm antes dos isoladores).",
  placeholderBusca: "Pesquisar por nome do exercício",
  ariaBusca: "Pesquisar exercícios por nome",
  alertaAdicionar: "Escolha um exercício para adicionar ao plano.",
  alertaSubstituir: "Escolha um exercício para substituir o item do plano.",
  msgVazioCadastro: "Nenhum exercício cadastrado.",
  msgTodosNoDia:
    "Todos os exercícios deste catálogo já estão neste dia do treino.",
  msgIncompativelNivel: (nivelAluno) =>
    `Nenhum exercício compatível com o nível ${nivelAluno} disponível.`,
  msgSemGrupoFiltro:
    "Selecione pelo menos um grupo muscular no filtro para exibir os exercícios.",
  msgSemNivelFiltro:
    "Selecione pelo menos um nível no filtro para exibir os exercícios.",
  msgNenhumEncontrado: "Nenhum exercício encontrado.",
};

export default function Exercicios(props) {
  return <CatalogoItensTreino config={CONFIG_EXERCICIOS} {...props} />;
}
