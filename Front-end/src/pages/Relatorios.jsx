import { Box, Paper, Typography, Button, Grid, Avatar } from "@mui/material";
import { Link } from "react-router-dom";
import PersonSearchOutlinedIcon from "@mui/icons-material/PersonSearchOutlined";
import PeopleOutlinedIcon from "@mui/icons-material/PeopleOutlined";
import AssignmentOutlinedIcon from "@mui/icons-material/AssignmentOutlined";

const RELATORIOS = [
  {
    titulo: "Relatório de Características dos Alunos",
    descricao:
      "Consulte o perfil individual de um aluno e suas caracteristicas!",
    icone: <PersonSearchOutlinedIcon />,
    rota: "/relatorios/caracteristicas",
  },
  {
    titulo: "Relatório de Alunos Cadastrados",
    descricao: "Listagem gerencial de todos os alunos ativos no sistema",
    icone: <PeopleOutlinedIcon />,
    rota: "/relatorios/alunos",
  },
  {
    titulo: "Relatório de Planos de Treino",
    descricao: "Todos os planos de treino gerados para os alunos",
    icone: <AssignmentOutlinedIcon />,
    rota: "/relatorios/planos",
  },
];

export default function Relatorios({ glassStyle }) {
  return (
    <Box>
      <Typography variant="h4" sx={{ fontWeight: "bold", mb: 3 }}>
        Relatórios
      </Typography>

      <Grid container spacing={3}>
        {RELATORIOS.map((relatorio) => (
          <Grid key={relatorio.titulo} size={{ xs: 12, md: 6 }}>
            <Paper
              elevation={0}
              sx={{
                ...glassStyle,
                p: 3,
                borderRadius: 4,
                height: "100%",
                display: "flex",
                flexDirection: "column",
                alignItems: "flex-start",
              }}
            >
              <Avatar
                variant="rounded"
                sx={{
                  width: 48,
                  height: 48,
                  mb: 2.5,
                  borderRadius: 3,
                  bgcolor: "action.hover",
                  color: "primary.main",
                }}
              >
                {relatorio.icone}
              </Avatar>
              <Typography variant="h6" sx={{ fontWeight: 600, mb: 1 }}>
                {relatorio.titulo}
              </Typography>
              <Typography variant="body1" color="text.secondary" sx={{ mb: 3 }}>
                {relatorio.descricao}
              </Typography>
              <Button
                component={Link}
                to={relatorio.rota}
                variant="contained"
                size="large"
                aria-label={`Acessar ${relatorio.titulo}`}
                sx={{ mt: "auto" }}
              >
                Acessar relatório
              </Button>
            </Paper>
          </Grid>
        ))}
      </Grid>
    </Box>
  );
}
