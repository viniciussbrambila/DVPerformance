import { Box, Typography } from "@mui/material";
import { Activity } from "lucide-react";

export default function Marca({ compacta = false }) {
  return (
    <Box
      sx={{
        display: "flex",
        alignItems: "center",
        justifyContent: compacta ? "flex-start" : "center",
        gap: compacta ? 1 : 1.25,
        mb: compacta ? 0 : 2,
        color: "primary.main",
        "& svg": {
          flexShrink: 0,
          width: compacta ? 24 : { xs: 28, sm: 32 },
          height: compacta ? 24 : { xs: 28, sm: 32 },
        },
      }}
    >
      <Activity size={32} strokeWidth={2} aria-hidden="true" focusable="false" />
      <Typography
        component={compacta ? "div" : "h1"}
        variant={compacta ? "h6" : "h4"}
        color="inherit"
        noWrap={compacta}
        sx={{
          fontWeight: 700,
          fontSize: compacta ? "1.25rem" : { xs: "1.5rem", sm: "2rem" },
          lineHeight: 1.2,
        }}
      >
        DV Performance
      </Typography>
    </Box>
  );
}
