import { useState } from "react";
import {
  Button,
  Checkbox,
  Chip,
  ListItemText,
  Menu,
  MenuItem,
} from "@mui/material";
import FilterListIcon from "@mui/icons-material/FilterList";
import KeyboardArrowDownIcon from "@mui/icons-material/KeyboardArrowDown";
import {
  OPCOES_NIVEL_MINIMO,
  nivelPermitidoParaAluno,
  normalizarNivelExperiencia,
} from "./FocoMuscularSelect";

export default function FiltroNivelExercicio({
  niveisSelecionados,
  onChange,
  experienciaAluno = null,
}) {
  const [anchorEl, setAnchorEl] = useState(null);
  const aberto = Boolean(anchorEl);
  const nivelAluno = normalizarNivelExperiencia(experienciaAluno);

  const alternarNivel = (nivel) => {
    if (!nivelPermitidoParaAluno(nivel, nivelAluno)) return;
    const jaMarcado = niveisSelecionados.includes(nivel);
    const proximo = jaMarcado
      ? niveisSelecionados.filter((item) => item !== nivel)
      : OPCOES_NIVEL_MINIMO.map((opcao) => opcao.valor).filter(
          (valor) => valor === nivel || niveisSelecionados.includes(valor),
        );
    onChange(proximo);
  };

  const textoBotao =
    niveisSelecionados.length === 0
      ? "Nível: Nenhum"
      : niveisSelecionados.length === OPCOES_NIVEL_MINIMO.length
        ? "Nível: Todos"
        : `Nível: ${niveisSelecionados.join(", ")}`;

  const filtroAtivo =
    niveisSelecionados.length <
    OPCOES_NIVEL_MINIMO.filter((o) =>
      nivelPermitidoParaAluno(o.valor, nivelAluno),
    ).length;

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
        aria-label="Filtrar por nível do exercício"
        sx={{
          minHeight: 40,
          px: 2,
          borderColor: filtroAtivo ? "primary.main" : "divider",
          fontWeight: 500,
          whiteSpace: "nowrap",
        }}
      >
        {textoBotao}
      </Button>

      <Menu
        anchorEl={anchorEl}
        open={aberto}
        onClose={() => setAnchorEl(null)}
        slotProps={{
          paper: {
            sx: { minWidth: 280, borderRadius: 2, mt: 0.5 },
          },
        }}
      >
        {OPCOES_NIVEL_MINIMO.map((opcao) => {
          const permitido = nivelPermitidoParaAluno(opcao.valor, nivelAluno);
          const marcado =
            permitido && niveisSelecionados.includes(opcao.valor);

          return (
            <MenuItem
              key={opcao.valor}
              disabled={!permitido}
              onClick={() => alternarNivel(opcao.valor)}
              sx={{ py: 0.75 }}
            >
              <Checkbox
                checked={marcado}
                disabled={!permitido}
                size="small"
                sx={{ mr: 1, p: 0.5 }}
              />
              <ListItemText
                primary={opcao.valor}
                secondary={
                  !permitido ? `Bloqueado (aluno ${nivelAluno})` : undefined
                }
                slotProps={{
                  primary: { fontWeight: 500, fontSize: "0.95rem" },
                  secondary: { fontSize: "0.75rem" },
                }}
              />
              <Chip
                label={opcao.valor}
                size="small"
                color={opcao.cor}
                variant="outlined"
                sx={{ ml: 1.5, height: 20, fontSize: "0.7rem", fontWeight: 600 }}
              />
            </MenuItem>
          );
        })}
      </Menu>
    </>
  );
}
