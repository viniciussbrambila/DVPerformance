import { Typography, TextField, Button, Link, Alert } from "@mui/material";
import { useState } from "react";
import { AuthCardLayout, CampoSenha } from "../components/AuthLayout";
import { API_BASE_URL } from "../services/api";

const API_LOGIN_URL = `${API_BASE_URL}/auth/login`;

export default function Login({ onLogin, onIrCadastro }) {
  const [usuario, setUsuario] = useState("");
  const [senha, setSenha] = useState("");
  const [tentouEntrar, setTentouEntrar] = useState(false);
  const [erroApi, setErroApi] = useState("");
  const [carregando, setCarregando] = useState(false);

  const handleSubmit = async (e) => {
    e.preventDefault(); // Impede o navegador de recarregar a página ao enviar o formulário
    setErroApi("");

    // Se usuário ou senha estiverem vazios, ativa os avisos vermelhos e para aqui
    if (!usuario.trim() || !senha.trim()) {
      setTentouEntrar(true);
      return;
    }

    setCarregando(true);
    try {
      const response = await fetch(API_LOGIN_URL, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
          usuario: usuario.trim(),
          senha,
        }),
      });

      const data = await response.json().catch(() => null);

      if (!response.ok) {
        setErroApi(
          data?.erros?.[0] ||
            data?.mensagem ||
            "Usuário ou senha incorretos.",
        );
        return;
      }

      const dadosUsuario = {
        ...data,
        usuario: data?.usuario || usuario.trim(),
        nome: data?.nome || data?.usuario || usuario.trim(),
      };

      onLogin(dadosUsuario);
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
      subtitulo="Insira suas credenciais para acessar o sistema"
      onSubmit={handleSubmit}
    >
      {erroApi && <Alert severity="error">{erroApi}</Alert>}

      <TextField
        label="Usuário"
        type="text"
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
        error={tentouEntrar && !usuario.trim()}
        helperText={
          tentouEntrar && !usuario.trim() ? "Informe o seu usuário" : ""
        }
      />

      <CampoSenha
        label="Senha"
        value={senha}
        onChange={(e) => setSenha(e.target.value)}
        error={tentouEntrar && !senha.trim()}
        helperText={tentouEntrar && !senha.trim() ? "Informe a sua senha" : ""}
      />

      <Button
        type="submit"
        variant="contained"
        fullWidth
        size="large"
        disabled={carregando}
        sx={{ py: 1.4 }}
      >
        {carregando ? "Entrando..." : "Entrar"}
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
    </AuthCardLayout>
  );
}
