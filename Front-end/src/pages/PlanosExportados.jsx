import {
  Box,
  Paper,
  Typography,
  Button,
  TextField,
  MenuItem,
  Chip,
  InputAdornment,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  CircularProgress,
  Alert,
} from "@mui/material";
import { useState, useEffect } from "react";
import { useNavigate } from "react-router-dom";
import AddIcon from "@mui/icons-material/Add";
import SearchIcon from "@mui/icons-material/Search";
import PrintOutlinedIcon from "@mui/icons-material/PrintOutlined";
import ImpressaoRelatorio from "../components/ImpressaoRelatorio";
import DesktopDateRangePicker from "../components/DesktopDateRangePicker";
import { DialogoConfirmacao } from "../components/DialogoConfirmacao";
import {
  exportarPdfRelatorio,
  filtrarPlanosTreino,
} from "../utils/relatorios";
import VisibilityOutlinedIcon from "@mui/icons-material/VisibilityOutlined";
import DeleteOutlinedIcon from "@mui/icons-material/DeleteOutlined";
import {
  buscarComCache,
  lerCache,
  salvarCache,
  temCache,
  removerCache,
  expirarCachePorPrefixo,
  removerCachePorPrefixo,
} from "../utils/cacheSessao";
import {
  API_BASE_URL,
  fetchAutenticado,
  lerUsuarioSessao,
} from "../services/api";

const API_URL = `${API_BASE_URL}/treinos`;

const formatarData = (iso) => new Date(iso).toLocaleDateString("pt-BR");

const corChip = { Validado: "success", Rascunho: "warning", Exportado: "info" };

const obterChavePlanos = () => {
  const usuarioLogado = lerUsuarioSessao();
  return `planos:${usuarioLogado?.id || "geral"}`;
};

export default function PlanosExportados() {
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
  const [intervaloData, setIntervaloData] = useState([null, null]);
  const [ordem, setOrdem] = useState("recentes");
  const [planoParaExcluir, setPlanoParaExcluir] = useState(null);
  const [excluindo, setExcluindo] = useState(false);

  useEffect(() => {
    let ativo = true;

    const carregar = async () => {
      setErro("");
      const usuarioLogado = lerUsuarioSessao();
      const chave = `planos:${usuarioLogado?.id || "geral"}`;
      const query = usuarioLogado?.id ? `?usuarioId=${usuarioLogado.id}` : "";

      if (!temCache(chave)) {
        setCarregando(true);
      }

      try {
        const dados = await buscarComCache(
          chave,
          async () => {
            const response = await fetchAutenticado(`${API_URL}${query}`, {
              cache: "no-store",
            });
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
          },
          { forcar: true },
        );
        if (ativo) setPlanos(dados);
      } catch (err) {
        console.error("Erro ao carregar planos:", err);
        if (ativo && !temCache(chave)) {
          setErro(
            err.message || "Não foi possível carregar os planos do servidor.",
          );
        }
      } finally {
        if (ativo) setCarregando(false);
      }
    };

    carregar();
    return () => {
      ativo = false;
    };
  }, []);

  const [dataInicio, dataFim] = intervaloData;
  const planosFiltrados = filtrarPlanosTreino(planos, {
    busca,
    status: filtroStatus,
    dataInicio,
    dataFim,
    ordem,
  });

  const confirmarExclusao = async () => {
    if (!planoParaExcluir) return;
    const planoId = planoParaExcluir.id;
    setExcluindo(true);
    setErro("");

    try {
      const response = await fetchAutenticado(`${API_URL}/${planoId}`, {
        method: "DELETE",
      });

      if (!response.ok) {
        const dados = await response.json().catch(() => null);
        throw new Error(dados?.mensagem || "Não foi possível excluir o plano.");
      }

      const chave = obterChavePlanos();
      const novaLista = planos.filter((p) => p.id !== planoId);
      setPlanos(novaLista);
      salvarCache(chave, novaLista);
      removerCache(`planoDetalhe:${planoId}`);
      expirarCachePorPrefixo("dashboard:");
      removerCachePorPrefixo("alunosSemTreino:");
      setPlanoParaExcluir(null);
    } catch (err) {
      console.error("Erro ao excluir plano:", err);
      setErro(err.message || "Não foi possível excluir o plano.");
      setPlanoParaExcluir(null);
    } finally {
      setExcluindo(false);
    }
  };

  return (
    <Box className="relatorio-impressao">
      <ImpressaoRelatorio />
      <Box
        sx={{
          display: "flex",
          justifyContent: "space-between",
          alignItems: "center",
          flexWrap: "wrap",
          gap: 2,
          mb: 3,
        }}
      >
        <Typography variant="h4">
          Planos de Treino
        </Typography>
        <Box
          className="no-print"
          sx={{ display: "flex", gap: 2, flexWrap: "wrap" }}
        >
          <Button
            variant="contained"
            size="large"
            startIcon={<PrintOutlinedIcon />}
            onClick={() => exportarPdfRelatorio("relatorio-planos-de-treino")}
            disabled={carregando || planosFiltrados.length === 0}
          >
            Exportar PDF
          </Button>
          <Button
            variant="contained"
            size="large"
            startIcon={<AddIcon />}
            onClick={() => navigate("/treinos")}
          >
            Novo Treino
          </Button>
        </Box>
      </Box>

      {erro && (
        <Alert severity="error" className="no-print" sx={{ mb: 3 }}>
          {erro}
        </Alert>
      )}

      <Box
        className="no-print"
        sx={{ display: "flex", gap: 2, mb: 3, flexWrap: "wrap" }}
      >
        <TextField
          placeholder="Pesquisar por nome do aluno"
          value={busca}
          onChange={(e) => setBusca(e.target.value)}
          sx={{ flex: "1 1 280px" }}
          slotProps={{
            htmlInput: { "aria-label": "Pesquisar planos por nome do aluno" },
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
          label="Status"
          value={filtroStatus}
          onChange={(e) => setFiltroStatus(e.target.value)}
          sx={{ minWidth: 160 }}
        >
          <MenuItem value="Todos">Todos</MenuItem>
          <MenuItem value="Rascunho">Rascunho</MenuItem>
          <MenuItem value="Validado">Validado</MenuItem>
          <MenuItem value="Exportado">Exportado</MenuItem>
        </TextField>

        <TextField
          select
          label="Ordenar por"
          value={ordem}
          onChange={(e) => setOrdem(e.target.value)}
          sx={{ minWidth: 180 }}
        >
          <MenuItem value="recentes">Mais recentes</MenuItem>
          <MenuItem value="antigos">Mais antigos</MenuItem>
          <MenuItem value="az">A–Z</MenuItem>
          <MenuItem value="za">Z–A</MenuItem>
        </TextField>

        <DesktopDateRangePicker
          label="Período de criação"
          startDate={dataInicio}
          endDate={dataFim}
          onChange={setIntervaloData}
        />
      </Box>

      <Paper elevation={0} sx={{ borderRadius: 4, overflow: "hidden" }}>
        <TableContainer>
          <Table>
            <TableHead>
              <TableRow>
                <TableCell>Data</TableCell>
                <TableCell>Aluno</TableCell>
                <TableCell>Status</TableCell>
                <TableCell className="no-print" align="center">
                  Ações
                </TableCell>
              </TableRow>
            </TableHead>

            <TableBody>
              {carregando && (
                <TableRow>
                  <TableCell colSpan={4} align="center" sx={{ py: 5 }}>
                    <CircularProgress size={32} />
                  </TableCell>
                </TableRow>
              )}

              {!carregando && planosFiltrados.length === 0 && (
                <TableRow>
                  <TableCell
                    colSpan={4}
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
                    </TableCell>
                    <TableCell>
                      <Chip
                        label={p.status}
                        size="small"
                        color={corChip[p.status] || "default"}
                        variant="outlined"
                      />
                    </TableCell>
                    <TableCell className="no-print" align="center">
                      <Box
                        sx={{
                          display: "flex",
                          justifyContent: "center",
                          alignItems: "center",
                          gap: 2,
                          flexWrap: "wrap",
                        }}
                      >
                        <Button
                          size="small"
                          color="inherit"
                          startIcon={
                            <VisibilityOutlinedIcon fontSize="small" />
                          }
                          onClick={() => navigate(`/planos/${p.id}`)}
                          sx={{ bgcolor: "action.hover" }}
                        >
                          Visualizar
                        </Button>
                        <Button
                          size="small"
                          color="inherit"
                          startIcon={<DeleteOutlinedIcon fontSize="small" />}
                          onClick={() => setPlanoParaExcluir(p)}
                          sx={{ bgcolor: "action.hover" }}
                        >
                          Excluir
                        </Button>
                      </Box>
                    </TableCell>
                  </TableRow>
                ))}
            </TableBody>
          </Table>
        </TableContainer>
      </Paper>

      <DialogoConfirmacao
        open={Boolean(planoParaExcluir)}
        onClose={() => !excluindo && setPlanoParaExcluir(null)}
        titulo="Confirmar exclusão"
        confirmarTexto="Excluir plano"
        desabilitado={excluindo}
        onConfirmar={confirmarExclusao}
      >
        <Typography color="text.secondary">
          Tem certeza de que deseja excluir este plano de treino? Esta ação é
          permanente e não poderá ser desfeita.
        </Typography>
      </DialogoConfirmacao>
    </Box>
  );
}
