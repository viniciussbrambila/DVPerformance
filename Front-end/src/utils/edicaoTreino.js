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

export function substituirItemDoPlano(dias, itemId, substituto) {
  return dias.map((dia) => ({
    ...dia,
    exercicios: dia.exercicios.map((item) => {
      if (item.id !== itemId) return item;
      const maquina = item.maquinaId != null;
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
      id: item.id,
      exercicioId: item.exercicioId ?? null,
      maquinaId: item.maquinaId ?? null,
      series: Number(item.series),
      repeticao: String(item.repeticoes).trim().replace(/\s*-\s*/, "-"),
      tempoDescanso: Number(item.descanso) + "s",
    })),
  );
}
