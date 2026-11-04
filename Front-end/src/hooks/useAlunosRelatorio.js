import { useState, useEffect } from "react";
import { buscarComCache, lerCache, temCache } from "../utils/cacheSessao";
import {
  obterUsuarioRelatorio,
  prepararAlunosRelatorio,
} from "../utils/relatorios";

export default function useAlunosRelatorio() {
  const usuario = obterUsuarioRelatorio();
  const usuarioId = usuario?.id;
  const chave = usuarioId ? `alunos:${usuarioId}` : null;
  const [lista, setLista] = useState(() => (chave && lerCache(chave)) || []);
  const [carregando, setCarregando] = useState(() =>
    Boolean(chave && !temCache(chave)),
  );
  const [erro, setErro] = useState("");

  useEffect(() => {
    let ativo = true;
    if (!usuarioId) {
      setLista([]);
      setCarregando(false);
      setErro("Usuário logado não identificado. Faça login novamente.");
      return;
    }
    const carregar = async () => {
      setErro("");
      setCarregando(!temCache(chave));
      try {
        const dados = await buscarComCache(chave, async () => {
          const resposta = await fetch(
            `http://localhost:5011/alunos?usuarioId=${usuarioId}`,
          );
          if (!resposta.ok)
            throw new Error("Não foi possível carregar os alunos.");
          return resposta.json();
        });
        if (ativo) setLista(dados);
      } catch (erroApi) {
        if (ativo)
          setErro(erroApi.message || "Erro de conexão com o servidor.");
      } finally {
        if (ativo) setCarregando(false);
      }
    };
    carregar();
    return () => {
      ativo = false;
    };
  }, [chave, usuarioId]);

  const medidas = chave ? lerCache(`medidas:${chave}`) || {} : {};
  const alunos = usuarioId
    ? prepararAlunosRelatorio(lista, usuarioId).map((aluno) => ({
        ...aluno,
        peso: aluno.peso ?? medidas[aluno.id]?.peso,
        altura: aluno.altura ?? medidas[aluno.id]?.altura,
      }))
    : [];
  return { alunos, carregando, erro };
}
