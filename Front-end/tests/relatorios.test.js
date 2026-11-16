import test from "node:test";
import assert from "node:assert/strict";
import { filtrarPlanosTreino } from "../src/utils/relatorios.js";

const planosExemplo = [
  {
    id: 1,
    aluno: "Carlos Silva",
    status: "Validado",
    data: "2026-10-05T10:00:00",
  },
  {
    id: 2,
    aluno: "Ana Beatriz",
    status: "Rascunho",
    data: "2026-11-10T14:30:00",
  },
  {
    id: 3,
    aluno: "Bruno Costa",
    status: "Exportado",
    data: "2026-11-01T09:15:00",
  },
  {
    id: 4,
    aluno: "Ágatha Souza",
    status: "Validado",
    data: "2026-11-15T18:00:00",
  },
];

test("ordena planos por mais recentes por padrão", () => {
  const resultado = filtrarPlanosTreino(planosExemplo);
  assert.deepEqual(
    resultado.map((p) => p.id),
    [4, 2, 3, 1],
  );
});

test("ordena planos por mais antigos, A-Z e Z-A", () => {
  assert.deepEqual(
    filtrarPlanosTreino(planosExemplo, { ordem: "antigos" }).map((p) => p.id),
    [1, 3, 2, 4],
  );
  assert.deepEqual(
    filtrarPlanosTreino(planosExemplo, { ordem: "az" }).map((p) => p.aluno),
    ["Ágatha Souza", "Ana Beatriz", "Bruno Costa", "Carlos Silva"],
  );
  assert.deepEqual(
    filtrarPlanosTreino(planosExemplo, { ordem: "za" }).map((p) => p.aluno),
    ["Carlos Silva", "Bruno Costa", "Ana Beatriz", "Ágatha Souza"],
  );
});

test("filtra planos por intervalo de datas, status e nome sem exigir acentos", () => {
  const porData = filtrarPlanosTreino(planosExemplo, {
    dataInicio: new Date(2026, 10, 1, 12, 0, 0),
    dataFim: new Date(2026, 10, 10, 12, 0, 0),
  });
  assert.deepEqual(
    porData.map((p) => p.id),
    [2, 3],
  );

  const porBuscaEStatus = filtrarPlanosTreino(planosExemplo, {
    busca: "agatha",
    status: "Validado",
  });
  assert.deepEqual(
    porBuscaEStatus.map((p) => p.id),
    [4],
  );
});
