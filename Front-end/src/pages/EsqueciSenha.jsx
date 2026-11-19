import {
  Typography,
  TextField,
  Button,
  Link,
  Alert,
  Box,
  IconButton,
  Tooltip,
} from "@mui/material";
import ContentCopyIcon from "@mui/icons-material/ContentCopy";
import CheckIcon from "@mui/icons-material/Check";
import { useState } from "react";
import { AuthCardLayout, CampoSenha } from "../components/AuthLayout";
import { API_BASE_URL } from "../services/api";

const API_RECUPERAR_SENHA_URL = `${API_BASE_URL}/auth/recuperar-senha`;

export default function EsqueciSenha({ onVoltarLogin }) {
  const [usuario, setUsuario] = useState("");
  const [chaveRecuperacao, setChaveRecuperacao] = useState("");
  const [novaSenha, setNovaSenha] = useState("");
  const [confirmarSenha, setConfirmarSenha] = useState("");
  const [tentouEnviar, setTentouEnviar] = useState(false);
  const [enviado, setEnviado] = useState(false);
  const [mensagemSucesso, setMensagemSucesso] = useState("");
  const [novaChaveGerada, setNovaChaveGerada] = useState("");
  const [copiado, setCopiado] = useState(false);
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
      !chaveRecuperacao.trim() ||
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
          chaveRecuperacao: chaveRecuperacao.trim(),
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
      setNovaChaveGerada(data?.chaveRecuperacao || "");
      setEnviado(true);
    } catch {
      setErroApi(
        `Não foi possível conectar ao servidor. Verifique se o Back-end está rodando em ${API_BASE_URL}.`,
      );
    } finally {
      setCarregando(false);
    }
  };

  const handleCopiarNovaChave = async () => {
    if (!novaChaveGerada) return;
    try {
      await navigator.clipboard.writeText(novaChaveGerada);
      setCopiado(true);
    } catch {
      setErroApi("Não foi possível copiar automaticamente. Copie a chave manualmente.");
    }
  };

  return (
    <AuthCardLayout
      subtitulo="Informe seu usuário, sua chave pessoal de recuperação e crie uma nova senha!"
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

      {enviado && novaChaveGerada && (
        <Box
          sx={{
            display: "flex",
            flexDirection: "column",
            gap: 1,
          }}
        >
          <Typography variant="caption" color="text.secondary">
            Sua nova chave de recuperação pessoal:
          </Typography>
          <Box
            sx={{
              display: "flex",
              alignItems: "center",
              justifyContent: "space-between",
              gap: 1,
              px: 2,
              py: 1.5,
              borderRadius: 2,
              border: 1,
              borderColor: "primary.main",
              bgcolor: "action.hover",
            }}
          >
            <Typography
              variant="subtitle1"
              sx={{
                fontFamily: "monospace",
                fontWeight: 700,
                letterSpacing: 1,
                wordBreak: "break-all",
              }}
            >
              {novaChaveGerada}
            </Typography>

            <Tooltip title={copiado ? "Copiado!" : "Copiar chave"}>
              <IconButton
                onClick={handleCopiarNovaChave}
                color={copiado ? "success" : "primary"}
                aria-label="Copiar nova chave de recuperação"
              >
                {copiado ? <CheckIcon /> : <ContentCopyIcon />}
              </IconButton>
            </Tooltip>
          </Box>
        </Box>
      )}

      <TextField
        label="Usuário"
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
        error={tentouEnviar && !usuario.trim()}
        helperText={
          tentouEnviar && !usuario.trim() ? "Informe o seu usuário" : ""
        }
        disabled={enviado}
      />

      <TextField
        label="Chave de recuperação"
        size="medium"
        placeholder="Ex.: DVP-XXXX-XXXX-XXXX"
        value={chaveRecuperacao}
        onChange={(e) => setChaveRecuperacao(e.target.value)}
        slotProps={{
          htmlInput: {
            autoCapitalize: "characters",
            autoCorrect: "off",
            spellCheck: false,
          },
        }}
        error={tentouEnviar && !chaveRecuperacao.trim()}
        helperText={
          tentouEnviar && !chaveRecuperacao.trim()
            ? "Informe a sua chave pessoal de recuperação"
            : "Consulte sua chave em Sobre o usuário após fazer login"
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
        sx={{ py: 1.4 }}
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
