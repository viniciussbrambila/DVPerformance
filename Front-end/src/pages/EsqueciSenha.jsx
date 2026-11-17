import { Typography, TextField, Button, Link, Alert } from "@mui/material";
import { useState } from "react";
import { AuthCardLayout, CampoSenha } from "../components/AuthLayout";
import { API_BASE_URL } from "../services/api";

const API_RECUPERAR_SENHA_URL = `${API_BASE_URL}/auth/recuperar-senha`;

export default function EsqueciSenha({ onVoltarLogin }) {
  const [usuario, setUsuario] = useState("");
  const [codigoRecuperacao, setCodigoRecuperacao] = useState("");
  const [novaSenha, setNovaSenha] = useState("");
  const [confirmarSenha, setConfirmarSenha] = useState("");
  const [tentouEnviar, setTentouEnviar] = useState(false);
  const [enviado, setEnviado] = useState(false);
  const [mensagemSucesso, setMensagemSucesso] = useState("");
  const [erroApi, setErroApi] = useState("");
  const [carregando, setCarregando] = useState(false);

  const senhaCurta = novaSenha.length < 6;
  const senhasDiferentes = novaSenha !== confirmarSenha;

  const handleSubmit = async (e) => {
    e.preventDefault();
    setTentouEnviar(true);
    setErroApi("");

    if (
      !usuario.trim() ||
      !codigoRecuperacao.trim() ||
      senhaCurta ||
      senhasDiferentes
    ) {
      return;
    }

    setCarregando(true);
    try {
      const response = await fetch(API_RECUPERAR_SENHA_URL, {
        method: "PUT",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
          usuario: usuario.trim(),
          codigoRecuperacao: codigoRecuperacao.trim(),
          novaSenha,
        }),
      });

      const data = await response.json().catch(() => null);

      if (!response.ok) {
        setErroApi(
          data?.erros?.[0] ||
            data?.mensagem ||
            "Não foi possível redefinir a senha.",
        );
        return;
      }

      setMensagemSucesso(
        data?.mensagem ||
          "Senha alterada com sucesso! Você já pode entrar com a nova senha.",
      );
      setEnviado(true);
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
      subtitulo="Informe seu usuário, o código de segurança da academia e crie uma nova senha!"
      subtituloCentralizado
      onSubmit={handleSubmit}
      noValidate
    >
      {erroApi && <Alert severity="error">{erroApi}</Alert>}

      {enviado && (
        <Alert severity="success">
          {mensagemSucesso ||
            "Senha alterada com sucesso! Você já pode entrar com a nova senha."}
        </Alert>
      )}

      <TextField
        label="Usuário"
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
        error={tentouEnviar && !usuario.trim()}
        helperText={
          tentouEnviar && !usuario.trim() ? "Informe o seu usuário" : ""
        }
        disabled={enviado}
      />

      <TextField
        label="Código de recuperação"
        placeholder="Ex.: DVP-2026"
        value={codigoRecuperacao}
        onChange={(e) => setCodigoRecuperacao(e.target.value)}
        slotProps={{
          htmlInput: {
            autoCapitalize: "characters",
            autoCorrect: "off",
            spellCheck: false,
          },
        }}
        error={tentouEnviar && !codigoRecuperacao.trim()}
        helperText={
          tentouEnviar && !codigoRecuperacao.trim()
            ? "Informe o código de recuperação (padrão acadêmico: DVP-2026)"
            : "Código de validação da academia (padrão: DVP-2026)"
        }
        disabled={enviado}
      />

      <CampoSenha
        label="Nova senha"
        value={novaSenha}
        onChange={(e) => setNovaSenha(e.target.value)}
        error={tentouEnviar && senhaCurta}
        helperText={
          tentouEnviar && senhaCurta
            ? "A senha deve ter ao menos 6 caracteres"
            : ""
        }
        disabled={enviado}
      />

      <CampoSenha
        label="Confirmar senha"
        value={confirmarSenha}
        onChange={(e) => setConfirmarSenha(e.target.value)}
        error={tentouEnviar && senhasDiferentes}
        helperText={
          tentouEnviar && senhasDiferentes ? "As senhas não coincidem" : ""
        }
        disabled={enviado}
      />

      <Button
        type="submit"
        variant="contained"
        fullWidth
        size="large"
        disabled={enviado || carregando}
      >
        {carregando ? "Salvando..." : "Salvar"}
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
    </AuthCardLayout>
  );
}
