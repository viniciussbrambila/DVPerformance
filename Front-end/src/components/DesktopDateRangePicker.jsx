import { useState, useMemo } from "react";
import {
  Box,
  Paper,
  Popover,
  TextField,
  Typography,
  IconButton,
  Button,
  Divider,
  InputAdornment,
  Tooltip,
} from "@mui/material";
import { alpha, useTheme } from "@mui/material/styles";
import DateRangeOutlinedIcon from "@mui/icons-material/DateRangeOutlined";
import ChevronLeftIcon from "@mui/icons-material/ChevronLeft";
import ChevronRightIcon from "@mui/icons-material/ChevronRight";
import ArrowDropDownIcon from "@mui/icons-material/ArrowDropDown";
import ArrowForwardIcon from "@mui/icons-material/ArrowForward";
import CloseIcon from "@mui/icons-material/Close";

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

const MESES_CURTOS = [
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
const ANO_MINIMO = 2026;

function normalizarData(data) {
  if (!data) return null;
  const d = new Date(data.getFullYear(), data.getMonth(), data.getDate(), 12, 0, 0, 0);
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

function formatarDataBR(data) {
  if (!data) return "";
  const dia = String(data.getDate()).padStart(2, "0");
  const mes = String(data.getMonth() + 1).padStart(2, "0");
  const ano = data.getFullYear();
  return `${dia}/${mes}/${ano}`;
}

function aplicarMascaraData(valor) {
  return valor
    .replace(/\D/g, "")
    .slice(0, 8)
    .replace(/^(\d{2})(\d)/, "$1/$2")
    .replace(/^(\d{2}\/\d{2})(\d)/, "$1/$2");
}

function parseTextoData(texto, fimDoMesSeMesAno = false) {
  const limpo = texto.trim();
  // Suporta DD/MM/AAAA
  const matchCompleto = /^(\d{2})\/(\d{2})\/(\d{4})$/.exec(limpo);
  if (matchCompleto) {
    const dia = Number(matchCompleto[1]);
    const mes = Number(matchCompleto[2]) - 1;
    const ano = Number(matchCompleto[3]);
    if (ano < ANO_MINIMO || ano > 2100 || mes < 0 || mes > 11) return null;
    const data = new Date(ano, mes, dia, 12, 0, 0, 0);
    if (
      data.getFullYear() === ano &&
      data.getMonth() === mes &&
      data.getDate() === dia
    ) {
      return data;
    }
    return null;
  }

  // Suporta MM/AAAA (ex.: 05/2026 -> 01/05/2026 ou último dia de 07/2026)
  const matchMesAno = /^(\d{2})\/(\d{4})$/.exec(limpo);
  if (matchMesAno) {
    const mes = Number(matchMesAno[1]) - 1;
    const ano = Number(matchMesAno[2]);
    if (ano < ANO_MINIMO || ano > 2100 || mes < 0 || mes > 11) return null;
    return fimDoMesSeMesAno
      ? new Date(ano, mes + 1, 0, 12, 0, 0, 0)
      : new Date(ano, mes, 1, 12, 0, 0, 0);
  }

  return null;
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

function CalendarioMes({
  titulo,
  ano,
  mes,
  onMudarAnoMes,
  dataInicio,
  dataFim,
  dataHover,
  selecionandoFim,
  onSelecionarDia,
  onHoverDia,
  anosDisponiveis,
}) {
  const theme = useTheme();
  const [modoSelecaoMesAno, setModoSelecaoMesAno] = useState(false);
  const hoje = useMemo(() => normalizarData(new Date()), []);
  const celulas = useMemo(() => gerarDiasCalendario(ano, mes), [ano, mes]);

  const podeVoltarMes = !(ano <= ANO_MINIMO && mes === 0);

  const irMesAnterior = () => {
    if (!podeVoltarMes) return;
    const novaData = new Date(ano, mes - 1, 1);
    onMudarAnoMes(novaData.getFullYear(), novaData.getMonth());
  };

  const irProximoMes = () => {
    const novaData = new Date(ano, mes + 1, 1);
    onMudarAnoMes(novaData.getFullYear(), novaData.getMonth());
  };

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

      {/* Cabeçalho estilo MUI DateCalendar */}
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
                transform: modoSelecaoMesAno ? "rotate(180deg)" : "rotate(0deg)",
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
              maxHeight: 112,
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
                  onClick={() => onMudarAnoMes(anoOpcao, mes)}
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
              return (
                <Button
                  key={nomeMes}
                  size="small"
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
          {/* Cabeçalho dos dias da semana */}
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

          {/* Grade de dias estilo MUI DateRangePickerDay */}
          <Box
            sx={{
              display: "grid",
              gridTemplateColumns: "repeat(7, 1fr)",
              rowGap: 0.5,
            }}
            onMouseLeave={() => onHoverDia(null)}
          >
            {celulas.map((diaData, idx) => {
              if (!diaData) {
                return <Box key={`vazio-${idx}`} sx={{ height: 36 }} />;
              }

              const tempo = diaData.getTime();
              const ehInicio = mesmoDia(diaData, intervaloMin);
              const ehFim = mesmoDia(diaData, intervaloMax);
              const selecionado = ehInicio || ehFim;
              const dentroDaFaixa =
                Boolean(intervaloMin && intervaloMax) &&
                tempo >= intervaloMin.getTime() &&
                tempo <= intervaloMax.getTime();
              const apenasPreview =
                selecionandoFim && !dataFim && Boolean(dataHover);
              const ehHoje = mesmoDia(diaData, hoje);
              const inicioLinha = idx % 7 === 0;
              const fimLinha = idx % 7 === 6;

              const arredondarEsquerda = ehInicio || inicioLinha;
              const arredondarDireita = ehFim || fimLinha;

              return (
                <Box
                  key={diaData.toISOString()}
                  onMouseEnter={() => onHoverDia(diaData)}
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

export default function DesktopDateRangePicker({
  label = "Período de cadastro",
  startDate,
  endDate,
  onChange,
  sx,
}) {
  const theme = useTheme();
  const [anchorEl, setAnchorEl] = useState(null);
  const [dataHover, setDataHover] = useState(null);
  const [selecionandoFim, setSelecionandoFim] = useState(false);

  const hoje = useMemo(() => new Date(), []);

  const [mesEsquerda, setMesEsquerda] = useState(() => {
    if (startDate) {
      return {
        ano: Math.max(ANO_MINIMO, startDate.getFullYear()),
        mes: startDate.getMonth(),
      };
    }
    const anterior = new Date(hoje.getFullYear(), hoje.getMonth() - 1, 1);
    if (anterior.getFullYear() < ANO_MINIMO) {
      return { ano: ANO_MINIMO, mes: 0 };
    }
    return { ano: anterior.getFullYear(), mes: anterior.getMonth() };
  });

  const [mesDireita, setMesDireita] = useState(() => {
    if (endDate) {
      return {
        ano: Math.max(ANO_MINIMO, endDate.getFullYear()),
        mes: endDate.getMonth(),
      };
    }
    if (hoje.getFullYear() <= ANO_MINIMO && hoje.getMonth() === 0) {
      return { ano: ANO_MINIMO, mes: 1 };
    }
    return {
      ano: Math.max(ANO_MINIMO, hoje.getFullYear()),
      mes: hoje.getMonth(),
    };
  });

  const [textoInicio, setTextoInicio] = useState("");
  const [textoFim, setTextoFim] = useState("");

  const anosDisponiveis = useMemo(() => {
    const anoFinal = Math.max(ANO_MINIMO, new Date().getFullYear());
    const lista = [];
    for (let a = ANO_MINIMO; a <= anoFinal; a += 1) {
      lista.push(a);
    }
    return lista;
  }, []);

  const aberto = Boolean(anchorEl);

  const sincronizarTextos = (inicio, fim) => {
    setTextoInicio(inicio ? formatarDataBR(inicio) : "");
    setTextoFim(fim ? formatarDataBR(fim) : "");
  };

  const handleAbrir = (event) => {
    sincronizarTextos(startDate, endDate);
    if (startDate) {
      setMesEsquerda({
        ano: startDate.getFullYear(),
        mes: startDate.getMonth(),
      });
    }
    if (endDate) {
      const mesmoMesAno =
        startDate &&
        startDate.getFullYear() === endDate.getFullYear() &&
        startDate.getMonth() === endDate.getMonth();
      if (mesmoMesAno) {
        const seguinte = new Date(
          endDate.getFullYear(),
          endDate.getMonth() + 1,
          1,
        );
        setMesDireita({
          ano: seguinte.getFullYear(),
          mes: seguinte.getMonth(),
        });
      } else {
        setMesDireita({
          ano: endDate.getFullYear(),
          mes: endDate.getMonth(),
        });
      }
    }
    setSelecionandoFim(Boolean(startDate && !endDate));
    setAnchorEl(event.currentTarget);
  };

  const handleFechar = () => {
    setAnchorEl(null);
    setDataHover(null);
    setSelecionandoFim(false);
  };

  const handleSelecionarDia = (diaClicado) => {
    const dataNorm = normalizarData(diaClicado);
    if (!startDate || (startDate && endDate) || !selecionandoFim) {
      onChange([dataNorm, null]);
      sincronizarTextos(dataNorm, null);
      setSelecionandoFim(true);
      return;
    }

    if (dataNorm.getTime() < startDate.getTime()) {
      onChange([dataNorm, startDate]);
      sincronizarTextos(dataNorm, startDate);
    } else {
      onChange([startDate, dataNorm]);
      sincronizarTextos(startDate, dataNorm);
    }
    setSelecionandoFim(false);
    setDataHover(null);
  };

  const handleChangeTextoInicio = (e) => {
    const valor = aplicarMascaraData(e.target.value);
    setTextoInicio(valor);
    if (valor === "") {
      onChange([null, endDate]);
      return;
    }
    const dataValida = parseTextoData(valor, false);
    if (dataValida) {
      setMesEsquerda({
        ano: dataValida.getFullYear(),
        mes: dataValida.getMonth(),
      });
      if (endDate && dataValida.getTime() > endDate.getTime()) {
        onChange([dataValida, null]);
      } else {
        onChange([dataValida, endDate]);
      }
    }
  };

  const handleChangeTextoFim = (e) => {
    const valor = aplicarMascaraData(e.target.value);
    setTextoFim(valor);
    if (valor === "") {
      onChange([startDate, null]);
      return;
    }
    const dataValida = parseTextoData(valor, true);
    if (dataValida) {
      setMesDireita({
        ano: dataValida.getFullYear(),
        mes: dataValida.getMonth(),
      });
      if (startDate && dataValida.getTime() < startDate.getTime()) {
        onChange([dataValida, startDate]);
        sincronizarTextos(dataValida, startDate);
      } else {
        onChange([startDate, dataValida]);
      }
    }
  };

  const handleSelecionarIntervaloMeses = () => {
    const inicioMesEsq = new Date(
      mesEsquerda.ano,
      mesEsquerda.mes,
      1,
      12,
      0,
      0,
      0,
    );
    const fimMesDir = new Date(
      mesDireita.ano,
      mesDireita.mes + 1,
      0,
      12,
      0,
      0,
      0,
    );
    if (inicioMesEsq <= fimMesDir) {
      onChange([inicioMesEsq, fimMesDir]);
      sincronizarTextos(inicioMesEsq, fimMesDir);
    } else {
      const inicioInvertido = new Date(
        mesDireita.ano,
        mesDireita.mes,
        1,
        12,
        0,
        0,
        0,
      );
      const fimInvertido = new Date(
        mesEsquerda.ano,
        mesEsquerda.mes + 1,
        0,
        12,
        0,
        0,
        0,
      );
      onChange([inicioInvertido, fimInvertido]);
      sincronizarTextos(inicioInvertido, fimInvertido);
    }
    setSelecionandoFim(false);
  };

  const handleLimpar = (e) => {
    if (e) e.stopPropagation();
    onChange([null, null]);
    setTextoInicio("");
    setTextoFim("");
    setSelecionandoFim(false);
    setDataHover(null);
  };

  const valorExibido = useMemo(() => {
    if (startDate && endDate) {
      return `${formatarDataBR(startDate)} – ${formatarDataBR(endDate)}`;
    }
    if (startDate) {
      return `${formatarDataBR(startDate)} – ...`;
    }
    if (endDate) {
      return `... – ${formatarDataBR(endDate)}`;
    }
    return "";
  }, [startDate, endDate]);

  return (
    <>
      <TextField
        label={label}
        placeholder="DD/MM/AAAA – DD/MM/AAAA"
        value={valorExibido}
        onClick={handleAbrir}
        sx={{
          minWidth: { xs: "100%", sm: 275 },
          cursor: "pointer",
          "& .MuiOutlinedInput-root": {
            cursor: "pointer",
            ...(aberto
              ? {
                  "& .MuiOutlinedInput-notchedOutline": {
                    borderColor: theme.palette.primary.main,
                    borderWidth: 2,
                  },
                }
              : {}),
          },
          "& input": {
            cursor: "pointer",
          },
          ...sx,
        }}
        slotProps={{
          inputLabel: { shrink: true },
          htmlInput: {
            readOnly: true,
            "aria-label": label,
          },
          input: {
            startAdornment: (
              <InputAdornment position="start">
                <DateRangeOutlinedIcon fontSize="small" color="action" />
              </InputAdornment>
            ),
            endAdornment: (startDate || endDate) && (
              <InputAdornment position="end">
                <Tooltip title="Limpar período">
                  <IconButton
                    size="small"
                    edge="end"
                    aria-label="Limpar período selecionado"
                    onClick={handleLimpar}
                  >
                    <CloseIcon fontSize="small" />
                  </IconButton>
                </Tooltip>
              </InputAdornment>
            ),
          },
        }}
      />

      <Popover
        open={aberto}
        anchorEl={anchorEl}
        onClose={handleFechar}
        anchorOrigin={{ vertical: "bottom", horizontal: "left" }}
        transformOrigin={{ vertical: "top", horizontal: "left" }}
        slotProps={{
          paper: {
            elevation: 8,
            sx: {
              mt: 1,
              borderRadius: 3,
              overflow: "hidden",
              maxWidth: "calc(100vw - 24px)",
            },
          },
        }}
      >
        <Paper
          elevation={0}
          sx={{
            p: 2,
            background: "transparent",
            boxShadow: "none",
            border: "none",
          }}
        >
          {/* Campos de entrada rápida (MultiInputDateRangeField do MUI) */}
          <Box
            sx={{
              display: "flex",
              alignItems: "center",
              gap: 1.5,
              mb: 2,
              flexWrap: { xs: "wrap", sm: "nowrap" },
            }}
          >
            <TextField
              label="Data inicial"
              placeholder="DD/MM/AAAA ou MM/AAAA"
              value={textoInicio}
              onChange={handleChangeTextoInicio}
              size="small"
              fullWidth
              slotProps={{ inputLabel: { shrink: true } }}
            />
            <ArrowForwardIcon
              fontSize="small"
              sx={{
                color: "text.secondary",
                display: { xs: "none", sm: "block" },
                flexShrink: 0,
              }}
            />
            <TextField
              label="Data final"
              placeholder="DD/MM/AAAA ou MM/AAAA"
              value={textoFim}
              onChange={handleChangeTextoFim}
              size="small"
              fullWidth
              slotProps={{ inputLabel: { shrink: true } }}
            />
          </Box>

          <Divider sx={{ mb: 1.5 }} />

          {/* Dois calendários lado a lado (DesktopDateRangePicker calendars={2}) */}
          <Box
            sx={{
              display: "flex",
              flexDirection: { xs: "column", md: "row" },
              gap: 2,
              alignItems: "flex-start",
            }}
          >
            <CalendarioMes
              titulo="Calendário inicial"
              ano={mesEsquerda.ano}
              mes={mesEsquerda.mes}
              onMudarAnoMes={(novoAno, novoMes) =>
                setMesEsquerda({ ano: novoAno, mes: novoMes })
              }
              dataInicio={startDate}
              dataFim={endDate}
              dataHover={dataHover}
              selecionandoFim={selecionandoFim}
              onSelecionarDia={handleSelecionarDia}
              onHoverDia={setDataHover}
              anosDisponiveis={anosDisponiveis}
            />

            <Divider
              orientation="vertical"
              flexItem
              sx={{ display: { xs: "none", md: "block" } }}
            />
            <Divider sx={{ display: { xs: "block", md: "none" }, width: "100%" }} />

            <CalendarioMes
              titulo="Calendário final"
              ano={mesDireita.ano}
              mes={mesDireita.mes}
              onMudarAnoMes={(novoAno, novoMes) =>
                setMesDireita({ ano: novoAno, mes: novoMes })
              }
              dataInicio={startDate}
              dataFim={endDate}
              dataHover={dataHover}
              selecionandoFim={selecionandoFim}
              onSelecionarDia={handleSelecionarDia}
              onHoverDia={setDataHover}
              anosDisponiveis={anosDisponiveis}
            />
          </Box>

          <Divider sx={{ mt: 1.5, mb: 1.5 }} />

          {/* Barra inferior de ações */}
          <Box
            sx={{
              display: "flex",
              justifyContent: "space-between",
              alignItems: "center",
              flexWrap: "wrap",
              gap: 1,
            }}
          >
            <Tooltip title="Seleciona do dia 1º do mês esquerdo até o último dia do mês direito">
              <Button
                size="small"
                variant="outlined"
                color="primary"
                onClick={handleSelecionarIntervaloMeses}
              >
                Usar {MESES_CURTOS[mesEsquerda.mes]}/{mesEsquerda.ano} até{" "}
                {MESES_CURTOS[mesDireita.mes]}/{mesDireita.ano}
              </Button>
            </Tooltip>

            <Box sx={{ display: "flex", gap: 1, ml: "auto" }}>
              <Button
                size="small"
                color="inherit"
                onClick={handleLimpar}
                disabled={!startDate && !endDate}
              >
                Limpar
              </Button>
              <Button
                size="small"
                variant="contained"
                color="primary"
                onClick={handleFechar}
              >
                Concluir
              </Button>
            </Box>
          </Box>
        </Paper>
      </Popover>
    </>
  );
}
