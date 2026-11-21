import MenuFiltro, { ChipFiltro } from "./MenuFiltro";
import { Checkbox, ListItemText, MenuItem } from "@mui/material";
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
    <MenuFiltro
      ativo={filtroAtivo}
      texto={textoBotao}
      ariaLabel="Filtrar por nível do exercício"
      minWidth={280}
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
            <ChipFiltro label={opcao.valor} color={opcao.cor} />
          </MenuItem>
        );
      })}
    </MenuFiltro>
  );
}
