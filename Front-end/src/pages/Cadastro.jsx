import {
  Box,
  Paper,
  Typography,
  TextField,
  Button,
  Link,
  IconButton,
  InputAdornment,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  FormHelperText,
} from "@mui/material";
import { useState } from "react";
import Visibility from "@mui/icons-material/Visibility";
import VisibilityOff from "@mui/icons-material/VisibilityOff";

export default function Cadastro({ glassStyle, onCadastro, onVoltarLogin }) {
  const [showPassword, setShowPassword] = useState(false);
  const [showConfirmar, setShowConfirmar] = useState(false);
  const [usuario, setUsuario] = useState("");
  const [senha, setSenha] = useState("");
  const [confirmarSenha, setConfirmarSenha] = useState("");
  const [perfil, setPerfil] = useState("");
  const [tentouCadastrar, setTentouCadastrar] = useState(false);

  const senhaCurta = senha.length < 6;
  const senhasDiferentes = senha !== confirmarSenha;

  const handleSubmit = (e) => {
    e.preventDefault();
    setTentouCadastrar(true);

    if (!usuario.trim() || senhaCurta || senhasDiferentes || !perfil) {
      return;
    }

    onCadastro({ usuario, senha, perfil });
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

        <Typography variant="body2" color="text.secondary" sx={{ mb: 3 }}>
          Preencha os dados para criar sua conta
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
          <TextField
            label="Nome de usuário"
            value={usuario}
            onChange={(e) => setUsuario(e.target.value)}
            error={tentouCadastrar && !usuario.trim()}
            helperText={
              tentouCadastrar && !usuario.trim() ? "Informe o seu usuário" : ""
            }
          />

          <TextField
            label="Senha"
            type={showPassword ? "text" : "password"}
            value={senha}
            onChange={(e) => setSenha(e.target.value)}
            error={tentouCadastrar && senhaCurta}
            helperText={
              tentouCadastrar && senhaCurta
                ? "A senha deve ter ao menos 6 caracteres"
                : ""
            }
            slotProps={campoSenhaProps(showPassword, setShowPassword)}
          />

          <TextField
            label="Confirmar senha"
            type={showConfirmar ? "text" : "password"}
            value={confirmarSenha}
            onChange={(e) => setConfirmarSenha(e.target.value)}
            error={tentouCadastrar && senhasDiferentes}
            helperText={
              tentouCadastrar && senhasDiferentes
                ? "As senhas não coincidem"
                : ""
            }
            slotProps={campoSenhaProps(showConfirmar, setShowConfirmar)}
          />

          <FormControl fullWidth error={tentouCadastrar && !perfil}>
            <InputLabel id="perfil-label">Entrar como</InputLabel>
            <Select
              labelId="perfil-label"
              id="perfil"
              value={perfil}
              label="Entrar como"
              onChange={(e) => setPerfil(e.target.value)}
            >
              <MenuItem value="admin">Admin</MenuItem>
              <MenuItem value="personal">Personal</MenuItem>
            </Select>
            {tentouCadastrar && !perfil && (
              <FormHelperText>Selecione um perfil</FormHelperText>
            )}
          </FormControl>

          <Button type="submit" variant="contained" fullWidth size="large">
            Cadastrar
          </Button>

          <Typography variant="body2" sx={{ textAlign: "center" }}>
            Já tem conta?{" "}
            <Link
              component="button"
              type="button"
              underline="hover"
              onClick={onVoltarLogin}
              sx={{ verticalAlign: "baseline" }}
            >
              Entrar
            </Link>
          </Typography>
        </Box>
      </Paper>
    </Box>
  );
}
