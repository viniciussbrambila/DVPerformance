  import {
  Box,
  Paper,
  Typography,
  Button,
  Avatar,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  CircularProgress,
  Alert,
} from "@mui/material";
import { useState, useEffect, useRef } from "react";
import EscolhaDivisaoTreino from "../components/EscolhaDivisaoTreino";
import BoltIcon from "@mui/icons-material/Bolt";
import RocketLaunchIcon from "@mui/icons-material/RocketLaunch";
import { useNavigate } from "react-router-dom";
import {
  buscarComCache,
  lerCache,
  salvarCache,
  expirarCachePorPrefixo,
} from "../utils/cacheSessao";

const ALUNOS_API_URL = "http://localhost:5011/alunos";
const TREINOS_API_URL = "http://localhost:5011/treinos";

const obterUsuarioLogado = () => {
  try {
    const salvo = localStorage.getItem("usuarioLogado");
    return salvo ? JSON.parse(salvo) : null;
  } catch {
    return null;
  }
};

export default function Treinos({ glassStyle }) {
  const navigate = useNavigate();
  const usuarioInicial = obterUsuarioLogado();
  const [alunoId, setAlunoId] = useState("");
  const [gerando, setGerando] = useState(false);
  const [alunos, setAlunos] = useState(
    () =>
      (usuarioInicial?.id &&
        lerCache(`alunosSemTreino:${usuarioInicial.id}`)) ||
      [],
  );
  const [carregando, setCarregando] = useState(false);
  const [erros, setErros] = useState([]);
  const [escolhendoDivisao, setEscolhendoDivisao] = useState(false);
  const geracaoEmAndamento = useRef(false);
  const alunoSelecionado = alunos.find((aluno) => String(aluno.id) === String(alunoId));

  useEffect(() => {
    const carregarAlunos = async () => {
      const usuarioLogado = obterUsuarioLogado();
      if (!usuarioLogado?.id) {
        setErros(["Usuário logado não identificado. Faça login novamente."]);
        return;
      }

      const chave = `alunosSemTreino:${usuarioLogado.id}`;
      setCarregando(true);

      try {
        const dados = await buscarComCache(chave, async () => {
          const response = await fetch(
            `${ALUNOS_API_URL}/sem-treino?usuarioId=${usuarioLogado.id}`,
          );
          if (!response.ok) {
            throw new Error(`Não foi possível carregar os alunos sem treino (HTTP ${response.status}).`);
          }
          return response.json();
        });
        setAlunos(dados);
      } catch (erro) {
        console.error("Erro ao conectar com a API:", erro);
        if (!lerCache(chave)) {
          setErros([
            erro instanceof TypeError
              ? "Não foi possível conectar ao servidor. Verifique se a API está rodando em http://localhost:5011."
              : erro.message || "Não foi possível carregar os alunos sem treino.",
          ]);
        }
      } finally {
        setCarregando(false);
      }
    };

    carregarAlunos();
  }, []);

  const abrirEscolhaDivisao = () => {
    if (!alunoSelecionado || geracaoEmAndamento.current) return;
    setErros([]);
    setEscolhendoDivisao(true);
  };

  const fecharEscolhaDivisao = () => {
    if (geracaoEmAndamento.current) return;
    setEscolhendoDivisao(false);
    setErros([]);
  };

  const handleGerar = async (divisaoId) => {
    if (!alunoSelecionado || !divisaoId || geracaoEmAndamento.current) return;
    const usuarioLogado = obterUsuarioLogado();
    if (!usuarioLogado?.id) {
      setErros(["Usuário logado não identificado. Faça login novamente."]);
      return;
    }

    geracaoEmAndamento.current = true;
    setGerando(true);
    setErros([]);
    const query = new URLSearchParams({
      usuarioId: String(usuarioLogado.id),
      divisao: divisaoId,
    });

    try {
      const response = await fetch(
        TREINOS_API_URL + "/gerar/" + alunoSelecionado.id + "?" + query,
        { method: "POST" },
      );
      const dados = await response.json().catch(() => null);

      if (!response.ok) {
        if (dados?.erros && Array.isArray(dados.erros)) {
          setErros(dados.erros);
        } else if (dados?.mensagem) {
          setErros([dados.mensagem]);
        } else {
          setErros(["Não foi possível gerar o plano de treino."]);
        }
        return;
      }

      if (dados?.id == null) {
        setErros(["O servidor não retornou a ficha gerada. Tente novamente."]);
        return;
      }

      salvarCache("planoDetalhe:" + dados.id, dados);
      expirarCachePorPrefixo("planos:");
      expirarCachePorPrefixo("dashboard:");
      expirarCachePorPrefixo("alunosSemTreino:");
      navigate("/planos/" + dados.id);
    } catch (erro) {
      console.error("Erro ao gerar treino:", erro);
      setErros(["Erro de conexão ao gerar o plano de treino no servidor."]);
    } finally {
      geracaoEmAndamento.current = false;
      setGerando(false);
    }
  };

  return (
    <Box sx={{ maxWidth: 672, mx: "auto" }}>
      <Typography variant="h4" sx={{ fontWeight: "bold", mb: 3 }}>
        Gerar Plano Automático
      </Typography>

      <Paper
        elevation={0}
        sx={{
          ...glassStyle,
          p: 4,
          borderRadius: 4,
          textAlign: "center",
        }}
      >
        <Avatar
          sx={{
            width: 64,
            height: 64,
            mx: "auto",
            mb: 3,
            bgcolor: "action.hover",
            color: "primary.main",
          }}
        >
          {gerando ? (
            <CircularProgress size={28} />
          ) : (
            <BoltIcon sx={{ fontSize: 36 }} />
          )}
        </Avatar>

        <Typography variant="h6" sx={{ fontWeight: 600, mb: 1 }}>
          Motor DV Performance
        </Typography>

        <Typography variant="body1" color="text.secondary" sx={{ mb: 4 }}>
          Selecione um aluno e escolha a divisão de treino. O sistema usará a
          frequência e o perfil do aluno para montar a ficha com os exercícios e
          máquinas do catálogo.
        </Typography>

        <Box
          sx={{
            maxWidth: 448,
            mx: "auto",
            display: "flex",
            flexDirection: "column",
            gap: 2,
          }}
        >
          {erros.length > 0 && (
            <Alert severity="error" sx={{ textAlign: "left" }}>
              {erros.map((msg, idx) => (
                <div key={idx}>{msg}</div>
              ))}
            </Alert>
          )}

          {!carregando && alunos.length === 0 && (
            <Alert severity="info" sx={{ textAlign: "left" }}>
              Todos os alunos ativos já possuem treino gerado. Para criar um novo treino para um aluno, exclua o plano atual dele em <b>Planos de Treino</b>.
            </Alert>
          )}

          <FormControl fullWidth disabled={gerando || carregando || alunos.length === 0}>
            <InputLabel id="aluno-label">
              {carregando
                ? "Carregando alunos..."
                : alunos.length === 0
                  ? "Nenhum aluno sem treino disponível"
                  : "Selecione um aluno..."}
            </InputLabel>
            <Select
              labelId="aluno-label"
              id="aluno"
              value={alunoId}
              label={
                carregando
                  ? "Carregando alunos..."
                  : alunos.length === 0
                    ? "Nenhum aluno sem treino disponível"
                    : "Selecione um aluno..."
              }
              onChange={(e) => {
                setAlunoId(e.target.value);
                setErros([]);
              }}
              sx={{ textAlign: "left" }}
            >
              {alunos.length === 0 && (
                <MenuItem disabled value="">
                  {carregando
                    ? "Carregando alunos..."
                    : "Nenhum aluno sem treino ativo"}
                </MenuItem>
              )}
              {alunos.map((a) => (
                <MenuItem key={a.id} value={a.id}>
                  {a.nome} - {a.objetivo} ({a.frequenciaSemanal}x)
                </MenuItem>
              ))}
            </Select>
          </FormControl>

          <Button
            variant="contained"
            fullWidth
            size="large"
            onClick={abrirEscolhaDivisao}
            disabled={!alunoSelecionado || gerando || carregando}
            startIcon={!gerando && <RocketLaunchIcon />}
          >
            {gerando
              ? "Analisando perfil e gerando treino..."
              : "Gerar Treino"}
          </Button>
        </Box>
      </Paper>

      <EscolhaDivisaoTreino
        open={escolhendoDivisao}
        aluno={alunoSelecionado}
        gerando={gerando}
        erros={erros}
        onEscolher={handleGerar}
        onFechar={fecharEscolhaDivisao}
      />
    </Box>
  );
}
