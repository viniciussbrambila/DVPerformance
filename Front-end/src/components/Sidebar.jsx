import { styled } from '@mui/material/styles';
import { 
  Drawer as MuiDrawer, Toolbar, List, Divider, IconButton, 
  ListItemButton, ListItemIcon, ListItemText, ListSubheader
} from '@mui/material';
import { Link } from 'react-router-dom';
import ChevronLeftIcon from '@mui/icons-material/ChevronLeft';
import PeopleIcon from '@mui/icons-material/People';
import FitnessCenterIcon from '@mui/icons-material/FitnessCenter';
import DirectionsRunIcon from '@mui/icons-material/DirectionsRun';
import AddTaskIcon from '@mui/icons-material/AddTask';
import AssignmentIcon from '@mui/icons-material/Assignment';
import DashboardIcon from '@mui/icons-material/Dashboard';

const Drawer = styled(MuiDrawer, { shouldForwardProp: (prop) => prop !== 'open' })(
  ({ theme, open, drawerwidth, glassstyle }) => ({
    '& .MuiDrawer-paper': {
      position: 'relative',
      whiteSpace: 'nowrap',
      width: drawerwidth,
      ...glassstyle, // Aplica o vidro na barra lateral
      borderRight: 'none', // Remove a borda sólida feia
      transition: theme.transitions.create('width', {
        easing: theme.transitions.easing.sharp,
        duration: theme.transitions.duration.enteringScreen,
      }),
      boxSizing: 'border-box',
      ...(!open && {
        overflowX: 'hidden',
        transition: theme.transitions.create('width', {
          easing: theme.transitions.easing.sharp,
          duration: theme.transitions.duration.leavingScreen,
        }),
        width: theme.spacing(7),
        [theme.breakpoints.up('sm')]: {
          width: theme.spacing(9),
        },
      }),
    },
  }),
);

export default function Sidebar({ open, toggleDrawer, drawerWidth, glassStyle }) {
  return (
    <Drawer variant="permanent" open={open} drawerwidth={drawerWidth} glassstyle={glassStyle}>
      <Toolbar sx={{ display: 'flex', alignItems: 'center', justifyContent: 'flex-end', px: [1] }}>
        <IconButton onClick={toggleDrawer}>
          <ChevronLeftIcon />
        </IconButton>
      </Toolbar>
      <Divider />
      <List component="nav">
        <ListItemButton component={Link} to="/Dashboard" sx={{ minHeight: 48, justifyContent: open ? 'initial' : 'center', px: 2.5 }}>
          <ListItemIcon sx={{ minWidth: 0, mr: open ? 3 : 'auto', justifyContent: 'center' }}>
            <DashboardIcon color="" />
          </ListItemIcon>
          <ListItemText primary="Dashboard" sx={{ opacity: open ? 1 : 0 }} />
        </ListItemButton>
        <ListItemButton component={Link} to="/treinos" sx={{ minHeight: 48, justifyContent: open ? 'initial' : 'center', px: 2.5 }}>
          <ListItemIcon sx={{ minWidth: 0, mr: open ? 3 : 'auto', justifyContent: 'center' }}>
            <AddTaskIcon color="" />
          </ListItemIcon>
          <ListItemText primary="Gerar Treino" sx={{ opacity: open ? 1 : 0 }} />
        </ListItemButton>

        <ListItemButton component={Link} to="/alunos" sx={{ minHeight: 48, justifyContent: open ? 'initial' : 'center', px: 2.5 }}>
          <ListItemIcon sx={{ minWidth: 0, mr: open ? 3 : 'auto', justifyContent: 'center' }}>
            <PeopleIcon color="" />
          </ListItemIcon>
          <ListItemText primary="Alunos" sx={{ opacity: open ? 1 : 0 }} />
        </ListItemButton>

        <ListItemButton component={Link} to="/" sx={{ minHeight: 48, justifyContent: open ? 'initial' : 'center', px: 2.5 }}>
          <ListItemIcon sx={{ minWidth: 0, mr: open ? 3 : 'auto', justifyContent: 'center' }}>
          <DirectionsRunIcon color="" />
          </ListItemIcon>
          <ListItemText primary="Exercícios" sx={{ opacity: open ? 1 : 0 }} />
        </ListItemButton>

        <ListItemButton component={Link} to="/" sx={{ minHeight: 48, justifyContent: open ? 'initial' : 'center', px: 2.5 }}>
          <ListItemIcon sx={{ minWidth: 0, mr: open ? 3 : 'auto', justifyContent: 'center' }}>
           
            <FitnessCenterIcon color="" />
          </ListItemIcon>
          <ListItemText primary="Máquinas" sx={{ opacity: open ? 1 : 0 }} />
        </ListItemButton>

        <Divider sx={{ my: 1 }} />
        
        <ListSubheader component="div" inset sx={{ opacity: open ? 1 : 0, transition: 'opacity 0.3s', backgroundColor: 'transparent' }}>
          Relatórios
        </ListSubheader>

        <ListItemButton sx={{ minHeight: 48, justifyContent: open ? 'initial' : 'center', px: 2.5 }}>
          <ListItemIcon sx={{ minWidth: 0, mr: open ? 3 : 'auto', justifyContent: 'center' }}>
            <AssignmentIcon />
          </ListItemIcon>
          <ListItemText primary="Planos Exportados" sx={{ opacity: open ? 1 : 0 }} />
        </ListItemButton>
      </List>
    </Drawer>
  );
}