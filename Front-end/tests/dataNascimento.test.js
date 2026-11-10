import test from "node:test";
import assert from "node:assert/strict";
import { spawnSync } from "node:child_process";
import {
  calcularIdade,
  converterDataNascimentoParaISO,
  dataNascimentoValida,
  formatarDataNascimento,
  limitesDataNascimento,
} from "../src/utils/dataNascimento.js";

const referencia = new Date(2026, 10, 9, 12);

test("a idade muda no dia do aniversário", () => {
  assert.equal(calcularIdade("2000-11-10", referencia), 25);
  assert.equal(calcularIdade("2000-11-09", referencia), 26);
  assert.equal(calcularIdade("2000-11-08", referencia), 26);
  assert.equal(calcularIdade("2026-11-09", referencia), 0);
});

test("rejeita datas futuras e calendários inválidos sem normalizá-los", () => {
  const invalidos = [
    undefined, null, "", 30, "2026-11-10", "2027-01-01",
    "2023-02-29", "1900-02-29", "2026-04-31", "2026-13-01",
    "2026-00-01", "2026-01-00", "0000-01-01", "2000-2-29",
    " 2000-02-29", "2000-02-29T00:00:00Z",
  ];
  for (const valor of invalidos) {
    assert.equal(calcularIdade(valor, referencia), null, String(valor));
    assert.equal(dataNascimentoValida(valor, referencia), false, String(valor));
  }
  assert.equal(calcularIdade("2000-02-29", referencia), 26);
  assert.equal(calcularIdade("2000-01-01", new Date("invalid")), null);
});

test("exige idade de 13 a 120 anos, com os extremos inclusivos", () => {
  assert.equal(dataNascimentoValida("2013-11-09", referencia), true);
  assert.equal(dataNascimentoValida("2013-11-10", referencia), false);
  assert.equal(dataNascimentoValida("1905-11-10", referencia), true);
  assert.equal(dataNascimentoValida("1905-11-09", referencia), false);
});

test("aniversário de 29 de fevereiro ocorre em março nos anos comuns", () => {
  assert.equal(calcularIdade("2000-02-29", new Date(2024, 1, 28, 12)), 23);
  assert.equal(calcularIdade("2000-02-29", new Date(2024, 1, 29, 12)), 24);
  assert.equal(calcularIdade("2000-02-29", new Date(2025, 1, 28, 12)), 24);
  assert.equal(calcularIdade("2000-02-29", new Date(2025, 2, 1, 12)), 25);
  assert.equal(dataNascimentoValida("2012-02-29", new Date(2025, 1, 28, 12)), false);
  assert.equal(dataNascimentoValida("2012-02-29", new Date(2025, 2, 1, 12)), true);
});

test("os limites do input incluem os alunos de 120 anos completos", () => {
  assert.deepEqual(limitesDataNascimento(referencia), {
    min: "1905-11-10", max: "2013-11-09",
  });
  assert.deepEqual(limitesDataNascimento(new Date(2026, 11, 31, 12)), {
    min: "1906-01-01", max: "2013-12-31",
  });
  assert.deepEqual(limitesDataNascimento(new Date(2025, 1, 28, 12)), {
    min: "1904-02-29", max: "2012-02-28",
  });
  assert.deepEqual(limitesDataNascimento(new Date(2025, 2, 1, 12)), {
    min: "1904-03-02", max: "2012-03-01",
  });
  assert.deepEqual(limitesDataNascimento(new Date(2024, 1, 29, 12)), {
    min: "1903-03-01", max: "2011-02-28",
  });
});

test("formata datas sem converter datas civis em instantes UTC", () => {
  assert.equal(formatarDataNascimento("2000-05-11"), "11/05/2000");
  assert.equal(formatarDataNascimento("2000-02-29"), "29/02/2000");
  assert.equal(formatarDataNascimento(null), "Não informada");
  assert.equal(formatarDataNascimento(""), "Não informada");
  assert.equal(formatarDataNascimento("2023-02-29"), "Não informada");
});

test("mantém data e idade corretas em fusos a oeste e a leste de UTC", () => {
  const modulo = new URL("../src/utils/dataNascimento.js", import.meta.url).href;
  const codigo = [
    "import { calcularIdade, formatarDataNascimento, limitesDataNascimento } from " + JSON.stringify(modulo) + ";",
    "const referencia = new Date(2026, 4, 10, 0, 30);",
    "process.stdout.write(JSON.stringify({ idade: calcularIdade('2000-05-11', referencia),",
    "data: formatarDataNascimento('2000-05-11'), limites: limitesDataNascimento(referencia) }));",
  ].join("\n");
  for (const TZ of ["America/Sao_Paulo", "Pacific/Kiritimati", "UTC"]) {
    const resultado = spawnSync(
      process.execPath, ["--input-type=module", "-e", codigo],
      { encoding: "utf8", env: { ...process.env, TZ } },
    );
    assert.equal(resultado.status, 0, resultado.stderr || String(resultado.error));
    assert.deepEqual(JSON.parse(resultado.stdout), {
      idade: 25, data: "11/05/2000",
      limites: { min: "1905-05-11", max: "2013-05-10" },
    }, TZ);
  }
});

test("converte nascimento digitado para a API sem aceitar datas inexistentes", () => {
  assert.equal(converterDataNascimentoParaISO("09/11/2000"), "2000-11-09");
  assert.equal(converterDataNascimentoParaISO("29/02/2000"), "2000-02-29");
  assert.equal(converterDataNascimentoParaISO("29/02/2023"), "");
  assert.equal(converterDataNascimentoParaISO("31/04/2000"), "");
  assert.equal(converterDataNascimentoParaISO("11/"), "");
  assert.equal(converterDataNascimentoParaISO("9/11/2000"), "");
  assert.equal(converterDataNascimentoParaISO(null), "");
  const nascimentoSalvo = "1999-08-23";
  assert.equal(
    converterDataNascimentoParaISO(formatarDataNascimento(nascimentoSalvo)),
    nascimentoSalvo,
  );
});
