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
  Alert,
  CircularProgress,
} from "@mui/material";
import { useState } from "react";
import BoltIcon from "@mui/icons-material/Bolt";
import RocketLaunchIcon from "@mui/icons-material/RocketLaunch";

// DADOS TEMPORÁRIOS: quando tiver backend, troque por dados da API.
const alunosMock = [
  {
    id: "1",
    nome: "Ana Souza",
    objetivo: "Emagrecimento",
    frequencia: 4,
    experiencia: "Iniciante",
  },
  {
    id: "2",
    nome: "Carlos Lima",
    objetivo: "Força",
    frequencia: 5,
    experiencia: "Avançado",
  },
  {
    id: "3",
    nome: "Marina Alves",
    objetivo: "Resistência",
    frequencia: 3,
    experiencia: "Intermediário",
  },
];

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
    repeticoes = "4-6";
    descanso = "120s";
  } else if (aluno.objetivo === "Resistência") {
    repeticoes = "15-20";
    descanso = "45s";
  } else if (aluno.objetivo === "Emagrecimento") {
    repeticoes = "12-15";
    descanso = "45s";
  }

  // Embaralha os exercícios para dar variedade
  const embaralhados = [...exercicios].sort(() => 0.5 - Math.random());
  const dias = [];

  for (let i = 1; i <= aluno.frequencia; i++) {
    const exerciciosDoDia = [];
    for (let j = 0; j < exerciciosPorDia; j++) {
      const ex = embaralhados[(i * exerciciosPorDia + j) % embaralhados.length];
      exerciciosDoDia.push({
        id: crypto.randomUUID(),
        exercicioId: ex.id,
        nome: ex.nome,
        series,
        repeticoes,
        descanso,
      });
    }
    dias.push({ nome: `Dia ${i}`, exercicios: exerciciosDoDia });
  }

  return {
    alunoId: aluno.id,
    data: new Date().toISOString(),
    status: "Rascunho",
    dias,
  };
}

export default function Treinos({ glassStyle }) {
  const [alunoId, setAlunoId] = useState("");
  const [gerando, setGerando] = useState(false);
  const [planoGerado, setPlanoGerado] = useState(null);

  const handleGerar = () => {
    if (!alunoId) return;
    setGerando(true);
    setPlanoGerado(null);

    // Simula o tempo de geração
    setTimeout(() => {
      const aluno = alunosMock.find((a) => a.id === alunoId);
      if (!aluno) {
        setGerando(false);
        return;
      }

      const plano = gerarPlano(aluno, exerciciosMock);
      console.log("Plano gerado:", plano); // depois trocar pela chamada à API
      setPlanoGerado({ aluno: aluno.nome, dias: plano.dias.length });
      setGerando(false);
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
          {planoGerado && (
            <Alert severity="success" sx={{ textAlign: "left" }}>
              Plano de {planoGerado.dias} dias gerado para {planoGerado.aluno}!
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
                setPlanoGerado(null);
              }}
              sx={{ textAlign: "left" }}
            >
              {alunosMock.map((a) => (
                <MenuItem key={a.id} value={a.id}>
                  {a.nome} - {a.objetivo} ({a.frequencia}x)
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
