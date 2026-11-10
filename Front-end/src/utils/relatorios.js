export function obterUsuarioRelatorio() {
  try {
    return JSON.parse(localStorage.getItem("usuarioLogado") || "null");
  } catch {
    return null;
  }
}

export function prepararAlunosRelatorio(lista, usuarioId) {
  return lista.filter(
    (aluno) =>
      aluno.ativo !== false &&
      (aluno.usuarioId == null ||
        String(aluno.usuarioId) === String(usuarioId)),
  );
}

export function obterDataCadastro(aluno) {
  const valor = aluno.dataCadastro || aluno.dataCriacao;
  if (!valor) return null;
  const data = /^\d{4}-\d{2}-\d{2}$/.test(valor)
    ? new Date(`${valor}T12:00:00`)
    : new Date(valor);
  return Number.isNaN(data.getTime()) ? null : data;
}

export function formatarDataCadastro(aluno) {
  return (
    obterDataCadastro(aluno)?.toLocaleDateString("pt-BR") || "Não informada"
  );
}

export function formatarMedidaRelatorio(valor, unidade) {
  if (valor == null || valor === "") return "Não informado";
  const minDig = unidade === "m" ? 2 : 0;
  return `${Number(valor).toLocaleString("pt-BR", {
    minimumFractionDigits: minDig,
    maximumFractionDigits: 2,
  })} ${unidade}`;
}

function normalizarDataFiltro(valor, fimDoDia = false) {
  if (!valor) return null;
  const base =
    valor instanceof Date
      ? valor
      : /^\d{4}-\d{2}-\d{2}$/.test(String(valor))
        ? new Date(`${valor}T12:00:00`)
        : new Date(valor);
  if (Number.isNaN(base.getTime())) return null;
  return new Date(
    base.getFullYear(),
    base.getMonth(),
    base.getDate(),
    fimDoDia ? 23 : 0,
    fimDoDia ? 59 : 0,
    fimDoDia ? 59 : 0,
    fimDoDia ? 999 : 0,
  );
}

export function filtrarAlunosCadastrados(
  alunos,
  { busca = "", dia, mes, ano, dataInicio, dataFim, ordem = "recentes" },
) {
  const inicio = normalizarDataFiltro(dataInicio, false);
  const fim = normalizarDataFiltro(dataFim, true);

  return alunos
    .filter((aluno) => {
      const data = obterDataCadastro(aluno);
      const dentroDoIntervalo =
        (!inicio || (data && data.getTime() >= inicio.getTime())) &&
        (!fim || (data && data.getTime() <= fim.getTime()));

      return (
        aluno.nome.toLowerCase().includes(busca.trim().toLowerCase()) &&
        dentroDoIntervalo &&
        (!dia || data?.getDate() === Number(dia)) &&
        (!mes || (data && data.getMonth() + 1 === Number(mes))) &&
        (!ano || data?.getFullYear() === Number(ano))
      );
    })
    .sort((a, b) => {
      const dataA = obterDataCadastro(a);
      const dataB = obterDataCadastro(b);
      if (!dataA && dataB) return 1;
      if (dataA && !dataB) return -1;
      const diferenca = (dataA?.getTime() || 0) - (dataB?.getTime() || 0);
      return (
        (ordem === "antigos" ? diferenca : -diferenca) ||
        a.nome.localeCompare(b.nome, "pt-BR", { sensitivity: "base" })
      );
    });
}

export function exportarPdfRelatorio(titulo) {
  const tituloAnterior = document.title;
  try {
    document.title = titulo;
    window.print();
  } finally {
    document.title = tituloAnterior;
  }
}
