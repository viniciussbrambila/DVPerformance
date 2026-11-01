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
import { useState, useEffect } from "react";
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
      (usuarioInicial?.id && lerCache(`alunos:${usuarioInicial.id}`)) || [],
  );
  const [erros, setErros] = useState([]);

  useEffect(() => {
    const carregarAlunos = async () => {
      const usuarioLogado = obterUsuarioLogado();
      if (!usuarioLogado?.id) {
        setErros(["Usuário logado não identificado. Faça login novamente."]);
        return;
      }

      const chave = `alunos:${usuarioLogado.id}`;

      try {
        const dados = await buscarComCache(chave, async () => {
          const response = await fetch(
            `${ALUNOS_API_URL}?usuarioId=${usuarioLogado.id}`,
          );
          if (!response.ok) {
            throw new Error("Não foi possível carregar os alunos.");
          }
          return response.json();
        });
        setAlunos(dados);
      } catch (erro) {
        console.error("Erro ao conectar com a API:", erro);
        if (!lerCache(chave)) {
          setErros(["Não foi possível conectar ao servidor."]);
        }
      }
    };

    carregarAlunos();
  }, []);

  const handleGerar = async () => {
    if (!alunoId) return;
    setGerando(true);
    setErros([]);

    const usuarioLogado = obterUsuarioLogado();
    const query = usuarioLogado?.id ? `?usuarioId=${usuarioLogado.id}` : "";

    try {
      const response = await fetch(
        `${TREINOS_API_URL}/gerar/${alunoId}${query}`,
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

      if (dados?.id != null) {
        salvarCache(`planoDetalhe:${dados.id}`, dados);
      }
      expirarCachePorPrefixo("planos:");
      expirarCachePorPrefixo("dashboard:");

      navigate(`/planos/${dados.id}`);
    } catch (erro) {
      console.error("Erro ao gerar treino:", erro);
      setErros(["Erro de conexão ao gerar o plano de treino no servidor."]);
    } finally {
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
          Selecione um aluno abaixo. O sistema analisará o perfil (idade, sexo,
          experiência, objetivo e frequência) e gerará um plano de treino
          otimizado utilizando nossa biblioteca de exercícios e máquinas.
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

          <FormControl fullWidth disabled={gerando}>
            <InputLabel id="aluno-label">Selecione um aluno...</InputLabel>
            <Select
              labelId="aluno-label"
              id="aluno"
              value={alunoId}
              label="Selecione um aluno..."
              onChange={(e) => {
                setAlunoId(e.target.value);
                setErros([]);
              }}
              sx={{ textAlign: "left" }}
            >
              {alunos.length === 0 && (
                <MenuItem disabled value="">
                  Nenhum aluno cadastrado
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
            onClick={handleGerar}
            disabled={!alunoId || gerando}
            startIcon={!gerando && <RocketLaunchIcon />}
          >
            {gerando
              ? "Analisando perfil e gerando treino..."
              : "Gerar Treino"}
          </Button>
        </Box>
      </Paper>
    </Box>
  );
}
