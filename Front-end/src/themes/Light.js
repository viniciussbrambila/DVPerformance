import { createTheme } from '@mui/material/styles';

// Estilo do Vidro Claro (Light Liquid Glass)
export const lightGlassStyle = {
  background: 'rgba(255, 255, 255, 0.65)', // Branco translúcido
  backdropFilter: 'blur(12px)',
  WebkitBackdropFilter: 'blur(12px)',
  border: '1px solid rgba(255, 255, 255, 0.7)', // Borda branca brilhante para efeito de vidro
  boxShadow: '0 8px 32px 0 rgba(16, 24, 40, 0.08)', // Sombra suave e clara
};

const LightTheme = createTheme({
  palette: {
    mode: 'light',
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
      default: '#F8FAFC',
      paper: 'rgba(255, 255, 255, 0.65)',
    },
    text: {
      primary: '#101828', // Texto escuro para leitura perfeita no fundo claro
      secondary: '#475467',
    },
    divider: 'rgba(16, 24, 40, 0.1)', // Divisórias escuras bem sutis
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
    // 1. Fundo animado em tons pastéis/azul-claro para destacar o vidro branco
    MuiCssBaseline: {
      styleOverrides: {
        body: {
          background: 'linear-gradient(45deg, #e0f2fe, #eef2ff, #dbeafe, #f1f5f9)',
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

    // 2. Todo Paper recebe o vidro claro automaticamente
    MuiPaper: {
      styleOverrides: {
        root: {
          backgroundImage: 'none',
          ...lightGlassStyle,
        },
      },
    },

    // 3. Modais e Menus com branco sólido para não ficar acinzentado sobre o fundo escuro do modal
    MuiDialog: {
      styleOverrides: {
        paper: {
          ...lightGlassStyle,
          background: '#FFFFFF',
        },
      },
    },
    MuiMenu: {
      styleOverrides: {
        paper: {
          ...lightGlassStyle,
          background: 'rgba(255, 255, 255, 0.95)',
        },
      },
    },

    // 4. Barra Superior e Sidebar com vidro claro e texto escuro
    MuiAppBar: {
      styleOverrides: {
        root: {
          ...lightGlassStyle,
          background: 'rgba(255, 255, 255, 0.75)',
          color: '#101828', // Garante que o título e os ícones do topo fiquem visíveis no fundo claro
        },
      },
    },
    MuiDrawer: {
      styleOverrides: {
        paper: {
          ...lightGlassStyle,
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

export default LightTheme;