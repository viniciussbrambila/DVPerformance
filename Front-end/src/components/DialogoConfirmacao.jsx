import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button,
} from "@mui/material";

export function DialogoConfirmacao({
  open,
  onClose,
  titulo,
  idTitulo,
  idDescricao,
  confirmarTexto = "Confirmar",
  cancelarTexto = "Cancelar",
  confirmarCor = "error",
  confirmarIcone = null,
  desabilitado = false,
  onConfirmar,
  justificativaAcoes = "flex-end",
  children,
}) {
  return (
    <Dialog
      open={open}
      onClose={desabilitado ? undefined : onClose}
      aria-labelledby={idTitulo}
      aria-describedby={idDescricao}
      maxWidth="xs"
      fullWidth
    >
      <DialogTitle id={idTitulo}>{titulo}</DialogTitle>
      <DialogContent>{children}</DialogContent>
      <DialogActions sx={{ justifyContent: justificativaAcoes }}>
        <Button onClick={onClose} disabled={desabilitado} color="inherit">
          {cancelarTexto}
        </Button>
        <Button
          onClick={onConfirmar}
          color={confirmarCor}
          variant="contained"
          startIcon={confirmarIcone}
          disabled={desabilitado}
        >
          {confirmarTexto}
        </Button>
      </DialogActions>
    </Dialog>
  );
}
