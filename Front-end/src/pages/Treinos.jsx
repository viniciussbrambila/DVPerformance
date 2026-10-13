import { Typography, Paper } from '@mui/material';

export default function Treinos({ glassStyle }) {
  return (
    <Paper sx={{ p: 3, display: 'flex', flexDirection: 'column', borderRadius: 4, ...glassStyle }}>
      <Typography variant="h5" sx={{ color: 'primary', fontWeight: 'bold' }} gutterBottom>
        Gerar Plano de Treino
      </Typography>
      <Typography variant="body1" sx={{ color: 'primary', opacity: 0.9 }}>
       Lorem, ipsum dolor sit amet consectetur adipisicing elit. Optio suscipit delectus eligendi, ducimus placeat laborum perspiciatis voluptatum impedit velit nemo neque voluptatem non est minus, libero ad. Ad, provident laborum!
      </Typography>
    </Paper>
  );
}