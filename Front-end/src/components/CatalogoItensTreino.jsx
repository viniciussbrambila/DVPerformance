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
} from "./FocoMuscularSelect";
import FiltroNivelExercicio from "./FiltroNivelExercicio";
import FiltroGrupoMuscular, {
  CATEGORIAS_GRUPOS,
  TODOS_GRUPOS_MUSCULARES,
  itemPertenceAosGrupos,
} from "./FiltroGrupoMuscular";
import { DialogoConfirmacao } from "./DialogoConfirmacao";
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
import {
  API_BASE_URL,
  fetchAutenticado,
  lerUsuarioSessao,
} from "../services/api";

const formVazio = {
  nome: "",
  grupo: "",
  focoMuscular: "",
  nivelMinimo: "Iniciante",
  descricao: "",
};

export default function CatalogoItensTreino({
  config,
  onSelecionar,
  onVoltar,
  itemAtualId,
  diaTreino,
  modoSelecao = "substituir",
  experienciaAluno = null,
}) {
  const apiUrl = `${API_BASE_URL}/${config.endpoint}`;
  const obterChaveCache = () => {
    const usuarioLogado = lerUsuarioSessao();
    return `${config.prefixoCache}:${usuarioLogado?.id || "geral"}`;
  };

  const [itens, setItens] = useState(() => lerCache(obterChaveCache()) || []);
  const [carregando, setCarregando] = useState(
    () => !temCache(obterChaveCache()),
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

  const carregarItens = async () => {
    const usuarioLogado = lerUsuarioSessao();
    const chave = `${config.prefixoCache}:${usuarioLogado?.id || "geral"}`;
    const url = usuarioLogado?.id
      ? `${apiUrl}?usuarioId=${usuarioLogado.id}`
      : apiUrl;

    setErroCarregamento("");
    try {
      const dados = await buscarComCache(chave, async () => {
        const response = await fetchAutenticado(url);
        if (!response.ok) throw new Error(`HTTP ${response.status}`);
        return response.json();
      });
      setItens(dados);
    } catch (erro) {
      console.error(`Erro ao carregar ${config.pluralMinusculo} da API:`, erro);
      setErroCarregamento(
        "Não foi possível carregar o catálogo. Tente novamente.",
      );
    } finally {
      setCarregando(false);
    }
  };

  useEffect(() => {
    carregarItens();
  }, []);

  useEffect(() => {
    setNiveisFiltro(obterNiveisPermitidos(experienciaAluno));
  }, [experienciaAluno]);

  const nivelAluno = normalizarNivelExperiencia(experienciaAluno);
  const termoBusca = normalizarTextoBusca(busca).trim();
  const itensDisponiveis = onSelecionar
    ? filtrarCatalogoDoDia(itens, diaTreino, config.tipoCatalogo)
    : itens;
  const itensCompatibilidade = nivelAluno
    ? itensDisponiveis.filter((item) =>
        nivelPermitidoParaAluno(item.nivelMinimo, nivelAluno),
      )
    : itensDisponiveis;
  const itensFiltrados = itensCompatibilidade
    .filter(
      (item) =>
        itemPertenceAosGrupos(item, gruposFiltro) &&
        niveisFiltro.includes(
          normalizarNivelExperiencia(item.nivelMinimo) || "Iniciante",
        ) &&
        normalizarTextoBusca(item.nome).includes(termoBusca),
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

  const abrirEdicao = (item) => {
    const focoExistente =
      item.focoMuscular && item.focoMuscular !== "geral"
        ? item.focoMuscular
        : sugerirFocoMuscular(item.grupo, item.nome);
    setForm({
      nome: item.nome,
      grupo: item.grupo,
      focoMuscular: focoExistente || "",
      nivelMinimo:
        item.nivelMinimo || sugerirNivelMinimo(item.nome) || "Iniciante",
      descricao: item.descricao || "",
    });
    setFocoAutomatico(false);
    setNivelManual(Boolean(item.nivelMinimo));
    setEditandoId(item.id);
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
    const usuarioLogado = lerUsuarioSessao();

    const payload = {
      nome: form.nome.trim(),
      grupo: form.grupo.trim(),
      focoMuscular: form.focoMuscular,
      nivelMinimo: form.nivelMinimo || "Iniciante",
      descricao: form.descricao?.trim() || "",
      usuarioId: usuarioLogado?.id || null,
    };

    try {
      const url = editandoId ? `${apiUrl}/${editandoId}` : apiUrl;
      const metodo = editandoId ? "PUT" : "POST";

      const response = await fetchAutenticado(url, {
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
          setErrosApi([config.erroSalvar]);
        }
        return;
      }

      const chave = obterChaveCache();
      setItens((lista) => {
        const novaLista = editandoId
          ? lista.map((item) => (item.id === editandoId ? dados : item))
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
      console.error(`Erro ao salvar ${config.singularMinusculo}:`, erro);
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
      const response = await fetchAutenticado(`${apiUrl}/${idParaExcluir}`, {
        method: "DELETE",
      });

      if (response.ok) {
        const chave = obterChaveCache();
        setItens((lista) => {
          const novaLista = lista.filter((item) => item.id !== idParaExcluir);
          salvarCache(chave, novaLista);
          return novaLista;
        });
        expirarCachePorPrefixo("planos:");
        expirarCachePorPrefixo("planoDetalhe:");
        if (editandoId === idParaExcluir) cancelar();
      }
    } catch (erro) {
      console.error(`Erro ao excluir ${config.singularMinusculo}:`, erro);
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
            ? config.alertaAdicionar
            : config.alertaSubstituir}{" "}
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
        <Typography variant="h4">
          {config.tituloPagina}
        </Typography>

        {!onSelecionar && (
          <Button
            variant="contained"
            size="large"
            startIcon={<AddIcon />}
            onClick={abrirNovo}
          >
            {config.botaoNovo}
          </Button>
        )}
      </Box>

      <Dialog open={formAberto} onClose={cancelar} maxWidth="sm" fullWidth>
        <Box component="form" onSubmit={salvar} noValidate>
          <DialogTitle>
            {editandoId ? config.tituloEditar : config.tituloNovo}
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
                  label={config.labelNome}
                  value={form.nome}
                  onChange={handleChange("nome")}
                  slotProps={
                    config.maxLengthNome
                      ? { htmlInput: { maxLength: config.maxLengthNome } }
                      : undefined
                  }
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
                  label={config.labelGrupo}
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
                    <ListSubheader key={categoria.subtitulo}>
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
                  helperText={focoInvalido ? config.erroFocoInvalido : ""}
                />
              </Grid>

              <Grid size={12}>
                <TextField
                  fullWidth
                  select
                  label="Nível de Experiência Mínimo"
                  value={form.nivelMinimo || "Iniciante"}
                  onChange={handleChange("nivelMinimo")}
                  helperText={config.helperNivelMinimo}
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
                  label={config.labelDescricao}
                  value={form.descricao}
                  onChange={handleChange("descricao")}
                  slotProps={
                    config.maxLengthDescricao
                      ? { htmlInput: { maxLength: config.maxLengthDescricao } }
                      : undefined
                  }
                />
              </Grid>
            </Grid>
          </DialogContent>

          <DialogActions>
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

      <Box
        sx={{
          display: "flex",
          gap: 2,
          mb: 3,
          flexWrap: "wrap",
          alignItems: "center",
        }}
      >
        <TextField
          placeholder={config.placeholderBusca}
          value={busca}
          onChange={(e) => setBusca(e.target.value)}
          sx={{ flex: "1 1 300px" }}
          slotProps={{
            htmlInput: { "aria-label": config.ariaBusca },
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

      <Paper elevation={0} sx={{ borderRadius: 4, overflow: "hidden" }}>
        <TableContainer>
          <Table>
            <TableHead>
              <TableRow>
                <TableCell>Nome</TableCell>
                <TableCell>Região / Grupo Muscular</TableCell>
                <TableCell>Descrição</TableCell>
                <TableCell align="right">Ações</TableCell>
              </TableRow>
            </TableHead>

            <TableBody>
              {carregando && itens.length === 0 && (
                <TableRow>
                  <TableCell colSpan={4} align="center" sx={{ py: 5 }}>
                    <CircularProgress size={32} />
                  </TableCell>
                </TableRow>
              )}

              {!carregando && itensFiltrados.length === 0 && (
                <TableRow>
                  <TableCell
                    colSpan={4}
                    align="center"
                    sx={{ py: 4, color: "text.secondary" }}
                  >
                    {itens.length === 0
                      ? config.msgVazioCadastro
                      : onSelecionar && itensDisponiveis.length === 0
                        ? config.msgTodosNoDia
                        : onSelecionar && itensCompatibilidade.length === 0
                          ? config.msgIncompativelNivel(nivelAluno)
                          : gruposFiltro.length === 0
                            ? config.msgSemGrupoFiltro
                            : niveisFiltro.length === 0
                              ? config.msgSemNivelFiltro
                              : config.msgNenhumEncontrado}
                  </TableCell>
                </TableRow>
              )}

              {itensFiltrados.map((item) => {
                const nivelPermitido =
                  !nivelAluno ||
                  nivelPermitidoParaAluno(item.nivelMinimo, nivelAluno);

                return (
                  <TableRow key={item.id} hover>
                    <TableCell sx={{ fontWeight: 500 }}>{item.nome}</TableCell>
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
                          {item.regiao}
                        </Typography>
                        <Chip
                          label={item.grupo}
                          size="small"
                          variant="outlined"
                          sx={{ fontWeight: 500 }}
                        />
                        <FocoMuscularLabel
                          grupo={item.grupo}
                          value={item.focoMuscular}
                          nivelMinimo={item.nivelMinimo}
                          catalogo={catalogoFocos}
                        />
                      </Box>
                    </TableCell>
                    <TableCell sx={{ color: "text.secondary" }}>
                      {item.descricao}
                    </TableCell>
                    <TableCell align="right">
                      {onSelecionar ? (
                        <Button
                          variant="contained"
                          size="small"
                          startIcon={<CheckCircleOutlinedIcon />}
                          disabled={item.id === itemAtualId || !nivelPermitido}
                          onClick={() => onSelecionar(item)}
                          aria-label={
                            (modoSelecao === "adicionar"
                              ? "Adicionar "
                              : "Substituir por ") + item.nome
                          }
                        >
                          {item.id === itemAtualId
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
                            aria-label={`Editar ${config.singularMinusculo}`}
                            onClick={() => abrirEdicao(item)}
                            sx={{ bgcolor: "action.hover" }}
                          >
                            <EditOutlinedIcon fontSize="small" />
                          </IconButton>
                          <IconButton
                            size="small"
                            aria-label={`Excluir ${config.singularMinusculo}`}
                            onClick={() => excluir(item.id)}
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

      <DialogoConfirmacao
        open={Boolean(idParaExcluir)}
        onClose={cancelarExclusao}
        titulo="Confirmar exclusão"
        confirmarTexto="Excluir cadastro"
        onConfirmar={confirmarExclusao}
      >
        <Typography color="text.secondary">
          Tem certeza de que deseja excluir este cadastro? Esta ação é
          permanente e não poderá ser desfeita.
        </Typography>
      </DialogoConfirmacao>
    </Box>
  );
}
