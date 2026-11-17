import { useEffect, useState } from "react";
import { Alert, Box, Button, Chip, MenuItem, TextField, Typography } from "@mui/material";
import { buscarComCache, lerCache, temCache } from "../utils/cacheSessao";
import { normalizarTextoBusca } from "../utils/busca";
import { fetchAutenticado } from "../services/api";

const CHAVE_CACHE = "focosMusculares";
const API_URL = "http://localhost:5011/exercicios/focos-musculares";

export const OPCOES_NIVEL_MINIMO = [
  {
    valor: "Iniciante",
    rotulo: "Iniciante (Liberado para Iniciante, Intermediário e Avançado)",
    resumo: "Iniciante (Todos)",
    cor: "success",
  },
  {
    valor: "Intermediário",
    rotulo: "Intermediário (Apenas Intermediário e Avançado)",
    resumo: "Intermediário+",
    cor: "warning",
  },
  {
    valor: "Avançado",
    rotulo: "Avançado (Exclusivo para Avançado)",
    resumo: "Avançado",
    cor: "error",
  },
];

export const NIVEL_EXPERIENCIA_ORDEM = {
  Iniciante: 1,
  "Intermediário": 2,
  "Avançado": 3,
};

export function normalizarNivelExperiencia(nivel) {
  const texto = normalizarTextoBusca(nivel).trim();
  if (texto === "iniciante") return "Iniciante";
  if (texto === "intermediario") return "Intermediário";
  if (texto === "avancado") return "Avançado";
  return "";
}

export function nivelPermitidoParaAluno(nivelItem, experienciaAluno) {
  const nivelAlunoNormalizado = normalizarNivelExperiencia(experienciaAluno);
  if (!nivelAlunoNormalizado) return true;
  const nivelItemNormalizado =
    normalizarNivelExperiencia(nivelItem) || "Iniciante";
  return (
    NIVEL_EXPERIENCIA_ORDEM[nivelItemNormalizado] <=
    NIVEL_EXPERIENCIA_ORDEM[nivelAlunoNormalizado]
  );
}

export function obterNiveisPermitidos(experienciaAluno) {
  return OPCOES_NIVEL_MINIMO.map((opcao) => opcao.valor).filter((nivel) =>
    nivelPermitidoParaAluno(nivel, experienciaAluno),
  );
}

const contemQualquer = (texto, termos) =>
  termos.some((termo) => texto.includes(termo));

export function sugerirFocoMuscular(grupo, nome) {
  const g = normalizarTextoBusca(grupo).trim();
  const n = normalizarTextoBusca(nome).trim();
  if (!g || !n) return "";

  switch (g) {
    case "peito":
      if (n.includes("inclinad")) return "superior";
      if (contemQualquer(n, ["declinad", "paralela", "mergulho", "polia alta"]))
        return "inferior";
      if (contemQualquer(n, ["reto", "peck deck", "voador", "crucifixo maquina"]))
        return "esternocostal";
      return "";
    case "costas":
      if (contemQualquer(n, ["puxada", "pulldown", "barra fixa", "gravitation"]))
        return "puxada_vertical";
      if (contemQualquer(n, ["remada", "serrote", "cavalinho"]))
        return "remada_horizontal";
      return "";
    case "quadriceps":
      if (n.includes("extensora")) return "extensao_joelho";
      if (contemQualquer(n, ["afundo", "passada", "bulgaro"]))
        return "unilateral";
      if (contemQualquer(n, ["agachamento", "leg press", "hack"]))
        return "agachamento_pressao";
      return "";
    case "posterior de coxa":
      if (contemQualquer(n, ["stiff", "terra", "bom dia", "good morning"]))
        return "extensao_quadril";
      if (contemQualquer(n, ["flexora", "nordica"])) return "flexao_joelho";
      return "";
    case "gluteos":
      if (contemQualquer(n, ["elevacao pelvica", "ponte de gluteo", "hip thrust"]))
        return "elevacao_pelvica";
      if (contemQualquer(n, ["coice", "extensao de quadril", "gluteo maquina"]))
        return "extensao_quadril";
      if (contemQualquer(n, ["bulgaro", "step-up", "step up", "subida no banco"]))
        return "unilateral";
      if (n.includes("sumo")) return "agachamento_sumo";
      return "";
    case "ombros":
      if (contemQualquer(n, ["posterior", "inverso", "face pull"]))
        return "posterior";
      if (n.includes("lateral")) return "lateral";
      if (n.includes("frontal")) return "frontal";
      if (contemQualquer(n, ["desenvolvimento", "arnold"]))
        return "desenvolvimento";
      return "";
    case "trapezio":
      if (n.includes("remada alta")) return "remada_alta";
      if (n.includes("encolhimento")) return "encolhimento";
      return "";
    case "adutores / abdutores":
      if (contemQualquer(n, ["adutor", "adutora", "aducao"])) return "aducao";
      if (contemQualquer(n, ["abdutor", "abdutora", "abducao"]))
        return "abducao";
      return "";
    case "biceps":
      if (contemQualquer(n, ["martelo", "inversa"])) return "neutro_pronado";
      if (n.includes("inclinad")) return "ombro_estendido";
      if (contemQualquer(n, ["scott", "concentrada", "aranha"]))
        return "ombro_flexionado";
      if (n.includes("rosca")) return "supinado";
      return "";
    case "triceps":
      if (contemQualquer(n, ["frances", "testa"])) return "braco_elevado";
      if (contemQualquer(n, ["coice", "banco", "paralela", "mergulho"]))
        return "braco_atras";
      if (contemQualquer(n, ["pulley", "corda", "barra", "supino fechado", "maquina"]))
        return "braco_ao_lado";
      return "";
    case "antebraco":
      if (n.includes("inversa")) return "extensao_punho";
      if (contemQualquer(n, ["punho", "pronacao", "supinacao"]))
        return "flexao_punho";
      if (n.includes("fazendeiro")) return "pegada";
      return "";
    case "panturrilha":
      if (n.includes("sentado")) return "joelho_flexionado";
      if (contemQualquer(n, ["em pe", "leg press", "hack", "smith", "burrinho"]))
        return "joelho_estendido";
      return "";
    case "abdomen":
      if (contemQualquer(n, ["obliquo", "bicicleta", "lenhador", "lateral"]))
        return "obliquo_rotacao";
      if (contemQualquer(n, ["prancha", "roda abdominal", "ab wheel"]))
        return "estabilizacao";
      if (n.includes("infra")) return "flexao_inferior";
      if (n.includes("abdominal")) return "flexao_tronco";
      return "";
    case "lombar":
      if (contemQualquer(n, ["superman", "bird dog", "perdigueiro", "ponte"]))
        return "estabilizacao";
      if (contemQualquer(n, ["hiperextensao", "extensao lombar", "banco romano", "bom dia", "good morning", "maquina"]))
        return "extensao_lombar";
      return "";
    default:
      return "";
  }
}

export function sugerirNivelMinimo(nome) {
  const n = normalizarTextoBusca(nome).trim();
  if (!n) return "Iniciante";

  if (
    contemQualquer(n, [
      "flexao nordica",
      "roda abdominal",
      "ab wheel",
      "bom dia",
      "good morning",
      "agachamento frontal",
      "barra fixa",
      "puxada na barra fixa",
      "levantamento terra",
    ])
  ) {
    return "Avançado";
  }

  if (
    contemQualquer(n, [
      "agachamento livre",
      "agachamento bulgaro",
      "agachamento sumo com barra",
      "passada com",
      "afundo com barra",
      "stiff com barra",
      "remada curvada",
      "remada cavalinho",
      "supino declinado com barra",
      "desenvolvimento militar",
      "desenvolvimento arnold",
      "paralelas",
      "mergulho nas paralelas",
      "triceps testa com barra",
      "rosca aranha",
      "elevacao pelvica com barra",
      "abdominal infra na barra",
      "caminhada do fazendeiro",
      "rolo de punho",
      "wrist roller",
    ])
  ) {
    return "Intermediário";
  }

  return "Iniciante";
}

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
        const resposta = await fetchAutenticado(API_URL);
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

export function FocoMuscularLabel({ grupo, value, nivelMinimo, catalogo }) {
  const nomeFoco = value
    ? catalogo.focos[grupo]?.find((opcao) => opcao.id === value)?.nome || value
    : null;
  const infoNivel =
    OPCOES_NIVEL_MINIMO.find((n) => n.valor === nivelMinimo) ||
    OPCOES_NIVEL_MINIMO[0];

  return (
    <Box sx={{ display: "flex", flexWrap: "wrap", alignItems: "center", gap: 0.75, mt: 0.25 }}>
      {nomeFoco && (
        <Typography variant="caption" color="text.secondary">
          Foco: {nomeFoco}
        </Typography>
      )}
      <Chip
        label={`Nível: ${infoNivel.resumo}`}
        size="small"
        color={infoNivel.cor}
        variant="outlined"
        sx={{ height: 20, fontSize: "0.7rem", fontWeight: 600 }}
      />
    </Box>
  );
}

export default function FocoMuscularSelect({
  grupo,
  value = "",
  onChange,
  catalogo,
  error = false,
  helperText = "",
  sugeridoAutomaticamente = false,
}) {
  const { focos, carregando, erro, recarregar } = catalogo;
  const opcoes = (focos[grupo] || []).filter((opcao) => opcao.id !== "geral");
  const focoForaDasOpcoes =
    value && value !== "geral" && !opcoes.some((opcao) => opcao.id === value);

  const textoAjuda =
    helperText ||
    (!grupo
      ? "Selecione o grupo muscular primeiro para liberar os padrões de movimento."
      : carregando && opcoes.length === 0
        ? "Carregando padrões de movimento..."
        : sugeridoAutomaticamente && value
          ? "Padrão de movimento sugerido automaticamente pelo nome (você pode alterá-lo se desejar)."
          : "Obrigatório: escolha o padrão biomecânico do movimento para que ele encaixe na ordem e nas vagas corretas do treino.");

  return (
    <Box>
      <TextField
        fullWidth
        select
        label="Foco Muscular / Padrão de Movimento"
        value={value === "geral" ? "" : value}
        onChange={onChange}
        error={error}
        disabled={!grupo || (carregando && opcoes.length === 0)}
        slotProps={{
          select: { displayEmpty: true },
          inputLabel: { shrink: true },
        }}
        helperText={textoAjuda}
      >
        <MenuItem value="" disabled>
          {grupo
            ? "Selecione o padrão biomecânico do movimento..."
            : "Selecione o grupo muscular primeiro"}
        </MenuItem>
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
