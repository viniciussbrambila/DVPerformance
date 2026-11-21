import MenuFiltro, { ChipFiltro } from "./MenuFiltro";
import Checkbox from "@mui/material/Checkbox";
import Divider from "@mui/material/Divider";
import ListItemText from "@mui/material/ListItemText";
import MenuItem from "@mui/material/MenuItem";

function obterTextoBotao({
  titulo,
  opcoes,
  selecionados,
  textoTodos,
  textoNenhum,
  textoSelecionadosPlural,
}) {
  if (selecionados.length === 0) {
    return `${titulo}: ${textoNenhum}`;
  }
  if (selecionados.length === opcoes.length) {
    return `${titulo}: ${textoTodos}`;
  }

  const rotulosSelecionados = opcoes
    .filter((opcao) => selecionados.includes(opcao.valor))
    .map((opcao) => opcao.rotuloCurto || opcao.rotulo);

  if (rotulosSelecionados.length <= 2) {
    return `${titulo}: ${rotulosSelecionados.join(", ")}`;
  }

  return `${titulo}: ${rotulosSelecionados.length} ${textoSelecionadosPlural}`;
}

export default function FiltroMultiSelecao({
  titulo,
  opcoes,
  selecionados,
  onChange,
  rotuloTodos = "Todos",
  textoTodos = "Todos",
  textoNenhum = "Nenhum",
  textoSelecionadosPlural = "selecionados",
  ariaLabel,
  minWidthMenu = 260,
}) {
  const todosValores = opcoes.map((opcao) => opcao.valor);
  const todosSelecionados = selecionados.length === opcoes.length;
  const algunsSelecionados = selecionados.length > 0 && !todosSelecionados;
  const filtroAtivo = !todosSelecionados;

  const alternarTodos = () => {
    onChange(todosSelecionados ? [] : [...todosValores]);
  };

  const alternarOpcao = (valor) => {
    const jaMarcado = selecionados.includes(valor);
    const proximo = jaMarcado
      ? selecionados.filter((item) => item !== valor)
      : todosValores.filter(
          (item) => item === valor || selecionados.includes(item),
        );
    onChange(proximo);
  };

  const textoBotao = obterTextoBotao({
    titulo,
    opcoes,
    selecionados,
    textoTodos,
    textoNenhum,
    textoSelecionadosPlural,
  });

  return (
    <MenuFiltro
      ativo={filtroAtivo}
      texto={textoBotao}
      ariaLabel={ariaLabel || `Filtrar por ${titulo.toLowerCase()}`}
      minWidth={minWidthMenu}
      maxHeight={400}
    >
      <MenuItem onClick={alternarTodos} sx={{ py: 0.75 }}>
        <Checkbox
          checked={todosSelecionados}
          indeterminate={algunsSelecionados}
          size="small"
          sx={{ mr: 1, p: 0.5 }}
        />
        <ListItemText
          primary={rotuloTodos}
          slotProps={{
            primary: { fontWeight: 600, fontSize: "0.9rem" },
          }}
        />
        <ChipFiltro
          label={`${selecionados.length}/${opcoes.length}`}
          color={todosSelecionados ? "primary" : "default"}
        />
      </MenuItem>

      <Divider sx={{ my: 0.5 }} />

      {opcoes.map((opcao) => {
        const marcado = selecionados.includes(opcao.valor);
        return (
          <MenuItem
            key={opcao.valor}
            onClick={() => alternarOpcao(opcao.valor)}
            sx={{ py: 0.6 }}
          >
            <Checkbox
              checked={marcado}
              size="small"
              sx={{ mr: 1, p: 0.5 }}
            />
            <ListItemText
              primary={opcao.rotulo}
              slotProps={{
                primary: { fontWeight: 500, fontSize: "0.9rem" },
              }}
            />
            {opcao.corChip && (
              <ChipFiltro
                label={opcao.rotulo}
                color={opcao.corChip}
              />
            )}
          </MenuItem>
        );
      })}
    </MenuFiltro>
  );
}
