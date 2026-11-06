export function prepararEdicao(dias) {
  return dias.map((dia) => ({
    ...dia,
    exercicios: (dia.exercicios || []).map((item) => ({
      ...item,
      series: String(item.series),
      repeticoes: String(item.repeticoes),
      descanso: String(parseInt(item.descanso, 10) || 60),
    })),
  }));
}

export function removerItemDoPlano(dias, itemId) {
  return dias.map((dia) => ({
    ...dia,
    exercicios: dia.exercicios.filter((item) => item.id !== itemId),
  }));
}

export function adicionarItemAoPlano(dias, indiceDia, selecionado, tipo) {
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
  return dias.map((dia) => ({
    ...dia,
    exercicios: dia.exercicios.map((item) => {
      if (item.id !== itemId) return item;
      const maquina = tipo ? tipo === "maquina" : item.maquinaId != null;
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
      exercicioId: item.exercicioId ?? null,
      maquinaId: item.maquinaId ?? null,
      series: Number(item.series),
      repeticao: String(item.repeticoes).trim().replace(/\s*-\s*/, "-"),
      tempoDescanso: Number(item.descanso) + "s",
    })),
  );
}
