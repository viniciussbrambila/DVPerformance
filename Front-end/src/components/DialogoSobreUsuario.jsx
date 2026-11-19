import { useState } from "react";
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Box,
  Typography,
  Button,
  IconButton,
  Alert,
  Divider,
  Tabs,
  Tab,
  Tooltip,
} from "@mui/material";
import ContentCopyIcon from "@mui/icons-material/ContentCopy";
import CheckIcon from "@mui/icons-material/Check";
import RefreshIcon from "@mui/icons-material/Refresh";
import AccountCircleOutlinedIcon from "@mui/icons-material/AccountCircleOutlined";
import { CampoSenha } from "./AuthLayout";
import { API_BASE_URL, fetchAutenticado } from "../services/api";

const API_CHAVE_RECUPERACAO_URL = `${API_BASE_URL}/auth/chave-recuperacao`;
const API_REGENERAR_CHAVE_URL = `${API_BASE_URL}/auth/chave-recuperacao/regenerar`;
const API_ALTERAR_SENHA_URL = `${API_BASE_URL}/auth/alterar-senha`;

export default function DialogoSobreUsuario({ open, onClose, usuarioLogado }) {
  const [abaAtiva, setAbaAtiva] = useState(0);

  // Estado da aba "Sobre o usuário" (Chave de Recuperação)
  const [senhaConfirmacao, setSenhaConfirmacao] = useState("");
  const [tentouRevelar, setTentouRevelar] = useState(false);
  const [chaveRecuperacao, setChaveRecuperacao] = useState("");
  const [copiado, setCopiado] = useState(false);
  const [erroChave, setErroChave] = useState("");
  const [mensagemChave, setMensagemChave] = useState("");
  const [carregandoChave, setCarregandoChave] = useState(false);

  // Estado da aba "Alterar senha"
  const [senhaAtual, setSenhaAtual] = useState("");
  const [novaSenha, setNovaSenha] = useState("");
  const [confirmarNovaSenha, setConfirmarNovaSenha] = useState("");
  const [tentouAlterar, setTentouAlterar] = useState(false);
  const [erroSenha, setErroSenha] = useState("");
  const [sucessoSenha, setSucessoSenha] = useState("");
  const [carregandoSenha, setCarregandoSenha] = useState(false);

  const resetarEstados = () => {
    setAbaAtiva(0);
    setSenhaConfirmacao("");
    setTentouRevelar(false);
    setChaveRecuperacao("");
    setCopiado(false);
    setErroChave("");
    setMensagemChave("");
    setCarregandoChave(false);

    setSenhaAtual("");
    setNovaSenha("");
    setConfirmarNovaSenha("");
    setTentouAlterar(false);
    setErroSenha("");
    setSucessoSenha("");
    setCarregandoSenha(false);
  };

  const handleFechar = () => {
    if (carregandoChave || carregandoSenha) return;
    resetarEstados();
    onClose();
  };

  const handleRevelarChave = async (e) => {
    e.preventDefault();
    setTentouRevelar(true);
    setErroChave("");
    setMensagemChave("");

    if (!senhaConfirmacao.trim()) {
      return;
    }

    setCarregandoChave(true);
    try {
      const resp = await fetchAutenticado(API_CHAVE_RECUPERACAO_URL, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ senha: senhaConfirmacao }),
      });

      const data = await resp.json().catch(() => null);
      if (!resp.ok) {
        setErroChave(
          data?.erros?.[0] ||
            data?.mensagem ||
            "Não foi possível verificar a senha.",
        );
        return;
      }

      setChaveRecuperacao(data?.chaveRecuperacao || "");
      setCopiado(false);
    } catch {
      setErroChave("Não foi possível conectar ao servidor.");
    } finally {
      setCarregandoChave(false);
    }
  };

  const handleRegenerarChave = async () => {
    setErroChave("");
    setMensagemChave("");
    setCarregandoChave(true);

    try {
      const resp = await fetchAutenticado(API_REGENERAR_CHAVE_URL, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ senha: senhaConfirmacao }),
      });

      const data = await resp.json().catch(() => null);
      if (!resp.ok) {
        setErroChave(
          data?.erros?.[0] ||
            data?.mensagem ||
            "Não foi possível gerar uma nova chave.",
        );
        return;
      }

      setChaveRecuperacao(data?.chaveRecuperacao || "");
      setMensagemChave(
        data?.mensagem || "Nova chave de recuperação gerada com sucesso.",
      );
      setCopiado(false);
    } catch {
      setErroChave("Não foi possível conectar ao servidor.");
    } finally {
      setCarregandoChave(false);
    }
  };

  const handleCopiarChave = async () => {
    if (!chaveRecuperacao) return;
    try {
      await navigator.clipboard.writeText(chaveRecuperacao);
      setCopiado(true);
    } catch {
      setErroChave("Não foi possível copiar automaticamente. Copie o código manualmente.");
    }
  };

  const novaSenhaCurta = novaSenha.length < 6;
  const novaSenhaIgualAtual =
    Boolean(senhaAtual) && Boolean(novaSenha) && novaSenha === senhaAtual;
  const senhasDiferentes = novaSenha !== confirmarNovaSenha;

  const handleAlterarSenha = async (e) => {
    e.preventDefault();
    setTentouAlterar(true);
    setErroSenha("");
    setSucessoSenha("");

    if (
      !senhaAtual.trim() ||
      novaSenhaCurta ||
      novaSenhaIgualAtual ||
      senhasDiferentes
    ) {
      return;
    }

    setCarregandoSenha(true);
    try {
      const resp = await fetchAutenticado(API_ALTERAR_SENHA_URL, {
        method: "PUT",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
          senhaAtual,
          novaSenha,
        }),
      });

      const data = await resp.json().catch(() => null);
      if (!resp.ok) {
        setErroSenha(
          data?.erros?.[0] ||
            data?.mensagem ||
            "Não foi possível alterar a senha.",
        );
        return;
      }

      setSucessoSenha(data?.mensagem || "Senha alterada com sucesso!");
      setSenhaAtual("");
      setNovaSenha("");
      setConfirmarNovaSenha("");
      setTentouAlterar(false);
    } catch {
      setErroSenha("Não foi possível conectar ao servidor.");
    } finally {
      setCarregandoSenha(false);
    }
  };

  return (
    <Dialog
      open={open}
      onClose={handleFechar}
      aria-labelledby="titulo-sobre-usuario"
      maxWidth="xs"
      fullWidth
    >
      <DialogTitle
        id="titulo-sobre-usuario"
        sx={{ display: "flex", alignItems: "center", gap: 1, pb: 1 }}
      >
        <AccountCircleOutlinedIcon color="primary" />
        Sobre o usuário
      </DialogTitle>

      <DialogContent sx={{ pt: 0 }}>
        <Box
          sx={{
            px: 2,
            py: 1.5,
            mb: 2,
            borderRadius: 2,
            bgcolor: "action.hover",
          }}
        >
          <Typography variant="caption" color="text.secondary">
            Usuário conectado
          </Typography>
          <Typography variant="subtitle1" sx={{ fontWeight: 700 }} noWrap>
            {usuarioLogado?.nome || usuarioLogado?.usuario}
          </Typography>
        </Box>

        <Tabs
          value={abaAtiva}
          onChange={(_, novoValor) => setAbaAtiva(novoValor)}
          variant="fullWidth"
          sx={{ mb: 2, borderBottom: 1, borderColor: "divider" }}
        >
          <Tab label="Chave de Recuperação" />
          <Tab label="Alterar Senha" />
        </Tabs>

        {abaAtiva === 0 && (
          <Box>
            <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
              Esta chave pessoal (hash) permite recuperar sua senha na tela de
              login. Digite sua senha de cadastro para visualizá-la.
            </Typography>

            {erroChave && (
              <Alert severity="error" sx={{ mb: 2 }}>
                {erroChave}
              </Alert>
            )}

            {mensagemChave && (
              <Alert severity="success" sx={{ mb: 2 }}>
                {mensagemChave}
              </Alert>
            )}

            {!chaveRecuperacao ? (
              <Box
                component="form"
                onSubmit={handleRevelarChave}
                noValidate
                sx={{ display: "flex", flexDirection: "column", gap: 2 }}
              >
                <CampoSenha
                  label="Senha de cadastro"
                  size="small"
                  fullWidth
                  autoComplete="current-password"
                  value={senhaConfirmacao}
                  onChange={(e) => setSenhaConfirmacao(e.target.value)}
                  error={tentouRevelar && !senhaConfirmacao.trim()}
                  helperText={
                    tentouRevelar && !senhaConfirmacao.trim()
                      ? "Informe a sua senha para revelar a chave"
                      : ""
                  }
                  disabled={carregandoChave}
                />

                <Button
                  type="submit"
                  variant="contained"
                  fullWidth
                  disabled={carregandoChave}
                >
                  {carregandoChave ? "Verificando..." : "Visualizar chave"}
                </Button>
              </Box>
            ) : (
              <Box sx={{ display: "flex", flexDirection: "column", gap: 2 }}>
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
                    {chaveRecuperacao}
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

                <Divider />

                <Button
                  variant="outlined"
                  startIcon={<RefreshIcon />}
                  onClick={handleRegenerarChave}
                  disabled={carregandoChave}
                  fullWidth
                >
                  {carregandoChave ? "Gerando..." : "Gerar nova chave"}
                </Button>
              </Box>
            )}
          </Box>
        )}

        {abaAtiva === 1 && (
          <Box
            component="form"
            onSubmit={handleAlterarSenha}
            noValidate
            sx={{ display: "flex", flexDirection: "column", gap: 2 }}
          >
            {erroSenha && <Alert severity="error">{erroSenha}</Alert>}
            {sucessoSenha && <Alert severity="success">{sucessoSenha}</Alert>}

            <CampoSenha
              label="Senha atual"
              size="small"
              fullWidth
              autoComplete="current-password"
              value={senhaAtual}
              onChange={(e) => setSenhaAtual(e.target.value)}
              error={tentouAlterar && !senhaAtual.trim()}
              helperText={
                tentouAlterar && !senhaAtual.trim()
                  ? "Informe a sua senha atual"
                  : ""
              }
              disabled={carregandoSenha}
            />

            <CampoSenha
              label="Nova senha"
              size="small"
              fullWidth
              autoComplete="new-password"
              value={novaSenha}
              onChange={(e) => setNovaSenha(e.target.value)}
              error={tentouAlterar && (novaSenhaCurta || novaSenhaIgualAtual)}
              helperText={
                tentouAlterar && novaSenhaCurta
                  ? "A nova senha deve ter ao menos 6 caracteres"
                  : tentouAlterar && novaSenhaIgualAtual
                    ? "A nova senha deve ser diferente da senha atual"
                    : ""
              }
              disabled={carregandoSenha}
            />

            <CampoSenha
              label="Confirmar nova senha"
              size="small"
              fullWidth
              autoComplete="new-password"
              value={confirmarNovaSenha}
              onChange={(e) => setConfirmarNovaSenha(e.target.value)}
              error={tentouAlterar && senhasDiferentes}
              helperText={
                tentouAlterar && senhasDiferentes
                  ? "As senhas não coincidem"
                  : ""
              }
              disabled={carregandoSenha}
            />

            <Button
              type="submit"
              variant="contained"
              fullWidth
              disabled={carregandoSenha}
            >
              {carregandoSenha ? "Salvando..." : "Alterar senha"}
            </Button>
          </Box>
        )}
      </DialogContent>

      <DialogActions>
        <Button
          onClick={handleFechar}
          disabled={carregandoChave || carregandoSenha}
          color="inherit"
        >
          Fechar
        </Button>
      </DialogActions>
    </Dialog>
  );
}
