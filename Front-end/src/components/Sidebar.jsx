import { useState } from "react";
import {
  Drawer,
  List,
  Divider,
  ListItemButton,
  ListItemIcon,
  ListItemText,
  Box,
  Typography,
  IconButton,
  Tooltip,
} from "@mui/material";
import { styled } from "@mui/material/styles";
import { Link, useLocation } from "react-router-dom";
import Marca from "./Marca";
import DialogoSobreUsuario from "./DialogoSobreUsuario";
import { DialogoConfirmacao } from "./DialogoConfirmacao";
import PeopleIcon from "@mui/icons-material/People";
import FitnessCenterIcon from "@mui/icons-material/FitnessCenter";
import DirectionsRunIcon from "@mui/icons-material/DirectionsRun";
import AddTaskIcon from "@mui/icons-material/AddTask";
import AssignmentIcon from "@mui/icons-material/Assignment";
import DashboardIcon from "@mui/icons-material/Dashboard";
import LogoutIcon from "@mui/icons-material/Logout";
import AccountCircleOutlinedIcon from "@mui/icons-material/AccountCircleOutlined";

const NavItemButton = styled(ListItemButton)(({ theme }) => ({
  minHeight: 48,
  paddingInline: theme.spacing(2.5),
  marginInline: theme.spacing(1),
  marginBottom: theme.spacing(0.5),
  borderRadius: theme.spacing(2),
  "& .MuiListItemIcon-root": {
    minWidth: 0,
    marginRight: theme.spacing(2.5),
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
  const [sobreUsuarioAberto, setSobreUsuarioAberto] = useState(false);
  const [confirmarSaidaAberto, setConfirmarSaidaAberto] = useState(false);

  const estaAtivo = (rota) =>
    pathname === rota || pathname.startsWith(`${rota}/`);

  const handleConfirmarSaida = () => {
    setConfirmarSaidaAberto(false);
    onSair();
  };

  return (
    <>
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
            variant="body2"
            sx={{
              mt: 0.75,
              fontWeight: 500,
              lineHeight: 1.4,
              color: "text.secondary",
              whiteSpace: "normal",
            }}
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

          <Box
            sx={{
              px: 2.5,
              py: 1.5,
              display: "flex",
              alignItems: "center",
              justifyContent: "space-between",
              gap: 1,
            }}
          >
            <Typography
              variant="subtitle2"
              sx={{ fontWeight: "bold", minWidth: 0 }}
              noWrap
            >
              {usuarioLogado?.nome || usuarioLogado?.usuario}
            </Typography>

            <Tooltip title="Sobre o usuário">
              <IconButton
                size="small"
                color="primary"
                aria-label="Sobre o usuário"
                onClick={() => setSobreUsuarioAberto(true)}
              >
                <AccountCircleOutlinedIcon />
              </IconButton>
            </Tooltip>
          </Box>

          <NavItemButton
            onClick={() => setConfirmarSaidaAberto(true)}
            sx={{ mb: 1, color: "error.main" }}
          >
            <ListItemIcon sx={{ color: "inherit" }}>
              <LogoutIcon />
            </ListItemIcon>
            <ListItemText primary="Sair" />
          </NavItemButton>
        </Box>
      </Drawer>

      <DialogoSobreUsuario
        open={sobreUsuarioAberto}
        onClose={() => setSobreUsuarioAberto(false)}
        usuarioLogado={usuarioLogado}
      />

      <DialogoConfirmacao
        open={confirmarSaidaAberto}
        onClose={() => setConfirmarSaidaAberto(false)}
        titulo="Confirmar saída"
        idTitulo="titulo-confirmar-saida"
        idDescricao="descricao-confirmar-saida"
        confirmarTexto="Sair"
        cancelarTexto="Cancelar"
        confirmarCor="error"
        confirmarIcone={<LogoutIcon />}
        onConfirmar={handleConfirmarSaida}
      >
        <Typography id="descricao-confirmar-saida" color="text.secondary">
          Tem certeza de que deseja sair da sua conta? Será necessário fazer
          login novamente para acessar o sistema.
        </Typography>
      </DialogoConfirmacao>
    </>
  );
}
