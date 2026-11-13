import { useEffect, useState } from "react";
import { Alert, Box, Button, MenuItem, TextField, Typography } from "@mui/material";
import { buscarComCache, lerCache, temCache } from "../utils/cacheSessao";

const CHAVE_CACHE = "focosMusculares";
const API_URL = "http://localhost:5011/exercicios/focos-musculares";

export function useFocosMusculares() {
  const [focos, setFocos] = useState(() => lerCache(CHAVE_CACHE) || {});
  const [carregando, setCarregando] = useState(() => !temCache(CHAVE_CACHE));
  const [erro, setErro] = useState("");
  const [tentativa, setTentativa] = useState(0);

  useEffect(() => {
    let ativo = true;
    setCarregando(true);
    setErro("");

    buscarComCache(
      CHAVE_CACHE,
      async () => {
        const resposta = await fetch(API_URL);
        if (!resposta.ok) throw new Error(`HTTP ${resposta.status}`);
        const dados = await resposta.json();
        if (
          !dados ||
          Array.isArray(dados) ||
          typeof dados !== "object" ||
          !Object.values(dados).every(
            (opcoes) =>
              Array.isArray(opcoes) &&
              opcoes.every(
                (opcao) =>
                  typeof opcao?.id === "string" &&
                  typeof opcao?.nome === "string",
              ),
          )
        ) {
          throw new Error("Catálogo de focos inválido.");
        }
        return dados;
      },
      { forcar: tentativa > 0 },
    )
      .then((dados) => {
        if (ativo) setFocos(dados);
      })
      .catch(() => {
        if (ativo) {
          setErro(
            "Não foi possível carregar os focos musculares. O foco atual será mantido.",
          );
        }
      })
      .finally(() => {
        if (ativo) setCarregando(false);
      });

    return () => {
      ativo = false;
    };
  }, [tentativa]);

  return {
    focos,
    carregando,
    erro,
    recarregar: () => setTentativa((anterior) => anterior + 1),
  };
}

export function FocoMuscularLabel({ grupo, value, catalogo }) {
  if (!value) return null;
  const nome = catalogo.focos[grupo]?.find((opcao) => opcao.id === value)?.nome;
  return (
    <Typography variant="caption" color="text.secondary">
      Foco: {nome || value}
    </Typography>
  );
}

export default function FocoMuscularSelect({
  grupo,
  value = "",
  onChange,
  catalogo,
}) {
  const { focos, carregando, erro, recarregar } = catalogo;
  const opcoes = focos[grupo] || [];
  const focoForaDasOpcoes =
    value && !opcoes.some((opcao) => opcao.id === value);

  return (
    <Box>
      <TextField
        fullWidth
        select
        label="Foco muscular"
        value={value}
        onChange={onChange}
        disabled={!grupo || (carregando && opcoes.length === 0)}
        slotProps={{
          select: { displayEmpty: true },
          inputLabel: { shrink: true },
        }}
        helperText={
          !grupo
            ? "Selecione o grupo muscular primeiro."
            : carregando && opcoes.length === 0
              ? "Carregando focos musculares..."
              : "Indica a ênfase ou o padrão do exercício, sem isolar uma parte do músculo. A geração prioriza focos diferentes e limita as repetições por dia. Ao alterar nome ou grupo, o foco volta para automático."
        }
      >
        <MenuItem value="">Automático pelo nome</MenuItem>
        {focoForaDasOpcoes && (
          <MenuItem value={value}>Foco salvo: {value}</MenuItem>
        )}
        {opcoes.map((opcao) => (
          <MenuItem key={opcao.id} value={opcao.id}>
            {opcao.nome}
          </MenuItem>
        ))}
      </TextField>
      {erro && (
        <Alert
          severity="warning"
          sx={{ mt: 1 }}
          action={
            <Button
              color="inherit"
              size="small"
              onClick={recarregar}
              disabled={carregando}
            >
              Tentar novamente
            </Button>
          }
        >
          {erro}
        </Alert>
      )}
    </Box>
  );
}