import { useState, useMemo } from "react";
import { Box, Typography, IconButton, Button } from "@mui/material";
import { alpha, useTheme } from "@mui/material/styles";
import ChevronLeftIcon from "@mui/icons-material/ChevronLeft";
import ChevronRightIcon from "@mui/icons-material/ChevronRight";
import ArrowDropDownIcon from "@mui/icons-material/ArrowDropDown";

export const PROPS_POPOVER_CALENDARIO = {
  anchorOrigin: { vertical: "bottom", horizontal: "left" },
  transformOrigin: { vertical: "top", horizontal: "left" },
  slotProps: {
    paper: {
      elevation: 8,
      sx: {
        mt: 1,
        p: 2,
        borderRadius: 3,
        overflow: "hidden",
        maxWidth: "calc(100vw - 24px)",
      },
    },
  },
};

const MESES = [
  "Janeiro",
  "Fevereiro",
  "Março",
  "Abril",
  "Maio",
  "Junho",
  "Julho",
  "Agosto",
  "Setembro",
  "Outubro",
  "Novembro",
  "Dezembro",
];

export const MESES_CURTOS = [
  "Jan",
  "Fev",
  "Mar",
  "Abr",
  "Mai",
  "Jun",
  "Jul",
  "Ago",
  "Set",
  "Out",
  "Nov",
  "Dez",
];

const DIAS_SEMANA = ["D", "S", "T", "Q", "Q", "S", "S"];

export function normalizarData(data) {
  if (!data) return null;
  const d = new Date(
    data.getFullYear(),
    data.getMonth(),
    data.getDate(),
    12,
    0,
    0,
    0,
  );
  return Number.isNaN(d.getTime()) ? null : d;
}

function mesmoDia(a, b) {
  if (!a || !b) return false;
  return (
    a.getFullYear() === b.getFullYear() &&
    a.getMonth() === b.getMonth() &&
    a.getDate() === b.getDate()
  );
}

export function formatarDataBR(data) {
  if (!data) return "";
  const dia = String(data.getDate()).padStart(2, "0");
  const mes = String(data.getMonth() + 1).padStart(2, "0");
  const ano = data.getFullYear();
  return `${dia}/${mes}/${ano}`;
}

export function aplicarMascaraDataBR(valor) {
  return String(valor || "")
    .replace(/\D/g, "")
    .slice(0, 8)
    .replace(/^(\d{2})(\d)/, "$1/$2")
    .replace(/^(\d{2}\/\d{2})(\d)/, "$1/$2");
}

export function diaForaDosLimites(data, minLim, maxLim) {
  if (!data) return false;
  const tempo = data.getTime();
  if (minLim && tempo < minLim.getTime()) return true;
  if (maxLim && tempo > maxLim.getTime()) return true;
  return false;
}

function mesForaDosLimites(ano, mes, minLim, maxLim) {
  const inicioMes = new Date(ano, mes, 1, 12, 0, 0, 0);
  const fimMes = new Date(ano, mes + 1, 0, 12, 0, 0, 0);
  if (minLim && fimMes.getTime() < minLim.getTime()) return true;
  if (maxLim && inicioMes.getTime() > maxLim.getTime()) return true;
  return false;
}

export function ajustarMesParaAno(ano, mesDesejado, minLim, maxLim) {
  let mes = mesDesejado;
  if (minLim && ano === minLim.getFullYear() && mes < minLim.getMonth()) {
    mes = minLim.getMonth();
  }
  if (maxLim && ano === maxLim.getFullYear() && mes > maxLim.getMonth()) {
    mes = maxLim.getMonth();
  }
  return mes;
}

function gerarDiasCalendario(ano, mes) {
  const primeiroDiaSemana = new Date(ano, mes, 1).getDay();
  const totalDiasMes = new Date(ano, mes + 1, 0).getDate();
  const celulas = [];

  for (let i = 0; i < primeiroDiaSemana; i += 1) {
    celulas.push(null);
  }

  for (let dia = 1; dia <= totalDiasMes; dia += 1) {
    celulas.push(new Date(ano, mes, dia, 12, 0, 0, 0));
  }

  while (celulas.length % 7 !== 0) {
    celulas.push(null);
  }

  return celulas;
}

export default function CalendarioMes({
  titulo,
  ano,
  mes,
  onMudarAnoMes,
  anosDisponiveis,
  minLim = null,
  maxLim = null,
  dataSelecionada,
  dataInicio,
  dataFim,
  dataHover,
  selecionandoFim = false,
  onSelecionarDia,
  onHoverDia,
  alturaListaAnos = 112,
}) {
  const theme = useTheme();
  const [modoSelecaoMesAno, setModoSelecaoMesAno] = useState(false);
  const hoje = useMemo(() => normalizarData(new Date()), []);
  const celulas = useMemo(() => gerarDiasCalendario(ano, mes), [ano, mes]);

  const prevData = new Date(ano, mes - 1, 1);
  const nextData = new Date(ano, mes + 1, 1);

  const podeVoltarMes = !mesForaDosLimites(
    prevData.getFullYear(),
    prevData.getMonth(),
    minLim,
    maxLim,
  );
  const podeAvancarMes = !mesForaDosLimites(
    nextData.getFullYear(),
    nextData.getMonth(),
    minLim,
    maxLim,
  );

  const irMesAnterior = () => {
    if (!podeVoltarMes) return;
    onMudarAnoMes(prevData.getFullYear(), prevData.getMonth());
  };

  const irProximoMes = () => {
    if (!podeAvancarMes) return;
    onMudarAnoMes(nextData.getFullYear(), nextData.getMonth());
  };

  const modoDataUnica = dataSelecionada !== undefined;

  const fimEfetivo =
    selecionandoFim && dataInicio && dataHover && !dataFim
      ? dataHover
      : dataFim;

  const intervaloMin =
    dataInicio && fimEfetivo
      ? dataInicio <= fimEfetivo
        ? dataInicio
        : fimEfetivo
      : dataInicio;

  const intervaloMax =
    dataInicio && fimEfetivo
      ? dataInicio <= fimEfetivo
        ? fimEfetivo
        : dataInicio
      : fimEfetivo;

  const faixaBackground = alpha(
    theme.palette.primary.main,
    theme.palette.mode === "dark" ? 0.24 : 0.14,
  );
  const faixaHoverBorda = alpha(theme.palette.primary.main, 0.55);

  return (
    <Box sx={{ width: 288, userSelect: "none" }}>
      {titulo && (
        <Typography
          variant="caption"
          sx={{
            display: "block",
            color: "text.secondary",
            fontWeight: 600,
            textTransform: "uppercase",
            letterSpacing: 0.6,
            px: 1,
            mb: 0.5,
          }}
        >
          {titulo}
        </Typography>
      )}

      <Box
        sx={{
          display: "flex",
          alignItems: "center",
          justifyContent: "space-between",
          px: 0.5,
          py: 0.75,
        }}
      >
        <Button
          size="small"
          color="inherit"
          onClick={() => setModoSelecaoMesAno((atual) => !atual)}
          endIcon={
            <ArrowDropDownIcon
              sx={{
                transition: "transform 0.2s",
                transform: modoSelecaoMesAno
                  ? "rotate(180deg)"
                  : "rotate(0deg)",
              }}
            />
          }
          sx={{
            fontWeight: 700,
            fontSize: "0.95rem",
            px: 1,
            py: 0.5,
            borderRadius: 2,
          }}
        >
          {MESES[mes]} {ano}
        </Button>

        <Box sx={{ display: "flex", alignItems: "center", gap: 0.25 }}>
          <IconButton
            size="small"
            onClick={irMesAnterior}
            disabled={!podeVoltarMes}
            aria-label="Mês anterior"
          >
            <ChevronLeftIcon fontSize="small" />
          </IconButton>
          <IconButton
            size="small"
            onClick={irProximoMes}
            disabled={!podeAvancarMes}
            aria-label="Próximo mês"
          >
            <ChevronRightIcon fontSize="small" />
          </IconButton>
        </Box>
      </Box>

      {modoSelecaoMesAno ? (
        <Box sx={{ px: 0.5, pt: 0.5, pb: 1 }}>
          <Typography
            variant="caption"
            color="text.secondary"
            sx={{ fontWeight: 600, display: "block", mb: 0.75 }}
          >
            Ano
          </Typography>
          <Box
            sx={{
              display: "grid",
              gridTemplateColumns: "repeat(4, 1fr)",
              gap: 0.5,
              maxHeight: alturaListaAnos,
              overflowY: "auto",
              mb: 1.5,
              pr: 0.5,
            }}
          >
            {anosDisponiveis.map((anoOpcao) => {
              const selecionado = anoOpcao === ano;
              return (
                <Button
                  key={anoOpcao}
                  size="small"
                  variant={selecionado ? "contained" : "text"}
                  color={selecionado ? "primary" : "inherit"}
                  onClick={() =>
                    onMudarAnoMes(
                      anoOpcao,
                      ajustarMesParaAno(anoOpcao, mes, minLim, maxLim),
                    )
                  }
                  sx={{
                    minWidth: 0,
                    py: 0.4,
                    borderRadius: 4,
                    fontSize: "0.8rem",
                    fontWeight: selecionado ? 700 : 500,
                  }}
                >
                  {anoOpcao}
                </Button>
              );
            })}
          </Box>

          <Typography
            variant="caption"
            color="text.secondary"
            sx={{ fontWeight: 600, display: "block", mb: 0.75 }}
          >
            Mês
          </Typography>
          <Box
            sx={{
              display: "grid",
              gridTemplateColumns: "repeat(3, 1fr)",
              gap: 0.5,
            }}
          >
            {MESES_CURTOS.map((nomeMes, indiceMes) => {
              const selecionado = indiceMes === mes;
              const desabilitado = mesForaDosLimites(
                ano,
                indiceMes,
                minLim,
                maxLim,
              );
              return (
                <Button
                  key={nomeMes}
                  size="small"
                  disabled={desabilitado}
                  variant={selecionado ? "contained" : "text"}
                  color={selecionado ? "primary" : "inherit"}
                  onClick={() => {
                    onMudarAnoMes(ano, indiceMes);
                    setModoSelecaoMesAno(false);
                  }}
                  sx={{
                    minWidth: 0,
                    py: 0.5,
                    borderRadius: 4,
                    fontSize: "0.82rem",
                    fontWeight: selecionado ? 700 : 500,
                  }}
                >
                  {nomeMes}
                </Button>
              );
            })}
          </Box>
        </Box>
      ) : (
        <>
          <Box
            sx={{
              display: "grid",
              gridTemplateColumns: "repeat(7, 1fr)",
              textAlign: "center",
              mb: 0.5,
            }}
          >
            {DIAS_SEMANA.map((diaSemana, idx) => (
              <Typography
                key={`${diaSemana}-${idx}`}
                variant="caption"
                sx={{
                  color: "text.secondary",
                  fontWeight: 600,
                  py: 0.5,
                }}
              >
                {diaSemana}
              </Typography>
            ))}
          </Box>

          <Box
            sx={{
              display: "grid",
              gridTemplateColumns: "repeat(7, 1fr)",
              rowGap: 0.5,
            }}
            onMouseLeave={onHoverDia ? () => onHoverDia(null) : undefined}
          >
            {celulas.map((diaData, idx) => {
              if (!diaData) {
                return <Box key={`vazio-${idx}`} sx={{ height: 36 }} />;
              }

              const tempo = diaData.getTime();
              const ehInicio =
                !modoDataUnica && mesmoDia(diaData, intervaloMin);
              const ehFim = !modoDataUnica && mesmoDia(diaData, intervaloMax);
              const selecionado = modoDataUnica
                ? mesmoDia(diaData, dataSelecionada)
                : ehInicio || ehFim;
              const dentroDaFaixa =
                !modoDataUnica &&
                Boolean(intervaloMin && intervaloMax) &&
                tempo >= intervaloMin.getTime() &&
                tempo <= intervaloMax.getTime();
              const apenasPreview =
                !modoDataUnica &&
                selecionandoFim &&
                !dataFim &&
                Boolean(dataHover);
              const ehHoje = mesmoDia(diaData, hoje);
              const desabilitado = diaForaDosLimites(diaData, minLim, maxLim);
              const inicioLinha = idx % 7 === 0;
              const fimLinha = idx % 7 === 6;

              const arredondarEsquerda = ehInicio || inicioLinha;
              const arredondarDireita = ehFim || fimLinha;

              return (
                <Box
                  key={diaData.toISOString()}
                  onMouseEnter={
                    onHoverDia ? () => onHoverDia(diaData) : undefined
                  }
                  sx={{
                    position: "relative",
                    display: "flex",
                    alignItems: "center",
                    justifyContent: "center",
                    height: 36,
                    bgcolor: dentroDaFaixa ? faixaBackground : "transparent",
                    borderTopLeftRadius: arredondarEsquerda ? 18 : 0,
                    borderBottomLeftRadius: arredondarEsquerda ? 18 : 0,
                    borderTopRightRadius: arredondarDireita ? 18 : 0,
                    borderBottomRightRadius: arredondarDireita ? 18 : 0,
                    ...(apenasPreview && dentroDaFaixa
                      ? {
                          borderTop: `1px dashed ${faixaHoverBorda}`,
                          borderBottom: `1px dashed ${faixaHoverBorda}`,
                        }
                      : {}),
                  }}
                >
                  <IconButton
                    size="small"
                    disabled={desabilitado}
                    onClick={() => onSelecionarDia(diaData)}
                    sx={{
                      width: 34,
                      height: 34,
                      fontSize: "0.82rem",
                      fontWeight: selecionado || ehHoje ? 700 : 500,
                      color: selecionado
                        ? "primary.contrastText"
                        : "text.primary",
                      bgcolor: selecionado ? "primary.main" : "transparent",
                      border:
                        ehHoje && !selecionado
                          ? `1px solid ${theme.palette.primary.main}`
                          : "1px solid transparent",
                      "&:hover": {
                        bgcolor: selecionado
                          ? "primary.dark"
                          : alpha(theme.palette.primary.main, 0.16),
                      },
                    }}
                  >
                    {diaData.getDate()}
                  </IconButton>
                </Box>
              );
            })}
          </Box>
        </>
      )}
    </Box>
  );
}
