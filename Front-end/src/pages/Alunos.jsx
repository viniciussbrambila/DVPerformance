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
  const [editingId, setEditingId] = useState(null); // Guarda o ID do aluno em edição (ou null se for novo)
  const [tentouGravar, setTentouGravar] = useState(false); // Ativa os avisos nos campos vazios ao tentar salvar
  const [formData, setFormData] = useState({
    nome: '', idade: '', sexo: '', experiencia: '', frequenciaSemanal: '', objetivo: ''
  });

  const handleAbrirModal = () => setOpen(true);

  const handleFecharModal = () => {
    setOpen(false);
    setEditingId(null); // Sai do modo de edição ao fechar
    setTentouGravar(false); // Limpa os avisos de erro ao fechar
    setFormData({ nome: '', idade: '', sexo: '', experiencia: '', frequenciaSemanal: '', objetivo: '' });
  };

  const handleChange = (e) => {
    const { name, value } = e.target;

    // Se for o campo idade, aceita apenas números de 0 a 9
    if (name === 'idade') {
      const apenasNumeros = value.replace(/\D/g, '');
      setFormData({ ...formData, [name]: apenasNumeros });
      return;
    }

    setFormData({ ...formData, [name]: value });
  };

  // Preenche o modal com os dados do aluno selecionado para editar
  const handleEditar = (aluno) => {
    setEditingId(aluno.id);
    setTentouGravar(false);
    setFormData({
      nome: aluno.nome,
      idade: String(aluno.idade),
      sexo: aluno.sexo,
      experiencia: aluno.experiencia,
      frequenciaSemanal: aluno.frequenciaSemanal,
      objetivo: aluno.objetivo
    });
    setOpen(true);
  };

  // Remove o aluno filtrando a lista pelo ID
  const handleExcluir = (id) => {
    setAlunos(alunos.filter((aluno) => aluno.id !== id));
  };

  // Verifica se todos os 6 campos estão preenchidos corretamente
  const formularioValido =
    formData.nome.trim() !== '' &&
    String(formData.idade).trim() !== '' &&
    Number(formData.idade) > 0 &&
    formData.sexo !== '' &&
    formData.experiencia !== '' &&
    formData.frequenciaSemanal !== '' &&
    formData.objetivo.trim() !== '';

  const handleGravar = () => {
    // Se faltar algum campo, mostra os avisos vermelhos nos campos vazios e interrompe
    if (!formularioValido) {
      setTentouGravar(true);
      return;
    }

    if (editingId) {
      // Atualiza apenas o aluno que está sendo editado
      setAlunos(alunos.map((aluno) => (aluno.id === editingId ? { ...aluno, ...formData } : aluno)));
    } else {
      // Cria um novo aluno com ID único
      const novoAluno = { id: Date.now(), ...formData };
      setAlunos([...alunos, novoAluno]);
    }
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
      <TableContainer component={Paper} elevation={0} sx={{ ...glassStyle, borderRadius: 4, p: 2, display: 'flex', flexDirection: 'column' }}>
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
                  <Button 
                    size="small" 
                    color="primary" 
                    sx={{ minWidth: 'auto', mr: 1 }}
                    onClick={() => handleEditar(aluno)}
                  >
                    <EditIcon fontSize="small" />
                  </Button>
                  <Button 
                    size="small" 
                    color="error" 
                    sx={{ minWidth: 'auto' }}
                    onClick={() => handleExcluir(aluno.id)}
                  >
                    <DeleteIcon fontSize="small" />
                  </Button>
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
        <DialogTitle>{editingId ? 'Editar Aluno' : 'Registar Novo Aluno'}</DialogTitle>
        <DialogContent dividers sx={{ borderColor: 'rgba(255,255,255,0.1)' }}>
          <Grid container spacing={2} sx={{ mt: 2 }}>
            <Grid size={10}>
              <TextField 
                fullWidth 
                label="Nome Completo" 
                name="nome" 
                value={formData.nome} 
                onChange={handleChange} 
                variant="outlined"
                error={tentouGravar && !formData.nome.trim()}
                helperText={tentouGravar && !formData.nome.trim() ? 'Informe o nome completo' : ''}
              />
            </Grid>
            <Grid size={4}>
              <TextField 
                fullWidth 
                label="Idade" 
                name="idade" 
                value={formData.idade} 
                onChange={handleChange}
                slotProps={{ htmlInput: { inputMode: 'numeric', maxLength: 3 } }}
                error={tentouGravar && (!String(formData.idade).trim() || Number(formData.idade) <= 0)}
                helperText={tentouGravar && (!String(formData.idade).trim() || Number(formData.idade) <= 0) ? 'Informe a idade' : ''}
              />
            </Grid>
            <Grid size={2}>
              <TextField 
                fullWidth 
                select 
                label="Sexo" 
                name="sexo" 
                value={formData.sexo} 
                onChange={handleChange}
                error={tentouGravar && !formData.sexo}
                helperText={tentouGravar && !formData.sexo ? 'Obrigatório' : ''}
              >
                <MenuItem value="M">Masculino</MenuItem>
                <MenuItem value="F">Feminino</MenuItem>
              </TextField>
            </Grid>
            <Grid size={4}>
              <TextField 
                fullWidth 
                select 
                label="Experiência" 
                name="experiencia" 
                value={formData.experiencia} 
                onChange={handleChange}
                error={tentouGravar && !formData.experiencia}
                helperText={tentouGravar && !formData.experiencia ? 'Selecione o nível' : ''}
              >
                <MenuItem value="Iniciante">Iniciante</MenuItem>
                <MenuItem value="Intermediário">Intermediário</MenuItem>
                <MenuItem value="Avançado">Avançado</MenuItem>
              </TextField>
            </Grid>
            <Grid size={6}>
              <TextField 
                fullWidth 
                select 
                label="Frequência semanal" 
                name="frequenciaSemanal" 
                value={formData.frequenciaSemanal} 
                onChange={handleChange}
                error={tentouGravar && !formData.frequenciaSemanal}
                helperText={tentouGravar && !formData.frequenciaSemanal ? 'Selecione a frequência' : ''}
              >
                {[2, 3, 4, 5, 6].map(num => (
                  <MenuItem key={num} value={num}>{num} dias</MenuItem>
                ))}
              </TextField>
            </Grid>
            <Grid size={4}>
              <TextField 
                fullWidth 
                label="Objetivo" 
                name="objetivo" 
                value={formData.objetivo} 
                onChange={handleChange}
                error={tentouGravar && !formData.objetivo.trim()}
                helperText={tentouGravar && !formData.objetivo.trim() ? 'Informe o objetivo' : ''}
              />
            </Grid>
          </Grid>
        </DialogContent>
        <DialogActions sx={{ p: 2 }}>
          <Button onClick={handleFecharModal} sx={{ color: 'text.secondary' }}>
            Cancelar
          </Button>
          <Button 
            variant="contained" 
            color="primary" 
            onClick={handleGravar}
          >
            {editingId ? 'Salvar Alterações' : 'Guardar'}
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
}