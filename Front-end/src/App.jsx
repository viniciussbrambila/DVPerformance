import { useState } from 'react';
import { BrowserRouter as Router, Routes, Route } from 'react-router-dom'; 
import { styled, ThemeProvider } from '@mui/material/styles';
import { 
  CssBaseline, Box, Toolbar, Typography, IconButton, Badge, Container, Grid, Switch
} from '@mui/material';
import MuiAppBar from '@mui/material/AppBar';
import MenuIcon from '@mui/icons-material/Menu';
import NotificationsIcon from '@mui/icons-material/Notifications';

import Darktheme from './themes/Dark';
import Lighttheme from './themes/Light';
import Sidebar from './components/Sidebar';
import Home from './pages/Home';
import Alunos from './pages/Alunos';
import Treinos from './pages/Treinos';
import Dashboard  from './pages/Dashboard';

const drawerWidth = 240;

const AppBar = styled(MuiAppBar, {
  shouldForwardProp: (prop) => prop !== 'open',
})(({ theme, open }) => ({
  zIndex: theme.zIndex.drawer + 1,
  transition: theme.transitions.create(['width', 'margin'], {
    easing: theme.transitions.easing.sharp,
    duration: theme.transitions.duration.leavingScreen,
  }),
  ...(open && {
    marginLeft: drawerWidth,
    width: `calc(100% - ${drawerWidth}px)`,
    transition: theme.transitions.create(['width', 'margin'], {
      easing: theme.transitions.easing.sharp,
      duration: theme.transitions.duration.enteringScreen,
    }),
  }),
}));

// SWITCH PERSONALIZADO AQUI:
const MaterialUISwitch = styled(Switch)(({ theme }) => ({
  width: 62,
  height: 34,
  padding: 7,
  '& .MuiSwitch-switchBase': {
    margin: 1,
    padding: 0,
    transform: 'translateX(6px)',
    '&.Mui-checked': {
      color: '#fff',
      transform: 'translateX(22px)',
      '& .MuiSwitch-thumb:before': {
        backgroundImage: `url('data:image/svg+xml;utf8,<svg xmlns="http://www.w3.org/2000/svg" height="20" width="20" viewBox="0 0 20 20"><path fill="${encodeURIComponent(
          '#fff',
        )}" d="M4.2 2.5l-.7 1.8-1.8.7 1.8.7.7 1.8.6-1.8L6.7 5l-1.9-.7-.6-1.8zm15 8.3a6.7 6.7 0 11-6.6-6.6 5.8 5.8 0 006.6 6.6z"/></svg>')`,
      },
      '& + .MuiSwitch-track': {
        opacity: 1,
        backgroundColor: '#aab4be',
        ...theme.applyStyles('dark', {
          backgroundColor: '#8796A5',
        }),
      },
    },
  },
  '& .MuiSwitch-thumb': {
    backgroundColor: '#001e3c',
    width: 32,
    height: 32,
    '&::before': {
      content: "''",
      position: 'absolute',
      width: '100%',
      height: '100%',
      left: 0,
      top: 0,
      backgroundRepeat: 'no-repeat',
      backgroundPosition: 'center',
      backgroundImage: `url('data:image/svg+xml;utf8,<svg xmlns="http://www.w3.org/2000/svg" height="20" width="20" viewBox="0 0 20 20"><path fill="${encodeURIComponent(
        '#fff',
      )}" d="M9.305 1.667V3.75h1.389V1.667h-1.39zm-4.707 1.95l-.982.982L5.09 6.072l.982-.982-1.473-1.473zm10.802 0L13.927 5.09l.982.982 1.473-1.473-.982-.982zM10 5.139a4.872 4.872 0 00-4.862 4.86A4.872 4.872 0 0010 14.862 4.872 4.872 0 0014.86 10 4.872 4.872 0 0010 5.139zm0 1.389A3.462 3.462 0 0113.471 10a3.462 3.462 0 01-3.473 3.472A3.462 3.462 0 016.527 10 3.462 3.462 0 0110 6.528zM1.665 9.305v1.39h2.083v-1.39H1.666zm14.583 0v1.39h2.084v-1.39h-2.084zM5.09 13.928L3.616 15.4l.982.982 1.473-1.473-.982-.982zm9.82 0l-.982.982 1.473 1.473.982-.982-1.473-1.473zM9.305 16.25v2.083h1.389V16.25h-1.39z"/></svg>')`,
    },
    ...theme.applyStyles('dark', {
      backgroundColor: '#003892',
    }),
  },
  '& .MuiSwitch-track': {
    opacity: 1,
    backgroundColor: '#aab4be',
    borderRadius: 20 / 2,
    ...theme.applyStyles('dark', {
      backgroundColor: '#8796A5',
    }),
  },
}));

export default function App() {
  const [open, setOpen] = useState(true);
  const toggleDrawer = () => setOpen(!open);

  // Começa como true para o Dark Mode ser o padrão
  const [isDarkMode, setIsDarkMode] = useState(false);
  const toggleTheme = () => setIsDarkMode(!isDarkMode);

  return (
    <ThemeProvider theme={isDarkMode ? Darktheme : Lighttheme}>
      <Router>
        <CssBaseline />
        <Box sx={{ display: 'flex' }}>
          <AppBar position="absolute" open={open} elevation={0}>
            <Toolbar sx={{ pr: '24px' }}>
              <IconButton 
                edge="start" 
                color="inherit" 
                onClick={toggleDrawer} 
                sx={{ marginRight: '36px', ...(open && { display: 'none' }) }}
              >
                <MenuIcon />
              </IconButton>
              
              <Typography component="h1" variant="h6" color="inherit" noWrap sx={{ flexGrow: 1 }}>
                DV Performance
              </Typography>

              {/* AQUI ENTRAMOS COM O SWITCH NO LUGAR DO BOTÃO ANTIGO */}
              <MaterialUISwitch 
                checked={isDarkMode} 
                onChange={toggleTheme} 
                sx={{ mr: 1 }} 
              />

              <IconButton color="inherit">
                <Badge badgeContent={978} color="error">
                  <NotificationsIcon />
                </Badge>
              </IconButton>
            </Toolbar>
          </AppBar>

          <Sidebar open={open} toggleDrawer={toggleDrawer} drawerWidth={drawerWidth} />

          <Box component="main" sx={{ flexGrow: 1, height: '100vh', overflow: 'auto' }}>
            <Toolbar />
            <Container maxWidth="lg" sx={{ mt: 4, mb: 4 }}>
              <Grid container spacing={3}>
                <Grid item xs={12}>
                  <Routes>
                    <Route path="/dashboard" element={<Dashboard />} />
                    <Route path="/" element={<Home />} />
                    <Route path="/alunos" element={<Alunos />} />
                    <Route path="/treinos" element={<Treinos />} />
                  </Routes>
                </Grid>
              </Grid>
            </Container>
          </Box>
        </Box>
      </Router>
    </ThemeProvider>
  );
}