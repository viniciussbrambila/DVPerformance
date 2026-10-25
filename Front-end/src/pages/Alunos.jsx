import { useState, useEffect } from "react";
import {
  Typography,
  Paper,
  Box,
  Button,
  IconButton,
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
  TextField,
  MenuItem,
  Grid,
  Alert,
} from "@mui/material";
import AddIcon from "@mui/icons-material/Add";
import EditOutlinedIcon from "@mui/icons-material/EditOutlined";
import DeleteOutlinedIcon from "@mui/icons-material/DeleteOutlined";
import InfoOutlinedIcon from "@mui/icons-material/InfoOutlined";
import CloseIcon from "@mui/icons-material/Close";
import SaveOutlinedIcon from "@mui/icons-material/SaveOutlined";

const API_URL = "http://localhost:5011/alunos";

const obterUsuarioLogado = () => {
  try {
    const salvo = localStorage.getItem("usuarioLogado");
    return salvo ? JSON.parse(salvo) : null;
  } catch {
    return null;
  }
};

export default function Alunos({ glassStyle }) {
  const [alunos, setAlunos] = useState([]);
  const [open, setOpen] = useState(false);
  const [editingId, setEditingId] = useState(null); // Guarda o ID do aluno em edição (ou null se for novo)
  const [tentouGravar, setTentouGravar] = useState(false); // Ativa os avisos nos campos vazios ao tentar salvar
  const [errosApi, setErrosApi] = useState([]); // Guarda mensagens de erro vindas do Back-end
  const [formData, setFormData] = useState({
    nome: "",
    idade: "",
    sexo: "",
    experiencia: "",
    frequenciaSemanal: "",
    objetivo: "",
  });

  // Carrega a lista de alunos do Back-end vinculados ao Personal logado ao abrir a página
  const carregarAlunos = async () => {
    const usuarioLogado = obterUsuarioLogado();
    if (!usuarioLogado?.id) {
      setAlunos([]);
      return;
    }

    try {
      const response = await fetch(`${API_URL}?usuarioId=${usuarioLogado.id}`);
      if (response.ok) {
        const dados = await response.json();
        setAlunos(dados);
      }
    } catch (erro) {
      console.error("Erro ao conectar com a API:", erro);
    }
  };

  useEffect(() => {
    carregarAlunos();
  }, []);

  const handleAbrirModal = () => {
    setErrosApi([]);
    setOpen(true);
  };

  const handleFecharModal = () => {
    setOpen(false);
    setEditingId(null); // Sai do modo de edição ao fechar
    setTentouGravar(false); // Limpa os avisos de erro ao fechar
    setErrosApi([]);
    setFormData({
      nome: "",
      idade: "",
      sexo: "",
      experiencia: "",
      frequenciaSemanal: "",
      objetivo: "",
    });
  };

  const handleChange = (e) => {
    const { name, value } = e.target;

    // Se for o campo idade, aceita apenas números de 0 a 9
    if (name === "idade") {
      const apenasNumeros = value.replace(/\D/g, "");
      setFormData({ ...formData, [name]: apenasNumeros });
      return;
    }

    setFormData({ ...formData, [name]: value });
  };

  // Preenche o modal com os dados do aluno selecionado para editar
  const handleEditar = (aluno) => {
    setEditingId(aluno.id);
    setTentouGravar(false);
    setErrosApi([]);
    setFormData({
      nome: aluno.nome,
      idade: String(aluno.idade),
      sexo: aluno.sexo,
      experiencia: aluno.experiencia,
      frequenciaSemanal: aluno.frequenciaSemanal,
      objetivo: aluno.objetivo,
    });
    setOpen(true);
  };

  // Remove o aluno no Back-end pelo ID
  const handleExcluir = async (id) => {
    try {
      const response = await fetch(`${API_URL}/${id}`, {
        method: "DELETE",
      });

      if (response.ok) {
        setAlunos(alunos.filter((aluno) => aluno.id !== id));
      }
    } catch (erro) {
      console.error("Erro ao excluir aluno:", erro);
    }
  };

  // Verifica se todos os 6 campos estão preenchidos corretamente
  const formularioValido =
    formData.nome.trim() !== "" &&
    String(formData.idade).trim() !== "" &&
    Number(formData.idade) > 0 &&
    formData.sexo !== "" &&
    formData.experiencia !== "" &&
    formData.frequenciaSemanal !== "" &&
    formData.objetivo.trim() !== "";

  const handleGravar = async () => {
    // Se faltar algum campo, mostra os avisos vermelhos nos campos vazios e interrompe
    if (!formularioValido) {
      setTentouGravar(true);
      return;
    }

    setErrosApi([]);

    const usuarioLogado = obterUsuarioLogado();
    if (!usuarioLogado?.id) {
      setErrosApi(["Usuário logado não identificado. Faça login novamente."]);
      return;
    }

    const payload = {
      nome: formData.nome,
      idade: Number(formData.idade),
      sexo: formData.sexo,
      experiencia: formData.experiencia,
      frequenciaSemanal: Number(formData.frequenciaSemanal),
      objetivo: formData.objetivo,
      usuarioId: usuarioLogado.id,
    };

    try {
      const url = editingId ? `${API_URL}/${editingId}` : API_URL;
      const metodo = editingId ? "PUT" : "POST";

      const response = await fetch(url, {
        method: metodo,
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(payload),
      });

      const dadosResposta = await response.json().catch(() => null);

      if (!response.ok) {
        if (dadosResposta?.erros && Array.isArray(dadosResposta.erros)) {
          setErrosApi(dadosResposta.erros);
        } else if (dadosResposta?.mensagem) {
          setErrosApi([dadosResposta.mensagem]);
        } else {
          setErrosApi(["Não foi possível salvar os dados do aluno."]);
        }
        return;
      }

      if (editingId) {
        setAlunos(
          alunos.map((aluno) =>
            aluno.id === editingId ? dadosResposta : aluno,
          ),
        );
      } else {
        setAlunos([...alunos, dadosResposta]);
      }

      handleFecharModal();
    } catch (erro) {
      setErrosApi([
        "Não foi possível conectar ao servidor Back-end (verifique se a API está rodando na porta 5011).",
      ]);
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
        <Typography variant="h4" color="text.primary">
          Gerir Alunos
        </Typography>
        <Button
          variant="contained"
          color="primary"
          startIcon={<AddIcon />}
          onClick={handleAbrirModal}
        >
          Novo Aluno
        </Button>
      </Box>

      {/* Tabela com o efeito de Vidro Fumado */}
      <TableContainer
        component={Paper}
        elevation={0}
        sx={{
          ...glassStyle,
          borderRadius: 4,
          p: 2,
          display: "flex",
          flexDirection: "column",
        }}
      >
        <Table>
          <TableHead>
            <TableRow>
              <TableCell sx={{ fontWeight: "bold" }}>Nome</TableCell>
              <TableCell align="center" sx={{ fontWeight: "bold" }}>
                Idade
              </TableCell>
              <TableCell align="center" sx={{ fontWeight: "bold" }}>
                Objetivo
              </TableCell>
              <TableCell align="center" sx={{ fontWeight: "bold" }}>
                Experiência
              </TableCell>
              <TableCell align="center" sx={{ fontWeight: "bold" }}>
                Ações
              </TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {alunos.map((aluno) => (
              <TableRow
                key={aluno.id}
                sx={{ "&:last-child td, &:last-child th": { border: 0 } }}
              >
                <TableCell component="th" scope="row">
                  {aluno.nome}
                </TableCell>
                <TableCell align="center">{aluno.idade}</TableCell>
                <TableCell align="center">{aluno.objetivo}</TableCell>
                <TableCell align="center">{aluno.experiencia}</TableCell>
                <TableCell align="center">
                  <IconButton
                    size="small"
                    aria-label="Editar aluno"
                    onClick={() => handleEditar(aluno)}
                    sx={{ mr: 1, bgcolor: "action.hover" }}
                  >
                    <EditOutlinedIcon fontSize="small" />
                  </IconButton>
                  <IconButton
                    size="small"
                    aria-label="Excluir aluno"
                    onClick={() => handleExcluir(aluno.id)}
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

      {/* Modal também com efeito de Vidro Fumado e cantos arredondados padrão */}
      <Dialog
        open={open}
        onClose={handleFecharModal}
        maxWidth="sm"
        fullWidth
        PaperProps={{ sx: { borderRadius: 4, p: 1 } }}
        slotProps={{ paper: { sx: { borderRadius: 4, p: 1 } } }}
      >
        <DialogTitle sx={{ fontWeight: 600 }}>
          {editingId ? "Editar Aluno" : "Registar Novo Aluno"}
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
                label="Nome Completo"
                name="nome"
                value={formData.nome}
                onChange={handleChange}
                variant="outlined"
                error={tentouGravar && !formData.nome.trim()}
                helperText={
                  tentouGravar && !formData.nome.trim()
                    ? "Informe o nome completo"
                    : ""
                }
              />
            </Grid>
            <Grid size={{ xs: 12, sm: 6 }}>
              <TextField
                fullWidth
                label="Idade"
                name="idade"
                value={formData.idade}
                onChange={handleChange}
                slotProps={{
                  htmlInput: { inputMode: "numeric", maxLength: 3 },
                }}
                error={
                  tentouGravar &&
                  (!String(formData.idade).trim() ||
                    Number(formData.idade) <= 0)
                }
                helperText={
                  tentouGravar &&
                  (!String(formData.idade).trim() ||
                    Number(formData.idade) <= 0)
                    ? "Informe a idade"
                    : ""
                }
              />
            </Grid>
            <Grid size={{ xs: 12, sm: 6 }}>
              <TextField
                fullWidth
                select
                label="Sexo"
                name="sexo"
                value={formData.sexo}
                onChange={handleChange}
                error={tentouGravar && !formData.sexo}
                helperText={tentouGravar && !formData.sexo ? "Obrigatório" : ""}
              >
                <MenuItem value="M">Masculino</MenuItem>
                <MenuItem value="F">Feminino</MenuItem>
              </TextField>
            </Grid>
            <Grid size={{ xs: 12, sm: 6 }}>
              <TextField
                fullWidth
                select
                label="Experiência"
                name="experiencia"
                value={formData.experiencia}
                onChange={handleChange}
                error={tentouGravar && !formData.experiencia}
                helperText={
                  tentouGravar && !formData.experiencia
                    ? "Selecione o nível"
                    : ""
                }
              >
                <MenuItem value="Iniciante">Iniciante</MenuItem>
                <MenuItem value="Intermediário">Intermediário</MenuItem>
                <MenuItem value="Avançado">Avançado</MenuItem>
              </TextField>
            </Grid>
            <Grid size={{ xs: 12, sm: 6 }}>
              <TextField
                fullWidth
                select
                label="Frequência semanal"
                name="frequenciaSemanal"
                value={formData.frequenciaSemanal}
                onChange={handleChange}
                error={tentouGravar && !formData.frequenciaSemanal}
                helperText={
                  tentouGravar && !formData.frequenciaSemanal
                    ? "Selecione a frequência"
                    : ""
                }
              >
                {[2, 3, 4, 5, 6].map((num) => (
                  <MenuItem key={num} value={num}>
                    {num} dias
                  </MenuItem>
                ))}
              </TextField>
            </Grid>
            <Grid size={12}>
              <TextField
                fullWidth
                select
                label="Objetivo"
                name="objetivo"
                value={formData.objetivo}
                onChange={handleChange}
                error={tentouGravar && !formData.objetivo.trim()}
                helperText={
                  tentouGravar && !formData.objetivo.trim()
                    ? "Selecione o objetivo"
                    : ""
                }
              >
                <MenuItem value="Hipertrofia">Hipertrofia</MenuItem>
                <MenuItem value="Emagrecimento">Emagrecimento</MenuItem>
                <MenuItem value="Força">Força</MenuItem>
                <MenuItem value="Resistência">Resistência</MenuItem>
              </TextField>
              <Box
                sx={{
                  display: "flex",
                  alignItems: "center",
                  gap: 0.75,
                  mt: 1,
                  px: 0.5,
                  color: "text.secondary",
                }}
              >
                <InfoOutlinedIcon sx={{ fontSize: 16, flexShrink: 0 }} />
                <Typography variant="caption" color="text.secondary">
                  O objetivo não influencia na geração automática de treino,
                  apenas na sessão dicas.
                </Typography>
              </Box>
            </Grid>
          </Grid>
        </DialogContent>
        <DialogActions sx={{ px: 3, pb: 2, pt: 1, gap: 1 }}>
          <Button
            onClick={handleFecharModal}
            color="inherit"
            startIcon={<CloseIcon />}
          >
            Cancelar
          </Button>
          <Button
            variant="contained"
            color="primary"
            startIcon={<SaveOutlinedIcon />}
            onClick={handleGravar}
          >
            {editingId ? "Salvar Alterações" : "Guardar"}
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
}
