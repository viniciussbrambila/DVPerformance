import { Box, Typography, Button } from "@mui/material";
import { Link } from "react-router-dom";
import ArrowBackIcon from "@mui/icons-material/ArrowBack";
import PrintOutlinedIcon from "@mui/icons-material/PrintOutlined";

export default function CabecalhoRelatorio({
  titulo,
  voltar = "/alunos",
  onExportar,
  desabilitado,
}) {
  return (
    <Box
      sx={{
        display: "flex",
        justifyContent: "space-between",
        alignItems: "center",
        flexWrap: "wrap",
        gap: 2,
        mb: 3,
      }}
    >
      <Box>
        <Button
          className="no-print"
          component={Link}
          to={voltar}
          color="inherit"
          startIcon={<ArrowBackIcon />}
          sx={{ color: "text.secondary", mb: 1 }}
        >
          Voltar
        </Button>
        <Typography variant="h4" sx={{ fontWeight: "bold" }}>
          {titulo}
        </Typography>
      </Box>
      {onExportar && (
        <Button
          className="no-print"
          variant="contained"
          size="large"
          startIcon={<PrintOutlinedIcon />}
          onClick={onExportar}
          disabled={desabilitado}
        >
          Exportar PDF
        </Button>
      )}
    </Box>
  );
}
