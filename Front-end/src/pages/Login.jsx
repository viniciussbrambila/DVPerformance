import {
  Box,
  Paper,
  Typography,
  TextField,
  Button,
  Link,
  IconButton,
  InputAdornment,
} from "@mui/material";
import { useState } from "react";
import Visibility from "@mui/icons-material/Visibility";
import VisibilityOff from "@mui/icons-material/VisibilityOff";

export default function Login({
  glassStyle,
  onLogin,
  onIrCadastro,
  onIrEsqueciSenha,
}) {
  const [showPassword, setShowPassword] = useState(false);
  const [usuario, setUsuario] = useState("");
  const [senha, setSenha] = useState("");
  const [tentouEntrar, setTentouEntrar] = useState(false);
  const handleSubmit = (e) => {
    e.preventDefault(); // Impede o navegador de recarregar a página ao enviar o formulário

    // Se usuário ou senha estiverem vazios, ativa os avisos vermelhos e para aqui
    if (!usuario.trim() || !senha.trim()) {
      setTentouEntrar(true);
      return;
    }

    // Se ambos estiverem preenchidos, avisa o App.jsx que o login foi feito!
    onLogin();
  };
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

        <Typography variant="body2" color="text.secondary" sx={{ mb: 3 }}>
          Insira suas credenciais para acessar o sistema
        </Typography>

        <Box
          component="form"
          onSubmit={handleSubmit}
          sx={{
            width: "100%",
            display: "flex",
            flexDirection: "column",
            gap: 2,
          }}
        >
          <TextField
            label="Usuário"
            type="text"
            size="Normal"
            value={usuario}
            onChange={(e) => setUsuario(e.target.value)}
            error={tentouEntrar && !usuario.trim()}
            helperText={
              tentouEntrar && !usuario.trim() ? "Informe o seu usuário" : ""
            }
          />

          <TextField
            label="Senha"
            size="Normal"
            type={showPassword ? "text" : "password"}
            value={senha}
            onChange={(e) => setSenha(e.target.value)}
            error={tentouEntrar && !senha.trim()}
            helperText={
              tentouEntrar && !senha.trim() ? "Informe a sua senha" : ""
            }
            slotProps={{
              input: {
                endAdornment: (
                  <InputAdornment position="end">
                    <IconButton
                      onClick={() => setShowPassword(!showPassword)}
                      edge="end"
                      aria-label="Mostrar ou ocultar senha"
                    >
                      {showPassword ? <VisibilityOff /> : <Visibility />}
                    </IconButton>
                  </InputAdornment>
                ),
              },
            }}
          />

          <Link
            component="button"
            type="button"
            underline="hover"
            onClick={onIrEsqueciSenha}
            sx={{ alignSelf: "flex-end", fontSize: 14 }}
          >
            Esqueci minha senha
          </Link>

          <Button type="submit" variant="contained" fullWidth size="large">
            Entrar
          </Button>
          <Typography variant="body2" sx={{ textAlign: "center" }}>
            Não tem conta?{" "}
            <Link
              component="button"
              type="button"
              underline="hover"
              onClick={onIrCadastro}
              sx={{ verticalAlign: "baseline" }}
            >
              Criar conta
            </Link>
          </Typography>
        </Box>
      </Paper>
    </Box>
  );
}
