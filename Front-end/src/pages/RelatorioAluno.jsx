import {
  Box,
  Paper,
  Typography,
  Grid,
  Alert,
  CircularProgress,
} from "@mui/material";
import { useParams } from "react-router-dom";
import { calcularIdade } from "../utils/dataNascimento";
import CabecalhoRelatorio from "../components/CabecalhoRelatorio";
import ImpressaoRelatorio from "../components/ImpressaoRelatorio";
import useAlunosRelatorio from "../hooks/useAlunosRelatorio";
import {
  formatarMedidaRelatorio,
  exportarPdfRelatorio,
} from "../utils/relatorios";

export default function RelatorioAluno({ glassStyle }) {
  const { id } = useParams();
  const { alunos, carregando, erro } = useAlunosRelatorio();
  const aluno = alunos.find((item) => String(item.id) === id);
  const idade = calcularIdade(aluno?.dataDeNascimento);
  const dados = aluno
    ? [
        ["Nome completo", aluno.nome],
        ["Idade", idade == null ? "Não informada" : `${idade} anos`],
        [
          "Sexo",
          aluno.sexo === "M"
            ? "Masculino"
            : aluno.sexo === "F"
              ? "Feminino"
              : "Não informado",
        ],
        ["Experiência", aluno.experiencia],
        ["Objetivo", aluno.objetivo],
        ["Frequência semanal", `${aluno.frequenciaSemanal} dias/semana`],
        ["Peso", formatarMedidaRelatorio(aluno.peso, "kg")],
        ["Altura", formatarMedidaRelatorio(aluno.altura, "m")],
      ]
    : [];
  return (
    <Box className="relatorio-impressao">
      <ImpressaoRelatorio />
      <CabecalhoRelatorio
        titulo="Características do Aluno"
        voltar="/alunos"
        onExportar={() =>
          exportarPdfRelatorio(`caracteristicas-${aluno?.nome || "aluno"}`)
        }
        desabilitado={carregando || !aluno}
      />
      {erro && (
        <Alert severity="error" className="no-print" sx={{ mb: 3 }}>
          {erro}
        </Alert>
      )}
      {carregando ? (
        <Box sx={{ display: "flex", justifyContent: "center", py: 5 }}>
          <CircularProgress />
        </Box>
      ) : aluno ? (
        <Paper
          elevation={0}
          sx={{ ...glassStyle, p: { xs: 3, md: 4 }, borderRadius: 4 }}
        >
          <Typography variant="h6" sx={{ fontWeight: 600, mb: 3 }}>
            Dados do aluno
          </Typography>
          <Grid container spacing={3}>
            {dados.map(([rotulo, valor]) => (
              <Grid key={rotulo} size={{ xs: 12, sm: 6, md: 4 }}>
                <Typography
                  variant="caption"
                  color="text.secondary"
                  sx={{ textTransform: "uppercase" }}
                >
                  {rotulo}
                </Typography>
                <Typography variant="h6" sx={{ fontWeight: 600 }}>
                  {valor}
                </Typography>
              </Grid>
            ))}
          </Grid>
        </Paper>
      ) : (
        <Alert severity="info">Aluno não encontrado ou inativo.</Alert>
      )}
    </Box>
  );
}
