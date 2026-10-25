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

const API_CADASTRO_URL = "http://localhost:5011/auth/cadastro";

export default function Cadastro({ glassStyle, onCadastro, onVoltarLogin }) {
  const [showPassword, setShowPassword] = useState(false);
  const [showConfirmar, setShowConfirmar] = useState(false);
  const [usuario, setUsuario] = useState("");
  const [senha, setSenha] = useState("");
  const [confirmarSenha, setConfirmarSenha] = useState("");
  const [tentouCadastrar, setTentouCadastrar] = useState(false);
  const [erroApi, setErroApi] = useState("");
  const [carregando, setCarregando] = useState(false);

  const usuarioTrim = usuario.trim();
  const usuarioInvalido = usuarioTrim.length < 3 || usuarioTrim.length > 50;
  const senhaCurta = senha.length < 6;
  const senhasDiferentes = senha !== confirmarSenha;

  const handleSubmit = async (e) => {
    e.preventDefault();
    setTentouCadastrar(true);
    setErroApi("");

    if (usuarioInvalido || senhaCurta || senhasDiferentes) {
      return;
    }

    setCarregando(true);
    try {
      const response = await fetch(API_CADASTRO_URL, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
          usuario: usuarioTrim,
          senha,
        }),
      });

      const data = await response.json().catch(() => null);

      if (!response.ok) {
        setErroApi(
          data?.erros?.[0] ||
            data?.mensagem ||
            "Não foi possível criar a conta.",
        );
        return;
      }

      const dadosUsuario = {
        ...data,
        usuario: data?.usuario || usuarioTrim,
        nome: data?.nome || data?.usuario || usuarioTrim,
      };

      localStorage.setItem("usuarioLogado", JSON.stringify(dadosUsuario));
      onCadastro(dadosUsuario);
    } catch (erro) {
      setErroApi(
        "Não foi possível conectar ao servidor. Verifique se o Back-end está rodando em http://localhost:5011.",
      );
    } finally {
      setCarregando(false);
    }
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
          {erroApi && <Alert severity="error">{erroApi}</Alert>}

          <TextField
            label="Nome de usuário"
            value={usuario}
            onChange={(e) => setUsuario(e.target.value)}
            error={tentouCadastrar && usuarioInvalido}
            helperText={
              tentouCadastrar && !usuarioTrim
                ? "Informe o seu usuário"
                : tentouCadastrar && usuarioInvalido
                  ? "O usuário deve ter entre 3 e 50 caracteres"
                  : ""
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

          <Button
            type="submit"
            variant="contained"
            fullWidth
            size="large"
            disabled={carregando}
          >
            {carregando ? "Cadastrando..." : "Cadastrar"}
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
