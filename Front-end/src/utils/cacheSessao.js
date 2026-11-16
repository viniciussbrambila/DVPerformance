// Cache leve em memória de sessão para manter os dados já carregados da API
// entre trocas de tela (padrão Stale-While-Revalidate, sem regras de negócio).

const cache = new Map();
const requisicoesEmAndamento = new Map();
const versoesCache = new Map();

const TTL_PADRAO_MS = 60_000;

function incrementarVersao(chave) {
  versoesCache.set(chave, (versoesCache.get(chave) ?? 0) + 1);
}

function invalidarPrefixoEmAndamento(prefixo) {
  const chavesParaInvalidar = new Set();

  for (const chave of cache.keys()) {
    if (chave.startsWith(prefixo)) chavesParaInvalidar.add(chave);
  }
  for (const chave of requisicoesEmAndamento.keys()) {
    if (chave.startsWith(prefixo)) chavesParaInvalidar.add(chave);
  }
  for (const chave of versoesCache.keys()) {
    if (chave.startsWith(prefixo)) chavesParaInvalidar.add(chave);
  }

  for (const chave of chavesParaInvalidar) {
    incrementarVersao(chave);
    requisicoesEmAndamento.delete(chave);
  }

  return chavesParaInvalidar;
}

export function lerCache(chave) {
  const entrada = cache.get(chave);
  return entrada ? entrada.dados : undefined;
}

export function temCache(chave) {
  return cache.has(chave);
}

export function salvarCache(chave, dados) {
  cache.set(chave, {
    dados,
    atualizadoEm: Date.now(),
  });
  return dados;
}

export function cacheEstaFresco(chave, ttlMs = TTL_PADRAO_MS) {
  const entrada = cache.get(chave);
  if (!entrada || entrada.atualizadoEm <= 0) return false;
  return Date.now() - entrada.atualizadoEm < ttlMs;
}

export function expirarCachePorPrefixo(prefixo) {
  const chaves = invalidarPrefixoEmAndamento(prefixo);
  for (const chave of chaves) {
    const entrada = cache.get(chave);
    if (entrada) {
      entrada.atualizadoEm = 0;
    }
  }
}

export function removerCachePorPrefixo(prefixo) {
  const chaves = invalidarPrefixoEmAndamento(prefixo);
  for (const chave of chaves) {
    cache.delete(chave);
  }
}

export function atualizarItemEmListasCache(prefixo, idItem, parcialOuFn) {
  invalidarPrefixoEmAndamento(prefixo);
  for (const [chave, entrada] of cache.entries()) {
    if (chave.startsWith(prefixo) && Array.isArray(entrada.dados)) {
      entrada.dados = entrada.dados.map((item) => {
        if (String(item?.id) !== String(idItem)) return item;
        return typeof parcialOuFn === "function"
          ? parcialOuFn(item)
          : { ...item, ...parcialOuFn };
      });
      entrada.atualizadoEm = 0;
    }
  }
}

export function removerCache(chave) {
  incrementarVersao(chave);
  requisicoesEmAndamento.delete(chave);
  cache.delete(chave);
}

export function limparCacheSessao() {
  for (const chave of cache.keys()) incrementarVersao(chave);
  for (const chave of requisicoesEmAndamento.keys()) incrementarVersao(chave);
  cache.clear();
  requisicoesEmAndamento.clear();
}

export async function buscarComCache(
  chave,
  buscarDaApi,
  { forcar = false, ttlMs = TTL_PADRAO_MS } = {},
) {
  if (!forcar && cacheEstaFresco(chave, ttlMs)) {
    return lerCache(chave);
  }

  if (!forcar && requisicoesEmAndamento.has(chave)) {
    return requisicoesEmAndamento.get(chave);
  }

  if (forcar) {
    incrementarVersao(chave);
  }

  const versaoInicial = versoesCache.get(chave) ?? 0;

  const promessa = (async () => {
    try {
      const dados = await buscarDaApi();
      if ((versoesCache.get(chave) ?? 0) === versaoInicial) {
        salvarCache(chave, dados);
      }
      return dados;
    } finally {
      if (requisicoesEmAndamento.get(chave) === promessa) {
        requisicoesEmAndamento.delete(chave);
      }
    }
  })();

  requisicoesEmAndamento.set(chave, promessa);
  return promessa;
}

export function preCarregarSessao(usuarioId) {
  if (!usuarioId) return;

  const base = "http://localhost:5011";

  const buscarJson = async (url) => {
    const resp = await fetch(url, { cache: "no-store" });
    if (!resp.ok) throw new Error(`HTTP ${resp.status}`);
    return resp.json();
  };

  buscarComCache(`exercicios:${usuarioId}`, () =>
    buscarJson(`${base}/exercicios?usuarioId=${usuarioId}`),
  ).catch(() => {});

  buscarComCache(`maquinas:${usuarioId}`, () =>
    buscarJson(`${base}/maquinas?usuarioId=${usuarioId}`),
  ).catch(() => {});

  buscarComCache(`alunos:${usuarioId}`, () =>
    buscarJson(`${base}/alunos?usuarioId=${usuarioId}`),
  ).catch(() => {});

  buscarComCache(`alunosSemTreino:${usuarioId}`, () =>
    buscarJson(`${base}/alunos/sem-treino?usuarioId=${usuarioId}`),
  ).catch(() => {});

  buscarComCache(`planos:${usuarioId}`, async () => {
    const planos = await buscarJson(`${base}/treinos?usuarioId=${usuarioId}`);
    if (Array.isArray(planos)) {
      planos.forEach((p) => {
        if (p?.id != null) salvarCache(`planoDetalhe:${p.id}`, p);
      });
    }
    return planos;
  }).catch(() => {});
}
