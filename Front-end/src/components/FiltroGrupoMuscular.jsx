import { useState } from "react";
import {
  Button,
  Checkbox,
  Chip,
  Divider,
  ListItemText,
  Menu,
  MenuItem,
} from "@mui/material";
import FilterListIcon from "@mui/icons-material/FilterList";
import KeyboardArrowDownIcon from "@mui/icons-material/KeyboardArrowDown";
import { normalizarTextoBusca } from "../utils/busca";

export const CATEGORIAS_GRUPOS = [
  {
    subtitulo: "Membros Superiores (Parte de Cima)",
    regiao: "Membros Superiores",
    grupos: [
      "Peito",
      "Costas",
      "Trapézio",
      "Ombros",
      "Bíceps",
      "Tríceps",
      "Antebraço",
    ],
  },
  {
    subtitulo: "Membros Inferiores (Parte de Baixo)",
    regiao: "Membros Inferiores",
    grupos: [
      "Quadríceps",
      "Posterior de Coxa",
      "Glúteos",
      "Adutores / Abdutores",
      "Panturrilha",
    ],
  },
  {
    subtitulo: "Core / Tronco (Centro do Corpo)",
    regiao: "Core / Tronco",
    grupos: ["Abdômen", "Lombar"],
  },
];

export const TODOS_GRUPOS_MUSCULARES = CATEGORIAS_GRUPOS.flatMap(
  (categoria) => categoria.grupos,
);

export function itemPertenceAosGrupos(item, gruposSelecionados) {
  if (!Array.isArray(gruposSelecionados) || gruposSelecionados.length === 0) {
    return false;
  }
  if (gruposSelecionados.length === TODOS_GRUPOS_MUSCULARES.length) {
    return true;
  }
  const grupoItem = normalizarTextoBusca(item?.grupo).trim();
  return gruposSelecionados.some(
    (grupo) => normalizarTextoBusca(grupo).trim() === grupoItem,
  );
}

function obterTextoBotao(gruposSelecionados) {
  if (gruposSelecionados.length === 0) {
    return "Região / Grupo: Nenhum";
  }
  if (gruposSelecionados.length === TODOS_GRUPOS_MUSCULARES.length) {
    return "Região / Grupo: Todos";
  }

  const regiaoCompleta = CATEGORIAS_GRUPOS.find(
    (categoria) =>
      categoria.grupos.length === gruposSelecionados.length &&
      categoria.grupos.every((grupo) => gruposSelecionados.includes(grupo)),
  );
  if (regiaoCompleta) {
    return `Região: ${regiaoCompleta.regiao}`;
  }

  if (gruposSelecionados.length <= 2) {
    return `Grupo: ${gruposSelecionados.join(", ")}`;
  }

  return `Região / Grupo: ${gruposSelecionados.length} selecionados`;
}

export default function FiltroGrupoMuscular({
  gruposSelecionados,
  onChange,
}) {
  const [anchorEl, setAnchorEl] = useState(null);
  const aberto = Boolean(anchorEl);

  const todosSelecionados =
    gruposSelecionados.length === TODOS_GRUPOS_MUSCULARES.length;
  const algunsSelecionados =
    gruposSelecionados.length > 0 && !todosSelecionados;
  const filtroAtivo = !todosSelecionados;

  const alternarTodos = () => {
    onChange(todosSelecionados ? [] : [...TODOS_GRUPOS_MUSCULARES]);
  };

  const alternarRegiao = (categoria) => {
    const todosDaRegiaoMarcados = categoria.grupos.every((grupo) =>
      gruposSelecionados.includes(grupo),
    );
    const proximoSet = new Set(gruposSelecionados);
    if (todosDaRegiaoMarcados) {
      categoria.grupos.forEach((grupo) => proximoSet.delete(grupo));
    } else {
      categoria.grupos.forEach((grupo) => proximoSet.add(grupo));
    }
    onChange(
      TODOS_GRUPOS_MUSCULARES.filter((grupo) => proximoSet.has(grupo)),
    );
  };

  const alternarGrupo = (grupo) => {
    const jaMarcado = gruposSelecionados.includes(grupo);
    const proximo = jaMarcado
      ? gruposSelecionados.filter((item) => item !== grupo)
      : TODOS_GRUPOS_MUSCULARES.filter(
          (item) => item === grupo || gruposSelecionados.includes(item),
        );
    onChange(proximo);
  };

  return (
    <>
      <Button
        variant="outlined"
        color={filtroAtivo ? "primary" : "inherit"}
        startIcon={<FilterListIcon />}
        endIcon={<KeyboardArrowDownIcon />}
        onClick={(e) => setAnchorEl(e.currentTarget)}
        aria-haspopup="menu"
        aria-expanded={aberto ? "true" : undefined}
        aria-label="Filtrar por região ou grupo muscular"
        sx={{
          minHeight: 56,
          px: 2,
          borderRadius: 1,
          borderColor: filtroAtivo ? "primary.main" : "divider",
          textTransform: "none",
          fontWeight: 500,
          whiteSpace: "nowrap",
        }}
      >
        {obterTextoBotao(gruposSelecionados)}
      </Button>

      <Menu
        anchorEl={anchorEl}
        open={aberto}
        onClose={() => setAnchorEl(null)}
        slotProps={{
          paper: {
            sx: { minWidth: 320, maxHeight: 440, borderRadius: 2, mt: 0.5 },
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
            primary="Todos os grupos musculares"
            slotProps={{
              primary: { fontWeight: 600, fontSize: "0.9rem" },
            }}
          />
          <Chip
            label={`${gruposSelecionados.length}/${TODOS_GRUPOS_MUSCULARES.length}`}
            size="small"
            variant="outlined"
            sx={{ ml: 1.5, height: 20, fontSize: "0.7rem", fontWeight: 600 }}
          />
        </MenuItem>

        <Divider sx={{ my: 0.5 }} />

        {CATEGORIAS_GRUPOS.flatMap((categoria) => {
          const selecionadosRegiao = categoria.grupos.filter((grupo) =>
            gruposSelecionados.includes(grupo),
          ).length;
          const todosDaRegiao = selecionadosRegiao === categoria.grupos.length;
          const algunsDaRegiao = selecionadosRegiao > 0 && !todosDaRegiao;

          return [
            <MenuItem
              key={`regiao-${categoria.regiao}`}
              onClick={() => alternarRegiao(categoria)}
              sx={{ py: 0.75 }}
            >
              <Checkbox
                checked={todosDaRegiao}
                indeterminate={algunsDaRegiao}
                size="small"
                sx={{ mr: 1, p: 0.5 }}
              />
              <ListItemText
                primary={categoria.regiao}
                slotProps={{
                  primary: {
                    fontWeight: 700,
                    fontSize: "0.9rem",
                    color: "primary.main",
                  },
                }}
              />
              <Chip
                label={`${selecionadosRegiao}/${categoria.grupos.length}`}
                size="small"
                color={todosDaRegiao ? "primary" : "default"}
                variant="outlined"
                sx={{ ml: 1.5, height: 20, fontSize: "0.7rem", fontWeight: 600 }}
              />
            </MenuItem>,
            ...categoria.grupos.map((grupo) => (
              <MenuItem
                key={grupo}
                onClick={() => alternarGrupo(grupo)}
                sx={{ py: 0.5, pl: 4 }}
              >
                <Checkbox
                  checked={gruposSelecionados.includes(grupo)}
                  size="small"
                  sx={{ mr: 1, p: 0.5 }}
                />
                <ListItemText
                  primary={grupo}
                  slotProps={{
                    primary: { fontWeight: 500, fontSize: "0.9rem" },
                  }}
                />
              </MenuItem>
            )),
          ];
        })}
      </Menu>
    </>
  );
}
