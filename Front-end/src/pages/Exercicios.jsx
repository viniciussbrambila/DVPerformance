import { useState, useEffect } from "react";
import FocoMuscularSelect, {
  FocoMuscularLabel,
  OPCOES_NIVEL_MINIMO,
  nivelPermitidoParaAluno,
  normalizarNivelExperiencia,
  obterNiveisPermitidos,
  sugerirFocoMuscular,
  sugerirNivelMinimo,
  useFocosMusculares,
} from "../components/FocoMuscularSelect";
import FiltroNivelExercicio from "../components/FiltroNivelExercicio";
import FiltroGrupoMuscular, {
  CATEGORIAS_GRUPOS,
  TODOS_GRUPOS_MUSCULARES,
  itemPertenceAosGrupos,
} from "../components/FiltroGrupoMuscular";
import { filtrarCatalogoDoDia } from "../utils/edicaoTreino";
import { normalizarTextoBusca } from "../utils/busca";
import {
  Box,
  Paper,
  Typography,
  Button,
  TextField,
  InputAdornment,
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
  CircularProgress,
} from "@mui/material";
import AddIcon from "@mui/icons-material/Add";
import ArrowBackIcon from "@mui/icons-material/ArrowBack";
import CheckCircleOutlinedIcon from "@mui/icons-material/CheckCircleOutlined";
import SearchIcon from "@mui/icons-material/Search";
import EditOutlinedIcon from "@mui/icons-material/EditOutlined";
import DeleteOutlinedIcon from "@mui/icons-material/DeleteOutlined";
import CloseIcon from "@mui/icons-material/Close";
import SaveOutlinedIcon from "@mui/icons-material/SaveOutlined";
import {
  buscarComCache,
  lerCache,
  salvarCache,
  temCache,
  expirarCachePorPrefixo,
} from "../utils/cacheSessao";

const API_URL = "http://localhost:5011/exercicios";

const obterUsuarioLogado = () => {
  try {
    const salvo = localStorage.getItem("usuarioLogado");
    return salvo ? JSON.parse(salvo) : null;
  } catch {
    return null;
  }
};

const formVazio = {
  nome: "",
  grupo: "",
  focoMuscular: "",
  nivelMinimo: "Iniciante",
  descricao: "",
};

const obterChaveExercicios = () => {
  const usuarioLogado = obterUsuarioLogado();
  return `exercicios:${usuarioLogado?.id || "geral"}`;
};

export default function Exercicios({
  glassStyle,
  onSelecionar,
  onVoltar,
  itemAtualId,
  diaTreino,
  modoSelecao = "substituir",
  experienciaAluno = null,
}) {
  const [exercicios, setExercicios] = useState(
    () => lerCache(obterChaveExercicios()) || [],
  );
  const [carregando, setCarregando] = useState(
    () => !temCache(obterChaveExercicios()),
  );
  const [busca, setBusca] = useState("");
  const [ordem, setOrdem] = useState("asc");
  const [gruposFiltro, setGruposFiltro] = useState(
    () => [...TODOS_GRUPOS_MUSCULARES],
  );
  const [niveisFiltro, setNiveisFiltro] = useState(() =>
    obterNiveisPermitidos(experienciaAluno),
  );
  const [formAberto, setFormAberto] = useState(false);
  const [editandoId, setEditandoId] = useState(null);
  const [idParaExcluir, setIdParaExcluir] = useState(null);
  const [form, setForm] = useState(formVazio);
  const [focoAutomatico, setFocoAutomatico] = useState(false);
  const [nivelManual, setNivelManual] = useState(false);
  const [tentouSalvar, setTentouSalvar] = useState(false);
  const [errosApi, setErrosApi] = useState([]);
  const [erroCarregamento, setErroCarregamento] = useState("");
  const catalogoFocos = useFocosMusculares();

  const carregarExercicios = async () => {
    const usuarioLogado = obterUsuarioLogado();
    const chave = `exercicios:${usuarioLogado?.id || "geral"}`;
    const url = usuarioLogado?.id
      ? `${API_URL}?usuarioId=${usuarioLogado.id}`
      : API_URL;

    setErroCarregamento("");
    try {
      const dados = await buscarComCache(chave, async () => {
        const response = await fetch(url);
        if (!response.ok) throw new Error(`HTTP ${response.status}`);
        return response.json();
      });
      setExercicios(dados);
    } catch (erro) {
      console.error("Erro ao carregar exercícios da API:", erro);
      setErroCarregamento("Não foi possível carregar o catálogo. Tente novamente.");
    } finally {
      setCarregando(false);
    }
  };

  useEffect(() => {
    carregarExercicios();
  }, []);

  useEffect(() => {
    setNiveisFiltro(obterNiveisPermitidos(experienciaAluno));
  }, [experienciaAluno]);

  const nivelAluno = normalizarNivelExperiencia(experienciaAluno);
  const termoBusca = normalizarTextoBusca(busca).trim();
  const exerciciosDisponiveis = onSelecionar
    ? filtrarCatalogoDoDia(exercicios, diaTreino, "exercicio")
    : exercicios;
  const exerciciosCompatibilidade = nivelAluno
    ? exerciciosDisponiveis.filter((ex) =>
        nivelPermitidoParaAluno(ex.nivelMinimo, nivelAluno),
      )
    : exerciciosDisponiveis;
  const exerciciosFiltrados = exerciciosCompatibilidade
    .filter(
      (ex) =>
        itemPertenceAosGrupos(ex, gruposFiltro) &&
        niveisFiltro.includes(
          normalizarNivelExperiencia(ex.nivelMinimo) || "Iniciante",
        ) &&
        normalizarTextoBusca(ex.nome).includes(termoBusca),
    )
    .sort((a, b) =>
      ordem === "asc"
        ? a.nome.localeCompare(b.nome, "pt-BR", { sensitivity: "base" })
        : b.nome.localeCompare(a.nome, "pt-BR", { sensitivity: "base" }),
    );

  const handleChange = (campo) => (e) => {
    const valor = e.target.value;
    if (campo === "focoMuscular") {
      setFocoAutomatico(false);
      setForm((anterior) => ({ ...anterior, focoMuscular: valor }));
      return;
    }
    if (campo === "nivelMinimo") {
      setNivelManual(true);
      setForm((anterior) => ({ ...anterior, nivelMinimo: valor }));
      return;
    }

    setForm((anterior) => {
      const proximo = { ...anterior, [campo]: valor };
      if (campo === "nome" || campo === "grupo") {
        const focoSugerido = sugerirFocoMuscular(proximo.grupo, proximo.nome);
        if (campo === "grupo" && valor !== anterior.grupo) {
          proximo.focoMuscular = focoSugerido;
          setFocoAutomatico(Boolean(focoSugerido));
        } else if (focoSugerido && (!anterior.focoMuscular || focoAutomatico)) {
          proximo.focoMuscular = focoSugerido;
          setFocoAutomatico(true);
        }
        if (campo === "nome" && !nivelManual) {
          proximo.nivelMinimo = sugerirNivelMinimo(proximo.nome);
        }
      }
      return proximo;
    });
  };

  const abrirNovo = () => {
    setForm(formVazio);
    setFocoAutomatico(false);
    setNivelManual(false);
    setEditandoId(null);
    setTentouSalvar(false);
    setErrosApi([]);
    setFormAberto(true);
  };

  const abrirEdicao = (exercicio) => {
    const focoExistente =
      exercicio.focoMuscular && exercicio.focoMuscular !== "geral"
        ? exercicio.focoMuscular
        : sugerirFocoMuscular(exercicio.grupo, exercicio.nome);
    setForm({
      nome: exercicio.nome,
      grupo: exercicio.grupo,
      focoMuscular: focoExistente || "",
      nivelMinimo:
        exercicio.nivelMinimo || sugerirNivelMinimo(exercicio.nome) || "Iniciante",
      descricao: exercicio.descricao || "",
    });
    setFocoAutomatico(false);
    setNivelManual(Boolean(exercicio.nivelMinimo));
    setEditandoId(exercicio.id);
    setTentouSalvar(false);
    setErrosApi([]);
    setFormAberto(true);
  };

  const cancelar = () => {
    setFormAberto(false);
    setEditandoId(null);
    setForm(formVazio);
    setFocoAutomatico(false);
    setNivelManual(false);
    setTentouSalvar(false);
    setErrosApi([]);
  };

  const salvar = async (e) => {
    e.preventDefault();
    setTentouSalvar(true);

    if (
      !form.nome.trim() ||
      !form.grupo.trim() ||
      !form.focoMuscular ||
      form.focoMuscular === "geral" ||
      !form.nivelMinimo
    ) {
      return;
    }

    setErrosApi([]);
    const usuarioLogado = obterUsuarioLogado();

    const payload = {
      nome: form.nome.trim(),
      grupo: form.grupo.trim(),
      focoMuscular: form.focoMuscular,
      nivelMinimo: form.nivelMinimo || "Iniciante",
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
          setErrosApi(["Não foi possível salvar o exercício."]);
        }
        return;
      }

      const chave = obterChaveExercicios();
      setExercicios((lista) => {
        const novaLista = editandoId
          ? lista.map((ex) => (ex.id === editandoId ? dados : ex))
          : [...lista, dados];
        salvarCache(chave, novaLista);
        return novaLista;
      });

      if (editandoId) {
        expirarCachePorPrefixo("planos:");
        expirarCachePorPrefixo("planoDetalhe:");
      }

      cancelar();
    } catch (erro) {
      console.error("Erro ao salvar exercício:", erro);
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
        const chave = obterChaveExercicios();
        setExercicios((lista) => {
          const novaLista = lista.filter((ex) => ex.id !== idParaExcluir);
          salvarCache(chave, novaLista);
          return novaLista;
        });
        expirarCachePorPrefixo("planos:");
        expirarCachePorPrefixo("planoDetalhe:");
        if (editandoId === idParaExcluir) cancelar();
      }
    } catch (erro) {
      console.error("Erro ao excluir exercício:", erro);
    } finally {
      setIdParaExcluir(null);
    }
  };

  const focoInvalido =
    tentouSalvar &&
    Boolean(form.grupo.trim()) &&
    (!form.focoMuscular || form.focoMuscular === "geral");

  return (
    <Box>
      {onSelecionar && (
        <Button
          color="inherit"
          startIcon={<ArrowBackIcon />}
          onClick={onVoltar}
          sx={{ color: "text.secondary", mb: 2 }}
        >
          Voltar ao plano
        </Button>
      )}
      {erroCarregamento && (
        <Alert severity="error" sx={{ mb: 3 }}>
          {erroCarregamento}
        </Alert>
      )}
      {onSelecionar && (
        <Alert severity="info" sx={{ mb: 3 }}>
          {modoSelecao === "adicionar"
            ? "Escolha um exercício para adicionar ao plano."
            : "Escolha um exercício para substituir o item do plano."}{" "}
          {nivelAluno
            ? `Nível do aluno: ${nivelAluno} (opções acima desse nível ficam desabilitadas). `
            : ""}
          A alteração será gravada ao salvar o plano.
        </Alert>
      )}
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

        {!onSelecionar && (
          <Button
            variant="contained"
            size="large"
            startIcon={<AddIcon />}
            onClick={abrirNovo}
          >
            Novo Exercício
          </Button>
        )}
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
            {editandoId ? "Editar Exercício" : "Novo Exercício"}
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
                  label="Nome do Exercício"
                  value={form.nome}
                  onChange={handleChange("nome")}
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
                  label="Grupo Muscular Relacionado"
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
                <FocoMuscularSelect
                  grupo={form.grupo}
                  value={form.focoMuscular}
                  onChange={handleChange("focoMuscular")}
                  catalogo={catalogoFocos}
                  error={focoInvalido}
                  sugeridoAutomaticamente={focoAutomatico}
                  helperText={
                    focoInvalido
                      ? "Selecione o padrão biomecânico do movimento para que ele encaixe na geração de treinos."
                      : ""
                  }
                />
              </Grid>

              <Grid size={12}>
                <TextField
                  fullWidth
                  select
                  label="Nível de Experiência Mínimo"
                  value={form.nivelMinimo || "Iniciante"}
                  onChange={handleChange("nivelMinimo")}
                  helperText="Define a partir de qual nível de aluno este exercício entra na geração automática (compostos multiarticulares sempre vêm antes dos isoladores)."
                >
                  {OPCOES_NIVEL_MINIMO.map((opcao) => (
                    <MenuItem key={opcao.valor} value={opcao.valor}>
                      {opcao.rotulo}
                    </MenuItem>
                  ))}
                </TextField>
              </Grid>

              <Grid size={12}>
                <TextField
                  fullWidth
                  label="Descrição / Equipamento"
                  value={form.descricao}
                  onChange={handleChange("descricao")}
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

      <Box sx={{ display: "flex", gap: 2, mb: 3, flexWrap: "wrap", alignItems: "center" }}>
        <TextField
          placeholder="Pesquisar por nome do exercício"
          value={busca}
          onChange={(e) => setBusca(e.target.value)}
          sx={{ flex: "1 1 300px" }}
          slotProps={{
            htmlInput: { "aria-label": "Pesquisar exercícios por nome" },
            input: {
              startAdornment: (
                <InputAdornment position="start">
                  <SearchIcon />
                </InputAdornment>
              ),
            },
          }}
        />
        <FiltroGrupoMuscular
          gruposSelecionados={gruposFiltro}
          onChange={setGruposFiltro}
        />
        <FiltroNivelExercicio
          niveisSelecionados={niveisFiltro}
          onChange={setNiveisFiltro}
          experienciaAluno={nivelAluno}
        />
        <TextField
          select
          label="Ordenar por"
          value={ordem}
          onChange={(e) => setOrdem(e.target.value)}
          sx={{ minWidth: 160 }}
        >
          <MenuItem value="asc">A–Z</MenuItem>
          <MenuItem value="desc">Z–A</MenuItem>
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
                <TableCell sx={{ fontWeight: 600, color: "text.primary" }}>
                  Nome
                </TableCell>
                <TableCell sx={{ fontWeight: 600, color: "text.primary" }}>
                  Região / Grupo Muscular
                </TableCell>
                <TableCell sx={{ fontWeight: 600, color: "text.primary" }}>
                  Descrição
                </TableCell>
                <TableCell
                  align="right"
                  sx={{ fontWeight: 600, color: "text.primary" }}
                >
                  Ações
                </TableCell>
              </TableRow>
            </TableHead>

            <TableBody>
              {carregando && exercicios.length === 0 && (
                <TableRow>
                  <TableCell colSpan={4} align="center" sx={{ py: 5 }}>
                    <CircularProgress size={32} />
                  </TableCell>
                </TableRow>
              )}

              {!carregando && exerciciosFiltrados.length === 0 && (
                <TableRow>
                  <TableCell
                    colSpan={4}
                    align="center"
                    sx={{ py: 4, color: "text.secondary" }}
                  >
                    {exercicios.length === 0
                      ? "Nenhum exercício cadastrado."
                      : onSelecionar && exerciciosDisponiveis.length === 0
                        ? "Todos os exercícios deste catálogo já estão neste dia do treino."
                        : onSelecionar && exerciciosCompatibilidade.length === 0
                          ? `Nenhum exercício compatível com o nível ${nivelAluno} disponível.`
                          : gruposFiltro.length === 0
                            ? "Selecione pelo menos um grupo muscular no filtro para exibir os exercícios."
                            : niveisFiltro.length === 0
                              ? "Selecione pelo menos um nível no filtro para exibir os exercícios."
                              : "Nenhum exercício encontrado."}
                  </TableCell>
                </TableRow>
              )}

              {exerciciosFiltrados.map((ex) => {
                const nivelPermitido =
                  !nivelAluno ||
                  nivelPermitidoParaAluno(ex.nivelMinimo, nivelAluno);

                return (
                  <TableRow key={ex.id} hover>
                    <TableCell sx={{ fontWeight: 500 }}>{ex.nome}</TableCell>
                    <TableCell>
                      <Box sx={{ display: "flex", flexDirection: "column", alignItems: "flex-start", gap: 0.3 }}>
                        <Typography variant="caption" color="text.secondary" sx={{ fontWeight: 600 }}>
                          {ex.regiao}
                        </Typography>
                        <Chip
                          label={ex.grupo}
                          size="small"
                          variant="outlined"
                          sx={{ fontWeight: 500 }}
                        />
                        <FocoMuscularLabel
                          grupo={ex.grupo}
                          value={ex.focoMuscular}
                          nivelMinimo={ex.nivelMinimo}
                          catalogo={catalogoFocos}
                        />
                      </Box>
                    </TableCell>
                    <TableCell sx={{ color: "text.secondary" }}>
                      {ex.descricao}
                    </TableCell>
                    <TableCell align="right">
                      {onSelecionar ? (
                        <Button
                          variant="contained"
                          size="small"
                          startIcon={<CheckCircleOutlinedIcon />}
                          disabled={ex.id === itemAtualId || !nivelPermitido}
                          onClick={() => onSelecionar(ex)}
                          aria-label={
                            (modoSelecao === "adicionar" ? "Adicionar " : "Substituir por ") + ex.nome
                          }
                        >
                          {ex.id === itemAtualId
                            ? "Atual"
                            : !nivelPermitido
                              ? "Acima do nível"
                              : modoSelecao === "adicionar"
                                ? "Adicionar"
                                : "Substituir"}
                        </Button>
                      ) : (
                        <Box
                          sx={{
                            display: "inline-flex",
                            alignItems: "center",
                            gap: 1,
                            flexWrap: "nowrap",
                          }}
                        >
                          <IconButton
                            size="small"
                            aria-label="Editar exercício"
                            onClick={() => abrirEdicao(ex)}
                            sx={{ bgcolor: "action.hover" }}
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
                        </Box>
                      )}
                    </TableCell>
                  </TableRow>
                );
              })}
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
            Tem certeza de que deseja excluir este cadastro? Esta ação é permanente e não poderá ser desfeita.
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
