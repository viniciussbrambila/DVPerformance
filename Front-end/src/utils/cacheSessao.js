// Cache leve em memória de sessão para manter os dados já carregados da API
// entre trocas de tela (padrão Stale-While-Revalidate, sem regras de negócio).

const cache = new Map();
const requisicoesEmAndamento = new Map();

const TTL_PADRAO_MS = 60_000;

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
  for (const [chave, entrada] of cache.entries()) {
    if (chave.startsWith(prefixo)) {
      entrada.atualizadoEm = 0;
    }
  }
}

export function atualizarItemEmListasCache(prefixo, idItem, parcialOuFn) {
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
  cache.delete(chave);
}

export function limparCacheSessao() {
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

  if (requisicoesEmAndamento.has(chave)) {
    return requisicoesEmAndamento.get(chave);
  }

  const promessa = (async () => {
    try {
      const dados = await buscarDaApi();
      salvarCache(chave, dados);
      return dados;
    } finally {
      requisicoesEmAndamento.delete(chave);
    }
  })();

  requisicoesEmAndamento.set(chave, promessa);
  return promessa;
}

export function preCarregarSessao(usuarioId) {
  if (!usuarioId) return;

  const base = "http://localhost:5011";

  const buscarJson = async (url) => {
    const resp = await fetch(url);
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
