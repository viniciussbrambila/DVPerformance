import { useState, useEffect } from "react";
import {
  Box,
  Paper,
  Typography,
  Button,
  TextField,
  MenuItem,
  ListSubheader,
  Grid,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  IconButton,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Alert,
  Chip,
} from "@mui/material";
import AddIcon from "@mui/icons-material/Add";
import EditOutlinedIcon from "@mui/icons-material/EditOutlined";
import DeleteOutlinedIcon from "@mui/icons-material/DeleteOutlined";
import CloseIcon from "@mui/icons-material/Close";
import SaveOutlinedIcon from "@mui/icons-material/SaveOutlined";

const API_URL = "http://localhost:5011/maquinas";

// Grupos musculares organizados por Subtítulo (Região do Corpo)
const CATEGORIAS_GRUPOS = [
  {
    subtitulo: "Membros Superiores (Parte de Cima)",
    regiao: "Membros Superiores",
    grupos: [
      "Peito",
      "Costas",
      "Trapézio",
      "Ombros",
      "Bíceps",
      "Tríceps",
      "Antebraço",
    ],
  },
  {
    subtitulo: "Membros Inferiores (Parte de Baixo)",
    regiao: "Membros Inferiores",
    grupos: [
      "Quadríceps",
      "Posterior de Coxa",
      "Glúteos",
      "Adutores / Abdutores",
      "Panturrilha",
    ],
  },
  {
    subtitulo: "Core / Tronco (Centro do Corpo)",
    regiao: "Core / Tronco",
    grupos: ["Abdômen", "Lombar"],
  },
];

const obterUsuarioLogado = () => {
  try {
    const salvo = localStorage.getItem("usuarioLogado");
    return salvo ? JSON.parse(salvo) : null;
  } catch {
    return null;
  }
};

const formVazio = { nome: "", grupo: "", descricao: "" };

export default function Maquinas({ glassStyle }) {
  const [maquinas, setMaquinas] = useState([]);
  const [formAberto, setFormAberto] = useState(false);
  const [editandoId, setEditandoId] = useState(null);
  const [idParaExcluir, setIdParaExcluir] = useState(null);
  const [form, setForm] = useState(formVazio);
  const [tentouSalvar, setTentouSalvar] = useState(false);
  const [errosApi, setErrosApi] = useState([]);

  const carregarMaquinas = async () => {
    const usuarioLogado = obterUsuarioLogado();
    const url = usuarioLogado?.id
      ? `${API_URL}?usuarioId=${usuarioLogado.id}`
      : API_URL;

    try {
      const response = await fetch(url);
      if (response.ok) {
        const dados = await response.json();
        setMaquinas(dados);
      }
    } catch (erro) {
      console.error("Erro ao carregar máquinas da API:", erro);
    }
  };

  useEffect(() => {
    carregarMaquinas();
  }, []);

  const handleChange = (campo) => (e) =>
    setForm((anterior) => ({ ...anterior, [campo]: e.target.value }));

  const abrirNovo = () => {
    setForm(formVazio);
    setEditandoId(null);
    setTentouSalvar(false);
    setErrosApi([]);
    setFormAberto(true);
  };

  const abrirEdicao = (maquina) => {
    setForm({
      nome: maquina.nome,
      grupo: maquina.grupo,
      descricao: maquina.descricao || "",
    });
    setEditandoId(maquina.id);
    setTentouSalvar(false);
    setErrosApi([]);
    setFormAberto(true);
  };

  const cancelar = () => {
    setFormAberto(false);
    setEditandoId(null);
    setForm(formVazio);
    setTentouSalvar(false);
    setErrosApi([]);
  };

  const salvar = async (e) => {
    e.preventDefault();
    setTentouSalvar(true);

    if (!form.nome.trim() || !form.grupo.trim()) {
      return;
    }

    setErrosApi([]);
    const usuarioLogado = obterUsuarioLogado();

    const payload = {
      nome: form.nome.trim(),
      grupo: form.grupo.trim(),
      descricao: form.descricao?.trim() || "",
      usuarioId: usuarioLogado?.id || null,
    };

    try {
      const url = editandoId ? `${API_URL}/${editandoId}` : API_URL;
      const metodo = editandoId ? "PUT" : "POST";

      const response = await fetch(url, {
        method: metodo,
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(payload),
      });

      const dados = await response.json().catch(() => null);

      if (!response.ok) {
        if (dados?.erros && Array.isArray(dados.erros)) {
          setErrosApi(dados.erros);
        } else if (dados?.mensagem) {
          setErrosApi([dados.mensagem]);
        } else {
          setErrosApi(["Não foi possível salvar a máquina."]);
        }
        return;
      }

      if (editandoId) {
        setMaquinas((lista) =>
          lista.map((maq) => (maq.id === editandoId ? dados : maq))
        );
      } else {
        setMaquinas((lista) => [...lista, dados]);
      }

      cancelar();
    } catch (erro) {
      console.error("Erro ao salvar máquina:", erro);
      setErrosApi(["Erro de conexão com o servidor."]);
    }
  };

  const excluir = (id) => {
    setIdParaExcluir(id);
  };

  const cancelarExclusao = () => {
    setIdParaExcluir(null);
  };

  const confirmarExclusao = async () => {
    if (!idParaExcluir) return;

    try {
      const response = await fetch(`${API_URL}/${idParaExcluir}`, {
        method: "DELETE",
      });

      if (response.ok) {
        setMaquinas((lista) => lista.filter((maq) => maq.id !== idParaExcluir));
        if (editandoId === idParaExcluir) cancelar();
      }
    } catch (erro) {
      console.error("Erro ao excluir máquina:", erro);
    } finally {
      setIdParaExcluir(null);
    }
  };

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
          Catálogo de Máquinas
        </Typography>

        <Button
          variant="contained"
          size="large"
          startIcon={<AddIcon />}
          onClick={abrirNovo}
        >
          Nova Máquina
        </Button>
      </Box>

      <Dialog
        open={formAberto}
        onClose={cancelar}
        maxWidth="sm"
        fullWidth
        PaperProps={{ sx: { borderRadius: 4, p: 1 } }}
        slotProps={{ paper: { sx: { borderRadius: 4, p: 1 } } }}
      >
        <Box component="form" onSubmit={salvar} noValidate>
          <DialogTitle sx={{ fontWeight: 600 }}>
            {editandoId ? "Editar Máquina" : "Nova Máquina"}
          </DialogTitle>

          <DialogContent sx={{ pt: 1 }}>
            {errosApi.length > 0 && (
              <Alert severity="error" sx={{ mb: 2 }}>
                {errosApi.map((erro, index) => (
                  <div key={index}>{erro}</div>
                ))}
              </Alert>
            )}
            <Grid container spacing={2.5} sx={{ mt: 1 }}>
              <Grid size={12}>
                <TextField
                  fullWidth
                  label="Nome da Máquina / Equipamento"
                  value={form.nome}
                  onChange={handleChange("nome")}
                  slotProps={{ htmlInput: { maxLength: 100 } }}
                  error={tentouSalvar && !form.nome.trim()}
                  helperText={
                    tentouSalvar && !form.nome.trim() ? "Informe o nome" : ""
                  }
                />
              </Grid>

              <Grid size={12}>
                <TextField
                  fullWidth
                  select
                  label="Grupo Muscular Principal Atendido"
                  value={form.grupo}
                  onChange={handleChange("grupo")}
                  error={tentouSalvar && !form.grupo.trim()}
                  helperText={
                    tentouSalvar && !form.grupo.trim()
                      ? "Selecione o grupo muscular"
                      : ""
                  }
                >
                  {CATEGORIAS_GRUPOS.flatMap((categoria) => [
                    <ListSubheader
                      key={categoria.subtitulo}
                      sx={{
                        fontWeight: 700,
                        color: "primary.main",
                        bgcolor: "background.paper",
                        lineHeight: "36px",
                      }}
                    >
                      {categoria.subtitulo}
                    </ListSubheader>,
                    ...categoria.grupos.map((grupo) => (
                      <MenuItem key={grupo} value={grupo} sx={{ pl: 3 }}>
                        {grupo}
                      </MenuItem>
                    )),
                  ])}
                </TextField>
              </Grid>

              <Grid size={12}>
                <TextField
                  fullWidth
                  label="Descrição / Especificação da Máquina"
                  value={form.descricao}
                  onChange={handleChange("descricao")}
                  slotProps={{ htmlInput: { maxLength: 255 } }}
                />
              </Grid>
            </Grid>
          </DialogContent>

          <DialogActions sx={{ px: 3, pb: 2, pt: 1, gap: 1 }}>
            <Button
              type="button"
              color="inherit"
              startIcon={<CloseIcon />}
              onClick={cancelar}
            >
              Cancelar
            </Button>
            <Button
              type="submit"
              variant="contained"
              color="primary"
              startIcon={<SaveOutlinedIcon />}
            >
              {editandoId ? "Salvar Alterações" : "Salvar"}
            </Button>
          </DialogActions>
        </Box>
      </Dialog>

      <Paper
        elevation={0}
        sx={{ ...glassStyle, borderRadius: 4, overflow: "hidden" }}
      >
        <TableContainer>
          <Table>
            <TableHead>
              <TableRow>
                <TableCell sx={{ fontWeight: 600, color: "text.secondary" }}>
                  Nome
                </TableCell>
                <TableCell sx={{ fontWeight: 600, color: "text.secondary" }}>
                  Região / Grupo Muscular
                </TableCell>
                <TableCell sx={{ fontWeight: 600, color: "text.secondary" }}>
                  Descrição
                </TableCell>
                <TableCell
                  align="right"
                  sx={{ fontWeight: 600, color: "text.secondary" }}
                >
                  Ações
                </TableCell>
              </TableRow>
            </TableHead>

            <TableBody>
              {maquinas.length === 0 && (
                <TableRow>
                  <TableCell
                    colSpan={4}
                    align="center"
                    sx={{ py: 4, color: "text.secondary" }}
                  >
                    Nenhuma máquina cadastrada.
                  </TableCell>
                </TableRow>
              )}

              {maquinas.map((maq) => (
                <TableRow key={maq.id} hover>
                  <TableCell sx={{ fontWeight: 500 }}>{maq.nome}</TableCell>
                  <TableCell>
                    <Box
                      sx={{
                        display: "flex",
                        flexDirection: "column",
                        alignItems: "flex-start",
                        gap: 0.3,
                      }}
                    >
                      <Typography
                        variant="caption"
                        color="text.secondary"
                        sx={{ fontWeight: 600 }}
                      >
                        {maq.regiao}
                      </Typography>
                      <Chip
                        label={maq.grupo}
                        size="small"
                        variant="outlined"
                        sx={{ fontWeight: 500 }}
                      />
                    </Box>
                  </TableCell>
                  <TableCell sx={{ color: "text.secondary" }}>
                    {maq.descricao}
                  </TableCell>
                  <TableCell align="right">
                    <IconButton
                      size="small"
                      aria-label="Editar máquina"
                      onClick={() => abrirEdicao(maq)}
                      sx={{ mr: 1, bgcolor: "action.hover" }}
                    >
                      <EditOutlinedIcon fontSize="small" />
                    </IconButton>
                    <IconButton
                      size="small"
                      aria-label="Excluir máquina"
                      onClick={() => excluir(maq.id)}
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
        open={Boolean(idParaExcluir)}
        onClose={cancelarExclusao}
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
            Tem certeza de que deseja excluir este cadastro? Esta ação é
            permanente e não poderá ser desfeita.
          </Typography>
        </DialogContent>
        <DialogActions sx={{ px: 3, pb: 2, pt: 1, gap: 1 }}>
          <Button onClick={cancelarExclusao} color="inherit">
            Cancelar
          </Button>
          <Button
            variant="contained"
            color="error"
            onClick={confirmarExclusao}
          >
            Excluir cadastro
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
}
