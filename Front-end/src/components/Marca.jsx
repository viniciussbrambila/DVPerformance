import { Box, SvgIcon, Typography } from "@mui/material";

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
      }}
    >
      <SvgIcon
        aria-hidden="true"
        focusable="false"
        viewBox="0 0 24 24"
        sx={{
          flexShrink: 0,
          width: compacta ? 24 : { xs: 28, sm: 32 },
          height: compacta ? 24 : { xs: 28, sm: 32 },
          fill: "none",
          stroke: "currentColor",
          strokeWidth: 2,
          strokeLinecap: "round",
          strokeLinejoin: "round",
        }}
      >
        <path d="M22 12h-2.48a2 2 0 0 0-1.93 1.46l-2.35 8.36a.25.25 0 0 1-.48 0L9.24 2.18a.25.25 0 0 0-.48 0l-2.35 8.36A2 2 0 0 1 4.49 12H2" />
      </SvgIcon>
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
