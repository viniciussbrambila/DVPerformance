import test from "node:test";
import assert from "node:assert/strict";
import { normalizarTextoBusca } from "../src/utils/busca.js";

test("encontra exercícios e máquinas sem exigir acentos na pesquisa", () => {
  for (const [nome, busca] of [
    ["Extensão de Tríceps", "extensao de triceps"],
    ["Cadeira Adutora Máquina", "MAQUINA"],
    ["Elevação de Quadríceps", "elevacao de quadriceps"],
    ["Abdução de Quadril", "abducao"],
    ["Rosca Bíceps", "BI\u0301CEPS"],
  ]) {
    assert.equal(
      normalizarTextoBusca(nome).includes(normalizarTextoBusca(busca)),
      true,
      nome,
    );
  }
  assert.equal(normalizarTextoBusca("Remada Baixa").includes("supino"), false);
});

test("a comparação preserva os nomes originais dos catálogos", () => {
  const item = { nome: "Elevação Lateral na Máquina" };
  normalizarTextoBusca(item.nome);
  assert.equal(item.nome, "Elevação Lateral na Máquina");
});
