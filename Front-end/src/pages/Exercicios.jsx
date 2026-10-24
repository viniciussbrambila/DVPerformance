import {
  Box,
  Paper,
  Typography,
  Button,
  TextField,
  IconButton,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
} from "@mui/material";
import { useState } from "react";
import AddIcon from "@mui/icons-material/Add";
import EditOutlinedIcon from "@mui/icons-material/EditOutlined";
import DeleteOutlinedIcon from "@mui/icons-material/DeleteOutlined";
import CloseIcon from "@mui/icons-material/Close";
import SaveOutlinedIcon from "@mui/icons-material/SaveOutlined";

// DADOS TEMPORÁRIOS: quando tiver backend, troque por dados da API.
const exerciciosIniciais = [
  {
    id: "1",
    nome: "Supino reto",
    grupo: "Peito",
    descricao: "Exercício com barra no banco reto",
  },
  {
    id: "2",
    nome: "Agachamento livre",
    grupo: "Pernas",
    descricao: "Exercício composto para membros inferiores",
  },
  {
    id: "3",
    nome: "Puxada frontal",
    grupo: "Costas",
    descricao: "Exercício na polia alta",
  },
  {
    id: "4",
    nome: "Rosca direta",
    grupo: "Bíceps",
    descricao: "Exercício com barra ou halteres",
  },
];

const formVazio = { nome: "", grupo: "", descricao: "" };

export default function Exercicios({ glassStyle }) {
  const [exercicios, setExercicios] = useState(exerciciosIniciais);
  const [formAberto, setFormAberto] = useState(false);
  const [editandoId, setEditandoId] = useState(null);
  const [form, setForm] = useState(formVazio);
  const [tentouSalvar, setTentouSalvar] = useState(false);

  const handleChange = (campo) => (e) =>
    setForm((anterior) => ({ ...anterior, [campo]: e.target.value }));

  const abrirNovo = () => {
    setForm(formVazio);
    setEditandoId(null);
    setTentouSalvar(false);
    setFormAberto(true);
  };

  const abrirEdicao = (exercicio) => {
    setForm({
      nome: exercicio.nome,
      grupo: exercicio.grupo,
      descricao: exercicio.descricao,
    });
    setEditandoId(exercicio.id);
    setTentouSalvar(false);
    setFormAberto(true);
  };

  const cancelar = () => {
    setFormAberto(false);
    setEditandoId(null);
    setForm(formVazio);
    setTentouSalvar(false);
  };

  const salvar = (e) => {
    e.preventDefault();
    setTentouSalvar(true);

    if (!form.nome.trim() || !form.grupo.trim()) {
      return;
    }

    if (editandoId) {
      setExercicios((lista) =>
        lista.map((ex) => (ex.id === editandoId ? { ...ex, ...form } : ex)),
      );
    } else {
      setExercicios((lista) => [
        ...lista,
        { id: crypto.randomUUID(), ...form },
      ]);
    }

    cancelar();
  };

  const excluir = (id) => {
    if (window.confirm("Deseja realmente excluir este exercício?")) {
      setExercicios((lista) => lista.filter((ex) => ex.id !== id));
      if (editandoId === id) cancelar();
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
          Catálogo de Exercícios
        </Typography>

        <Button
          variant="contained"
          size="large"
          startIcon={<AddIcon />}
          onClick={abrirNovo}
        >
          Novo Exercício
        </Button>
      </Box>

      {formAberto && (
        <Paper
          elevation={0}
          component="form"
          onSubmit={salvar}
          noValidate
          sx={{ ...glassStyle, p: 3, borderRadius: 4, mb: 3 }}
        >
          <Typography variant="h6" sx={{ fontWeight: 600, mb: 2 }}>
            {editandoId ? "Editar Exercício" : "Novo Exercício"}
          </Typography>

          <Box
            sx={{
              display: "grid",
              gridTemplateColumns: { xs: "1fr", md: "1fr 1fr" },
              gap: 2,
            }}
          >
            <TextField
              label="Nome"
              value={form.nome}
              onChange={handleChange("nome")}
              error={tentouSalvar && !form.nome.trim()}
              helperText={
                tentouSalvar && !form.nome.trim() ? "Informe o nome" : ""
              }
            />

            <TextField
              label="Grupo Muscular Relacionado"
              value={form.grupo}
              onChange={handleChange("grupo")}
              error={tentouSalvar && !form.grupo.trim()}
              helperText={
                tentouSalvar && !form.grupo.trim()
                  ? "Informe o grupo muscular"
                  : ""
              }
            />

            <TextField
              label="Descrição"
              value={form.descricao}
              onChange={handleChange("descricao")}
              sx={{ gridColumn: { md: "1 / -1" } }}
            />
          </Box>

          <Box
            sx={{
              display: "flex",
              justifyContent: "flex-end",
              gap: 1.5,
              mt: 3,
            }}
          >
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
              startIcon={<SaveOutlinedIcon />}
            >
              Salvar
            </Button>
          </Box>
        </Paper>
      )}

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
                  Grupo Relacionado
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
              {exercicios.length === 0 && (
                <TableRow>
                  <TableCell
                    colSpan={4}
                    align="center"
                    sx={{ py: 4, color: "text.secondary" }}
                  >
                    Nenhum exercício cadastrado.
                  </TableCell>
                </TableRow>
              )}

              {exercicios.map((ex) => (
                <TableRow key={ex.id} hover>
                  <TableCell sx={{ fontWeight: 500 }}>{ex.nome}</TableCell>
                  <TableCell sx={{ color: "text.secondary" }}>
                    {ex.grupo}
                  </TableCell>
                  <TableCell sx={{ color: "text.secondary" }}>
                    {ex.descricao}
                  </TableCell>
                  <TableCell align="right">
                    <IconButton
                      size="small"
                      aria-label="Editar exercício"
                      onClick={() => abrirEdicao(ex)}
                      sx={{ mr: 1, bgcolor: "action.hover" }}
                    >
                      <EditOutlinedIcon fontSize="small" />
                    </IconButton>
                    <IconButton
                      size="small"
                      aria-label="Excluir exercício"
                      onClick={() => excluir(ex.id)}
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
    </Box>
  );
}
