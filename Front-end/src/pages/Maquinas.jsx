import CatalogoItensTreino from "../components/CatalogoItensTreino";

const CONFIG_MAQUINAS = {
  endpoint: "maquinas",
  prefixoCache: "maquinas",
  tipoCatalogo: "maquina",
  singularMinusculo: "máquina",
  pluralMinusculo: "máquinas",
  tituloPagina: "Catálogo de Máquinas",
  botaoNovo: "Nova Máquina",
  tituloNovo: "Nova Máquina",
  tituloEditar: "Editar Máquina",
  labelNome: "Nome da Máquina / Equipamento",
  maxLengthNome: 100,
  labelGrupo: "Grupo Muscular Principal Atendido",
  labelDescricao: "Descrição / Especificação da Máquina",
  maxLengthDescricao: 255,
  erroSalvar: "Não foi possível salvar a máquina.",
  erroFocoInvalido:
    "Selecione o padrão biomecânico da máquina para que ela encaixe na geração de treinos.",
  helperNivelMinimo:
    "Define a partir de qual nível de aluno esta máquina entra na geração automática (máquinas costumam ser ideais desde o Iniciante, salvo equipamentos específicos avançados).",
  placeholderBusca: "Pesquisar por nome da máquina",
  ariaBusca: "Pesquisar máquinas por nome",
  alertaAdicionar: "Escolha uma máquina para adicionar ao plano.",
  alertaSubstituir: "Escolha uma máquina para substituir o item do plano.",
  msgVazioCadastro: "Nenhuma máquina cadastrada.",
  msgTodosNoDia:
    "Todas as máquinas deste catálogo já estão neste dia do treino.",
  msgIncompativelNivel: (nivelAluno) =>
    `Nenhuma máquina compatível com o nível ${nivelAluno} disponível.`,
  msgSemGrupoFiltro:
    "Selecione pelo menos um grupo muscular no filtro para exibir as máquinas.",
  msgSemNivelFiltro:
    "Selecione pelo menos um nível no filtro para exibir as máquinas.",
  msgNenhumEncontrado: "Nenhuma máquina encontrada.",
};

export default function Maquinas(props) {
  return <CatalogoItensTreino config={CONFIG_MAQUINAS} {...props} />;
}
