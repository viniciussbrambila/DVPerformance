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

const API_CADASTRO_URL = `${API_BASE_URL}/auth/cadastro`;

export default function Cadastro({ onCadastro, onVoltarLogin }) {
  const [usuario, setUsuario] = useState("");
  const [senha, setSenha] = useState("");
  const [confirmarSenha, setConfirmarSenha] = useState("");
  const [tentouCadastrar, setTentouCadastrar] = useState(false);
  const [erroApi, setErroApi] = useState("");
  const [carregando, setCarregando] = useState(false);
  const [chaveGerada, setChaveGerada] = useState("");
  const [copiado, setCopiado] = useState(false);

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

      if (data?.chaveRecuperacao) {
        setChaveGerada(data.chaveRecuperacao);
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

  const handleCopiarChave = async () => {
    if (!chaveGerada) return;
    try {
      await navigator.clipboard.writeText(chaveGerada);
      setCopiado(true);
    } catch {
      setErroApi("Não foi possível copiar automaticamente. Copie a chave manualmente.");
    }
  };

  if (chaveGerada) {
    return (
      <AuthCardLayout
        subtitulo="Guarde sua chave pessoal de recuperação antes de continuar"
        subtituloCentralizado
        onSubmit={(e) => {
          e.preventDefault();
          onCadastro();
        }}
      >
        <Alert severity="success">
          Conta criada com sucesso! Use esta chave caso precise redefinir sua
          senha no futuro.
        </Alert>

        {erroApi && <Alert severity="error">{erroApi}</Alert>}

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
            {chaveGerada}
          </Typography>

          <Tooltip title={copiado ? "Copiado!" : "Copiar chave"}>
            <IconButton
              onClick={handleCopiarChave}
              color={copiado ? "success" : "primary"}
              aria-label="Copiar chave de recuperação"
            >
              {copiado ? <CheckIcon /> : <ContentCopyIcon />}
            </IconButton>
          </Tooltip>
        </Box>

        <Typography variant="body2" color="text.secondary" sx={{ textAlign: "center" }}>
          Você também pode consultar ou gerar uma nova chave depois clicando no
          ícone de usuário no menu lateral.
        </Typography>

        <Button
          type="submit"
          variant="contained"
          fullWidth
          size="large"
          sx={{ py: 1.4 }}
        >
          Ir para o login
        </Button>
      </AuthCardLayout>
    );
  }

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
        error={tentouCadastrar && senhaCurta}
        helperText={
          tentouCadastrar && senhaCurta
            ? "A senha deve ter ao menos 6 caracteres"
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
