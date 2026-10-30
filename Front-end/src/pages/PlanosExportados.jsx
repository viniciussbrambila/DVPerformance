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
  DialogContentText,
  DialogActions,
} from "@mui/material";
import { useState } from "react";
import { useNavigate } from "react-router-dom";
import SearchIcon from "@mui/icons-material/Search";
import VisibilityOutlinedIcon from "@mui/icons-material/VisibilityOutlined";
import FileDownloadOutlinedIcon from "@mui/icons-material/FileDownloadOutlined";
import DeleteOutlinedIcon from "@mui/icons-material/DeleteOutlined";

const formatarData = (iso) => new Date(iso).toLocaleDateString("pt-BR");

const corChip = { Validado: "success", Rascunho: "warning", Exportado: "info" };

function montarTexto(plano) {
  const linhas = [
    "DV Performance - Plano de Treino",
    `Aluno: ${plano.aluno}`,
    `Objetivo: ${plano.objetivo}`,
    `Frequência: ${plano.frequencia}x/semana`,
    `Data: ${formatarData(plano.data)}`,
    "",
  ];

  plano.dias.forEach((dia) => {
    linhas.push(dia.nome);
    dia.exercicios.forEach((ex) => {
      linhas.push(
        `  - ${ex.nome}: ${ex.series}x${ex.repeticoes} (descanso ${ex.descanso})`,
      );
    });
    linhas.push("");
  });

  return linhas.join("\n");
}

export default function PlanosExportados({
  glassStyle,
  planos,
  onAtualizar,
  onExcluir,
}) {
  const navigate = useNavigate();
  const [busca, setBusca] = useState("");
  const [filtroStatus, setFiltroStatus] = useState("Todos");
  const [planoParaExcluir, setPlanoParaExcluir] = useState(null);

  const planosFiltrados = planos.filter((p) => {
    const bateNome = p.aluno.toLowerCase().includes(busca.trim().toLowerCase());
    const bateStatus = filtroStatus === "Todos" || p.status === filtroStatus;
    return bateNome && bateStatus;
  });

  const baixarTxt = (plano) => {
    const blob = new Blob([montarTexto(plano)], {
      type: "text/plain;charset=utf-8",
    });
    const url = URL.createObjectURL(blob);
    const link = document.createElement("a");
    link.href = url;
    link.download = `plano-${plano.aluno.toLowerCase().replace(/\s+/g, "-")}.txt`;
    link.click();
    URL.revokeObjectURL(url);
    onAtualizar(plano.id, { status: "Exportado" });
  };

  const confirmarExclusao = () => {
    onExcluir(planoParaExcluir.id);
    setPlanoParaExcluir(null);
  };

  const cabecalhoSx = { fontWeight: 600, color: "text.secondary" };

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
              {planosFiltrados.length === 0 && (
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

              {planosFiltrados.map((p) => (
                <TableRow key={p.id} hover>
                  <TableCell sx={{ color: "text.secondary" }}>
                    {formatarData(p.data)}
                  </TableCell>
                  <TableCell sx={{ fontWeight: 600 }}>{p.aluno}</TableCell>
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
        onClose={() => setPlanoParaExcluir(null)}
        maxWidth="xs"
        fullWidth
        slotProps={{ paper: { sx: { ...glassStyle, borderRadius: 4 } } }}
      >
        <DialogTitle sx={{ fontWeight: "bold" }}>
          Confirmar exclusão
        </DialogTitle>

        <DialogContent>
          <DialogContentText sx={{ color: "text.primary" }}>
            Tem certeza de que deseja excluir este plano de treino? Esta ação é
            permanente e não poderá ser desfeita.
          </DialogContentText>
        </DialogContent>

        <DialogActions sx={{ px: 3, pb: 2.5 }}>
          <Button
            color="inherit"
            onClick={() => setPlanoParaExcluir(null)}
            sx={{ fontWeight: "bold" }}
          >
            Cancelar
          </Button>
          <Button
            variant="contained"
            color="error"
            onClick={confirmarExclusao}
            sx={{ fontWeight: "bold" }}
          >
            Excluir plano
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
}
