import { useState } from "react";
import {
  Box,
  Paper,
  Alert,
  CircularProgress,
  TextField,
  MenuItem,
  InputAdornment,
  Table,
  TableHead,
  TableBody,
  TableRow,
  TableCell,
  TableContainer,
} from "@mui/material";
import SearchIcon from "@mui/icons-material/Search";
import CabecalhoRelatorio from "../components/CabecalhoRelatorio";
import ImpressaoRelatorio from "../components/ImpressaoRelatorio";
import useAlunosRelatorio from "../hooks/useAlunosRelatorio";
import {
  obterDataCadastro,
  formatarDataCadastro,
  filtrarAlunosCadastrados,
  exportarPdfRelatorio,
} from "../utils/relatorios";

export default function RelatorioAlunosCadastrados({ glassStyle }) {
  const { alunos, carregando, erro } = useAlunosRelatorio();
  const [busca, setBusca] = useState("");
  const [dia, setDia] = useState("");
  const [mes, setMes] = useState("");
  const [ano, setAno] = useState("");
  const [ordem, setOrdem] = useState("recentes");
  const filtrados = filtrarAlunosCadastrados(alunos, {
    busca,
    dia,
    mes,
    ano,
    ordem,
  });
  const anos = [
    ...new Set(
      alunos
        .map((aluno) => obterDataCadastro(aluno)?.getFullYear())
        .filter(Boolean),
    ),
  ].sort((a, b) => b - a);
  const semData = alunos.some((aluno) => !obterDataCadastro(aluno));
  return (
    <Box className="relatorio-impressao">
      <ImpressaoRelatorio />
      <CabecalhoRelatorio
        titulo="Relatório de Alunos Cadastrados"
        onExportar={() => exportarPdfRelatorio("relatorio-alunos-cadastrados")}
        desabilitado={carregando || filtrados.length === 0}
      />
      {erro && (
        <Alert severity="error" className="no-print" sx={{ mb: 3 }}>
          {erro}
        </Alert>
      )}
      {semData && (
        <Alert severity="info" className="no-print" sx={{ mb: 3 }}>
          A data de cadastro ainda não está disponível para alguns alunos. Os
          filtros de data se aplicam aos alunos com data informada.
        </Alert>
      )}
      <Box
        className="no-print"
        sx={{ display: "flex", flexWrap: "wrap", gap: 2, mb: 3 }}
      >
        <TextField
          placeholder="Pesquisar por nome do aluno"
          value={busca}
          onChange={(e) => setBusca(e.target.value)}
          sx={{ flex: "1 1 280px" }}
          slotProps={{
            htmlInput: { "aria-label": "Pesquisar alunos por nome" },
            input: {
              startAdornment: (
                <InputAdornment position="start">
                  <SearchIcon />
                </InputAdornment>
              ),
            },
          }}
        />
        <TextField
          select
          label="Ordenar por data"
          value={ordem}
          onChange={(e) => setOrdem(e.target.value)}
          sx={{ minWidth: 180 }}
        >
          <MenuItem value="recentes">Mais recentes</MenuItem>
          <MenuItem value="antigos">Mais antigos</MenuItem>
        </TextField>
        <TextField
          select
          label="Dia"
          value={dia}
          onChange={(e) => setDia(e.target.value)}
          sx={{ minWidth: 100 }}
        >
          <MenuItem value="">Todos</MenuItem>
          {Array.from({ length: 31 }, (_, i) => i + 1).map((valor) => (
            <MenuItem key={valor} value={valor}>
              {valor}
            </MenuItem>
          ))}
        </TextField>
        <TextField
          select
          label="Mês"
          value={mes}
          onChange={(e) => setMes(e.target.value)}
          sx={{ minWidth: 140 }}
        >
          <MenuItem value="">Todos</MenuItem>
          {[
            "Janeiro",
            "Fevereiro",
            "Março",
            "Abril",
            "Maio",
            "Junho",
            "Julho",
            "Agosto",
            "Setembro",
            "Outubro",
            "Novembro",
            "Dezembro",
          ].map((nome, i) => (
            <MenuItem key={nome} value={i + 1}>
              {nome}
            </MenuItem>
          ))}
        </TextField>
        <TextField
          select
          label="Ano"
          value={ano}
          onChange={(e) => setAno(e.target.value)}
          sx={{ minWidth: 110 }}
        >
          <MenuItem value="">Todos</MenuItem>
          {anos.map((valor) => (
            <MenuItem key={valor} value={valor}>
              {valor}
            </MenuItem>
          ))}
        </TextField>
      </Box>
      <Paper
        elevation={0}
        sx={{ ...glassStyle, borderRadius: 4, overflow: "hidden" }}
      >
        <TableContainer>
          <Table>
            <TableHead>
              <TableRow>
                <TableCell sx={{ fontWeight: 600 }}>Nome do aluno</TableCell>
                <TableCell sx={{ fontWeight: 600 }}>Data de cadastro</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {carregando && (
                <TableRow>
                  <TableCell colSpan={2} align="center" sx={{ py: 5 }}>
                    <CircularProgress size={32} />
                  </TableCell>
                </TableRow>
              )}
              {!carregando && filtrados.length === 0 && (
                <TableRow>
                  <TableCell
                    colSpan={2}
                    align="center"
                    sx={{ py: 4, color: "text.secondary" }}
                  >
                    Nenhum aluno encontrado.
                  </TableCell>
                </TableRow>
              )}
              {!carregando &&
                filtrados.map((aluno) => (
                  <TableRow key={aluno.id} hover>
                    <TableCell sx={{ fontWeight: 500 }}>{aluno.nome}</TableCell>
                    <TableCell>{formatarDataCadastro(aluno)}</TableCell>
                  </TableRow>
                ))}
            </TableBody>
          </Table>
        </TableContainer>
      </Paper>
    </Box>
  );
}
