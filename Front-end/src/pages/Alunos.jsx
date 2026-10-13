import { useState } from 'react';
import { 
  Typography, Paper, Box, Button, Table, TableBody, TableCell, 
  TableContainer, TableHead, TableRow, Dialog, DialogTitle, 
  DialogContent, DialogActions, TextField, MenuItem, Grid
} from '@mui/material';
import AddIcon from '@mui/icons-material/Add';
import EditIcon from '@mui/icons-material/Edit';
import DeleteIcon from '@mui/icons-material/Delete';

const alunosIniciais = [
  { id: 1, nome: 'João Carlos', idade: 28, sexo: 'M', objetivo: 'Hipertrofia', experiencia: 'Intermediário', frequenciaSemanal: 5 },
  { id: 2, nome: 'Maria Clara', idade: 34, sexo: 'F', objetivo: 'Emagrecimento', experiencia: 'Iniciante', frequenciaSemanal: 3 },
  { id: 3, nome: 'Sr. Antônio', idade: 66, sexo: 'M', objetivo: 'Condicionamento', experiencia: 'Iniciante', frequenciaSemanal: 3 },
];

export default function Alunos({ glassStyle }) {
  const [alunos, setAlunos] = useState(alunosIniciais);
  const [open, setOpen] = useState(false);
  const [formData, setFormData] = useState({
    nome: '', idade: '', sexo: '', experiencia: '', frequenciaSemanal: '', objetivo: ''
  });

  const handleAbrirModal = () => setOpen(true);
  const handleFecharModal = () => {
    setOpen(false);
    setFormData({ nome: '', idade: '', sexo: '', experiencia: '', frequenciaSemanal: '', objetivo: '' });
  };

  const handleChange = (e) => {
    const { name, value } = e.target;
    setFormData({ ...formData, [name]: value });
  };

  const handleGravar = () => {
    const novoAluno = { id: alunos.length + 1, ...formData };
    setAlunos([...alunos, novoAluno]);
    handleFecharModal();
  };

  return (
    <Box>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
        <Typography variant="h4" color="text.primary">
          Gerir Alunos
        </Typography>
        <Button variant="contained" color="primary" startIcon={<AddIcon />} onClick={handleAbrirModal}>
          Novo Aluno
        </Button>
      </Box>

      {/* Tabela com o efeito de Vidro Fumado */}
      <TableContainer component={Paper} elevation={0} sx={{ ...glassStyle, border: 'none' }}>
        <Table>
          <TableHead>
            <TableRow>
              <TableCell sx={{ fontWeight: 'bold' }}>Nome</TableCell>
              <TableCell align="center" sx={{ fontWeight: 'bold' }}>Idade</TableCell>
              <TableCell align="center" sx={{ fontWeight: 'bold' }}>Objetivo</TableCell>
              <TableCell align="center" sx={{ fontWeight: 'bold' }}>Experiência</TableCell>
              <TableCell align="center" sx={{ fontWeight: 'bold' }}>Ações</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {alunos.map((aluno) => (
              <TableRow key={aluno.id} sx={{ '&:last-child td, &:last-child th': { border: 0 } }}>
                <TableCell component="th" scope="row">{aluno.nome}</TableCell>
                <TableCell align="center">{aluno.idade}</TableCell>
                <TableCell align="center">{aluno.objetivo}</TableCell>
                <TableCell align="center">{aluno.experiencia}</TableCell>
                <TableCell align="center">
                  <Button size="small" color="primary" sx={{ minWidth: 'auto', mr: 1 }}><EditIcon fontSize="small" /></Button>
                  <Button size="small" color="error" sx={{ minWidth: 'auto' }}><DeleteIcon fontSize="small" /></Button>
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </TableContainer>

      {/* Modal também com efeito de Vidro Fumado */}
      <Dialog 
        open={open} 
        onClose={handleFecharModal} 
        maxWidth="sm" 
        fullWidth
        PaperProps={{ sx: { ...glassStyle, background: 'rgba(15, 23, 42, 0.85)' } }}
      >
        <DialogTitle>Registar Novo Aluno</DialogTitle>
        <DialogContent dividers sx={{ borderColor: 'rgba(255,255,255,0.1)' }} >
          <Grid  container spacing={2} sx={{ mt: 2 }}>
            <Grid size={10}>
              <TextField fullWidth label="Nome Completo" name="nome" value={formData.nome} onChange={handleChange} variant="outlined" />
            </Grid>
            <Grid size={4}>
              <TextField fullWidth type="number" label="Idade" name="idade" value={formData.idade} onChange={handleChange} />
            </Grid>
            <Grid size={2}>
              <TextField fullWidth select label="Sexo" name="sexo" value={formData.sexo} onChange={handleChange}>
                <MenuItem value="M">Masculino</MenuItem>
                <MenuItem value="F">Feminino</MenuItem>
              </TextField>
            </Grid>
            <Grid size={4}>
              <TextField fullWidth select label="Experiência" name="experiencia" value={formData.experiencia} onChange={handleChange}>
                <MenuItem value="Iniciante">Iniciante</MenuItem>
                <MenuItem value="Intermediário">Intermediário</MenuItem>
                <MenuItem value="Avançado">Avançado</MenuItem>
              </TextField>
            </Grid>
            <Grid size={6}>
              <TextField fullWidth select label="Frequência semanal" name="frequenciaSemanal" value={formData.frequenciaSemanal} onChange={handleChange}>
                {[2, 3, 4, 5, 6].map(num => (
                  <MenuItem key={num} value={num}>{num} dias</MenuItem>
                ))}
              </TextField>
            </Grid>
            <Grid size={4}>
              <TextField fullWidth label="Objetivo" name="objetivo" value={formData.objetivo} onChange={handleChange} />
            </Grid>
          </Grid>
        </DialogContent>
        <DialogActions sx={{ p: 2 }}>
          <Button onClick={handleFecharModal} sx={{ color: 'text.secondary' }}>Cancelar</Button>
          <Button variant="contained" color="primary" onClick={handleGravar}>
            Guardar
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
}