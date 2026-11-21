import { useState, useMemo } from "react";
import {
  Box,
  Popover,
  TextField,
  IconButton,
  Button,
  Divider,
  InputAdornment,
  Tooltip,
} from "@mui/material";
import { useTheme } from "@mui/material/styles";
import DateRangeOutlinedIcon from "@mui/icons-material/DateRangeOutlined";
import ArrowForwardIcon from "@mui/icons-material/ArrowForward";
import CloseIcon from "@mui/icons-material/Close";
import CalendarioMes, {
  PROPS_POPOVER_CALENDARIO,
  MESES_CURTOS,
  aplicarMascaraDataBR,
  formatarDataBR,
  normalizarData,
} from "./CalendarioMes";

const ANO_MINIMO = 2026;
const LIMITE_MINIMO_PADRAO = new Date(ANO_MINIMO, 0, 1, 12, 0, 0, 0);

function parseTextoData(texto, fimDoMesSeMesAno = false) {
  const limpo = texto.trim();
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
    const valor = aplicarMascaraDataBR(e.target.value);
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
    const valor = aplicarMascaraDataBR(e.target.value);
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
        {...PROPS_POPOVER_CALENDARIO}
      >
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
            fullWidth
            slotProps={{ inputLabel: { shrink: true } }}
          />
        </Box>

        <Divider sx={{ mb: 1.5 }} />

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
            minLim={LIMITE_MINIMO_PADRAO}
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
            minLim={LIMITE_MINIMO_PADRAO}
            dataInicio={startDate}
            dataFim={endDate}
            dataHover={dataHover}
            selecionandoFim={selecionandoFim}
            onSelecionarDia={handleSelecionarDia}
            onHoverDia={setDataHover}
            anosDisponiveis={anosDisponiveis}
          />
        </Box>

        <Divider sx={{ my: 1.5 }} />

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
      </Popover>
    </>
  );
}
