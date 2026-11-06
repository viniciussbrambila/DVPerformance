import { Box, Paper, Typography, Avatar, ButtonBase } from "@mui/material";
import { useNavigate } from "react-router-dom";
import { useState, useEffect } from "react";
import PeopleOutlinedIcon from "@mui/icons-material/PeopleOutlined";
import DescriptionOutlinedIcon from "@mui/icons-material/DescriptionOutlined";
import AddTaskIcon from "@mui/icons-material/AddTask";
import DirectionsRunIcon from "@mui/icons-material/DirectionsRun";
import FitnessCenterIcon from "@mui/icons-material/FitnessCenter";
import AssessmentOutlinedIcon from "@mui/icons-material/AssessmentOutlined";
import { buscarComCache, lerCache } from "../utils/cacheSessao";

const DASHBOARD_RESUMO_URL = "http://localhost:5011/dashboard/resumo";

const obterUsuarioLogado = () => {
  try {
    const salvo = localStorage.getItem("usuarioLogado");
    return salvo ? JSON.parse(salvo) : null;
  } catch {
    return null;
  }
};

function CardContagem({ glassStyle, icone, titulo, valor }) {
  return (
    <Paper
      elevation={0}
      sx={{
        ...glassStyle,
        p: 3,
        borderRadius: 4,
        display: "flex",
        alignItems: "center",
        gap: 2,
        minWidth: 260,
      }}
    >
      <Avatar
        sx={{
          width: 56,
          height: 56,
          bgcolor: "action.hover",
          color: "primary.main",
        }}
      >
        {icone}
      </Avatar>
      <Box>
        <Typography variant="body2" color="text.secondary">
          {titulo}
        </Typography>
        <Typography variant="h4" sx={{ fontWeight: "bold" }}>
          {valor}
        </Typography>
      </Box>
    </Paper>
  );
}

function BotaoAcesso({ glassStyle, icone, texto, onClick }) {
  return (
    <Paper
      elevation={0}
      sx={{ ...glassStyle, borderRadius: 3, minWidth: 0 }}
    >
      <ButtonBase
        onClick={onClick}
        sx={{
          width: "100%",
          height: "100%",
          justifyContent: "flex-start",
          gap: 2,
          px: 3,
          py: 2,
          borderRadius: 3,
          color: "text.primary",
        }}
      >
        <Box sx={{ display: "flex", color: "primary.main" }}>{icone}</Box>
        <Typography variant="subtitle1" sx={{ fontWeight: 500 }}>
          {texto}
        </Typography>
      </ButtonBase>
    </Paper>
  );
}

export default function Dashboard({ glassStyle }) {
  const navigate = useNavigate();
  const usuarioInicial = obterUsuarioLogado();
  const chaveCache = usuarioInicial?.id ? `dashboard:${usuarioInicial.id}` : null;
  const resumoSalvo = chaveCache ? lerCache(chaveCache) : null;

  const [totalAlunos, setTotalAlunos] = useState(
    resumoSalvo?.totalAlunos ?? 0,
  );
  const [totalPlanos, setTotalPlanos] = useState(
    resumoSalvo?.totalPlanos ?? 0,
  );

  useEffect(() => {
    const carregarContagens = async () => {
      const usuarioLogado = obterUsuarioLogado();
      if (!usuarioLogado?.id) return;

      const chave = `dashboard:${usuarioLogado.id}`;

      try {
        const resumo = await buscarComCache(chave, async () => {
          const resp = await fetch(
            `${DASHBOARD_RESUMO_URL}?usuarioId=${usuarioLogado.id}`,
          );

          if (!resp.ok) throw new Error("HTTP " + resp.status);
          return resp.json();
        });

        setTotalAlunos(resumo.totalAlunos ?? 0);
        setTotalPlanos(resumo.totalPlanos ?? 0);
      } catch (erro) {
        console.error("Erro ao carregar contagens do dashboard:", erro);
      }
    };

    carregarContagens();
  }, []);

  return (
    <Box>
      <Typography variant="h4" sx={{ fontWeight: "bold", mb: 3 }}>
        Dashboard
      </Typography>

      <Box sx={{ display: "flex", flexWrap: "wrap", gap: 3, mb: 5 }}>
        <CardContagem
          glassStyle={glassStyle}
          icone={<PeopleOutlinedIcon />}
          titulo="Alunos Cadastrados"
          valor={totalAlunos}
        />
        <CardContagem
          glassStyle={glassStyle}
          icone={<DescriptionOutlinedIcon />}
          titulo="Planos Gerados"
          valor={totalPlanos}
        />
      </Box>

      <Typography variant="h6" sx={{ fontWeight: 600, mb: 2 }}>
        Acesso Rápido
      </Typography>

      <Box
        sx={{
          display: "grid",
          gridTemplateColumns: {
            xs: "1fr",
            sm: "repeat(2, minmax(0, 1fr))",
            md: "repeat(3, minmax(0, 1fr))",
          },
          gridAutoRows: "1fr",
          gap: 2,
        }}
      >
        <BotaoAcesso
          glassStyle={glassStyle}
          icone={<PeopleOutlinedIcon />}
          texto="Gerenciar Alunos"
          onClick={() => navigate("/alunos")}
        />
        <BotaoAcesso
          glassStyle={glassStyle}
          icone={<AddTaskIcon />}
          texto="Gerar Novo Treino"
          onClick={() => navigate("/treinos")}
        />
        <BotaoAcesso
          glassStyle={glassStyle}
          icone={<DescriptionOutlinedIcon />}
          texto="Ver Planos"
          onClick={() => navigate("/planos")}
        />
        <BotaoAcesso
          glassStyle={glassStyle}
          icone={<AssessmentOutlinedIcon />}
          texto="Relatórios"
          onClick={() => navigate("/relatorios")}
        />
        <BotaoAcesso
          glassStyle={glassStyle}
          icone={<DirectionsRunIcon />}
          texto="Exercícios"
          onClick={() => navigate("/exercicios")}
        />
        <BotaoAcesso
          glassStyle={glassStyle}
          icone={<FitnessCenterIcon />}
          texto="Máquinas"
          onClick={() => navigate("/maquinas")}
        />
      </Box>
    </Box>
  );
}
