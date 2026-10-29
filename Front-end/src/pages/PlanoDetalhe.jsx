import {
  Box,
  Paper,
  Typography,
  Button,
  Divider,
  TextField,
  GlobalStyles,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
} from "@mui/material";
import { useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import ArrowBackIcon from "@mui/icons-material/ArrowBack";
import CheckCircleOutlinedIcon from "@mui/icons-material/CheckCircleOutlined";
import EditOutlinedIcon from "@mui/icons-material/EditOutlined";
import PrintOutlinedIcon from "@mui/icons-material/PrintOutlined";
import CloseIcon from "@mui/icons-material/Close";
import SaveOutlinedIcon from "@mui/icons-material/SaveOutlined";

const corStatus = {
  Rascunho: "warning.main",
  Validado: "success.main",
  Exportado: "info.main",
};

const formatarData = (iso) => new Date(iso).toLocaleDateString("pt-BR");

const cabecalhoSx = {
  fontWeight: 700,
  textTransform: "uppercase",
  color: "text.secondary",
};

export default function PlanoDetalhe({ glassStyle, planos, onAtualizar }) {
  const { id } = useParams();
  const navigate = useNavigate();
  const plano = planos.find((p) => p.id === id);

  const [editando, setEditando] = useState(false);
  const [diasEdicao, setDiasEdicao] = useState([]);

  if (!plano) {
    return (
      <Box>
        <Typography variant="h5" sx={{ fontWeight: "bold", mb: 2 }}>
          Plano não encontrado
        </Typography>
        <Button
          variant="contained"
          startIcon={<ArrowBackIcon />}
          onClick={() => navigate("/planos")}
        >
          Voltar para Planos
        </Button>
      </Box>
    );
  }

  const dias = editando ? diasEdicao : plano.dias;

  const iniciarEdicao = () => {
    setDiasEdicao(JSON.parse(JSON.stringify(plano.dias)));
    setEditando(true);
  };

  const alterarCampo = (indiceDia, indiceEx, campo, valor) => {
    setDiasEdicao((lista) =>
      lista.map((dia, i) =>
        i !== indiceDia
          ? dia
          : {
              ...dia,
              exercicios: dia.exercicios.map((ex, j) =>
                j !== indiceEx ? ex : { ...ex, [campo]: valor },
              ),
            },
      ),
    );
  };

  const salvarEdicao = () => {
    onAtualizar(plano.id, { dias: diasEdicao });
    setEditando(false);
  };

  const validar = () => onAtualizar(plano.id, { status: "Validado" });

  const exportarPdf = () => {
    onAtualizar(plano.id, { status: "Exportado" });

    // Abre a janela de impressão do navegador: escolha "Salvar como PDF".
    setTimeout(() => {
      const tituloAnterior = document.title;
      document.title = `plano-${plano.aluno.toLowerCase().replace(/\s+/g, "-")}`;
      window.print();
      document.title = tituloAnterior;
    }, 300);
  };

  const infos = [
    ["Aluno", plano.aluno],
    ["Objetivo", plano.objetivo],
    ["Frequência", `${plano.frequencia} dias/semana`],
    ["Status", plano.status],
    ["Data", formatarData(plano.data)],
  ];

  return (
    <Box>
      {/* Na impressão, esconde a barra lateral e os botões */}
      <GlobalStyles
        styles={{
          "@media print": {
            ".MuiDrawer-root, .no-print": { display: "none !important" },
            "html, body": { background: "#fff !important" },
            main: { padding: "0 !important", overflow: "visible !important" },
          },
        }}
      />

      <Box
        className="no-print"
        sx={{
          display: "flex",
          justifyContent: "space-between",
          alignItems: "flex-start",
          flexWrap: "wrap",
          gap: 2,
          mb: 3,
        }}
      >
        <Box sx={{ display: "flex", alignItems: "flex-start", gap: 2 }}>
          <Button
            color="inherit"
            startIcon={<ArrowBackIcon />}
            onClick={() => navigate("/planos")}
            sx={{ color: "text.secondary", mt: 0.5 }}
          >
            Planos
          </Button>

          <Box>
            <Typography variant="h4" sx={{ fontWeight: "bold" }}>
              Plano de Treino
            </Typography>
            <Typography variant="body2" color="text.secondary">
              Status:{" "}
              <Box
                component="span"
                sx={{ color: corStatus[plano.status], fontWeight: 600 }}
              >
                {plano.status}
              </Box>
            </Typography>
          </Box>
        </Box>

        <Box sx={{ display: "flex", gap: 1.5, flexWrap: "wrap" }}>
          {editando ? (
            <>
              <Button
                color="inherit"
                startIcon={<CloseIcon />}
                onClick={() => setEditando(false)}
              >
                Cancelar
              </Button>
              <Button
                variant="contained"
                startIcon={<SaveOutlinedIcon />}
                onClick={salvarEdicao}
              >
                Salvar
              </Button>
            </>
          ) : (
            <>
              {plano.status === "Rascunho" && (
                <Button
                  variant="contained"
                  color="success"
                  size="large"
                  startIcon={<CheckCircleOutlinedIcon />}
                  onClick={validar}
                >
                  Validar Plano
                </Button>
              )}

              <Button
                color="inherit"
                size="large"
                startIcon={<EditOutlinedIcon />}
                onClick={iniciarEdicao}
                sx={{ bgcolor: "action.hover" }}
              >
                Editar
              </Button>

              <Button
                variant="contained"
                size="large"
                startIcon={<PrintOutlinedIcon />}
                onClick={exportarPdf}
                sx={{
                  bgcolor: "text.primary",
                  color: "background.default",
                  "&:hover": { bgcolor: "text.secondary" },
                }}
              >
                Exportar PDF
              </Button>
            </>
          )}
        </Box>
      </Box>

      <Paper
        elevation={0}
        sx={{
          ...glassStyle,
          p: { xs: 3, md: 5 },
          borderRadius: 4,
          "@media print": {
            background: "#fff !important",
            color: "#000 !important",
            boxShadow: "none !important",
            border: "none !important",
            backdropFilter: "none !important",
            "& .MuiTypography-root, & .MuiTableCell-root": {
              color: "#000 !important",
            },
            "& .info-faixa": {
              backgroundColor: "#f3f4f6 !important",
              printColorAdjust: "exact",
              WebkitPrintColorAdjust: "exact",
            },
            "& .rotulo-dia": {
              backgroundColor: "#000 !important",
              color: "#fff !important",
              printColorAdjust: "exact",
              WebkitPrintColorAdjust: "exact",
            },
          },
        }}
      >
        <Box sx={{ textAlign: "center", mb: 3 }}>
          <Typography variant="h3" sx={{ fontWeight: 800 }}>
            DV PERFORMANCE
          </Typography>
          <Typography
            variant="body2"
            color="text.secondary"
            sx={{ letterSpacing: 3 }}
          >
            PRESCRIÇÃO DE TREINAMENTO
          </Typography>
        </Box>

        <Divider
          sx={{ borderBottomWidth: 2, borderColor: "text.primary", mb: 4 }}
        />

        <Box
          className="info-faixa"
          sx={{
            display: "grid",
            gridTemplateColumns: { xs: "repeat(2, 1fr)", md: "repeat(5, 1fr)" },
            gap: 2,
            p: 2.5,
            mb: 5,
            borderRadius: 3,
            bgcolor: "action.hover",
          }}
        >
          {infos.map(([rotulo, valor]) => (
            <Box key={rotulo}>
              <Typography
                variant="caption"
                color="text.secondary"
                sx={{ fontWeight: 700, textTransform: "uppercase" }}
              >
                {rotulo}
              </Typography>
              <Typography variant="h6" sx={{ fontWeight: 600 }}>
                {valor}
              </Typography>
            </Box>
          ))}
        </Box>

        {dias.map((dia, indiceDia) => (
          <Box
            key={`${dia.nome}-${indiceDia}`}
            sx={{ mb: 5, breakInside: "avoid" }}
          >
            <Typography
              className="rotulo-dia"
              variant="h5"
              sx={{
                display: "inline-block",
                fontWeight: "bold",
                px: 2.5,
                py: 1,
                mb: 2,
                bgcolor: "text.primary",
                color: "background.default",
              }}
            >
              {dia.nome}
            </Typography>

            <TableContainer>
              <Table>
                <TableHead>
                  <TableRow>
                    <TableCell sx={cabecalhoSx}>Exercício</TableCell>
                    <TableCell align="center" sx={cabecalhoSx}>
                      Séries
                    </TableCell>
                    <TableCell align="center" sx={cabecalhoSx}>
                      Repetições
                    </TableCell>
                    <TableCell align="center" sx={cabecalhoSx}>
                      Descanso
                    </TableCell>
                  </TableRow>
                </TableHead>

                <TableBody>
                  {dia.exercicios.map((ex, indiceEx) => (
                    <TableRow key={`${ex.nome}-${indiceEx}`}>
                      <TableCell sx={{ fontWeight: 700 }}>{ex.nome}</TableCell>

                      {["series", "repeticoes", "descanso"].map((campo) => (
                        <TableCell
                          key={campo}
                          align="center"
                          sx={{
                            color:
                              campo === "descanso"
                                ? "text.secondary"
                                : "text.primary",
                          }}
                        >
                          {editando ? (
                            <TextField
                              size="small"
                              value={ex[campo]}
                              onChange={(e) =>
                                alterarCampo(
                                  indiceDia,
                                  indiceEx,
                                  campo,
                                  e.target.value,
                                )
                              }
                              sx={{ width: 100 }}
                              slotProps={{
                                htmlInput: { style: { textAlign: "center" } },
                              }}
                            />
                          ) : (
                            ex[campo]
                          )}
                        </TableCell>
                      ))}
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </TableContainer>
          </Box>
        ))}
      </Paper>
    </Box>
  );
}
