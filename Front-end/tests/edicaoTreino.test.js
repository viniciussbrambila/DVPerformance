import test from "node:test";
import assert from "node:assert/strict";
import {
  prepararEdicao,
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
    { id: 2, exercicioId: null, maquinaId: 21, series: 4, repeticao: "8-12", tempoDescanso: "90s" },
    { id: 3, exercicioId: 10, maquinaId: null, series: 3, repeticao: "8-12", tempoDescanso: "60s" },
  ]);
});
