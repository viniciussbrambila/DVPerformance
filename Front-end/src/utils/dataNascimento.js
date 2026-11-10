const IDADE_MINIMA = 13;
const IDADE_MAXIMA = 120;

function diasNoMes(ano, mes) {
  if (mes === 2) {
    return ano % 4 === 0 && (ano % 100 !== 0 || ano % 400 === 0) ? 29 : 28;
  }
  return [4, 6, 9, 11].includes(mes) ? 30 : 31;
}

function lerData(valor) {
  if (typeof valor !== "string" || !/^\d{4}-\d{2}-\d{2}$/.test(valor)) {
    return null;
  }
  const [ano, mes, dia] = valor.split("-").map(Number);
  if (
    ano < 1 ||
    mes < 1 ||
    mes > 12 ||
    dia < 1 ||
    dia > diasNoMes(ano, mes)
  ) {
    return null;
  }
  return { ano, mes, dia };
}

function lerReferencia(referencia) {
  if (!(referencia instanceof Date) || Number.isNaN(referencia.getTime())) {
    return null;
  }
  return {
    ano: referencia.getFullYear(),
    mes: referencia.getMonth() + 1,
    dia: referencia.getDate(),
  };
}

function paraISO({ ano, mes, dia }) {
  return [
    String(ano).padStart(4, "0"),
    String(mes).padStart(2, "0"),
    String(dia).padStart(2, "0"),
  ].join("-");
}

export function calcularIdade(valor, referencia = new Date()) {
  const nascimento = lerData(valor);
  const hoje = lerReferencia(referencia);
  if (!nascimento || !hoje) return null;

  let idade = hoje.ano - nascimento.ano;
  // Em anos comuns, quem nasceu em 29/02 faz aniversário em 01/03.
  if (
    hoje.mes < nascimento.mes ||
    (hoje.mes === nascimento.mes && hoje.dia < nascimento.dia)
  ) {
    idade -= 1;
  }
  return idade < 0 ? null : idade;
}

export function dataNascimentoValida(valor, referencia = new Date()) {
  const idade = calcularIdade(valor, referencia);
  return idade !== null && idade >= IDADE_MINIMA && idade <= IDADE_MAXIMA;
}

export function converterDataNascimentoParaISO(valor) {
  if (typeof valor !== "string" || !/^\d{2}\/\d{2}\/\d{4}$/.test(valor)) {
    return "";
  }
  const [dia, mes, ano] = valor.split("/");
  const dataISO = [ano, mes, dia].join("-");
  return lerData(dataISO) ? dataISO : "";
}

export function formatarDataNascimento(valor) {
  const nascimento = lerData(valor);
  if (!nascimento) return "Não informada";
  const { ano, mes, dia } = nascimento;
  return [
    String(dia).padStart(2, "0"),
    String(mes).padStart(2, "0"),
    String(ano).padStart(4, "0"),
  ].join("/");
}

export function limitesDataNascimento(referencia = new Date()) {
  const hoje = lerReferencia(referencia);
  if (!hoje) return { min: "", max: "" };

  const deslocarAnos = (anos) => {
    const ano = hoje.ano - anos;
    return {
      ano,
      mes: hoje.mes,
      dia: Math.min(hoje.dia, diasNoMes(ano, hoje.mes)),
    };
  };

  // Inclui todo o período em que a pessoa ainda tem 120 anos.
  const min = deslocarAnos(IDADE_MAXIMA + 1);
  min.dia += 1;
  if (min.dia > diasNoMes(min.ano, min.mes)) {
    min.dia = 1;
    min.mes += 1;
    if (min.mes > 12) {
      min.mes = 1;
      min.ano += 1;
    }
  }

  return {
    min: paraISO(min),
    max: paraISO(deslocarAnos(IDADE_MINIMA)),
  };
}
