import { createTheme } from '@mui/material/styles';
// Exportamos a constante caso você queira usar em algum elemento específico no futuro
export const darkGlassStyle = {
  background: 'rgba(15, 23, 42, 0.45)',
  backdropFilter: 'blur(12px)',
  WebkitBackdropFilter: 'blur(12px)',
  border: '1px solid rgba(255, 255, 255, 0.08)',
  boxShadow: '0 8px 32px 0 rgba(0, 0, 0, 0.37)',
};

const DarkTheme = createTheme({
  palette: {
    mode: 'dark',
    primary: {
      main: '#2563EB',
      light: '#60A5FA',
      dark: '#1D4ED8',
      contrastText: '#FFFFFF',
    },
    secondary: {
      main: '#101828',
      light: '#344054',
      dark: '#0B1220',
      contrastText: '#FFFFFF',
    },
    background: {
      default: '#0B1120',
      paper: 'rgba(15, 23, 42, 0.45)',
    },
    text: {
      primary: '#F8FAFC',
      secondary: '#94A3B8',
    },
    divider: 'rgba(255, 255, 255, 0.1)',
    success: { main: '#16A34A' },
    warning: { main: '#D97706' },
    error: { main: '#DC2626' },
  },

  shape: {
    borderRadius: 10,
  },

  typography: {
    fontFamily: '"Inter", "Roboto", "Helvetica", "Arial", sans-serif',
    h1: { fontWeight: 700 },
    h2: { fontWeight: 700 },
    h3: { fontWeight: 700 },
    h4: { fontWeight: 700 },
    h5: { fontWeight: 700 },
    h6: { fontWeight: 700 },
    button: {
      textTransform: 'none',
      fontWeight: 600,
    },
  },

  components: {
    // 1. O fundo animado da página agora fica embutido no CssBaseline do tema!
    MuiCssBaseline: {
      styleOverrides: {
        body: {
          background: 'linear-gradient(45deg, #0f172a, #1e1b4b, #020617)',
          backgroundSize: '400% 400%',
          animation: 'gradient 15s ease infinite',
          minHeight: '100vh',
        },
        '@keyframes gradient': {
          '0%': { backgroundPosition: '0% 50%' },
          '50%': { backgroundPosition: '100% 50%' },
          '100%': { backgroundPosition: '0% 50%' },
        },
      },
    },

    // 2. Todo Paper (Tabelas, Cards, Containers) já recebe o Liquid Glass automaticamente
    MuiPaper: {
      styleOverrides: {
        root: {
          backgroundImage: 'none',
          ...darkGlassStyle,
        },
      },
    },

    // 3. Modais e Menus suspensos ficam um pouco mais escuros para facilitar a leitura
    MuiDialog: {
      styleOverrides: {
        paper: {
          ...darkGlassStyle,
          background: 'rgba(15, 23, 42, 0.88)',
        },
      },
    },
    MuiMenu: {
      styleOverrides: {
        paper: {
          ...darkGlassStyle,
          background: 'rgba(15, 23, 42, 0.92)',
        },
      },
    },

    // 4. Barra Superior e Sidebar também herdam do tema
    MuiAppBar: {
      styleOverrides: {
        root: {
          ...darkGlassStyle,
          background: 'rgba(15, 23, 42, 0.7)',
        },
      },
    },
    MuiDrawer: {
      styleOverrides: {
        paper: {
          ...darkGlassStyle,
          borderRight: 'none',
        },
      },
    },

    MuiButton: {
      styleOverrides: {
        root: {
          borderRadius: 8,
          paddingInline: 16,
        },
      },
    },

    MuiTextField: {
      defaultProps: {
        size: 'small',
      },
    },

    MuiOutlinedInput: {
      styleOverrides: {
        root: {
          borderRadius: 8,
        },
      },
    },
  },
});

export default DarkTheme;