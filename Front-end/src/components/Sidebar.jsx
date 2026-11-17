import {
  Drawer,
  List,
  Divider,
  ListItemButton,
  ListItemIcon,
  ListItemText,
  Box,
  Typography,
} from "@mui/material";
import { styled } from "@mui/material/styles";
import { Link, useLocation } from "react-router-dom";
import Marca from "./Marca";
import PeopleIcon from "@mui/icons-material/People";
import FitnessCenterIcon from "@mui/icons-material/FitnessCenter";
import DirectionsRunIcon from "@mui/icons-material/DirectionsRun";
import AddTaskIcon from "@mui/icons-material/AddTask";
import AssignmentIcon from "@mui/icons-material/Assignment";
import DashboardIcon from "@mui/icons-material/Dashboard";
import LogoutIcon from "@mui/icons-material/Logout";

const NavItemButton = styled(ListItemButton)(({ theme }) => ({
  minHeight: 48,
  paddingInline: theme.spacing(2.5),
  marginInline: theme.spacing(1),
  marginBottom: theme.spacing(0.5),
  borderRadius: theme.spacing(2),
  "& .MuiListItemIcon-root": {
    minWidth: 0,
    marginRight: theme.spacing(3),
    justifyContent: "center",
  },
  "&.Mui-selected": {
    backgroundColor: theme.palette.primary.main,
    color: theme.palette.primary.contrastText,
    "&:hover": {
      backgroundColor: theme.palette.primary.dark,
    },
    "& .MuiListItemIcon-root": {
      color: "inherit",
    },
  },
}));

const ITENS_NAVEGACAO = [
  { rota: "/dashboard", texto: "Dashboard", Icone: DashboardIcon },
  { rota: "/treinos", texto: "Gerar Treino", Icone: AddTaskIcon },
  { rota: "/alunos", texto: "Alunos", Icone: PeopleIcon },
  { rota: "/exercicios", texto: "Exercícios", Icone: DirectionsRunIcon },
  { rota: "/maquinas", texto: "Máquinas", Icone: FitnessCenterIcon },
  { rota: "/planos", texto: "Planos de Treino", Icone: AssignmentIcon },
];

export default function Sidebar({ drawerWidth, onSair, usuarioLogado }) {
  const { pathname } = useLocation();
  const estaAtivo = (rota) =>
    pathname === rota || pathname.startsWith(`${rota}/`);

  return (
    <Drawer
      variant="permanent"
      sx={{
        width: drawerWidth,
        flexShrink: 0,
        "& .MuiDrawer-paper": {
          width: drawerWidth,
          boxSizing: "border-box",
          display: "flex",
          flexDirection: "column",
          whiteSpace: "nowrap",
        },
      }}
    >
      <Box sx={{ px: 2.5, py: 2.5 }}>
        <Marca compacta />
        <Typography
          variant="h6"
          sx={{
            mt: 0.5,
            fontSize: 14,
            fontWeight: 500,
            color: "text.secondary",
          }}
          noWrap
        >
          Sistema de Geração de Treino
        </Typography>
      </Box>

      <Divider />

      <List component="nav">
        {ITENS_NAVEGACAO.map(({ rota, texto, Icone }) => (
          <NavItemButton
            key={rota}
            component={Link}
            to={rota}
            selected={estaAtivo(rota)}
          >
            <ListItemIcon>
              <Icone />
            </ListItemIcon>
            <ListItemText primary={texto} />
          </NavItemButton>
        ))}
      </List>

      <Box sx={{ mt: "auto" }}>
        <Divider />

        <Box sx={{ px: 2.5, py: 1.5 }}>
          <Typography variant="subtitle2" sx={{ fontWeight: "bold" }} noWrap>
            {usuarioLogado?.nome || usuarioLogado?.usuario}
          </Typography>
          <Typography variant="caption" color="text.secondary">
            {usuarioLogado?.perfil === "admin" ? "Admin" : "Personal"}
          </Typography>
        </Box>

        <NavItemButton onClick={onSair} sx={{ mb: 1, color: "error.main" }}>
          <ListItemIcon sx={{ color: "inherit" }}>
            <LogoutIcon />
          </ListItemIcon>
          <ListItemText primary="Sair" />
        </NavItemButton>
      </Box>
    </Drawer>
  );
}
