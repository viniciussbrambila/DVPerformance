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

const API_URL = "http://localhost:5011/alunos";

const obterUsuarioLogado = () => {
  try {
    const salvo = localStorage.getItem("usuarioLogado");
    return salvo ? JSON.parse(salvo) : null;
  } catch {
    return null;
  }
};

const exerciciosMock = [
  { id: "e1", nome: "Supino reto" },
  { id: "e2", nome: "Agachamento livre" },
  { id: "e3", nome: "Remada curvada" },
  { id: "e4", nome: "Desenvolvimento com halteres" },
  { id: "e5", nome: "Leg press" },
  { id: "e6", nome: "Puxada frontal" },
  { id: "e7", nome: "Rosca direta" },
  { id: "e8", nome: "Tríceps na polia" },
  { id: "e9", nome: "Cadeira extensora" },
  { id: "e10", nome: "Mesa flexora" },
  { id: "e11", nome: "Elevação lateral" },
  { id: "e12", nome: "Panturrilha em pé" },
];

function gerarPlano(aluno, exercicios) {
  const frequencia = Number(aluno.frequenciaSemanal);
  for (let i = 1; i <= frequencia; i++) {
    let series = 3;
    let repeticoes = "10-12";
    let descanso = "60s";
    let exerciciosPorDia = 5;

    if (aluno.experiencia === "Iniciante") {
      series = 3;
      exerciciosPorDia = 4;
    } else if (aluno.experiencia === "Intermediário") {
      series = 4;
      exerciciosPorDia = 5;
    } else {
      series = 4;
      exerciciosPorDia = 6;
    }

    if (aluno.objetivo === "Força") {
      repeticoes = "6-8";
      descanso = "120s";
    } else if (aluno.objetivo === "Resistência") {
      repeticoes = "15-20";
      descanso = "45s";
    } else if (aluno.objetivo === "Emagrecimento") {
      repeticoes = "12-15";
      descanso = "45s";
    }
  }
  // Embaralha os exercícios para dar variedade
  const embaralhados = [...exercicios].sort(() => 0.5 - Math.random());
  const dias = [];

  return {
    alunoId: aluno.id,
    data: new Date().toISOString(),
    status: "Rascunho",
    dias,
  };
}

export default function Treinos({ glassStyle, onGerarPlano }) {
  const navigate = useNavigate();
  const [alunoId, setAlunoId] = useState("");
  const [gerando, setGerando] = useState(false);
  const [alunos, setAlunos] = useState([]);
  const [erroAlunos, setErroAlunos] = useState("");

  useEffect(() => {
    const carregarAlunos = async () => {
      const usuarioLogado = obterUsuarioLogado();
      if (!usuarioLogado?.id) {
        setErroAlunos("Usuário logado não identificado. Faça login novamente.");
        return;
      }

      try {
        const response = await fetch(
          `${API_URL}?usuarioId=${usuarioLogado.id}`,
        );
        if (response.ok) {
          setAlunos(await response.json());
        } else {
          setErroAlunos("Não foi possível carregar os alunos.");
        }
      } catch (erro) {
        console.error("Erro ao conectar com a API:", erro);
        setErroAlunos("Não foi possível conectar ao servidor.");
      }
    };

    carregarAlunos();
  }, []);

  const handleGerar = () => {
    if (!alunoId) return;
    setGerando(true);

    // Simula o tempo de geração
    setTimeout(() => {
      const aluno = alunos.find((a) => String(a.id) === String(alunoId));
      if (!aluno) {
        setGerando(false);
        return;
      }

      const plano = gerarPlano(aluno, exerciciosMock);
      const novoId = onGerarPlano({
        ...plano,
        aluno: aluno.nome,
        objetivo: aluno.objetivo,
        frequencia: Number(aluno.frequenciaSemanal),
      });

      setGerando(false);
      navigate(`/planos/${novoId}`);
    }, 1500);
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
          experiência e frequência) e gerará um plano de treino otimizado
          utilizando nossa biblioteca de exercícios e máquinas.
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
          {erroAlunos && <Alert severity="error">{erroAlunos}</Alert>}
          <FormControl fullWidth disabled={gerando}>
            <InputLabel id="aluno-label">Selecione um aluno...</InputLabel>
            <Select
              labelId="aluno-label"
              id="aluno"
              value={alunoId}
              label="Selecione um aluno..."
              onChange={(e) => setAlunoId(e.target.value)}
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
            {gerando ? "Analisando perfil e gerando treino..." : "Gerar Treino"}
          </Button>
        </Box>
      </Paper>
    </Box>
  );
}
