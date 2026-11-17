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
        minHeight: "80vh",
        display: "flex",
        justifyContent: "center",
        alignItems: "center",
      }}
    >
      <Paper
        elevation={0}
        sx={{
          p: 4,
          width: "100%",
          maxWidth: 400,
          borderRadius: 4,
          display: "flex",
          flexDirection: "column",
          alignItems: "center",
        }}
      >
        <Marca />

        <Typography
          variant="body2"
          color="text.secondary"
          sx={{
            mb: 3,
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
            gap: 2,
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
