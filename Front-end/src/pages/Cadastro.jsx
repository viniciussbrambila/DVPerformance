import {
  Typography,
  TextField,
  Button,
  Link,
  Alert,
} from "@mui/material";
import { useState } from "react";
import { AuthCardLayout, CampoSenha } from "../components/AuthLayout";
import { API_BASE_URL } from "../services/api";

const API_CADASTRO_URL = `${API_BASE_URL}/auth/cadastro`;

export default function Cadastro({ onCadastro, onVoltarLogin }) {
  const [usuario, setUsuario] = useState("");
  const [senha, setSenha] = useState("");
  const [confirmarSenha, setConfirmarSenha] = useState("");
  const [tentouCadastrar, setTentouCadastrar] = useState(false);
  const [erroApi, setErroApi] = useState("");
  const [carregando, setCarregando] = useState(false);

  const usuarioTrim = usuario.trim();
  const usuarioInvalido = usuarioTrim.length < 3 || usuarioTrim.length > 50;
  const senhaInvalida = senha.length < 6 || senha.length > 100;
  const senhasDiferentes = senha !== confirmarSenha;

  const handleSubmit = async (e) => {
    e.preventDefault();
    setTentouCadastrar(true);
    setErroApi("");

    if (usuarioInvalido || senhaInvalida || senhasDiferentes) {
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

      onCadastro();
    } catch {
      setErroApi(
        `Não foi possível conectar ao servidor. Verifique se o Back-end está rodando em ${API_BASE_URL}.`,
      );
    } finally {
      setCarregando(false);
    }
  };

  return (
    <AuthCardLayout
      subtitulo="Preencha os dados para criar sua conta"
      onSubmit={handleSubmit}
      noValidate
    >
      {erroApi && <Alert severity="error">{erroApi}</Alert>}

      <TextField
        label="Nome de usuário"
        size="medium"
        value={usuario}
        onChange={(e) => setUsuario(e.target.value)}
        autoComplete="username"
        slotProps={{
          htmlInput: {
            autoCapitalize: "none",
            autoCorrect: "off",
            spellCheck: false,
          },
        }}
        error={tentouCadastrar && usuarioInvalido}
        helperText={
          tentouCadastrar && !usuarioTrim
            ? "Informe o seu usuário"
            : tentouCadastrar && usuarioInvalido
              ? "O usuário deve ter entre 3 e 50 caracteres"
              : ""
        }
      />

      <CampoSenha
        label="Senha"
        value={senha}
        onChange={(e) => setSenha(e.target.value)}
        error={tentouCadastrar && senhaInvalida}
        helperText={
          tentouCadastrar && senhaInvalida
            ? "A senha deve ter entre 6 e 100 caracteres"
            : ""
        }
      />

      <CampoSenha
        label="Confirmar senha"
        value={confirmarSenha}
        onChange={(e) => setConfirmarSenha(e.target.value)}
        error={tentouCadastrar && senhasDiferentes}
        helperText={
          tentouCadastrar && senhasDiferentes ? "As senhas não coincidem" : ""
        }
      />

      <Button
        type="submit"
        variant="contained"
        fullWidth
        size="large"
        disabled={carregando}
        sx={{ py: 1.4 }}
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
    </AuthCardLayout>
  );
}
