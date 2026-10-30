import {
  Drawer,
  List,
  Divider,
  ListItemButton,
  ListItemIcon,
  ListItemText,
  ListSubheader,
  Box,
  Typography,
} from "@mui/material";
import { Link, useLocation } from "react-router-dom";
import PeopleIcon from "@mui/icons-material/People";
import FitnessCenterIcon from "@mui/icons-material/FitnessCenter";
import DirectionsRunIcon from "@mui/icons-material/DirectionsRun";
import AddTaskIcon from "@mui/icons-material/AddTask";
import AssignmentIcon from "@mui/icons-material/Assignment";
import DashboardIcon from "@mui/icons-material/Dashboard";
import LogoutIcon from "@mui/icons-material/Logout";

const iconSx = { minWidth: 0, mr: 3, justifyContent: "center" };
const itemSx = {
  minHeight: 48,
  px: 2.5,
  mx: 1,
  mb: 0.5,
  borderRadius: 2,
  "&.Mui-selected": {
    bgcolor: "primary.main",
    color: "primary.contrastText",
    "&:hover": { bgcolor: "primary.dark" },
    "& .MuiListItemIcon-root": { color: "inherit" },
  },
};

export default function Sidebar({
  drawerWidth,
  glassStyle,
  onSair,
  usuarioLogado,
}) {
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
          borderRight: "none",
          ...(glassStyle || {}),
        },
      }}
    >
      <Box sx={{ px: 2.5, py: 2.5 }}>
        <Typography variant="h5" sx={{ fontWeight: "bold" }} noWrap>
          DV Performance
        </Typography>
        <Typography
          variant="h6"
          sx={{ fontSize: 14, fontWeight: 500, color: "text.secondary" }}
          noWrap
        >
          Sistema de Geração de Treino
        </Typography>
      </Box>

      <Divider />

      <List component="nav">
        <ListItemButton
          component={Link}
          to="/dashboard"
          selected={estaAtivo("/dashboard")}
          sx={itemSx}
        >
          <ListItemIcon sx={iconSx}>
            <DashboardIcon />
          </ListItemIcon>
          <ListItemText primary="Dashboard" />
        </ListItemButton>

        <ListItemButton
          component={Link}
          to="/treinos"
          selected={estaAtivo("/treinos")}
          sx={itemSx}
        >
          <ListItemIcon sx={iconSx}>
            <AddTaskIcon />
          </ListItemIcon>
          <ListItemText primary="Gerar Treino" />
        </ListItemButton>

        <ListItemButton
          component={Link}
          to="/alunos"
          selected={estaAtivo("/alunos")}
          sx={itemSx}
        >
          <ListItemIcon sx={iconSx}>
            <PeopleIcon />
          </ListItemIcon>
          <ListItemText primary="Alunos" />
        </ListItemButton>

        <ListItemButton
          component={Link}
          to="/exercicios"
          selected={estaAtivo("/exercicios")}
          sx={itemSx}
        >
          <ListItemIcon sx={iconSx}>
            <DirectionsRunIcon />
          </ListItemIcon>
          <ListItemText primary="Exercícios" />
        </ListItemButton>

        <ListItemButton
          component={Link}
          to="/maquinas"
          selected={estaAtivo("/maquinas")}
          sx={itemSx}
        >
          <ListItemIcon sx={iconSx}>
            <FitnessCenterIcon />
          </ListItemIcon>
          <ListItemText primary="Máquinas" />
        </ListItemButton>

        <Divider sx={{ my: 1 }} />

        <ListSubheader
          component="div"
          inset
          sx={{ backgroundColor: "transparent" }}
        >
          Relatórios
        </ListSubheader>

        <ListItemButton
          component={Link}
          to="/planos"
          selected={estaAtivo("/planos")}
          sx={itemSx}
        >
          <ListItemIcon sx={iconSx}>
            <AssignmentIcon />
          </ListItemIcon>
          <ListItemText primary="Planos Exportados" />
        </ListItemButton>
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

        <ListItemButton
          onClick={onSair}
          sx={{ ...itemSx, mb: 1, color: "error.main" }}
        >
          <ListItemIcon sx={{ ...iconSx, color: "inherit" }}>
            <LogoutIcon />
          </ListItemIcon>
          <ListItemText primary="Sair" />
        </ListItemButton>
      </Box>
    </Drawer>
  );
}
