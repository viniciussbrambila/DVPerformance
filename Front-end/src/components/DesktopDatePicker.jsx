import { useState, useMemo, useRef } from "react";
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
import CalendarioMes, {
  ajustarMesParaAno,
  aplicarMascaraDataBR,
  diaForaDosLimites,
  formatarDataBR,
  normalizarData,
} from "./CalendarioMes";

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

  const minLim = useMemo(() => parseLimiteData(minDate), [minDate]);
  const maxLim = useMemo(() => parseLimiteData(maxDate), [maxDate]);
  const dataSelecionada = useMemo(() => parseDataBR(value), [value]);

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

  const aberto = Boolean(anchorEl);

  const handleAbrir = () => {
    setMesVisivel(obterMesInicial());
    setAnchorEl(containerRef.current);
  };

  const handleFechar = () => {
    setAnchorEl(null);
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
              p: 2,
              borderRadius: 3,
              overflow: "hidden",
              maxWidth: "calc(100vw - 24px)",
            },
          },
        }}
      >
        <CalendarioMes
          ano={mesVisivel.ano}
          mes={mesVisivel.mes}
          onMudarAnoMes={(novoAno, novoMes) =>
            setMesVisivel({ ano: novoAno, mes: novoMes })
          }
          anosDisponiveis={anosDisponiveis}
          minLim={minLim}
          maxLim={maxLim}
          dataSelecionada={dataSelecionada}
          onSelecionarDia={handleSelecionarDia}
          alturaListaAnos={140}
        />

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
      </Popover>
    </>
  );
}
