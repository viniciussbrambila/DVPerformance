import { useState } from "react";
import Button from "@mui/material/Button";
import Checkbox from "@mui/material/Checkbox";
import Chip from "@mui/material/Chip";
import Divider from "@mui/material/Divider";
import ListItemText from "@mui/material/ListItemText";
import Menu from "@mui/material/Menu";
import MenuItem from "@mui/material/MenuItem";
import { styled } from "@mui/material/styles";
import FilterListIcon from "@mui/icons-material/FilterList";
import KeyboardArrowDownIcon from "@mui/icons-material/KeyboardArrowDown";

const BotaoFiltro = styled(Button, {
  shouldForwardProp: (prop) => prop !== "filtroAtivo",
})(({ theme, filtroAtivo }) => ({
  minHeight: 40,
  paddingInline: theme.spacing(2),
  borderColor: filtroAtivo
    ? theme.palette.primary.main
    : theme.palette.divider,
  fontWeight: 500,
  whiteSpace: "nowrap",
}));

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
  const [anchorEl, setAnchorEl] = useState(null);
  const aberto = Boolean(anchorEl);

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
    <>
      <BotaoFiltro
        variant="outlined"
        color={filtroAtivo ? "primary" : "inherit"}
        filtroAtivo={filtroAtivo}
        startIcon={<FilterListIcon />}
        endIcon={<KeyboardArrowDownIcon />}
        onClick={(e) => setAnchorEl(e.currentTarget)}
        aria-haspopup="menu"
        aria-expanded={aberto ? "true" : undefined}
        aria-label={ariaLabel || `Filtrar por ${titulo.toLowerCase()}`}
      >
        {textoBotao}
      </BotaoFiltro>

      <Menu
        anchorEl={anchorEl}
        open={aberto}
        onClose={() => setAnchorEl(null)}
        slotProps={{
          paper: {
            sx: {
              minWidth: minWidthMenu,
              maxHeight: 400,
              borderRadius: 2,
              mt: 0.5,
            },
          },
        }}
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
          <Chip
            label={`${selecionados.length}/${opcoes.length}`}
            size="small"
            color={todosSelecionados ? "primary" : "default"}
            variant="outlined"
            sx={{ ml: 1.5, height: 20, fontSize: "0.7rem", fontWeight: 600 }}
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
                <Chip
                  label={opcao.rotulo}
                  size="small"
                  color={opcao.corChip}
                  variant="outlined"
                  sx={{
                    ml: 1.5,
                    height: 20,
                    fontSize: "0.7rem",
                    fontWeight: 600,
                  }}
                />
              )}
            </MenuItem>
          );
        })}
      </Menu>
    </>
  );
}
