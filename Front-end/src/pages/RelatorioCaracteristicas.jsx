import {
  Box,
  Paper,
  Alert,
  CircularProgress,
  Button,
  Table,
  TableHead,
  TableBody,
  TableRow,
  TableCell,
  TableContainer,
} from "@mui/material";
import { Link } from "react-router-dom";
import PrintOutlinedIcon from "@mui/icons-material/PrintOutlined";
import CabecalhoRelatorio from "../components/CabecalhoRelatorio";
import useAlunosRelatorio from "../hooks/useAlunosRelatorio";
import { formatarMedidaRelatorio } from "../utils/relatorios";

export default function RelatorioCaracteristicas({ glassStyle }) {
  const { alunos, carregando, erro } = useAlunosRelatorio();
  const ordenados = [...alunos].sort((a, b) =>
    a.nome.localeCompare(b.nome, "pt-BR", { sensitivity: "base" }),
  );
  return (
    <Box>
      <CabecalhoRelatorio titulo="Relatório de Características dos Alunos" />
      {erro && (
        <Alert severity="error" sx={{ mb: 3 }}>
          {erro}
        </Alert>
      )}
      <Paper
        elevation={0}
        sx={{ ...glassStyle, borderRadius: 4, overflow: "hidden" }}
      >
        <TableContainer>
          <Table>
            <TableHead>
              <TableRow>
                {[
                  "Nome",
                  "Idade",
                  "Sexo",
                  "Peso",
                  "Altura",
                  "Objetivo",
                  "Experiência",
                  "Frequência",
                  "Ações",
                ].map((titulo) => (
                  <TableCell key={titulo} sx={{ fontWeight: 600 }}>
                    {titulo}
                  </TableCell>
                ))}
              </TableRow>
            </TableHead>
            <TableBody>
              {carregando && (
                <TableRow>
                  <TableCell colSpan={9} align="center" sx={{ py: 5 }}>
                    <CircularProgress size={32} />
                  </TableCell>
                </TableRow>
              )}
              {!carregando && ordenados.length === 0 && (
                <TableRow>
                  <TableCell
                    colSpan={9}
                    align="center"
                    sx={{ py: 4, color: "text.secondary" }}
                  >
                    Nenhum aluno cadastrado.
                  </TableCell>
                </TableRow>
              )}
              {!carregando &&
                ordenados.map((aluno) => (
                  <TableRow key={aluno.id} hover>
                    <TableCell sx={{ fontWeight: 500 }}>{aluno.nome}</TableCell>
                    <TableCell>{aluno.idade}</TableCell>
                    <TableCell>
                      {aluno.sexo === "M"
                        ? "Masculino"
                        : aluno.sexo === "F"
                          ? "Feminino"
                          : "Não informado"}
                    </TableCell>
                    <TableCell>
                      {formatarMedidaRelatorio(aluno.peso, "kg")}
                    </TableCell>
                    <TableCell>
                      {formatarMedidaRelatorio(aluno.altura, "m")}
                    </TableCell>
                    <TableCell>{aluno.objetivo}</TableCell>
                    <TableCell>{aluno.experiencia}</TableCell>
                    <TableCell>{aluno.frequenciaSemanal} dias/semana</TableCell>
                    <TableCell sx={{ whiteSpace: "nowrap" }}>
                      <Button
                        component={Link}
                        to={`/relatorios/caracteristicas/${aluno.id}`}
                        size="small"
                        color="inherit"
                        startIcon={<PrintOutlinedIcon />}
                        sx={{ bgcolor: "action.hover" }}
                      >
                        Exportar PDF
                      </Button>
                    </TableCell>
                  </TableRow>
                ))}
            </TableBody>
          </Table>
        </TableContainer>
      </Paper>
    </Box>
  );
}
