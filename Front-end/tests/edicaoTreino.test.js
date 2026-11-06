import test from "node:test";
import assert from "node:assert/strict";
import {
  prepararEdicao,
  adicionarItemAoPlano,
  removerItemDoPlano,
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
    { id: 2, divisaoTreino: "A", exercicioId: null, maquinaId: 21, series: 4, repeticao: "8-12", tempoDescanso: "90s" },
    { id: 3, divisaoTreino: "B", exercicioId: 10, maquinaId: null, series: 3, repeticao: "8-12", tempoDescanso: "60s" },
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
  dias = adicionarItemAoPlano(dias, 0, { id: 10, nome: "Agachamento", grupo: "Quadríceps" }, "exercicio");
  const itemNovoId = dias[0].exercicios.at(-1).id;
  dias = adicionarItemAoPlano(dias, 1, { id: 10, nome: "Agachamento", grupo: "Quadríceps" }, "exercicio");
  const outroNovoId = dias[1].exercicios.at(-1).id;
  dias = substituirItemDoPlano(dias, itemNovoId, { id: 21, nome: "Hack", grupo: "Quadríceps" }, "maquina");
  assert.equal(dias[0].exercicios.at(-1).maquinaId, 21);
  assert.equal(dias[1].exercicios.at(-1).exercicioId, 10);
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
