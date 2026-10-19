import {
  Box,
  Paper,
  Typography,
  TextField,
  Button,
  Link,
  Alert,
  IconButton,
  InputAdornment,
} from "@mui/material";
import { useState } from "react";
import Visibility from "@mui/icons-material/Visibility";
import VisibilityOff from "@mui/icons-material/VisibilityOff";

export default function EsqueciSenha({ glassStyle, onEnviar, onVoltarLogin }) {
  const [usuario, setUsuario] = useState("");
  const [novaSenha, setNovaSenha] = useState("");
  const [confirmarSenha, setConfirmarSenha] = useState("");
  const [showNova, setShowNova] = useState(false);
  const [showConfirmar, setShowConfirmar] = useState(false);
  const [tentouEnviar, setTentouEnviar] = useState(false);
  const [enviado, setEnviado] = useState(false);

  const senhaCurta = novaSenha.length < 6;
  const senhasDiferentes = novaSenha !== confirmarSenha;

  const handleSubmit = (e) => {
    e.preventDefault();
    setTentouEnviar(true);

    if (!usuario.trim() || senhaCurta || senhasDiferentes) {
      return;
    }

    // Quando tiver backend: verificar aqui se o usuário existe
    // antes de salvar a nova senha.
    onEnviar?.({ usuario, novaSenha });
    setEnviado(true);
  };

  const campoSenhaProps = (visivel, setVisivel) => ({
    input: {
      endAdornment: (
        <InputAdornment position="end">
          <IconButton
            onClick={() => setVisivel(!visivel)}
            edge="end"
            aria-label="Mostrar ou ocultar senha"
          >
            {visivel ? <VisibilityOff /> : <Visibility />}
          </IconButton>
        </InputAdornment>
      ),
    },
  });

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
          ...glassStyle,
          p: 4,
          width: "100%",
          maxWidth: 400,
          borderRadius: 4,
          display: "flex",
          flexDirection: "column",
          alignItems: "center",
        }}
      >
        <Typography variant="h4" sx={{ fontWeight: "bold", mb: 2 }}>
          DV Performance
        </Typography>

        <Typography
          variant="body2"
          color="text.secondary"
          sx={{ mb: 3, textAlign: "center" }}
        >
          Informe seu usuário e crie uma nova senha!
        </Typography>

        <Box
          component="form"
          onSubmit={handleSubmit}
          noValidate
          sx={{
            width: "100%",
            display: "flex",
            flexDirection: "column",
            gap: 2,
          }}
        >
          {enviado && (
            <Alert severity="success">
              Senha alterada com sucesso! Você já pode entrar com a nova senha.
            </Alert>
          )}

          <TextField
            label="Usuário"
            value={usuario}
            onChange={(e) => setUsuario(e.target.value)}
            error={tentouEnviar && !usuario.trim()}
            helperText={
              tentouEnviar && !usuario.trim() ? "Informe o seu usuário" : ""
            }
            disabled={enviado}
          />

          <TextField
            label="Nova senha"
            type={showNova ? "text" : "password"}
            value={novaSenha}
            onChange={(e) => setNovaSenha(e.target.value)}
            error={tentouEnviar && senhaCurta}
            helperText={
              tentouEnviar && senhaCurta
                ? "A senha deve ter ao menos 6 caracteres"
                : ""
            }
            disabled={enviado}
            slotProps={campoSenhaProps(showNova, setShowNova)}
          />

          <TextField
            label="Confirmar senha"
            type={showConfirmar ? "text" : "password"}
            value={confirmarSenha}
            onChange={(e) => setConfirmarSenha(e.target.value)}
            error={tentouEnviar && senhasDiferentes}
            helperText={
              tentouEnviar && senhasDiferentes ? "As senhas não coincidem" : ""
            }
            disabled={enviado}
            slotProps={campoSenhaProps(showConfirmar, setShowConfirmar)}
          />

          <Button
            type="submit"
            variant="contained"
            fullWidth
            size="large"
            disabled={enviado}
          >
            Salvar
          </Button>

          <Typography variant="body2" sx={{ textAlign: "center" }}>
            Lembrou a senha?{" "}
            <Link
              component="button"
              type="button"
              underline="hover"
              onClick={onVoltarLogin}
              sx={{ verticalAlign: "baseline" }}
            >
              Voltar ao login
            </Link>
          </Typography>
        </Box>
      </Paper>
    </Box>
  );
}
