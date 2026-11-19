import { useState } from "react";
import {
  Box,
  Paper,
  Typography,
  TextField,
  InputAdornment,
  IconButton,
} from "@mui/material";
import Visibility from "@mui/icons-material/Visibility";
import VisibilityOff from "@mui/icons-material/VisibilityOff";
import Marca from "./Marca";

export function AuthCardLayout({
  subtitulo,
  subtituloCentralizado = false,
  onSubmit,
  noValidate = false,
  children,
}) {
  return (
    <Box
      sx={{
        minHeight: "100vh",
        display: "flex",
        justifyContent: "center",
        alignItems: "center",
        px: 2,
        py: 4,
      }}
    >
      <Paper
        elevation={0}
        sx={{
          p: { xs: 3.5, sm: 5 },
          width: "100%",
          maxWidth: 500,
          borderRadius: 4,
          display: "flex",
          flexDirection: "column",
          alignItems: "center",
        }}
      >
        <Marca />

        <Typography
          variant="body1"
          color="text.secondary"
          sx={{
            mb: 3.5,
            ...(subtituloCentralizado ? { textAlign: "center" } : {}),
          }}
        >
          {subtitulo}
        </Typography>

        <Box
          component="form"
          onSubmit={onSubmit}
          noValidate={noValidate}
          sx={{
            width: "100%",
            display: "flex",
            flexDirection: "column",
            gap: 2.5,
          }}
        >
          {children}
        </Box>
      </Paper>
    </Box>
  );
}

export function CampoSenha({
  label,
  value,
  onChange,
  error = false,
  helperText,
  disabled = false,
  size = "medium",
  fullWidth = false,
  autoComplete,
}) {
  const [mostrarSenha, setMostrarSenha] = useState(false);

  return (
    <TextField
      label={label}
      type={mostrarSenha ? "text" : "password"}
      value={value}
      onChange={onChange}
      error={error}
      helperText={helperText}
      disabled={disabled}
      size={size}
      fullWidth={fullWidth}
      autoComplete={autoComplete}
      slotProps={{
        input: {
          endAdornment: (
            <InputAdornment position="end">
              <IconButton
                onClick={() => setMostrarSenha((prev) => !prev)}
                edge="end"
                aria-label="Mostrar ou ocultar senha"
              >
                {mostrarSenha ? <VisibilityOff /> : <Visibility />}
              </IconButton>
            </InputAdornment>
          ),
        },
      }}
    />
  );
}
