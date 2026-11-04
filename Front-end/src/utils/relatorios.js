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

export function filtrarAlunosCadastrados(
  alunos,
  { busca, dia, mes, ano, ordem },
) {
  return alunos
    .filter((aluno) => {
      const data = obterDataCadastro(aluno);
      return (
        aluno.nome.toLowerCase().includes(busca.trim().toLowerCase()) &&
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
