export const API_BASE_URL = "http://localhost:5011";

export function lerUsuarioSessao() {
  try {
    if (typeof localStorage === "undefined") return null;
    const salvo = localStorage.getItem("usuarioLogado");
    return salvo ? JSON.parse(salvo) : null;
  } catch {
    return null;
  }
}

export function obterHeadersAutenticados(headersExtras = {}) {
  const usuario = lerUsuarioSessao();
  const headers = { ...headersExtras };

  if (usuario?.token && typeof usuario.token === "string") {
    headers.Authorization = `Bearer ${usuario.token}`;
  }

  return headers;
}

export function fetchAutenticado(url, opcoes = {}) {
  const headers = obterHeadersAutenticados(opcoes.headers || {});
  return fetch(url, {
    ...opcoes,
    headers,
  });
}
