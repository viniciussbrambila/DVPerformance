import { useState, useMemo, useRef } from "react";
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

function normalizarData(data) {
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

function parseLimiteData(valor) {
  if (!valor) return null;
  if (valor instanceof Date) return normalizarData(valor);
  const match = /^(\d{4})-(\d{2})-(\d{2})$/.exec(String(valor).trim());
  if (!match) return null;
  const ano = Number(match[1]);
  const mes = Number(match[2]) - 1;
  const dia = Number(match[3]);
  const d = new Date(ano, mes, dia, 12, 0, 0, 0);
  if (
    d.getFullYear() !== ano ||
    d.getMonth() !== mes ||
    d.getDate() !== dia
  ) {
    return null;
  }
  return d;
}

function parseDataBR(texto) {
  const match = /^(\d{2})\/(\d{2})\/(\d{4})$/.exec(String(texto || "").trim());
  if (!match) return null;
  const dia = Number(match[1]);
  const mes = Number(match[2]) - 1;
  const ano = Number(match[3]);
  if (ano < 1900 || ano > 2100 || mes < 0 || mes > 11) return null;
  const data = new Date(ano, mes, dia, 12, 0, 0, 0);
  if (
    data.getFullYear() !== ano ||
    data.getMonth() !== mes ||
    data.getDate() !== dia
  ) {
    return null;
  }
  return data;
}

function formatarDataBR(data) {
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

function mesmoDia(a, b) {
  if (!a || !b) return false;
  return (
    a.getFullYear() === b.getFullYear() &&
    a.getMonth() === b.getMonth() &&
    a.getDate() === b.getDate()
  );
}

function diaForaDosLimites(data, minLim, maxLim) {
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

function ajustarMesParaAno(ano, mesDesejado, minLim, maxLim) {
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

export default function DesktopDatePicker({
  label = "Data",
  placeholder = "DD/MM/AAAA",
  name,
  value = "",
  onChange,
  minDate,
  maxDate,
  required = false,
  error = false,
  helperText = "",
  htmlInputProps = {},
  sx,
}) {
  const theme = useTheme();
  const containerRef = useRef(null);
  const [anchorEl, setAnchorEl] = useState(null);
  const [modoSelecaoMesAno, setModoSelecaoMesAno] = useState(false);

  const minLim = useMemo(() => parseLimiteData(minDate), [minDate]);
  const maxLim = useMemo(() => parseLimiteData(maxDate), [maxDate]);
  const dataSelecionada = useMemo(() => parseDataBR(value), [value]);
  const hoje = useMemo(() => normalizarData(new Date()), []);

  const anoMinimo = minLim ? minLim.getFullYear() : 1900;
  const anoMaximo = maxLim ? maxLim.getFullYear() : new Date().getFullYear();

  const obterMesInicial = () => {
    if (dataSelecionada) {
      return {
        ano: dataSelecionada.getFullYear(),
        mes: dataSelecionada.getMonth(),
      };
    }
    const anoReferencia = Math.min(anoMaximo, Math.max(anoMinimo, 2000));
    return {
      ano: anoReferencia,
      mes: ajustarMesParaAno(anoReferencia, 0, minLim, maxLim),
    };
  };

  const [mesVisivel, setMesVisivel] = useState(obterMesInicial);

  const anosDisponiveis = useMemo(() => {
    const lista = [];
    for (let a = anoMaximo; a >= anoMinimo; a -= 1) {
      lista.push(a);
    }
    return lista;
  }, [anoMinimo, anoMaximo]);

  const celulas = useMemo(
    () => gerarDiasCalendario(mesVisivel.ano, mesVisivel.mes),
    [mesVisivel.ano, mesVisivel.mes],
  );

  const aberto = Boolean(anchorEl);

  const handleAbrir = () => {
    setMesVisivel(obterMesInicial());
    setModoSelecaoMesAno(false);
    setAnchorEl(containerRef.current);
  };

  const handleFechar = () => {
    setAnchorEl(null);
    setModoSelecaoMesAno(false);
  };

  const handleInputChange = (e) => {
    const mascarado = aplicarMascaraDataBR(e.target.value);
    onChange(mascarado);
    const dataDigitada = parseDataBR(mascarado);
    if (dataDigitada) {
      setMesVisivel({
        ano: dataDigitada.getFullYear(),
        mes: dataDigitada.getMonth(),
      });
    }
  };

  const handleSelecionarDia = (diaData) => {
    if (diaForaDosLimites(diaData, minLim, maxLim)) return;
    onChange(formatarDataBR(diaData));
    handleFechar();
  };

  const handleLimpar = () => {
    onChange("");
  };

  const prevData = new Date(mesVisivel.ano, mesVisivel.mes - 1, 1);
  const nextData = new Date(mesVisivel.ano, mesVisivel.mes + 1, 1);

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
    setMesVisivel({
      ano: prevData.getFullYear(),
      mes: prevData.getMonth(),
    });
  };

  const irProximoMes = () => {
    if (!podeAvancarMes) return;
    setMesVisivel({
      ano: nextData.getFullYear(),
      mes: nextData.getMonth(),
    });
  };

  return (
    <>
      <Box ref={containerRef} sx={{ width: "100%" }}>
        <TextField
          fullWidth
          required={required}
          type="text"
          label={label}
          placeholder={placeholder}
          name={name}
          value={value}
          onChange={handleInputChange}
          error={error}
          helperText={helperText}
          sx={{
            ...(aberto
              ? {
                  "& .MuiOutlinedInput-notchedOutline": {
                    borderColor: theme.palette.primary.main,
                    borderWidth: 2,
                  },
                }
              : {}),
            ...sx,
          }}
          slotProps={{
            inputLabel: { shrink: true },
            htmlInput: {
              inputMode: "numeric",
              maxLength: 10,
              ...htmlInputProps,
            },
            input: {
              endAdornment: (
                <InputAdornment position="end">
                  <Tooltip title="Abrir calendário">
                    <IconButton
                      size="small"
                      edge="end"
                      onClick={handleAbrir}
                      aria-label={`Abrir calendário de ${label.toLowerCase()}`}
                    >
                      <DateRangeOutlinedIcon
                        fontSize="small"
                        color={aberto ? "primary" : "action"}
                      />
                    </IconButton>
                  </Tooltip>
                </InputAdornment>
              ),
            },
          }}
        />
      </Box>

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
          <Box sx={{ width: 288, userSelect: "none" }}>
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
                {MESES[mesVisivel.mes]} {mesVisivel.ano}
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
                    maxHeight: 140,
                    overflowY: "auto",
                    mb: 1.5,
                    pr: 0.5,
                  }}
                >
                  {anosDisponiveis.map((anoOpcao) => {
                    const selecionado = anoOpcao === mesVisivel.ano;
                    return (
                      <Button
                        key={anoOpcao}
                        size="small"
                        variant={selecionado ? "contained" : "text"}
                        color={selecionado ? "primary" : "inherit"}
                        onClick={() =>
                          setMesVisivel((atual) => ({
                            ano: anoOpcao,
                            mes: ajustarMesParaAno(
                              anoOpcao,
                              atual.mes,
                              minLim,
                              maxLim,
                            ),
                          }))
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
                    const selecionado = indiceMes === mesVisivel.mes;
                    const desabilitado = mesForaDosLimites(
                      mesVisivel.ano,
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
                          setMesVisivel((atual) => ({
                            ...atual,
                            mes: indiceMes,
                          }));
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
                >
                  {celulas.map((diaData, idx) => {
                    if (!diaData) {
                      return <Box key={`vazio-${idx}`} sx={{ height: 36 }} />;
                    }

                    const selecionado = mesmoDia(diaData, dataSelecionada);
                    const ehHoje = mesmoDia(diaData, hoje);
                    const desabilitado = diaForaDosLimites(
                      diaData,
                      minLim,
                      maxLim,
                    );

                    return (
                      <Box
                        key={diaData.toISOString()}
                        sx={{
                          display: "flex",
                          alignItems: "center",
                          justifyContent: "center",
                          height: 36,
                        }}
                      >
                        <IconButton
                          size="small"
                          disabled={desabilitado}
                          onClick={() => handleSelecionarDia(diaData)}
                          sx={{
                            width: 34,
                            height: 34,
                            fontSize: "0.82rem",
                            fontWeight: selecionado || ehHoje ? 700 : 500,
                            color: selecionado
                              ? "primary.contrastText"
                              : "text.primary",
                            bgcolor: selecionado
                              ? "primary.main"
                              : "transparent",
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

            <Divider sx={{ mt: 1.5, mb: 1.5 }} />

            <Box
              sx={{
                display: "flex",
                justifyContent: "space-between",
                alignItems: "center",
                gap: 1,
              }}
            >
              <Button
                size="small"
                color="inherit"
                onClick={handleLimpar}
                disabled={!value}
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
