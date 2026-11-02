import {
  Box,
  Paper,
  Typography,
  Button,
  Divider,
  TextField,
  Alert,
  InputAdornment,
  GlobalStyles,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  CircularProgress,
} from "@mui/material";
import { useState, useEffect } from "react";
import { useNavigate, useParams } from "react-router-dom";
import ArrowBackIcon from "@mui/icons-material/ArrowBack";
import CheckCircleOutlinedIcon from "@mui/icons-material/CheckCircleOutlined";
import EditOutlinedIcon from "@mui/icons-material/EditOutlined";
import PrintOutlinedIcon from "@mui/icons-material/PrintOutlined";
import CloseIcon from "@mui/icons-material/Close";
import SaveOutlinedIcon from "@mui/icons-material/SaveOutlined";
import {
  buscarComCache,
  lerCache,
  salvarCache,
  temCache,
  removerCache,
  atualizarItemEmListasCache,
} from "../utils/cacheSessao";

const API_URL = "http://localhost:5011/treinos";

const formatarData = (iso) => new Date(iso).toLocaleDateString("pt-BR");

// Peso e altura ainda são mantidos somente no cache da sessão do front-end.
const obterMedidasAluno = (alunoId) => {
  try {
    const usuario = JSON.parse(localStorage.getItem("usuarioLogado") || "null");
    if (!usuario?.id) return {};
    return lerCache(`medidas:alunos:${usuario.id}`)?.[alunoId] || {};
  } catch {
    return {};
  }
};

const formatarMedida = (valor, unidade) =>
  valor == null || valor === ""
    ? "Não informado"
    : `${Number(valor).toLocaleString("pt-BR", { maximumFractionDigits: 2 })} ${unidade}`;

const cabecalhoSx = {
  fontWeight: 700,
  textTransform: "uppercase",
  color: "text.secondary",
};

const CAMPOS = ["series", "repeticoes", "descanso"];

const LIMITES = {
  series: { min: 1, max: 10 },
  repeticoes: { min: 6, max: 30 },
  descanso: { min: 30, max: 300 },
};

function validarCampo(campo, valor) {
  const { min, max } = LIMITES[campo];
  const texto = String(valor).trim();

  if (campo === "repeticoes") {
    const m = texto.match(/^(\d+)(?:\s*-\s*(\d+))?$/);
    if (!m) return "Ex.: 8-12";

    const inicio = Number(m[1]);
    const fim = m[2] !== undefined ? Number(m[2]) : inicio;

    if (inicio < min || inicio > max || fim < min || fim > max)
      return `${min} a ${max}`;
    if (inicio > fim) return "Ordem inválida";
    return "";
  }

  if (!/^\d+$/.test(texto)) return `${min} a ${max}`;
  const numero = Number(texto);
  return numero < min || numero > max ? `${min} a ${max}` : "";
}

const normalizarRepeticoes = (valor) =>
  String(valor)
    .trim()
    .replace(/\s*-\s*/, "-");

export default function PlanoDetalhe({ glassStyle }) {
  const { id } = useParams();
  const navigate = useNavigate();

  const [plano, setPlano] = useState(
    () => lerCache(`planoDetalhe:${id}`) || null,
  );
  const [carregando, setCarregando] = useState(
    () => !temCache(`planoDetalhe:${id}`),
  );
  const [erroApi, setErroApi] = useState("");
  const [salvando, setSalvando] = useState(false);

  const [editando, setEditando] = useState(false);
  const [diasEdicao, setDiasEdicao] = useState([]);

  useEffect(() => {
    const chave = `planoDetalhe:${id}`;
    const salvo = lerCache(chave);

    if (salvo) {
      setPlano(salvo);
      setCarregando(false);
    } else {
      setPlano(null);
      setCarregando(true);
    }

    const carregar = async () => {
      setErroApi("");

      try {
        const dados = await buscarComCache(chave, async () => {
          const response = await fetch(`${API_URL}/${id}`);
          if (response.status === 404) {
            const erro404 = new Error("NAO_ENCONTRADO");
            erro404.status = 404;
            throw erro404;
          }
          if (!response.ok) {
            throw new Error("Não foi possível carregar a ficha de treino.");
          }
          return response.json();
        });
        setPlano(dados);
      } catch (err) {
        if (err?.status === 404) {
          removerCache(chave);
          setPlano(null);
          return;
        }
        console.error("Erro ao carregar detalhes do plano:", err);
        if (!temCache(chave)) {
          setErroApi(
            err.message || "Não foi possível carregar os detalhes deste plano.",
          );
        }
      } finally {
        setCarregando(false);
      }
    };

    carregar();
  }, [id]);

  if (carregando) {
    return (
      <Box sx={{ display: "flex", justifyContent: "center", py: 8 }}>
        <CircularProgress />
      </Box>
    );
  }

  if (!plano) {
    return (
      <Box>
        {erroApi && (
          <Alert severity="error" sx={{ mb: 2 }}>
            {erroApi}
          </Alert>
        )}
        <Typography variant="h5" sx={{ fontWeight: "bold", mb: 2 }}>
          Plano não encontrado
        </Typography>
        <Button
          variant="contained"
          startIcon={<ArrowBackIcon />}
          onClick={() => navigate("/planos")}
        >
          Voltar para Planos
        </Button>
      </Box>
    );
  }

  const dias = editando ? diasEdicao : plano.dias || [];

  const erros = {};
  if (editando) {
    diasEdicao.forEach((dia, i) => {
      dia.exercicios.forEach((ex, j) => {
        CAMPOS.forEach((campo) => {
          const mensagem = validarCampo(campo, ex[campo]);
          if (mensagem) erros[`${i}-${j}-${campo}`] = mensagem;
        });
      });
    });
  }
  const temErros = Object.keys(erros).length > 0;

  const iniciarEdicao = () => {
    setErroApi("");
    setDiasEdicao(
      (plano.dias || []).map((dia) => ({
        ...dia,
        exercicios: (dia.exercicios || []).map((ex) => ({
          ...ex,
          series: String(ex.series),
          descanso: String(parseInt(ex.descanso, 10) || 60),
        })),
      })),
    );
    setEditando(true);
  };

  const alterarCampo = (indiceDia, indiceEx, campo, valor) => {
    setDiasEdicao((lista) =>
      lista.map((dia, i) =>
        i !== indiceDia
          ? dia
          : {
              ...dia,
              exercicios: dia.exercicios.map((ex, j) =>
                j !== indiceEx ? ex : { ...ex, [campo]: valor },
              ),
            },
      ),
    );
  };

  const salvarEdicao = async () => {
    if (temErros) return;
    setSalvando(true);
    setErroApi("");

    const diasFinal = diasEdicao.map((dia) => ({
      ...dia,
      exercicios: dia.exercicios.map((ex) => ({
        ...ex,
        series: Number(ex.series),
        repeticoes: normalizarRepeticoes(ex.repeticoes),
        descanso: `${Number(ex.descanso)}s`,
      })),
    }));

    const originaisPorId = new Map();
    (plano.dias || []).forEach((d) =>
      (d.exercicios || []).forEach((ex) => originaisPorId.set(ex.id, ex)),
    );

    const itensModificados = [];
    diasFinal.forEach((d) =>
      d.exercicios.forEach((ex) => {
        const orig = originaisPorId.get(ex.id);
        if (
          !orig ||
          orig.series !== ex.series ||
          orig.repeticoes !== ex.repeticoes ||
          orig.descanso !== ex.descanso
        ) {
          itensModificados.push(ex);
        }
      }),
    );

    try {
      await Promise.all(
        itensModificados.map(async (item) => {
          const response = await fetch(`${API_URL}/itens/${item.id}`, {
            method: "PUT",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({
              id: Number(item.id),
              treinoId: Number(item.treinoId),
              divisaoTreino: String(item.divisaoTreino),
              ordemExercicio: Number(item.ordemExercicio),
              exercicioId:
                item.exercicioId != null ? Number(item.exercicioId) : null,
              maquinaId: item.maquinaId != null ? Number(item.maquinaId) : null,
              series: Number(item.series),
              repeticao: String(item.repeticoes),
              tempoDescanso: String(item.descanso),
              tempoAerobico: String(item.tempoAerobico || ""),
            }),
          });

          if (!response.ok) {
            const dados = await response.json().catch(() => null);
            throw new Error(
              dados?.mensagem || "Não foi possível salvar o item do treino.",
            );
          }
        }),
      );

      setPlano((atual) => {
        const atualizado = { ...atual, dias: diasFinal };
        salvarCache(`planoDetalhe:${atual.id}`, atualizado);
        return atualizado;
      });
      atualizarItemEmListasCache("planos:", plano.id, { dias: diasFinal });
      setEditando(false);
    } catch (err) {
      console.error("Erro ao salvar itens do treino:", err);
      setErroApi(
        err.message || "Não foi possível salvar as alterações no servidor.",
      );
    } finally {
      setSalvando(false);
    }
  };

  const atualizarStatusNoBanco = async (novoStatus) => {
    try {
      const response = await fetch(`${API_URL}/${plano.id}/status`, {
        method: "PATCH",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ status: novoStatus }),
      });

      if (!response.ok) {
        const dados = await response.json().catch(() => null);
        setErroApi(
          dados?.mensagem || "Não foi possível atualizar o status do plano.",
        );
        return false;
      }

      setPlano((atual) => {
        const atualizado = { ...atual, status: novoStatus };
        salvarCache(`planoDetalhe:${atual.id}`, atualizado);
        return atualizado;
      });
      atualizarItemEmListasCache("planos:", plano.id, { status: novoStatus });
      return true;
    } catch (err) {
      console.error("Erro ao atualizar status:", err);
      setErroApi("Erro de conexão ao atualizar o status do plano.");
      return false;
    }
  };

  const validar = () => {
    atualizarStatusNoBanco("Validado");
  };

  const exportarPdf = async () => {
    const ok = await atualizarStatusNoBanco("Exportado");
    if (!ok) return;

    setTimeout(() => {
      const tituloAnterior = document.title;
      document.title = `plano-${plano.aluno.toLowerCase().replace(/\s+/g, "-")}`;
      window.print();
      document.title = tituloAnterior;
    }, 300);
  };

  const medidasAluno = obterMedidasAluno(plano.alunoId);
  const infos = [
    ["Aluno", plano.aluno],
    ["Peso", formatarMedida(medidasAluno.peso, "kg")],
    ["Altura", formatarMedida(medidasAluno.altura, "m")],
    ["Objetivo", plano.objetivo],
    ["Frequência", `${plano.frequencia} dias/semana`],
    ["Data", formatarData(plano.data)],
  ];

  return (
    <Box>
      <GlobalStyles
        styles={{
          "@media print": {
            ".MuiDrawer-root, .no-print": { display: "none !important" },
            "html, body": { background: "#fff !important" },
            main: { padding: "0 !important", overflow: "visible !important" },
          },
        }}
      />

      <Box
        className="no-print"
        sx={{
          display: "flex",
          justifyContent: "space-between",
          alignItems: "flex-start",
          flexWrap: "wrap",
          gap: 2,
          mb: 3,
        }}
      >
        <Box sx={{ display: "flex", alignItems: "center", gap: 2 }}>
          <Button
            color="inherit"
            startIcon={<ArrowBackIcon />}
            onClick={() => navigate("/planos")}
            sx={{ color: "text.secondary" }}
          >
            Planos
          </Button>

          <Typography variant="h4" sx={{ fontWeight: "bold" }}>
            Plano de Treino
          </Typography>
        </Box>

        <Box sx={{ display: "flex", gap: 1.5, flexWrap: "wrap" }}>
          {editando ? (
            <>
              <Button
                color="inherit"
                disabled={salvando}
                startIcon={<CloseIcon />}
                onClick={() => {
                  setErroApi("");
                  setEditando(false);
                }}
              >
                Cancelar
              </Button>
              <Button
                variant="contained"
                startIcon={<SaveOutlinedIcon />}
                onClick={salvarEdicao}
                disabled={temErros || salvando}
              >
                {salvando ? "Salvando..." : "Salvar"}
              </Button>
            </>
          ) : (
            <>
              {plano.status === "Rascunho" && (
                <Button
                  variant="contained"
                  color="success"
                  size="large"
                  startIcon={<CheckCircleOutlinedIcon />}
                  onClick={validar}
                >
                  Validar Plano
                </Button>
              )}

              <Button
                color="inherit"
                size="large"
                startIcon={<EditOutlinedIcon />}
                onClick={iniciarEdicao}
                sx={{ bgcolor: "action.hover" }}
              >
                Editar
              </Button>

              <Button
                variant="contained"
                size="large"
                startIcon={<PrintOutlinedIcon />}
                onClick={exportarPdf}
                sx={{
                  bgcolor: "text.primary",
                  color: "background.default",
                  "&:hover": { bgcolor: "text.secondary" },
                }}
              >
                Exportar PDF
              </Button>
            </>
          )}
        </Box>
      </Box>

      {erroApi && (
        <Alert severity="error" className="no-print" sx={{ mb: 3 }}>
          {erroApi}
        </Alert>
      )}

      {editando && (
        <Alert severity="info" className="no-print" sx={{ mb: 3 }}>
          Limites: séries de 1 a 10 · repetições de 6 a 30 (ex.: 8-12) ·
          descanso de 30 a 300 segundos.
        </Alert>
      )}

      <Paper
        elevation={0}
        sx={{
          ...glassStyle,
          p: { xs: 3, md: 5 },
          borderRadius: 4,
          "@media print": {
            background: "#fff !important",
            color: "#000 !important",
            boxShadow: "none !important",
            border: "none !important",
            backdropFilter: "none !important",
            "& .MuiTypography-root, & .MuiTableCell-root": {
              color: "#000 !important",
            },
            "& .info-faixa": {
              backgroundColor: "#f3f4f6 !important",
              printColorAdjust: "exact",
              WebkitPrintColorAdjust: "exact",
            },
            "& .rotulo-dia": {
              backgroundColor: "#000 !important",
              color: "#fff !important",
              printColorAdjust: "exact",
              WebkitPrintColorAdjust: "exact",
            },
          },
        }}
      >
        <Box sx={{ textAlign: "center", mb: 3 }}>
          <Typography variant="h3" sx={{ fontWeight: 800 }}>
            DV PERFORMANCE
          </Typography>
          <Typography
            variant="body2"
            color="text.secondary"
            sx={{ letterSpacing: 3 }}
          >
            PRESCRIÇÃO DE TREINAMENTO
          </Typography>
        </Box>

        <Divider
          sx={{ borderBottomWidth: 2, borderColor: "text.primary", mb: 4 }}
        />

        <Box
          className="info-faixa"
          sx={{
            display: "grid",
            gridTemplateColumns: {
              xs: "repeat(2, 1fr)",
              md: "repeat(3, 1fr)",
              lg: "repeat(6, 1fr)",
            },
            gap: 2,
            p: 2.5,
            mb: 5,
            borderRadius: 3,
            bgcolor: "action.hover",
          }}
        >
          {infos.map(([rotulo, valor]) => (
            <Box key={rotulo}>
              <Typography
                variant="caption"
                color="text.secondary"
                sx={{ fontWeight: 700, textTransform: "uppercase" }}
              >
                {rotulo}
              </Typography>
              <Typography variant="h6" sx={{ fontWeight: 600 }}>
                {valor}
              </Typography>
            </Box>
          ))}
        </Box>

        {dias.map((dia, indiceDia) => (
          <Box
            key={`${dia.nome}-${indiceDia}`}
            sx={{ mb: 5, breakInside: "avoid" }}
          >
            <Typography
              className="rotulo-dia"
              variant="h5"
              sx={{
                display: "inline-block",
                fontWeight: "bold",
                px: 2.5,
                py: 1,
                mb: 2,
                bgcolor: "text.primary",
                color: "background.default",
              }}
            >
              {dia.nome}
            </Typography>

            <TableContainer>
              <Table>
                <TableHead>
                  <TableRow>
                    <TableCell sx={cabecalhoSx}>Exercício / Máquina</TableCell>
                    <TableCell align="center" sx={cabecalhoSx}>
                      Séries
                    </TableCell>
                    <TableCell align="center" sx={cabecalhoSx}>
                      Repetições
                    </TableCell>
                    <TableCell align="center" sx={cabecalhoSx}>
                      Descanso
                    </TableCell>
                  </TableRow>
                </TableHead>

                <TableBody>
                  {(dia.exercicios || []).map((ex, indiceEx) => (
                    <TableRow key={ex.id ?? `${ex.nome}-${indiceEx}`}>
                      <TableCell>
                        <Typography sx={{ fontWeight: 700 }}>
                          {ex.nome}
                        </Typography>
                        {ex.grupo && (
                          <Typography variant="caption" color="text.secondary">
                            {ex.grupo} · {ex.tipo}
                          </Typography>
                        )}
                      </TableCell>

                      {CAMPOS.map((campo) => {
                        const chave = `${indiceDia}-${indiceEx}-${campo}`;
                        const { min, max } = LIMITES[campo];

                        return (
                          <TableCell
                            key={campo}
                            align="center"
                            sx={{
                              color:
                                campo === "descanso"
                                  ? "text.secondary"
                                  : "text.primary",
                            }}
                          >
                            {editando ? (
                              <TextField
                                size="small"
                                type={
                                  campo === "repeticoes" ? "text" : "number"
                                }
                                value={ex[campo]}
                                onChange={(e) =>
                                  alterarCampo(
                                    indiceDia,
                                    indiceEx,
                                    campo,
                                    e.target.value,
                                  )
                                }
                                error={Boolean(erros[chave])}
                                helperText={erros[chave] || ""}
                                sx={{ width: 120 }}
                                slotProps={{
                                  htmlInput: {
                                    style: { textAlign: "center" },
                                    ...(campo !== "repeticoes" && {
                                      min,
                                      max,
                                      step: 1,
                                    }),
                                  },
                                  input:
                                    campo === "descanso"
                                      ? {
                                          endAdornment: (
                                            <InputAdornment position="end">
                                              s
                                            </InputAdornment>
                                          ),
                                        }
                                      : undefined,
                                }}
                              />
                            ) : (
                              ex[campo]
                            )}
                          </TableCell>
                        );
                      })}
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </TableContainer>

            {(dia.tempoAerobico || plano.tempoAerobico) && (
              <Box
                className="info-faixa"
                sx={{
                  p: 2,
                  mt: 2,
                  borderRadius: 3,
                  bgcolor: "action.hover",
                }}
              >
                <Typography
                  variant="caption"
                  color="text.secondary"
                  sx={{ fontWeight: 700, textTransform: "uppercase" }}
                >
                  Orientação de Cardio / Aeróbico
                </Typography>
                <Typography variant="body1" sx={{ fontWeight: 500 }}>
                  {dia.tempoAerobico || plano.tempoAerobico}
                </Typography>
              </Box>
            )}
          </Box>
        ))}
      </Paper>
    </Box>
  );
}
