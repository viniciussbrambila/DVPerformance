import test from "node:test";
import assert from "node:assert/strict";
import {
  filtrarAlunosCadastrados,
  filtrarPlanosTreino,
} from "../src/utils/relatorios.js";

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

const alunosExemplo = [
  {
    id: 1,
    nome: "Lucas Pereira",
    objetivo: "Hipertrofia",
    experiencia: "Iniciante",
    frequenciaSemanal: 3,
    dataCadastro: "2026-11-01",
  },
  {
    id: 2,
    nome: "Mariana Alves",
    objetivo: "Emagrecimento",
    experiencia: "Intermediário",
    frequenciaSemanal: 5,
    dataCadastro: "2026-11-05",
  },
  {
    id: 3,
    nome: "Rafael Gomes",
    objetivo: "Força",
    experiencia: "Avançado",
    frequenciaSemanal: 4,
    dataCadastro: "2026-11-10",
  },
  {
    id: 4,
    nome: "Érica Lima",
    objetivo: "Resistência",
    experiencia: "Intermediário",
    frequenciaSemanal: 3,
    dataCadastro: "2026-11-12",
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

test("filtra alunos por objetivo, experiência e frequência combinados", () => {
  const porObjetivo = filtrarAlunosCadastrados(alunosExemplo, {
    objetivos: ["Hipertrofia", "Força"],
  });
  assert.deepEqual(
    porObjetivo.map((a) => a.id),
    [3, 1],
  );

  const porExperienciaEFrequencia = filtrarAlunosCadastrados(alunosExemplo, {
    experiencias: ["Intermediário"],
    frequencias: [3, 5],
  });
  assert.deepEqual(
    porExperienciaEFrequencia.map((a) => a.id),
    [4, 2],
  );

  const combinado = filtrarAlunosCadastrados(alunosExemplo, {
    busca: "erica",
    objetivos: ["Resistência"],
    experiencias: ["Intermediário"],
    frequencias: [3],
  });
  assert.deepEqual(
    combinado.map((a) => a.id),
    [4],
  );
});

test("retorna lista vazia quando algum filtro multi-seleção está vazio", () => {
  assert.deepEqual(
    filtrarAlunosCadastrados(alunosExemplo, { objetivos: [] }),
    [],
  );
  assert.deepEqual(
    filtrarAlunosCadastrados(alunosExemplo, { experiencias: [] }),
    [],
  );
  assert.deepEqual(
    filtrarAlunosCadastrados(alunosExemplo, { frequencias: [] }),
    [],
  );
});
