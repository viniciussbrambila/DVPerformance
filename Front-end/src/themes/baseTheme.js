import { alpha, createTheme } from '@mui/material/styles';

export const CORES_BASE = {
  brandBlue: '#2563EB',
  brandBlueLight: '#60A5FA',
  brandBlueDark: '#1D4ED8',
  slate950: '#020617',
  slate900: '#0F172A',
  navy950: '#0B1120',
  navy900: '#0B1220',
  indigo950: '#1E1B4B',
  gray900: '#101828',
  gray700: '#344054',
  gray600: '#475467',
  slate400: '#94A3B8',
  slate100: '#F1F5F9',
  slate50: '#F8FAFC',
  sky100: '#E0F2FE',
  indigo50: '#EEF2FF',
  blue100: '#DBEAFE',
  white: '#FFFFFF',
  black: '#000000',
  switchTrackLight: '#AAB4BE',
  switchTrackDark: '#8796A5',
  switchThumbLight: '#001E3C',
  switchThumbDark: '#003892',
};

const PALETA_MARCA = {
  primary: {
    main: CORES_BASE.brandBlue,
    light: CORES_BASE.brandBlueLight,
    dark: CORES_BASE.brandBlueDark,
    contrastText: CORES_BASE.white,
  },
  secondary: {
    main: CORES_BASE.gray900,
    light: CORES_BASE.gray700,
    dark: CORES_BASE.navy900,
    contrastText: CORES_BASE.white,
  },
  success: { main: '#16A34A' },
  warning: { main: '#D97706' },
  error: { main: '#DC2626' },
};

const TIPOGRAFIA_COMPARTILHADA = {
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
};

const ANIMACAO_GRADIENTE_BASE = {
  backgroundSize: '400% 400%',
  animation: 'gradient 15s ease infinite',
  minHeight: '100vh',
};

const KEYFRAMES_GRADIENTE = {
  '0%': { backgroundPosition: '0% 50%' },
  '50%': { backgroundPosition: '100% 50%' },
  '100%': { backgroundPosition: '0% 50%' },
};

function criarEstiloVidro({ background, borderColor, boxShadow }) {
  return {
    background,
    backdropFilter: 'blur(12px)',
    WebkitBackdropFilter: 'blur(12px)',
    border: `1px solid ${borderColor}`,
    boxShadow,
  };
}

export function criarTemaApp(mode = 'dark') {
  const isDark = mode === 'dark';

  const corBaseSuperficie = isDark ? CORES_BASE.slate900 : CORES_BASE.white;
  const corTextoPrimario = isDark ? CORES_BASE.slate50 : CORES_BASE.gray900;

  const glassStyle = criarEstiloVidro({
    background: alpha(corBaseSuperficie, isDark ? 0.45 : 0.65),
    borderColor: alpha(CORES_BASE.white, isDark ? 0.08 : 0.7),
    boxShadow: `0 8px 32px 0 ${alpha(
      isDark ? CORES_BASE.black : CORES_BASE.gray900,
      isDark ? 0.37 : 0.08,
    )}`,
  });

  const gradienteBody = isDark
    ? `linear-gradient(45deg, ${CORES_BASE.slate900}, ${CORES_BASE.indigo950}, ${CORES_BASE.slate950})`
    : `linear-gradient(45deg, ${CORES_BASE.sky100}, ${CORES_BASE.indigo50}, ${CORES_BASE.blue100}, ${CORES_BASE.slate100})`;

  const fundoAutofill = isDark
    ? CORES_BASE.slate900
    : alpha(CORES_BASE.white, 0.85);

  return createTheme({
    palette: {
      mode,
      ...PALETA_MARCA,
      background: {
        default: isDark ? CORES_BASE.navy950 : CORES_BASE.slate50,
        paper: glassStyle.background,
      },
      text: {
        primary: corTextoPrimario,
        secondary: isDark ? CORES_BASE.slate400 : CORES_BASE.gray600,
      },
      divider: alpha(isDark ? CORES_BASE.white : CORES_BASE.gray900, 0.1),
    },

    shape: {
      borderRadius: 10,
    },

    typography: TIPOGRAFIA_COMPARTILHADA,

    components: {
      MuiCssBaseline: {
        styleOverrides: {
          body: {
            background: gradienteBody,
            ...ANIMACAO_GRADIENTE_BASE,
          },
          '@keyframes gradient': KEYFRAMES_GRADIENTE,
        },
      },

      MuiPaper: {
        styleOverrides: {
          root: {
            backgroundImage: 'none',
            ...glassStyle,
          },
        },
      },

      MuiDialog: {
        styleOverrides: {
          paper: ({ theme }) => ({
            ...glassStyle,
            background: isDark
              ? alpha(CORES_BASE.slate900, 0.88)
              : CORES_BASE.white,
            borderRadius: theme.shape.borderRadius * 4,
            padding: theme.spacing(1),
          }),
        },
      },

      MuiDialogTitle: {
        styleOverrides: {
          root: {
            fontWeight: 700,
            fontSize: '1.35rem',
          },
        },
      },

      MuiDialogActions: {
        styleOverrides: {
          root: ({ theme }) => ({
            padding: theme.spacing(1, 3, 2),
            gap: theme.spacing(1),
          }),
        },
      },

      MuiMenu: {
        styleOverrides: {
          paper: {
            ...glassStyle,
            background: alpha(corBaseSuperficie, isDark ? 0.92 : 0.95),
          },
        },
      },

      MuiPopover: {
        styleOverrides: {
          paper: {
            ...glassStyle,
            background: alpha(corBaseSuperficie, isDark ? 0.94 : 0.96),
          },
        },
      },

      MuiAppBar: {
        styleOverrides: {
          root: {
            ...glassStyle,
            background: alpha(corBaseSuperficie, isDark ? 0.7 : 0.75),
            ...(isDark ? {} : { color: CORES_BASE.gray900 }),
          },
        },
      },

      MuiDrawer: {
        styleOverrides: {
          paper: {
            ...glassStyle,
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
          root: ({ theme }) => ({
            borderRadius: 8,
            ...(isDark
              ? {
                  '& .MuiOutlinedInput-notchedOutline': {
                    borderColor: alpha(CORES_BASE.white, 0.15),
                  },
                  '&:hover .MuiOutlinedInput-notchedOutline': {
                    borderColor: alpha(CORES_BASE.white, 0.35),
                  },
                  '&.Mui-focused .MuiOutlinedInput-notchedOutline': {
                    borderColor: theme.palette.primary.main,
                  },
                }
              : {}),
            '& input:-webkit-autofill': {
              WebkitBoxShadow: `0 0 0 1000px ${fundoAutofill} inset !important`,
              WebkitTextFillColor: `${corTextoPrimario} !important`,
              caretColor: corTextoPrimario,
              borderRadius: 'inherit',
              transition: 'background-color 5000s ease-in-out 0s',
            },
          }),
        },
      },

      MuiTableCell: {
        styleOverrides: {
          head: ({ theme }) => ({
            fontWeight: 600,
            color: theme.palette.text.primary,
          }),
        },
      },

      MuiListSubheader: {
        styleOverrides: {
          root: ({ theme }) => ({
            fontWeight: 700,
            color: theme.palette.primary.main,
            backgroundColor: theme.palette.background.paper,
            lineHeight: '36px',
          }),
        },
      },
    },
  });
}
