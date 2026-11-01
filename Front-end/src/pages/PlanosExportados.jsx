import {
  Box,
  Paper,
  Typography,
  Button,
  TextField,
  MenuItem,
  IconButton,
  Chip,
  InputAdornment,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  CircularProgress,
  Alert,
} from "@mui/material";
import { useState, useEffect } from "react";
import { useNavigate } from "react-router-dom";
import SearchIcon from "@mui/icons-material/Search";
import VisibilityOutlinedIcon from "@mui/icons-material/VisibilityOutlined";
import FileDownloadOutlinedIcon from "@mui/icons-material/FileDownloadOutlined";
import DeleteOutlinedIcon from "@mui/icons-material/DeleteOutlined";
import {
  buscarComCache,
  lerCache,
  salvarCache,
  temCache,
  removerCache,
  expirarCachePorPrefixo,
} from "../utils/cacheSessao";

const API_URL = "http://localhost:5011/treinos";

const obterUsuarioLogado = () => {
  try {
    const salvo = localStorage.getItem("usuarioLogado");
    return salvo ? JSON.parse(salvo) : null;
  } catch {
    return null;
  }
};

const formatarData = (iso) => new Date(iso).toLocaleDateString("pt-BR");

const corChip = { Validado: "success", Rascunho: "warning", Exportado: "info" };

function montarTexto(plano) {
  const linhas = [
    "DV Performance - Plano de Treino",
    plano.nomePlano ? `Protocolo: ${plano.nomePlano}` : null,
    `Aluno: ${plano.aluno}`,
    `Objetivo: ${plano.objetivo}`,
    `Frequência: ${plano.frequencia}x/semana`,
    plano.divisao ? `Divisão: ${plano.divisao}` : null,
    plano.tempoAerobico ? `Orientação Aeróbica: ${plano.tempoAerobico}` : null,
    `Data: ${formatarData(plano.data)}`,
    "",
  ].filter((l) => l !== null);

  (plano.dias || []).forEach((dia) => {
    linhas.push(dia.nome);
    (dia.exercicios || []).forEach((ex) => {
      const rotuloGrupo = ex.grupo ? ` [${ex.grupo}]` : "";
      linhas.push(
        `  - ${ex.nome}${rotuloGrupo}: ${ex.series}x${ex.repeticoes} (descanso ${ex.descanso})`,
      );
    });
    if (dia.tempoAerobico) {
      linhas.push(`  * Cardio do dia: ${dia.tempoAerobico}`);
    }
    linhas.push("");
  });

  return linhas.join("\n");
}

const obterChavePlanos = () => {
  const usuarioLogado = obterUsuarioLogado();
  return `planos:${usuarioLogado?.id || "geral"}`;
};

export default function PlanosExportados({ glassStyle }) {
  const navigate = useNavigate();
  const [planos, setPlanos] = useState(
    () => lerCache(obterChavePlanos()) || [],
  );
  const [carregando, setCarregando] = useState(
    () => !temCache(obterChavePlanos()),
  );
  const [erro, setErro] = useState("");
  const [busca, setBusca] = useState("");
  const [filtroStatus, setFiltroStatus] = useState("Todos");
  const [planoParaExcluir, setPlanoParaExcluir] = useState(null);
  const [excluindo, setExcluindo] = useState(false);

  useEffect(() => {
    const carregar = async () => {
      setErro("");
      const usuarioLogado = obterUsuarioLogado();
      const chave = `planos:${usuarioLogado?.id || "geral"}`;
      const query = usuarioLogado?.id ? `?usuarioId=${usuarioLogado.id}` : "";

      if (!temCache(chave)) {
        setCarregando(true);
      }

      try {
        const dados = await buscarComCache(chave, async () => {
          const response = await fetch(`${API_URL}${query}`);
          if (!response.ok) {
            throw new Error("Não foi possível carregar os planos de treino.");
          }
          const lista = await response.json();
          if (Array.isArray(lista)) {
            lista.forEach((p) => {
              if (p?.id != null) salvarCache(`planoDetalhe:${p.id}`, p);
            });
          }
          return lista;
        });
        setPlanos(dados);
      } catch (err) {
        console.error("Erro ao carregar planos:", err);
        if (!temCache(chave)) {
          setErro(
            err.message || "Não foi possível carregar os planos do servidor.",
          );
        }
      } finally {
        setCarregando(false);
      }
    };

    carregar();
  }, []);

  const planosFiltrados = planos.filter((p) => {
    const bateNome = p.aluno.toLowerCase().includes(busca.trim().toLowerCase());
    const bateStatus = filtroStatus === "Todos" || p.status === filtroStatus;
    return bateNome && bateStatus;
  });

  const baixarTxt = async (plano) => {
    const blob = new Blob([montarTexto(plano)], {
      type: "text/plain;charset=utf-8",
    });
    const url = URL.createObjectURL(blob);
    const link = document.createElement("a");
    link.href = url;
    link.download = `plano-${plano.aluno.toLowerCase().replace(/\s+/g, "-")}.txt`;
    link.click();
    URL.revokeObjectURL(url);

    try {
      const response = await fetch(`${API_URL}/${plano.id}/status`, {
        method: "PATCH",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ status: "Exportado" }),
      });

      if (response.ok) {
        const chave = obterChavePlanos();
        setPlanos((lista) => {
          const novaLista = lista.map((p) =>
            p.id === plano.id ? { ...p, status: "Exportado" } : p,
          );
          salvarCache(chave, novaLista);
          return novaLista;
        });
        const detalheAtual = lerCache(`planoDetalhe:${plano.id}`);
        if (detalheAtual) {
          salvarCache(`planoDetalhe:${plano.id}`, {
            ...detalheAtual,
            status: "Exportado",
          });
        }
      }
    } catch (err) {
      console.error("Erro ao atualizar status do plano:", err);
    }
  };

  const confirmarExclusao = async () => {
    if (!planoParaExcluir) return;
    setExcluindo(true);
    setErro("");

    try {
      const response = await fetch(`${API_URL}/${planoParaExcluir.id}`, {
        method: "DELETE",
      });

      if (!response.ok) {
        const dados = await response.json().catch(() => null);
        throw new Error(dados?.mensagem || "Não foi possível excluir o plano.");
      }

      const chave = obterChavePlanos();
      setPlanos((lista) => {
        const novaLista = lista.filter((p) => p.id !== planoParaExcluir.id);
        salvarCache(chave, novaLista);
        return novaLista;
      });
      removerCache(`planoDetalhe:${planoParaExcluir.id}`);
      expirarCachePorPrefixo("dashboard:");
      setPlanoParaExcluir(null);
    } catch (err) {
      console.error("Erro ao excluir plano:", err);
      setErro(err.message || "Não foi possível excluir o plano.");
      setPlanoParaExcluir(null);
    } finally {
      setExcluindo(false);
    }
  };

  const cabecalhoSx = { fontWeight: 600, color: "text.primary" };

  return (
    <Box>
      <Box
        sx={{
          display: "flex",
          justifyContent: "space-between",
          alignItems: "center",
          mb: 3,
        }}
      >
        <Typography variant="h4" sx={{ fontWeight: "bold" }}>
          Planos de Treino
        </Typography>

        <Button
          variant="contained"
          size="large"
          onClick={() => navigate("/treinos")}
        >
          Novo Treino
        </Button>
      </Box>

      {erro && (
        <Alert severity="error" sx={{ mb: 3 }}>
          {erro}
        </Alert>
      )}

      <Box sx={{ display: "flex", gap: 2, mb: 3, flexWrap: "wrap" }}>
        <TextField
          placeholder="Pesquisar por nome do aluno"
          value={busca}
          onChange={(e) => setBusca(e.target.value)}
          sx={{ flex: "1 1 300px" }}
          slotProps={{
            input: {
              startAdornment: (
                <InputAdornment position="start">
                  <SearchIcon />
                </InputAdornment>
              ),
            },
          }}
        />

        <TextField
          select
          value={filtroStatus}
          onChange={(e) => setFiltroStatus(e.target.value)}
          sx={{ minWidth: 160 }}
        >
          <MenuItem value="Todos">Todos</MenuItem>
          <MenuItem value="Rascunho">Rascunho</MenuItem>
          <MenuItem value="Validado">Validado</MenuItem>
          <MenuItem value="Exportado">Exportado</MenuItem>
        </TextField>
      </Box>

      <Paper
        elevation={0}
        sx={{ ...glassStyle, borderRadius: 4, overflow: "hidden" }}
      >
        <TableContainer>
          <Table>
            <TableHead>
              <TableRow>
                <TableCell sx={cabecalhoSx}>Data</TableCell>
                <TableCell sx={cabecalhoSx}>Aluno</TableCell>
                <TableCell sx={cabecalhoSx}>Objetivo</TableCell>
                <TableCell sx={cabecalhoSx}>Frequência</TableCell>
                <TableCell sx={cabecalhoSx}>Status</TableCell>
                <TableCell align="right" sx={cabecalhoSx}>
                  Ações
                </TableCell>
              </TableRow>
            </TableHead>

            <TableBody>
              {carregando && (
                <TableRow>
                  <TableCell colSpan={6} align="center" sx={{ py: 5 }}>
                    <CircularProgress size={32} />
                  </TableCell>
                </TableRow>
              )}

              {!carregando && planosFiltrados.length === 0 && (
                <TableRow>
                  <TableCell
                    colSpan={6}
                    align="center"
                    sx={{ py: 4, color: "text.secondary" }}
                  >
                    Nenhum plano encontrado.
                  </TableCell>
                </TableRow>
              )}

              {!carregando &&
                planosFiltrados.map((p) => (
                  <TableRow key={p.id} hover>
                    <TableCell sx={{ color: "text.secondary" }}>
                      {formatarData(p.data)}
                    </TableCell>
                    <TableCell>
                      <Typography sx={{ fontWeight: 600 }}>
                        {p.aluno}
                      </Typography>
                      {p.divisao && (
                        <Typography variant="caption" color="text.secondary">
                          {p.divisao}
                        </Typography>
                      )}
                    </TableCell>
                    <TableCell sx={{ color: "text.secondary" }}>
                      {p.objetivo}
                    </TableCell>
                    <TableCell sx={{ color: "text.secondary" }}>
                      {p.frequencia}x/semana
                    </TableCell>
                    <TableCell>
                      <Chip
                        label={p.status}
                        size="small"
                        color={corChip[p.status] || "default"}
                        variant="outlined"
                      />
                    </TableCell>
                    <TableCell align="right">
                      <IconButton
                        size="small"
                        aria-label="Visualizar plano"
                        onClick={() => navigate(`/planos/${p.id}`)}
                        sx={{ mr: 1, bgcolor: "action.hover" }}
                      >
                        <VisibilityOutlinedIcon fontSize="small" />
                      </IconButton>
                      <IconButton
                        size="small"
                        aria-label="Baixar plano"
                        onClick={() => baixarTxt(p)}
                        sx={{ mr: 1, bgcolor: "action.hover" }}
                      >
                        <FileDownloadOutlinedIcon fontSize="small" />
                      </IconButton>
                      <IconButton
                        size="small"
                        aria-label="Excluir plano"
                        onClick={() => setPlanoParaExcluir(p)}
                        sx={{ bgcolor: "action.hover" }}
                      >
                        <DeleteOutlinedIcon fontSize="small" />
                      </IconButton>
                    </TableCell>
                  </TableRow>
                ))}
            </TableBody>
          </Table>
        </TableContainer>
      </Paper>

      <Dialog
        open={Boolean(planoParaExcluir)}
        onClose={() => !excluindo && setPlanoParaExcluir(null)}
        maxWidth="xs"
        fullWidth
        PaperProps={{ sx: { borderRadius: 4, p: 1 } }}
        slotProps={{ paper: { sx: { borderRadius: 4, p: 1 } } }}
      >
        <DialogTitle sx={{ fontWeight: "bold", fontSize: "1.35rem" }}>
          Confirmar exclusão
        </DialogTitle>

        <DialogContent>
          <Typography color="text.secondary">
            Tem certeza de que deseja excluir este plano de treino? Esta ação é
            permanente e não poderá ser desfeita.
          </Typography>
        </DialogContent>

        <DialogActions sx={{ px: 3, pb: 2, pt: 1, gap: 1 }}>
          <Button
            color="inherit"
            disabled={excluindo}
            onClick={() => setPlanoParaExcluir(null)}
          >
            Cancelar
          </Button>
          <Button
            variant="contained"
            color="error"
            disabled={excluindo}
            onClick={confirmarExclusao}
          >
            Excluir plano
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
}
