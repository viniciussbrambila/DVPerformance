import { useEffect, useState } from "react";
import {
  Alert,
  Box,
  Button,
  ButtonBase,
  CircularProgress,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  Typography,
} from "@mui/material";
import AccountTreeOutlinedIcon from "@mui/icons-material/AccountTreeOutlined";

const API_URL = "http://localhost:5011/treinos/divisoes";

export default function EscolhaDivisaoTreino({
  open,
  aluno,
  gerando,
  erros,
  onEscolher,
  onFechar,
}) {
  const [opcoes, setOpcoes] = useState([]);
  const [carregando, setCarregando] = useState(false);
  const [erroCarregamento, setErroCarregamento] = useState("");
  const [tentativa, setTentativa] = useState(0);
  const [selecionada, setSelecionada] = useState(null);
  const alunoId = aluno?.id;
  const frequencia = aluno?.frequenciaSemanal;

  useEffect(() => {
    if (!open || !alunoId) return;
    const controller = new AbortController();

    const carregar = async () => {
      setCarregando(true);
      setErroCarregamento("");
      setOpcoes([]);
      setSelecionada(null);
      try {
        const resposta = await fetch(API_URL + "?dias=" + frequencia, {
          signal: controller.signal,
        });
        const dados = await resposta.json().catch(() => null);
        if (!resposta.ok || !Array.isArray(dados) || dados.length === 0) {
          throw new Error(
            dados?.mensagem || "Não foi possível carregar as divisões de treino.",
          );
        }
        if (!controller.signal.aborted) setOpcoes(dados);
      } catch (erro) {
        if (!controller.signal.aborted) {
          setErroCarregamento(
            erro instanceof TypeError
              ? "Não foi possível conectar ao servidor. Tente novamente."
              : erro.message,
          );
        }
      } finally {
        if (!controller.signal.aborted) setCarregando(false);
      }
    };

    carregar();
    return () => controller.abort();
  }, [open, alunoId, frequencia, tentativa]);

  return (
    <Dialog
      open={open}
      onClose={gerando ? undefined : onFechar}
      disableEscapeKeyDown={gerando}
      maxWidth={opcoes.length > 2 ? "md" : "sm"}
      fullWidth
      aria-labelledby="escolher-divisao-titulo"
      slotProps={{ paper: { sx: { borderRadius: 4, p: 1 } } }}
    >
      <DialogTitle
        id="escolher-divisao-titulo"
        sx={{ fontWeight: "bold", fontSize: "1.35rem", textAlign: "center" }}
      >
        Qual divisão de treino deseja utilizar?
      </DialogTitle>
      <DialogContent>
        <Typography variant="body2" color="text.secondary" sx={{ textAlign: "center", mb: 2 }}>
          {aluno?.nome} · {frequencia} dias por semana
        </Typography>

        {erros.length > 0 && (
          <Alert severity="error" sx={{ mb: 2 }}>
            {erros.map((erro, indice) => <div key={indice}>{erro}</div>)}
          </Alert>
        )}

        {carregando ? (
          <Box role="status" aria-label="Carregando divisões" sx={{ textAlign: "center", py: 5 }}>
            <CircularProgress size={32} />
          </Box>
        ) : erroCarregamento ? (
          <Box sx={{ textAlign: "center", py: 2 }}>
            <Alert severity="error" sx={{ mb: 2 }}>{erroCarregamento}</Alert>
            <Button onClick={() => setTentativa((valor) => valor + 1)}>
              Tentar novamente
            </Button>
          </Box>
        ) : (
          <Box
            sx={{
              display: "grid",
              gridTemplateColumns: {
                xs: "1fr",
                sm: opcoes.length === 1 ? "1fr" : "repeat(2, minmax(0, 1fr))",
                md: opcoes.length > 2 ? "repeat(3, minmax(0, 1fr))" : undefined,
              },
              gap: 2,
              mt: 1,
            }}
          >
            {opcoes.map((divisao) => (
              <ButtonBase
                key={divisao.id}
                aria-label={"Gerar treino com " + divisao.nome}
                disabled={gerando}
                onClick={() => {
                  setSelecionada(divisao.id);
                  onEscolher(divisao.id);
                }}
                sx={{
                  display: "flex",
                  flexDirection: "column",
                  justifyContent: "flex-start",
                  gap: 1,
                  p: 3,
                  width: "100%",
                  border: "1px solid",
                  borderColor: gerando && selecionada === divisao.id ? "primary.main" : "divider",
                  borderRadius: 3,
                  color: "text.primary",
                  bgcolor: "action.hover",
                  "&:hover": { borderColor: "primary.main" },
                  "&.Mui-disabled": { opacity: 0.65 },
                  "&.Mui-focusVisible": {
                    outline: "2px solid",
                    outlineColor: "primary.main",
                    outlineOffset: 2,
                  },
                }}
              >
                {gerando && selecionada === divisao.id ? (
                  <CircularProgress size={32} />
                ) : (
                  <AccountTreeOutlinedIcon sx={{ fontSize: 32, color: "primary.main" }} />
                )}
                <Typography component="span" variant="h6" sx={{ fontWeight: 700 }}>
                  {divisao.nome}
                </Typography>
                <Typography component="span" variant="body2" color="text.secondary">
                  {divisao.descricao}
                </Typography>
                <Box component="span" sx={{ display: "grid", gap: 0.75, mt: 1, textAlign: "left", width: "100%" }}>
                  {divisao.sessoes.map((sessao) => (
                    <Typography component="span" key={sessao.titulo} variant="caption" color="text.secondary">
                      <Box component="span" sx={{ fontWeight: 700 }}>
                        {sessao.titulo.replace(/^Treino /, "")}:
                      </Box>{" "}
                      {(sessao.gruposExibicao || sessao.grupos).join(", ")}
                    </Typography>
                  ))}
                </Box>
              </ButtonBase>
            ))}
          </Box>
        )}

        {gerando && (
          <Typography role="status" variant="body2" sx={{ mt: 2, textAlign: "center" }}>
            Gerando a ficha com a divisão escolhida...
          </Typography>
        )}
      </DialogContent>
      <DialogActions sx={{ px: 3, pb: 2, pt: 1, gap: 1 }}>
        <Button color="inherit" onClick={onFechar} disabled={gerando}>
          Cancelar
        </Button>
      </DialogActions>
    </Dialog>
  );
}
