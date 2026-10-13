import { Typography, Paper } from '@mui/material';

export default function Dashboard({ glassStyle }) {
  return (
    <Paper sx={{ p: 3, display: 'flex', flexDirection: 'column', borderRadius: 4, ...glassStyle }}>
      <Typography variant="h5" sx={{ color:'primary', fontWeight: 'bold' }} gutterBottom>
        Bem-vindo ao DV Performance
      </Typography>
      <Typography variant="body1" sx={{ color:'primary', opacity: 0.9 }}>
        Selecione uma opção no menu lateral para começar.
      </Typography>
    </Paper>
  );
}