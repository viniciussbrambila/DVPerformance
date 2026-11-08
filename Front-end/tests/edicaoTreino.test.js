import test from "node:test";
import assert from "node:assert/strict";
import {
  prepararEdicao,
  filtrarCatalogoDoDia,
  formatarTempoAerobico,
  alterarAerobicoDoPlano,
  adicionarItemAoPlano,
  removerItemDoPlano,
  moverItemNoPlano,
  restaurarOrdemDoPlano,
  substituirItemDoPlano,
  montarItensEdicao,
} from "../src/utils/edicaoTreino.js";

function plano() {
  return [
    { nome: "A", exercicios: [
      { id: 1, exercicioId: 10, maquinaId: null, nome: "Agachamento", grupo: "Quadríceps", series: 3, repeticoes: "8-12", descanso: "60s" },
      { id: 2, exercicioId: null, maquinaId: 20, nome: "Leg press", grupo: "Quadríceps", series: 4, repeticoes: "10", descanso: "90s" },
    ] },
    { nome: "B", exercicios: [
      { id: 3, exercicioId: 10, maquinaId: null, nome: "Agachamento", grupo: "Quadríceps", series: 3, repeticoes: "8-12", descanso: "60s" },
    ] },
  ];
}

test("editar e cancelar preserva o plano original", () => {
  const original = plano();
  const copia = structuredClone(original);
  const rascunho = prepararEdicao(original);
  rascunho[0].exercicios[0].series = "5";
  assert.deepEqual(original, copia);
});

test("substituir exercicio preserva a prescricao e altera apenas a ocorrencia escolhida", () => {
  const original = prepararEdicao(plano());
  const dias = substituirItemDoPlano(original, 1, { id: 11, nome: "Avanço", grupo: "Glúteos" });
  assert.equal(dias[0].exercicios[0].exercicioId, 11);
  assert.equal(dias[0].exercicios[0].maquinaId, null);
  assert.equal(dias[0].exercicios[0].nome, "Avanço");
  assert.equal(dias[0].exercicios[0].grupo, "Glúteos");
  assert.equal(dias[0].exercicios[0].series, "3");
  assert.equal(dias[0].exercicios[0].repeticoes, "8-12");
  assert.equal(dias[1].exercicios[0].exercicioId, 10);
  assert.equal(original[0].exercicios[0].exercicioId, 10);
});

test("substituir maquina mantem vinculo exclusivo com o catalogo de maquinas", () => {
  const dias = substituirItemDoPlano(prepararEdicao(plano()), 2, { id: 21, nome: "Hack", grupo: "Quadríceps" });
  assert.equal(dias[0].exercicios[1].maquinaId, 21);
  assert.equal(dias[0].exercicios[1].exercicioId, null);
  assert.equal(dias[0].exercicios[1].tipo, "Máquina");
  assert.equal(dias[0].exercicios[1].descanso, "90");
});

test("remover exercicio ou maquina afeta somente o item do plano, sem apagar ocorrencias em outros dias", () => {
  const original = prepararEdicao(plano());
  const dias = removerItemDoPlano(removerItemDoPlano(original, 1), 2);
  assert.equal(dias[0].exercicios.length, 0);
  assert.equal(dias[1].exercicios.length, 1);
  assert.equal(original[0].exercicios.length, 2);
});

test("salvar envia substituicoes, prescricao e apenas os itens mantidos", () => {
  let dias = prepararEdicao(plano());
  dias = substituirItemDoPlano(dias, 2, { id: 21, nome: "Hack", grupo: "Quadríceps" });
  dias = removerItemDoPlano(dias, 1);
  dias[0].exercicios[0].repeticoes = " 8 - 12 ";
  assert.deepEqual(montarItensEdicao(dias), [
    { id: 2, divisaoTreino: "A", tempoAerobico: 0, exercicioId: null, maquinaId: 21, series: 4, repeticao: "8-12", tempoDescanso: "90s" },
    { id: 3, divisaoTreino: "B", tempoAerobico: 0, exercicioId: 10, maquinaId: null, series: 3, repeticao: "8-12", tempoDescanso: "60s" },
  ]);
});

test("substituir entre catalogos limpa o vinculo anterior e preserva a prescricao", () => {
  const original = prepararEdicao(plano());
  let dias = substituirItemDoPlano(original, 1, { id: 10, nome: "Hack", grupo: "Quadríceps" }, "maquina");
  dias = substituirItemDoPlano(dias, 2, { id: 20, nome: "Avanço", grupo: "Glúteos" }, "exercicio");
  assert.equal(dias[0].exercicios[0].exercicioId, null);
  assert.equal(dias[0].exercicios[0].maquinaId, 10);
  assert.equal(dias[0].exercicios[0].tipo, "Máquina");
  assert.equal(dias[0].exercicios[0].repeticoes, "8-12");
  assert.equal(dias[0].exercicios[1].exercicioId, 20);
  assert.equal(dias[0].exercicios[1].maquinaId, null);
  assert.equal(dias[0].exercicios[1].tipo, "Exercício");
  assert.equal(dias[0].exercicios[1].series, "4");
  assert.equal(dias[0].exercicios[1].descanso, "90");
  assert.equal(original[0].exercicios[0].exercicioId, 10);
});

test("adicionar em dias diferentes cria ids temporarios unicos e envia o dia correto", () => {
  const original = prepararEdicao(plano());
  let dias = adicionarItemAoPlano(original, 0, { id: 30, nome: "Supino", grupo: "Peito" }, "exercicio");
  dias = adicionarItemAoPlano(dias, 1, { id: 30, nome: "Cadeira extensora", grupo: "Quadríceps" }, "maquina");
  const primeiro = dias[0].exercicios.at(-1);
  const segundo = dias[1].exercicios.at(-1);
  assert.equal(dias[0].exercicios.length, 3);
  assert.equal(dias[1].exercicios.length, 2);
  assert.ok(primeiro.id < 0 && segundo.id < 0);
  assert.notEqual(primeiro.id, segundo.id);
  assert.equal(primeiro.exercicioId, 30);
  assert.equal(primeiro.maquinaId, null);
  assert.equal(segundo.exercicioId, null);
  assert.equal(segundo.maquinaId, 30);
  assert.equal(primeiro.series, "3");
  assert.equal(primeiro.repeticoes, "10-12");
  assert.equal(primeiro.descanso, "60");
  assert.equal(original[0].exercicios.length, 2);
  const enviados = montarItensEdicao(dias).filter((item) => item.id === 0);
  assert.equal(enviados.length, 2);
  assert.deepEqual(enviados.map((item) => item.divisaoTreino), ["A", "B"]);
});

test("substituir e excluir itens novos afeta somente a ocorrencia escolhida", () => {
  let dias = prepararEdicao(plano());
  dias = adicionarItemAoPlano(dias, 0, { id: 30, nome: "Supino", grupo: "Peito" }, "exercicio");
  const itemNovoId = dias[0].exercicios.at(-1).id;
  dias = adicionarItemAoPlano(dias, 1, { id: 30, nome: "Supino", grupo: "Peito" }, "exercicio");
  const outroNovoId = dias[1].exercicios.at(-1).id;
  dias = substituirItemDoPlano(dias, itemNovoId, { id: 21, nome: "Hack", grupo: "Quadríceps" }, "maquina");
  assert.equal(dias[0].exercicios.at(-1).maquinaId, 21);
  assert.equal(dias[1].exercicios.at(-1).exercicioId, 30);
  dias = removerItemDoPlano(dias, itemNovoId);
  assert.equal(dias[0].exercicios.length, 2);
  assert.equal(dias[1].exercicios.at(-1).id, outroNovoId);
});

test("adicionar em dia esvaziado preserva o dia e envia o novo item ao final", () => {
  let dias = removerItemDoPlano(prepararEdicao(plano()), 3);
  dias = adicionarItemAoPlano(dias, 1, { id: 22, nome: "Remada", grupo: "Costas" }, "maquina");
  const enviados = montarItensEdicao(dias);
  assert.equal(dias[1].nome, "B");
  assert.equal(dias[1].exercicios.length, 1);
  assert.equal(enviados.at(-1).id, 0);
  assert.equal(enviados.at(-1).divisaoTreino, "B");
  assert.equal(enviados.at(-1).maquinaId, 22);
});

test("aerobico exibe apenas minutos e o texto padronizado", () => {
  assert.equal(formatarTempoAerobico(20), "20 min de aeróbico");
  assert.equal(formatarTempoAerobico(15), "15 min de aeróbico");
  assert.equal(formatarTempoAerobico(0), "");
});

test("editar, remover e readicionar aerobico preserva outros dias e o plano original", () => {
  const original = plano();
  original[0].tempoAerobico = 20;
  original[1].tempoAerobico = 15;
  let dias = alterarAerobicoDoPlano(prepararEdicao(original), 0, 35);
  assert.equal(dias[0].tempoAerobico, 35);
  assert.equal(dias[1].tempoAerobico, 15);
  dias = alterarAerobicoDoPlano(dias, 0, 0);
  assert.equal(dias[0].tempoAerobico, 0);
  dias = alterarAerobicoDoPlano(dias, 0, 25);
  assert.equal(dias[0].tempoAerobico, 25);
  assert.equal(original[0].tempoAerobico, 20);
});

test("salvar propaga o aerobico para todos os itens do dia inclusive novos", () => {
  let dias = alterarAerobicoDoPlano(prepararEdicao(plano()), 0, 30);
  dias = alterarAerobicoDoPlano(dias, 1, 15);
  dias = adicionarItemAoPlano(dias, 0, { id: 30, nome: "Supino", grupo: "Peito" }, "exercicio");
  const enviados = montarItensEdicao(dias);
  assert.deepEqual(enviados.filter((item) => item.divisaoTreino === "A").map((item) => item.tempoAerobico), [30, 30, 30]);
  assert.equal(enviados.at(-1).tempoAerobico, 15);
});

test("remover aerobico envia zero e excluir o primeiro exercicio nao perde o tempo do dia", () => {
  let dias = alterarAerobicoDoPlano(prepararEdicao(plano()), 0, 25);
  dias = alterarAerobicoDoPlano(dias, 1, 0);
  dias = removerItemDoPlano(dias, 1);
  const enviados = montarItensEdicao(dias);
  assert.equal(enviados[0].id, 2);
  assert.equal(enviados[0].tempoAerobico, 25);
  assert.equal(enviados[1].tempoAerobico, 0);
});

test("catalogos ocultam apenas os itens do mesmo tipo presentes no dia escolhido", () => {
  const catalogo = [{ id: 10 }, { id: 20 }, { id: 30 }];
  const dias = prepararEdicao(plano());
  assert.deepEqual(filtrarCatalogoDoDia(catalogo, dias[0], "exercicio"), [{ id: 20 }, { id: 30 }]);
  assert.deepEqual(filtrarCatalogoDoDia(catalogo, dias[0], "maquina"), [{ id: 10 }, { id: 30 }]);
  assert.deepEqual(filtrarCatalogoDoDia(catalogo, dias[1], "maquina"), catalogo);
  assert.deepEqual(filtrarCatalogoDoDia(catalogo, undefined, "exercicio"), catalogo);
  assert.deepEqual(catalogo, [{ id: 10 }, { id: 20 }, { id: 30 }]);
});

for (const tipo of ["exercicio", "maquina"]) {
  const selecionado = tipo === "maquina"
    ? { id: 20, nome: "Leg press", grupo: "Quadríceps" }
    : { id: 10, nome: "Agachamento", grupo: "Quadríceps" };

  test(`impede adicionar ${tipo} que ja existe no mesmo dia`, () => {
    const original = prepararEdicao(plano());
    const copia = structuredClone(original);
    const dias = adicionarItemAoPlano(original, 0, selecionado, tipo);
    assert.strictEqual(dias, original);
    assert.deepEqual(original, copia);
  });

  test(`adicionar e remover ${tipo} atualiza as opcoes do catalogo`, () => {
    const catalogo = [selecionado, { id: 30, nome: "Novo item" }];
    let dias = prepararEdicao(plano());
    dias = adicionarItemAoPlano(dias, 0, catalogo[1], tipo);
    const novoId = dias[0].exercicios.at(-1).id;
    assert.deepEqual(filtrarCatalogoDoDia(catalogo, dias[0], tipo), []);
    assert.strictEqual(adicionarItemAoPlano(dias, 0, catalogo[1], tipo), dias);
    dias = removerItemDoPlano(dias, novoId);
    assert.deepEqual(filtrarCatalogoDoDia(catalogo, dias[0], tipo), [catalogo[1]]);
  });

  test(`permite o mesmo ${tipo} em dias diferentes`, () => {
    let dias = prepararEdicao(plano());
    const novo = { id: 30, nome: "Novo item" };
    dias = adicionarItemAoPlano(dias, 0, novo, tipo);
    dias = adicionarItemAoPlano(dias, 1, novo, tipo);
    const campo = tipo === "maquina" ? "maquinaId" : "exercicioId";
    assert.equal(dias[0].exercicios.at(-1)[campo], 30);
    assert.equal(dias[1].exercicios.at(-1)[campo], 30);
    assert.notEqual(dias[0].exercicios.at(-1).id, dias[1].exercicios.at(-1).id);
  });

  test(`impede substituir por ${tipo} ja presente no mesmo dia inclusive entre catalogos`, () => {
    const original = prepararEdicao(plano());
    const outroItemId = tipo === "maquina" ? 1 : 2;
    assert.strictEqual(substituirItemDoPlano(original, outroItemId, selecionado, tipo), original);
  });
}

test("exercicio e maquina com o mesmo ID podem coexistir no mesmo dia", () => {
  const original = prepararEdicao(plano());
  let dias = adicionarItemAoPlano(original, 0, { id: 10, nome: "Máquina 10" }, "maquina");
  dias = adicionarItemAoPlano(dias, 0, { id: 20, nome: "Exercício 20" }, "exercicio");
  assert.equal(dias[0].exercicios.length, 4);
  assert.equal(dias[0].exercicios.at(-2).maquinaId, 10);
  assert.equal(dias[0].exercicios.at(-1).exercicioId, 20);
});

test("substituicao libera o item anterior e oculta o novo no catalogo do dia", () => {
  const catalogo = [{ id: 10, nome: "Agachamento" }, { id: 30, nome: "Supino" }];
  const dias = substituirItemDoPlano(prepararEdicao(plano()), 1, catalogo[1], "exercicio");
  assert.deepEqual(filtrarCatalogoDoDia(catalogo, dias[0], "exercicio"), [catalogo[0]]);
  assert.deepEqual(filtrarCatalogoDoDia(catalogo, dias[1], "exercicio"), [catalogo[1]]);
});

test("mover item muda somente a ordem no dia escolhido e permite cancelar sem alterar o plano", () => {
  const original = plano();
  original[0].tempoAerobico = 25;
  const copia = structuredClone(original);
  const rascunho = prepararEdicao(original);
  const dias = moverItemNoPlano(rascunho, 0, 1, -1);

  assert.deepEqual(dias[0].exercicios.map((item) => item.id), [2, 1]);
  assert.equal(dias[0].exercicios[0], rascunho[0].exercicios[1]);
  assert.equal(dias[0].exercicios[1], rascunho[0].exercicios[0]);
  assert.equal(dias[0].tempoAerobico, 25);
  assert.equal(dias[1], rascunho[1]);
  assert.deepEqual(original, copia);
  assert.deepEqual(rascunho[0].exercicios.map((item) => item.id), [1, 2]);
});

test("mover para baixo salva os itens na sequencia escolhida com a prescricao e aerobico", () => {
  const rascunho = alterarAerobicoDoPlano(prepararEdicao(plano()), 0, 30);
  rascunho[0].exercicios[0].series = "5";
  rascunho[0].exercicios[0].repeticoes = " 10 - 15 ";
  rascunho[0].exercicios[0].descanso = "120";
  const enviados = montarItensEdicao(moverItemNoPlano(rascunho, 0, 0, 1));

  assert.deepEqual(enviados.map((item) => item.id), [2, 1, 3]);
  assert.deepEqual(enviados[1], {
    id: 1, divisaoTreino: "A", tempoAerobico: 30,
    exercicioId: 10, maquinaId: null, series: 5,
    repeticao: "10-15", tempoDescanso: "120s",
  });
  assert.equal(enviados[0].tempoAerobico, 30);
  assert.equal(enviados[2].divisaoTreino, "B");
});

test("limites da reordenacao impedem atravessar dias ou mover indices invalidos", () => {
  const dias = prepararEdicao(plano());
  assert.equal(moverItemNoPlano(dias, 0, 0, -1), dias);
  assert.equal(moverItemNoPlano(dias, 0, 1, 1), dias);
  assert.equal(moverItemNoPlano(dias, 1, 0, -1), dias);
  assert.equal(moverItemNoPlano(dias, 1, 0, 1), dias);
  assert.equal(moverItemNoPlano(dias, 2, 0, 1), dias);
  assert.equal(moverItemNoPlano(dias, 0, -1, 1), dias);
  assert.equal(moverItemNoPlano(dias, 0, 5, -1), dias);
  assert.equal(moverItemNoPlano(dias, 0, 0.5, 1), dias);
  assert.equal(moverItemNoPlano(dias, 0, 0, 2), dias);
});

test("reordenar apos adicionar, substituir e remover preserva os vinculos enviados", () => {
  let dias = alterarAerobicoDoPlano(prepararEdicao(plano()), 0, 20);
  dias = adicionarItemAoPlano(dias, 0, { id: 30, nome: "Hack", grupo: "Pernas" }, "maquina");
  const novoId = dias[0].exercicios[2].id;
  dias = moverItemNoPlano(dias, 0, 2, -1);
  dias = substituirItemDoPlano(dias, novoId, { id: 31, nome: "Extensora", grupo: "Pernas" }, "maquina");
  dias = removerItemDoPlano(dias, 1);
  const enviados = montarItensEdicao(dias);

  assert.deepEqual(enviados.map((item) => item.id), [0, 2, 3]);
  assert.equal(enviados[0].maquinaId, 31);
  assert.equal(enviados[0].exercicioId, null);
  assert.equal(enviados[0].tempoAerobico, 20);
  assert.equal(enviados[1].tempoAerobico, 20);
});

test("restaurar ordem preserva edicoes e substituicoes sem recriar removidos ou perder novos", () => {
  const original = plano();
  let dias = prepararEdicao(original);
  dias[0].exercicios[1].series = "6";
  dias = substituirItemDoPlano(dias, 2, { id: 21, nome: "Hack", grupo: "Pernas" }, "maquina");
  dias = adicionarItemAoPlano(dias, 0, { id: 30, nome: "Supino", grupo: "Peito" }, "exercicio");
  const novoId = dias[0].exercicios[2].id;
  dias = moverItemNoPlano(dias, 0, 2, -1);
  dias = moverItemNoPlano(dias, 0, 1, -1);
  dias = removerItemDoPlano(dias, 1);
  dias = alterarAerobicoDoPlano(dias, 0, 35);
  const restaurado = restaurarOrdemDoPlano(dias, 0, original);

  assert.deepEqual(dias[0].exercicios.map((item) => item.id), [novoId, 2]);
  assert.deepEqual(restaurado[0].exercicios.map((item) => item.id), [2, novoId]);
  assert.equal(restaurado[0].exercicios[0].maquinaId, 21);
  assert.equal(restaurado[0].exercicios[0].series, "6");
  assert.equal(restaurado[0].tempoAerobico, 35);
  assert.equal(restaurado[1], dias[1]);
});

test("restaurar ordem sem alteracoes ou em dia inexistente preserva a lista", () => {
  const original = plano();
  const dias = prepararEdicao(original);
  assert.equal(restaurarOrdemDoPlano(dias, 0, original), dias);
  assert.equal(restaurarOrdemDoPlano(dias, 2, original), dias);
  assert.equal(restaurarOrdemDoPlano(dias, 0, []), dias);
});
