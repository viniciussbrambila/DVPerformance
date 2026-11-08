import {
  Box,
  Paper,
  Typography,
  Button,
  ButtonBase,
  IconButton,
  Divider,
  TextField,
  Alert,
  InputAdornment,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  CircularProgress,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
} from "@mui/material";
import { useState, useEffect } from "react";
import { useNavigate, useParams } from "react-router-dom";
import ArrowBackIcon from "@mui/icons-material/ArrowBack";
import CheckCircleOutlinedIcon from "@mui/icons-material/CheckCircleOutlined";
import EditOutlinedIcon from "@mui/icons-material/EditOutlined";
import PrintOutlinedIcon from "@mui/icons-material/PrintOutlined";
import CloseIcon from "@mui/icons-material/Close";
import SaveOutlinedIcon from "@mui/icons-material/SaveOutlined";
import DeleteOutlinedIcon from "@mui/icons-material/DeleteOutlined";
import SwapHorizOutlinedIcon from "@mui/icons-material/SwapHorizOutlined";
import FitnessCenterIcon from "@mui/icons-material/FitnessCenter";
import DirectionsRunIcon from "@mui/icons-material/DirectionsRun";
import KeyboardArrowUpIcon from "@mui/icons-material/KeyboardArrowUp";
import KeyboardArrowDownIcon from "@mui/icons-material/KeyboardArrowDown";
import RestoreOutlinedIcon from "@mui/icons-material/RestoreOutlined";
import ImpressaoRelatorio from "../components/ImpressaoRelatorio";
import { exportarPdfRelatorio } from "../utils/relatorios";
import Exercicios from "./Exercicios";
import Maquinas from "./Maquinas";
import {
  prepararEdicao,
  formatarTempoAerobico,
  alterarAerobicoDoPlano,
  adicionarItemAoPlano,
  removerItemDoPlano,
  moverItemNoPlano,
  restaurarOrdemDoPlano,
  substituirItemDoPlano,
  montarItensEdicao,
} from "../utils/edicaoTreino";
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

const CATALOGOS = [
  {
    tipo: "maquina",
    titulo: "Máquinas",
    descricao: "Abrir catálogo de máquinas",
    Icone: FitnessCenterIcon,
  },
  {
    tipo: "exercicio",
    titulo: "Exercícios",
    descricao: "Abrir biblioteca de exercícios",
    Icone: DirectionsRunIcon,
  },
];

const LIMITES = {
  series: { min: 1, max: 10 },
  repeticoes: { min: 6, max: 30 },
  descanso: { min: 30, max: 300 },
  aerobico: { min: 10, max: 100 },
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
  const [avisoValidacaoAberto, setAvisoValidacaoAberto] = useState(false);

  const [editando, setEditando] = useState(false);
  const [diasEdicao, setDiasEdicao] = useState([]);
  const [selecaoItem, setSelecaoItem] = useState(null);
  const [substitutoSelecionado, setSubstitutoSelecionado] = useState(null);
  const [itemParaExcluir, setItemParaExcluir] = useState(null);
  const [edicaoAerobico, setEdicaoAerobico] = useState(null);
  const [diaParaRemoverAerobico, setDiaParaRemoverAerobico] = useState(null);

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
  const dicas = Array.isArray(plano.dicas)
    ? plano.dicas.slice(0, 2)
    : plano.dica
      ? [plano.dica]
      : [];
  const minutosAerobico = Number(edicaoAerobico?.minutos);
  const tempoAerobicoValido = Boolean(
    edicaoAerobico &&
    /^\d+$/.test(edicaoAerobico.minutos) &&
    Number.isInteger(minutosAerobico) &&
    minutosAerobico >= LIMITES.aerobico.min &&
    minutosAerobico <= LIMITES.aerobico.max,
  );

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
  const planoVazio =
    editando && diasEdicao.every((dia) => dia.exercicios.length === 0);
  const temErros = Object.keys(erros).length > 0 || planoVazio;

  const iniciarEdicao = () => {
    setErroApi("");
    setDiasEdicao(prepararEdicao(plano.dias || []));
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

  const abrirEdicaoAerobico = (indiceDia) => {
    if (!editando) iniciarEdicao();
    const minutos = dias[indiceDia].tempoAerobico || 0;
    setEdicaoAerobico({
      indiceDia,
      minutos: minutos > 0 ? String(minutos) : "20",
      adicionando: minutos === 0,
    });
  };

  const confirmarAerobico = (event) => {
    event.preventDefault();
    if (!tempoAerobicoValido || salvando) return;
    setDiasEdicao((lista) =>
      alterarAerobicoDoPlano(lista, edicaoAerobico.indiceDia, minutosAerobico),
    );
    setEdicaoAerobico(null);
  };

  const confirmarRemocaoAerobico = () => {
    if (diaParaRemoverAerobico === null) return;
    setDiasEdicao((lista) =>
      alterarAerobicoDoPlano(lista, diaParaRemoverAerobico, 0),
    );
    setDiaParaRemoverAerobico(null);
  };

  const fecharSelecao = () => {
    setSelecaoItem(null);
    setSubstitutoSelecionado(null);
  };

  const abrirAdicao = (indiceDia) => {
    if (!editando) iniciarEdicao();
    setSelecaoItem({ indiceDia, itemAtual: null, tipoCatalogo: null });
  };

  const selecionarItem = (selecionado) => {
    if (selecaoItem.itemAtual) {
      setSubstitutoSelecionado(selecionado);
      return;
    }

    setDiasEdicao((lista) =>
      adicionarItemAoPlano(
        lista,
        selecaoItem.indiceDia,
        selecionado,
        selecaoItem.tipoCatalogo,
      ),
    );
    fecharSelecao();
  };

  const confirmarSubstituicao = () => {
    if (!selecaoItem?.itemAtual || !substitutoSelecionado) return;
    setDiasEdicao((lista) =>
      substituirItemDoPlano(
        lista,
        selecaoItem.itemAtual.id,
        substitutoSelecionado,
        selecaoItem.tipoCatalogo,
      ),
    );
    fecharSelecao();
  };

  const confirmarExclusao = () => {
    if (!itemParaExcluir) return;
    setDiasEdicao((lista) => removerItemDoPlano(lista, itemParaExcluir.id));
    setItemParaExcluir(null);
  };

  const salvarEdicao = async () => {
    if (temErros || salvando) return;
    setSalvando(true);
    setErroApi("");

    try {
      const response = await fetch(API_URL + "/" + plano.id + "/itens", {
        method: "PUT",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ itens: montarItensEdicao(diasEdicao) }),
      });

      const dados = await response.json().catch(() => null);
      if (!response.ok) {
        throw new Error(
          dados?.mensagem || "Não foi possível salvar as alterações do plano.",
        );
      }

      setPlano(dados);
      salvarCache("planoDetalhe:" + plano.id, dados);
      atualizarItemEmListasCache("planos:", plano.id, dados);
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
    if (plano.status !== "Validado" && plano.status !== "Exportado") {
      setAvisoValidacaoAberto(true);
      return;
    }

    const ok = await atualizarStatusNoBanco("Exportado");
    if (!ok) return;

    setTimeout(() => {
      exportarPdfRelatorio(
        `plano-${plano.aluno.toLowerCase().replace(/\s+/g, "-")}`,
      );
    }, 300);
  };

  const infos = [
    ["Aluno", plano.aluno],
    ["Peso", formatarMedida(plano.peso, "kg")],
    ["Altura", formatarMedida(plano.altura, "m")],
    ["Objetivo", plano.objetivo],
    ["Frequência", `${plano.frequencia} dias/semana`],
    ["Data", formatarData(plano.data)],
  ];

  return (
    <Box className="plano-impressao">
      <ImpressaoRelatorio margemSuperior="12mm" />

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
                  fecharSelecao();
                  setItemParaExcluir(null);
                  setEdicaoAerobico(null);
                  setDiaParaRemoverAerobico(null);
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

      {planoVazio && (
        <Alert severity="warning" className="no-print" sx={{ mb: 3 }}>
          O plano precisa manter pelo menos um exercício ou máquina.
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
            padding: "0 !important",
            borderRadius: 0,
            boxDecorationBreak: "clone",
            WebkitBoxDecorationBreak: "clone",
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
            <Box
              sx={{
                display: "flex",
                alignItems: "center",
                justifyContent: "space-between",
                flexWrap: "wrap",
                gap: 2,
                mb: 2,
              }}
            >
              <Typography
                className="rotulo-dia"
                variant="h5"
                sx={{
                  display: "inline-block",
                  fontWeight: "bold",
                  px: 2.5,
                  py: 1,
                  bgcolor: "text.primary",
                  color: "background.default",
                }}
              >
                {dia.nome}
              </Typography>
              {editando && (
                <Button
                  className="no-print"
                  size="small"
                  color="inherit"
                  startIcon={<RestoreOutlinedIcon />}
                  disabled={
                    salvando ||
                    restaurarOrdemDoPlano(diasEdicao, indiceDia, plano.dias || []) ===
                      diasEdicao
                  }
                  onClick={() =>
                    setDiasEdicao((lista) =>
                      restaurarOrdemDoPlano(lista, indiceDia, plano.dias || []),
                    )
                  }
                  sx={{ bgcolor: "action.hover" }}
                >
                  Restaurar ordem original
                </Button>
              )}
            </Box>

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
                    {editando && (
                      <TableCell
                        className="no-print"
                        align="center"
                        sx={cabecalhoSx}
                      >
                        Ações
                      </TableCell>
                    )}
                  </TableRow>
                </TableHead>

                <TableBody>
                  {editando && dia.exercicios.length === 0 && (
                    <TableRow>
                      <TableCell colSpan={5} align="center" sx={{ py: 3 }}>
                        Nenhum item neste dia. Ao salvar, ele será removido do
                        plano.
                      </TableCell>
                    </TableRow>
                  )}
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
                                disabled={salvando}
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
                      {editando && (
                        <TableCell className="no-print" align="center">
                          <Box
                            sx={{
                              display: "flex",
                              gap: 1,
                              justifyContent: "center",
                              alignItems: "center",
                              flexWrap: "wrap",
                            }}
                          >
                            <Box sx={{ display: "flex" }}>
                              <IconButton
                                size="small"
                                color="inherit"
                                title="Mover para cima"
                                aria-label={`Mover ${ex.nome} para cima em ${dia.nome}`}
                                disabled={salvando || indiceEx === 0}
                                onClick={() =>
                                  setDiasEdicao((lista) =>
                                    moverItemNoPlano(lista, indiceDia, indiceEx, -1),
                                  )
                                }
                                sx={{ bgcolor: "action.hover" }}
                              >
                                <KeyboardArrowUpIcon />
                              </IconButton>
                              <IconButton
                                size="small"
                                color="inherit"
                                title="Mover para baixo"
                                aria-label={`Mover ${ex.nome} para baixo em ${dia.nome}`}
                                disabled={
                                  salvando || indiceEx === dia.exercicios.length - 1
                                }
                                onClick={() =>
                                  setDiasEdicao((lista) =>
                                    moverItemNoPlano(lista, indiceDia, indiceEx, 1),
                                  )
                                }
                                sx={{ bgcolor: "action.hover" }}
                              >
                                <KeyboardArrowDownIcon />
                              </IconButton>
                            </Box>
                            <Button
                              size="small"
                              color="inherit"
                              startIcon={<SwapHorizOutlinedIcon />}
                              disabled={salvando}
                              onClick={() =>
                                setSelecaoItem({
                                  indiceDia,
                                  itemAtual: ex,
                                  tipoCatalogo: null,
                                })
                              }
                              sx={{ bgcolor: "action.hover" }}
                            >
                              Substituir
                            </Button>
                            <Button
                              size="small"
                              color="error"
                              startIcon={<DeleteOutlinedIcon />}
                              disabled={salvando}
                              onClick={() => setItemParaExcluir(ex)}
                              sx={{ bgcolor: "action.hover" }}
                            >
                              Excluir
                            </Button>
                          </Box>
                        </TableCell>
                      )}
                    </TableRow>
                  ))}
                  <TableRow className="no-print">
                    <TableCell
                      colSpan={editando ? 5 : 4}
                      align="left"
                      sx={{ py: 3, px: 0 }}
                    >
                      <Button
                        variant="contained"
                        color="primary"
                        startIcon={<FitnessCenterIcon />}
                        disabled={salvando}
                        onClick={() => abrirAdicao(indiceDia)}
                        aria-label={`Adicionar exercício ou máquina em ${dia.nome}`}
                        sx={{ whiteSpace: "nowrap" }}
                      >
                        Adicionar
                      </Button>
                    </TableCell>
                  </TableRow>
                </TableBody>
              </Table>
            </TableContainer>

            {dia.tempoAerobico > 0 ? (
              <Box
                className="info-faixa"
                sx={{
                  display: "flex",
                  alignItems: "center",
                  justifyContent: "space-between",
                  flexWrap: "wrap",
                  gap: 2,
                  p: 2,
                  mt: 2,
                  borderRadius: 3,
                  bgcolor: "action.hover",
                }}
              >
                <Box>
                  <Typography
                    variant="caption"
                    color="text.secondary"
                    sx={{ fontWeight: 700, textTransform: "uppercase" }}
                  >
                    Aeróbico
                  </Typography>
                  <Typography variant="body1" sx={{ fontWeight: 500 }}>
                    {formatarTempoAerobico(dia.tempoAerobico)}
                  </Typography>
                </Box>
                {editando && (
                  <Box className="no-print" sx={{ display: "flex", gap: 1 }}>
                    <Button
                      color="inherit"
                      startIcon={<EditOutlinedIcon />}
                      disabled={salvando}
                      onClick={() => abrirEdicaoAerobico(indiceDia)}
                      aria-label={`Editar aeróbico em ${dia.nome}`}
                    >
                      Editar
                    </Button>
                    <Button
                      color="error"
                      startIcon={<DeleteOutlinedIcon />}
                      disabled={salvando}
                      onClick={() => setDiaParaRemoverAerobico(indiceDia)}
                      aria-label={`Remover aeróbico em ${dia.nome}`}
                    >
                      Remover
                    </Button>
                  </Box>
                )}
              </Box>
            ) : (
              <Box className="no-print" sx={{ mt: 2 }}>
                <Button
                  variant="contained"
                  color="primary"
                  startIcon={<DirectionsRunIcon />}
                  disabled={salvando}
                  onClick={() => abrirEdicaoAerobico(indiceDia)}
                  aria-label={`Adicionar aeróbico em ${dia.nome}`}
                >
                  Aeróbico+
                </Button>
              </Box>
            )}
          </Box>
        ))}

        {dicas.length > 0 && (
          <Box sx={{ mt: 4, breakInside: "avoid" }}>
            <Divider sx={{ mb: 3 }} />
            <Typography variant="h6" sx={{ fontWeight: 700, mb: 2 }}>
              Dicas para {plano.objetivo}
            </Typography>
            {dicas.map((dica, indice) => (
              <Typography key={indice} sx={{ mb: indice < dicas.length - 1 ? 1.5 : 0 }}>
                {indice + 1}. {dica}
              </Typography>
            ))}
          </Box>
        )}
      </Paper>

      <Dialog
        open={Boolean(selecaoItem && !selecaoItem.tipoCatalogo)}
        onClose={fecharSelecao}
        maxWidth="sm"
        fullWidth
        aria-labelledby="escolher-catalogo-titulo"
        slotProps={{ paper: { sx: { borderRadius: 4, p: 1 } } }}
      >
        <DialogTitle
          id="escolher-catalogo-titulo"
          sx={{ fontWeight: "bold", fontSize: "1.35rem", textAlign: "center" }}
        >
          {selecaoItem?.itemAtual
            ? "Como deseja substituir este item?"
            : "O que deseja adicionar ao plano?"}
        </DialogTitle>
        <DialogContent>
          <Box
            sx={{
              display: "grid",
              gridTemplateColumns: { xs: "1fr", sm: "repeat(2, 1fr)" },
              gap: 2,
              mt: 1,
            }}
          >
            {CATALOGOS.map(({ tipo, titulo, descricao, Icone }) => (
              <ButtonBase
                key={tipo}
                onClick={() =>
                  setSelecaoItem((atual) => ({ ...atual, tipoCatalogo: tipo }))
                }
                sx={{
                  display: "flex",
                  flexDirection: "column",
                  gap: 1,
                  p: 3,
                  border: "1px solid",
                  borderColor: "divider",
                  borderRadius: 3,
                  color: "text.primary",
                  bgcolor: "action.hover",
                  "&:hover": { borderColor: "primary.main" },
                  "&.Mui-focusVisible": {
                    outline: "2px solid",
                    outlineColor: "primary.main",
                    outlineOffset: 2,
                  },
                }}
              >
                <Icone sx={{ fontSize: 32, color: "primary.main" }} />
                <Typography variant="h6" sx={{ fontWeight: 700 }}>
                  {titulo}
                </Typography>
                <Typography variant="body2" color="text.secondary">
                  {descricao}
                </Typography>
              </ButtonBase>
            ))}
          </Box>
        </DialogContent>
        <DialogActions sx={{ px: 3, pb: 2, pt: 1, gap: 1 }}>
          <Button color="inherit" onClick={fecharSelecao}>
            Cancelar
          </Button>
        </DialogActions>
      </Dialog>

      <Dialog
        fullScreen
        open={Boolean(selecaoItem?.tipoCatalogo)}
        onClose={fecharSelecao}
        aria-label="Selecionar exercício ou máquina para o plano de treino"
      >
        <Box sx={{ p: { xs: 2, md: 4 } }}>
          {selecaoItem?.tipoCatalogo === "maquina" ? (
            <Maquinas
              glassStyle={glassStyle}
              onSelecionar={selecionarItem}
              onVoltar={fecharSelecao}
              itemAtualId={selecaoItem.itemAtual?.maquinaId}
              diaTreino={diasEdicao[selecaoItem.indiceDia]}
              modoSelecao={selecaoItem.itemAtual ? "substituir" : "adicionar"}
            />
          ) : selecaoItem?.tipoCatalogo === "exercicio" ? (
            <Exercicios
              glassStyle={glassStyle}
              onSelecionar={selecionarItem}
              onVoltar={fecharSelecao}
              itemAtualId={selecaoItem.itemAtual?.exercicioId}
              diaTreino={diasEdicao[selecaoItem.indiceDia]}
              modoSelecao={selecaoItem.itemAtual ? "substituir" : "adicionar"}
            />
          ) : null}
        </Box>

        <Dialog
          open={Boolean(substitutoSelecionado)}
          onClose={() => setSubstitutoSelecionado(null)}
          maxWidth="xs"
          fullWidth
          aria-labelledby="substituir-item-titulo"
          aria-describedby="substituir-item-descricao"
          slotProps={{ paper: { sx: { borderRadius: 4, p: 1 } } }}
        >
          <DialogTitle
            id="substituir-item-titulo"
            sx={{ fontWeight: "bold", fontSize: "1.35rem" }}
          >
            Confirmar substituição
          </DialogTitle>
          <DialogContent>
            <Typography id="substituir-item-descricao" color="text.secondary">
              Confirmar a substituição{" "}
              {selecaoItem?.itemAtual?.maquinaId != null
                ? "da máquina"
                : "do exercício"}{" "}
              <Box
                component="span"
                sx={{ fontWeight: 700, color: "text.primary" }}
              >
                {selecaoItem?.itemAtual?.nome}
              </Box>{" "}
              {selecaoItem?.tipoCatalogo === "maquina"
                ? "pela máquina"
                : "pelo exercício"}{" "}
              <Box
                component="span"
                sx={{ fontWeight: 700, color: "text.primary" }}
              >
                {substitutoSelecionado?.nome}
              </Box>
              ?
            </Typography>
          </DialogContent>
          <DialogActions sx={{ px: 3, pb: 2, pt: 1, gap: 1 }}>
            <Button
              color="inherit"
              onClick={() => setSubstitutoSelecionado(null)}
            >
              Cancelar
            </Button>
            <Button
              variant="contained"
              startIcon={<CheckCircleOutlinedIcon />}
              onClick={confirmarSubstituicao}
            >
              Confirmar
            </Button>
          </DialogActions>
        </Dialog>
      </Dialog>

      <Dialog
        open={Boolean(itemParaExcluir)}
        onClose={() => setItemParaExcluir(null)}
        maxWidth="xs"
        fullWidth
        aria-labelledby="excluir-item-titulo"
        aria-describedby="excluir-item-descricao"
        slotProps={{ paper: { sx: { borderRadius: 4, p: 1 } } }}
      >
        <DialogTitle
          id="excluir-item-titulo"
          sx={{ fontWeight: "bold", fontSize: "1.35rem" }}
        >
          Confirmar exclusão
        </DialogTitle>
        <DialogContent>
          <Typography id="excluir-item-descricao" color="text.secondary">
            Tem certeza de que deseja excluir{" "}
            {itemParaExcluir?.maquinaId != null
              ? "esta máquina"
              : "este exercício"}{" "}
            <Box
              component="span"
              sx={{ fontWeight: 700, color: "text.primary" }}
            >
              {itemParaExcluir?.nome}
            </Box>{" "}
            do plano?
          </Typography>
        </DialogContent>
        <DialogActions sx={{ px: 3, pb: 2, pt: 1, gap: 1 }}>
          <Button color="inherit" onClick={() => setItemParaExcluir(null)}>
            Cancelar
          </Button>
          <Button
            variant="contained"
            color="error"
            startIcon={<DeleteOutlinedIcon />}
            onClick={confirmarExclusao}
          >
            Excluir
          </Button>
        </DialogActions>
      </Dialog>

      <Dialog
        open={Boolean(edicaoAerobico)}
        onClose={() => setEdicaoAerobico(null)}
        maxWidth="xs"
        fullWidth
        aria-labelledby="editar-aerobico-titulo"
        slotProps={{ paper: { sx: { borderRadius: 4, p: 1 } } }}
      >
        <Box component="form" onSubmit={confirmarAerobico}>
          <DialogTitle
            id="editar-aerobico-titulo"
            sx={{ fontWeight: "bold", fontSize: "1.35rem" }}
          >
            {edicaoAerobico?.adicionando
              ? "Adicionar aeróbico"
              : "Editar aeróbico"}
          </DialogTitle>
          <DialogContent sx={{ pt: 1 }}>
            <TextField
              autoFocus
              fullWidth
              type="number"
              label="Tempo do aeróbico"
              value={edicaoAerobico?.minutos ?? ""}
              onChange={(event) =>
                setEdicaoAerobico((atual) => ({
                  ...atual,
                  minutos: event.target.value,
                }))
              }
              error={Boolean(edicaoAerobico && !tempoAerobicoValido)}
              helperText={
                edicaoAerobico && !tempoAerobicoValido
                  ? `Informe minutos inteiros entre ${LIMITES.aerobico.min} e ${LIMITES.aerobico.max}.`
                  : `Tempo permitido: ${LIMITES.aerobico.min} a ${LIMITES.aerobico.max} minutos.`
              }
              sx={{ mt: 1 }}
              slotProps={{
                htmlInput: {
                  min: LIMITES.aerobico.min,
                  max: LIMITES.aerobico.max,
                  step: 1,
                },
                input: {
                  endAdornment: (
                    <InputAdornment position="end">min</InputAdornment>
                  ),
                },
              }}
            />
            {tempoAerobicoValido && (
              <Typography color="text.secondary" sx={{ mt: 2 }}>
                {formatarTempoAerobico(minutosAerobico)}
              </Typography>
            )}
          </DialogContent>
          <DialogActions sx={{ px: 3, pb: 2, pt: 1, gap: 1 }}>
            <Button color="inherit" onClick={() => setEdicaoAerobico(null)}>
              Cancelar
            </Button>
            <Button
              type="submit"
              variant="contained"
              startIcon={<CheckCircleOutlinedIcon />}
              disabled={!tempoAerobicoValido || salvando}
            >
              Confirmar
            </Button>
          </DialogActions>
        </Box>
      </Dialog>

      <Dialog
        open={diaParaRemoverAerobico !== null}
        onClose={() => setDiaParaRemoverAerobico(null)}
        maxWidth="xs"
        fullWidth
        aria-labelledby="remover-aerobico-titulo"
        aria-describedby="remover-aerobico-descricao"
        slotProps={{ paper: { sx: { borderRadius: 4, p: 1 } } }}
      >
        <DialogTitle
          id="remover-aerobico-titulo"
          sx={{ fontWeight: "bold", fontSize: "1.35rem" }}
        >
          Remover aeróbico
        </DialogTitle>
        <DialogContent>
          <Typography id="remover-aerobico-descricao" color="text.secondary">
            Tem certeza de que deseja remover o aeróbico de{" "}
            {dias[diaParaRemoverAerobico]?.nome}?
          </Typography>
        </DialogContent>
        <DialogActions sx={{ px: 3, pb: 2, pt: 1, gap: 1 }}>
          <Button
            color="inherit"
            onClick={() => setDiaParaRemoverAerobico(null)}
          >
            Cancelar
          </Button>
          <Button
            variant="contained"
            color="error"
            startIcon={<DeleteOutlinedIcon />}
            onClick={confirmarRemocaoAerobico}
          >
            Remover
          </Button>
        </DialogActions>
      </Dialog>

      <Dialog
        open={avisoValidacaoAberto}
        onClose={() => setAvisoValidacaoAberto(false)}
        maxWidth="xs"
        fullWidth
        aria-labelledby="validar-plano-titulo"
        aria-describedby="validar-plano-descricao"
        slotProps={{ paper: { sx: { borderRadius: 4, p: 1 } } }}
      >
        <DialogTitle
          id="validar-plano-titulo"
          sx={{ fontWeight: "bold", fontSize: "1.35rem" }}
        >
          Validação necessária
        </DialogTitle>
        <DialogContent>
          <Typography id="validar-plano-descricao" color="text.secondary">
            Você precisa validar esse plano antes de exportá-lo!
          </Typography>
        </DialogContent>
        <DialogActions sx={{ px: 3, pb: 2, pt: 1, gap: 1 }}>
          <Button
            color="inherit"
            onClick={() => setAvisoValidacaoAberto(false)}
          >
            Voltar
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
}
