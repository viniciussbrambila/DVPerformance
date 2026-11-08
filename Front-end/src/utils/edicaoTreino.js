export function prepararEdicao(dias) {
  return dias.map((dia) => ({
    ...dia,
    tempoAerobico: dia.tempoAerobico ?? 0,
    exercicios: (dia.exercicios || []).map((item) => ({
      ...item,
      series: String(item.series),
      repeticoes: String(item.repeticoes),
      descanso: String(parseInt(item.descanso, 10) || 60),
    })),
  }));
}

export function formatarTempoAerobico(minutos) {
  return minutos > 0 ? `${minutos} min de aeróbico` : "";
}

export function alterarAerobicoDoPlano(dias, indiceDia, minutos) {
  return dias.map((dia, indice) =>
    indice === indiceDia ? { ...dia, tempoAerobico: minutos } : dia,
  );
}

export function removerItemDoPlano(dias, itemId) {
  return dias.map((dia) => ({
    ...dia,
    exercicios: dia.exercicios.filter((item) => item.id !== itemId),
  }));
}

export function moverItemNoPlano(dias, indiceDia, indiceItem, direcao) {
  const dia = dias[indiceDia];
  const destino = indiceItem + direcao;
  if (
    !dia ||
    !Number.isInteger(indiceItem) ||
    (direcao !== -1 && direcao !== 1) ||
    indiceItem < 0 ||
    indiceItem >= dia.exercicios.length ||
    destino < 0 ||
    destino >= dia.exercicios.length
  ) {
    return dias;
  }

  const exercicios = [...dia.exercicios];
  const [item] = exercicios.splice(indiceItem, 1);
  exercicios.splice(destino, 0, item);
  return dias.map((atual, indice) =>
    indice === indiceDia ? { ...atual, exercicios } : atual,
  );
}

export function restaurarOrdemDoPlano(dias, indiceDia, diasOriginais) {
  const dia = dias[indiceDia];
  const original = diasOriginais[indiceDia];
  if (!dia || !original) return dias;

  const posicoes = new Map(
    (original.exercicios || []).map((item, indice) => [item.id, indice]),
  );
  const exercicios = [...dia.exercicios].sort(
    (a, b) =>
      (posicoes.get(a.id) ?? Infinity) - (posicoes.get(b.id) ?? Infinity),
  );
  if (exercicios.every((item, indice) => item === dia.exercicios[indice])) {
    return dias;
  }

  return dias.map((atual, indice) =>
    indice === indiceDia ? { ...atual, exercicios } : atual,
  );
}

export function filtrarCatalogoDoDia(catalogo, dia, tipo) {
  const campo = tipo === "maquina" ? "maquinaId" : "exercicioId";
  const idsUtilizados = new Set(
    (dia?.exercicios || []).map((item) => item[campo]).filter((id) => id != null),
  );
  return catalogo.filter((item) => !idsUtilizados.has(item.id));
}

export function adicionarItemAoPlano(dias, indiceDia, selecionado, tipo) {
  const dia = dias[indiceDia];
  if (!dia || filtrarCatalogoDoDia([selecionado], dia, tipo).length === 0) {
    return dias;
  }

  const id = Math.min(0, ...dias.flatMap((dia) => dia.exercicios.map((item) => item.id))) - 1;
  const maquina = tipo === "maquina";

  return dias.map((dia, indice) =>
    indice !== indiceDia
      ? dia
      : {
          ...dia,
          exercicios: [
            ...dia.exercicios,
            {
              id,
              nome: selecionado.nome,
              grupo: selecionado.grupo,
              tipo: maquina ? "Máquina" : "Exercício",
              exercicioId: maquina ? null : selecionado.id,
              maquinaId: maquina ? selecionado.id : null,
              series: "3",
              repeticoes: "10-12",
              descanso: "60",
            },
          ],
        },
  );
}

export function substituirItemDoPlano(dias, itemId, substituto, tipo) {
  const dia = dias.find((dia) => dia.exercicios.some((item) => item.id === itemId));
  const itemAtual = dia?.exercicios.find((item) => item.id === itemId);
  if (!itemAtual) return dias;

  const maquina = tipo ? tipo === "maquina" : itemAtual.maquinaId != null;
  if (filtrarCatalogoDoDia([substituto], dia, maquina ? "maquina" : "exercicio").length === 0) {
    return dias;
  }

  return dias.map((dia) => ({
    ...dia,
    exercicios: dia.exercicios.map((item) => {
      if (item.id !== itemId) return item;
      return {
        ...item,
        nome: substituto.nome,
        grupo: substituto.grupo,
        tipo: maquina ? "Máquina" : "Exercício",
        exercicioId: maquina ? null : substituto.id,
        maquinaId: maquina ? substituto.id : null,
      };
    }),
  }));
}

export function montarItensEdicao(dias) {
  return dias.flatMap((dia) =>
    dia.exercicios.map((item) => ({
      id: item.id > 0 ? item.id : 0,
      divisaoTreino: dia.nome,
      tempoAerobico: dia.tempoAerobico ?? 0,
      exercicioId: item.exercicioId ?? null,
      maquinaId: item.maquinaId ?? null,
      series: Number(item.series),
      repeticao: String(item.repeticoes).trim().replace(/\s*-\s*/, "-"),
      tempoDescanso: Number(item.descanso) + "s",
    })),
  );
}
